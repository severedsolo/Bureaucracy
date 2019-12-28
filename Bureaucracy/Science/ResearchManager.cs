using System.Collections.Generic;
using System.Linq;
using FinePrint;
using UnityEngine;

namespace Bureaucracy
{
    public class ResearchManager : Manager
    {
        public static ResearchManager Instance;
        public List<ScienceEvent> ProcessingScience = new List<ScienceEvent>();
        public List<ScienceEvent> CompletedEvents = new List<ScienceEvent>();

        public ResearchManager()
        {
            InternalListeners.OnBudgetAboutToFire.Add(RunResearchBudget);
            Name = "Research Manager";
            Instance = this;
        }
        
        public override void UnregisterEvents()
        {
            InternalListeners.OnBudgetAboutToFire.Remove(RunResearchBudget);
        }

        private void RunResearchBudget()
        {
            double researchBudget = Utilities.Instance.GetNetBudget("Research");
            if (researchBudget == 0.0f) return;
            for (int i = 0; i < ProcessingScience.Count; i++)
            {
                ScienceEvent se = ProcessingScience.ElementAt(i);
                researchBudget = se.ProgressResearch(researchBudget);
                if (researchBudget <= 0.0f) return;
            }
        }

        protected override Report GetReport()
        {
            return new ScienceReport();
        }

        public void NewScienceReceived(float science, ScienceSubject subject, ProtoVessel protoVessel, bool reverseEngineered)
        {
            ResearchAndDevelopment.Instance.AddScience(-science, TransactionReasons.ScienceTransmission);
            ProcessingScience.Add(new ScienceEvent(science, subject, this));
        }

        public override void OnEventCompletedManagerActions(BureaucracyEvent eventCompleted)
        {
            ProcessingScience.Remove(eventCompleted as ScienceEvent);
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
                else ProcessingScience.Add(se);
            }
        }

        public void OnSave(ConfigNode node)
        {
            ConfigNode researchNode = new ConfigNode("RESEARCH");
            for (int i = 0; i < ProcessingScience.Count; i++)
            {
                ScienceEvent se = ProcessingScience.ElementAt(i);
                se.OnSave(researchNode);
            }

            node.AddNode(researchNode);
        }
    }
}