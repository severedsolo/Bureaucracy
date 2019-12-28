using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
        class InternalListeners : MonoBehaviour
    {
        public static EventData<double, double> OnBudgetAwarded;
        public static EventVoid OnBudgetAboutToFire;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            OnBudgetAwarded = new EventData<double, double>("OnBudgetAwarded");
            OnBudgetAboutToFire = new EventVoid("OnBudgetAboutToFire");
        }
    }
}
