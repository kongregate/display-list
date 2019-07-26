using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DisplayList
{
    public abstract class DynamicDisplayList<D> : BaseDisplayList
    {
        private List<D> _data = null;

        public ReadOnlyCollection<D> Data
        {
            get { return _data.AsReadOnly(); }
        }

        public virtual void Populate(List<D> data)
        {
            _data = data ?? throw new ArgumentNullException("data parameter must not be null");

            Clear();

            foreach (var dataElement in _data)
            {
                InstantiateElement(dataElement);
            }
        }

        protected abstract void InstantiateElement(D data);
    }
}
