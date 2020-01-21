namespace Bureaucracy
{
    public class QaEvent : RandomEventBase
    {
        public QaEvent(ConfigNode eventNode)
        {
            LoadConfig(eventNode);
        }

        public override bool EventCanFire()
        {
            if (EventEffect < 0.0f && FacilityManager.Instance.FireChance <= 0.0f) return false;
            return true;
        }

        protected override void OnEventAccepted()
        {
            FacilityManager.Instance.CostMultiplier += EventEffect;
        }

        protected override void OnEventDeclined()
        {
            FacilityManager.Instance.FireChance += EventEffect;
        }
    }
}