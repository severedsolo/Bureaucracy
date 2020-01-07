
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
            return CrewManager.Instance.Kerbals[kerbalName].CrewReference() != null && !CrewManager.Instance.Kerbals[kerbalName].CrewReference().inactive;
        }

        protected override void OnEventAccepted()
        {
            CrewManager.Instance.Kerbals[kerbalName].CrewReference().SetInactive(eventEffect*FlightGlobals.GetHomeBody().solarDayLength);
        }

        protected override void OnEventDeclined()
        {
            return;
        }
    }
}