using System.Linq;

namespace Bureaucracy
{
    public class ExplosionEvent : BureaucracyEvent
    {
        public ExplosionEvent()
        {
            Name = "QA Event";
            CompletionTime = Planetarium.GetUniversalTime() + Utilities.Instance.Randomise.Next(1, 120);
            AddTimer();
        }

        public override void OnEventCompleted()
        {
            Part p = FlightGlobals.ActiveVessel.parts.ElementAt(Utilities.Instance.Randomise.Next(0, FlightGlobals.ActiveVessel.parts.Count));
            p.explode();
            ScreenMessages.PostScreenMessage("[Bureaucracy]: Critical Failure!");
        }
    }
}