using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class ScriptableSettingsBucket : ScriptableObject
{
    [field:SerializeField, HideInInspector] public List<BaseScriptableSettings> Values { get; private set; }= new List<BaseScriptableSettings>();
    
    public bool IsEmpty => Values.Count == 0;

    public Type ContentType => Values.Count == 0 ? null : Values[0].GetType();
    public int Count => Values.Count;

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
        for (var i = 0; i < Values.Count; i++)
        {
            BaseScriptableSettings settings = Values[i];
            options.Add(new Options($"{i}-{settings.name}", () => Select(settings)));
        }

        return options;
    }

    private void Select(BaseScriptableSettings settings)
    {
        int index = Values.IndexOf(settings);
        Selected = Values[index];
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
    public BaseScriptableSettings Selected
    {
        get => Values[index];
        set => index = Values.IndexOf(value);
    }

    public string MenuName => ContentType.Name.Replace("Settings", string.Empty);


    [Button("Set As Default"),VerticalGroup("Main/Right")]
    private void SetAsMain()
    {
        SetMain(index);
        index = 0;
        Selected = Values[index];
        RefreshList();
    }
    [Button("↑"),HorizontalGroup("Main/Right/Move")]
    private void MoveUp()
    {
        if (index == 0) return;
        var current = Selected;
        Values.Remove(current);
        Values.Insert(--index,current);
        RefreshList();
    }
    
    [Button("↓"),HorizontalGroup("Main/Right/Move")]
    private void MoveDown()
    {
        if (index == Values.Count-1) return;
        var current = Selected;
        Values.Remove(current);
        Values.Insert(++index,current);
        RefreshList();
    }

    private void RefreshList() => _settingsOptions = GetOptions();

    [Button("Create New"), VerticalGroup("Main/Right")]
    private void AddNew()
    {
        Selected = Add(ContentType);
        RefreshList();
    }

    [Button("Duplicate"), VerticalGroup("Main/Right")]
    private void Duplicate()
    {
        Selected = SaveAsset(Instantiate(Selected));
        RefreshList();
    }
    
    [Button, VerticalGroup("Main/Right"),EnableIf(nameof(CanRemove))]
    private void Remove()
    {
        Remove(index);
        index = 0;
        RefreshList();
    }

    private bool CanRemove() => Values.Count > 1;
    

    private BaseScriptableSettings Add(Type bucketContentType) => SaveAsset(CreateInstance(bucketContentType));
    
    private BaseScriptableSettings SaveAsset(ScriptableObject scriptableObject)
    {
        scriptableObject.name = Values.Count.ToString();
        AssetDatabase.AddObjectToAsset(scriptableObject, this);
        Values.Add(scriptableObject as BaseScriptableSettings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return scriptableObject as BaseScriptableSettings;
    }

    public IReadOnlyList<BaseScriptableSettings> GetValues() =>Values;

    
    public T Add<T>() where T : BaseScriptableSettings => Add(typeof(T)) as T;

    public void Remove(int index) => Remove(Values[index]);

    public void Remove(BaseScriptableSettings BaseScriptableSettings)
    {
        Values.Remove(BaseScriptableSettings);
        Undo.DestroyObjectImmediate(BaseScriptableSettings);
    }

    public void SetMain(int index) => SetMain(Values[index]);

    private void SetMain(BaseScriptableSettings value)
    {
        Values.Remove(value);
        Values.Insert(0,value);
    }
    
    public void Initialize(Type item)
    {
        Values.RemoveAll(x => x == null);
        if (Values.Count == 0) Add(item);
    }
    
#endif
    
    public IReadOnlyList<T> GetValues<T>() where T : BaseScriptableSettings
    {
#if UNITY_EDITOR
        if (Values.Count == 0)
            Add<T>();
#endif
        if (typeof(T) != ContentType) throw new Exception($"Wrong type requested. ContentType:{ContentType} Request:{typeof(T)}.");
        return Values.ConvertAll(x=> x as T);
    }

    public IReadOnlyList<BaseScriptableSettings> GetValues(Type bucketContentType)
    {
#if UNITY_EDITOR
        if (Values.Count == 0)
            Add(bucketContentType);
#endif
        if (bucketContentType != ContentType) throw new Exception($"Wrong type requested. ContentType:{ContentType} Request:{bucketContentType}.");
        return Values;
    }


    public string[] GetLabel() => new[] { $"BaseScriptableSettings, BaseScriptableSettings/{ContentType.FullName}" };


    public T GetDefault<T>() where T : ScriptableSettings<T>  => Values[0] as T;
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

