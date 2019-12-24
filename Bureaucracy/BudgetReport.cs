using System;
using System.Text;

namespace Bureaucracy
{
    public class BudgetReport : Report
    {
        public BudgetReport()
        {
            ReportTitle = "Budget Report";
        }

        public override string ReportBody()
        {
            ReportBuilder.Clear();
            ReportBuilder.AppendLine("Gross Budget: " + BudgetManager.Instance.GetGrossBudget());
            ReportBuilder.AppendLine("Staff Costs: " + Costs.Instance.GetWageCosts());
            ReportBuilder.AppendLine("Facility Maintenance Costs: " + Math.Round(Costs.Instance.GetFacilityMaintenanceCosts()));
            ReportBuilder.AppendLine("Launch Costs: " + Costs.Instance.GetLaunchCosts());
            ReportBuilder.AppendLine("Total Maintenance Costs: " + Math.Round(Costs.Instance.GetTotalMaintenanceCosts()));
            ReportBuilder.AppendLine("Net Budget: " + BudgetManager.Instance.GetNetBudget());
            return ReportBuilder.ToString();
        }
    }
}