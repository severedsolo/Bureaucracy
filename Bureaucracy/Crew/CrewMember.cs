using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bureaucracy
{
    public class CrewMember
    {
        private double bonusAwaitingPayment;
        private ProtoCrewMember crewRef;
        public readonly int maxStrikes;
        public readonly List<CrewUnhappiness> unhappinessEvents = new List<CrewUnhappiness>();
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
            Debug.Log("[Bureaucracy]: New CrewMember setup: "+kerbalName);
        }
        
        public void AllocateBonus(double timeOnMission)
        {
            KeyValuePair<int, string> kvp = Utilities.Instance.ConvertUtToRealTime(timeOnMission);
            double payout;
            if (kvp.Value == "years") payout = kvp.Key * SettingsClass.Instance.LongTermBonusYears;
            else payout = kvp.Key * SettingsClass.Instance.LongTermBonusDays;
            bonusAwaitingPayment += payout;
            Debug.Log("[Bureaucracy]: Assigned Bonus of "+(int)payout+" to "+Name);
        }

        public ProtoCrewMember CrewReference()
        {
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
            ConfigNode crewNode = new ConfigNode("CREW_MEMBER");
            crewNode.SetValue("Name", Name, true);
            crewNode.SetValue("Bonus", bonusAwaitingPayment, true);
            for (int i = 0; i < unhappinessEvents.Count; i++)
            {
                CrewUnhappiness cu = unhappinessEvents.ElementAt(i);
                cu.OnSave(crewNode);
            }
            crewManagerNode.AddNode(crewNode);
        }

        public void OnLoad(ConfigNode crewConfig)
        {
            Name = crewConfig.GetValue("Name");
            double.TryParse(crewConfig.GetValue("Bonus"), out bonusAwaitingPayment);
            ConfigNode[] unhappyNodes = crewConfig.GetNodes("UNHAPPINESS");
            for (int i = 0; i < unhappyNodes.Length; i++)
            {
                CrewUnhappiness cu = new CrewUnhappiness("loading", this);
                cu.OnLoad(unhappyNodes.ElementAt(i));
                unhappinessEvents.Add(cu);
            }
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