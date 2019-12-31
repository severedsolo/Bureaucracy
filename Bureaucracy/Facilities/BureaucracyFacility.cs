using System;
using System.Linq;
using UnityEngine;
using Upgradeables;

namespace Bureaucracy
{
    public class BureaucracyFacility
    {
        private int level = 1;
        private readonly int upkeepCost;
        private bool recentlyUpgraded;
        public FacilityUpgradeEvent Upgrade;
        private bool isClosed;
        public int LaunchesThisMonth;
        
        public bool IsClosed => isClosed;

        private bool CanBeClosed { get; set; }

        public void ReopenFacility()
        {
            if (!isClosed) return;
            Debug.Log("[Bureaucracy]: Reopening "+Name);
            LaunchesThisMonth = 0;
            isClosed = false;
        }

        public void CloseFacility()
        {
            if (!CanBeClosed) return;
            isClosed = true;
            Debug.Log("[Bureaucracy]: "+Name+" has been closed");
        }
        public bool Upgrading { get; private set; }

        public int MaintenanceCost => upkeepCost * level;

        public string Name { get; }

        public BureaucracyFacility(SpaceCenterFacility spf)
        {
            Name = SetName(spf);
            upkeepCost = SetCosts();
            Debug.Log("[Bureaucracy]: Setup Facility "+Name);
        }

        private string SetName(SpaceCenterFacility spf)
        {
            switch (spf)
            {
                case SpaceCenterFacility.Administration:
                    return "Administration";
                case SpaceCenterFacility.AstronautComplex:
                    CanBeClosed = true;
                    return "AstronautComplex";
                case SpaceCenterFacility.MissionControl:
                    return "MissionControl";
                case SpaceCenterFacility.SpaceplaneHangar:
                    CanBeClosed = true;
                    return "SpaceplaneHangar";
                case SpaceCenterFacility.TrackingStation:
                    return "TrackingStation";
                case SpaceCenterFacility.ResearchAndDevelopment:
                    return "ResearchAndDevelopment";
                case SpaceCenterFacility.VehicleAssemblyBuilding:
                    CanBeClosed = true;
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
            int cost;
            switch (Name)
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
            Upgrading = true;
            ScreenMessages.PostScreenMessage("[Bureaucracy]: Upgrade of " + Name + " requested");
            Debug.Log("[Bureaucracy]: Upgrade of "+Name+" requested for " +Upgrade.OriginalCost);
        }
        
        public string GetProgressReport(FacilityUpgradeEvent upgrade)
        {
            // ReSharper disable once BuiltInTypeReferenceStyleForMemberAccess
            if (!Upgrading && !recentlyUpgraded) return String.Empty;
            if (!recentlyUpgraded) return Name + ": $" + upgrade.RemainingInvestment + " / "+ upgrade.OriginalCost;
            recentlyUpgraded = false;
            return Name + ": Upgrade completed successfully";
        }
        
        public void OnLoad(ConfigNode[] facilityNodes)
        {
            for (int i = 0; i < facilityNodes.Length; i++)
            {
                ConfigNode cn = facilityNodes.ElementAt(i);
                if (cn.GetValue("Name") != Name) continue;
                int.TryParse(cn.GetValue("Level"), out level);
                bool.TryParse(cn.GetValue("RecentlyUpgraded"), out recentlyUpgraded);
                bool.TryParse(cn.GetValue("Closed"), out isClosed);
                int.TryParse(cn.GetValue("LaunchesThisMonth"), out LaunchesThisMonth);
                SetCosts();
                ConfigNode upgradeNode = cn.GetNode("UPGRADE");
                if (upgradeNode == null) return;
                Upgrading = true;
                Upgrade = new FacilityUpgradeEvent(upgradeNode.GetValue("ID"), this);
                Upgrade.OnLoad(upgradeNode);
            }
        }

        public void OnSave(ConfigNode cn)
        {
            ConfigNode thisNode = new ConfigNode("FACILITY");
            thisNode.SetValue("Name", Name, true);
            thisNode.SetValue("Level", level, true);
            thisNode.SetValue("RecentlyUpgraded", recentlyUpgraded, true);
            thisNode.SetValue("Closed", isClosed, true);
            thisNode.SetValue("LaunchesThisMonth", LaunchesThisMonth, true);
            if (Upgrading) Upgrade.OnSave(thisNode);
            cn.AddNode(thisNode);
        }

        public void OnUpgradeCompleted()
        {
            Upgrading = false;
            Upgrade = null;
            recentlyUpgraded = true;
            level++;
            Debug.Log("[Bureaucracy]: Upgrade of "+Name+" completed");
        }
    }
}