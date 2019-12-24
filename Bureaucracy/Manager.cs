using System;
using System.Text;

namespace Bureaucracy
{
    public class Manager
    {
        protected StringBuilder reportBuilder = new StringBuilder();
        protected Report report;
        
        public virtual void OnEventCompleted() { }

        public virtual Report GetReport() { return report; }


    }
}