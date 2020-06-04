using System;
using System.Reflection;
using UnityEngine;

namespace Bureaucracy
{
    public class ResearchBodiesWrapper
    {
        public bool RBInstalled()
        {
            Debug.Log("[Bureaucracy]: Checking For ResearchBodies");
            //Adapted from Kerbalism.Contracts
            //We don't actually need to interface with ResearchBodies, just know if it's installed so FacilityManager can handle the Observatory
            foreach (AssemblyLoader.LoadedAssembly a in AssemblyLoader.loadedAssemblies)
            {
                if (!a.name.Equals("ResearchBodies"))continue;
                Debug.Log("[Bureaucracy]: Found ResearchBodies");
                return true;
            }
            Debug.Log("[Bureaucracy]: Did not find ResearchBodies");
            return false;
        }
    }
}