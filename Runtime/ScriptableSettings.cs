using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class ScriptableSettings : ScriptableObject
{
        public static T GetDefault<T>() where T : ScriptableSettings => ScriptableSettingsManager.GetMainSettings<T>();
        public static ScriptableSettings GetDefault(Type type) => ScriptableSettingsManager.GetMainSettings(type);
        
        public static ScriptableSettings Get(int index, Type type) => ScriptableSettingsManager.GetSettings(index, type);
        public static T Get<T>(int index) where T : ScriptableSettings => ScriptableSettingsManager.GetSettings<T>(index);
        
        [SerializeField, HideInInspector] private string tabName;
        public string TabName
        {
                get
                {
                        if (string.IsNullOrEmpty(tabName)) tabName = DefaultTabName;
                        return tabName;
                }
                set => tabName = value;
        }

        public string DefaultTabName => GetType().Name.Replace("Settings", string.Empty);
        
        [System.Serializable, InlineProperty]
        public class Provider<T> where T : ScriptableSettings
        {
                public enum Mode { Default, Manual, Index }
                [SerializeField] private Mode mode;
                
                [SerializeField, ShowIf(nameof(ShowManual)), ValueDropdown(nameof(GetOptions))] private T selected;

                [SerializeField, HideInInspector] private int index;

                [ShowInInspector, ShowIf(nameof(ShowIndex))]
                private int Index
                {
                        get => index;
                        set => index = Mathf.Clamp(value, 0, ScriptableSettingsManager.GetAllSettings<T>().Count);
                }

                private bool ShowManual => mode == Mode.Manual; 
                private bool ShowIndex => mode == Mode.Index; 

                private IEnumerable GetOptions()
                {
                        List<ValueDropdownItem<ScriptableSettings>> dropdownItems = new List<ValueDropdownItem<ScriptableSettings>>();
                        var all= ScriptableSettingsManager.GetAllSettings<T>();
                        foreach (var value in all)
                                dropdownItems.Add(new ValueDropdownItem<ScriptableSettings>(value.name, value));
                        return dropdownItems;
                }

                public virtual T Get()
                {
                        switch (mode)
                        {
                                case Mode.Default:
                                        return GetDefault<T>();
                                case Mode.Manual:
                                        return selected;
                                case Mode.Index:
                                        return Get<T>(index);
                                default:
                                        throw new ArgumentOutOfRangeException();
                        }
                        return mode == Mode.Default ? GetDefault<T>() : selected;
                }
        }
        
}

