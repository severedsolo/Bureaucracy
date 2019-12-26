using System;
using System.Linq;
using Expansions.Missions.Tests;
using UnityEngine;
using Upgradeables;
using VehiclePhysics;

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
                    return "AstronautComplex";
                case SpaceCenterFacility.MissionControl:
                    return "MissionControl";
                case SpaceCenterFacility.SpaceplaneHangar:
                    return "SpacePlaneHangar";
                case SpaceCenterFacility.TrackingStation:
                    return "TrackingStation";
                case SpaceCenterFacility.ResearchAndDevelopment:
                    return "ResearchAndDevelopment";
                case SpaceCenterFacility.VehicleAssemblyBuilding:
                    return "VehicleAssemblyBuilding";
                default:
                    return "OtherFacility";
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
                case "AstronautComplex:":
                    cost = SettingsClass.Instance.AstronautComplexCost;
                    break;
                case "MissionControl":
                    cost = SettingsClass.Instance.MissionControlCost;
                    break;
                case "SpacePlaneHangar":
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
                    Debug.Log("[Bureaucracy]: Facility " + name + " could not be found!");
                    break;
            }

            return cost;
        }

        public void StartUpgrade(UpgradeableFacility facilityToUpgrade)
        {
            Upgrade = new FacilityUpgradeEvent(facilityToUpgrade, this);
            upgrading = true;
            ScreenMessages.PostScreenMessage("[Bureacracy]: Upgrade of " + name + " requested");
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
                SetCosts();
                ConfigNode upgradeNode = cn.GetNode("UPGRADE");
                if (upgradeNode != null)
                {
                    upgrading = true;
                    Upgrade = new FacilityUpgradeEvent(FacilityManager.Instance.ActualFacilityToUpgradeableFacility(this), this);
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
            if (upgrading) Upgrade.OnSave(thisNode);
            cn.AddNode(thisNode);
        }

        public void OnUpgradeCompleted()
        {
            upgrading = false;
            Upgrade = null;
            recentlyUpgraded = true;
        }
    }
}