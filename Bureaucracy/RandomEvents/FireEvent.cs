using System.Linq;

namespace Bureaucracy
{
    public class FireEvent : RandomEventBase
    {
        private readonly BureaucracyFacility facilityToBurn;
        public FireEvent()
        {
            facilityToBurn = FacilityManager.Instance.Facilities.ElementAt(Utilities.Instance.Randomise.Next(0, FacilityManager.Instance.Facilities.Count));
            title = "Fire!";
            body = "Recent cutbacks have resulted in poor safety protocols at " + facilityToBurn.Name + ". As a result, a small fire has gotten out of control";
            acceptString = "Oh dear. (" + facilityToBurn.Name + " is destroyed)";
            CanBeDeclined = false;
        }
        public override bool EventCanFire()
        {
            if (Utilities.Instance.Randomise.NextDouble() > FacilityManager.Instance.FireChance) return false;
            return facilityToBurn.CanBeDestroyed();
        }

        protected override void OnEventAccepted()
        {
            facilityToBurn.DestroyBuilding();
        }

        protected override void OnEventDeclined()
        {
            
        }
    }
}