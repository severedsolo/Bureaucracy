using System;
using System.Reflection;
using UnityEngine;

namespace Bureaucracy
{
    internal class KerbalismApi
    {
        private static bool available;
        private static Type kerbalismApi;
        private FieldInfo addScienceBlocker;
        private FieldInfo enableEvent;

        private bool Available()
        {
            Debug.Log("[Bureaucracy]: Attempting to find Kerbalism");
            //Borrowed from Kerbalism.Contracts
            foreach (AssemblyLoader.LoadedAssembly a in AssemblyLoader.loadedAssemblies)
            {
                if (kerbalismApi != null && addScienceBlocker != null && enableEvent != null) return true;
                // name will be "Kerbalism" for debug builds,
                // and "Kerbalism18" or "Kerbalism16_17" for releases
                // there also is a KerbalismBootLoader, possibly a KerbalismContracts and other mods
                // that start with Kerbalism, so explicitly request equality or test for anything
                // that starts with Kerbalism1
                if (!a.name.Equals("Kerbalism") && !a.name.StartsWith("Kerbalism1", StringComparison.Ordinal)) continue;
                kerbalismApi = a.assembly.GetType("KERBALISM.API");
                Debug.Log("Found KERBALISM API in " + a.name + ": " + kerbalismApi);
                if (kerbalismApi != null)
                {
                    addScienceBlocker = kerbalismApi.GetField("preventScienceCrediting", BindingFlags.Public | BindingFlags.Static);
                    enableEvent = kerbalismApi.GetField("subjectsReceivedEventEnabled", BindingFlags.Public | BindingFlags.Static);
                }
                available = kerbalismApi != null;
                Debug.Log("[Bureaucracy]: Kerbalism found: " + available);
            }
            return available;
        }

        public bool ActivateKerbalismInterface()
        {
            if (!Available()) return false;
            if (addScienceBlocker == null || enableEvent == null) return false;
            addScienceBlocker.SetValue(null, true);
            enableEvent.SetValue(null, true);
            Debug.Log("[Bureaucracy]: Kerbalism Science Suppressed");
            return (bool) enableEvent.GetValue(kerbalismApi);
        }
    }
}