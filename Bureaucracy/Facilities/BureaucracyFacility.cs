using System;
using System.Linq;
using UnityEngine;
using Upgradeables;

namespace Bureaucracy
{
    public class BureaucracyFacility
    {
        private int level = 1;
        private int upkeepCost = 0;
        private string name;
        private bool upgrading = false;
        private bool recentlyUpgraded = false;
        public FacilityUpgradeEvent Upgrade;
        private bool canBeClosed;
        private bool isClosed;
        public int LaunchesThisMonth = 0;
        
        public bool IsClosed => isClosed;

        public bool CanBeClosed => canBeClosed;

        public void ReopenFacility()
        {
            LaunchesThisMonth = 0;
            isClosed = false;
        }

        public void CloseFacility()
        {
            if (!canBeClosed) return;
            isClosed = true;
        }
        public bool Upgrading => upgrading;

        public int MaintenanceCost => upkeepCost * level;

        public string Name => name;

        public BureaucracyFacility(SpaceCenterFacility spf)
        {
            name = SetName(spf);
            upkeepCost = SetCosts();
        }

        private string SetName(SpaceCenterFacility spf)
        {
            switch (spf)
            {
                case SpaceCenterFacility.Administration:
                    return "Administration";
                case SpaceCenterFacility.AstronautComplex:
                    canBeClosed = true;
                    return "AstronautComplex";
                case SpaceCenterFacility.MissionControl:
                    return "MissionControl";
                case SpaceCenterFacility.SpaceplaneHangar:
                    canBeClosed = true;
                    return "SpaceplaneHangar";
                case SpaceCenterFacility.TrackingStation:
                    return "TrackingStation";
                case SpaceCenterFacility.ResearchAndDevelopment:
                    return "ResearchAndDevelopment";
                case SpaceCenterFacility.VehicleAssemblyBuilding:
                    canBeClosed = true;
                    return "VehicleAssemblyBuilding";
                case SpaceCenterFacility.Runway:
                    return "Runway";
                case SpaceCenterFacility.LaunchPad:
                    return "LaunchPad";
                default:
                    return "Other Facility";
            }
        }

        private int SetCosts()
        {
            int cost = 0;
            switch (name)
            {
                case "Administration":
                    cost = SettingsClass.Instance.AdminCost;
                    break;
                case "AstronautComplex":
                    cost = SettingsClass.Instance.AstronautComplexCost;
                    break;
                case "MissionControl":
                    cost = SettingsClass.Instance.MissionControlCost;
                    break;
                case "SpaceplaneHangar":
                    cost = SettingsClass.Instance.SphCost;
                    break;
                case "TrackingStation":
                    cost = SettingsClass.Instance.TrackingStationCost;
                    break;
                case "ResearchAndDevelopment":
                    cost = SettingsClass.Instance.RndCost;
                    break;
                case "VehicleAssemblyBuilding":
                    cost = SettingsClass.Instance.VabCost;
                    break;
                case "Other Facility":
                    cost = SettingsClass.Instance.OtherFacilityCost;
                    break;
                default:
                    cost = 0;
                    break;
            }
            return cost;
        }

        public void StartUpgrade(UpgradeableFacility facilityToUpgrade)
        {
            Upgrade = new FacilityUpgradeEvent(facilityToUpgrade.id, this);
            upgrading = true;
            ScreenMessages.PostScreenMessage("[Bureaucracy]: Upgrade of " + name + " requested");
        }
        
        public string GetProgressReport(FacilityUpgradeEvent upgrade)
        {
            if (!upgrading && !recentlyUpgraded) return String.Empty;
            if (recentlyUpgraded)
            {
                recentlyUpgraded = false;
                return name + ": Upgrade completed successfully";
            }
            return name+ ": $" + upgrade.Cost + " of investment needed to complete";
        }
        
        public void OnLoad(ConfigNode[] facilityNodes)
        {
            for (int i = 0; i < facilityNodes.Length; i++)
            {
                ConfigNode cn = facilityNodes.ElementAt(i);
                if (cn.GetValue("Name") != name) continue;
                int.TryParse(cn.GetValue("Level"), out level);
                bool.TryParse(cn.GetValue("RecentlyUpgraded"), out recentlyUpgraded);
                bool.TryParse(cn.GetValue("Closed"), out isClosed);
                int.TryParse(cn.GetValue("LaunchesThisMonth"), out LaunchesThisMonth);
                SetCosts();
                ConfigNode upgradeNode = cn.GetNode("UPGRADE");
                if (upgradeNode != null)
                {
                    upgrading = true;
                    Upgrade = new FacilityUpgradeEvent(upgradeNode.GetValue("ID"), this);
                    Upgrade.OnLoad(upgradeNode);
                }
                return;
            }
        }

        public void OnSave(ConfigNode cn)
        {
            ConfigNode thisNode = new ConfigNode("FACILITY");
            thisNode.SetValue("Name", name, true);
            thisNode.SetValue("Level", level, true);
            thisNode.SetValue("RecentlyUpgraded", recentlyUpgraded, true);
            thisNode.SetValue("Closed", isClosed, true);
            thisNode.SetValue("LaunchesThisMonth", LaunchesThisMonth, true);
            if (upgrading) Upgrade.OnSave(thisNode);
            cn.AddNode(thisNode);
        }

        public void OnUpgradeCompleted()
        {
            upgrading = false;
            Upgrade = null;
            recentlyUpgraded = true;
            level++;
        }
    }
}