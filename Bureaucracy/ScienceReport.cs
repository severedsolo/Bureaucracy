using System;
using System.Linq;

namespace Bureaucracy
{
    public class ScienceReport : Report
    {
        public ScienceReport()
        {
            ReportTitle = "Research Report";
        }

        public override string ReportBody()
        {
            ReportBuilder.Clear();
            for (int i = 0; i < ResearchManager.Instance.CompletedEvents.Count; i++)
            {
                ScienceEvent se = ResearchManager.Instance.CompletedEvents.ElementAt(i);
                float processedScience = se.OriginalScience - se.RemainingScience;
                ReportBuilder.AppendLine(se.UiName + ": " + Math.Round(processedScience, 0) + "/" + se.OriginalScience);
            }
            for (int i = 0; i < ResearchManager.Instance.processingScience.Count; i++)
            {
                ScienceEvent se = ResearchManager.Instance.processingScience.ElementAt(i);
                float processedScience = se.OriginalScience - se.RemainingScience;
                ReportBuilder.AppendLine(se.UiName + ": " + Math.Round(processedScience, 0) + "/" + se.OriginalScience);
            }
            return ReportBuilder.ToString();
        }
    }
}