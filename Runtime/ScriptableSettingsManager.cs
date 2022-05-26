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
    private  List<ScriptableSettingsTag> tags = new List<ScriptableSettingsTag>();
    public List<ScriptableSettingsTag> Tags => tags;

    public bool removeSettingsFromNames = false;
    public bool removeManagerFromNames = false;

    [SerializeField, ListDrawerSettings(HideAddButton = true)]
    private List<ScriptableSettings> scriptableSettings = new List<ScriptableSettings>();

    public List<ScriptableSettings> ScriptableSettings => scriptableSettings;

    private Dictionary<string, ScriptableSettings> _index;
    public Dictionary<string, ScriptableSettings> Index
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
        _index = new Dictionary<string, ScriptableSettings>();
        foreach (ScriptableSettings item in scriptableSettings)
            _index.Add(GetKey(item.GetType()), item);
    }
    
    public static string GetKey(Type type) => type.FullName;
    public static T Get<T>() where T : ScriptableSettings
    {
        string key = GetKey(typeof(T));
        //Debug.Log("Searching for key: " + key);
        
        if (!Instance.Index.ContainsKey(key))
            Instance.InitializeIndex();
        
        return Instance.Index[key] as T;
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
        return (tag == null)?CreateTag(tagName): tag;
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
        scriptableSettings ??= new List<ScriptableSettings>();
        
        scriptableSettings.Clear();

        IEnumerable<Type> types = GetAllSubclassTypes<ScriptableSettings>();
        
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");

        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Settings"))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Settings");

        string folderParent = "Assets/ScriptableObjects/Settings";
        
        foreach (Type item in types)
        {
            string key = GetKey(item);
            
            string path = $"{folderParent}/{key}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(folderParent, key);
            
            string currentPath = $"{path}/{key}.asset";
            string localPath = $"{key}";
            UnityEngine.Object uObject = Resources.Load(localPath, item);
            if (uObject == null)
            {
                uObject = CreateInstance(item);
                AssetDatabase.CreateAsset(uObject, $"{currentPath}");
            }
            scriptableSettings.Add(uObject as ScriptableSettings);
        }
        AssetDatabase.SaveAssets();
        scriptableSettings.Sort(SortByName);
    }

    private int SortByName(ScriptableSettings x, ScriptableSettings y)=> string.Compare(x.name, y.name, StringComparison.Ordinal);

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
        DestroyImmediate(tag,true);
        AssetDatabase.SaveAssets();    
    }
#endif

  
}