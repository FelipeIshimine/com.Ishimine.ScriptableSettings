using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/*
[CustomPropertyDrawer(typeof(ScriptableSettings<>), true)]*/
public class ScriptableSettingsDrawer : PropertyDrawer
{
    private int _index;
    private List<Object> sObjects;

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
        //GUI.enabled = false;
        EditorGUI.ObjectField(objectPosition, property, GUIContent.none);
        //GUI.enabled = true;

        Rect buttonPosition = new Rect(position.x + labelWidth + objectFieldWidth, position.y, dropdownButtonWidth, 16f);

        ScriptableObject current = property.objectReferenceValue as ScriptableObject;
        Type targetType = fieldInfo.FieldType;
        
        //Debug.Log($"Current:{current}");
        if (EditorGUI.DropdownButton(buttonPosition, new GUIContent(current != null?current.name:"Null"), FocusType.Keyboard))
        {
            GenericMenu menu = new GenericMenu();
            var main = ScriptableSettingsEditor.GetMain(targetType);
            sObjects = new List<Object>(main.Settings);
            for (var index = 0; index < sObjects.Count; index++)
            {
                int localIndex = index;
                Object obj = sObjects[localIndex];

                string path = obj != null ? obj.name : "Null";
                menu.AddItem(
                    new GUIContent(path),
                    obj == current, 
                    ()=> 
                    {
                        property.objectReferenceValue = sObjects[localIndex];
                        //_index = localIndex;
                    });
            }
            
            menu.ShowAsContext();
        }

        /*if (sObjects != null && _index >= 0 && _index < sObjects.Count)
            property.objectReferenceValue = sObjects[_index];
        else
        {
            var main = ScriptableSettingsEditor.GetMain(targetType);
            sObjects = new List<Object>(main.Settings);
            if(current != null) _index = sObjects.FindIndex(x => x.name == current.name);
            if (_index == -1)
                _index = 0;
        }*/

        /*if (_index == -1)
        {
            _index = property.objectReferenceValue == null ? 0 : _scriptableSettingsOptions.IndexOf(property.objectReferenceValue as BaseScriptableSettings);
        }*/

        /*if(_index >= 0)
            property.objectReferenceValue = _scriptableSettingsOptions[_index];*/
    }

}