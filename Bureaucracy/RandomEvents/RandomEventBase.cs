using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expansions.Missions.Tests;
using KSPAchievements;
using UniLinq;
using UnityEngine;

namespace Bureaucracy
{
    public abstract class RandomEventBase
    {
        protected PopupDialog EventDialog;
        protected string name;
        protected string title;
        protected string body;
        protected string acceptString;
        protected string declineString;
        protected bool canBeDeclined = false;
        protected float eventEffect;
        protected string kerbalName;
        protected string bodyName;

        public abstract bool EventCanFire();

        protected void LoadConfig(ConfigNode cn)
        {
            if(!cn.TryGetValue("Name", ref name)) throw new ArgumentException("Event is missing a Name!");
            if(!cn.TryGetValue("Title", ref title)) throw new ArgumentException(name+" has no title!");
            if(!cn.TryGetValue("Body", ref body)) throw new ArgumentException(name+" has no body!");
            if(!cn.TryGetValue("AcceptButtonText", ref acceptString)) throw new ArgumentException(name + " missing AcceptButtonText");
            if(cn.TryGetValue("canBeDeclined", ref canBeDeclined) && !cn.TryGetValue("DeclineButtonText", ref declineString)) throw new ArgumentException(name + "Can be declined but DeclineButtonText not set");
            if(!cn.TryGetValue("Effect", ref eventEffect)) throw new ArgumentException(name+" has no Effect defined in cfg");
            ReplaceStrings();
        }

        private void ReplaceStrings()
        {
            bodyName = Utilities.Instance.GetARandomBody();
            kerbalName = Utilities.Instance.GetARandomKerbal();
            name = name.Replace("<kerbal>", kerbalName);
            name = name.Replace("<body>", bodyName);
            title = title.Replace("<kerbal>", kerbalName);
            title = title.Replace("<body>", bodyName);
            body = body.Replace("<kerbal>", kerbalName);
            body = body.Replace("<body>", bodyName);
            acceptString = acceptString.Replace("<kerbal>", kerbalName);
            acceptString = acceptString.Replace("<body>", bodyName);
            if (declineString != null) declineString = declineString.Replace("<kerbal>", kerbalName);
            if (declineString != null) declineString = declineString.Replace("<body>", bodyName);
        }

        protected abstract void OnEventAccepted();

        protected abstract void OnEventDeclined();

        public void OnEventFire()
        {
            List<DialogGUIBase> dialogElements = new List<DialogGUIBase>();
            List<DialogGUIBase> innerElements = new List<DialogGUIBase>();
            innerElements.Add(new DialogGUISpace(10));
            innerElements.Add(new DialogGUIHorizontalLayout(PaddedLabel(body)));
            DialogGUIVerticalLayout vertical = new DialogGUIVerticalLayout(innerElements.ToArray());
            dialogElements.Add(new DialogGUIScrollList(-Vector2.one, false, false, vertical));
            dialogElements.Add(new DialogGUIButton(acceptString, OnEventAccepted));
            if(canBeDeclined) dialogElements.Add(new DialogGUIButton(acceptString, OnEventDeclined));
            EventDialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new MultiOptionDialog("EventDialog", "", title, UISkinManager.GetSkin("MainMenuSkin"), new Rect(0.5f, 0.5f, 300, 200), dialogElements.ToArray()), false, UISkinManager.GetSkin("MainMenuSkin"));
        }
        
        protected DialogGUIBase[] PaddedLabel(string stringToPad)
        {
            DialogGUIBase[] paddedLayout = new DialogGUIBase[2];
            paddedLayout[0] = new DialogGUISpace(10);
            paddedLayout[1] = new DialogGUILabel(stringToPad);
            return paddedLayout;
        }

    }
}