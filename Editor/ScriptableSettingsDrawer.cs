using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ScriptableSettings<>), true)]
public class ScriptableSettingsDrawer : PropertyDrawer
{
    private int _index;
    private List<BaseScriptableSettings> _scriptableSettingsOptions;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool drawLabel = !string.IsNullOrEmpty(label.text);
        
        float labelWidth = position.width * .25f;
        
        float objectFieldWidth = position.width * .375f;
        float dropdownButtonWidth = position.width * .375f;
        
        if(drawLabel)
        {
            Rect labelPosition = new Rect(position.x, position.y, position.width * .25f, 16f);
            EditorGUI.LabelField(labelPosition, label);
        }
        else
        {
            objectFieldWidth += labelWidth/2;
            dropdownButtonWidth += labelWidth/2;
            labelWidth = 0;
        }
        
        Rect objectPosition = new Rect(position.x + labelWidth, position.y, objectFieldWidth, 16f);
        GUI.enabled = false;
        EditorGUI.ObjectField(objectPosition, property, GUIContent.none);
        GUI.enabled = true;

        Rect buttonPosition = new Rect(position.x + labelWidth + objectFieldWidth, position.y, dropdownButtonWidth, 16f);
        string buttonName = "NULL";

        if (property.objectReferenceValue != null)
            buttonName = property.objectReferenceValue.name;

        if (EditorGUI.DropdownButton(buttonPosition, new GUIContent(buttonName), FocusType.Keyboard))
        {
            GenericMenu menu = new GenericMenu();

            var targetType = fieldInfo.FieldType;

            if (typeof(IEnumerable).IsAssignableFrom(targetType))
                targetType = targetType.GenericTypeArguments[0];

            var main = ScriptableSettingsEditor.GetMain(targetType);
            
            _scriptableSettingsOptions = new List<BaseScriptableSettings>(main.Settings);
            
            _scriptableSettingsOptions.Insert(0,null);
            for (var index = 0; index < _scriptableSettingsOptions.Count; index++)
            {
                int localIndex = index;
                BaseScriptableSettings value = _scriptableSettingsOptions[localIndex];

                string path;

                if (value)
                    path = value.name;
                else
                    path = "Null";
             
                menu.AddItem(
                    new GUIContent(path),
                    value == property.objectReferenceValue, 
                    ()=>
                    {
                        property.objectReferenceValue = _scriptableSettingsOptions[localIndex];
                        _index = localIndex;
                    });
                menu.ShowAsContext();
            } 
        }

        if (_scriptableSettingsOptions != null && _scriptableSettingsOptions.Count > 0)
            property.objectReferenceValue = _scriptableSettingsOptions[_index];
    }

}