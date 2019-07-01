using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class DynamicDisplayList : MonoBehaviour
{
    [SerializeField]
    private Transform _root;

    [SerializeField]
    private bool _forwardDragElements;

    [SerializeField]
    private bool _invertedDirection;

    private List<Transform> _elements = new List<Transform>();

    public Transform Root
    {
        get { return _root; }
    }

    public bool IsInverted
    {
        get { return _invertedDirection; }
    }

    public IEnumerable<Transform> ActiveElements
    {
        get { return _elements.AsReadOnly().Where(element => element.gameObject.activeSelf); }
    }

    public ReadOnlyCollection<Transform> AllElements
    {
        get { return _elements.AsReadOnly(); }
    }

    public Transform this[int key]
    {
        get { return _elements[key]; }
    }

    public void Clear()
    {
        foreach (var element in _elements)
        {
            Destroy(element.gameObject);
        }
        _elements.Clear();
    }

    public T CreateChild<T>(
        T prefab,
        bool setToIdentity = true,
        Vector2? sizeDelta = null)
    where T : Component
    {
        var instance = Instantiate(prefab, _root);

        var t = instance.transform;
        if (setToIdentity)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        if (t is RectTransform rect)
        {
            if (setToIdentity)
            {
                rect.anchoredPosition = Vector2.zero;
            }

            if (sizeDelta.HasValue)
            {
                rect.sizeDelta = sizeDelta.Value;
            }
        }

        return instance;
    }

    public void AppendElement(Transform element)
    {
        _elements.Add(element);
        element.gameObject.SetActive(true);
    }

    /// <summary>
    /// Inserts the given element at the specified index.
    /// </summary>
    ///
    /// <param name="index"></param>
    /// <param name="element"></param>
    ///
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0, or <paramref name="index"/> is greater than Count.
    /// </exception>
    public void Insert(int index, Transform element)
    {
        if (index < 0 || index > _elements.Count)
        {
            throw new ArgumentOutOfRangeException();
        }

        if (index == _elements.Count)
        {
            AppendElement(element);
            return;
        }

        element.SetSiblingIndex(index);
        _elements.Insert(index, element);

        if (element == null)
        {
            Debug.LogError(
                $"Element could not be inserted into List {_root.name} at {index}, " +
                $"Data: {element}");
        }
        else
        {
            element.gameObject.SetActive(true);
        }
    }

    public bool RemoveAt(int index)
    {
        if (index < 0 || index >= _elements.Count)
        {
            return false;
        }

        var element = _elements[index];
        _elements.RemoveAt(index);
        element.gameObject.transform.SetSiblingIndex(_elements.Count);
        element.gameObject.SetActive(false);

        _elements.Add(element);

        return true;
    }

    private void Reset()
    {
        _root = transform;
    }
}
