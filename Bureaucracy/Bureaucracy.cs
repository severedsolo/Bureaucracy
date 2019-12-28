using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinePrint;
using KSP.UI.Screens;
using Steamworks;
using UnityEngine;
using UnityEngine.Serialization;

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
        public SettingsClass settings = new SettingsClass();
        public static Bureaucracy Instance;
        private Utilities utilities = new Utilities();
        public List<Manager> registeredManagers = new List<Manager>();

        private void Awake()
        {
            RegisterBureaucracyManagers();
            Instance = this;
        }

        private void Start()
        {
            InternalListeners.OnBudgetAwarded.Add(GeneratePostBudgetReport);
            
        }

        private void RegisterBureaucracyManagers()
        {
            registeredManagers.Add(new BudgetManager());
            registeredManagers.Add(new FacilityManager());
            registeredManagers.Add(new ResearchManager());
            registeredManagers.Add(new CrewManager(HighLogic.CurrentGame.CrewRoster.Crew.ToList()));
        }

        public void SetCalcsDirty()
        {
            Costs.Instance.SetCalcsDirty();
        }
        public void RegisterManager(Manager m)
        {
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
            SettingsClass.Instance.OnLoad(node);
            BudgetManager.Instance.OnLoad(node);
            FacilityManager.Instance.OnLoad(node);
            ResearchManager.Instance.OnLoad(node);
            CrewManager.Instance.OnLoad(node);
        }

        public void OnSave(ConfigNode node)
        {
            SettingsClass.Instance.OnSave(node);
            BudgetManager.Instance.OnSave(node);
            FacilityManager.Instance.OnSave(node);
            ResearchManager.Instance.OnSave(node);
            CrewManager.Instance.OnSave(node);
        }

        private void GeneratePostBudgetReport(double data0, double data1)
        {
            for (int i = 0; i < registeredManagers.Count; i++)
            {
                Manager m = registeredManagers.ElementAt(i);
                m.MakeReport();
            }
        }

        void OnDisable()
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