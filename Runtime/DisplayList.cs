using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public abstract class DisplayList<T, U> : DynamicDisplayList where T : Component, IDisplayElement<U>
{
    [SerializeField]
    private T _elementPrefab = null;

    private List<U> _data = null;
    private List<T> _elements = new List<T>();
    private int _activeCount = 0;

    public new IEnumerable<T> ActiveElements
    {
        get { return _elements.Where(element => element.gameObject.activeSelf); }
    }

    public new ReadOnlyCollection<T> AllElements
    {
        get { return _elements.AsReadOnly(); }
    }

    public int Capacity
    {
        get { return _elements.Count; }
    }

    public int Count
    {
        get { return _activeCount; }
    }

    public new T this[int key]
    {
        get { return _elements[key]; }
    }

    public void Populate(List<U> data)
    {
        _data = data ?? throw new ArgumentNullException();
        _activeCount = _data.Count;

        int index = 0;
        for (; index < _data.Count; index += 1)
        {
            var element = GetOrAddElement(index);
            element.Populate(_data[index]);
        }

        for (; index < _elements.Count; index++)
        {
            _elements[index].gameObject.SetActive(false);
        }
    }

    #region Unity Lifecycle Methods
    private void Awake()
    {
        _elementPrefab.gameObject.SetActive(false);
    }
    #endregion

    private T GetOrAddElement(int index)
    {
        if (index < Capacity)
        {
            return _elements[index];
        }

        var element = CreateElementInstance();
        _elements.Add(element);

        return element;
    }

    /// <summary>
    /// Creates an instance of the element prefab, and makes it a child of the root transform.
    /// </summary>
    ///
    /// <returns>The new element instance.</returns>
    private T CreateElementInstance()
    {
        var element = CreateChild(_elementPrefab, true, Vector2.zero);
        element.gameObject.SetActive(true);
        if (IsInverted)
        {
            element.transform.SetAsFirstSibling();
        }

        return element;
    }
}
