using System.Collections.Generic;
using UnityEngine;

namespace Bureaucracy
{
    public class TimerScript : MonoBehaviour
    {
        Dictionary<double, BureaucracyEvent> events = new Dictionary<double, BureaucracyEvent>();
        public static TimerScript Instance;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            InvokeRepeating(nameof(CheckTimers), 0.5f, 0.5f);
        }

        public void AddTimer(BureaucracyEvent bureaucracyEvent)
        {
            events.Add(bureaucracyEvent.CompletionTime, bureaucracyEvent);
        }

        private void CheckTimers()
        {
            double time = Planetarium.GetUniversalTime();
            foreach (var v in events)
            {
                if(v.Key > time) continue;
                v.Value.OnEventCompleted();
            }
        }
    }
}