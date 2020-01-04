using System;
using UnityEngine;

namespace Bureaucracy
{
    public class QAEvent : RandomEventBase
    {
        protected override string AcceptedString()
        {
            return "Accept";
        }

        protected override string DeclinedString()
        {
            return "Decline";
        }

        protected override Rect WindowSize()
        {
            return new Rect(0.5f, 0.5f, 300, 140);
        }

        public override bool EventIsValid()
        {
            return true;
        }

        protected override string EventName()
        {
            return "Cost Saving Initiative";
        }

        protected override string EventBody()
        {
            string[] message = new string[3];
            message[0] = "We've identified some efficiency savings that can be made at the KSC. This may impact our QA process though";
            message[1] = String.Empty;
            message[2] = "Accept: Maintenance costs will be reduced. " + (100-Math.Round((Bureaucracy.Instance.qaModifier-0.1f) * 100, 0)) + " chance of vessel failure on each launch";
            return BuildMessage(message);
        }

        protected override bool EventCanBeDeclined()
        {
            return true;
        }

        protected override void OnEventAccepted()
        {
            Bureaucracy.Instance.qaModifier -= 0.1f;
        }

        protected override void OnEventDeclined()
        {
            return;
        }
    }
}