using System.Linq;

namespace Bureaucracy
{
    public class ManagerProgressEvent : BureaucracyEvent
    {
        public ManagerProgressEvent()
        {
            CompletionTime = Planetarium.GetUniversalTime() + FlightGlobals.GetHomeBody().solarDayLength;
            AddTimer();
        }

        public override void OnEventCompleted()
        {
            for (int i = 0; i < Bureaucracy.Instance.registeredManagers.Count; i++)
            {
                Manager m = Bureaucracy.Instance.registeredManagers.ElementAt(i);
                m.ProgressTask();
            }
            Bureaucracy.Instance.progressEvent = new ManagerProgressEvent();
            Bureaucracy.Instance.lastProgressUpdate = Planetarium.GetUniversalTime();
        }
    }
}