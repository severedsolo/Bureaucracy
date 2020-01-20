using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using KSP.UI.Screens;
using UnityEngine;

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
        private int fundingAllocation;
        private int constructionAllocation;
        private int researchAllocation;
        [UsedImplicitly] public PopupDialog errorWindow;
        private int padding;
        private const int PadFactor = 10;

        private void Awake()
        {
            Instance = this;
            SetAllocation("Budget Manager", "40");
            SetAllocation("Research", "30");
            SetAllocation("Construction", "30");
        }



        public void SetupToolbarButton()
        {
            if(HighLogic.CurrentGame.Mode == Game.Modes.CAREER) toolbarButton = ApplicationLauncher.Instance.AddModApplication(() => ActivateUi("main"), () => ActivateUi("main"), null, null, null, null, ApplicationLauncher.AppScenes.SPACECENTER, GameDatabase.Instance.GetTexture("Bureaucracy/Icon", false));
        }
        private void ActivateUi(string screen)
        {
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER) return;
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
                case "allocation":
                    allocationWindow = DrawBudgetAllocationUi();
                    break;
            }
        }

        private PopupDialog DrawBudgetAllocationUi()
        {
            padding = 0;
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            List<DialogGUIBase> innerElements = new List<DialogGUIBase>();
            DialogGUIBase[] horizontalArray = new DialogGUIBase[4];
            horizontalArray[0] = new DialogGUISpace(10);
            horizontalArray[1] = new DialogGUILabel("Budget", MessageStyle(true));
            horizontalArray[2] = new DialogGUISpace(70);
            horizontalArray[3] = new DialogGUITextInput(fundingAllocation.ToString(), false, 3, s => SetAllocation("Budget Manager", s), 40.0f, 30.0f);
            innerElements.Add(new DialogGUIHorizontalLayout(horizontalArray));
            horizontalArray = new DialogGUIBase[4];
            horizontalArray[0] = new DialogGUISpace(10);
            horizontalArray[1] = new DialogGUILabel("Construction", MessageStyle(true));
            horizontalArray[2] = new DialogGUISpace(10);
            horizontalArray[3] = new DialogGUITextInput(constructionAllocation.ToString(), false, 3, s => SetAllocation("Construction", s), 40.0f, 30.0f);
            innerElements.Add(new DialogGUIHorizontalLayout(horizontalArray));
            horizontalArray = new DialogGUIBase[4];
            horizontalArray[0] = new DialogGUISpace(10);
            horizontalArray[1] = new DialogGUILabel("Research", MessageStyle(true));
            horizontalArray[2] = new DialogGUISpace(45);
            horizontalArray[3] = new DialogGUITextInput(researchAllocation.ToString(), false, 3, s => SetAllocation("Research", s), 40.0f, 30.0f);
            innerElements.Add(new DialogGUIHorizontalLayout(horizontalArray));
            horizontalArray = new DialogGUIBase[2];
            horizontalArray[0] = new DialogGUISpace(10);
            horizontalArray[1] = new DialogGUIButton("Load Settings", () => SettingsClass.Instance.InGameLoad(), false); 
            innerElements.Add(new DialogGUIHorizontalLayout(horizontalArray));
            DialogGUIVerticalLayout vertical = new DialogGUIVerticalLayout(innerElements.ToArray());
            dialogElements.Add(new DialogGUIScrollList(-Vector2.one, false, false, vertical));
            dialogElements.Add(GetBoxes("allocation"));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("Bureaucracy", "", "Bureaucracy: Budget Allocation", UISkinManager.GetSkin("MainMenuSkin"),
                    GetRect(dialogElements), dialogElements.ToArray()), false, UISkinManager.GetSkin("MainMenuSkin"));
        }

        private string SetAllocation(string managerName, string passedString)
        {
            int.TryParse(passedString, out int i);
            float actualAllocation = i / 100.0f;
            Manager m = Utilities.Instance.GetManagerByName(managerName);
            m.FundingAllocation = actualAllocation;
            switch (managerName)
            {
                case "Budget Manager":
                    fundingAllocation = i;
                    break;
                case "Research":
                    researchAllocation = i;
                    break;
                case "Construction":
                    constructionAllocation = i;
                    break;
            }

            return passedString;
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
                innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel("Next Budget: " + Utilities.Instance.ConvertUtToKspTimeStamp(BudgetManager.Instance.NextBudget.CompletionTime), false)));
                innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel("Gross Budget: $" + Utilities.Instance.GetGrossBudget(), false)));
                innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel("Wage Costs: $" + Costs.Instance.GetWageCosts(), false)));
                innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel("Facility Maintenance Costs: $" + Costs.Instance.GetFacilityMaintenanceCosts(), false)));
                innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel("Mission Bonuses: $" + GetBonusesToPay(), false)));
                for (int i = 0; i < Bureaucracy.Instance.registeredManagers.Count; i++)
                {
                    Manager m = Bureaucracy.Instance.registeredManagers.ElementAt(i);
                    if (m.Name == "Budget") continue;
                    double departmentFunding = Math.Round(Utilities.Instance.GetNetBudget(m.Name), 0);
                    if (departmentFunding < 0.0f) continue;
                    innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel(m.Name + " Department Funding: $" + departmentFunding, false)));
                }
                innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel("Net Budget: $"+Utilities.Instance.GetNetBudget("Budget"), false)));
                DialogGUIVerticalLayout vertical = new DialogGUIVerticalLayout(innerElements.ToArray());
                dialogElements.Add(new DialogGUIScrollList(-Vector2.one, false, false, vertical));
                DialogGUIBase[] horizontal = new DialogGUIBase[6];
                horizontal[0] = new DialogGUILabel("Allocations: ");
                horizontal[1] = new DialogGUILabel("Funds: "+fundingAllocation+"%");
                horizontal[2] = new DialogGUILabel("|");
                horizontal[3] = new DialogGUILabel("Construction: "+constructionAllocation+"%");
                horizontal[4] = new DialogGUILabel("|");
                horizontal[5] = new DialogGUILabel("Research: "+researchAllocation+"%");
                dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
                dialogElements.Add(GetBoxes("main"));
            }
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("BureaucracyMain", "", "Bureaucracy: Budget", UISkinManager.GetSkin("MainMenuSkin"),
                    GetRect(dialogElements), dialogElements.ToArray()), false, UISkinManager.GetSkin("MainMenuSkin"));
        }

        private Rect GetRect(List<DialogGUIBase> dialogElements)
        {
            return new Rect(0.5f, 0.5f, 300, 265) {height = 150 + 50 * dialogElements.Count, width = Math.Max(padding, 280)};
        }

        private DialogGUIBase[] PaddedLabel(string stringToPad, bool largePrint)
        {
            DialogGUIBase[] paddedLayout = new DialogGUIBase[2];
            paddedLayout[0] = new DialogGUISpace(10);
            EvaluatePadding(stringToPad);
            paddedLayout[1] = new DialogGUILabel(stringToPad, MessageStyle(largePrint));
            return paddedLayout;
        }

        private void EvaluatePadding(string stringToEvaluate)
        {
            if (stringToEvaluate.Length *PadFactor > padding) padding = stringToEvaluate.Length * PadFactor;
        }

        private UIStyle MessageStyle(bool largePrint)
        {
            UIStyle style = new UIStyle
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerCenter,
                stretchWidth = false,
                normal = new UIStyleState
                {
                    textColor = new Color(0.89f, 0.86f, 0.72f)
                }
            };
            if (largePrint) style.fontSize = 23;
            return style;
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
            padding = 0;
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            List<DialogGUIBase> innerElements = new List<DialogGUIBase>();
            int upgradeCount = 0;
            innerElements.Add(new DialogGUISpace(10));
            float investmentNeeded = 0;
            for (int i = 0; i < FacilityManager.Instance.Facilities.Count; i++)
            {
                BureaucracyFacility bf = FacilityManager.Instance.Facilities.ElementAt(i);
                if (!bf.Upgrading) continue;
                upgradeCount++;
                investmentNeeded += bf.Upgrade.RemainingInvestment;
                float percentage = bf.Upgrade.OriginalCost - bf.Upgrade.RemainingInvestment;
                percentage = (float)Math.Round(percentage / bf.Upgrade.OriginalCost * 100,0);
                innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel(bf.Name + " "+percentage + "% ($" + bf.Upgrade.RemainingInvestment + " needed)", false)));
            }
            if (upgradeCount == 0) innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel("No Facility Upgrades in progress", false)));
            DialogGUIVerticalLayout vertical = new DialogGUIVerticalLayout(innerElements.ToArray());
            dialogElements.Add(new DialogGUIScrollList(new Vector2(300, 300), false, false, vertical));
            dialogElements.Add(new DialogGUILabel("Processing: $"+investmentNeeded));
            dialogElements.Add(GetBoxes("facility"));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog("FacilitiesDialog", "", "Bureaucracy: Facilities", UISkinManager.GetSkin("MainMenuSkin"), new Rect(0.5f, 0.5f, 320, 350), dialogElements.ToArray()), false, UISkinManager.GetSkin("MainMenuSkin"));
        }

        private PopupDialog DrawResearchUi()
        {
            padding = 0;
            float scienceCount = 0;
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            List<DialogGUIBase> innerElements = new List<DialogGUIBase>();
            innerElements.Add(new DialogGUISpace(10));
            if(ResearchManager.Instance.ProcessingScience.Count == 0) innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel("No research in progress", false)));
            for (int i = 0; i < ResearchManager.Instance.ProcessingScience.Count; i++)
            {
                ScienceEvent se = ResearchManager.Instance.ProcessingScience.ElementAt(i);
                scienceCount += se.RemainingScience;
                innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel(se.UiName+": "+Math.Round(se.OriginalScience-se.RemainingScience, 1)+"/"+Math.Round(se.OriginalScience, 1), false)));
            }
            DialogGUIVerticalLayout vertical = new DialogGUIVerticalLayout(innerElements.ToArray());
            dialogElements.Add(new DialogGUIScrollList(-Vector2.one, false, false, vertical));
            DialogGUIBase[] horizontal = new DialogGUIBase[3];
            horizontal[0] = new DialogGUILabel("Processing Science: " + Math.Round(scienceCount, 1));
            horizontal[1] = new DialogGUILabel("|");
            double scienceOutput = ResearchManager.Instance.GetAllocatedFunding() / SettingsClass.Instance.ScienceMultiplier * ResearchManager.Instance.ScienceMultiplier;
            horizontal[2] = new DialogGUILabel("Research Output: "+Math.Round(scienceOutput, 1));
            dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            dialogElements.Add(GetBoxes("research"));
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
            if (passingUi != "allocation")
            {
                horizontal[arrayPointer] = new DialogGUIButton("Allocation", () => ActivateUi("allocation"));
                arrayPointer++;
            }
            horizontal[arrayPointer] = new DialogGUIButton("Close", ValidateAllocations, false);
            return new DialogGUIHorizontalLayout(280, 35, horizontal);
        }

        public void ValidateAllocations()
        {
            int allocations = fundingAllocation + constructionAllocation + researchAllocation;
            if (allocations != 100) errorWindow = AllocationErrorWindow();
            else DismissAllWindows();
        }

        private PopupDialog AllocationErrorWindow()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            dialogElements.Add(new DialogGUILabel("Allocations do not add up to 100%"));
            dialogElements.Add(new DialogGUIButton("OK", () => { }, true));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog("AllocationError", "", "Bureaucracy: Error", UISkinManager.GetSkin("MainMenuSkin"), new Rect(0.5f, 0.5f, 200,90), dialogElements.ToArray()), false, UISkinManager.GetSkin("MainMenuSkin"));
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

        public PopupDialog NoHireWindow()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            dialogElements.Add(new DialogGUILabel("Due to reduced staffing levels we are unable to take on any new kerbals at this time"));
            dialogElements.Add(new DialogGUIButton("OK", () => { }, true));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog("NoHire", "", "Can't Hire!", UISkinManager.GetSkin("MainMenuSkin"), new Rect(0.5f, 0.5f, 100, 200), dialogElements.ToArray()), false, UISkinManager.GetSkin("MainMenuSkin"));
        }
        
        public PopupDialog GeneralError(string error)
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            dialogElements.Add(new DialogGUILabel(error));
            dialogElements.Add(new DialogGUIButton("OK", () => { }, true));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog("GeneralErrorDialog", "", "Bureaucracy: Error", UISkinManager.GetSkin("MainMenuSkin"), new Rect(0.5f, 0.5f, 200,200), dialogElements.ToArray()), false, UISkinManager.GetSkin("MainMenuSkin"));
        }

        public void OnSave(ConfigNode cn)
        {
            ConfigNode uiNode = new ConfigNode("UI");
            uiNode.SetValue("FundingAllocation", fundingAllocation, true);
            uiNode.SetValue("ResearchAllocation", researchAllocation, true);
            uiNode.SetValue("ConstructionAllocation", constructionAllocation, true);
            cn.AddNode(uiNode);
        }

        public void OnLoad(ConfigNode cn)
        {
            ConfigNode uiNode = cn.GetNode("UI");
            if (uiNode == null) return;
            int.TryParse(uiNode.GetValue("FundingAllocation"), out fundingAllocation);
            SetAllocation("Budget Manager", fundingAllocation.ToString());
            int.TryParse(uiNode.GetValue("ResearchAllocation"), out researchAllocation);
            SetAllocation("Research", researchAllocation.ToString());
            int.TryParse(uiNode.GetValue("ConstructionAllocation"), out constructionAllocation);
            SetAllocation("Construction", constructionAllocation.ToString());
        }

        public PopupDialog NoLaunchesWindow()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            dialogElements.Add(new DialogGUILabel("Due to reduced funding levels, we were unable to afford any fuel"));
            dialogElements.Add(new DialogGUISpace(20));
            dialogElements.Add(new DialogGUILabel("No fuel will be available until the end of the month."));
            dialogElements.Add(new DialogGUIButton("OK", () => { }, true));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog("NoFuel", "", "No Fuel Available!", UISkinManager.GetSkin("MainMenuSkin"), new Rect(0.5f, 0.5f, 200,160), dialogElements.ToArray()), false, UISkinManager.GetSkin("MainMenuSkin"));
        }

        public PopupDialog KctError()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            dialogElements.Add(new DialogGUILabel("It looks like you have Kerbal Construction Time installed. You should not use KCT's Facility Upgrade and Bureaucracy's Facility Upgrade at the same time. Bad things will happen."));
            dialogElements.Add(new DialogGUIToggle(() => SettingsClass.Instance.KctError, "Show this again", b => SettingsClass.Instance.KctError = b ));
            dialogElements.Add(new DialogGUIButton("OK", () => { }, true));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog("KCTError", "", "KCT Detected!", UISkinManager.GetSkin("MainMenuSkin"), new Rect(0.5f, 0.5f, 400,100), dialogElements.ToArray()), false, UISkinManager.GetSkin("MainMenuSkin"));
        }
    }
}