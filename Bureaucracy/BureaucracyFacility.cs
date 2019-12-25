using System;
using System.Linq;
using Expansions.Missions.Tests;
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
        public FacilityUpgradeEvent Upgrade;
        private bool recentlyUpgraded = false;

        public bool Upgrading => upgrading;

        public int MaintenanceCost => upkeepCost * level;

        public string Name => name;

        public BureaucracyFacility(UpgradeableFacility spf)
        {
            name = spf.id;
            upkeepCost = SetCosts();
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
                case "RnD":
                    cost = SettingsClass.Instance.RndCost;
                    break;
                case "VAB":
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

        public void StartUpgrade(UpgradeableFacility facility, int requestedLevel)
        {
            upgrading = true;
            if(Upgrade == null) Upgrade = new FacilityUpgradeEvent(facility, requestedLevel, this);
        }

        public string GetProgressReport()
        {
            if (!upgrading && !recentlyUpgraded) return String.Empty;
            if (recentlyUpgraded) return name + ": Upgrade completed successfully";
            return name+ ": $" + Upgrade.Cost + " remaining";
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
                    Upgrade = new FacilityUpgradeEvent(ScenarioUpgradeableFacilities.protoUpgradeables[name].facilityRefs, upgradeNode, name, this);
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