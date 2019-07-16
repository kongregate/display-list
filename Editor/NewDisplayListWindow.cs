using System.IO;
using UnityEditor;
using UnityEngine;

public class NewDisplayListWindow : EditorWindow
{
    private string _className;
    private string _viewType;
    private string _dataType;

    private bool IsValid
    {
        get
        {
            return !string.IsNullOrEmpty(_className)
                && !string.IsNullOrEmpty(_viewType)
                && !string.IsNullOrEmpty(_dataType);
        }
    }

    [MenuItem("Assets/Create/Display List")]
    public static void CreateDisplayList()
    {
        EditorWindow.GetWindow<NewDisplayListWindow>();
    }

    private void OnGUI()
    {
        _className = EditorGUILayout.TextField("Class Name", _className);
        _viewType = EditorGUILayout.TextField("View Type", _viewType);
        _dataType = EditorGUILayout.TextField("Data Type", _dataType);

        GUI.enabled = IsValid;
        if (GUILayout.Button("Create"))
        {
            var scriptAsset = new string[] {
                $"public class {_className} : DisplayList<{_viewType}, {_dataType}>",
                "{",
                "}",
            };
            File.WriteAllLines(
                Path.Combine(Application.dataPath, $"{_className}.cs"),
                scriptAsset);

            Close();
        }
    }
}
