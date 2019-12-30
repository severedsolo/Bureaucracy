using System;
using System.Collections.Generic;
using System.Linq;
using KSP.UI.Screens;
using UnityEngine;
using Upgradeables;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class UiController : MonoBehaviour
    {
        private ApplicationLauncherButton toolbarButton;
        public static UiController Instance;
        private bool showMainUi;
        private bool showFacilityUi;
        private bool showResearchUi;
        private bool showSettingsUi;
        private bool showErrorUi;
        private List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
        private GUIStyle boxStyle;
        private GUIStyle buttonStyle;
        private bool hasCentred;
        private GUIStyle messageStyle;
        private GUIStyle titleStyle;
        private Rect window = new Rect(20, 100, 400, 50);
        private int windowId = 0;

        private void Awake()
        {
            Instance = this;
            SetupStyles();
        }

        private void SetupStyles()
        {
            boxStyle = new GUIStyle(HighLogic.Skin.box)
            {
                padding = new RectOffset(10, 10, 5, 5)
            };
            titleStyle = new GUIStyle(HighLogic.Skin.label)
            {
                normal =
                {
                    textColor = Color.white
                },
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                stretchWidth = true
            };

            messageStyle = new GUIStyle(HighLogic.Skin.label)
            {
                stretchWidth = true
            };

            buttonStyle = new GUIStyle(HighLogic.Skin.button)
            {
                normal =
                {
                    textColor = Color.white
                }
            };
        }

        
        public void SetupToolbarButton()
        {
            toolbarButton = ApplicationLauncher.Instance.AddModApplication(() => ActivateUi("main"), ClearUi, null, null, null, null, ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.TRACKSTATION, GameDatabase.Instance.GetTexture("Bureaucracy/Icon", false));
        }
        private void ActivateUi(string screen)
        {
            ClearUi();
            switch (screen)
            {
                case "main":
                    showMainUi = true;
                    break;
                case "facility":
                    showFacilityUi = true;
                    break;
                case "research":
                    showResearchUi = true;
                    break;
                case "settings":
                    showSettingsUi = true;
                    break;
                default:
                    showErrorUi = true;
                    break;
            }
        }

        private int GenerateWindowId()
        {
            return Guid.NewGuid().GetHashCode();
        }

        private void OnGUI()
        {
            if (showMainUi) window = GUILayout.Window(windowId, window, DrawMainUi, "Bureaucracy - Budget Manager", HighLogic.Skin.window, GUILayout.Height(200), GUILayout.Height(200));
            else if(showFacilityUi) window = GUILayout.Window(windowId, window, DrawFacilityUi, "Bureaucracy - Construction Manager", HighLogic.Skin.window, GUILayout.Height(200), GUILayout.Height(200)); 
            else if(showResearchUi) window = GUILayout.Window(windowId, window, DrawResearchUi, "Bureaucracy - Research Manager", HighLogic.Skin.window, GUILayout.Height(200), GUILayout.Height(200));
        }

        private void DrawMainUi(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Next Budget: "+Utilities.Instance.ConvertUtToKspTimeStamp(BudgetManager.Instance.NextBudget.CompletionTime), messageStyle);
            GUILayout.Label("Gross Budget: "+Utilities.Instance.GetGrossBudget(), messageStyle);
            GUILayout.Label("Wage Costs: "+Costs.Instance.GetWageCosts(), messageStyle);
            GUILayout.Label("Facility Maintenance Costs:"+Costs.Instance.GetFacilityMaintenanceCosts(), messageStyle);
            int bonusesToPay = 0;
            for (int i = 0; i < CrewManager.Instance.Kerbals.Count; i++)
            {
                CrewMember c = CrewManager.Instance.Kerbals.ElementAt(i).Value;
                bonusesToPay += c.GetBonus(false);
            }
            GUILayout.Label("Mission Bonuses: "+bonusesToPay, messageStyle);
            for (int i = 0; i < Bureaucracy.Instance.registeredManagers.Count; i++)
            {
                Manager m = Bureaucracy.Instance.registeredManagers.ElementAt(i);
                if (m.Name == "Budget") continue;
                double departmentFunding = Utilities.Instance.GetNetBudget(m.Name);
                if(departmentFunding < 0.0f) continue;
                GUILayout.Label(m.Name+" Department Funding: "+departmentFunding, messageStyle);
            }
            GUILayout.Label("Net Budget:" +Utilities.Instance.GetNetBudget("Budget"), messageStyle);
            GUILayout.EndVertical();
            DrawBottomOfWindow("Main");
            GUI.DragWindow();
        }
        
        private void DrawFacilityUi(int id)
        {
        GUILayout.BeginVertical();
        int upgradeCount = 0;
            for (int i = 0; i < FacilityManager.Instance.Facilities.Count; i++)
            {
                BureaucracyFacility bf = FacilityManager.Instance.Facilities.ElementAt(i);
                if (!bf.Upgrading) continue;
                upgradeCount++;
                GUILayout.BeginHorizontal();
                GUILayout.Label(bf.Name+": "+bf.Upgrade.RemainingInvestment+" / "+bf.Upgrade.OriginalCost, messageStyle);
                if (GUILayout.Button("X", buttonStyle, GUILayout.MaxWidth(20.0f))) bf.CancelUpgrade();
                GUILayout.EndHorizontal();
            }
            if(upgradeCount == 0) GUILayout.Label("No Facility Upgrades in progress", messageStyle);
            GUILayout.EndVertical();
            DrawBottomOfWindow("Facility");
        }
        
        private void DrawResearchUi(int id)
        {
            GUILayout.BeginVertical();
            if(ResearchManager.Instance.ProcessingScience.Count == 0) GUILayout.Label("No research in progress", messageStyle);
            for (int i = 0; i < ResearchManager.Instance.ProcessingScience.Count; i++)
            {
                ScienceEvent se = ResearchManager.Instance.ProcessingScience.ElementAt(i);
                GUILayout.Label(se.UiName+": "+se.RemainingScience+se.OriginalScience);
            }
            GUILayout.EndVertical();
            DrawBottomOfWindow("Research");
        }

        private void DrawBottomOfWindow(string passingUi)
        {
            GUILayout.BeginHorizontal();
            if (passingUi != "Main")
            {
                if (GUILayout.Button("Budget", buttonStyle))
                {
                    ActivateUi("main");
                }
            }
            if (passingUi != "Facility")
            {
                if (GUILayout.Button("Budget", buttonStyle))
                {
                    ActivateUi("facility");
                }
            }
            if (passingUi != "Research")
            {
                if (GUILayout.Button("Research", buttonStyle))
                {
                    ActivateUi("research");
                }
            }

            if (passingUi != "Settings")
            {
                if (GUILayout.Button("Settings", buttonStyle))
                {
                    ActivateUi("settings");
                }
            }
            GUILayout.EndHorizontal();
            if(GUILayout.Button("Close", buttonStyle)) ClearUi();
            GUI.DragWindow();
        }

        private void ClearUi()
        {
            showMainUi = false;
            showFacilityUi = false;
            showResearchUi = false;
            showSettingsUi = false;
        }

        public void RemoveToolbarButton()
        {
            if (toolbarButton == null) return;
            ApplicationLauncher.Instance.RemoveModApplication(toolbarButton);
        }

        private void OnDisable()
        {
            RemoveToolbarButton();
        }
    }
}