using System;
using System.Collections.Generic;

namespace Not.Core.Model
{
    public class BusinessObject
    {
        public int OID { get; set; }

        /// <summary>
        /// Synchronizes a reference (n:1) setter with the inverse collection (1:n) on the target.
        /// Use in the property setter of any ReferencePropertyInfo that has an InverseNavigation.
        ///
        /// The backing field is updated BEFORE the collection is touched, which breaks any
        /// potential recursion: the setter guard (ReferenceEquals) fires immediately if the
        /// collection's Add/Remove triggers the setter again.
        /// </summary>
        protected static void SetReference<TChild, TParent>(
            TChild self,
            ref TParent? field,
            TParent? newValue,
            Func<TParent, ICollection<TChild>> getCollection)
            where TChild : BusinessObject
            where TParent : BusinessObject
        {
            if (ReferenceEquals(field, newValue)) return;

            var oldParent = field;
            field = newValue;                               // update first → breaks recursion

            if (oldParent != null)
                getCollection(oldParent).Remove(self);

            if (field != null && !getCollection(field).Contains(self))
                getCollection(field).Add(self);
        }

        /// <summary>
        /// Synchronizes a one-to-one reference setter with the inverse single reference on
        /// the other entity. Use on BOTH sides of a 1:1 (or 0..1:1) association.
        ///
        /// The backing field is updated first (recursion guard), then the old partner's
        /// inverse is cleared and the new partner's inverse is pointed back to self.
        /// </summary>
        protected static void SetOneToOne<TSelf, TOther>(
            TSelf self,
            ref TOther? field,
            TOther? newValue,
            Func<TOther, TSelf?> getInverse,
            Action<TOther, TSelf?> setInverse)
            where TSelf : BusinessObject
            where TOther : BusinessObject
        {
            if (ReferenceEquals(field, newValue)) return;

            var oldOther = field;
            field = newValue;                               // update first → breaks recursion

            if (oldOther != null && ReferenceEquals(getInverse(oldOther), self))
                setInverse(oldOther, null);

            if (field != null && !ReferenceEquals(getInverse(field), self))
                setInverse(field, self);
        }
    }
}
