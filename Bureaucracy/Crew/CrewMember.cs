using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Enumerable = UniLinq.Enumerable;

namespace Bureaucracy
{
    public class CrewMember
    {
        private double bonusAwaitingPayment = 0;
        private string name;
        private ProtoCrewMember crewRef;
        private int maxStrikes = 0;
        public int LastMissionPayout;
        List<CrewUnhappiness> unhappinessEvents = new List<CrewUnhappiness>();
        public bool Unhappy = false;
        
        public string Name => name;

        public double Wage
        {
            get
            {
                float experienceLevel = crewRef.experienceLevel;
                if (experienceLevel == 0) experienceLevel = 0.5f;
                return experienceLevel * SettingsClass.Instance.KerbalBaseWage;
            }
        }

        public CrewMember(string kerbalName)
        {
            name = kerbalName;
            maxStrikes = (int)(SettingsClass.Instance.BaseStrikesToQuit * CrewReference().stupidity);
        }
        
        public void AllocateBonus(double timeOnMission)
        {
            KeyValuePair<int, string> kvp = Utilities.Instance.ConvertUtToRealTime(timeOnMission);
            double payout;
            if (kvp.Value == "years") payout = kvp.Key * SettingsClass.Instance.LongTermBonusYears;
            else payout = kvp.Key * SettingsClass.Instance.LongTermBonusDays;
            LastMissionPayout = (int)payout;
            Debug.Log("[Bureaucracy]: Assigned Bonus of "+(int)payout+" to "+name);
        }

        public ProtoCrewMember CrewReference()
        {
            if (crewRef != null)
            {
                Debug.Log("[Bureaucracy]: Returning cached crew ref " + crewRef.name);
                return crewRef;
            }

            List<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
            for (int i = 0; i < crew.Count; i++)
            {
                ProtoCrewMember p = crew.ElementAt(i);
                if (p.name != name) continue;
                crewRef = p;
                break;
            }

            //Newly hired crew members aren't actually in the crew yet, so we need to check the Applicants too.
            if (crewRef == null)
            {
                Debug.Log("[Bureaucracy]: Couldn't find " + name + " in the crew list. Checking Applicants");
                crew = HighLogic.CurrentGame.CrewRoster.Applicants.ToList();
                for (int i = 0; i < crew.Count; i++)
                {
                    ProtoCrewMember p = crew.ElementAt(i);
                    if (p.name != name) continue;
                    crewRef = p;
                    break;
                }
            }

            if (crewRef == null) Debug.Log("[Bureaucracy]: Couldn't find a crew ref for " + name);
            Debug.Log("[Bureaucracy]: Returning uncached crewRef " + name);
            return crewRef;
        }

        public int GetBonus()
        {
            double payment = bonusAwaitingPayment;
            bonusAwaitingPayment = 0;
            return (int)payment;
        }

        public void OnSave(ConfigNode crewManagerNode)
        {
            ConfigNode cn = new ConfigNode("CREW_MEMBER");
            cn.SetValue("Name", name, true);
            cn.SetValue("Bonus", bonusAwaitingPayment, true);
            //TODO: Save unhappiness events
            cn.SetValue("LastPayout", LastMissionPayout, true);
            crewManagerNode.AddNode(cn);
        }

        public void OnLoad(ConfigNode crewConfig)
        {
            name = crewConfig.GetValue("Name");
            double.TryParse(crewConfig.GetValue("Bonus"), out bonusAwaitingPayment);
            int.TryParse(crewConfig.GetValue("LastPayout"), out LastMissionPayout);
            //TODO: Load unhappiness events;
        }

        public string UnhappyOutcome()
        {
            if (CrewReference().rosterStatus == ProtoCrewMember.RosterStatus.Assigned) return " is not happy but will continue for the sake of the mission";
            if(unhappinessEvents.Count >= maxStrikes) return " Quit the space program due to "+unhappinessEvents.Last().Reason;
            return "is not happy due to " + unhappinessEvents.Last().Reason;
        }

        public void MonthWithoutIncident()
        {
            for (int i = unhappinessEvents.Count-1; i >= 0; i--)
            {
                CrewUnhappiness cu = unhappinessEvents.ElementAt(i);
                if (cu.ClearStrike()) unhappinessEvents.Remove(cu);
            }
        }

        public void AddUnhappiness(string reason)
        {
            unhappinessEvents.Add(new CrewUnhappiness(reason, this));
            Unhappy = true;
        }
    }
}