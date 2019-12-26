using System.Collections.Generic;
using System.Linq;
using FinePrint;
using UnityEngine;

namespace Bureaucracy
{
    public class ResearchManager : Manager
    {
        public static ResearchManager Instance;
        public List<ScienceEvent> processingScience = new List<ScienceEvent>();
        public List<ScienceEvent> CompletedEvents = new List<ScienceEvent>();

        public ResearchManager()
        {
            InternalEvents.OnBudgetAboutToFire.Add(RunResearchBudget);
            Name = "Research Manager";
            Instance = this;
        }

        private void RunResearchBudget()
        {
            double facilityBudget = Utilities.Instance.GetNetBudget("Research");
            if (facilityBudget == 0.0f) return;
            for (int i = 0; i < processingScience.Count; i++)
            {
                ScienceEvent se = processingScience.ElementAt(i);
                facilityBudget = se.ProgressResearch(facilityBudget);
                if (facilityBudget <= 0.0f) return;
            }
        }

        public override Report GetReport()
        {
            return new ScienceReport();
        }

        public void NewScienceReceived(float science, ScienceSubject subject, ProtoVessel protoVessel, bool reverseEngineered)
        {
            ResearchAndDevelopment.Instance.AddScience(-science, TransactionReasons.ScienceTransmission);
            processingScience.Add(new ScienceEvent(science, subject, this));
        }

        public override void OnEventCompleted(BureaucracyEvent eventCompleted)
        {
            processingScience.Remove(eventCompleted as ScienceEvent);
            CompletedEvents.Add(eventCompleted as ScienceEvent);
        }
        
        //TODO: Write OnSave and OnLoad
    }
}