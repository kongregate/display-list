using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseDisplayList : MonoBehaviour
{
    [SerializeField]
    private Transform _root = null;

    private List<Transform> _rawElements = new List<Transform>();

    public Transform Root
    {
        get { return _root; }
    }

    public IEnumerable<Transform> Elements
    {
        get { return _rawElements; }
    }

    public Transform this[int key]
    {
        get { return _rawElements[key]; }
    }

    public void Clear()
    {
        foreach (var element in _rawElements)
        {
            Destroy(element.gameObject);
        }
        _rawElements.Clear();
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

        AppendElement(instance.transform);

        return instance;
    }

    public void AppendElement(Transform element)
    {
        _rawElements.Add(element);
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
        if (index < 0 || index > _rawElements.Count)
        {
            throw new ArgumentOutOfRangeException();
        }

        if (index == _rawElements.Count)
        {
            AppendElement(element);
            return;
        }

        element.SetSiblingIndex(index);
        _rawElements.Insert(index, element);

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
        if (index < 0 || index >= _rawElements.Count)
        {
            return false;
        }

        var element = _rawElements[index];
        _rawElements.RemoveAt(index);
        element.gameObject.transform.SetSiblingIndex(_rawElements.Count);
        element.gameObject.SetActive(false);

        _rawElements.Add(element);

        return true;
    }

    #region Unity Lifecycle Methods
    private void Reset()
    {
        _root = transform;
    }
    #endregion
}
