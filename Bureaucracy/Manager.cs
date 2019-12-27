using System;
using System.Text;
using KSP.UI.Screens;
using UnityEngine;

namespace Bureaucracy
{
    public class Manager
    {
        public string Name = "Blank Manager";
        private float fundingAllocation = 0.3f;
        public void MakeReport() 
        {                
            Report r = GetReport();
            MessageSystem.Message message = new MessageSystem.Message(r.ReportTitle, r.ReportBody(), MessageSystemButton.MessageButtonColor.BLUE, MessageSystemButton.ButtonIcons.MESSAGE);
            MessageSystem.Instance.AddMessage(message); 
        }
        public int FundingAllocation
        {
            get => (int)(fundingAllocation * 100);
            set => fundingAllocation = value / 100.0f;
        }

        public virtual void UnregisterEvents() { Debug.Log("[Bureaucracy]: No Events to Unregister for "+Name); }


        public virtual double GetAllocatedFunding() { return 0; }

        public virtual void OnEventCompleted(BureaucracyEvent eventCompleted) { }

        public virtual Report GetReport() { return new Report(); }
    }
}