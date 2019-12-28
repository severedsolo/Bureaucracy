
using System.Linq;

namespace Bureaucracy
{
    public class BudgetManager : Manager
    {
        private BudgetEvent nextBudget;
        private Costs costs = new Costs();
        public static BudgetManager Instance;

        public BudgetManager()
        {
            Name = "Budget Manager";
            Instance = this;
        }

        protected override Report GetReport()
        {
            return new BudgetReport();
        }

        public override void OnEventCompletedManagerActions(BureaucracyEvent eventCompleted)
        {
            nextBudget = new BudgetEvent(GetNextBudgetTime(), this, true);
        }

        private double GetNextBudgetTime()
        {
            double time = SettingsClass.Instance.TimeBetweenBudgets;
            time *= FlightGlobals.GetHomeBody().solarDayLength;
            double offset = 0;
            if (nextBudget != null) offset = Planetarium.GetUniversalTime() - nextBudget.CompletionTime;
            time += Planetarium.GetUniversalTime() - offset;
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