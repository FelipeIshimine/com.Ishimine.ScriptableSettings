using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(menuName = "GameSettings/Manager", fileName = "GameSettingsManager")]
public class ScriptableSettingsManager : RuntimeScriptableSingleton<ScriptableSettingsManager>
{
    [SerializeField]
#if UNITY_EDITOR
    [ListDrawerSettings(HideAddButton = true, CustomRemoveIndexFunction = nameof(RemoveTag))]
#endif
    private List<ScriptableSettingsTag> tags = new List<ScriptableSettingsTag>();

    public List<ScriptableSettingsTag> Tags => tags;

    public bool removeSettingsFromNames = false;
    public bool removeManagerFromNames = false;

    [SerializeField, ListDrawerSettings(HideAddButton = true)]
    private List<ScriptableSettingsBucket> buckets = new List<ScriptableSettingsBucket>();

    public List<ScriptableSettingsBucket> Buckets => buckets;

    private Dictionary<string, IReadOnlyList<ScriptableSettings>> _index;

    public Dictionary<string, IReadOnlyList<ScriptableSettings>> Index
    {
        get
        {
            if (_index == null) InitializeIndex();
            return _index;
        }
    }

    public static bool ShowRuntimeScriptableSingleton
    {
        get => Instance.showRuntimeScriptableSingleton;
        set => Instance.showRuntimeScriptableSingleton = value;
    }

    [SerializeField] private bool showRuntimeScriptableSingleton = true;

    private void InitializeIndex()
    {
        _index = new Dictionary<string, IReadOnlyList<ScriptableSettings>>();
        for (var index = buckets.Count - 1; index >= 0; index--)
        {
            var bucket = buckets[index];
            if (bucket.IsEmpty)
                buckets.RemoveAt(index);
            else
                _index.Add(GetKey(bucket.ContentType), bucket.GetValues(bucket.ContentType));
        }
    }

    public static string GetKey(Type type) => type.FullName;

    public static ScriptableSettings GetMainSettings(Type type) => GetSettings(0, type);

    public static T GetMainSettings<T>() where T : ScriptableSettings => GetSettings<T>(0);

    public static T GetSettings<T>(int index) where T : ScriptableSettings => GetSettings(index, typeof(T)) as T;

    public static IReadOnlyList<ScriptableSettings> GetAllSettings(Type type) => Instance.Index[GetKey(type)];

    public static IReadOnlyList<T> GetAllSettings<T>() where T : ScriptableSettings
    {
        var allSettings = GetAllSettings(typeof(T));
        List<T> list = new List<T>();
        foreach (ScriptableSettings allSetting in allSettings)
            list.Add(allSetting as T);
        return list;
    }

    public static int GetSettingsCount(Type type) => Instance.Index[GetKey(type)].Count;

    public static ScriptableSettings GetSettings(int index, Type type) 
    {
        string key = GetKey(type);

        /*if (!Instance.Index.ContainsKey(key))
            Instance.InitializeIndex();*/

        return Instance.Index[key][index];
    }


#if UNITY_EDITOR

    #region Static

    static ScriptableSettingsManager()
    {
        EditorApplication.delayCall += EditorInitialize;
    }

    private static void EditorInitialize()
    {
        EditorApplication.delayCall -= EditorInitialize;
        Instance.InstantiateMissingSettings();
    }

    #endregion



    [Button]
    public static ScriptableSettingsTag CreateNewTag(string nTagName)
    {
        return Instance.FindOrCreateTag(nTagName);
    }

    public void RemoveTag(int index)
    {
        var tag = tags[index];
        tags.RemoveAt(index);
        DestroyImmediate(tag, true);
        AssetDatabase.SaveAssets();
    }

    public ScriptableSettingsTag FindOrCreateTag(string tagName)
    {
        ScriptableSettingsTag tag = tags.Find(x => x.name == tagName);
        return (tag == null) ? CreateTag(tagName) : tag;
    }

    private ScriptableSettingsTag CreateTag(string tagName)
    {
        ScriptableSettingsTag nTag = CreateInstance<ScriptableSettingsTag>();
        nTag.name = tagName;
        AssetDatabase.AddObjectToAsset(nTag, this);
        tags.Add(nTag);
        return nTag;
    }

    public void InstantiateMissingSettings()
    {
        buckets ??= new List<ScriptableSettingsBucket>();

        buckets.Clear();

        IEnumerable<Type> types = GetAllSubclassTypes<ScriptableSettings>();

        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");

        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Settings"))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Settings");

        string folderParent = "Assets/ScriptableObjects/Settings";

        foreach (Type item in types)
        {
            string key = GetKey(item);

            /*
             string path = $"{folderParent}/{key}";
             
             if (!AssetDatabase.IsValidFolder(path))
                 AssetDatabase.CreateFolder(folderParent, key);
            
            string localPath = $"{key}.asset";
            string currentPath = $"{path}/{localPath}";
            
            */
            string localPath = $"{folderParent}/{key}.asset";
            ScriptableSettingsBucket bucket = AssetDatabase.LoadAssetAtPath<ScriptableSettingsBucket>(localPath);
            if (bucket == null)
            {
                bucket = CreateInstance<ScriptableSettingsBucket>();
                AssetDatabase.CreateAsset(bucket, $"{localPath}");
            }

            bucket.GetValues(item);
            buckets.Add(bucket);
        }

        AssetDatabase.SaveAssets();
        buckets.Sort(SortByName);
    }

    private int SortByName(ScriptableObject x, ScriptableObject y) =>
        string.Compare(x.name, y.name, StringComparison.Ordinal);

    private static IEnumerable<Type> GetAllSubclassTypes<T>()
    {
        return from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in assembly.GetTypes()
            where (type.IsSubclassOf(typeof(T)) && !type.IsAbstract)
            select type;
    }

    public static void DeleteTag(ScriptableSettingsTag tag)
    {
        Instance.tags.Remove(tag);
        DestroyImmediate(tag, true);
        AssetDatabase.SaveAssets();
    }
#endif

}
