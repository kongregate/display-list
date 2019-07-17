using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class NewDisplayListWindow : EditorWindow
{
    private readonly static Type DISPLAY_ELEMENT_TYPE = typeof(IDisplayElement<>);

    private List<DisplayElementType> _displayElementTypes;

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
        var window = ScriptableObject.CreateInstance<NewDisplayListWindow>();
        window.ShowModalUtility();
    }

    private void Awake()
    {
        _displayElementTypes = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
            {
                return type
                    .GetInterfaces()
                    .Where(iface => iface.IsGenericType)
                    .Any(iface => DISPLAY_ELEMENT_TYPE.IsAssignableFrom(iface.GetGenericTypeDefinition()));
            })
            .Select(displayType =>
            {
                var dataTypes = displayType
                    .GetInterfaces()
                    .Where(iface => iface.GetGenericTypeDefinition() == DISPLAY_ELEMENT_TYPE)
                    .Select(iface => iface.GetGenericArguments()[0])
                    .ToList();
                return new DisplayElementType
                {
                    DisplayType = displayType,
                    DataTypes = dataTypes,
                };
            })
            .ToList();

        var displayElements = _displayElementTypes.Select(type => $"{type.DisplayType.FullName} ({string.Join(", ", type.DataTypes.Select(dataType => dataType.FullName))})");
        Debug.Log($"Found {_displayElementTypes.Count} display element types:\n{string.Join("\n", displayElements)}");
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
        foreach (var obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
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

    private struct DisplayElementType
    {
        public Type DisplayType;
        public List<Type> DataTypes;
    }
}
