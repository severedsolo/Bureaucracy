using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bureaucracy
{
    public class CrewManager : Manager
    {
        public static CrewManager Instance;
        public readonly Dictionary<string, CrewMember> Kerbals = new Dictionary<string, CrewMember>();
        private int lastBonus;
        public readonly Dictionary<CrewMember, string> UnhappyCrewOutcomes = new Dictionary<CrewMember, string>();
        private Guid lastProcessedVessel = Guid.Empty;
        public readonly List<string> Retirees = new List<string>();

        public int LastBonus
        {
            get
            {
                int bonusToReturn = lastBonus;
                lastBonus = 0;
                return bonusToReturn;
            }
        }

        protected override Report GetReport()
        {
            return new CrewReport();
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        public CrewManager(List<ProtoCrewMember> crewMembers)
        {
            FundingAllocation = 0;
            for (int i = 0; i < crewMembers.Count; i++)
            {
                ProtoCrewMember p = crewMembers.ElementAt(i);
                if (p.rosterStatus == ProtoCrewMember.RosterStatus.Dead) continue;
                Kerbals.Add(p.name, new CrewMember(p.name));
            }
            InternalListeners.OnBudgetAboutToFire.Add(ProcessCrew);
            Instance = this;
            Debug.Log("[Bureaucracy]: Crew Manager Ready");
        }

        private void ProcessCrew()
        {
            Debug.Log("[Bureaucracy]: Processing Crew");
            for (int i = 0; i < Kerbals.Count; i++)
            {
                CrewMember c = Kerbals.ElementAt(i).Value;
                c.MonthWithoutIncident();
            }
            if(SettingsClass.Instance.RetirementEnabled)ProcessRetirees();
            Debug.Log("[Bureaucracy]: Crew Processed");
        }


        public void OnSave(ConfigNode cn)
        {
            Debug.Log("[Bureaucracy]: CrewManager OnSave");
            ConfigNode crewManagerNode = new ConfigNode("CREW_MANAGER");
            for (int i = 0; i < Kerbals.Count; i++)
            {
                KeyValuePair<string, CrewMember> crewKeys = Kerbals.ElementAt(i);
                crewKeys.Value.OnSave(crewManagerNode);
            }
            cn.AddNode(crewManagerNode);
            Debug.Log("[Bureaucracy]: Crew Manager OnSaveComplete");
        }

        public void OnLoad(ConfigNode cn)
        {
            Debug.Log("[Bureaucracy]: Crew Manager OnLoad");
            ConfigNode crewManagerNode = cn.GetNode("CREW_MANAGER");
            if (crewManagerNode == null) return;
            ConfigNode[] crewNodes = crewManagerNode.GetNodes("CREW_MEMBER");
            for (int i = 0; i < crewNodes.Length; i++)
            {
                ConfigNode crewConfig = crewNodes.ElementAt(i);
                if(Kerbals.TryGetValue(crewConfig.GetValue("Name"), out CrewMember c))c.OnLoad(crewConfig);
                else Debug.Log("[Bureaucracy]: Loaded config for "+crewConfig.GetValue("Name")+" but actual CrewMember could not be found! Skipping");
            }
            Debug.Log("[Bureaucracy]: Crew Manager OnLoad Complete");
        }

        private void ProcessRetirees()
        {
            Debug.Log("[Bureaucracy]: Processing Retirees");
            for (int i = 0; i < Kerbals.Count; i++)
            {
                KeyValuePair<string, CrewMember> kvp = Kerbals.ElementAt(i);
                if (kvp.Value.retirementDate > Planetarium.GetUniversalTime()) continue;
                Debug.Log("[Bureaucracy]: " + kvp.Value.Name + " has retired");
                HighLogic.CurrentGame.CrewRoster.Remove(kvp.Value.CrewReference());
                ScreenMessages.PostScreenMessage("[Bureaucracy]: " + kvp.Key + " has retired");
                Retirees.Add(kvp.Key);
                Kerbals.Remove(kvp.Value.Name);
            }
            Debug.Log("[Bureaucracy]: Retirees Processed");
        }

        public void UpdateCrewBonus(ProtoCrewMember crewMember, double launchTime)
        {
            Kerbals[crewMember.name].AllocateBonus(Planetarium.GetUniversalTime()-launchTime);   
        }

        public int Bonuses(double availableFunding, bool clearBonuses)
        {
            int bonus = 0;
            for (int i = 0; i < Kerbals.Count; i++)
            {
                CrewMember c = Kerbals.ElementAt(i).Value;
                int bonusToProcess = c.GetBonus(clearBonuses);
                if (clearBonuses && bonusToProcess > 0 && availableFunding < bonusToProcess)
                {
                    c.AddUnhappiness("not being paid");
                }
                else bonus += bonusToProcess;
                availableFunding -= bonus;
            }
            lastBonus = bonus;
            return bonus;
        }

        public void ProcessUnhappyCrew()
        {
            Debug.Log("[Bureaucracy]: Processing Unhappy Crew");
            UnhappyCrewOutcomes.Clear();
            for (int i = 0; i < Kerbals.Count; i++)
            {
                CrewMember c = Kerbals.ElementAt(i).Value;
                if (!c.Unhappy) continue;
                string outcome = c.UnhappyOutcome();
                Debug.Log("[Bureaucracy]: Unhappy Crewmember "+c.Name+" "+outcome);
                UnhappyCrewOutcomes.Add(c, outcome);
            }
        }

        public void ProcessUnpaidKerbals(List<CrewMember> unpaidKerbals)
        {
            for ( int i = 0; i < unpaidKerbals.Count; i++)
            {
                CrewMember c = Kerbals.ElementAt(i).Value;
                c.AddUnhappiness("not being paid");
                Debug.Log("[Bureaucracy]: Adding new unpaid crew member "+c.Name);
            }
        }

        public void ProcessQuitters()
        {
            for (int i = 0; i < UnhappyCrewOutcomes.Count; i++)
            {
                KeyValuePair<CrewMember, string> kvp = UnhappyCrewOutcomes.ElementAt(i);
                if(!kvp.Value.Contains("Quit")) continue;
                Debug.Log("[Bureaucracy]: "+kvp.Key.Name+" has quit due to unhappiness");
                HighLogic.CurrentGame.CrewRoster.Remove(kvp.Key.CrewReference());
                Kerbals.Remove(kvp.Key.Name);
            }
        }

        public void AddNewCrewMember(ProtoCrewMember crewMember)
        {
            CrewMember newCrew = new CrewMember(crewMember.name);
            Kerbals.Add(crewMember.name, newCrew );
            if (!SettingsClass.Instance.AstronautTraining) return;
            double trainingPeriod = FlightGlobals.GetHomeBody().solarDayLength * SettingsClass.Instance.TimeBetweenBudgets;
            crewMember.SetInactive(trainingPeriod);
            Debug.Log("[Bureaucracy]: New Crewmember added: " + newCrew.Name + ". Training for " + trainingPeriod);
        }

        public void ProcessDeadKerbal(ProtoCrewMember crewMember)
        {
            Kerbals.Remove(crewMember.name);
            if (LossAlreadyProcessed(crewMember)) return;
            for (int i = 0; i < Kerbals.Count; i++)
            {
                CrewMember c = Kerbals.ElementAt(i).Value;
                if(Utilities.Instance.Randomise.NextDouble() < c.CrewReference().courage) continue;
                string lostVessel = crewMember.name;
                if (CrewOnValidVessel(crewMember)) lostVessel = crewMember.seat.vessel.vesselName;
                c.AddUnhappiness("Loss of "+lostVessel);
                Debug.Log("[Bureaucracy]: Unhappiness event registered for "+crewMember.name+": Dead Kerbal");
            }
            float penalty = Reputation.Instance.reputation * (SettingsClass.Instance.DeadKerbalPenalty / 100.0f);
            Reputation.Instance.AddReputation(-penalty, TransactionReasons.VesselLoss);
            Debug.Log("[Bureaucracy]: Dead Kerbal Penalty Applied");
        }

        private bool LossAlreadyProcessed(ProtoCrewMember crewMember)
        {
            if (!CrewOnValidVessel(crewMember)) return false;
            if (crewMember.seat.vessel.id == lastProcessedVessel) return true;
            lastProcessedVessel = crewMember.seat.vessel.id;
            return false;
        }

        private bool CrewOnValidVessel(ProtoCrewMember crewMember)
        {
            return crewMember.seat != null && crewMember.seat.vessel != null;
        }

        public void ExtendRetirement(ProtoCrewMember crewMember, double launchTime)
        {
            double extension = (Planetarium.GetUniversalTime() - launchTime)*SettingsClass.Instance.RetirementExtensionFactor;
            Kerbals[crewMember.name].ExtendRetirementAge(extension);
        }
    }
}