namespace Bureaucracy
{
    public class CrewUnhappiness
    {
        private int expiry = SettingsClass.Instance.StrikeMemory;
        private readonly CrewMember parentCrew;

        public string Reason { get; }

        public CrewUnhappiness(string reason, CrewMember passingCrewMember)
        {
            Reason = reason;
            parentCrew = passingCrewMember;
        }

        public bool ClearStrike()
        {
            parentCrew.Unhappy = false;
            expiry--;
            return expiry <= 0;
        }
    }
}