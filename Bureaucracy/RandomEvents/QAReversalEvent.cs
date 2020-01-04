using System;
using UnityEngine;

namespace Bureaucracy
{
    public class QAReversalEvent:RandomEventBase

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
            return Bureaucracy.Instance.qaModifier < 1.0f;
        }

        protected override string EventName()
        {
            return "QA Concerns";
        }

        protected override string EventBody()
        {
            string[] message = new string [3];
            message[0] = "Some of our engineers have expressed concern at the cost saving measures, and are afraid that QA may suffer as a result. They would like us to reverse the cuts.";
            message[1] = String.Empty;
            message[2] = "Accept: Maintenance costs will be reset to default. Vessels will no longer have a chance of failing at launch";
            return BuildMessage(message);
        }

        protected override bool EventCanBeDeclined()
        {
            return true;
        }

        protected override void OnEventAccepted()
        {
            Bureaucracy.Instance.qaModifier = 1.0f;
        }

        protected override void OnEventDeclined()
        {
            return;
        }
    }
}