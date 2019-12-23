using System;
using UnityEngine;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class BureaucracyGameEvents : MonoBehaviour
    {
        public static EventData<double, double> OnBudgetAwarded;
        public static BureaucracyGameEvents Instance;
        private void Awake()
        {
            DontDestroyOnLoad(this);
            OnBudgetAwarded = new EventData<double, double>("OnBudgetAwarded");
        }
    }
}