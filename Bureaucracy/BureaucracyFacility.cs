using System.Linq;
using UnityEngine;

namespace Bureaucracy
{
    public class BureaucracyFacility
    {
        private int level = 0;
        private int upkeepCost = 0;
        private string name;

        public int MaintenanceCost => upkeepCost * level;

        public BureaucracyFacility(SpaceCenterFacility spf)
        {
            name = SetName(spf);
            upkeepCost = SetCosts();
        }

        private int SetCosts()
        {
            int cost = 0;
            switch (name)
            {
                case "Administration":
                    cost = SettingsManager.Instance.AdminCost;
                    break;
                case "AstronautComplex:":
                    cost = SettingsManager.Instance.AstronautComplexCost;
                    break;
                case "MissionControl":
                    cost = SettingsManager.Instance.MissionControlCost;
                    break;
                case "SPH":
                    cost = SettingsManager.Instance.SphCost;
                    break;
                case "TrackingStation":
                    cost = SettingsManager.Instance.TrackingStationCost;
                    break;
                case "RnD":
                    cost = SettingsManager.Instance.RndCost;
                    break;
                case "VAB":
                    cost = SettingsManager.Instance.VabCost;
                    break;
                case "Other Facility":
                    cost = SettingsManager.Instance.OtherFacilityCost;
                    break;
                default:
                    Debug.Log("[Bureaucracy]: Facility " + name + " could not be found!");
                    break;
            }

            return cost;
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
                    return "SPH";
                case SpaceCenterFacility.TrackingStation:
                    return "TrackingStation";
                case SpaceCenterFacility.ResearchAndDevelopment:
                    return "RnD";
                case SpaceCenterFacility.VehicleAssemblyBuilding:
                    return "VAB";
                default:
                    return "OtherFacility";
            }
        }

        public void OnLoad(ConfigNode[] facilityNodes)
        {
            for (int i = 0; i < facilityNodes.Length; i++)
            {
                ConfigNode cn = facilityNodes.ElementAt(i);
                if (cn.GetValue("Name") != name) continue;
                int.TryParse(cn.GetValue("Level"), out level);
                SetCosts();
                return;
            }
        }

        public void OnSave(ConfigNode cn)
        {
            ConfigNode thisNode = new ConfigNode("FACILITY");
            thisNode.SetValue("Name", name, true);
            thisNode.SetValue("Level", level, true);
            cn.AddNode(thisNode);
        }
    }
}