using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace DisplayList
{
    /// <summary>
    /// Base class for managing a list of display elements based on a list of data objects.
    /// </summary>
    ///
    /// <typeparam name="V">The type of the display element.</typeparam>
    /// <typeparam name="D">The type of the data element.</typeparam>
    ///
    /// <remarks>
    /// <para>
    /// This is a specialized type of display list for the case where
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
    public abstract class DisplayList<V, D> : BaseDisplayList, IEnumerable<V>
        where V : Component, IDisplayElement<D>
    {
        [SerializeField]
        [Tooltip("Prefab for the view element to be used in the list. A new instance of " +
            "the prefab will be created for each list element.")]
        private V _elementPrefab = null;

        private List<D> _data = null;
        private List<V> _elements = new List<V>();

        /// <summary>
        /// The active display elements in the list.
        /// </summary>
        public new IEnumerable<V> Elements => _elements.Take(Count);

        /// <summary>
        /// The underlying list of data elements.
        /// </summary>
        public ReadOnlyCollection<D> Data => _data.AsReadOnly();

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
        ///
        /// <remarks>
        /// This represents the number of elements that can be added to the list before
        /// new display elements will need to be instantiated.
        /// </remarks>
        public int Capacity
        {
            get { return _elements.Count; }
        }

        public new V this[int key]
        {
            get { return _elements[key]; }
        }

        /// <summary>
        /// Indicates that a new display element was instantiated and added to the pool
        /// of available elements.
        /// </summary>
        ///
        /// <remarks>
        /// This will only be raised once for each display element that was created. It
        /// can be used to perform any one-time setup that needs to be done with the
        /// display elements, such as registering callbacks.
        /// </remarks>
        public event Action<V> ElementInstantiated;

        /// <summary>
        /// Indicates that a display element has been added to the list.
        /// </summary>
        ///
        /// <remarks>
        /// This is guaranteed to be raised every time an element is logically added to
        /// the list, even in cases where the game object was never really removed from
        /// the list. This specifically applies to the case where you re-populate a list
        /// and existing display elements are reused. In this case the element added
        /// event will be raised for any existing elements that are in the list, as well
        /// as any new elements that are instantiated.
        /// </remarks>
        public event Action<V> ElementAdded;

        /// <summary>
        /// Indicates that a display element has been removed from the list.
        /// </summary>
        ///
        /// <remarks>
        /// This is guaranteed to be raised every time an element is logically removed
        /// from the list, even in cases where the game object was never really removed.
        /// This specifically applies to the cases where you re-populate a list and
        /// existing display elements are reused. In this case the element removed event
        /// will be raised for any existing elements that are in the list before the new
        /// list of elements is populated.
        /// </remarks>
        public event Action<V> ElementRemoved;

        /// <summary>
        /// Populates the display list with the provided data elements.
        /// </summary>
        ///
        /// <param name="data">The data to use when populating the list.</param>
        ///
        /// <remarks>
        /// Any existing view elements will be removed from the list before populating
        /// the list with <paramref name="data"/>. One display element is added for each
        /// element in <paramref name="data"/>, and the display element is automatically
        /// populated with the corresponding data element. Any existing display elements
        /// will be reused in order to reduce object instantiation costs.
        /// </remarks>
        public void Populate(List<D> data)
        {
            _data = data ?? throw new ArgumentNullException();

            // "Remove" all active elements.
            foreach (var element in this)
            {
                OnElementRemoved(element);
                ElementRemoved?.Invoke(element);
            }

            // Add a display element for every data element in the list.
            for (int index = 0; index < _data.Count; index += 1)
            {
                var element = GetOrAddElement(index);
                element.Populate(_data[index]);

                OnElementAdded(element);
                ElementAdded?.Invoke(element);
            }

            // Disable any leftover display elements that were previously active in the
            // list but are no longer needed.
            for (int index = _data.Count; index < _elements.Count; index += 1)
            {
                _elements[index].gameObject.SetActive(false);
            }

            OnPopulated();
        }

        #region Virtual Lifecycle Methods
        /// <summary>
        /// Called when the list has been populated (both the first time and on any later
        /// re-populations).
        /// </summary>
        ///
        /// <remarks>
        /// This is a lifecycle method that custom subclasses can override in order to
        /// add custom behavior to the display list. It will be called after the list
        /// has finished populating.
        /// </remarks>
        protected virtual void OnPopulated() { }

        /// <summary>
        /// Called when a new list element is first intantiated.
        /// </summary>
        ///
        /// <param name="element">The new element that was instantiated.</param>
        ///
        /// <remarks>
        /// This is a lifecycle method that custom subclasses can override in order to
        /// add custom behavior to the display list. It will be called immediately before
        /// the <see cref="ElementInstantiated"/> event is raised.
        /// </remarks>
        protected virtual void OnElementInstantiated(V element) { }

        /// <summary>
        /// Called when a display element is logically added to the list.
        /// </summary>
        ///
        /// <param name="element">The element that was added to the list.</param>
        ///
        /// <remarks>
        /// <para>
        /// This is a lifecycle method that custom subclasses can override in order to
        /// add custom behavior to the display list. It will be called immediately before
        /// the <see cref="ElementAdded"/> event is raised.
        /// </para>
        ///
        /// <para>
        /// Note that, due to how list elements are pooled and reused, an element may not
        /// have been literally removed from the list before this method is called.
        /// <see cref="OnElementRemoved(V)"/> will always be called before this method is
        /// called again, though.
        /// </para>
        /// </remarks>
        protected virtual void OnElementAdded(V element) { }

        /// <summary>
        /// Called when a display element is logically removed from the list.
        /// </summary>
        ///
        /// <param name="element">The element that was removed from the list.</param>
        ///
        /// <remarks>
        /// <para>
        /// This is a lifecycle method that custom subclasses can override in order to
        /// add custom behavior to the display list. It will be called immediately before
        /// the <see cref="ElementRemoved"/> event is raised.
        /// </para>
        ///
        /// <para>
        /// Note that, due to how list elements are pooled and reused, an element may not
        /// have literally been removed from the list before this method is called.
        /// <see cref="OnElementAdded(V)"/> will always be called before this method is
        /// called again, though.
        /// </para>
        /// </remarks>
        protected virtual void OnElementRemoved(V element) { }
        #endregion

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

            // Notify listeners that a new element was instantiated.
            OnElementInstantiated(element);
            ElementInstantiated?.Invoke(element);

            return element;
        }

        #region IEnumerator
        public IEnumerator<V> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Elements.GetEnumerator();
        }
        #endregion
    }
}
