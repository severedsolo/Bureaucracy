using System.Collections.Generic;
using UniLinq;

namespace Bureaucracy
{
    public class CrewReport : Report
    {
        public CrewReport()
        {
            ReportTitle = "Crew Report";
        }
        
        public override string ReportBody()
        {
            ReportBuilder.Clear();
            Dictionary<CrewMember, string> unhappyCrew = CrewManager.Instance.UnhappyCrewOutcomes;
            if (unhappyCrew.Count == 0) return "No Crew Issues";
            for (int i = 0; i < unhappyCrew.Count; i++)
            {
                KeyValuePair<CrewMember, string> unhappyCrewMember = unhappyCrew.ElementAt(i);
                ReportBuilder.AppendLine(unhappyCrewMember.Key.Name + ": " + unhappyCrewMember.Value);
            }
            CrewManager.Instance.ProcessQuitters();
            for (int i = 0; i < CrewManager.Instance.Retirees.Count; i++)
            {
                string s = CrewManager.Instance.Retirees.ElementAt(i);
                ReportBuilder.AppendLine(s + " has retired");
            }
            return ReportBuilder.ToString();
        }
    }
}