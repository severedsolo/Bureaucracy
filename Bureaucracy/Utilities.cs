
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
            funding -= Costs.Instance.GetTotalMaintenanceCosts();
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

                    return allocation;
                }
                case "Facilities":
                    return funding * FacilityManager.Instance.FundingAllocation / 100.0f;
                case "Research":
                    return funding * ResearchManager.Instance.FundingAllocation / 100.0f;
            }
            return 0;
        }

        public string TrimFacilityString(string s)
        {
            s.Replace("SpaceCenter/", String.Empty);
            return s;
        }
    }
}