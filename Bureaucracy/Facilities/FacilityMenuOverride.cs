using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using KSP.UI.Screens;
using UnityEngine;
using UnityEngine.UI;
using Upgradeables;

namespace Bureaucracy
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
    [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeNullComparison")]
    public class FacilityMenuOverride : MonoBehaviour
    {
        public static FacilityMenuOverride Instance;
        private KSCFacilityContextMenu menuToOverride;
        private UpgradeableFacility overriddenFacility;

        private void Awake()
        { 
            Instance = this;
        }

        public void FacilityMenuSpawned(KSCFacilityContextMenu menu)
        {
            if (!SettingsClass.Instance.HandleKscUpgrades) return;
            menuToOverride = menu;
            StartCoroutine(HandleUpgradeButton());
        }
        

        private IEnumerator HandleUpgradeButton()
        {
            //borrowed from Magico13's KCT :)
            yield return new WaitForFixedUpdate();
            SpaceCenterBuilding hostBuilding = GetMember<SpaceCenterBuilding>("host");
            overriddenFacility = hostBuilding.Facility;
            Debug.Log("Trying to override upgrade button of menu for " + hostBuilding.facilityName);
            Button button = GetMember<Button>("UpgradeButton");
            if (button == null)
            {
                Debug.Log("Could not find UpgradeButton by name, using index instead.");
                button = GetMember<Button>(2);
            }
            if (button != null)
            {
                Debug.Log("Found upgrade button, overriding it.");
                button.onClick = new Button.ButtonClickedEvent(); //Clear existing KSP listener
                button.onClick.AddListener(HandleUpgrade);
            }
            else
            {
                throw new Exception("UpgradeButton not found. Cannot override.");
            }
        }

        private void HandleUpgrade()
        {
            FacilityManager.Instance.StartUpgrade(overriddenFacility);
        }

        [SuppressMessage("ReSharper", "UseCollectionCountProperty")]
        [SuppressMessage("ReSharper", "RedundantTypeSpecificationInDefaultExpression")]
        private T GetMember<T>(int index)
        {
            List<MemberInfo> memberList = menuToOverride.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).Where(m => m.ToString().Contains(typeof(T).ToString())).ToList();
            Debug.Log($"Found {memberList.Count()} matches for {typeof(T)}");
            MemberInfo member = memberList.Count() >= index ? memberList.ElementAt(index) : null;
            if (member == null)
            {
                Debug.Log($"Member was null when trying to find element at index {index} for type '{typeof(T)}'");
                return default(T);
            }
            object o = GetMemberInfoValue(member, menuToOverride);
            if (o is T)
            {
                return (T)o;
            }
            return default(T);
        }
        
        [SuppressMessage("ReSharper", "RedundantTypeSpecificationInDefaultExpression")]
        private T GetMember<T>(string memberName)
        {
            
            MemberInfo member = menuToOverride.GetType().GetMember(memberName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).FirstOrDefault();
            if (member == null)
            {
                Debug.Log($"Member was null when trying to find '{name}'");
                return default(T);
            }
            object o = GetMemberInfoValue(member, menuToOverride);
            if (o is T)
            {
                return (T)o;
            }
            return default(T);
        }
        private static object GetMemberInfoValue(MemberInfo member, object sourceObject)
        {
            object newVal;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (member is FieldInfo)
                newVal = ((FieldInfo)member).GetValue(sourceObject);
            else
                newVal = ((PropertyInfo)member).GetValue(sourceObject, null);
            return newVal;
        }
    }
}