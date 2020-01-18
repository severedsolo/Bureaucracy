using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class RandomEventLoader : MonoBehaviour
    {
        List<RandomEventBase> loadedEvents = new List<RandomEventBase>();
        private double cooldownTimer = 0;
        public static RandomEventLoader Instance;

        private void Awake()
        {
            Instance = this;
        }

        private void RollEvent()
        {
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER) return;
            if (!SettingsClass.Instance.RandomEventsEnabled || Utilities.Instance.Randomise.NextDouble() > SettingsClass.Instance.RandomEventChance) return;
            if (cooldownTimer > Planetarium.GetUniversalTime()) return;
            LoadEvents();
            RandomEventBase e = loadedEvents.ElementAt(Utilities.Instance.Randomise.Next(0, loadedEvents.Count));
            Debug.Log("[Bureaucracy]: Attempting to Fire Event "+e.name);
            if (!e.EventCanFire()) return;
            Debug.Log("[Bureaucracy]: EventCanFire");
            e.OnEventFire();
            cooldownTimer = Planetarium.GetUniversalTime() + FlightGlobals.GetHomeBody().solarDayLength * 30;
        }

        private void LoadEvents()
        {
            ConfigNode[] eventCache = GameDatabase.Instance.GetConfigNodes("BUREAUCRACY_EVENT");
            for (int i = 0; i < eventCache.Length; i++)
            {
                ConfigNode eventNode = eventCache.ElementAt(i);
                RandomEventBase re;
                try
                {
                    switch (eventNode.GetValue("Type"))
                    {
                        case "Currency":
                            re = new CurrencyEvent(eventNode);
                            loadedEvents.Add(re);
                            break;
                        case "Training":
                            re = new TrainingEvent(eventNode);
                            loadedEvents.Add(re);
                            break;
                        case "QA":
                            re = new QaEvent(eventNode);
                            loadedEvents.Add(re);
                            break;
                        default:
                            throw new ArgumentException("[Bureaucracy]: Event "+eventNode.GetValue("Name")+" is not a valid type!");
                    }
                }
                catch
                {
                    continue;
                }
            }
            Debug.Log("[Bureaucracy]: Loaded "+loadedEvents.Count+" events");
        }

        public void OnSave(ConfigNode cn)
        {
            ConfigNode eventNode = new ConfigNode("EVENTS");
            eventNode.SetValue("cooldown", cooldownTimer, true);
            cn.AddNode(eventNode);
        }
        
        public void OnLoad(ConfigNode cn)
        {
            ConfigNode eventNode = cn.GetNode("EVENTS");
            if (eventNode == null) return;
            double.TryParse(eventNode.GetValue("cooldown"), out cooldownTimer);
            RollEvent();
        }
    }
}