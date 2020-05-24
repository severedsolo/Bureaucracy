using System.IO;
using UnityEngine;

namespace Bureaucracy
{
    public class SettingsClass
    {
        public static SettingsClass Instance;
        public bool ContractInterceptor = true;
        private bool autoBalanceSettings = true;
        public bool KctError = true;
        public bool HandleKscUpgrades = true;
        public bool HandleScience = true;
        public bool StopTimeWarp = true;
        public bool UseItOrLoseIt = true;
        public bool HardMode;
        public bool RepDecayEnabled;
        public bool RandomEventsEnabled = true;
        public float RandomEventChance = 0.05f;
        public bool AstronautTraining = true;
        public float TimeBetweenBudgets = 30.0f;
        public int BudgetMultiplier = 2227;
        public int ScienceMultiplier = 1000;
        public int RepDecayPercent;
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
        public float LongTermBonusYears = 10000;
        public float LongTermBonusDays = 30;
        public int BaseStrikesToQuit = 10;
        public int MinimumTerm = 1;
        public int MaximumTerm = 5;
        public int BaseTrainingFee = 60000;
        public bool RetirementEnabled = true;
        public double RetirementExtensionFactor = 0.33f;
        public int StrikeMemory = 6;
        public int DeadKerbalPenalty = 25;
        private readonly string defaultPath;
        private const string SettingsVersion = "1.4";
        private const string PreviousVersion = "1.1";
        private readonly string savePath;

        public SettingsClass()
        {
            Instance = this;
            defaultPath = KSPUtil.ApplicationRootPath+ "/GameData/Bureaucracy/defaultSettings.cfg";
            savePath = KSPUtil.ApplicationRootPath+"/saves/"+HighLogic.SaveFolder+"/BureaucracySettings.cfg";
            if (!File.Exists(defaultPath))
            {
                Debug.Log("[Bureaucracy]: Can't find default settings file. Creating it");
                OnSave(defaultPath);
            }
            if (!File.Exists(savePath))
            {
                Debug.Log("[Bureaucracy]: Can't find settings file for save. Creating it");
                OnLoad(defaultPath);
                OnSave(savePath);
            }
            else OnLoad(savePath);
        }

        public void InGameSave()
        {
            OnSave(savePath);
        }

        public void InGameLoad()
        {
            OnLoad(savePath);
        }

