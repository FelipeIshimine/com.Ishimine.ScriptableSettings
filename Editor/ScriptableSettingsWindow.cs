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
            var settingsManager = ScriptableSettingsEditorManager.Instance;


            var list = settingsManager.Buckets;
            for (int i = list.Count - 1; i >= 0; i--)
                if(list[i] == null) list.RemoveAt(i);
            
            foreach (var bucket in list)
            {
                //Editor.CreateCachedEditor(null, this);
                
                tree.Add($"Setting/{bucket.ContentType}", bucket);
                var settings = bucket.GetValues();
                for (int i = 0; i < settings.Count; i++)
                {
                    tree.Add($"Setting/{bucket.ContentType}/{settings[i].name}",settings[i]);
                }
            }

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
            
            /*foreach (ScriptableSettingsTag tag in settingsManager.Tags)
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
            }*/

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
            tree.Add("PERSONALIZATION", settingsManager);

            return tree;
        }
    }
