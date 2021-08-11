using System;
using System.Collections.Generic;
using System.Linq;
using KSP.UI.Screens;
using UnityEngine;
using UnityEngine.UI;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]

    public class BankOfKerbinSc : BankOfKerbin
    {
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class BankOfKerbinFlight : BankOfKerbin
    {
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class BankOfKerbinEditor : BankOfKerbin
    {
    }

    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class BankOfKerbinTrackStation : BankOfKerbin
    {
    }

    public class BankOfKerbin : MonoBehaviour
    {
        private ApplicationLauncherButton toolbarButton;
        private PopupDialog dialogWindow;
        private double balance = 0;
        private int playerInput = 0;
        
        private void Start()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(AddToolbarButton);
            GameEvents.onGUIApplicationLauncherUnreadifying.Add(RemoveToolbarButton);
        }

        private void RemoveToolbarButton(GameScenes data)
        {
            if (toolbarButton == null) return;
            ApplicationLauncher.Instance.RemoveModApplication(toolbarButton);
        }

        private void AddToolbarButton()
        {
            //TODO: Get an Icon
            if(HighLogic.CurrentGame.Mode == Game.Modes.CAREER) toolbarButton = ApplicationLauncher.Instance.AddModApplication(ToggleUI, ToggleUI, null, null, null, null, ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.FLIGHT, GameDatabase.Instance.GetTexture("Bureaucracy/BankIcon", false));
        }
        
        private void ToggleUI()
        {
            if (dialogWindow == null) dialogWindow = DrawUI();
        }

        private PopupDialog DrawUI()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            List<DialogGUIBase> innerElements = new List<DialogGUIBase>();
            innerElements.Add(new DialogGUIImage(new Vector2(300, 147), new Vector2(0, 0), Color.gray, GameDatabase.Instance.GetTexture("Bureaucracy/Mortimer", false)));
            innerElements.Add(new DialogGUILabel(() => "Bank Balance: " + Math.Round(balance, 0)));
            innerElements.Add(new DialogGUITextInput(playerInput.ToString(), false, 30, s => SetPlayerInput(s), 300.0f, 30.0f));
            DialogGUIBase[] horizontal = new DialogGUIBase[3];
            horizontal[0] = new DialogGUIButton("Deposit", () => DepositFunds(playerInput), false);
            horizontal[1] = new DialogGUIButton("Withdraw", () => WithdrawFunds(playerInput), false);
            horizontal[2] = new DialogGUIButton("Close", null, true);
            innerElements.Add(new DialogGUIHorizontalLayout(horizontal));
            DialogGUIVerticalLayout vertical = new DialogGUIVerticalLayout(innerElements.ToArray());
            dialogElements.Add(vertical);
            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("Bureaucracy", "", "Bank of "+FlightGlobals.GetHomeBody().bodyName, UISkinManager.GetSkin("MainMenuSkin"),
                    new Rect(0.5f, 0.5f, 350, 265), dialogElements.ToArray()), false, UISkinManager.GetSkin("MainMenuSkin"), false);
        }

        private void WithdrawFunds(int playerInput)
        {
            double fundsToWithdraw = Math.Min(balance, playerInput);
            Funding.Instance.AddFunds(fundsToWithdraw, TransactionReasons.None);
            balance -= fundsToWithdraw;
        }

        private void DepositFunds(int playerInput)
        {
            if (!Funding.CanAfford(playerInput)) return;
            balance += playerInput;
            Funding.Instance.AddFunds(-playerInput, TransactionReasons.None);
        }

        private string SetPlayerInput(string s)
        {
            int.TryParse(s, out playerInput);
            return s;
        }

        private void OnDisable()
        {
            RemoveToolbarButton(HighLogic.LoadedScene);
        }
    }
}