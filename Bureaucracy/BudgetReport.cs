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
            double netBudget = BudgetManager.Instance.GetNetBudget();
            ReportBuilder.AppendLine("Net Budget: " + Math.Max(0, netBudget));
            if(netBudget <0) ReportBuilder.AppendLine("The budget didn't fully cover your space programs costs. A penalty of $"+Math.Round(netBudget,0)+" will be applied");
            return ReportBuilder.ToString();
        }
    }
}