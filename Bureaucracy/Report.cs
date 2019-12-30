using System.Text;

namespace Bureaucracy
{
    public class Report
    {
        protected readonly StringBuilder ReportBuilder = new StringBuilder();

        public string ReportTitle { get; protected set; } = "Report Not Implemented";

        public virtual string ReportBody() { return "If you are seeing this message, you've forgotten to override GetReport() in a Manager class."; }
    }
}