using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Upgradeables;

namespace Bureaucracy
{
    public class FacilityManager : Manager
    {
        public readonly List<BureaucracyFacility> Facilities = new List<BureaucracyFacility>();
        public static FacilityManager Instance;
        [UsedImplicitly] private PopupDialog warningDialog;

        public FacilityManager()
        {
            InternalListeners.OnBudgetAboutToFire.Add(RunFacilityBudget);
            SpaceCenterFacility[] spaceCentreFacilities = (SpaceCenterFacility[]) Enum.GetValues(typeof(SpaceCenterFacility));
            for (int i = 0; i < spaceCentreFacilities.Length; i++)
            {
                SpaceCenterFacility spf = spaceCentreFacilities.ElementAt(i);
                Facilities.Add(new BureaucracyFacility(spf));
            }

            Name = "Construction";
            Instance = this;
            Debug.Log("[Bureaucracy]: Facility Manager Ready");
        }

        public override void UnregisterEvents()
        {
            InternalListeners.OnBudgetAboutToFire.Remove(RunFacilityBudget);
        }

        public override double GetAllocatedFunding()
        {
            return Math.Round(Utilities.Instance.GetNetBudget(Name), 0);
        }

        protected override Report GetReport()
        {
            return new FacilityReport();
        }

        private void RunFacilityBudget()
        {
            ReopenAllFacilities();
            double facilityBudget = Utilities.Instance.GetNetBudget(Name);
            //Find the priority build first
            for (int i = 0; i < Facilities.Count; i++)
            {
                BureaucracyFacility bf = Facilities.ElementAt(i);
                if (!bf.Upgrading) continue;
                if (!bf.IsPriority) continue;
                facilityBudget = bf.Upgrade.ProgressUpgrade(facilityBudget);
                break;
            }
            if (facilityBudget <= 0.0f) return;
            //then run the others
            for (int i = 0; i < Facilities.Count; i++)
            {
                BureaucracyFacility bf = Facilities.ElementAt(i);
                if (!bf.Upgrading) continue;
                //Skip priority build as this should have been progressed first.
                if (bf.IsPriority) continue;
                facilityBudget = bf.Upgrade.ProgressUpgrade(facilityBudget);
                if (facilityBudget <= 0.0f) return;
            }
        }

        public void OnLoad(ConfigNode cn)
        {
            Debug.Log("[Bureaucracy]: FacilityManager OnLoad");
            ConfigNode managerNode = cn.GetNode("FACILITY_MANAGER");
            if (managerNode == null) return;
            int.TryParse(managerNode.GetValue("FundingAllocation"), out int funding);
            FundingAllocation = funding;
            ConfigNode[] facilityNodes = managerNode.GetNodes("FACILITY");
            for (int i = 0; i < Facilities.Count; i++)
            {
                BureaucracyFacility bf = Facilities.ElementAt(i);
                bf.OnLoad(facilityNodes);
            }

            Debug.Log("[Bureaucracy]: FacilityManager OnLoadComplete");
        }



        public void OnSave(ConfigNode cn)
        {
            Debug.Log("[Bureaucracy]: FacilityManager OnSave");
            ConfigNode managerNode = new ConfigNode("FACILITY_MANAGER");
            managerNode.SetValue("FundingAllocation", FundingAllocation, true);
            for (int i = 0; i < Facilities.Count; i++)
            {
                BureaucracyFacility bf = Facilities.ElementAt(i);
                bf.OnSave(managerNode);
            }

            cn.AddNode(managerNode);
            Debug.Log("[Bureaucracy]: FacilityManager OnSave Complete");
        }

        public void StartUpgrade(UpgradeableFacility facility)
        {
            BureaucracyFacility facilityToUpgrade = UpgradeableToActualFacility(facility);
            if (facilityToUpgrade == null)
            {
                Debug.Log("[Bureaucracy]: Upgrade of " + facility.id + " requested but no facility found");
                UiController.Instance.errorWindow = UiController.Instance.GeneralError("Can't find facility "+facility.id+" - please report this (with your KSP.log) on the Bureaucracy forum thread");
                return;
            }

            Debug.Log("[Bureaucracy]: Upgrade of " + facility.id + " requested");
            if (facilityToUpgrade.IsDestroyed())
            {
                Debug.Log("[Bureaucracy]: " + facility.id + " is destroyed. Aborting upgrade");
                ScreenMessages.PostScreenMessage("[Bureaucracy]: Can't upgrade " + facilityToUpgrade.Name + " Building is destroyed");
                return;
            }

            if (facilityToUpgrade.Upgrading)
            {
                if (facilityToUpgrade.IsPriority)
                {
                    warningDialog = DrawWarningDialog(facilityToUpgrade);
                }
                else
                {
                    Debug.Log("[Bureaucracy]: " + facility.id + " is already being upgraded. Prioritising");
                    SetPriority(facilityToUpgrade, true);
                    ScreenMessages.PostScreenMessage("Upgrade of " + facilityToUpgrade.Name + " prioritised");
                    return;
                }
            }

            facilityToUpgrade.StartUpgrade(facility);
        }

        private PopupDialog DrawWarningDialog(BureaucracyFacility facility)
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            dialogElements.Add(new DialogGUILabel("Upgrade of "+facility.Name+" will be cancelled. "+(facility.Upgrade.RemainingInvestment-facility.Upgrade.OriginalCost+" will be lost. Are you sure?")));
            dialogElements.Add(new DialogGUIButton("Yes", facility.CancelUpgrade, true));
            dialogElements.Add(new DialogGUIButton("No", () => { }, true));
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog("CancelUpgradeDialog", "", "Bureaucracy: Cancel Upgrade", UISkinManager.GetSkin("MainMenuSkin"), new Rect(0.5f, 0.5f, 210, 100), dialogElements.ToArray()), false, UISkinManager.GetSkin("MainMenuSkin"));
        }
        


        private BureaucracyFacility UpgradeableToActualFacility(UpgradeableFacility facility)
        {
            for (int i = 0; i < Facilities.Count; i++)
            {
                BureaucracyFacility bf = Facilities.ElementAt(i);
                if (!facility.id.Contains(bf.Name)) continue;
                return bf;
            }

            return null;
        }

        public BureaucracyFacility GetFacilityByName(string name)
        {
            for (int i = 0; i < Facilities.Count; i++)
            {
                BureaucracyFacility bf = Facilities.ElementAt(i);
                if (bf.Name == name) return bf;
            }

            return null;
        }

        private void ReopenAllFacilities()
        {
            for (int i = 0; i < Facilities.Count; i++)
            {
                BureaucracyFacility bf = Facilities.ElementAt(i);
                bf.ReopenFacility();
            }
        }

        private void SetPriority(BureaucracyFacility priorityFacility, bool b)
        {
            for (int i = 0; i < Facilities.Count; i++)
            {
                BureaucracyFacility bf = Facilities.ElementAt(i);
                if (bf != priorityFacility) bf.IsPriority = false;
                else bf.IsPriority = b;
            }
        }
    }
}