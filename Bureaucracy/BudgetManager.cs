
namespace Bureaucracy
{
    public class BudgetManager : Manager
    {
        private BudgetEvent nextBudget;
        private Costs costs = new Costs();
        public static BudgetManager Instance;

        public BudgetManager()
        {
            Instance = this;
        }

        public override Report GetReport()
        {
            return new BudgetReport();
        }

        public double GetGrossBudget()
        {
            return Reputation.Instance.reputation * SettingsClass.Instance.BudgetMultiplier;
        }

        public double GetNetBudget()
        {
            double funding = GetGrossBudget();
            funding -= costs.GetTotalMaintenanceCosts();
            return funding;
        }

        public override void OnEventCompleted()
        {
            nextBudget = new BudgetEvent(GetNextBudgetTime(), this, true);
        }

        private double GetNextBudgetTime()
        {
            double time = SettingsClass.Instance.TimeBetweenBudgets;
            time *= FlightGlobals.GetHomeBody().solarDayLength;
            time += Planetarium.GetUniversalTime();
            return time;
        }
        
        public void OnLoad(ConfigNode cn)
        {
            ConfigNode managerNode = cn.GetNode("BUDGET_MANAGER");
            double nextBudgetTime = GetNextBudgetTime();
            if(managerNode != null) double.TryParse(managerNode.GetValue("nextBudget"), out nextBudgetTime);
            nextBudget = new BudgetEvent(nextBudgetTime, this, false);
            Costs.Instance.OnLoad(cn);
        }

        public void OnSave(ConfigNode cn)
        {
            ConfigNode managerNode = new ConfigNode("BUDGET_MANAGER");
            managerNode.SetValue("nextBudget", nextBudget.CompletionTime, true);
            cn.AddNode(managerNode);
            Costs.Instance.OnSave(cn);
        }
    }
}