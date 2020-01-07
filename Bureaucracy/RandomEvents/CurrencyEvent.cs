using System;
using UnityEngine;

namespace Bureaucracy
{
    public class CurrencyEvent : RandomEventBase
    {
        private string currency;
        public CurrencyEvent(ConfigNode eventNode)
        {
            LoadConfig(eventNode);
            if(!eventNode.TryGetValue("CurrencyType", ref currency)) throw new ArgumentException(name+" is designated as Currency Event but no currency declared");
        }


        public override bool EventCanFire()
        {
            try
            {

                switch (currency)
                {
                    case "Funds":
                        if (eventEffect < 0 && !Funding.CanAfford(eventEffect)) return false;
                        break;
                    case "Science":
                        if (ResearchManager.Instance.scienceMultiplier > 1.2f && eventEffect > 0) return false;
                        if (ResearchManager.Instance.scienceMultiplier < 0.8f && eventEffect < 0) return false;
                        break;
                    case "Reputation":
                        if (Reputation.Instance.reputation > 500 && eventEffect > 0) return false;
                        if (Reputation.Instance.reputation < 45 && eventEffect < 0) return false;
                        break;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override void OnEventAccepted()
        {
            switch (currency)
            {
                case "Funds":
                    Funding.Instance.AddFunds(eventEffect, TransactionReasons.None);
                    break;
                case "Science":
                    ResearchManager.Instance.scienceMultiplier += eventEffect;
                    break;
                case "Reputation":
                    Reputation.Instance.AddReputation(Reputation.Instance.reputation * eventEffect, TransactionReasons.None);
                    break;
            }
        }

        protected override void OnEventDeclined()
        {
            return;
        }
    }
}