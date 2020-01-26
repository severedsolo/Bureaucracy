using UnityEngine;

namespace Bureaucracy
{
    public class ScienceEvent : BureaucracyEvent
    {
        private readonly float originalScience;
        private float scienceLeftToProcess;
        private readonly string scienceSubject;
        public readonly string UiName;
        public bool IsComplete;

        public float OriginalScience => originalScience;

        public float RemainingScience => scienceLeftToProcess;

        // ReSharper disable once SuggestBaseTypeForParameter
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
            bool.TryParse(dataNode.GetValue("isComplete"), out IsComplete);
            ParentManager = passingManager;
        }

        public double ProgressResearch(double funding)
        {
            if (IsComplete) return funding;
            float scienceAvailable = (float)funding / SettingsClass.Instance.ScienceMultiplier;
            float originalScienceRemaining = scienceLeftToProcess;
            scienceLeftToProcess -= scienceAvailable;
            if (scienceLeftToProcess <= 0.0f)
            {
                scienceAvailable -= originalScienceRemaining;
                ResearchAndDevelopment.Instance.AddScience(originalScienceRemaining, TransactionReasons.ScienceTransmission);
                Debug.Log("[Bureaucracy]: " + scienceSubject + " completed. Adding " + originalScienceRemaining + " science");
                OnEventCompleted();
                return scienceAvailable * SettingsClass.Instance.ScienceMultiplier;
            }

            scienceAvailable = originalScienceRemaining - scienceLeftToProcess;
            Debug.Log("[Bureaucracy]: Adding "+scienceAvailable+" for "+scienceSubject);
            ResearchAndDevelopment.Instance.AddScience(originalScienceRemaining - scienceLeftToProcess, TransactionReasons.ScienceTransmission);
            return 0.0f;
        }

        public override void OnEventCompleted()
        {
            IsComplete = true;
            ScreenMessages.PostScreenMessage(UiName + ": Research Complete");
            Debug.Log("[Bureaucracy]: Science Event "+UiName+" completed");
            InformParent();
        }

        public void OnSave(ConfigNode researchNode)
        {
            ConfigNode dataNode = new ConfigNode("SCIENCE_DATA");
            dataNode.SetValue("originalScience", originalScience, true);
            dataNode.SetValue("scienceLeftToProcess", scienceLeftToProcess, true);
            dataNode.SetValue("scienceSubject", scienceSubject, true);
            dataNode.SetValue("UiName", UiName, true);
            dataNode.SetValue("isComplete", IsComplete, true);
            researchNode.AddNode(dataNode);
        }
    }
}