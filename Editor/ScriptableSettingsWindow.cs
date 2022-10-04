using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
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
        var tree = new OdinMenuTree(false,
            new OdinMenuTreeDrawingConfig() { DrawSearchToolbar = true, AutoHandleKeyboardNavigation = false });

        var type = typeof(BaseRuntimeScriptableSingleton);
        HashSet<Object> assets = new HashSet<Object>();
        var guids = AssetDatabase.FindAssets($"t:{type}");
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


        string[] folders = Directory.GetDirectories(Path.Combine(Application.dataPath, "ScriptableObjects"));


        foreach (string folder in folders)
        {
            var found = LoadFilesInFolder<ScriptableObject>(folder, "*", SearchOption.AllDirectories);
            var unityPath = SystemToUnityPath(folder).Replace("Assets/",string.Empty);
            tree.Add(unityPath,new ScriptableSettingsBucket(tree,found, unityPath));
        }
        
        tree.AddAllAssetsAtPath("ScriptableObjects", "ScriptableObjects", typeof(ScriptableObject), true);
        
        tree.SortMenuItemsByName();

        return tree;
    }
    
    [System.Serializable]
    public class ScriptableSettingsBucket
    {
        private OdinMenuTree _odinMenuTree;
        [ShowInInspector] private readonly string _path;
        public ScriptableSettingsBucket(OdinMenuTree odinMenuTree, ScriptableObject[] scriptableObjects, string path)
        {
            _path = path;
            _odinMenuTree = odinMenuTree;
            foreach (ScriptableObject scriptableObject in scriptableObjects)
                if(scriptableObject) ScriptableObjects.Add(new Slot(scriptableObject,_odinMenuTree, $"{_path}/{scriptableObject.name}"));
        }

        [System.Serializable]
        public class Slot
        {
            private OdinMenuTree _odinMenuTree;

            [field: ShowInInspector,  HideLabel, HorizontalGroup, InlineEditor] public ScriptableObject Instance { get; private set; }

            private readonly string _path;

            public Slot(ScriptableObject instance, OdinMenuTree odinMenuTree, string path)
            {
                _odinMenuTree = odinMenuTree;
                _path = path;
                Instance = instance;
            }

            [Button, HorizontalGroup]
            private void Select()
            {
                _odinMenuTree.Selection.Clear();

                PrintRecursive(_odinMenuTree.RootMenuItem);
                
                _odinMenuTree.Selection.Add(_odinMenuTree.GetMenuItem(_path));
            }

            private void PrintRecursive(OdinMenuItem rootMenuItem)
            {
                Debug.Log(rootMenuItem.GetFullPath());
                foreach (OdinMenuItem item in rootMenuItem.ChildMenuItems)
                    PrintRecursive(item);
            }
        }
        
        [field:SerializeField, ListDrawerSettings(HideRemoveButton = true, HideAddButton = true, Expanded = true, DraggableItems = false)] public List<Slot> ScriptableObjects { get; set; }= new List<Slot>();
    }
    
    public static T[] LoadFilesInFolder<T>(string folderPath, string pattern, SearchOption searchOption) where T : Object
    {
        string[] files = Directory.GetFiles(folderPath, pattern, searchOption);
        T[] results = new T[files.Length];
        for (var index = 0; index < files.Length; index++)
        {
            string file = files[index];
            string assetPath = SystemToUnityPath(file);
            results[index] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
        return results;
    }

    private static string SystemToUnityPath(string file) =>  "Assets" + file.Replace(Application.dataPath, "").Replace('\\', '/');
}