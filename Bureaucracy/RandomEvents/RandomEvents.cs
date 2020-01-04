using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = System.Object;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class RandomEvents : MonoBehaviour
    {
        private List<RandomEventBase> events = new List<RandomEventBase>();

        private void Start()
        {
            if (!SettingsClass.Instance.RandomEventsEnabled) return;
            events.Add(new AliensEvent());
            events.Add(new PublicInterestEvent());
            events.Add(new FireEvent());
            events.Add(new MunchiesEvent());
            events.Add(new QAEvent());
            events.Add(new QAReversalEvent());
            double d = Utilities.Instance.Randomise.NextDouble();
            Debug.Log("[Bureaucracy]: Checking for Random event - rolled "+d);
            if (d > SettingsClass.Instance.RandomEventChance) return;
            RandomEventBase r = events.ElementAt(Utilities.Instance.Randomise.Next(0, events.Count));
            if (!r.EventIsValid()) return;
            r.OnEventGenerated();
        }
    }
}