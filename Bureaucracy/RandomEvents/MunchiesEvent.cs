using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bureaucracy
{
    public class MunchiesEvent : RandomEventBase
    {
        private string kerbalWhoGotHigh;
        protected override string AcceptedString()
        {
            return "Not again. ("+kerbalWhoGotHigh + " is unavailable for 7 days)";
        }

        protected override string DeclinedString()
        {
            return String.Empty;
        }

        protected override Rect WindowSize()
        {
            return new Rect(0.5f, 0.5f, 300, 140);
        }

        public override bool EventIsValid()
        {
            kerbalWhoGotHigh = Utilities.Instance.GetARandomKerbal();
            if (CrewManager.Instance.Kerbals[kerbalWhoGotHigh].CrewReference().inactive) return false;
            return kerbalWhoGotHigh != String.Empty;
        }

        protected override string EventName()
        {
            return "Crew Issue";
        }

        protected override string EventBody()
        {
            return GetBodyChoice();
        }

        protected override string GetBodyChoice()
        {
            List<string> outcomes = new List<string>();
            outcomes.Add("During a recent training session it emerged that " + kerbalWhoGotHigh + " has been eating a new type of Snacks called \"Munchies\" which affected their performance. They have been put back into training while they think about what they did");
            outcomes.Add(kerbalWhoGotHigh+" has failed their recertification. They have been put back into training");
            outcomes.Add(kerbalWhoGotHigh+" had an accident while handling a sample of Mystery Goo. The review following the accident determined that "+kerbalWhoGotHigh+" could do with some extra training");
            return outcomes.ElementAt(Utilities.Instance.Randomise.Next(0, outcomes.Count));
        }

        protected override bool EventCanBeDeclined()
        {
            return false;
        }

        protected override void OnEventAccepted()
        {
            List<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
            for (int i = 0; i < crew.Count; i++)
            {
                ProtoCrewMember p = crew.ElementAt(i);
                if (p.name != kerbalWhoGotHigh) continue;
                p.SetInactive(FlightGlobals.GetHomeBody().solarDayLength * SettingsClass.Instance.TimeBetweenBudgets);
                return;
            }
        }

        protected override void OnEventDeclined()
        {
            return;
        }
    }
}