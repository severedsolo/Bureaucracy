using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Bureaucracy
{
    public class Costs
    {
        private int launchCostsVab;
        private int launchCostsSph;
        public static Costs Instance;

        public Costs()
        {
            Instance = this;
        }

        public void AddLaunch(ShipConstruct ship)
        {
            if (ship.shipFacility == EditorFacility.SPH) launchCostsSph += SettingsManager.Instance.launchCostSPH;
            else launchCostsVab += SettingsManager.Instance.launchCostVAB;
        }

        public double GetMaintenanceCosts()
        {
            double costs = 0;
            costs += GetFacilityMaintenanceCosts();
            costs += GetWageCosts();
            costs += GetLaunchCosts();
            return costs;
        }

        private double GetLaunchCosts()
        {
            return launchCostsSph + launchCostsVab;
        }

        private double GetWageCosts()
        {
            float validCrewCount = 0;
            List<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
            for (int i = 0; i < crew.Count; i++)
            {
                ProtoCrewMember p = crew.ElementAt(i);
                if(p.type == ProtoCrewMember.KerbalType.Applicant) continue;
                if(p.type == ProtoCrewMember.KerbalType.Tourist) continue;
                if(p.type == ProtoCrewMember.KerbalType.Unowned) continue;
                if(p.rosterStatus == ProtoCrewMember.RosterStatus.Dead || p.rosterStatus == ProtoCrewMember.RosterStatus.Missing) continue;
                float experienceLevel = Math.Max(0.5f, p.experienceLevel);
                validCrewCount += experienceLevel;
            }
            return validCrewCount * SettingsManager.Instance.kerbalBaseWage;
        }

        private double GetFacilityMaintenanceCosts()
        {
            double d = 0;
            for (int i = 0; i < FacilityManager.Instance.Facilities.Count; i++)
            {
                BureaucracyFacility bf = FacilityManager.Instance.Facilities.ElementAt(i);
                d += bf.MaintenanceCost;
            }

            return d;
        }

        public void OnLoad(ConfigNode cn)
        {
            ConfigNode costsNode = cn.GetNode("COSTS");
            if (costsNode == null) return;
            int.TryParse(costsNode.GetValue("launchCostsVAB"), out launchCostsVab);
            int.TryParse(costsNode.GetValue("launchCostsSPH"), out launchCostsSph);
        }

        public void OnSave(ConfigNode cn)
        {
            ConfigNode costsNode = new ConfigNode("COSTS");
            costsNode.SetValue("launchCostsVAB", launchCostsVab, true);
            costsNode.SetValue("launchCostsSPH", launchCostsSph, true);
            cn.AddNode(costsNode);
        }
    }
}