using System;
using System.Collections.Generic;
using System.Linq;
using FlightTracker;
using UnityEngine;
using Random = System.Random;

namespace Bureaucracy
{
    public class Utilities
    {
        public static Utilities Instance;
        public readonly Random Randomise = new Random();  

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
            return Math.Round(Reputation.Instance.reputation * SettingsClass.Instance.BudgetMultiplier, 0);
        }

        public double GetNetBudget(string department)
        {
            
            double funding = GetGrossBudget();
            double costs = Costs.Instance.GetTotalMaintenanceCosts();
            funding -= costs;
            funding -= CrewManager.Instance.Bonuses(funding, false);
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
                    return Math.Round(funding*allocation, 0);
                }
                case "Construction":
                    return Math.Max(funding * FacilityManager.Instance.FundingAllocation / 100.0f, 0.0f);
                case "Research":
                    return Math.Max(funding * ResearchManager.Instance.FundingAllocation / 100.0f, 0.0f);
                default:
                    return -1.0f;
            }
        }

        public KeyValuePair<int, string> ConvertUtToRealTime(double ut)
        {
            int timeStamp = 0;
            CelestialBody homeworld = FlightGlobals.GetHomeBody();
            while (ut > homeworld.orbit.period)
            {
                timeStamp++;
                ut -= homeworld.orbit.period;
            }
            if(timeStamp >0) return new KeyValuePair<int, string>(timeStamp, "years");
            while (ut > homeworld.solarDayLength)
            {
                timeStamp++;
                ut -= homeworld.solarDayLength;
            }
            return new KeyValuePair<int, string>(timeStamp, "days");
        }

        public void PayWageDebt(double debt)
        {
            debt = Math.Abs(debt);
            debt -= Funding.Instance.Funds;
            if (debt <= 0) return;
            List<CrewMember> unpaidKerbals = new List<CrewMember>();
            for(int i = 0; i<CrewManager.Instance.Kerbals.Count; i++)
            {
                CrewMember c = CrewManager.Instance.Kerbals.ElementAt(i).Value;
                unpaidKerbals.Add(c);
                debt -= c.Wage;
                if (debt <= 0) break;
            }
            CrewManager.Instance.ProcessUnpaidKerbals(unpaidKerbals);
        }

        public void PayFacilityDebt(double debt, double wageDebt)
        {
            double fundsAvailable = Funding.Instance.Funds - wageDebt;
            debt -= fundsAvailable;
            for (int i = 0; i < FacilityManager.Instance.Facilities.Count; i++)
            {
                BureaucracyFacility bf = FacilityManager.Instance.Facilities.ElementAt(i);
                bf.CloseFacility();
                debt += bf.MaintenanceCost;
                if (debt <= 0) break;
            }
        }

        public void SabotageLaunch(string facilityName)
        {
            for (int i = 0; i < FlightGlobals.ActiveVessel.Parts.Count; i++)
            {
                Part p = FlightGlobals.ActiveVessel.Parts.ElementAt(i);
                List<PartResource> resources = p.Resources.ToList();
                for (int resourceCount = 0; resourceCount < resources.Count; resourceCount++)
                {
                    PartResource r = resources.ElementAt(resourceCount);
                    r.amount = 0;
                }
            }
            UiController.Instance.GenerateErrorWindow("Due to reduced staffing levels at the "+facilityName+" it seems they forgot to actually fuel the vessel.");
        }

        public string ConvertUtToKspTimeStamp(double universalTimeStamp)
        {
            int years = 1;
            int days = 1;
            while (universalTimeStamp > FlightGlobals.GetHomeBody().orbit.period)
            {
                years++;
                universalTimeStamp -= FlightGlobals.GetHomeBody().orbit.period;
            }

            while (universalTimeStamp > FlightGlobals.GetHomeBody().solarDayLength)
            {
                days++;
                universalTimeStamp -= FlightGlobals.GetHomeBody().solarDayLength;
            }

            return "Y" + years + " D" + days;
        }

        public string GetARandomKerbal()
        {
            List<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
            int tries = 0;
            if (crew.Count == 0) return String.Empty;
            while (tries < 100)
            {
                ProtoCrewMember p = crew.ElementAt(Randomise.Next(0, crew.Count));
                if (p.rosterStatus != ProtoCrewMember.RosterStatus.Available)
                {
                    tries++;
                    continue;
                }
                return p.name;
            }

            return String.Empty;
        }
        public Manager GetManagerByName(string managerName)
        {
            for (int i = 0; i < Bureaucracy.Instance.registeredManagers.Count; i++)
            {
                Manager m = Bureaucracy.Instance.registeredManagers.ElementAt(i);
                if (m.Name != managerName) continue;
                return m;
            }

            return null;
        }
    }
}