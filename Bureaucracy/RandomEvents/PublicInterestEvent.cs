using System;
using System.Collections.Generic;
using System.Linq;
using FinePrint.Utilities;
using UnityEngine;

namespace Bureaucracy
{
    public class PublicInterestEvent : RandomEventBase
    {
        private string bodyName = String.Empty;
        protected override string AcceptedString()
        {
            string[] message = new string[2];
            message[0] = "Excellent news!";
            message[1] = "(Reputation increased)";
            return BuildMessage(message);
        }

        protected override string DeclinedString()
        {
            return String.Empty;
        }

        protected override Rect WindowSize()
        {
            return new Rect(0.5f, 0.5f, 300,130);
        }

        public override bool EventIsValid()
        {
#if DEBUG
            bodyName = "Kerbin";
            return true;
#endif
            if (Reputation.Instance.reputation <= 0.0f) return false;
            int tries = 0;
            while (tries < 50)
            {
                CelestialBody c = FinePrint.Utilities.CelestialUtilities.RandomBody(FlightGlobals.Bodies);
                if (ProgressUtilities.GetBodyProgress(ProgressType.FLYBY, c))
                {
                    bodyName = c.displayName;
                    return true;
                }
                tries++;
            }
            if (Utilities.Instance.GetARandomKerbal() != String.Empty) return true;
            return false;
        }

        protected override string EventName()
        {
            return "Good News Everybody!";
        }

        protected override string EventBody()
        {
            string[] message = new string[3];
            message[0] = GetBodyChoice();
            message[1] = String.Empty;
            message[2] = "Interest in the space program has increased";
            return BuildMessage(message);
        }

        protected override string GetBodyChoice()
        {
            List<string> outcomes = new List<string>();
            if(bodyName != String.Empty)outcomes.Add("Recently released pictures of " + bodyName + " have sparked the publics imagination");
            outcomes.Add("A truly remarkable speech by "+Utilities.Instance.GetARandomKerbal()+" recently has been attracting alot of media attention");
            outcomes.Add("Don't look now but "+Utilities.Instance.GetARandomKerbal()+" has been making headlines with their groundbreaking theories on the origins of Mystery Goo");
            return outcomes.ElementAt(Utilities.Instance.Randomise.Next(0, outcomes.Count));
        }

        protected override bool EventCanBeDeclined()
        {
            return false;
        }

        protected override void OnEventAccepted()
        {
            Reputation.Instance.AddReputation(Reputation.Instance.reputation*0.1f, TransactionReasons.Progression);
        }

        protected override void OnEventDeclined()
        {
            return;
        }
    }
}