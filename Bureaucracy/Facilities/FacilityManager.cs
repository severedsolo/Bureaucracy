using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Upgradeables;

namespace Bureaucracy
{
    public class FacilityManager : Manager
    {
        public List<BureaucracyFacility> Facilities = new List<BureaucracyFacility>();
        public static FacilityManager Instance;

        public FacilityManager()
        {
            InternalEvents.OnBudgetAboutToFire.Add(RunFacilityBudget);
            SpaceCenterFacility[] spaceCentreFacilities = (SpaceCenterFacility[]) Enum.GetValues(typeof(SpaceCenterFacility));
            for (int i = 0; i < spaceCentreFacilities.Length; i++)
            {
                SpaceCenterFacility spf = spaceCentreFacilities.ElementAt(i);
                
                if(spf == SpaceCenterFacility.LaunchPad || spf == SpaceCenterFacility.Runway) continue;
                Facilities.Add(new BureaucracyFacility(spf));
            }
            Name = "Facility Manager";
            Instance = this;
        }
        
        public override void UnregisterEvents()
        {
            InternalEvents.OnBudgetAboutToFire.Remove(RunFacilityBudget);
        }

        public override double GetAllocatedFunding()
        {
            return Math.Round(Utilities.Instance.GetNetBudget("Facilities"), 0);
        }

        public override Report GetReport()
        {
            return new FacilityReport();
        }

        private void RunFacilityBudget()
        {
            double facilityBudget = Utilities.Instance.GetNetBudget("Facilities");
            for (int i = 0; i < Facilities.Count; i++)
            {
                BureaucracyFacility bf = Facilities.ElementAt(i);
                if(!bf.Upgrading) continue;
                facilityBudget = bf.Upgrade.ProgressUpgrade(facilityBudget);
                if (facilityBudget <= 0.0f) return;
            }
        }
        
        public void OnLoad(ConfigNode cn)
        {
            ConfigNode[] facilityNodes = cn.GetNodes("FACILITY");
            for (int i = 0; i < Facilities.Count; i++)
            {
                BureaucracyFacility bf = Facilities.ElementAt(i);
                bf.OnLoad(facilityNodes);
            }
        }
        
        

        public void OnSave(ConfigNode cn)
        {
            for (int i = 0; i < Facilities.Count; i++)
            {
                BureaucracyFacility bf = Facilities.ElementAt(i);
                bf.OnSave(cn);
            }
        }

        public void StartUpgrade(UpgradeableFacility facility)
        {
            BureaucracyFacility facilityToUpgrade = UpgradeableToActualFacility(facility);
            if (facilityToUpgrade == null)
            {
                Debug.Log("[Bureaucracy]: Upgrade of "+facility.id+" requested but no facility found");
                return;
            }
            Debug.Log("[Bureaucracy]: Upgrade of "+facility.id+" requested");
            if (facilityToUpgrade.Upgrading)
            {
                Debug.Log("[Bureaucracy]: "+facility.id+" is already being upgraded");
                ScreenMessages.PostScreenMessage(facilityToUpgrade.Name + " is already being upgraded");
                return;
            }
            facilityToUpgrade.StartUpgrade(facility);
        }

        private BureaucracyFacility UpgradeableToActualFacility(UpgradeableFacility facility)
        {
            for (int i = 0; i < Facilities.Count; i++)
            {
                BureaucracyFacility bf = Facilities.ElementAt(i);
                if(!facility.id.Contains(bf.Name)) continue;
                return bf;
            }
            return null;
        }
    }
}