        private void OnLoad(string path)
        {
            Debug.Log("[Bureaucracy]: Loading Settings");
            ConfigNode cn = ConfigNode.Load(path);
            string saveVersion = cn.GetValue("Version");
            if (saveVersion != SettingsVersion && saveVersion != PreviousVersion)
            {
                Debug.Log("[Bureaucracy]: Settings are not compatible with this version of Bureaucracy. Aborting Load");
                return;
            }
            bool.TryParse(cn.GetValue("AutoBalanceSettings"), out autoBalanceSettings);
            bool.TryParse(cn.GetValue("HandleScience"), out HandleScience);
            bool.TryParse(cn.GetValue("ShowKCTWarning"), out KctError);
            bool.TryParse(cn.GetValue("ContractInterceptorEnabled"), out ContractInterceptor);
            bool.TryParse(cn.GetValue("HandleKSCUpgrades"), out HandleKscUpgrades);
            bool.TryParse(cn.GetValue("StopTimeWarp"), out StopTimeWarp);
            bool.TryParse(cn.GetValue("UseItOrLoseIt"), out UseItOrLoseIt);
            bool.TryParse(cn.GetValue("HardModeEnabled"), out HardMode);
            bool.TryParse(cn.GetValue("RepDecayEnabled"), out RepDecayEnabled);
            bool.TryParse(cn.GetValue("AstronautTrainingEnabled"), out AstronautTraining);
            bool.TryParse(cn.GetValue("RandomEventsEnabled"), out RandomEventsEnabled);
            float.TryParse(cn.GetValue("RandomEventChance"), out RandomEventChance);
            float.TryParse(cn.GetValue("TimeBetweenBudgetsDays"), out TimeBetweenBudgets);
            int.TryParse(cn.GetValue("RepToFundsMultiplier"), out BudgetMultiplier);
            int.TryParse(cn.GetValue("ScienceToFundsMultiplier"), out ScienceMultiplier);
            int.TryParse(cn.GetValue("RepDecayPercent"), out RepDecayPercent);
            int.TryParse(cn.GetValue("AdminFacilityBaseCost"), out AdminCost);
            int.TryParse(cn.GetValue("AstronautComplexBaseCost"), out AstronautComplexCost);
            int.TryParse(cn.GetValue("MissionControlBaseCost"), out MissionControlCost);
            int.TryParse(cn.GetValue("SPHBaseCost"), out SphCost);
            int.TryParse(cn.GetValue("TrackingStationBaseCost"), out TrackingStationCost);
            int.TryParse(cn.GetValue("RndBaseCost"), out RndCost);
            int.TryParse(cn.GetValue("VABBaseCost"), out VabCost);
            int.TryParse(cn.GetValue("ModFacilityBaseCost"), out OtherFacilityCost);
            int.TryParse(cn.GetValue("BaseLaunchCostSPH"), out SphCost);
            int.TryParse(cn.GetValue("BaseLaunchCostVAB"), out VabCost);
            int.TryParse(cn.GetValue("CrewBaseWage"), out KerbalBaseWage);
            float.TryParse(cn.GetValue("LongTermMissionBonusPerYear"), out LongTermBonusYears);
            float.TryParse(cn.GetValue("LongTermMissionBonusPerDay"), out LongTermBonusDays);
            int.TryParse(cn.GetValue("BaseStrikesBeforeKerbalQuits"), out BaseStrikesToQuit);
            int.TryParse(cn.GetValue("StrikeMemoryMonths"), out StrikeMemory);
            int.TryParse(cn.GetValue("DeadKerbalRepPenaltyPercent"), out DeadKerbalPenalty);
            if (saveVersion == "1.4")
            {
                bool.TryParse(cn.GetValue("RetirementEnabled"), out RetirementEnabled);
                double.TryParse(cn.GetValue("RetirementExtensionFactor"), out RetirementExtensionFactor);
                int.TryParse(cn.GetValue("MinimumTerm"), out MinimumTerm);
                int.TryParse(cn.GetValue("MaximumTerm"), out MaximumTerm);
                int.TryParse(cn.GetValue("BaseTrainingFee"), out BaseTrainingFee);
            }
            if (RefreshBudget())
            {
                TimerScript.Instance.RemoveTimer(BudgetManager.Instance.NextBudget);
                BudgetManager.Instance.NextBudget = null;
                BudgetManager.Instance.NextBudget = new BudgetEvent(Planetarium.GetUniversalTime()+TimeBetweenBudgets*FlightGlobals.GetHomeBody().solarDayLength, BudgetManager.Instance, true);
            }

            if (autoBalanceSettings) BalanceSettings();
            if(saveVersion != SettingsVersion) OnSave(defaultPath); 
            Debug.Log("[Bureaucracy]: Settings Loaded");
        }

        private void BalanceSettings()
        {
            float balanceMultiplier = TimeBetweenBudgets / 30.0f;
            AdminCost = BalanceCost(4000, balanceMultiplier);
            AstronautComplexCost = BalanceCost(2000, balanceMultiplier);
            MissionControlCost = BalanceCost(6000, balanceMultiplier);
            SphCost = BalanceCost(8000, balanceMultiplier);
            TrackingStationCost = BalanceCost(4000, balanceMultiplier);
            RndCost = BalanceCost(8000, balanceMultiplier);
            VabCost = BalanceCost(8000, balanceMultiplier);
            OtherFacilityCost = BalanceCost(5000, balanceMultiplier);
            LaunchCostSph = BalanceCost(100, balanceMultiplier);
            LaunchCostVab = BalanceCost(1000, balanceMultiplier);
            KerbalBaseWage = BalanceCost(1000, balanceMultiplier);
            LongTermBonusYears = BalanceCost(10000, balanceMultiplier);
            LongTermBonusDays = BalanceCost(30, balanceMultiplier);
        }

