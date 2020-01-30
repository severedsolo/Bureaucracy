using System;
using System.Collections.Generic;
using System.Linq;
using KSP.UI;
using UnityEngine;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class AstronautComplexOverride : MonoBehaviour
    {
        public bool astronautComplexSpawned;
        public static AstronautComplexOverride Instance;
        public int updateCount = 4;

        private void Awake()
        {
            Instance = this;
        }

        private void LateUpdate()
        {
            if (!astronautComplexSpawned || updateCount <= 0) return;
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            List<CrewListItem> crewItems = FindObjectsOfType<CrewListItem>().ToList();
            updateCount--;
            for (int i = 0; i < crewItems.Count; i++)
            {
                CrewListItem c = crewItems.ElementAt(i);
                if (c.GetCrewRef().type != ProtoCrewMember.KerbalType.Crew) continue;
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                c.SetLabel(GenerateAstronautString(c.GetCrewRef().name));
            }
        }

        private string GenerateAstronautString(string kerbalName)
        {
            CrewMember c = CrewManager.Instance.Kerbals[kerbalName];
            //if for whatever reason we can't find the CrewMember just leave it at default
            if (c == null) return "Available For Next Mission";
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            if (c.CrewReference().inactive) return "In Training | " + "Wage: " + c.Wage;
            float morale = (1-(float)c.UnhappinessEvents.Count / c.MaxStrikes)*100;
            if (float.IsNaN(morale)) morale = 100;
            if (float.IsNegativeInfinity(morale)) morale = 0;
            return "Morale: " + Math.Round(morale, 0)+"% | Wage: "+c.Wage;
        }
    }
}