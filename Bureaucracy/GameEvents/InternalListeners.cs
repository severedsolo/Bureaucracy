using UnityEngine;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
        public class InternalListeners : MonoBehaviour
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
