using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class NewDisplayListWindow : EditorWindow
{
    private readonly static Type DISPLAY_ELEMENT_TYPE = typeof(IDisplayElement<>);

    private List<DisplayElementType> _displayElementTypes = null;

    private string _className = null;
    private Type _selectedDisplayType = null;
    private Type _selectedDataType = null;

#if UNITY_2019
    private AdvancedDropdownState _displayDropdownState = new AdvancedDropdownState();
#endif

    private bool IsValid
    {
        get
        {
            return !string.IsNullOrEmpty(_className) && _selectedDisplayType != null;
        }
    }

    [MenuItem("Assets/Create/Display List Script", priority = 81)]
    public static void CreateDisplayList()
    {
        var window = CreateInstance<NewDisplayListWindow>();
        window.titleContent = new GUIContent("Create Display List Script");
        window.ShowAuxWindow();
    }

    private void Awake()
    {
        // Use reflection to find all of the display element types defined in
        // the project.
        _displayElementTypes = AppDomain
            .CurrentDomain

            // Find all the types in all of the assemblies in the app domain.
            .GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())

            // Filter down to only types that implement IDisplayElement<>.
            .Where(type =>
            {
                return type
                    .GetInterfaces()
                    .Where(iface => iface.IsGenericType)
                    .Select(iface => iface.GetGenericTypeDefinition())
                    .Any(genericIface => DISPLAY_ELEMENT_TYPE.IsAssignableFrom(genericIface));
            })

            // Get the list of all data elements implemented for each display element
            // type. That is, if IDisplayElement<> is implemented multiple times for a
            // given type, find all of the different generic parameters used.
            .Select(displayType =>
            {
                var dataTypes = displayType
                    .GetInterfaces()
                    .Where(iface => iface.IsGenericType)
                    .Where(iface => DISPLAY_ELEMENT_TYPE.IsAssignableFrom(iface.GetGenericTypeDefinition()))
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
    }

    private void OnGUI()
    {
        _className = EditorGUILayout.TextField("Class Name", _className);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PrefixLabel("Display Element");

        var dropdownText = _selectedDisplayType != null
            ? $"{_selectedDataType.Name} ({_selectedDataType.Name})"
            : "Select Display Element...";
        if (EditorGUILayout.DropdownButton(new GUIContent(dropdownText), FocusType.Keyboard))
        {
            ShowDisplayElementDropdown();
        }

        EditorGUILayout.EndHorizontal();

        // Disable the "Create" button until the user has entered valid data for all
        // the fields.
        GUI.enabled = IsValid;

        if (GUILayout.Button("Create"))
        {
            var outputPath = Path.Combine(FindSelectedDirectory(), $"{_className}.cs");

            var scriptAsset = new string[] {
                $"public class {_className} : DisplayList<{_selectedDisplayType.FullName}, {_selectedDataType.FullName}>",
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

#if UNITY_2019 && false
    private void ShowDisplayElementDropdown()
    {
        var dropdown = new DisplayElementDropdown(_displayElementTypes, _displayDropdownState);
        dropdown.Selected += (displayType, dataType) =>
        {
            _selectedDisplayType = displayType;
            _selectedDataType = dataType;
        };
        dropdown.Show(GUILayoutUtility.GetLastRect());
    }
#else
    private void ShowDisplayElementDropdown()
    {
        var menu = new GenericMenu();

        foreach (var displayType in _displayElementTypes)
        {
            if (displayType.DataTypes.Count > 1)
            {
                foreach (var dataType in displayType.DataTypes)
                {
                    var itemPath = $"{displayType.DisplayType.Name}/{dataType.Name}";
                    var isActive = displayType.DisplayType.Equals(_selectedDisplayType)
                        && dataType.Equals(_selectedDataType);
                    menu.AddItem(
                        new GUIContent(itemPath),
                        isActive,
                        () =>
                        {
                            _selectedDisplayType = displayType.DisplayType;
                            _selectedDataType = dataType;
                        });
                }
            }
            else
            {
                menu.AddItem(
                    new GUIContent(displayType.ToString()),
                    displayType.DisplayType.Equals(_selectedDisplayType),
                    () =>
                    {
                        _selectedDisplayType = displayType.DisplayType;
                        _selectedDataType = displayType.DataTypes[0];
                    });
            }

        }

        menu.DropDown(GUILayoutUtility.GetLastRect());
    }
#endif
}
