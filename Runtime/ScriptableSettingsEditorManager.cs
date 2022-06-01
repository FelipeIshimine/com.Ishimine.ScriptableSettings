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

    public void OnBeforeSerialize()
    {
        loadMode = AssetMode.EditorOnly;
    }

    public void OnAfterDeserialize()
    {
    }
}

