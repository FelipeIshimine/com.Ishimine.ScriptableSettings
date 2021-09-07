using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

    public class RenameScriptableObjectWindow : EditorWindow
    {
        public float rotationAmount = 0.33f;
        public string selected = "";

        public ScriptableObject target;
        private TextField _textField;

        private static RenameScriptableObjectWindow window;
    
        [MenuItem("Assets/RenameAsset")]
        public static void ChangeName()
        {
            Show(Selection.activeObject as ScriptableObject);
        }
    
        [MenuItem("Assets/RenameAsset",true)]
        public static bool ChangeNameValidator()
        {
            return Selection.activeObject is ScriptableObject;
        }
    
        public static void Show(ScriptableObject target)
        {
            if(window == null)
                window = CreateInstance(typeof(RenameScriptableObjectWindow)) as RenameScriptableObjectWindow;
        
            Debug.Assert(window != null, nameof(window) + " != null");
            window.titleContent.text = "Rename tab";
            window.Focus();
            window.SetTarget(target);
            window.ShowUtility();
        }

        private void SetTarget(ScriptableObject target)
        {
            this.target = target;
            _textField.SetValueWithoutNotify(target.name);
        }

        private void OnEnable()
        {
            VisualElement root = rootVisualElement;

            var visualTree = Resources.Load<VisualTreeAsset>("RenameTab_Main");

            visualTree.CloneTree(root);

            _textField = rootVisualElement.Q<TextField>("TextField");
            Button acceptButton = rootVisualElement.Q<Button>("AcceptB");
            Button cancelButton = rootVisualElement.Q<Button>("CancelB");
            Button defaultButton = rootVisualElement.Q<Button>("DefaultB");

            acceptButton.clicked += () => ChangeName(_textField.value);
            cancelButton.clicked += Close;
            defaultButton.clicked += () => _textField.SetValueWithoutNotify(target.name);
        }

        private void ChangeName(string newName)
        {
            target.name = newName;
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Close();
        }

        private void OnGUI()
        {
            if (Input.GetKeyDown(KeyCode.Return))
                ChangeName(_textField.value);
            else if (Input.GetKeyDown(KeyCode.Escape))
                Close();
        }
    }
