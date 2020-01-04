using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expansions.Missions.Tests;
using KSPAchievements;
using UnityEngine;

namespace Bureaucracy
{
    public abstract class RandomEventBase
    {
        protected PopupDialog EventDialog;
        
        protected abstract string AcceptedString();

        protected abstract string DeclinedString();

        protected abstract Rect WindowSize();
        public abstract bool EventIsValid();

        protected abstract string EventName();

        protected abstract string EventBody();
        
        public virtual void OnEventGenerated()
        {
            EventDialog = GenerateDialog();
        }

        protected string BuildMessage(string[] stringsToBuild)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < stringsToBuild.Length; i++)
            {
                sb.AppendLine(stringsToBuild.ElementAt(i));
            }

            return sb.ToString();
        }

        private PopupDialog GenerateDialog()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            dialogElements.Add(new DialogGUILabel(EventBody()));
            if (!EventCanBeDeclined()) dialogElements.Add(new DialogGUIButton(AcceptedString(), OnEventAccepted, true));
            else
            {
                DialogGUIBase[] horizontal = new DialogGUIBase[2];
                horizontal[0] = new DialogGUIButton(AcceptedString(), OnEventAccepted);
                horizontal[1] = new DialogGUIButton(DeclinedString(), OnEventDeclined);
                dialogElements.Add(new DialogGUIHorizontalLayout(horizontal));
            }

            return PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("BureaucracyEvent", "", EventName(), UISkinManager.GetSkin("MainMenuSkin"),
                    WindowSize(), dialogElements.ToArray()), false, UISkinManager.GetSkin("MainMenuSkin"));
        }

        protected abstract bool EventCanBeDeclined();

        protected abstract void OnEventAccepted();

        protected abstract void OnEventDeclined();

        protected virtual string GetBodyChoice()
        {
            return String.Empty;
        }
    }
}