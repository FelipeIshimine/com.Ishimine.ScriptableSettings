using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AddressableAssets;

[InitializeOnLoad]
public static class ScriptableSettingsEditor
{
    public static string Folder = "Assets/ScriptableObjects/Settings";
    public static string AddressableAssetsGroupName = "ScriptableSettings";

    static ScriptableSettingsEditor()
    {
        Debug.Log("ScriptableSettingsEditor");
        var list = InstantiateMissing();
        AddToAddressableAssets(list);
    }

    public static List<ScriptableSettingsBucket> InstantiateMissing()
    {
        var buckets = new List<ScriptableSettingsBucket>();

        var types = new List<Type>(GetAllSubclassTypes<BaseScriptableSettings>());

        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");

        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Settings"))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Settings");

        foreach (Type item in types)
        {
            string key = GetKey(item);
            string localPath = $"{Folder}/{key}.asset";
            ScriptableSettingsBucket bucket = AssetDatabase.LoadMainAssetAtPath(localPath) as ScriptableSettingsBucket;
            if (bucket == null)
            {
                bucket = ScriptableObject.CreateInstance<ScriptableSettingsBucket>();
                AssetDatabase.CreateAsset(bucket, $"{localPath}");
            }
            bucket.Initialize(item);
            buckets.Add(bucket);
        }

        AssetDatabase.SaveAssets();
        buckets.Sort(SortByName);
        return buckets;
    }

    private static int SortByName(ScriptableSettingsBucket x, ScriptableSettingsBucket y) => string.Compare(x.name, y.name, StringComparison.Ordinal);

    private static string GetKey(Type item) => item.FullName;

    public static void AddToAddressableAssets(List<ScriptableSettingsBucket> scriptableSettingsBuckets)
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
                bucket.GetLabel());
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