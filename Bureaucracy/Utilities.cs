
using System;
using System.Linq;

namespace Bureaucracy
{
    public class Utilities
    {
        public static Utilities Instance;

        public Utilities()
        {
            Instance = this;
        }
        public void NewKacAlarm(string alarmName, double alarmTime)
        {
            if (!Bureaucracy.Instance.settings.StopTimeWarp) return;
            if (!KacWrapper.AssemblyExists) return;
            if (!KacWrapper.ApiReady) return;
            KacWrapper.Kac.CreateAlarm(KacWrapper.Kacapi.AlarmTypeEnum.Raw, alarmName, alarmTime);
        }
        
        public double GetGrossBudget()
        {
            return Reputation.Instance.reputation * SettingsClass.Instance.BudgetMultiplier;
        }

        public double GetNetBudget(string department)
        {
            
            double funding = GetGrossBudget();
            double costs = Costs.Instance.GetTotalMaintenanceCosts();
            funding -= costs;
            float allocation = 1.0f;
            switch (department)
            {
                case "Budget":
                {
                    for (int i = 0; i < Bureaucracy.Instance.registeredManagers.Count; i++)
                    {
                        Manager m = Bureaucracy.Instance.registeredManagers.ElementAt(i);
                        if (m == BudgetManager.Instance) continue;
                        allocation -= m.FundingAllocation / 100.0f;
                    }
                    if (funding < 0.0f) return funding;
                    return funding*allocation;
                }
                case "Facilities":
                    return Math.Max(funding * FacilityManager.Instance.FundingAllocation / 100.0f, 0);
                case "Research":
                    return Math.Max(funding * ResearchManager.Instance.FundingAllocation / 100.0f, 0);
            }
            return 0;
        }
    }
}