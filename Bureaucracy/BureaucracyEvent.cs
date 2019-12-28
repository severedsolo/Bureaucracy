namespace Bureaucracy
{
    public class BureaucracyEvent
    {
        public double CompletionTime;
        protected Manager ParentManager;
        protected string Name = "Uninitialised Event";

        protected void AddTimer()
        {
            TimerScript.Instance.AddTimer(this);
        }

        public virtual void OnEventCompleted() { }

        protected void InformParent()
        {
            ParentManager.OnEventCompletedManagerActions(this);
        }
    }
}