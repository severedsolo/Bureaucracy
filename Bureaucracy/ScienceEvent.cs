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

        public float OriginalScience => originalScience;

        public float RemainingScience => scienceLeftToProcess;

        public ScienceEvent(float science, ScienceSubject subject, ResearchManager passingManager)
        {
            scienceLeftToProcess = science;
            originalScience = science;
            scienceSubject = subject.id;
            //TODO: Parse the name properly
            UiName = subject.id;
            ParentManager = passingManager;
            Debug.Log("[Bureaucracy]: Added new event: "+scienceSubject);
        }

        public double ProgressResearch(double funding)
        {
            //Potential TODO: Make research rate configurable?
            float scienceAvailable = (float)funding / 10000;
            float originalScienceRemaining = scienceLeftToProcess;
            scienceLeftToProcess -= scienceAvailable;
            if (scienceAvailable <= 0.0f)
            {
                scienceAvailable = Math.Abs(scienceAvailable);
                ResearchAndDevelopment.Instance.AddScience(originalScienceRemaining, TransactionReasons.ScienceTransmission);
                Debug.Log("[Bureaucracy]: " + scienceSubject + " completed. Adding " + originalScienceRemaining + " science");
                //TODO: Research Event Completed event
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
            InformParent();
        }
        
        //TODO: Write OnSave and OnLoad
    }
}