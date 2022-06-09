using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class ScriptableSettingsEditor
{
    public const string Folder = "Assets/ScriptableObjects/Settings";
    public const string AddressableAssetsGroupName = "ScriptableSettings";

    private static List<BaseScriptableSettings> AllSettings;
    static ScriptableSettingsEditor()
    {
        Debug.Log("ScriptableSettingsEditor");
        var list = InstantiateMissing();
        AddToAddressableAssets(list);
        AllSettings = list;
    }

    [MenuItem("ScriptableSettings/InstantiateMissing")]
    public static void InstantiateMissingButton() => AllSettings = InstantiateMissing();
    
    public static List<BaseScriptableSettings> InstantiateMissing()
    {
        var baseScriptableSettingsList = new List<BaseScriptableSettings>();

        var types = new List<Type>(GetAllSubclassTypes<BaseScriptableSettings>());

        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");

        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Settings"))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Settings");

        foreach (Type item in types)
        {
            string key = GetKey(item);
            string localPath = $"{Folder}/NO USAR - {key}.asset";
            var baseScriptable = AssetDatabase.LoadMainAssetAtPath(localPath);
            if (baseScriptable == null)
            {
                baseScriptable = ScriptableObject.CreateInstance(item);
                AssetDatabase.CreateAsset(baseScriptable, $"{localPath}");
                ((BaseScriptableSettings)baseScriptable).InitializeMain();
            }

            var newSettings = (BaseScriptableSettings)baseScriptable;
            baseScriptableSettingsList.Add(newSettings);
        }

        AssetDatabase.SaveAssets();
        baseScriptableSettingsList.Sort(SortByName);
        return baseScriptableSettingsList;
    }

    public static List<BaseScriptableSettings> GetAllMains() => AllSettings;

    public static BaseScriptableSettings GetMain(Type targetType) => AllSettings.Find(x => x.GetType() == targetType);
    
    private static int SortByName(Object x, Object y) => string.Compare(x.name, y.name, StringComparison.Ordinal);

    private static string GetKey(Type item) => item.FullName;

    public static void AddToAddressableAssets(List<BaseScriptableSettings> scriptableSettingsBuckets)
    {
        var group = AddressableAssetSettingsDefaultObject.Settings.groups.Find(x => x.Name == AddressableAssetsGroupName);

        if (group)
        {
            List<AddressableAssetEntry> entries = new List<AddressableAssetEntry>(group.entries);
            foreach (AddressableAssetEntry addressableAssetEntry in entries)
                group.RemoveAssetEntry(addressableAssetEntry);
        }
        
        foreach (var bucket in scriptableSettingsBuckets)
        {
            AddToAddressableAssets(
                bucket,
                AddressableAssetsGroupName,
                "ScriptableSettings");
        }
    }
    
  
    
    private static IEnumerable<Type> GetAllSubclassTypes<T>()
    {
        return from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in assembly.GetTypes()
            where (type.IsSubclassOf(typeof(T)) && !type.IsAbstract)
            select type;
    }

    
    public static void AddToAddressableAssets(UnityEngine.Object asset, string groupName, params string[] labels)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;

        var currentLabels = new List<string>(settings.GetLabels());
        foreach (string label in labels)
        {
            if(!currentLabels.Contains(label))
                AddressableAssetSettingsDefaultObject.Settings.AddLabel(label);
        }
        
        if (settings)
        {
            var group = settings.FindGroup(groupName);
            if (!group)
                group = settings.CreateGroup(groupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
 
            var assetPath = AssetDatabase.GetAssetPath(asset);
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
 
            var addressableAssetEntry = settings.CreateOrMoveEntry(guid, group, false, false);

            foreach (string label in labels)
                addressableAssetEntry.labels.Add(label);
            
            var entriesAdded = new List<AddressableAssetEntry> {addressableAssetEntry};
 
            group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true, false);
        }
    }

   
}