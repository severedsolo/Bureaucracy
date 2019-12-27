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
        
        public override void UnregisterEvents()
        {
            InternalEvents.OnBudgetAboutToFire.Remove(RunResearchBudget);
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
        
        public void OnLoad(ConfigNode node)
        {
            ConfigNode researchNode = node.GetNode("RESEARCH");
            if (researchNode == null) return;
            ConfigNode[] scienceNodes = researchNode.GetNodes("SCIENCE_DATA");
            if (scienceNodes.Length == 0) return;
            ScienceEvent se;
            for (int i = 0; i < scienceNodes.Length; i++)
            {
                bool isComplete;
                ConfigNode cn = scienceNodes.ElementAt(i);
                bool.TryParse(cn.GetValue("isComplete"), out isComplete);
                se = new ScienceEvent(cn, this);
                if(isComplete) CompletedEvents.Add(se);
                else processingScience.Add(se);
            }
        }

        public void OnSave(ConfigNode node)
        {
            ConfigNode researchNode = new ConfigNode("RESEARCH");
            for (int i = 0; i < processingScience.Count; i++)
            {
                ScienceEvent se = processingScience.ElementAt(i);
                se.OnSave(researchNode);
            }

            node.AddNode(researchNode);
        }
    }
}