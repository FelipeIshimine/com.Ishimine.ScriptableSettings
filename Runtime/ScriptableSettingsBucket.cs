using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class ScriptableSettingsBucket : ScriptableObject
{
    [SerializeField, HideInInspector] private List<ScriptableSettings> values = new List<ScriptableSettings>();
    
    public bool IsEmpty => values.Count == 0;
    public Type ContentType => values.Count == 0 ? null : values[0].GetType();

#if UNITY_EDITOR

 

    private List<Options> _settingsOptions;

    [ShowInInspector, HideLabel, ListDrawerSettings(Expanded = true, DraggableItems = false, HideRemoveButton = true, HideAddButton = true), HorizontalGroup("Main"), VerticalGroup("Main/Right", 1)]
    private List<Options> SettingsOptions
    {
        get => _settingsOptions ??= GetOptions();
        set
        {
            _settingsOptions = value;
            RefreshList();
        }
    }

    public List<Options> GetOptions()
    {
        List<Options> options = new List<Options>();
        for (var i = 0; i < values.Count; i++)
        {
            ScriptableSettings settings = values[i];
            options.Add(new Options($"{i}-{settings.name}", () => Select(settings)));
        }

        return options;
    }

    private void Select(ScriptableSettings settings)
    {
        int index = values.IndexOf(settings);
        Selected = values[index];
    }

    [ShowInInspector, VerticalGroup("Main/Left"), HideLabel]
    public string Name
    {
        get => Selected.name;
        set
        {
            Selected.name = value;
            RefreshList();
        }
}
    
    [SerializeField, HideInInspector] private int index;

    [ShowInInspector, Title("", HorizontalLine = true), VerticalGroup("Main/Left"), InlineEditor(Expanded = true, DrawHeader = false, ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
    public ScriptableSettings Selected
    {
        get => values[index];
        set => index = values.IndexOf(value);
    }
        
   
    [Button("Set As Default"),VerticalGroup("Main/Right")]
    private void SetAsMain()
    {
        SetMain(index);
        index = 0;
        Selected = values[index];
        RefreshList();
    }
    [Button("↑"),HorizontalGroup("Main/Right/Move")]
    private void MoveUp()
    {
        if (index == 0) return;
        var current = Selected;
        values.Remove(current);
        values.Insert(--index,current);
        RefreshList();
    }
    
    [Button("↓"),HorizontalGroup("Main/Right/Move")]
    private void MoveDown()
    {
        if (index == values.Count-1) return;
        var current = Selected;
        values.Remove(current);
        values.Insert(++index,current);
        RefreshList();
    }

    private void RefreshList() => _settingsOptions = GetOptions();

    [Button("Create New"), VerticalGroup("Main/Right")]
    private void AddNew()
    {
        Selected = Add(ContentType);
        RefreshList();
    }

    [Button, VerticalGroup("Main/Right"),EnableIf(nameof(CanRemove))]
    private void Remove()
    {
        Remove(index);
        index = 0;
        RefreshList();
    }

    private bool CanRemove() => values.Count > 1;
    private ScriptableSettings Add(Type bucketContentType)
    {
        var nSettings = CreateInstance(bucketContentType);
        nSettings.name = values.Count.ToString();
        AssetDatabase.AddObjectToAsset(nSettings, this);
        values.Add(nSettings as ScriptableSettings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return nSettings as ScriptableSettings;
    }

    public IReadOnlyList<ScriptableSettings> GetValues() =>values;

    
    public T Add<T>() where T : ScriptableSettings => Add(typeof(T)) as T;

    public void Remove(int index) => Remove(values[index]);

    public void Remove(ScriptableSettings scriptableSettings)
    {
        values.Remove(scriptableSettings);
        Undo.DestroyObjectImmediate(scriptableSettings);
    }

    public void SetMain(int index) => SetMain(values[index]);

    private void SetMain(ScriptableSettings value)
    {
        values.Remove(value);
        values.Insert(0,value);
    }
#endif
    
    public IReadOnlyList<T> GetValues<T>() where T : ScriptableSettings
    {
#if UNITY_EDITOR
        if (values.Count == 0)
            Add<T>();
#endif
        if (typeof(T) != ContentType) throw new Exception($"Wrong type requested. ContentType:{ContentType} Request:{typeof(T)}.");
        return values.ConvertAll(x=> x as T);
    }

    public IReadOnlyList<ScriptableSettings> GetValues(Type bucketContentType)
    {
#if UNITY_EDITOR
        if (values.Count == 0)
            Add(bucketContentType);
#endif
        if (bucketContentType != ContentType) throw new Exception($"Wrong type requested. ContentType:{ContentType} Request:{bucketContentType}.");
        return values;
    }

    
    
}


public struct Options
{
    private string Label;
    private readonly Action _callback;

    public Options(string label, Action callback)
    {
        Label = label;
        _callback = callback;
    }

    [Button("$Label")]
    public void Pressed() => _callback?.Invoke();
}