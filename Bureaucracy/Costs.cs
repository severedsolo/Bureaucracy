
using System.Linq;

namespace Bureaucracy
{
    public class Costs
    {
        private int launchCostsVab;
        private int launchCostsSph;
        public static Costs Instance;

        public Costs()
        {
            Instance = this;
        }

        public void AddLaunch(EditorFacility editor)
        {
            if (editor == EditorFacility.SPH) launchCostsSph += SettingsManager.Instance.launchCostSPH;
            else launchCostsVab += SettingsManager.Instance.launchCostVAB;
        }

        public double GetMaintenanceCosts()
        {
            double costs = 0;
            costs += GetFacilityMaintenanceCosts();
            costs += GetWageCosts();
            costs += GetLaunchCosts();
            return costs;
        }

        private double GetLaunchCosts()
        {
            return launchCostsSph + launchCostsVab;
        }

        private double GetWageCosts()
        {
            throw new System.NotImplementedException();
        }

        private double GetFacilityMaintenanceCosts()
        {
            double d = 0;
            for (int i = 0; i < FacilityManager.Instance.Facilities.Count; i++)
            {
                BureaucracyFacility bf = FacilityManager.Instance.Facilities.ElementAt(i);
                d += bf.MaintenanceCost;
            }

            return d;
        }

        public void OnLoad(ConfigNode cn)
        {
            ConfigNode costsNode = cn.GetNode("COSTS");
            int.TryParse(costsNode.GetValue("launchCostsVAB"), out launchCostsVab);
            int.TryParse(costsNode.GetValue("launchCostsSPH"), out launchCostsSph);
        }

        public void OnSave(ConfigNode cn)
        {
            ConfigNode costsNode = new ConfigNode("COSTS");
            costsNode.SetValue("launchCostsVAB", launchCostsVab, true);
            costsNode.SetValue("launchCostsSPH", launchCostsSph, true);
            cn.AddNode(costsNode);
        }
    }
}