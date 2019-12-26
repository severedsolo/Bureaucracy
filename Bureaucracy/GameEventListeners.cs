using System;
using Contracts;
using KSP.UI.Screens;
using PreFlightTests;
using UnityEngine;
using Upgradeables;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class GameEventListeners : MonoBehaviour
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
                GameEvents.OnVesselRollout.Add(AddLaunch);
                GameEvents.Contract.onOffered.Add(OnContractOffered);
                GameEvents.onFacilityContextMenuSpawn.Add(OnFacilityContextMenuSpawn);
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
        }
        
        //TODO: Add destructors for all events
    }
}