        private int BalanceCost(int initialCost, float balanceMultiplier)
        {
            float actualCost = initialCost * balanceMultiplier;
            return (int)actualCost;
        }

        private bool RefreshBudget()
        {
            if (BudgetManager.Instance == null) return false;
            if (BudgetManager.Instance.NextBudget == null) return false;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (BudgetManager.Instance.NextBudget.MonthLength == TimeBetweenBudgets) return false;
            return true;
        }

        private void OnSave(string path)
        {
            Debug.Log("[Bureaucracy]: Saving Settings");
            ConfigNode cn = new ConfigNode("SETTINGS");
            cn.SetValue("Version", SettingsVersion, true);
            cn.SetValue("AutoBalanceSettings", autoBalanceSettings, true);
            cn.SetValue("ShowKCTWarning", KctError, true);
            cn.SetValue("ContractInterceptorEnabled", ContractInterceptor, true);
            cn.SetValue("HandleKSCUpgrades", HandleKscUpgrades, true);
            cn.SetValue("HandleScience", HandleScience, true);
            cn.SetValue("StopTimeWarp", StopTimeWarp, true);
            cn.SetValue("UseItOrLoseIt", UseItOrLoseIt, true);
            cn.SetValue("HardModeEnabled", HardMode, true);
            cn.SetValue("RepDecayEnabled", RepDecayEnabled, true);
            cn.SetValue("RepDecayPercent", RepDecayPercent, true);
            cn.SetValue("AstronautTrainingEnabled", AstronautTraining, true);
            cn.SetValue("RandomEventsEnabled", RandomEventsEnabled, true);
            cn.SetValue("RandomEventChance", RandomEventChance, true);
            cn.SetValue("RetirementEnabled", RetirementEnabled, true);
            cn.SetValue("RetirementExtensionFactor", RetirementExtensionFactor, true);
            cn.SetValue("MinimumTerm", MinimumTerm, true);
            cn.SetValue("MaximumTerm", MaximumTerm, true);
            cn.SetValue("BaseTrainingFee", BaseTrainingFee, true);
            cn.SetValue("TimeBetweenBudgetsDays", TimeBetweenBudgets, true);
            cn.SetValue("RepToFundsMultiplier", BudgetMultiplier, true);
            cn.SetValue("ScienceToFundsMultiplier", ScienceMultiplier, true);
            cn.SetValue("AdminFacilityBaseCost", AdminCost, true);
            cn.SetValue("AstronautComplexBaseCost", AstronautComplexCost, true);
            cn.SetValue("MissionControlBaseCost", MissionControlCost, true);
            cn.SetValue("SPHBaseCost", SphCost, true);
            cn.SetValue("TrackingStationBaseCost", TrackingStationCost, true);
            cn.SetValue("RndBaseCost", RndCost, true);
            cn.SetValue("VABBaseCost", VabCost, true);
            cn.SetValue("ModFacilityBaseCost", OtherFacilityCost, true);
            cn.SetValue("BaseLaunchCostSPH", LaunchCostSph, true);
            cn.SetValue("BaseLaunchCostVAB", LaunchCostVab, true);
            cn.SetValue("CrewBaseWage", KerbalBaseWage, true);
            cn.SetValue("LongTermMissionBonusPerYear", LongTermBonusYears, true);
            cn.SetValue("LongTermMissionBonusPerDay", LongTermBonusDays, true);
            cn.SetValue("BaseStrikesBeforeKerbalQuits", BaseStrikesToQuit, true);
            cn.SetValue("StrikeMemoryMonths", StrikeMemory, true);
            cn.SetValue("DeadKerbalRepPenaltyPercent", DeadKerbalPenalty, true);
            cn.Save(path);
            Debug.Log("[Bureaucracy]: Settings Saved");
        }
}
}