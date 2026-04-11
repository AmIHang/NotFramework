using System;
using System.Collections;
using System.Collections.Generic;

namespace Not.Core.Model
{
    /// <summary>
    /// A collection for the "one" side of a 1:n association that keeps the inverse reference
    /// on each item in sync automatically.
    ///
    /// Add(item)    → sets item's back-reference to the owner (if not already set).
    /// Remove(item) → clears item's back-reference (only if it still points to this owner;
    ///                if the item was re-assigned to another owner in the meantime the
    ///                reference is left unchanged).
    ///
    /// Recursion safety: BusinessObject.SetReference updates the backing field BEFORE
    /// touching collections. The getter-check in Add/Remove uses the already-updated
    /// property value, so re-entrant calls become no-ops.
    /// </summary>
    public sealed class BidirectionalCollection<TParent, TChild> : ICollection<TChild>
        where TParent : BusinessObject
        where TChild : BusinessObject
    {
        private readonly TParent _owner;
        private readonly Func<TChild, TParent?> _getReference;
        private readonly Action<TChild, TParent?> _setReference;
        private readonly List<TChild> _inner = new();

        public BidirectionalCollection(
            TParent owner,
            Func<TChild, TParent?> getReference,
            Action<TChild, TParent?> setReference)
        {
            _owner = owner;
            _getReference = getReference;
            _setReference = setReference;
        }

        public void Add(TChild item)
        {
            if (_inner.Contains(item)) return;          // guard — also breaks recursion
            _inner.Add(item);
            if (!ReferenceEquals(_getReference(item), _owner))
                _setReference(item, _owner);            // sync back-reference if not already set
        }

        public bool Remove(TChild item)
        {
            if (!_inner.Remove(item)) return false;
            // Only clear the reference if the item still points to this owner.
            // If it was already re-assigned (e.g. item.Order = other), leave it alone.
            if (ReferenceEquals(_getReference(item), _owner))
                _setReference(item, null);
            return true;
        }

        public void Clear()
        {
            var snapshot = _inner.ToArray();
            _inner.Clear();
            foreach (var item in snapshot)
                if (ReferenceEquals(_getReference(item), _owner))
                    _setReference(item, null);
        }

        public bool Contains(TChild item) => _inner.Contains(item);
        public int Count => _inner.Count;
        public bool IsReadOnly => false;

        public void CopyTo(TChild[] array, int arrayIndex) => _inner.CopyTo(array, arrayIndex);
        public IEnumerator<TChild> GetEnumerator() => _inner.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();
    }
}
