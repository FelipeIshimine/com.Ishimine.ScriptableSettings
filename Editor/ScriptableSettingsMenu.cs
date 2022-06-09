using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class ScriptableSettingsMenu
{
    private BaseScriptableSettings _main;

    public ScriptableSettingsMenu(BaseScriptableSettings main)
    {
        _main = main;
        index = _main.Settings.Count - 1;
    }

    public List<BaseScriptableSettings> Values => _main.Settings;
    private List<Options> _settingsOptions;

    [ShowInInspector, LabelText("Order"),
     ListDrawerSettings(Expanded = true, DraggableItems = false, HideRemoveButton = true, HideAddButton = true),
     HorizontalGroup("Main"), VerticalGroup("Main/Right", 1)]
    private List<Options> SettingsOptions
    {
        get=> _settingsOptions ??= GetOptions();
        set => _settingsOptions = value; //Solo para que sea interactivo
    }

    public List<Options> GetOptions()
    {
        List<Options> options = new List<Options>();
        for (var i = 0; i < Values.Count; i++)
        {
            BaseScriptableSettings settings = Values[i];
            options.Add(new Options($"{settings.name}", () => Select(settings)));
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

    [Button,VerticalGroup("Main/Right")]
    private void SetFirst()
    {
        if(index == 0) return;
        
        Selected.SetOrder(0);
        index = 0;
        RefreshList();
    }
    
    [Button,VerticalGroup("Main/Right")]
    private void SetLast()
    {
        if(index == 0) return;
        
        Selected.SetOrder(Values.Count-1);
        index = Values.Count-1;
        RefreshList();
    }
    
    [Button("↑"),HorizontalGroup("Main/Right/Move")]
    private void MoveUp()
    {
        if (index == 0) return;

        Selected.SetOrder(--index);
        RefreshList();
    }
    
    [Button("↓"),HorizontalGroup("Main/Right/Move")]
    private void MoveDown()
    {
        if (index == Values.Count-1) return;
        Selected.SetOrder(++index);
        RefreshList();
    }

    private void RefreshList() => _settingsOptions = GetOptions();

    [Button("Duplicate"), VerticalGroup("Main/Right")]
    private void Duplicate()
    {
        var baseScriptableSettings = Selected.Duplicate();
        index = Values.IndexOf(baseScriptableSettings);
        RefreshList();
    }
    
    [Button, VerticalGroup("Main/Right"),EnableIf(nameof(CanRemove))]
    private void Remove()
    {
        _main.Remove(index);
        RefreshList();
    }

    private bool CanRemove() => Values.Count > 1;
    
    public IReadOnlyList<BaseScriptableSettings> GetValues() =>Values;

    public void Remove(int index) => Remove(Values[index]);

    public void Remove(BaseScriptableSettings baseScriptableSettings)
    {
        Values.Remove(baseScriptableSettings);
        Undo.DestroyObjectImmediate(baseScriptableSettings);
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

