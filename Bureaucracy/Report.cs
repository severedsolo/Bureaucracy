using System.Reflection;
using System.Text;

namespace Bureaucracy
{
    public class Report
    {
        private string title;
        protected StringBuilder ReportBuilder = new StringBuilder();
        
        public string ReportTitle
        {
            get => title;
            protected set => title = value;
        }

        public virtual string ReportBody() { return "Report Not Found"; }
    }
}