
namespace Bureaucracy
{
    public class TrainingEvent : RandomEventBase
    {
        public TrainingEvent(ConfigNode eventNode)
        {
            LoadConfig(eventNode);
        }


        public override bool EventCanFire()
        {
            return CrewManager.Instance.Kerbals[KerbalName].CrewReference() != null && !CrewManager.Instance.Kerbals[KerbalName].CrewReference().inactive;
        }

        protected override void OnEventAccepted()
        {
            CrewManager.Instance.Kerbals[KerbalName].CrewReference().SetInactive(EventEffect*FlightGlobals.GetHomeBody().solarDayLength);
        }

        protected override void OnEventDeclined()
        {
        }
    }
}