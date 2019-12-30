using System;

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
            ReportBuilder.AppendLine("Gross Budget: " + Utilities.Instance.GetGrossBudget());
            ReportBuilder.AppendLine("Staff Wages: " + Costs.Instance.GetWageCosts());
            ReportBuilder.AppendLine("Facility Maintenance Costs: " + Costs.Instance.GetFacilityMaintenanceCosts());
            ReportBuilder.AppendLine("Launch Costs: " + Costs.Instance.GetLaunchCosts());
            ReportBuilder.AppendLine("Total Maintenance Costs: " + Costs.Instance.GetTotalMaintenanceCosts());
            ReportBuilder.AppendLine("Mission Bonuses: " + CrewManager.Instance.LastBonus);
            ReportBuilder.AppendLine("Construction Department: " + FacilityManager.Instance.GetAllocatedFunding());
            ReportBuilder.AppendLine("Research Department: " + ResearchManager.Instance.GetAllocatedFunding());
            double netBudget = Utilities.Instance.GetNetBudget("Budget");
            ReportBuilder.AppendLine("Net Budget: " + Math.Max(0, netBudget));
            if (netBudget > 0 && netBudget < Funding.Instance.Funds) ReportBuilder.AppendLine("We can't justify extending your funding");
            // ReSharper disable once InvertIf
            if (netBudget < 0)
            {
                ReportBuilder.AppendLine("The budget didn't fully cover your space programs costs.");
                ReportBuilder.Append("A penalty of " + Math.Round(netBudget, 0) + " will be applied");
            }
            return ReportBuilder.ToString();
        }
    }
}