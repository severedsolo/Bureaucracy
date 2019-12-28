namespace Bureaucracy
{
    public class CrewUnhappiness
    {
        private int expiry = SettingsClass.Instance.StrikeMemory;
        private string unhappinessReason;
        private CrewMember parentCrew;

        public string Reason => unhappinessReason;

        public CrewUnhappiness(string reason, CrewMember passingCrewMember)
        {
            unhappinessReason = reason;
            parentCrew = passingCrewMember;
        }

        public bool ClearStrike()
        {
            parentCrew.Unhappy = false;
            expiry--;
            if (expiry <= 0) return true;
            return false;
        }
    }
}