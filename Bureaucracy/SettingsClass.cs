
using UnityEngine;

namespace Bureaucracy
{
    public class SettingsClass
    {
        public static SettingsClass Instance;
        public int BudgetMultiplier = 2227;
        public int ScienceMultiplier = 1000;
        public float TimeBetweenBudgets = 30.0f;
        public bool StopTimeWarp = true;
        public bool UseItOrLoseIt = true;
        public bool HardMode = false;
        public bool RepDecayEnabled = false;
        public int RepDecayPercent = 0;
        public int AdminCost = 4000;
        public int AstronautComplexCost = 2000;
        public int MissionControlCost = 6000;
        public int SphCost = 8000;
        public int TrackingStationCost = 4000;
        public int RndCost = 8000;
        public int VabCost = 8000;
        public int OtherFacilityCost = 5000;
        public int LaunchCostSph = 100;
        public int LaunchCostVab = 1000;
        public int KerbalBaseWage = 1000;
        public bool ContractInterceptor = true;
        public bool HandleKscUpgrades = true;
        public float LongTermBonusYears = 10000;
        public float LongTermBonusDays = 30;
        public int BaseStrikesToQuit = 6;
        public int StrikeMemory = 6;
        public bool AstronautTraining = true;
        //TODO: Tidy this up so they are in a logical order

        public SettingsClass()
        {
            Instance = this;
        }

        //TODO: Make Settings Load and Save
        public void OnLoad(ConfigNode cn)
        {
            Debug.Log("[Bureaucracy]: Settings Class would have loaded if you'd written it");
        }

        public void OnSave(ConfigNode cn)
        {
            Debug.Log("[Bureaucracy]: Settings Class would have saved if you'd written it");
        }
    }
}