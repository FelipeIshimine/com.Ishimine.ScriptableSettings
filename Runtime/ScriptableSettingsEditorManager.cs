using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "GameSettings/Manager", fileName = "GameSettingsManager")]
public class ScriptableSettingsEditorManager : RuntimeScriptableSingleton<ScriptableSettingsEditorManager>, ISerializationCallbackReceiver
{
    [SerializeField, ListDrawerSettings(HideAddButton = true)]
    private List<ScriptableSettingsBucket> buckets = new List<ScriptableSettingsBucket>();

    public List<ScriptableSettingsBucket> Buckets => buckets;

    private void OnEnable()
    {
        loadMode = AssetMode.EditorOnly;
    }

    public static string GetKey(Type type) => type.FullName;
    public static ScriptableSettingsBucket GetBucket<T>() where T : ScriptableObject => GetBucket(typeof(T));

    public static ScriptableSettingsBucket GetBucket(Type type)
    {
        for (var index = 0; index < Instance.buckets.Count; index++)
        {
            ScriptableSettingsBucket scriptableSettingsBucket = Instance.buckets[index];
          
            if (scriptableSettingsBucket.ContentType == type)
                return scriptableSettingsBucket;
        }
        return null;
    }

    #region Static

    static ScriptableSettingsEditorManager()
    {
        EditorApplication.delayCall += EditorInitialize;
    }

    private static void EditorInitialize()
    {
        EditorApplication.delayCall -= EditorInitialize;
        if(!Application.isPlaying)
            Instance.InstantiateMissingSettings();
    }

    #endregion

    [Button]
    public void InstantiateMissingSettings()
    {
        if (buckets != null)
        {
            for (int i = 0; i < buckets.Count; i++)
            {
                if(buckets[i].IsEmpty)
                    DestroyImmediate(buckets[i]);
            }
        }
        else
            buckets = new List<ScriptableSettingsBucket>();

        buckets.Clear();

        IEnumerable<Type> types = GetAllSubclassTypes<BaseScriptableSettings>();

        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");

        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Settings"))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Settings");

        string folderParent = "Assets/ScriptableObjects/Settings";

        foreach (Type item in types)
        {
            string key = GetKey(item);
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

    private int SortByName(ScriptableObject x, ScriptableObject y) => string.Compare(x.name, y.name, StringComparison.Ordinal);

    private static IEnumerable<Type> GetAllSubclassTypes<T>()
    {
        return from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in assembly.GetTypes()
            where (type.IsSubclassOf(typeof(T)) && !type.IsAbstract)
            select type;
    }

    public void OnBeforeSerialize()
    {
        loadMode = AssetMode.EditorOnly;
    }

    public void OnAfterDeserialize()
    {
    }
}

