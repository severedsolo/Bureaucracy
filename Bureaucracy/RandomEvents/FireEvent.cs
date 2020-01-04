using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bureaucracy
{
    public class FireEvent : RandomEventBase
    {
        private string buildingToBurn;

        protected override string AcceptedString()
        {
            if (!Funding.CanAfford(50000)) return "Tragic (" + buildingToBurn + " is destroyed)";
            return "We need to save it! ($50,000)";
        }

        protected override string DeclinedString()
        {
            return "Tragic (" + buildingToBurn + " is destroyed)";
        }

        protected override Rect WindowSize()
        {
            return new Rect(0.5f, 0.5f, 300, 90);
        }

        public override bool EventIsValid()
        {
            BureaucracyFacility bf = FacilityManager.Instance.Facilities.ElementAt(Utilities.Instance.Randomise.Next(0, FacilityManager.Instance.Facilities.Count));
            buildingToBurn = FacilityManager.Instance.Facilities.ElementAt(Utilities.Instance.Randomise.Next(0, FacilityManager.Instance.Facilities.Count)).Name;
#if DEBUG
            return true;
#endif
            if (bf.Level == 1 || FacilityManager.Instance.GetFacilityByName(buildingToBurn).Upgrading || bf.IsDestroyed()) return false;
            return true;
        }

        protected override string EventName()
        {
            return "Fire!";
        }

        protected override string EventBody()
        {
            return "A fire has broken out at " + buildingToBurn;
        }

        protected override bool EventCanBeDeclined()
        {
            if (Funding.CanAfford(50000)) return true;
            return false;
        }

        protected override void OnEventAccepted()
        {
            if (!Funding.CanAfford(50000)) DestroyBuilding();
            Funding.Instance.AddFunds(-50000, TransactionReasons.Structures);
        }

        protected override void OnEventDeclined()
        {
            DestroyBuilding();
        }

        private void DestroyBuilding()
        {
            foreach (KeyValuePair<string, ScenarioDestructibles.ProtoDestructible> kvp in ScenarioDestructibles.protoDestructibles)
            {
                if (!kvp.Key.Contains(buildingToBurn)) continue;
                List<DestructibleBuilding> buildingsToDestroy = kvp.Value.dBuildingRefs;
                foreach (var building in buildingsToDestroy)
                {
                    building.Demolish();
                }
            }
        }
    }
}