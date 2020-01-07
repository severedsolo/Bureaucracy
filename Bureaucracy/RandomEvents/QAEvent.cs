using System;

namespace Bureaucracy
{
    public class QaEvent : RandomEventBase
    {
        public QaEvent(ConfigNode eventNode)
        {
            LoadConfig(eventNode);
            canBeDeclined = true;
            declineString = "No thanks";
        }

        public override bool EventCanFire()
        {
            if (Bureaucracy.Instance.qaModifier < 0.8f && eventEffect < 0) return false;
            if (Bureaucracy.Instance.qaModifier >= 1.0f && eventEffect > 0) return false;
            return true;
        }

        protected override void OnEventAccepted()
        {
            Bureaucracy.Instance.qaModifier += eventEffect;
        }

        protected override void OnEventDeclined()
        {
            return;
        }
    }
}