using System.Linq;
using UnityEngine;

namespace Bureaucracy
{
    public class BudgetManager : Manager
    {
        public BudgetEvent NextBudget;

        // ReSharper disable once UnusedMember.Local
        private Costs costs = new Costs();
        public static BudgetManager Instance;

        public BudgetManager()
        {
            Name = "Budget";
            Instance = this;
            FundingAllocation = 0.4f;
            Debug.Log("[Bureaucracy]: Budget Manager is ready");
        }

        protected override Report GetReport()
        {
            return new BudgetReport();
        }

        public override void OnEventCompletedManagerActions(BureaucracyEvent eventCompleted)
        {
            Debug.Log("[Bureaucracy]: Budget Event completed. Setting next budget");
            NextBudget = new BudgetEvent(GetNextBudgetTime(), this, true);
        }

        private double GetNextBudgetTime()
        {
            double time = SettingsClass.Instance.TimeBetweenBudgets;
            time *= FlightGlobals.GetHomeBody().solarDayLength;
            double offset = 0;
            if (NextBudget != null) offset = Planetarium.GetUniversalTime() - NextBudget.CompletionTime;
            time += Planetarium.GetUniversalTime() - offset;
            Debug.Log("[Bureaucracy]: Next Budget at "+time);
            return time;
        }
        
        public void OnLoad(ConfigNode cn)
        {
            Debug.Log("[Bureaucracy]: Budget Manager: OnLoad");
            ConfigNode managerNode = cn.GetNode("BUDGET_MANAGER");
            double nextBudgetTime = GetNextBudgetTime();
            if (managerNode != null)
            {
                int.TryParse(managerNode.GetValue("FundingAllocation"), out int i);
                FundingAllocation = i;
                double.TryParse(managerNode.GetValue("nextBudget"), out nextBudgetTime);
            }
            NextBudget = new BudgetEvent(nextBudgetTime, this, NeedNewKacAlarm());
            ConfigNode costsNode = cn.GetNode("COSTS");
            Costs.Instance.OnLoad(costsNode);
            Debug.Log("[Bureaucracy]: Budget Manager: OnLoad Complete");
        }

        private bool NeedNewKacAlarm()
        {
            if (!SettingsClass.Instance.StopTimeWarp) return false;
            if (!KacWrapper.AssemblyExists)
            {
                Debug.Log("[Bureaucracy]: Couldn't find KAC. I'll try again in 1 second");
                Bureaucracy.Instance.Invoke(nameof(Bureaucracy.Instance.RetryKacAlarm), 1.0f);
                return false;
            }
            KacWrapper.Kacapi.KacAlarmList kacAlarms = KacWrapper.Kac.Alarms;
            for (int i = 0; i < kacAlarms.Count; i++)
            {
                KacWrapper.Kacapi.KacAlarm alarm = kacAlarms.ElementAt(i);
                if (alarm.Name == "Next Budget") return false;
            }

            return true;
        }

        public void OnSave(ConfigNode cn)
        {
            Debug.Log("[Bureaucracy]: Budget Manager: OnSave");
            ConfigNode managerNode = new ConfigNode("BUDGET_MANAGER");
            managerNode.SetValue("FundingAllocation", FundingAllocation, true);
            managerNode.SetValue("nextBudget", NextBudget.CompletionTime, true);
            cn.AddNode(managerNode);
            Costs.Instance.OnSave(managerNode);
            Debug.Log("[Bureaucracy]: Budget Manager: OnSave Complete");
        }
    }
}