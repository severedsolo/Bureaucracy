using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
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
        [UsedImplicitly] private Utilities utilitiesReference = new Utilities();
        public SettingsClass settings;
        public static Bureaucracy Instance;
        public List<Manager> registeredManagers = new List<Manager>();
        public bool existingSave;
        public ManagerProgressEvent progressEvent;
        public double lastProgressUpdate = 0;

        private void Awake()
        {
            //Mod starts here.
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
            {
                Destroy(this);
                return;
            }
            settings = new SettingsClass();
            RegisterBureaucracyManagers();
            Instance = this;
            lastProgressUpdate = Planetarium.GetUniversalTime();
            Debug.Log("[Bureaucracy]: Awake");
        }

        private void Start()
        {
            InternalListeners.OnBudgetAwarded.Add(GeneratePostBudgetReport);
            if (FireKCTWarning()) UiController.Instance.errorWindow = UiController.Instance.KctError();
        }

        private bool FireKCTWarning()
        {
            Debug.Log("[Bureaucracy]: Checking for KCT");
            //No point going any further if we're not handling facility upgrades
            if (!SettingsClass.Instance.HandleKscUpgrades)
            {
                Debug.Log("[Bureaucracy]: KSC Upgrades are disabled");
                return false;
            }
            //Now let's see if KCT is installed
            bool kctFound = false;
            foreach (AssemblyLoader.LoadedAssembly a in AssemblyLoader.loadedAssemblies)
            {
                if (!a.name.Equals("KerbalConstructionTime")) continue;
                kctFound = true;
                Debug.Log("[Bureaucracy]: KCT found");
                break;
            }

            if (!kctFound)
            {
                Debug.Log("[Bureaucracy]: KCT not found");
                return false;
            }
            //KCT installed? Let's try loading the settings file.
            ConfigNode kctNode = ConfigNode.Load(KSPUtil.ApplicationRootPath + "/saves/" + HighLogic.SaveFolder + "/KCT_Settings.cfg");
            //If it's null, probably first load so the warning needs to fire
            if (kctNode == null)
            {
                Debug.Log("[Bureaucracy]: KCT Node is null. Firing Warning");
                return true;
            }
            //Start drilling down the nodes until we get to the one we want
            kctNode = kctNode.GetNode("KCT_Preset");
            kctNode = kctNode.GetNode("KCT_Preset_General");
            //Finally - let's see if we can get the setting
            string s = kctNode.GetValue("KSCUpgradeTimes");
            bool.TryParse(s, out bool b);
            Debug.Log("[Bureaucracy]: KCT Facility Upgrades Enabled? " + b);
            return b;
        }

        private void RegisterBureaucracyManagers()
        {
            //Register internal manager classes (and gives me a place to store the references). Expandable.
            registeredManagers.Add(new BudgetManager());
            registeredManagers.Add(new FacilityManager());
            registeredManagers.Add(new ResearchManager());
            registeredManagers.Add(new CrewManager(HighLogic.CurrentGame.CrewRoster.Crew.ToList()));
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
            SettingsClass.Instance.InGameLoad();
            BudgetManager.Instance.OnLoad(node);
            FacilityManager.Instance.OnLoad(node);
            ResearchManager.Instance.OnLoad(node);
            CrewManager.Instance.OnLoad(node);
            RandomEventLoader.Instance.OnLoad(node);
            node.TryGetValue("existingSave", ref existingSave);
            node.TryGetValue("lastProgressUpdate", ref lastProgressUpdate);
            if(progressEvent == null) progressEvent = new ManagerProgressEvent();
            Debug.Log("[Bureaucracy]: OnLoad Complete");
        }

        public void OnSave(ConfigNode node)
        {
            //ScenarioModule OnLoad event redirects here, each class handles it's own saving/loading (for better encapsulation).
            Debug.Log("[Bureaucracy]: OnSave");
            SettingsClass.Instance.InGameSave();
            BudgetManager.Instance.OnSave(node);
            FacilityManager.Instance.OnSave(node);
            ResearchManager.Instance.OnSave(node);
            CrewManager.Instance.OnSave(node);
            RandomEventLoader.Instance.OnSave(node);
            node.SetValue("existingSave", existingSave, true);
            node.SetValue("lastProgressUpdate", lastProgressUpdate, true);
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
        /*This is a horrible hack but Planetarium.GetUniversalTime() isn't up when the game first loads.
         So if we can't load the last budget time from the save (ie first install of the mod), we'll get 0 (new game) as a time - even if the time is not actually 0.
        Bureaucracy will then run budgets in sequence from Y1D1 (timestamp 0). Obviously this is bad for existing saves as Kerbals will retire, and the mod has no business retroactively running.
        So wait 1 second to give Planetarium.GetUniversalTime() to return a sensible value. If it's still 0 this is probably a new game. If not, we should have the right timestamp.
        This obviously leads to another issue where the first budget will be 30 days after whenever the player installed the mod, but better that than running 5 million budgets.
        */
        public void YieldAndCreateBudgetOnNewGame()
        {
            Invoke(nameof(NewGameBudget), 1.0f);
        }

        public void NewGameBudget()
        {
            BudgetManager.Instance.CreateNewBudget();
        }
    }
}