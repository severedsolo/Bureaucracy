using Contracts;
using KSP.UI.Screens;
using UnityEngine;
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
            GameEvents.onKerbalStatusChanged.Add(PotentialKerbalDeath);
            GameEvents.onGUIApplicationLauncherReady.Add(AddToolbarButton);
            GameEvents.onGUIApplicationLauncherUnreadifying.Add(RemoveToolbarButton);
            GameEvents.onGUIAstronautComplexSpawn.Add(AstronautComplexSpawned);
            GameEvents.onGUIAstronautComplexDespawn.Add(AstronautComplexDespawned);
        }

        private void AstronautComplexDespawned()
        {
            AstronautComplexOverride.Instance.AstronautComplexSpawned = false;
        }

        private void AstronautComplexSpawned()
        {
            AstronautComplexOverride.Instance.updateCount = 4;
            AstronautComplexOverride.Instance.AstronautComplexSpawned = true;
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
            CrewManager.Instance.UpdateCrewBonus(crewMember, ActiveFlightTracker.instance.GetLaunchTime(crewMember.name));
        }

        private void OnScienceReceived(float science, ScienceSubject subject, ProtoVessel protoVessel, bool reverseEngineered)
        {
            ResearchManager.Instance.NewScienceReceived(science, subject);
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
            if (Utilities.Instance.Randomise.NextDouble() > Bureaucracy.Instance.qaModifier)
            {
                ExplosionEvent e = new ExplosionEvent();
            }
            Costs.Instance.AddLaunch(ship);
            string editor = ship.shipFacility == EditorFacility.VAB ? "VehicleAssemblyBuilding" : "SpaceplaneHangar";
            BureaucracyFacility bf = FacilityManager.Instance.GetFacilityByName(editor);
            if (!bf.IsClosed) return;
            bf.LaunchesThisMonth++;
            if (bf.LaunchesThisMonth > 2) Utilities.Instance.SabotageLaunch(editor);

        }

        private void OnDisable()
        {
            GameEvents.OnVesselRollout.Remove(AddLaunch);
            GameEvents.Contract.onOffered.Remove(OnContractOffered);
            GameEvents.onFacilityContextMenuSpawn.Remove(OnFacilityContextMenuSpawn);
            GameEvents.OnScienceRecieved.Remove(OnScienceReceived);
            GameEvents.OnCrewmemberHired.Remove(OnCrewMemberHired);
            ActiveFlightTracker.onFlightTrackerUpdated.Remove(AllocateCrewBonuses);
            GameEvents.onKerbalStatusChanged.Remove(PotentialKerbalDeath);
            GameEvents.onGUIApplicationLauncherReady.Remove(AddToolbarButton);
            GameEvents.onGUIApplicationLauncherUnreadifying.Remove(RemoveToolbarButton);
            GameEvents.onGUIAstronautComplexSpawn.Remove(AstronautComplexSpawned);
            GameEvents.onGUIAstronautComplexDespawn.Remove(AstronautComplexDespawned);
        }
    }
}