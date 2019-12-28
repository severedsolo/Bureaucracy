using System;
using Contracts;
using Contracts.Parameters;
using KSP.UI.Screens;
using PreFlightTests;
using UnityEngine;
using Upgradeables;
using FlightTracker;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class ExternalListeners : MonoBehaviour
    {

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
            GameEvents.OnVesselRollout.Add(AddLaunch);
            GameEvents.Contract.onOffered.Add(OnContractOffered);
            GameEvents.onFacilityContextMenuSpawn.Add(OnFacilityContextMenuSpawn);
            GameEvents.OnScienceRecieved.Add(OnScienceReceived);
            GameEvents.OnCrewmemberHired.Add(OnCrewMemberHired);
            ActiveFlightTracker.onFlightTrackerUpdated.Add(AllocateCrewBonuses);
        }

        private void OnCrewMemberHired(ProtoCrewMember crewMember, int numberOfActiveKerbals)
        {
            if (FacilityManager.Instance.GetFacilityByName("AstronautComplex").IsClosed)
            {
                Funding.Instance.AddFunds(GameVariables.Instance.GetRecruitHireCost(numberOfActiveKerbals - 1), TransactionReasons.CrewRecruited);
                HighLogic.CurrentGame.CrewRoster.Remove(crewMember);
                //TODO: Popup error message that the astronaut complex is closed
                return;
            }
            CrewManager.Instance.AddNewCrewMember(crewMember);
        }

        private void AllocateCrewBonuses(ProtoCrewMember crewMember)
        {
            CrewManager.Instance.UpdateCrewBonus(crewMember, ActiveFlightTracker.instance.GetLaunchTime(crewMember.name));
        }

        private void OnScienceReceived(float science, ScienceSubject subject, ProtoVessel protoVessel, bool reverseEngineered)
        {
            ResearchManager.Instance.NewScienceReceived(science, subject, protoVessel, reverseEngineered);
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
            string editor = "None";
            if (ship.shipFacility == EditorFacility.VAB) editor = "VehicleAssemblyBuilding";
            else editor = "SpaceplaneHangar";
            BureaucracyFacility bf = FacilityManager.Instance.GetFacilityByName(editor);
            if (bf.IsClosed)
            {
                bf.LaunchesThisMonth++;
                if (bf.LaunchesThisMonth > 2) Utilities.Instance.SabotageLaunch();
            }
        }

        private void OnDisable()
        {
            GameEvents.OnVesselRollout.Remove(AddLaunch);
            GameEvents.Contract.onOffered.Remove(OnContractOffered);
            GameEvents.onFacilityContextMenuSpawn.Remove(OnFacilityContextMenuSpawn);
            GameEvents.OnScienceRecieved.Remove(OnScienceReceived);
            ActiveFlightTracker.onFlightTrackerUpdated.Remove(AllocateCrewBonuses);
        }
    }
}