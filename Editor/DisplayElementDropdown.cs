#if UNITY_2019
using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

public class DisplayElementDropdown : AdvancedDropdown
{
    private List<DisplayElementType> _displayTypes;
    private Dictionary<int, (Type, Type)> _dropdownItemMap = new Dictionary<int, (Type, Type)>();

    public event Action<Type, Type> Selected;

    public DisplayElementDropdown(
        List<DisplayElementType> displayTypes,
        AdvancedDropdownState state)
    : base(state)
    {
        _displayTypes = displayTypes;
    }

    protected override AdvancedDropdownItem BuildRoot()
    {
        var root = new AdvancedDropdownItem("Display Element Types");

        foreach (var displayType in _displayTypes)
        {
            AdvancedDropdownItem displayTypeItem;
            if (displayType.DataTypes.Count > 1)
            {
                displayTypeItem = new AdvancedDropdownItem(displayType.DisplayType.Name);
                foreach (var dataType in displayType.DataTypes)
                {
                    var dataTypeItem = new AdvancedDropdownItem(dataType.Name);
                    displayTypeItem.AddChild(dataTypeItem);

                    _dropdownItemMap.Add(dataTypeItem.id, (displayType.DisplayType, dataType));
                }
            }
            else
            {
                displayTypeItem = new AdvancedDropdownItem(displayType.ToString());
                _dropdownItemMap.Add(
                    displayTypeItem.id,
                    (displayType.DisplayType, displayType.DataTypes[0]));
            }

            root.AddChild(displayTypeItem);
        }

        return root;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        var (displayType, dataType) = _dropdownItemMap[item.id];
        Selected?.Invoke(displayType, dataType);
    }
}
#endif
