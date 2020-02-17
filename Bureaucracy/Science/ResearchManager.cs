using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bureaucracy
{
    public class ResearchManager : Manager
    {
        public static ResearchManager Instance;
        public readonly Dictionary<string, ScienceEvent> ProcessingScience = new Dictionary<string, ScienceEvent>();
        public readonly List<ScienceEvent> CompletedEvents = new List<ScienceEvent>();
        public float ScienceMultiplier = 1.0f;

        public ResearchManager()
        {
            Name = "Research";
            Instance = this;
            ThisMonthsBudget = HighLogic.CurrentGame.Parameters.Career.StartingFunds * FundingAllocation;
            Debug.Log("[Bureaucracy]: Research Manager Ready");
        }
        
        public override double GetAllocatedFunding()
        {
            return Math.Round(Utilities.Instance.GetNetBudget(Name), 0);
        }


        public override void ProgressTask()
        {
            double researchBudget = Utilities.Instance.ConvertMonthlyBudgetToDaily(ThisMonthsBudget) * ProgressTime() * ScienceMultiplier;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (researchBudget == 0.0f) return;
            List<ScienceEvent> scienceCache = ProcessingScience.Values.ToList();
            for (int i = 0; i < scienceCache.Count; i++)
            {
                ScienceEvent se = scienceCache.ElementAt(i);
                researchBudget = se.ProgressResearch(researchBudget);
                if (researchBudget <= 0.0f) break;
            }
            RemoveCompletedEvents();
        }

        protected override Report GetReport()
        {
            return new ScienceReport();
        }

        public void NewScienceReceived(float science, ScienceSubject subject)
        {
            Debug.Log("[Bureaucracy]: New Science Received "+subject.title+" for "+science+" science");
            if(ProcessingScience.TryGetValue(subject.id, out ScienceEvent se)) se.AddScience(science);
            else
            {
                ProcessingScience.Add(subject.id, new ScienceEvent(science, subject, this));
                Debug.Log("[Bureaucracy]: Registered new science event " + subject.title + " for " + science + " science");
            }
        }

        public override void OnEventCompletedManagerActions(BureaucracyEvent eventCompleted)
        {
            CompletedEvents.Add(eventCompleted as ScienceEvent);
        }
        
        public void OnLoad(ConfigNode node)
        {
            Debug.Log("[Bureaucracy]: Research Manager OnLoad");
            ConfigNode researchNode = node.GetNode("RESEARCH");
            if (researchNode == null) return;
            float.TryParse(researchNode.GetValue("ScienceMultiplier"), out ScienceMultiplier);
            float.TryParse(researchNode.GetValue("FundingAllocation"), out float funding);
            if(double.TryParse(researchNode.GetValue("thisMonth"), out double d)) ThisMonthsBudget = d;
            else ThisMonthsBudget = Utilities.Instance.GetNetBudget(Name);
            FundingAllocation = funding;
            ConfigNode[] scienceNodes = researchNode.GetNodes("SCIENCE_DATA");
            if (scienceNodes.Length == 0) return;
            for (int i = 0; i < scienceNodes.Length; i++)
            {
                ConfigNode cn = scienceNodes.ElementAt(i);
                bool.TryParse(cn.GetValue("isComplete"), out bool isComplete);
                ScienceEvent se = new ScienceEvent(cn, this);
                if(isComplete) CompletedEvents.Add(se);
                else ProcessingScience.Add(se.ScienceSubject, se);
            }
            Debug.Log("[Bureaucracy]: Research Manager OnLoad Complete");
        }

        public void OnSave(ConfigNode node)
        {
            Debug.Log("[Bureaucracy]: Research Manager OnSave");
            ConfigNode researchNode = new ConfigNode("RESEARCH");
            researchNode.SetValue("FundingAllocation", FundingAllocation, true);
            researchNode.SetValue("ScienceMultiplier", ScienceMultiplier, true);
            researchNode.SetValue("thisMonth", ThisMonthsBudget, true);
            for (int i = 0; i < ProcessingScience.Count; i++)
            {
                ScienceEvent se = ProcessingScience.ElementAt(i).Value;
                se.OnSave(researchNode);
            }

            node.AddNode(researchNode);
            Debug.Log("[Bureaucracy]: Research Manager OnSaveComplete");
        }

        private void RemoveCompletedEvents()
        {
            for (int i = 0; i < CompletedEvents.Count; i++)
            {
                ScienceEvent se = CompletedEvents.ElementAt(i);
                ProcessingScience.Remove(se.ScienceSubject);
            }
        }
    }
}