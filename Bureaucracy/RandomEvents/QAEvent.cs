namespace Bureaucracy
{
    public class QaEvent : RandomEventBase
    {
        public QaEvent(ConfigNode eventNode)
        {
            LoadConfig(eventNode);
            CanBeDeclined = true;
            DeclineString = "No thanks";
        }

        public override bool EventCanFire()
        {
            if (Bureaucracy.Instance.qaModifier < 0.8f && EventEffect < 0) return false;
            if (Bureaucracy.Instance.qaModifier >= 1.0f && EventEffect > 0) return false;
            return true;
        }

        protected override void OnEventAccepted()
        {
            Bureaucracy.Instance.qaModifier += EventEffect;
        }

        protected override void OnEventDeclined()
        {
        }
    }
}