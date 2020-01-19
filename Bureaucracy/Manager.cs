using JetBrains.Annotations;
using KSP.UI.Screens;
using UnityEngine;

namespace Bureaucracy
{
    //Intended to be extended. Anything that extends a Manager class can register itself in Bureaucracy.cs
    public class Manager
    {
        //obviously you change these in the constructor.
        public string Name = "Blank Manager";
        public float FundingAllocation = 0.3f;

        public void MakeReport() 
        {                
            Report r = GetReport();
            MessageSystem.Message message = new MessageSystem.Message(r.ReportTitle, r.ReportBody(), MessageSystemButton.MessageButtonColor.BLUE, MessageSystemButton.ButtonIcons.MESSAGE);
            MessageSystem.Instance.AddMessage(message); 
        }

        public virtual void UnregisterEvents() { Debug.Log("[Bureaucracy]: No Events to Unregister for "+Name); }


        [UsedImplicitly]
        public virtual double GetAllocatedFunding() { return 0; }
        

        public virtual void OnEventCompletedManagerActions(BureaucracyEvent eventCompleted) { }

        protected virtual Report GetReport() { return new Report(); }
    }
}