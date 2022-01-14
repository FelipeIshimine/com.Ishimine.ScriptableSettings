using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

    public class ScriptableSettingsWindow : OdinMenuEditorWindow
    {
        private string _filterValue = string.Empty;

        [MenuItem("Window/ScriptableSettings")]
        public static void OpenWindow()
        {
            ScriptableSettingsWindow wnd = GetWindow<ScriptableSettingsWindow>();
            wnd.titleContent = new GUIContent("Scriptable Settings");
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(false, new OdinMenuTreeDrawingConfig(){ DrawSearchToolbar = true, AutoHandleKeyboardNavigation = false});
            
            tree.AddAllAssetsAtPath("All", "Assets/ScriptableObjects/Managers", true);
        
            tree.AddAllAssetsAtPath("Settings", "Assets/ScriptableObjects/Settings",
                typeof(ScriptableSettings));
            
            var settingsManager = ScriptableSettingsManager.Instance;


            foreach (ScriptableTag tag in settingsManager.Tags)
            {
                if(tag == null) continue;
                tree.Add($"{tag.name}", tag);
                foreach (BaseRuntimeScriptableSingleton element in tag.Elements)
                {
                    if(element == null) continue;

                    string elementName = element.name;

                    if (settingsManager.removeManagerFromNames) elementName = elementName.Replace("Manager", String.Empty);
                    if (settingsManager.removeSettingsFromNames) elementName = elementName.Replace("Settings", String.Empty);
                    
                    tree.Add($"{tag.name}/{elementName}", element);
                }
            }
            tree.SortMenuItemsByName();
            tree.Add("PERSONALIZATION", settingsManager);

            return tree;
        }
    }
