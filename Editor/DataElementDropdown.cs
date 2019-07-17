#if UNITY_2019
using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

public class DataElementDropdown : AdvancedDropdown
{
    private DisplayElementType _displayType;
    private Dictionary<int, Type> _dropdownItemMap = new Dictionary<int, Type>();

    public event Action<Type> ElementSelected;

    public DataElementDropdown(
        DisplayElementType displayType,
        AdvancedDropdownState state)
    : base(state)
    {
        _displayType = displayType;
    }

    protected override AdvancedDropdownItem BuildRoot()
    {
        var root = new AdvancedDropdownItem("Display Element Types");

        foreach (var dataType in _displayType.DataTypes)
        {
            var dropdownItem = new AdvancedDropdownItem(dataType.Name);
            root.AddChild(dropdownItem);
            _dropdownItemMap.Add(dropdownItem.id, dataType);
        }

        return root;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        var selectedType = _dropdownItemMap[item.id];
        ElementSelected?.Invoke(selectedType);
    }
}
#endif
