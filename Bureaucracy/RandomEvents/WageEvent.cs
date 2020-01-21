using System.Linq;

namespace Bureaucracy
{
    public class WageEvent : RandomEventBase
    {
        private CrewMember crewMember;

        public WageEvent(ConfigNode eventNode)
        {
            CrewMember c = FindCrew();
            if (c == null) return;
            crewMember = c;
            LoadConfig(eventNode);
            body = body.Replace("<crew>", c.Name);
            acceptString = acceptString.Replace("<crew>", c.Name);
        }

        private CrewMember FindCrew()
        {
            if (CrewManager.Instance.Kerbals.Count == 0) return null;
            int i = Utilities.Instance.Randomise.Next(0, CrewManager.Instance.Kerbals.Count);
            CrewMember c = CrewManager.Instance.Kerbals.ElementAt(i).Value;
            return c;
        }

        public override bool EventCanFire()
        {
            if (crewMember.CrewReference().rosterStatus != ProtoCrewMember.RosterStatus.Available) return false;
            if (crewMember.WageModifier <= 1.0f && EventEffect < 0.0f) return false;
            return true;
        }

        protected override void OnEventAccepted()
        {
            crewMember.WageModifier += EventEffect;
        }

        protected override void OnEventDeclined()
        {
            
        }
    }
}