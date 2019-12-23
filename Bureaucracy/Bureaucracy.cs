using System;
using System.Text;
using UnityEngine;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class BureaucracySpaceCentre : Bureaucracy
    {
        
    }
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class BureaucracyFlight : Bureaucracy
    {
        
    }
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class BureaucracyTrackingStation : Bureaucracy
    {
        
    }
    public class Bureaucracy : MonoBehaviour
    {
        private BudgetManager budgetManager = new BudgetManager();
        public SettingsManager settings = new SettingsManager();
        public static Bureaucracy Instance;
        private Utilities utilities = new Utilities();
        private FacilityManager facilityManager = new FacilityManager();


        private void Awake()
        {
            Instance = this;
            BureaucracyGameEvents.OnBudgetAwarded.Add(GenerateReport);
        }
    }
}