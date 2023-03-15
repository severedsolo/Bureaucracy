using System;
using System.Reflection;
using UnityEngine;

namespace Bureaucracy
{
    internal static class KerbalismApi
    {
        private static Type kerbalismApi;
        private static FieldInfo addScienceBlocker;
        private static FieldInfo enableEvent;

        public static bool Available()
        {
            //As Bureaucracy should only run in Career mode just return false if not in Career.
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER) return false;
            Debug.Log("[Bureaucracy]: Attempting to find Kerbalism");
            //Borrowed from Kerbalism.Contracts
            foreach (AssemblyLoader.LoadedAssembly a in AssemblyLoader.loadedAssemblies)
            {
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
                    Debug.Log("[Bureuacracy] Found Kerbalism. Setting Field Instances");
                    addScienceBlocker = kerbalismApi.GetField("preventScienceCrediting", BindingFlags.Public | BindingFlags.Static);
                    enableEvent = kerbalismApi.GetField("subjectsReceivedEventEnabled", BindingFlags.Public | BindingFlags.Static);
                    return true;
                }
            }
            Debug.Log("[Bureaucracy]: Failed to find Kerbalism API");
            return false;
        }

        public static bool SuppressKerbalismScience()
        {
            if (addScienceBlocker == null || enableEvent == null) return false;
            addScienceBlocker.SetValue(null, true);
            enableEvent.SetValue(null, true);
            Debug.Log("[Bureaucracy]: Kerbalism Science Suppressed");
            return (bool) enableEvent.GetValue(kerbalismApi);
        }

        public static void UnsuppressKerbalismScience()
        {
            if (addScienceBlocker == null || enableEvent == null) return;
            addScienceBlocker.SetValue(null, false);
            enableEvent.SetValue(null, false);
            Debug.Log("[Bureaucracy]: Kerbalism Science Unsuppressed");
        }
    }
}