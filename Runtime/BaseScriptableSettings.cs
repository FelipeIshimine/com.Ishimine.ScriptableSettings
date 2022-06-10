using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class BaseScriptableSettings : ScriptableObject
{
    [SerializeField,HideInInspector] protected BaseScriptableSettings main;

    public abstract List<BaseScriptableSettings> Settings { get; }

#if UNITY_EDITOR
    public abstract void InitializeMain();

    public abstract BaseScriptableSettings Duplicate();

    public abstract void SetOrder(int i);
    public abstract void Remove(int index);
#endif
}