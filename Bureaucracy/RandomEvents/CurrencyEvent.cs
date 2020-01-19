using System;

namespace Bureaucracy
{
    public class CurrencyEvent : RandomEventBase
    {
        private readonly string currency;
        public CurrencyEvent(ConfigNode eventNode)
        {
            LoadConfig(eventNode);
            if(!eventNode.TryGetValue("CurrencyType", ref currency)) throw new ArgumentException(Name+" is designated as Currency Event but no currency declared");
        }


        public override bool EventCanFire()
        {
            try
            {

                switch (currency)
                {
                    case "Funds":
                        if (EventEffect < 0 && !Funding.CanAfford(EventEffect)) return false;
                        break;
                    case "Science":
                        if (ResearchManager.Instance.ScienceMultiplier > 1.2f && EventEffect > 0) return false;
                        if (ResearchManager.Instance.ScienceMultiplier < 0.8f && EventEffect < 0) return false;
                        break;
                    case "Reputation":
                        if (Reputation.Instance.reputation > 500 && EventEffect > 0) return false;
                        if (Reputation.Instance.reputation < 45 && EventEffect < 0) return false;
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
                    Funding.Instance.AddFunds(EventEffect, TransactionReasons.None);
                    break;
                case "Science":
                    ResearchManager.Instance.ScienceMultiplier += EventEffect;
                    break;
                case "Reputation":
                    Reputation.Instance.AddReputation(Reputation.Instance.reputation * EventEffect, TransactionReasons.None);
                    break;
            }
        }

        protected override void OnEventDeclined()
        {
        }
    }
}