using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bureaucracy
{
    public class CrewMember
    {
        private double bonusAwaitingPayment;
        private ProtoCrewMember crewRef;
        private readonly int maxStrikes;
        private int lastMissionPayout;
        private readonly List<CrewUnhappiness> unhappinessEvents = new List<CrewUnhappiness>();
        public bool Unhappy;
        
        public string Name { get; private set; }

        public double Wage
        {
            get
            {
                float experienceLevel = crewRef.experienceLevel;
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (experienceLevel == 0) experienceLevel = 0.5f;
                return experienceLevel * SettingsClass.Instance.KerbalBaseWage;
            }
        }

        public CrewMember(string kerbalName)
        {
            Name = kerbalName;
            maxStrikes = (int)(SettingsClass.Instance.BaseStrikesToQuit * CrewReference().stupidity);
        }
        
        public void AllocateBonus(double timeOnMission)
        {
            KeyValuePair<int, string> kvp = Utilities.Instance.ConvertUtToRealTime(timeOnMission);
            double payout;
            if (kvp.Value == "years") payout = kvp.Key * SettingsClass.Instance.LongTermBonusYears;
            else payout = kvp.Key * SettingsClass.Instance.LongTermBonusDays;
            lastMissionPayout = (int)payout;
            Debug.Log("[Bureaucracy]: Assigned Bonus of "+(int)payout+" to "+Name);
        }

        public ProtoCrewMember CrewReference()
        {
            if (crewRef != null) { return crewRef; }
            List<ProtoCrewMember> crew = HighLogic.CurrentGame.CrewRoster.Crew.ToList();
            for (int i = 0; i < crew.Count; i++)
            {
                ProtoCrewMember p = crew.ElementAt(i);
                if (p.name != Name) continue;
                crewRef = p;
                break;
            }

            //Newly hired crew members aren't actually in the crew yet, so we need to check the Applicants too.
            if (crewRef == null)
            {
                Debug.Log("[Bureaucracy]: Couldn't find " + Name + " in the crew list. Checking Applicants");
                crew = HighLogic.CurrentGame.CrewRoster.Applicants.ToList();
                for (int i = 0; i < crew.Count; i++)
                {
                    ProtoCrewMember p = crew.ElementAt(i);
                    if (p.name != Name) continue;
                    crewRef = p;
                    break;
                }
            }

            if (crewRef == null) Debug.Log("[Bureaucracy]: Couldn't find a crew ref for " + Name);
            else Debug.Log("[Bureaucracy]: Returning and caching crewRef " + Name);
            return crewRef;
        }

        public int GetBonus(bool clearBonus)
        {
            double payment = bonusAwaitingPayment;
            if(clearBonus) bonusAwaitingPayment = 0;
            return (int)payment;
        }

        public void OnSave(ConfigNode crewManagerNode)
        {
            ConfigNode cn = new ConfigNode("CREW_MEMBER");
            cn.SetValue("Name", Name, true);
            cn.SetValue("Bonus", bonusAwaitingPayment, true);
            //TODO: Save unhappiness events
            cn.SetValue("LastPayout", lastMissionPayout, true);
            crewManagerNode.AddNode(cn);
        }

        public void OnLoad(ConfigNode crewConfig)
        {
            Name = crewConfig.GetValue("Name");
            double.TryParse(crewConfig.GetValue("Bonus"), out bonusAwaitingPayment);
            int.TryParse(crewConfig.GetValue("LastPayout"), out lastMissionPayout);
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