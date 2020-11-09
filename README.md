# Display List

This package acts as a framework for building and managing data-driven lists in your Unity projects. Simply define the data model for what you want do display, create a `MonoBehaviour` script and prefab for the elements in the list, and then give display-list the list of data elements you want to display.

For simple cases display-list handles all the work of managing a list of objects. Even in more complex cases, display-list provides tools to make it as easy as possible for you to write clean, correct view logic.

```csharp
// MyData.cs
//
// Define the data you want to diplay in your game.
public struct MyData
{
    public string Name;
    public int Value;
}

// MyElement.cs
//
// Create a script for the view elements for your data.
public class MyElement : MonoBehaviour, IDisplayElement<MyData>
{
    [SerializeField]
    private TextMeshPro _nameText;

    [SerializeField]
    private TextMeshPro _valueText;

    public void Populate(MyData data)
    {
        // Do whatever you need to display your data.
        _nameText.text = data.Name;
        _valueText.text = data.Value.ToString();
    }
}

// MyDisplayList.cs
//
// Create a specialized subclass of DisplayList for your data type.
public class MyDisplayList : DisplayList<MyElement, MyData> { }

// MyGameLogic.cs
//
// Use your specialized display list to display data in your game.
public class MyGameLogic : MonoBehaviour
{
    [SerializeField]
    private MyDisplayList _displayList;

    public void Start()
    {
        // Get the data that you want to display.
        var dataToDisplay = new List<MyData>()
        {
            new MyData { Name = "Foo", Value = 12 },
            new MyData { Name = "Bar", Value = 7 },
            new MyData { Name = "Baz", Value = 123 },
        };

        // Populate the display list with your data.
        _displayList.Populate(dataToDisplay);
    }
}
```

## Setup

To include display-list as a Unity package, you'll need Unity 2018.3 or later. Open `Packages/manifest.json` in your project and add "com.synapse-games.display-list" to the `dependencies` object:

```json
{
  "dependencies": {
    "com.synapse-games.display-list": "https://github.com/randomPoison/display-list.git"
  }
}
```

> NOTE: You'll need to have Git installed on your development machine for Unity to be able to download the dependency. See https://git-scm.com/ for more information.

> NOTE: If you're using an older version of Unity, you can still use this package by copying the contents into your project's `Plugins` folder.

If you're using [assembly definitions](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html) for your project, you'll need to reference the `Unity.DisplayList.Runtime` assembly in order to access display-list.

## Usage

There are two primary ways to use display-list:

* Simple case: You have a single type of data element and a single type of view element.
* Complex case: You have multiple types of data element, multiple types of view elements, or both.

### Simple Case

> TODO: Flesh this out. Use the example at the top of the README as a quick reference.

### Complex Case

Create a subclass of `DynamicDisplayList` and implement the `InstantiateElement()` method. You'll have to manually implement any logic for deciding which view elements to instantiate from your data elements, but `DynamicDisplayList` will take care of everything else.

As an example, let's take a look at how we might display rewards from an in-game quest in a hero collector game. In practice, quests might be able to award a number of different types of items, but for our purposes let's say there are three reward types:

* An item, represented by `ItemRewardData`.
* A specific hero, represented by `HeroRewardData`.
* A random hero, represented by `RandomHeroReward`.

In this case we have two display elements: A display element for item rewards, and a display element for hero rewards (which knows how to display both specific heroes and random heroes).

When representing the raw list of rewards data, we use a `List<object>` (or some other list of untyped data) in order to allow us to store the multiple different reward types in our list. When creating the display list for our rewards, we have to manually check the type of each element and instantiate the appropriate display element for that type of reward.

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestRewardDisplayList : DynamicDisplayList<object>
{
    [SerializeField]
    private ItemRewardDisplay ItemRewardPrefab;

    [SerializeField]
    private HeroRewardDisplay HeroRewardPrefab;

    // Quests can reward both items and new heroes, so the InstantiateElement()
    // method takes an untyped item and uses different display elements based
    // on the type of the reward.
    public void InstantiateElement(object reward)
    {
        switch (reward)
        {
            case ItemRewardData itemReward:
                CreateChild(ItemRewardPrefab).Populate(itemReward);
                break;

            case HeroRewardData heroReward:
                CreateChild(HeroRewardPrefab).Populate(heroReward);
                break;

            case RandomHeroRewardData randomHeroReward:
                CreateChild(HeroRewardPrefab).Populate(randomHeroReward);
                break;

            default:
                throw new ArgumentException(
                    $"Unknown reward data type {reward.GetType().Name}");
        }
    }
}
```

While this is less convenient than the simple case, it still provides a number of advantages over manually managing the list of elements:

* Down-casting is centralized in the `InstantiateElement()` method of your display list, so the display elements can still work with strongly-typed data.
* `DynamicDisplayList` handles the bulk of the work of manging the view elements at runtime, including pooling and recycling elements.
* Boilerplate is minimal, and creating a new display list type is easy.
* You still have complete control over the logic for instantiating your view elements, and can perform any custom logic necessary.

### Custom Display Lists

If `DynamicDisplayList` is still too constraining for your purposes, you can create a direct subclass of `BaseDisplayList`. This provides only minimal functionality for managing a list of view elements, but gives you the most flexibility and control.

> TODO: Provide more documentation on what functionality `BaseDisplayList` provides and how to use it correctly.
