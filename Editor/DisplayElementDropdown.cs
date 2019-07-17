using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

public class DisplayElementDropdown : AdvancedDropdown
{
    private List<DisplayElementType> _displayTypes;
    private Dictionary<int, DisplayElementType> _dropdownItemMap = new Dictionary<int, DisplayElementType>();

    public event Action<DisplayElementType> ElementSelected;

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
            var dropdownItem = new AdvancedDropdownItem(displayType.ToString());
            root.AddChild(dropdownItem);
            _dropdownItemMap.Add(dropdownItem.id, displayType);
        }

        return root;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        var selectedType = _dropdownItemMap[item.id];
        ElementSelected?.Invoke(selectedType);
    }
}
