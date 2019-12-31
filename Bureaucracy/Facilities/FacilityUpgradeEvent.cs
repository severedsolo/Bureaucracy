using System.Collections.Generic;
using System.Linq;
using Upgradeables;

namespace Bureaucracy
{
    public class FacilityUpgradeEvent : BureaucracyEvent
    {
        private string facilityId;
        private float remainingInvestment;
        private int levelRequested = 1;
        private float originalCost;
        private readonly BureaucracyFacility parentFacility;

        public FacilityUpgradeEvent(string id, BureaucracyFacility passingFacility)
        {
            facilityId = id;
            List<UpgradeableFacility> upgradeables = GetFacilityById(id);
            for (int i = 0; i < upgradeables.Count; i++)
            {
                UpgradeableFacility potentialUpgrade = upgradeables.ElementAt(i);
                if (potentialUpgrade.GetUpgradeCost() <= 0) continue;
                remainingInvestment = potentialUpgrade.GetUpgradeCost();
                originalCost = potentialUpgrade.GetUpgradeCost();
                levelRequested = potentialUpgrade.FacilityLevel + 1;
                break;
            }
            parentFacility = passingFacility;
        }
        
        public float RemainingInvestment => remainingInvestment;

        public float OriginalCost => originalCost;

        public float ProgressUpgrade(double funding)
        {
            double remainingFunding = funding - remainingInvestment;
            if (remainingFunding > 0)
            {
                OnEventCompleted();
                return  (float)remainingFunding;
            }
            remainingInvestment -= (float)funding;
            return 0.0f;
        }

        private List<UpgradeableFacility> GetFacilityById(string id)
        {
            return ScenarioUpgradeableFacilities.protoUpgradeables[id].facilityRefs;
        }
        public override void OnEventCompleted()
        {
            List<UpgradeableFacility> facilitiesToUpgrade = GetFacilityById(facilityId);
            for (int i = 0; i < facilitiesToUpgrade.Count; i++)
            {
                UpgradeableFacility facilityToUpgrade = facilitiesToUpgrade.ElementAt(i);
                facilityToUpgrade.SetLevel(levelRequested);
            }
            parentFacility.OnUpgradeCompleted();
        }

        public void OnSave(ConfigNode facilityNode)
        {
            ConfigNode upgradeNode = new ConfigNode("UPGRADE");
            upgradeNode.SetValue("ID", facilityId, true);
            upgradeNode.SetValue("cost", remainingInvestment, true);
            upgradeNode.SetValue("originalCost", originalCost, true);
            upgradeNode.SetValue("level", levelRequested, true);
            facilityNode.AddNode(upgradeNode);
        }

        public void OnLoad(ConfigNode upgradeNode)
        {
            facilityId = upgradeNode.GetValue("ID");
            float.TryParse(upgradeNode.GetValue("cost"), out remainingInvestment);
            int.TryParse(upgradeNode.GetValue("level"), out levelRequested);
            float.TryParse(upgradeNode.GetValue("originalCost"), out originalCost);
        }
    }
}