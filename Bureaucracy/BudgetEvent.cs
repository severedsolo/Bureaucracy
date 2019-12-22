
namespace Bureaucracy
{
    public class BudgetEvent : BureaucracyEvent
    {
        public BudgetEvent(double budgetTime, BudgetManager manager, bool newKacaLarm)
        {
            CompletionTime = budgetTime;
            Name = "Next Budget";
            ParentManager = manager;
            if(newKacaLarm) Utilities.Instance.NewKacAlarm("Next Budget", CompletionTime);
            AddTimer();
        }

        public override void OnEventCompleted()
        {
            RepDecay repDecay = new RepDecay();
            repDecay.ApplyHardMode();
            double funding = BudgetManager.Instance.GetNetBudget();
            if (Bureaucracy.Instance.settings.UseItOrLoseIt) Funding.Instance.SetFunds(0, TransactionReasons.Contracts);
            Funding.Instance.AddFunds(funding, TransactionReasons.Contracts);
            repDecay.ApplyRepDecay(Bureaucracy.Instance.settings.RepDecayPercent);
            if(SettingsManager.Instance.StopTimeWarp)TimeWarp.SetRate(0, true);
            InformParent();
        }
        
    }
}