using System;
using System.Collections.Generic;
using System.Linq;
using FlightTracker;
using KSP.UI.Screens;
using KSP.UI.Screens.Settings;
using UnityEngine;
using Upgradeables;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class UiController : MonoBehaviour
    {
        private ApplicationLauncherButton toolbarButton;
        public static UiController Instance;
        private PopupDialog mainWindow;
        private PopupDialog facilitiesWindow;
        private PopupDialog researchWindow;
        private PopupDialog allocationWindow;
        private PopupDialog errorWindow;
        private int padding = 0;
        private int padFactor = 10;

        private void Awake()
        {
            Instance = this;
        }

        public void GenerateErrorWindow(string error)
        {
            errorWindow = DrawErrorWindow(error);
        }

        private PopupDialog DrawErrorWindow(string error)
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            dialogElements.Add(new DialogGUIHorizontalLayout(PaddedLabel(error)));
            dialogElements.Add(CloseButton());
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("BureaucracyError", "", "Bureaucracy: Warning", UISkinManager.GetSkin("MainMenuSkin"),
                    new Rect(0.5f, 0.5f, 650, 90), dialogElements.ToArray()), false, UISkinManager.GetSkin("MainMenuSkin"));
        }

        public void SetupToolbarButton()
        {
            toolbarButton = ApplicationLauncher.Instance.AddModApplication(() => ActivateUi("main"), () => ActivateUi("main"), null, null, null, null, ApplicationLauncher.AppScenes.SPACECENTER, GameDatabase.Instance.GetTexture("Bureaucracy/Icon", false));
        }
        private void ActivateUi(string screen)
        {
           DismissAllWindows();
            switch (screen)
            {
                case "main":
                    mainWindow = DrawMainUi();
                    break;
                case "facility":
                    facilitiesWindow = DrawFacilityUi();
                    break;
                case "research":
                    researchWindow = DrawResearchUi();
                    break;
                case "settings":
                    allocationWindow = DrawBudgetAllocationUi();
                    break;
            }
        }

        private PopupDialog DrawBudgetAllocationUi()
        {
            throw new NotImplementedException();
        }

        private void DismissAllWindows()
        {
            if (mainWindow != null) mainWindow.Dismiss();
            if (facilitiesWindow != null) facilitiesWindow.Dismiss();
            if (researchWindow != null) researchWindow.Dismiss();
            if (allocationWindow != null) allocationWindow.Dismiss();
        }

        private PopupDialog DrawMainUi()
        {
            padding = 0;
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            List<DialogGUIBase> innerElements = new List<DialogGUIBase>();
            if(HighLogic.CurrentGame.Mode != Game.Modes.CAREER)  innerElements.Add(new DialogGUILabel("Bureaucracy is only available in Career Games"));
            else
            {
                innerElements.Add(new DialogGUISpace(10));
                innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel("Next Budget: " + Utilities.Instance.ConvertUtToKspTimeStamp(BudgetManager.Instance.NextBudget.CompletionTime))));
                innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel("Gross Budget: $" + Utilities.Instance.GetGrossBudget())));
                innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel("Wage Costs: $" + Costs.Instance.GetWageCosts())));
                innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel("Facility Maintenance Costs: $" + Costs.Instance.GetFacilityMaintenanceCosts())));
                innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel("Mission Bonuses: $" + GetBonusesToPay())));
                for (int i = 0; i < Bureaucracy.Instance.registeredManagers.Count; i++)
                {
                    Manager m = Bureaucracy.Instance.registeredManagers.ElementAt(i);
                    if (m.Name == "Budget") continue;
                    double departmentFunding = Utilities.Instance.GetNetBudget(m.Name);
                    if (departmentFunding < 0.0f) continue;
                    innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel(m.Name + " Department Funding: $" + departmentFunding)));
                }
                innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel("Net Budget: $"+Utilities.Instance.GetNetBudget("Budget"))));
                DialogGUIVerticalLayout vertical = new DialogGUIVerticalLayout(innerElements.ToArray());
                dialogElements.Add(new DialogGUIScrollList(-Vector2.one, false, false, vertical));
                dialogElements.Add(GetBoxes("main"));
            }
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("BureaucracyMain", "", "Bureaucracy: Budget", UISkinManager.GetSkin("MainMenuSkin"),
                    GetRect(dialogElements), dialogElements.ToArray()), false, UISkinManager.GetSkin("MainMenuSkin"));
        }

        private Rect GetRect(List<DialogGUIBase> dialogElements)
        {
            return new Rect(0.5f, 0.5f, 300, 265) {height = 150 + 50 * dialogElements.Count, width = padding};
        }

        private DialogGUIBase[] PaddedLabel(string stringToPad)
        {
            DialogGUIBase[] paddedLayout = new DialogGUIBase[2];
            paddedLayout[0] = new DialogGUISpace(10);
            EvaluatePadding(stringToPad);
            paddedLayout[1] = new DialogGUILabel(stringToPad, MessageStyle());
            return paddedLayout;
        }

        private void EvaluatePadding(string stringToEvaluate)
        {
            if (stringToEvaluate.Length *padFactor > padding) padding = stringToEvaluate.Length * padFactor;
        }

        private UIStyle MessageStyle()
        {
            return new UIStyle
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = false,
                normal = new UIStyleState
                {
                    textColor = new Color(0.89f, 0.86f, 0.72f)
                }
            };
        }

        private DialogGUIButton CloseButton()
        {
            return new DialogGUIButton("Close", () => { }, true);
            
        }

        private int GetBonusesToPay()
        {
            int bonusesToPay = 0;
            for (int i = 0; i < CrewManager.Instance.Kerbals.Count; i++)
            {
                CrewMember c = CrewManager.Instance.Kerbals.ElementAt(i).Value;
                bonusesToPay += c.GetBonus(false);
            }
            return bonusesToPay;
        }

        private PopupDialog DrawFacilityUi()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            List<DialogGUIBase> innerElements = new List<DialogGUIBase>();
            int upgradeCount = 0;
            innerElements.Add(new DialogGUISpace(10));
            for (int i = 0; i < FacilityManager.Instance.Facilities.Count; i++)
            {
                BureaucracyFacility bf = FacilityManager.Instance.Facilities.ElementAt(i);
                if (!bf.Upgrading) continue;
                upgradeCount++;
                innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel(bf.Name+": $"+(bf.Upgrade.OriginalCost-bf.Upgrade.RemainingInvestment)+" / $"+bf.Upgrade.OriginalCost)));
            }
            if (upgradeCount == 0) innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel("No Facility Upgrades in progress")));
            DialogGUIVerticalLayout vertical = new DialogGUIVerticalLayout(innerElements.ToArray());
            dialogElements.Add(new DialogGUIScrollList(-Vector2.one, false, false, vertical));
            dialogElements.Add(GetBoxes("facility"));
            dialogElements.Add(CloseButton());
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog("FacilitiesDialog", "", "Bureaucracy: Facilities", UISkinManager.GetSkin("MainMenuSkin"), GetRect(dialogElements), dialogElements.ToArray()), false, UISkinManager.GetSkin("MainMenuSkin"));
        }
        
        private PopupDialog DrawResearchUi()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            List<DialogGUIBase> innerElements = new List<DialogGUIBase>();
            innerElements.Add(new DialogGUISpace(10));
            if(ResearchManager.Instance.ProcessingScience.Count == 0) innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel("No research in progress")));
            for (int i = 0; i < ResearchManager.Instance.ProcessingScience.Count; i++)
            {
                ScienceEvent se = ResearchManager.Instance.ProcessingScience.ElementAt(i);
                innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel(se.UiName+": "+(se.OriginalScience-se.RemainingScience)+"/"+se.OriginalScience)));
            }
            DialogGUIVerticalLayout vertical = new DialogGUIVerticalLayout(innerElements.ToArray());
            dialogElements.Add(new DialogGUIScrollList(-Vector2.one, false, false, vertical));
            dialogElements.Add(GetBoxes("research"));
            dialogElements.Add(CloseButton());
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog("ResearchDialog", "", "Bureaucracy: Research", UISkinManager.GetSkin("MainMenuSkin"), GetRect(dialogElements), dialogElements.ToArray()), false, UISkinManager.GetSkin("MainMenuSkin"));
        }

        private DialogGUIHorizontalLayout GetBoxes(string passingUi)
        {
            int arrayPointer = 0;
            DialogGUIBase[] horizontal = new DialogGUIBase[4];
            if (passingUi != "main")
            {
                horizontal[arrayPointer] = new DialogGUIButton("Budget", ()=> ActivateUi("main"));
                arrayPointer++;
            }
            if (passingUi != "facility")
            {
                horizontal[arrayPointer] = new DialogGUIButton("Construction", () => ActivateUi("facility"));
                arrayPointer++;
            }
            if (passingUi != "research")
            {
             horizontal[arrayPointer] = new DialogGUIButton("Research", () => ActivateUi("research"));
             arrayPointer++;
            }
            if (passingUi != "settings")
            {
                horizontal[arrayPointer] = new DialogGUIButton("Settings", () => ActivateUi("settings"));
                arrayPointer++;
            }
            horizontal[arrayPointer] = new DialogGUIButton("Close", () => { }, true);
            return new DialogGUIHorizontalLayout(280, 35, horizontal);
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