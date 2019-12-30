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
        }

        private void ProcessCrew()
        {
            for (int i = 0; i < Kerbals.Count; i++)
            {
                CrewMember c = Kerbals.ElementAt(i).Value;
                c.MonthWithoutIncident();
            }
        }


        public void OnSave(ConfigNode cn)
        {
            ConfigNode crewManagerNode = new ConfigNode("CREW_MANAGER");
            for (int i = 0; i < Kerbals.Count; i++)
            {
                KeyValuePair<string, CrewMember> crewKeys = Kerbals.ElementAt(i);
                crewKeys.Value.OnSave(crewManagerNode);
            }

            cn.AddNode(crewManagerNode);
        }

        public void OnLoad(ConfigNode cn)
        {
            ConfigNode crewManagerNode = cn.GetNode("CREW_MANAGER");
            if (crewManagerNode == null) return;
            ConfigNode[] crewNodes = crewManagerNode.GetNodes("CREW_MEMBER");
            for (int i = 0; i < crewNodes.Length; i++)
            {
                ConfigNode crewConfig = crewNodes.ElementAt(i);
                Kerbals[crewConfig.GetValue("Name")].OnLoad(crewConfig);
            }
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
            UnhappyCrewOutcomes.Clear();
            for (int i = 0; i < Kerbals.Count; i++)
            {
                CrewMember c = Kerbals.ElementAt(i).Value;
                if (!c.Unhappy) continue;
                UnhappyCrewOutcomes.Add(c, c.UnhappyOutcome());
            }
        }

        public void ProcessUnpaidKerbals(List<CrewMember> unpaidKerbals)
        {
            for ( int i = 0; i < unpaidKerbals.Count; i++)
            {
                CrewMember c = Kerbals.ElementAt(i).Value;
                c.AddUnhappiness("not being paid");
            }
        }

        public void ProcessQuitters()
        {
            for (int i = 0; i < UnhappyCrewOutcomes.Count; i++)
            {
                KeyValuePair<CrewMember, string> kvp = UnhappyCrewOutcomes.ElementAt(i);
                if(!kvp.Value.Contains("Quit")) continue;
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
            //TODO: Make the Astronaut Complex UI reflect that the astronaut is training;
            //TODO: Make the Astronaut Complex UI reflect that the astronaut is training;
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
            }
            float penalty = Reputation.Instance.reputation * (SettingsClass.Instance.DeadKerbalPenalty / 100.0f);
            Reputation.Instance.AddReputation(-penalty, TransactionReasons.VesselLoss);
        }

        private bool LossAlreadyProcessed(ProtoCrewMember crewMember)
        {
            if (!CrewOnValidVessel(crewMember)) return false;
            if (crewMember.seat.vessel.id == lastProcessedVessel) return true;
            lastProcessedVessel = crewMember.seat.vessel.id;
            return true;
        }

        private bool CrewOnValidVessel(ProtoCrewMember crewMember)
        {
            return crewMember.seat != null && crewMember.seat.vessel != null;
        }
    }
}