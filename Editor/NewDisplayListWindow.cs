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

    [MenuItem("Assets/Create/Display List Script", priority = 81)]
    public static void CreateDisplayList()
    {
        _ = GetWindow<NewDisplayListWindow>();
    }

    private void OnGUI()
    {
        _className = EditorGUILayout.TextField("Class Name", _className);
        _viewType = EditorGUILayout.TextField("View Type", _viewType);
        _dataType = EditorGUILayout.TextField("Data Type", _dataType);

        // Disable the "Create" button until the user has entered valid data for all
        // the fields.
        GUI.enabled = IsValid;

        if (GUILayout.Button("Create"))
        {
            var outputPath = Path.Combine(FindSelectedDirectory(), $"{_className}.cs");

            var scriptAsset = new string[] {
                $"public class {_className} : DisplayList<{_viewType}, {_dataType}>",
                "{",
                "}",
            };

            File.WriteAllLines(outputPath, scriptAsset);
            AssetDatabase.Refresh();

            Close();
        }
    }

    private static string FindSelectedDirectory()
    {
        var path = "Assets";
        foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
            }
            break;
        }

        return path;
    }
}
