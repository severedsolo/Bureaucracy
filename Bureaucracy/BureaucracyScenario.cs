namespace Bureaucracy
{
    [KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames | ScenarioCreationOptions.AddToNewCareerGames, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
    public class BureaucracyScenario : ScenarioModule
    {
        public override void OnSave(ConfigNode node)
        {
            BudgetManager.Instance.OnSave(node);
            FacilityManager.Instance.OnSave(node);
            SettingsManager.Instance.OnSave(node);
        }

        public override void OnLoad(ConfigNode node)
        {
            BudgetManager.Instance.OnLoad(node);
            FacilityManager.Instance.OnLoad(node);
            SettingsManager.Instance.OnLoad(node);
        }
    }
}