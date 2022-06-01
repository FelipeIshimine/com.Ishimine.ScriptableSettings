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
            
            /*tree.AddAllAssetsAtPath("Settings", "Assets/ScriptableObjects/Settings",
                typeof(ScriptableSettings));*/
            //var settingsManager = ScriptableSettingsEditorManager.Instance;

            //tree.AddAllAssetsAtPath("Settings/", ScriptableSettingsEditor.Folder, typeof(ScriptableSettingsBucket), true);
            
            Type type = typeof(ScriptableSettingsBucket);
            HashSet<ScriptableSettingsBucket> buckets = new HashSet<ScriptableSettingsBucket>();
            string[] guids = AssetDatabase.FindAssets($"t:{type}");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                ScriptableSettingsBucket bucket = AssetDatabase.LoadAssetAtPath<ScriptableSettingsBucket>(assetPath);
                buckets.Add(bucket);
            }
            foreach (var bucket in buckets)
                tree.Add($"Settings/{bucket.MenuName}", bucket);
            
            type = typeof(BaseRuntimeScriptableSingleton);
            HashSet<Object> assets = new HashSet<Object>();
            guids = AssetDatabase.FindAssets($"t:{type}");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                UnityEngine.Object[] found = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                for (int index = 0; index < found.Length; index++)
                    if (found[index].GetType().IsSubclassOf(type) && !assets.Contains(found[index]))
                        assets.Add(found[index]);
            }

            foreach (Object asset in assets)
            {
                if (asset is BaseRuntimeScriptableSingleton baseRuntimeScriptableSingleton)
                {
                    switch (baseRuntimeScriptableSingleton.loadMode)
                    {
                        case BaseRuntimeScriptableSingleton.AssetMode.EditorOnly:
                            tree.Add($"EditorOnly/{asset.name}", asset);
                            break;
                        case BaseRuntimeScriptableSingleton.AssetMode.Addressable:
                            tree.Add($"Release/Addressable/{asset.name}", asset);
                            break;
                        case BaseRuntimeScriptableSingleton.AssetMode.Resources:
                            tree.Add($"Release/Resources/{asset.name}", asset);
                            break;
                        case BaseRuntimeScriptableSingleton.AssetMode.AddressableManual:
                            tree.Add($"Release/ManualAddressable/{asset.name}", asset);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            
            tree.SortMenuItemsByName();

            return tree;
        }
    }
