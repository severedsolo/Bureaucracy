using System;
using System.Text;

namespace Bureaucracy
{
    public class Manager
    {
        public string Name = "Blank Manager";
        private float fundingAllocation = 0.3f;

        public int FundingAllocation
        {
            get => (int)(fundingAllocation * 100);
            set => fundingAllocation = value / 100.0f;
        }

        public virtual void OnEventCompleted(BureaucracyEvent eventCompleted) { }

        public virtual Report GetReport() { return new Report(); }


    }
}