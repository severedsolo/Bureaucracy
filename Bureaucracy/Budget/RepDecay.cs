using UnityEngine;

namespace Bureaucracy
{
    public class RepDecay
    {
        public void ApplyHardMode()
        {
            if (!DecayIsValid(true)) return;
            Debug.Log("[Bureaucracy]: Applying Hard Mode");
            double penalty = Funding.Instance.Funds / 1000;
            Debug.Log("[Bureaucracy]: Penalising: "+penalty);
            Reputation.Instance.AddReputation((float)-penalty, TransactionReasons.ContractPenalty);
        }

        private static bool DecayIsValid(bool hardMode)
        {
            if (hardMode && !SettingsClass.Instance.HardMode) return false;
            return hardMode || SettingsClass.Instance.RepDecayEnabled;
        }

        public void ApplyRepDecay(int decayPercent)
        {
            if (!DecayIsValid(false)) return;
            Debug.Log("[Bureaucracy]: Applying Rep Decay");
            float decayFactor = decayPercent / 100.0f;
            Debug.Log("[Bureaucracy]: Rep Decay: "+Reputation.Instance.reputation*decayFactor);
            Reputation.Instance.SetReputation(Reputation.Instance.reputation*decayFactor, TransactionReasons.Contracts);
        }
    }
}