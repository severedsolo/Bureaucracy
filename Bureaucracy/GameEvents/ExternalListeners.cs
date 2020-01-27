using System.Collections.Generic;
using System.Linq;
using Contracts;
using KSP.UI.Screens;
using UnityEngine;
using FlightTracker;
using JetBrains.Annotations;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class ExternalListeners : MonoBehaviour
    {
        private KerbalismApi kerbalism;
        private EventData<List<ScienceSubject>, List<double>> onKerbalismScience;
        [UsedImplicitly] private Utilities utilitiesReference = new Utilities();
        private void Awake()
        {
            Debug.Log("[Bureaucracy]: Waking GameEvents");
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
            {
                Destroy(this);
                Debug.Log("[Bureaucracy]: Game Mode is not career. Destroying Event Handler");
            }
            else
            {
                DontDestroyOnLoad(this);
            }
        }

        private void Start()
        {
            Debug.Log("[Bureaucracy]: Registering Events");
            GameEvents.OnVesselRollout.Add(AddLaunch);
            GameEvents.Contract.onOffered.Add(OnContractOffered);
            GameEvents.onFacilityContextMenuSpawn.Add(OnFacilityContextMenuSpawn);
            GameEvents.OnScienceRecieved.Add(OnScienceReceived);
            GameEvents.OnCrewmemberHired.Add(OnCrewMemberHired);
            GameEvents.onKerbalStatusChanged.Add(PotentialKerbalDeath);
            GameEvents.onGUIApplicationLauncherReady.Add(AddToolbarButton);
            GameEvents.onGUIApplicationLauncherUnreadifying.Add(RemoveToolbarButton);
            GameEvents.onGUIAstronautComplexSpawn.Add(AstronautComplexSpawned);
            GameEvents.onGUIAstronautComplexDespawn.Add(AstronautComplexDespawned);
            Debug.Log("[Bureaucracy]: Stock Events Registered");
            FlightTrackerApi.OnFlightTrackerUpdated.Add(AllocateCrewBonuses);
            Debug.Log("[Bureaucracy]: FlightTracker Events Registered");
            kerbalism = new KerbalismApi();
            if (!kerbalism.ActivateKerbalismInterface() || !SettingsClass.Instance.HandleScience) return;
            onKerbalismScience = GameEvents.FindEvent<EventData<List<ScienceSubject>, List<double>>>("onSubjectsReceived");
            if (onKerbalismScience == null) return;
            onKerbalismScience.Add(OnKerbalismScienceReceived);
            Debug.Log("[Bureaucracy]: Kerbalism Event Registered");
        }
        
        private void AstronautComplexDespawned()
        {
            AstronautComplexOverride.Instance.astronautComplexSpawned = false;
        }

        private void AstronautComplexSpawned()
        {
            AstronautComplexOverride.Instance.updateCount = 4;
            AstronautComplexOverride.Instance.astronautComplexSpawned = true;
        }

        private void AddToolbarButton()
        {
            UiController.Instance.SetupToolbarButton();
        }
        private void RemoveToolbarButton(GameScenes data)
        {
            UiController.Instance.RemoveToolbarButton();
        }

        private void PotentialKerbalDeath(ProtoCrewMember crewMember, ProtoCrewMember.RosterStatus statusFrom, ProtoCrewMember.RosterStatus statusTo)
        {
            //No need to check if the Kerbal hasn't died. Kerbals always go missing before they go dead, so just check for Missing.
            if (statusTo != ProtoCrewMember.RosterStatus.Missing) return;
            CrewManager.Instance.ProcessDeadKerbal(crewMember);
        }

        private void OnCrewMemberHired(ProtoCrewMember crewMember, int numberOfActiveKerbals)
        {
            if (FacilityManager.Instance.GetFacilityByName("AstronautComplex").IsClosed)
            {
                Funding.Instance.AddFunds(GameVariables.Instance.GetRecruitHireCost(numberOfActiveKerbals - 1), TransactionReasons.CrewRecruited);
                UiController.Instance.errorWindow = UiController.Instance.NoHireWindow();
                HighLogic.CurrentGame.CrewRoster.Remove(crewMember);
                return;
            }
            CrewManager.Instance.AddNewCrewMember(crewMember);
        }

        private void AllocateCrewBonuses(ProtoCrewMember crewMember)
        {
            CrewManager.Instance.UpdateCrewBonus(crewMember, FlightTrackerApi.Instance.GetLaunchTime(crewMember.name));
        }

        private void OnScienceReceived(float science, ScienceSubject subject, ProtoVessel protoVessel, bool reverseEngineered)
        {
            if (!SettingsClass.Instance.HandleScience) return;
            if (science < 0.1f) return;
            ResearchAndDevelopment.Instance.AddScience(-science, TransactionReasons.ScienceTransmission);
            ResearchManager.Instance.NewScienceReceived(science, subject);
        }

        private void OnKerbalismScienceReceived(List<ScienceSubject> subjects, List<double> data)
        {
            if (!SettingsClass.Instance.HandleScience) return;
            for (int i = 0; i < subjects.Count; i++)
            {
                ResearchManager.Instance.NewScienceReceived((float)data.ElementAt(i), subjects.ElementAt(i));
            }
        }

        private void OnFacilityContextMenuSpawn(KSCFacilityContextMenu menu)
        {
            FacilityMenuOverride.Instance.FacilityMenuSpawned(menu);
        }


        private void OnContractOffered(Contract contract)
        {
            ContractInterceptor.Instance.OnContractOffered(contract);
        }

        private void AddLaunch(ShipConstruct ship)
        {
            Costs.Instance.AddLaunch(ship);
            string editor = ship.shipFacility == EditorFacility.VAB ? "VehicleAssemblyBuilding" : "SpaceplaneHangar";
            BureaucracyFacility bf = FacilityManager.Instance.GetFacilityByName(editor);
            if (!bf.IsClosed) return;
            bf.LaunchesThisMonth++;
            if (bf.LaunchesThisMonth > 2) Utilities.Instance.SabotageLaunch();

        }

        private void OnDisable()
        {
            Debug.Log("[Bureaucracy]: Unregistering Events");
            GameEvents.OnVesselRollout.Remove(AddLaunch);
            GameEvents.Contract.onOffered.Remove(OnContractOffered);
            GameEvents.onFacilityContextMenuSpawn.Remove(OnFacilityContextMenuSpawn);
            GameEvents.OnScienceRecieved.Remove(OnScienceReceived);
            GameEvents.OnCrewmemberHired.Remove(OnCrewMemberHired);
            GameEvents.onKerbalStatusChanged.Remove(PotentialKerbalDeath);
            GameEvents.onGUIApplicationLauncherReady.Remove(AddToolbarButton);
            GameEvents.onGUIApplicationLauncherUnreadifying.Remove(RemoveToolbarButton);
            GameEvents.onGUIAstronautComplexSpawn.Remove(AstronautComplexSpawned);
            GameEvents.onGUIAstronautComplexDespawn.Remove(AstronautComplexDespawned);
            Debug.Log("[Bureaucracy] Unregistered Stock Events");
            FlightTrackerApi.OnFlightTrackerUpdated.Remove(AllocateCrewBonuses);
            Debug.Log("[Bureaucracy] Unregistered Flight Tracker Event");
            if (onKerbalismScience == null) return;
            onKerbalismScience.Remove(OnKerbalismScienceReceived);
            Debug.Log("[Bureaucracy]: Unregistered Kerbalism Event");
        }
    }
}