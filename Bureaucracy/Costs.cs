using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using UnityEngine;

namespace Bureaucracy
{
    public class Costs
    {
        private int launchCostsVab;
        private int launchCostsSph;
        private bool costsDirty = true;
        public static Costs Instance;
        private double cachedCosts;

        public Costs()
        {
            Instance = this;
        }

        public void AddLaunch(ShipConstruct ship)
        {
            if (ship.shipFacility == EditorFacility.SPH) launchCostsSph += SettingsClass.Instance.LaunchCostSph;
            else launchCostsVab += SettingsClass.Instance.LaunchCostVab;
        }

        public void ResetLaunchCosts()
        {
            launchCostsSph = 0;
            launchCostsVab = 0;
        }
        
        public double GetTotalMaintenanceCosts()
        {
            Debug.Log("[Bureaucracy]: Maintenance Costs requested");
            if (!costsDirty)
            {
                Debug.Log("[Bureaucracy]: Returning cached costs: "+cachedCosts);
                return cachedCosts;
            }
            Debug.Log("[Bureaucracy]: Costs are dirty. Recalculating");
            double costs = 0;
            costs += GetFacilityMaintenanceCosts();
            costs += GetWageCosts();
            costs += GetLaunchCosts();
            cachedCosts = costs;
            costsDirty = false;
            Debug.Log("[Bureaucracy]: Cached costs "+costs+". Costs are not dirty");
            Bureaucracy.Instance.Invoke(nameof(Bureaucracy.Instance.SetCalcsDirty), 5.0f);
            return costs;
        }

        public void SetCalcsDirty()
        {
            costsDirty = true;
            Debug.Log("[Bureaucracy]: Costs are dirty");
        }

        public double GetLaunchCosts()
        {
            return launchCostsSph + launchCostsVab;
        }

        public double GetWageCosts()
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
            return validCrewCount * SettingsClass.Instance.KerbalBaseWage;
        }

        public double GetFacilityMaintenanceCosts()
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