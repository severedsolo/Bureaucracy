
using System;

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
            InternalListeners.OnBudgetAboutToFire.Fire();
            RepDecay repDecay = new RepDecay();
            repDecay.ApplyHardMode();
            double funding = Utilities.Instance.GetNetBudget("Budget");
            funding -= CrewManager.Instance.Bonuses(funding);
            double facilityDebt = Costs.Instance.GetFacilityMaintenanceCosts();
            double wageDebt = Math.Abs(funding + facilityDebt);

            if (funding < 0)
            {
                //pay wages first then facilities
                Utilities.Instance.PayWageDebt(wageDebt);
                Utilities.Instance.PayFacilityDebt(facilityDebt, wageDebt);
            }
            else FacilityManager.Instance.ReopenAllFacilities();
            CrewManager.Instance.ProcessUnhappyCrew();
            if(SettingsClass.Instance.UseItOrLoseIt && funding > Funding.Instance.Funds) Funding.Instance.SetFunds(0.0d, TransactionReasons.Contracts);
            if(!SettingsClass.Instance.UseItOrLoseIt || Funding.Instance.Funds <= 0.0d || funding <= 0.0d) Funding.Instance.AddFunds(funding, TransactionReasons.Contracts);
            InternalListeners.OnBudgetAwarded.Fire(funding, Costs.Instance.GetTotalMaintenanceCosts());
            Costs.Instance.ResetLaunchCosts();
            repDecay.ApplyRepDecay(Bureaucracy.Instance.settings.RepDecayPercent);
            InformParent();
        }
        
    }
}