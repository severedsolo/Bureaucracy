using Contracts;
using System.Linq;
using UnityEngine;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class ContractInterceptor : MonoBehaviour
    {
        public static ContractInterceptor Instance;
        protected void Awake()
        {
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER) Destroy(this);
            else
            {
                DontDestroyOnLoad(this);
                Instance = this;
            }
        }

        public void OnContractOffered(Contract contract)
        {
            if (!SettingsClass.Instance.ContractInterceptor) return;
            if (contract.FundsCompletion <= 0) return;
            //Set Failure Penalty to Advance - Failure Rep.
            float rep = (float)contract.FundsAdvance / 10000 * -1 - (float)contract.FundsFailure / 10000;
            contract.FundsFailure = 0;
            contract.ReputationFailure = rep - contract.ReputationFailure;
            rep = (float)contract.FundsAdvance / 10000 + (float)contract.FundsCompletion / 10000;
            for (int i = 0; i < contract.AllParameters.Count(); i++)
            {
                ContractParameter p = contract.AllParameters.ElementAt(i);
                rep += (float)p.FundsCompletion / 10000;
                p.FundsCompletion = 0;
            }
            contract.ReputationCompletion += rep;
            if (contract.ReputationCompletion < 1) contract.ReputationCompletion = 1;
            contract.FundsAdvance = 0;
            contract.FundsCompletion = 0;
        }

        public void OnDisable()
        {
            GameEvents.Contract.onOffered.Remove(OnContractOffered);
        }
    }
}
