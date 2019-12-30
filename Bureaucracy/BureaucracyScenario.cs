// ReSharper disable All
namespace Bureaucracy
{
    [KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames | ScenarioCreationOptions.AddToNewCareerGames, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
    public class BureaucracyScenario : ScenarioModule
    {
        public override void OnSave(ConfigNode node)
        {
            Bureaucracy.Instance.OnSave(node);
        }

        public override void OnLoad(ConfigNode node)
        {
            Bureaucracy.Instance.OnLoad(node);
        }
    }
}