using Sirenix.OdinInspector.Editor;
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
            var tree = new OdinMenuTree(false, new OdinMenuTreeDrawingConfig(){ DrawSearchToolbar = true});
            tree.AddAllAssetsAtPath("Managers", "Assets/ScriptableObjects/Managers");
        
            tree.AddAllAssetsAtPath("Settings", "Assets/ScriptableObjects/Settings",
                typeof(ScriptableSettings));

            tree.SortMenuItemsByName();
            return tree;
        }
    }
