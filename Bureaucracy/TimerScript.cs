using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class TimerScriptFlight : TimerScript
    {
        
    }
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class TimerScriptSpaceCentre : TimerScript
    {
        
    }
    
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class TimerScriptTrackingStation : TimerScript
    {
        
    }

    public class TimerScript : MonoBehaviour
    {
        Dictionary<BureaucracyEvent, double> events = new Dictionary<BureaucracyEvent, double>();
        public static TimerScript Instance;
        private List<KeyValuePair<BureaucracyEvent, double>> eventCache;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            InvokeRepeating(nameof(CheckTimers), 0.1f, 0.1f);
        }

        public void AddTimer(BureaucracyEvent eventToAdd)
        {
            events.Add(eventToAdd, eventToAdd.CompletionTime);
        }

        public void RemoveTimer(BureaucracyEvent eventToRemove)
        {
            events.Remove(eventToRemove);
        }

        private void CheckTimers()
        {
            //TODO: Make some kind of gradual timewarp stop
            double time = Planetarium.GetUniversalTime();
            eventCache = events.ToList();
            foreach (var v in eventCache)
            {
                if(v.Value > time) continue;
                v.Key.OnEventCompleted();
                events.Remove(v.Key);
                if (SettingsClass.Instance.StopTimeWarp) TimeWarp.SetRate(0, true);
            }
        }
    }
}