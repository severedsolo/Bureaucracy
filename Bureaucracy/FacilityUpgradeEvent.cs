using System.Collections.Generic;
using System.Linq;
using Upgradeables;

namespace Bureaucracy
{
    public class FacilityUpgradeEvent : BureaucracyEvent
    {
        private UpgradeableFacility facilityToUpgrade;
        private float cost;
        private int levelRequested;
        private BureaucracyFacility parentFacility;

        public FacilityUpgradeEvent(UpgradeableFacility facility, int requestedLevel, BureaucracyFacility passingFacility)
        {
            facilityToUpgrade = facility;
            levelRequested = requestedLevel;
            cost = facility.GetUpgradeCost();
            parentFacility = passingFacility;
        }

        public FacilityUpgradeEvent(List<UpgradeableFacility> facilityRefs, ConfigNode cn, string facilityName, BureaucracyFacility passingFacility)
        {
            parentFacility = passingFacility;
            for (int i = 0; i < facilityRefs.Count; i++)
            {
                UpgradeableFacility f = facilityRefs.ElementAt(i);
                if(f.id != facilityName) continue;
                facilityToUpgrade = f;
                int.TryParse(cn.GetValue("level"), out levelRequested);
                float.TryParse(cn.GetValue("cost"), out cost);
                return;
            }
        }

        public float Cost => cost;

        public float UpdateProgress(double funding)
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
            facilityToUpgrade.SetLevel(levelRequested);
            parentFacility.OnUpgradeCompleted();
        }

        public void OnSave(ConfigNode facilityNode)
        {
            ConfigNode upgradeNode = new ConfigNode("UPGRADE");
            upgradeNode.SetValue("cost", cost, true);
            upgradeNode.SetValue("level", levelRequested, true);
            facilityNode.AddNode(upgradeNode);
        }
    }
}