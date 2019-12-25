using System;
using System.Linq;

namespace Bureaucracy
{
    public class FacilityReport : Report
    {
        public FacilityReport()
        {
            ReportTitle = "Facilities Report";
        }

        public override string ReportBody()
        {
            ReportBuilder.Clear();
            for (int i = 0; i < FacilityManager.Instance.Facilities.Count; i++)
            {
                BureaucracyFacility bf = FacilityManager.Instance.Facilities.ElementAt(i);
                string s = bf.GetProgressReport();
                if(s == String.Empty) continue;
                ReportBuilder.AppendLine(s);
            }

            return ReportBuilder.ToString();
        }
    }
}