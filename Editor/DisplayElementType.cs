using System;
using System.Collections.Generic;
using System.Linq;

public struct DisplayElementType
{
    public Type DisplayType;
    public List<Type> DataTypes;

    public override string ToString()
    {
        var dataTypes = DataTypes.Select(type => type.FullName);
        return $"{DisplayType.FullName} ({string.Join(", ", dataTypes)})";
    }
}
