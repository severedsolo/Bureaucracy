using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bureaucracy
{
    public class AliensEvent : RandomEventBase
    {
        private string kerbalWhoFuckedUp = String.Empty;
        protected override string AcceptedString()
        {
            string[] message = new string[2];
            message[0] = "Well that's not good";
            message[1] = "(Reputation decreases by 10%";
            return BuildMessage(message);
        }

        protected override string DeclinedString()
        {
            return null;
        }

        protected override Rect WindowSize()
        {
            return new Rect(0.5f, 0.5f, 300,160);
        }

        public override bool EventIsValid()
        {
            #if DEBUG
            return true;
            #endif
            kerbalWhoFuckedUp = Utilities.Instance.GetARandomKerbal();
            if (kerbalWhoFuckedUp == String.Empty) return false;
            return Reputation.Instance.reputation > 0;
        }

        protected override string EventName()
        {
            return "PR Crisis";
        }

        protected override string EventBody()
        {
            string[] message = new string[3];
            message[0] = GetBodyChoice();
            message[1] = String.Empty;
            message[2] = "The reputation of the space program has been damaged";
            return BuildMessage(message);
        }

        protected override string GetBodyChoice()
        {
            List<string> outcomes = new List<string>();
            outcomes.Add("During a recent scientific symposium, " + kerbalWhoFuckedUp + " mentioned that they believe intelligent alien life exists on " + FlightGlobals.GetHomeBody().orbitingBodies.FirstOrDefault().name + " but hasn't been found yet.");
            outcomes.Add("One of your competitors has been pushing rumours that your latest designs are not safe. We've put out a press statement correcting them, but the damage has already been done");
            outcomes.Add("Photos of "+kerbalWhoFuckedUp+" at a \"Munchies party\" have recently surfaced");
            return outcomes.ElementAt(Utilities.Instance.Randomise.Next(0, outcomes.Count));
        }

        protected override bool EventCanBeDeclined()
        {
            return false;
        }

        protected override void OnEventAccepted()
        {
            Reputation.Instance.AddReputation(-Reputation.Instance.reputation*0.1f, TransactionReasons.ContractPenalty);
        }

        protected override void OnEventDeclined()
        {
            return;
        }
    }
}