using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using KSP.UI;
using TMPro;
using UnityEngine;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class AstronautComplexOverride : MonoBehaviour
    {
        public bool AstronautComplexSpawned = false;
        public static AstronautComplexOverride Instance;
        private int updateCount = 4;

        private void Awake()
        {
            Instance = this;
        }

        private void LateUpdate()
        {
            if (!AstronautComplexSpawned) return;
            List<CrewListItem> crewItems = FindObjectsOfType<CrewListItem>().ToList();
            updateCount--;
            CrewListItem c;
            for (int i = 0; i < crewItems.Count; i++)
            {
                c = crewItems.ElementAt(i);
                if (c.GetCrewRef().type != ProtoCrewMember.KerbalType.Crew) continue;
                c.SetLabel(GenerateAstronautString(c.GetCrewRef().name));
            }
        }

        private string GenerateAstronautString(string name)
        {
            CrewMember c = CrewManager.Instance.Kerbals[name];
            //if for whatever reason we can't find the CrewMember just leave it at default
            if (c == null) return "Available For Next Mission";
            if (c.CrewReference().inactive) return "In Training | " + "Wage: " + c.Wage;
            StringBuilder sb = new StringBuilder();
            float morale = (1-(float)c.unhappinessEvents.Count / c.maxStrikes)*100;
            if (float.IsNaN(morale)) morale = 100;
            return "Morale: " + Math.Round(morale, 0)+"% | Wage: "+c.Wage;
        }
    }
}