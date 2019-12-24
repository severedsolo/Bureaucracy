using System;
using UnityEngine;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class GameEventListeners : MonoBehaviour
    {

        public static GameEventListeners Instance;

        private void Awake()
        {
            Debug.Log("[Bureaucracy]: Waking GameEvents");
            DontDestroyOnLoad(this);
            GameEvents.OnVesselRollout.Add(AddLaunch);
            Instance = this;
        }

        private void AddLaunch(ShipConstruct ship)
        {
            Costs.Instance.AddLaunch(ship);
        }
    }
}