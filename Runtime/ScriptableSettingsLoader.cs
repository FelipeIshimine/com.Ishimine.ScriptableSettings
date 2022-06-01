using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable, InlineProperty]
public class ScriptableSettingsLoader<T> where T : ScriptableSettings
{
    [SerializeField, LabelWidth(70), InfoBox("No bucket found", InfoMessageType.Error, nameof(ShowErrorMessage))]
    [OnValueChanged(nameof(ValidateBucket))]
    private ScriptableSettingsBucket bucket;

    public enum Mode
    {
        Default,
        Manual,
        Index
    }

    [SerializeField, OnValueChanged(nameof(ModeRefresh))]
    private Mode mode;

    [SerializeField, LabelWidth(70), ShowIf(nameof(ShowManual)), ValueDropdown(nameof(GetOptions))]
    private T selected;

    [SerializeField, HideInInspector] private int index;


    [ShowInInspector, ShowIf(nameof(ShowIndex))]
    private int Index
    {
        get => index;
        set => index = Mathf.Clamp(value, 0, bucket.Count);
    }

    private bool ShowManual => mode == Mode.Manual;
    private bool ShowIndex => mode == Mode.Index;
    private bool ShowErrorMessage => !bucket;

    private void ValidateBucket()
    {
        if (bucket && bucket.ContentType != typeof(T))
        {
            bucket = null;
            Debug.LogWarning("Wrong bucket.");
        }
    }

    private IEnumerable GetOptions()
    {
        List<ValueDropdownItem<ScriptableSettings>> dropdownItems = new List<ValueDropdownItem<ScriptableSettings>>();
        if (bucket)
        {
            var all = bucket.GetValues();
            foreach (var value in all)
                dropdownItems.Add(new ValueDropdownItem<ScriptableSettings>(value.name, value));
        }

        return dropdownItems;
    }

    public virtual T Get()
    {
        switch (mode)
        {
            case Mode.Default:
                return bucket.GetValues<T>()[0];
            case Mode.Manual:
                return selected;
            case Mode.Index:
                return bucket.GetValues<T>()[index];
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ModeRefresh(Mode nMode)
    {
        switch (nMode)
        {
            case Mode.Default:
                index = 0;
                goto case Mode.Manual;
            case Mode.Manual:
            case Mode.Index:
                selected = bucket.GetValues<T>()[index];
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(nMode), nMode, null);
        }
    }

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
         if (!bucket) 
             bucket = ScriptableSettingsEditorManager.GetBucket<T>();
#endif
    }

    public void OnAfterDeserialize()
    {
    }
}