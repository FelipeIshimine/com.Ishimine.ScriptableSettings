using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(BucketDropdownAttribute))]
public class ScriptableObjectDropdownDrawer : PropertyDrawer
{
    //public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 32;
    private int _index;
    private List<Object> sObjects;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect buttonPosition;
        if (!string.IsNullOrEmpty(label.text))
        {
            Rect labelPosition = new Rect(position.x, position.y, position.width*.50f, 16f);
            buttonPosition= new Rect(position.x + labelPosition.width, position.y, position.width * .50f, 16f);
            EditorGUI.LabelField(labelPosition, label);
        }
        else
            buttonPosition = new Rect(position.x , position.y, position.width, 16f);

        var targetContent = (attribute as BucketDropdownAttribute).TargetType;
        
        
        ScriptableObject current = property.objectReferenceValue as ScriptableObject;

        Type targetType = typeof(ScriptableSettingsBucket);
            
        //Debug.Log($"Current:{current}");
        if (EditorGUI.DropdownButton(buttonPosition, new GUIContent(current != null?current.name:"Null"), FocusType.Keyboard))
        {
            GenericMenu menu = new GenericMenu();
            sObjects = FindAssetsByType(targetType);
            sObjects.Insert(0, null);
            
            
            for (var index = 0; index < sObjects.Count; index++)
            {
                if (sObjects[index] is ScriptableSettingsBucket bucket)
                {
                    if(bucket.ContentType != targetContent)
                        continue;
                }
                else
                    continue;

                int localIndex = index;
                Object obj = sObjects[localIndex];

                string path = obj != null ? obj.name : "Null";
                
                menu.AddItem(
                    new GUIContent(path),
                    obj == current, 
                    ()=> 
                    {
                        property.objectReferenceValue = sObjects[localIndex];
                        _index = localIndex;
                    });
            }
            
            menu.ShowAsContext();
        }

        if (sObjects != null && _index >= 0 && _index < sObjects.Count)
            property.objectReferenceValue = sObjects[_index];
        else
        {
            sObjects = FindAssetsByType(targetType);
            if(current != null) _index = sObjects.FindIndex(x => x.name == current.name);
            if (_index == -1)
                _index = 0;
        }
        
    }


    private string GetProperName(SerializedProperty property)
    {
        if (!property.type.StartsWith("PPtr<"))
            return property.type;
        return property.type.Substring(5).Trim('<', '>', '$');
    }
    
    
    private static List<Object> FindAssetsByType(Type type)
    {
        List<Object> assets = new List<Object>();
        string[] guids = AssetDatabase.FindAssets($"t:{type}");
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            Object[] found = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            for (int index = 0; index < found.Length; index++)
                if (found[index] is { } item && !assets.Contains(item))
                    assets.Add(item);
        }

        return assets;
    }

    public override bool CanCacheInspectorGUI(SerializedProperty property) => true;
}