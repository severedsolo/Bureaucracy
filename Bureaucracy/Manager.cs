using System;
using System.Text;

namespace Bureaucracy
{
    public class Manager
    {
        protected StringBuilder reportBuilder = new StringBuilder();
        
        public virtual void OnEventCompleted() { }

        public virtual Report GetReport() { return new Report(); }


    }
}