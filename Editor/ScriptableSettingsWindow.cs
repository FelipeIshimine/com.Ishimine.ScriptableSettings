using System;
using System.Collections.Generic;
using GameStateMachineCore;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Object = UnityEngine.Object;

public class ScriptableSettingsWindow : OdinMenuEditorWindow
{
    private Object _selection;
    private string _filterValue = string.Empty;

    [MenuItem("Other/ScriptableSettings", priority = 1)]
    public static void OpenWindow()
    {
        ScriptableSettingsWindow wnd = GetWindow<ScriptableSettingsWindow>();
        wnd.titleContent = new GUIContent("Scriptable Settings");
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(false, new OdinMenuTreeDrawingConfig(){ DrawSearchToolbar = true});
        tree.AddAllAssetsAtPath("Managers", "Assets/ScriptableObjects/Managers");
        
        tree.AddAllAssetsAtPath("Settings", "Assets/ScriptableObjects/Settings",
            typeof(ScriptableSettings));

        tree.SortMenuItemsByName();
        return tree;
    }
}
