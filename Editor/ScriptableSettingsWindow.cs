using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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
            
            tree.AddAllAssetsAtPath("Settings", "Assets/ScriptableObjects/Settings",
                typeof(ScriptableSettings));

            var settingsManager = ScriptableSettingsManager.Instance;
            Type type = typeof(BaseRuntimeScriptableSingleton);
            HashSet<Object> assets = new HashSet<Object>();
            string[] guids = AssetDatabase.FindAssets($"t:{type}");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                UnityEngine.Object[] found = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                for (int index = 0; index < found.Length; index++)
                    if (found[index].GetType().IsSubclassOf(type) && !assets.Contains(found[index]))
                        assets.Add(found[index]);
            }
            
            foreach (ScriptableSettingsTag tag in settingsManager.Tags)
            {
                if(tag == null) continue;
                tree.Add($"{tag.name}", tag);
                foreach (BaseRuntimeScriptableSingleton element in tag.Elements)
                {
                    if(element == null) continue;

                    if (assets.Contains(element))
                        assets.Remove(element);
                    
                    string elementName = element.name;

                    if (settingsManager.removeManagerFromNames) elementName = elementName.Replace("Manager", String.Empty);
                    if (settingsManager.removeSettingsFromNames) elementName = elementName.Replace("Settings", String.Empty);
                    
                    tree.Add($"{tag.name}/{elementName}", element);
                }
            }

            foreach (Object asset in assets)
                tree.Add($"_/{asset.name}", asset);
            
            tree.SortMenuItemsByName();
            tree.Add("PERSONALIZATION", settingsManager);

            return tree;
        }
    }
