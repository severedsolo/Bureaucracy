namespace Bureaucracy
{
    //Intended to be extended. Really only used for BudgetEvents at the moment, but the option is there if we want it.
    public class BureaucracyEvent
    {
        public double CompletionTime;
        //Events should Instantiate themselves from a Manager, so "InformParent" can tell them when it's finished.
        protected Manager ParentManager;
        // ReSharper disable once NotAccessedField.Global
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