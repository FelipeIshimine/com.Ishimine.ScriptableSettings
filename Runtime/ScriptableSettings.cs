using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public abstract class ScriptableSettings<T> : BaseScriptableSettings where T : ScriptableSettings<T>
{
    [field:SerializeField, ShowIf(nameof(IsMain))] private List<T> _values;

    public override List<BaseScriptableSettings> Settings => _values.ConvertAll(x => x as BaseScriptableSettings);
    private T Main => main as T;

    public List<T> Values => Main == this ? _values : Main._values;

    public string[] GetLabel() => new[] { $"BaseScriptableSettings, BaseScriptableSettings/{GetType().FullName}" };
    public int Count => Values.Count;
    public bool IsMain => this == main;

    public T Get() => Main._values[0];
    public T Get(int index) => Main._values[index];
    
#if UNITY_EDITOR
    public override void InitializeMain()
    {
        main = this;
        _values = new List<T>();
        Duplicate();
    }

    public override void SetOrder(int i)
    {
        Main._values.Remove(this as T);
        Main._values.Insert(i,this as T);
    }

    public override BaseScriptableSettings Duplicate()
    {
        var value = Instantiate(this as T);
        SaveAsset(value);
        return value;
    }

    public override void Remove(int index)
    {
        var value = Main._values[index];
        Main._values.RemoveAt(index);
        Undo.DestroyObjectImmediate(value);
    }

    private bool CanRemove() => Values.Count > 1 && this != Main;

    private T SaveAsset(T scriptableObject)
    {
        if (Main != this) return Main.SaveAsset(scriptableObject);
        scriptableObject.main = this;
        scriptableObject.name = _values.Count.ToString();
        AssetDatabase.AddObjectToAsset(scriptableObject, this);
        _values.Add(scriptableObject);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return scriptableObject;
    }

#endif

}
