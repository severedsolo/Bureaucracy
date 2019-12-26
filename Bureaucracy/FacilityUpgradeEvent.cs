using System.Collections.Generic;
using System.Linq;
using Upgradeables;

namespace Bureaucracy
{
    public class FacilityUpgradeEvent : BureaucracyEvent
    {
        public UpgradeableFacility FacilityToUpgrade;
        private float cost;
        private int levelRequested;
        private BureaucracyFacility parentFacility;

        public FacilityUpgradeEvent(UpgradeableFacility facility, BureaucracyFacility passingFacility)
        {
            FacilityToUpgrade = facility;
            levelRequested = facility.FacilityLevel + 1;
            cost = facility.GetUpgradeCost();
            parentFacility = passingFacility;
        }

        public float Cost => cost;

        public float ProgressUpgrade(double funding)
        {
            double remainingFunding = funding - cost;
            if (remainingFunding > 0)
            {
                OnEventCompleted();
                return  (float)remainingFunding;
            }
            cost -= (float)funding;
            return 0.0f;
        }

        public override void OnEventCompleted()
        {
            FacilityToUpgrade.SetLevel(levelRequested);
            parentFacility.OnUpgradeCompleted();
        }

        public void OnSave(ConfigNode facilityNode)
        {
            ConfigNode upgradeNode = new ConfigNode("UPGRADE");
            upgradeNode.SetValue("cost", cost, true);
            upgradeNode.SetValue("level", levelRequested, true);
            facilityNode.AddNode(upgradeNode);
        }

        public void OnLoad(ConfigNode upgradeNode)
        {
            float.TryParse(upgradeNode.GetValue("cost"), out cost);
            int.TryParse(upgradeNode.GetValue("level"), out levelRequested);
        }
    }
}