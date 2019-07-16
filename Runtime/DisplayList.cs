using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

/// <summary>
/// Base class for managing a list of display elements based on a list of data objects.
/// </summary>
///
/// <typeparam name="V">The type of the display element.</typeparam>
/// <typeparam name="D">The type of the data element.</typeparam>
///
/// <remarks>
/// <para>
/// This is a specialized version of <see cref="DynamicDisplayList"/> for the case where
/// you only have one type of data element that corresponds to one type of display element.
/// To use it, create a new script that inherits from <see cref="DisplayList{T, U}"/>, and
/// specify the display element type and data element type as the type parameters.
/// </para>
///
/// <para>
/// If you have a situation where you have multiple types of data objects (or multiple
/// types of display elements) that you need to display in a single list, you should use
/// <see cref="DynamicDisplayList"/> instead.
/// </para>
/// </remarks>
public abstract class DisplayList<V, D> : BaseDisplayList, IEnumerable<V> where V : Component, IDisplayElement<D>
{
    [SerializeField]
    [Tooltip("Prefab for the view element to be used in the list. A new instance of " +
        "the prefab will be created for each list element.")]
    private V _elementPrefab = null;

    private List<D> _data = null;
    private List<V> _elements = new List<V>();

    public new IEnumerable<V> Elements
    {
        get { return _elements.Take(Count); }
    }

    public ReadOnlyCollection<V> AllElements
    {
        get { return _elements.AsReadOnly(); }
    }

    /// <summary>
    /// The number of active elements in the list.
    /// </summary>
    public int Count
    {
        get { return _data?.Count ?? 0; }
    }

    /// <summary>
    /// The total number of elements that have been instantiated, including inactive
    /// elements that are currently being pooled.
    /// </summary>
    public int Capacity
    {
        get { return _elements.Count; }
    }

    public new V this[int key]
    {
        get { return _elements[key]; }
    }

    public void Populate(List<D> data)
    {
        _data = data ?? throw new ArgumentNullException();

        for (int index = 0; index < _data.Count; index += 1)
        {
            var element = GetOrAddElement(index);
            element.Populate(_data[index]);
        }

        for (int index = _data.Count; index < _elements.Count; index += 1)
        {
            _elements[index].gameObject.SetActive(false);
        }
    }

    public IEnumerator<V> GetEnumerator()
    {
        return Elements.GetEnumerator();
    }

    private V GetOrAddElement(int index)
    {
        // If there are additional elements that are currently unused, enable and
        // return the first one.
        if (index < Capacity)
        {
            var reusedElement = _elements[index];
            reusedElement.gameObject.SetActive(true);
            return reusedElement;
        }

        // There are no pooled display elements, so create a new one and add it.
        //
        // TODO: This doesn't seem correct, given the signature of the function.
        // It seems like this method should return the element at the specified index,
        // so do we need to potentially create multiple new elements if the specified
        // index is past the end of the list?
        var element = CreateElementInstance();
        _elements.Add(element);
        return element;
    }

    /// <summary>
    /// Creates an instance of the element prefab, and makes it a child of the root transform.
    /// </summary>
    ///
    /// <returns>The new element instance.</returns>
    private V CreateElementInstance()
    {
        var element = CreateChild(_elementPrefab, true, Vector2.zero);
        element.gameObject.SetActive(true);
        return element;
    }

    #region IEnumerator
    IEnumerator IEnumerable.GetEnumerator()
    {
        return Elements.GetEnumerator();
    }
    #endregion
}
