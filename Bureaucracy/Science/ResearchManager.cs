using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bureaucracy
{
    public class ResearchManager : Manager
    {
        public static ResearchManager Instance;
        public readonly List<ScienceEvent> ProcessingScience = new List<ScienceEvent>();
        public readonly List<ScienceEvent> CompletedEvents = new List<ScienceEvent>();

        public ResearchManager()
        {
            InternalListeners.OnBudgetAboutToFire.Add(RunResearchBudget);
            Name = "Research";
            Instance = this;
            Debug.Log("[Bureaucracy]: Research Manager Ready");
        }
        
        public override double GetAllocatedFunding()
        {
            return Math.Round(Utilities.Instance.GetNetBudget(Name), 0);
        }

        
        public override void UnregisterEvents()
        {
            InternalListeners.OnBudgetAboutToFire.Remove(RunResearchBudget);
        }

        private void RunResearchBudget()
        {
            double researchBudget = Utilities.Instance.GetNetBudget(Name);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
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

        public void NewScienceReceived(float science, ScienceSubject subject)
        {
            Debug.Log("[Bureaucracy]: New Science Logged"+subject.title+" for "+science+" science");
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
            Debug.Log("[Bureaucracy]: Research Manager OnLoad");
            ConfigNode researchNode = node.GetNode("RESEARCH");
            if (researchNode == null) return;
            ConfigNode[] scienceNodes = researchNode.GetNodes("SCIENCE_DATA");
            if (scienceNodes.Length == 0) return;
            for (int i = 0; i < scienceNodes.Length; i++)
            {
                ConfigNode cn = scienceNodes.ElementAt(i);
                bool.TryParse(cn.GetValue("isComplete"), out bool isComplete);
                ScienceEvent se = new ScienceEvent(cn, this);
                if(isComplete) CompletedEvents.Add(se);
                else ProcessingScience.Add(se);
            }
            Debug.Log("[Bureaucracy]: Research Manager OnLoad Complete");
        }

        public void OnSave(ConfigNode node)
        {
            Debug.Log("[Bureaucracy]: Research Manager OnSave");
            ConfigNode researchNode = new ConfigNode("RESEARCH");
            for (int i = 0; i < ProcessingScience.Count; i++)
            {
                ScienceEvent se = ProcessingScience.ElementAt(i);
                se.OnSave(researchNode);
            }

            node.AddNode(researchNode);
            Debug.Log("[Bureaucracy]: Research Manager OnSaveComplete");
        }
    }
}