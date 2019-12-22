namespace Bureaucracy
{
    public class RepDecay
    {
        public void ApplyHardMode()
        {
            if (!DecayIsValid(true)) return;
            double penalty = Funding.Instance.Funds / 1000;
            Reputation.Instance.AddReputation((float)-penalty, TransactionReasons.ContractPenalty);
        }

        private bool DecayIsValid(bool hardMode)
        {
            if (hardMode && !SettingsManager.Instance.HardMode) return false;
            if (!hardMode && !SettingsManager.Instance.RepDecayEnabled) return false;
            return true;
        }

        public void ApplyRepDecay(int decayPercent)
        {
            if (!DecayIsValid(false)) return;
            float decayFactor = decayPercent / 100.0f;
            Reputation.Instance.SetReputation(Reputation.Instance.reputation*decayFactor, TransactionReasons.Contracts);
        }
    }
}