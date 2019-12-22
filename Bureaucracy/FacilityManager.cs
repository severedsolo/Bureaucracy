using System;
using System.Collections.Generic;
using System.Linq;

namespace Bureaucracy
{
    public class FacilityManager : Manager
    {
        public List<BureaucracyFacility> Facilities = new List<BureaucracyFacility>();
        public static FacilityManager Instance;
        public FacilityManager()
        {
            SpaceCenterFacility[] spaceCentreFacilities = (SpaceCenterFacility[]) Enum.GetValues(typeof(SpaceCenterFacility));
            for (int i = 0; i < spaceCentreFacilities.Length; i++)
            {
                SpaceCenterFacility spf = spaceCentreFacilities.ElementAt(i);
                if(spf == SpaceCenterFacility.LaunchPad || spf == SpaceCenterFacility.Runway) continue;
                Facilities.Add(new BureaucracyFacility(spf));
            }
            Instance = this;
        }

        public void OnLoad(ConfigNode cn)
        {
            ConfigNode[] facilityNodes = cn.GetNodes("FACILITY");
            for (int i = 0; i < Facilities.Count; i++)
            {
                BureaucracyFacility bf = Facilities.ElementAt(i);
                bf.OnLoad(facilityNodes);
            }
        }

        public void OnSave(ConfigNode cn)
        {
            for (int i = 0; i < Facilities.Count; i++)
            {
                BureaucracyFacility bf = Facilities.ElementAt(i);
                bf.OnSave(cn);
            }
        }
    }
}