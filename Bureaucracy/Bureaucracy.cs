using System.Collections.Generic;
using System.IO;
using System.Linq;
using Smooth.Compare.Utilities;
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
        public SettingsClass settings;
        public static Bureaucracy Instance;
        public float qaModifier = 1.0f;
        public List<Manager> registeredManagers = new List<Manager>();

        private void Awake()
        {
            //Mod starts here.
            settings = new SettingsClass();
            RegisterBureaucracyManagers();
            Instance = this;
            Debug.Log("[Bureaucracy]: Awake");
        }

        private void Start()
        {
            InternalListeners.OnBudgetAwarded.Add(GeneratePostBudgetReport);
            KacWrapper.InitKacWrapper();
            if (SettingsClass.Instance.KctError && Directory.Exists(KSPUtil.ApplicationRootPath + "/GameData/KerbalConstructionTime")) UiController.Instance.errorWindow = UiController.Instance.KctError();
        }
        
        private void RegisterBureaucracyManagers()
        {
            //Register internal manager classes (and gives me a place to store the references). Expandable.
            registeredManagers.Add(new BudgetManager());
            registeredManagers.Add(new FacilityManager());
            registeredManagers.Add(new ResearchManager());
            registeredManagers.Add(new CrewManager(HighLogic.CurrentGame.CrewRoster.Crew.ToList()));
        }

        public void RetryKACAlarm()
        {
            //KAC API isn't always ready when we try to add an alarm, so we retry after a few seconds.
            if (!KacWrapper.AssemblyExists) return;
            KacWrapper.Kacapi.KacAlarmList kacAlarms = KacWrapper.Kac.Alarms;
            for (int i = 0; i < kacAlarms.Count; i++)
            {
                KacWrapper.Kacapi.KacAlarm alarm = kacAlarms.ElementAt(i);
                if (alarm.Name == "Next Budget") return;
            }

            double alarmTime = Planetarium.GetUniversalTime() + SettingsClass.Instance.TimeBetweenBudgets * FlightGlobals.GetHomeBody().solarDayLength;
            Utilities.Instance.NewKacAlarm("Next Budget", alarmTime);
        }

        public void SetCalcsDirty()
        {
            //Costs gets called alot at budget time, to save overheads we cache them for 5 seconds.
            //Setting Dirty lets mod know that it needs to calculate them again.
            Costs.Instance.SetCalcsDirty();
        }
        public void RegisterManager(Manager m)
        {
            //For mods to register custom managers.
            if (registeredManagers.Contains(m))
            {
                Debug.Log("[Bureaucracy]: Attempted to register manager" +m.Name+ " but already exists");
                return;
            }

            Debug.Log("[Bureaucracy]: Registered Custom Manager" +m.Name);
            registeredManagers.Add(m);
        }

        public void OnLoad(ConfigNode node)
        {
            //ScenarioModule OnLoad event redirects here, each class handles it's own saving/loading (for better encapsulation).
            Debug.Log("[Bureaucracy]: OnLoad");
            if(float.TryParse(node.GetValue("QAModifier"), out float f))qaModifier = f;
            SettingsClass.Instance.InGameLoad();
            BudgetManager.Instance.OnLoad(node);
            FacilityManager.Instance.OnLoad(node);
            ResearchManager.Instance.OnLoad(node);
            CrewManager.Instance.OnLoad(node);
            RandomEventLoader.Instance.OnLoad(node);
            UiController.Instance.OnLoad(node);
            Debug.Log("[Bureaucracy]: OnLoad Complete");
        }

        public void OnSave(ConfigNode node)
        {
            //ScenarioModule OnLoad event redirects here, each class handles it's own saving/loading (for better encapsulation).
            Debug.Log("[Bureaucracy]: OnSave");
            node.SetValue("QAModifier", qaModifier, true);
            SettingsClass.Instance.InGameSave();
            BudgetManager.Instance.OnSave(node);
            FacilityManager.Instance.OnSave(node);
            ResearchManager.Instance.OnSave(node);
            CrewManager.Instance.OnSave(node);
            RandomEventLoader.Instance.OnSave(node);
            UiController.Instance.OnSave(node);
            Debug.Log("[Bureaucracy]: OnSave Complete");
        }

        private void GeneratePostBudgetReport(double data0, double data1)
        {
            //All Managers generate a report.
            Debug.Log("[Bureaucracy]: Firing Manager Reports");
            for (int i = 0; i < registeredManagers.Count; i++)
            {
                Manager m = registeredManagers.ElementAt(i);
                m.MakeReport();
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < registeredManagers.Count; i++)
            {
                Manager m = registeredManagers.ElementAt(i);
                m.UnregisterEvents();
            }
            InternalListeners.OnBudgetAwarded.Remove(GeneratePostBudgetReport);
        }
    }
}