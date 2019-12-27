using System;
using FinePrint;
using UnityEngine;
using VehiclePhysics;

namespace Bureaucracy
{
    public class ScienceEvent : BureaucracyEvent
    {
        private float originalScience;
        private float scienceLeftToProcess;
        private string scienceSubject;
        public string UiName;
        private bool isComplete = false;

        public float OriginalScience => originalScience;

        public float RemainingScience => scienceLeftToProcess;

        public ScienceEvent(float science, ScienceSubject subject, ResearchManager passingManager)
        {
            scienceLeftToProcess = science;
            originalScience = science;
            scienceSubject = subject.id;
            UiName = subject.title;
            ParentManager = passingManager;
            Debug.Log("[Bureaucracy]: Added new event: "+scienceSubject);
        }

        public ScienceEvent(ConfigNode dataNode, ResearchManager passingManager)
        {
            float.TryParse(dataNode.GetValue("originalScience"), out originalScience);
            float.TryParse(dataNode.GetValue("scienceLeftToProcess"), out scienceLeftToProcess);
            scienceSubject = dataNode.GetValue("scienceSubject");
            UiName = dataNode.GetValue("UiName");
            bool.TryParse(dataNode.GetValue("isComplete"), out isComplete);
            ParentManager = passingManager;
        }

        public double ProgressResearch(double funding)
        {
            //Potential TODO: Make research rate configurable?
            float scienceAvailable = (float)funding / SettingsClass.Instance.ScienceMultiplier;
            float originalScienceRemaining = scienceLeftToProcess;
            scienceLeftToProcess -= scienceAvailable;
            if (scienceLeftToProcess <= 0.0f)
            {
                scienceAvailable = scienceAvailable - originalScienceRemaining;
                ResearchAndDevelopment.Instance.AddScience(originalScienceRemaining, TransactionReasons.ScienceTransmission);
                Debug.Log("[Bureaucracy]: " + scienceSubject + " completed. Adding " + originalScienceRemaining + " science");
                OnEventCompleted();
                return scienceAvailable * 10000;
            }

            scienceAvailable = originalScienceRemaining - scienceLeftToProcess;
            Debug.Log("[Bureaucracy]: Adding "+scienceAvailable+" for "+scienceSubject);
            ResearchAndDevelopment.Instance.AddScience(originalScienceRemaining - scienceLeftToProcess, TransactionReasons.ScienceTransmission);
            return 0.0f;
        }

        public override void OnEventCompleted()
        {
            isComplete = true;
            InformParent();
        }

        public void OnSave(ConfigNode researchNode)
        {
            ConfigNode dataNode = new ConfigNode("SCIENCE_DATA");
            dataNode.SetValue("originalScience", originalScience, true);
            dataNode.SetValue("scienceLeftToProcess", scienceLeftToProcess, true);
            dataNode.SetValue("scienceSubject", scienceSubject, true);
            dataNode.SetValue("UiName", UiName, true);
            dataNode.SetValue("isComplete", isComplete, true);
            researchNode.AddNode(dataNode);
        }
    }
}