
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
            InternalEvents.OnBudgetAboutToFire.Fire();
            RepDecay repDecay = new RepDecay();
            repDecay.ApplyHardMode();
            double funding = Utilities.Instance.GetNetBudget("Budget");
            if(SettingsClass.Instance.UseItOrLoseIt && funding > Funding.Instance.Funds) Funding.Instance.SetFunds(0.0f, TransactionReasons.Contracts);
            if(!SettingsClass.Instance.UseItOrLoseIt || Funding.Instance.Funds == 0.0f) Funding.Instance.AddFunds(funding, TransactionReasons.Contracts);
            InternalEvents.OnBudgetAwarded.Fire(funding, Costs.Instance.GetTotalMaintenanceCosts());
            repDecay.ApplyRepDecay(Bureaucracy.Instance.settings.RepDecayPercent);
            InformParent();
        }
        
    }
}