using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP.UI.Screens;
using Steamworks;
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
        public SettingsClass settings = new SettingsClass();
        public static Bureaucracy Instance;
        protected Utilities utilities = new Utilities();
        protected List<Manager> reportManagers = new List<Manager>();

        private void Awake()
        {
            RegisterBureaucracyManagers();
            Instance = this;
        }

        private void Start()
        {
            InternalEvents.OnBudgetAwarded.Add(GenerateReport);
        }

        protected void RegisterBureaucracyManagers()
        {
            reportManagers.Add(new BudgetManager());
            reportManagers.Add(new FacilityManager());
        }

        public void RegisterManager(Manager m)
        {
            if (reportManagers.Contains(m))
            {
                Debug.Log("[Bureaucracy]: Attempted to register manager " + m.ToString() + " but already exists");
                return;
            }
            Debug.Log("[Bureaucracy]: Registered Custom Manager " + m);
            reportManagers.Add(m);
        }

        public void OnLoad(ConfigNode node)
        {
            SettingsClass.Instance.OnLoad(node);
            BudgetManager.Instance.OnLoad(node);
            FacilityManager.Instance.OnLoad(node);
        }

        public void OnSave(ConfigNode node)
        {
            SettingsClass.Instance.OnSave(node);
            BudgetManager.Instance.OnSave(node);
            FacilityManager.Instance.OnSave(node);
        }

        protected void GenerateReport(double data0, double data1)
        {
            for (int i = 0; i < reportManagers.Count; i++)
            {
                Manager m = reportManagers.ElementAt(i);
                Report r = m.GetReport();
                MessageSystem.Message message = new MessageSystem.Message(r.ReportTitle, r.ReportBody(), MessageSystemButton.MessageButtonColor.BLUE, MessageSystemButton.ButtonIcons.MESSAGE);
                MessageSystem.Instance.AddMessage(message);
            }
        }

        void OnDisable()
        {
            InternalEvents.OnBudgetAwarded.Remove(GenerateReport);
        }
    }
}