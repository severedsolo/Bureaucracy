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
        private BudgetManager budgetManager = new BudgetManager();
        public SettingsManager settings = new SettingsManager();
        public static Bureaucracy Instance;
        private Utilities utilities = new Utilities();
        private FacilityManager facilityManager = new FacilityManager();
        private List<Manager> reportManagers = new List<Manager>();

        private void Awake()
        {
            Instance = this;
            BureaucracyGameEvents.OnBudgetAwarded.Add(GenerateReport);
        }

        public void RegisterManager(Manager m)
        {
            reportManagers.Add(m);
        }

        private void GenerateReport(double data0, double data1)
        {
            for (int i = 0; i < reportManagers.Count; i++)
            {
                Manager m = reportManagers.ElementAt(i);
                Report r = m.GetReport();
                MessageSystem.Message message = new MessageSystem.Message(r.ReportTitle, r.ReportBody(), MessageSystemButton.MessageButtonColor.BLUE, MessageSystemButton.ButtonIcons.MESSAGE);
                MessageSystem.Instance.AddMessage(message);
            }
        }
    }
}