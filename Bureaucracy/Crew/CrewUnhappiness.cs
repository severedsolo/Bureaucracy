using UnityEngine;

namespace Bureaucracy
{
    public class CrewUnhappiness
    {
        private int expiry = SettingsClass.Instance.StrikeMemory;
        private readonly CrewMember parentCrew;

        public string Reason { get; private set; }

        public CrewUnhappiness(string reason, CrewMember passingCrewMember)
        {
            Reason = reason;
            parentCrew = passingCrewMember;
        }

        public bool ClearStrike()
        {
            parentCrew.Unhappy = false;
            expiry--;
            if (expiry <= 0)
            {
                Debug.Log("Bureaucracy]: "+parentCrew.Name+" unhappiness for "+Reason+" expired");
                return true;
            }

            return false;
        }

        public void OnSave(ConfigNode crewNode)
        {
            ConfigNode unhappyNode = new ConfigNode("UNHAPPINESS");
            unhappyNode.SetValue("expiry", expiry, true);
            unhappyNode.SetValue("reason", Reason, true);
            crewNode.AddNode(unhappyNode);
        }


        public void OnLoad(ConfigNode unhappyNode)
        {
            Reason = unhappyNode.GetValue("reason");
            int.TryParse(unhappyNode.GetValue("expiry"), out expiry);
        }
    }
}