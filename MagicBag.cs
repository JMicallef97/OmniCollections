using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmniCollections
{
    /// <summary>
    /// A collection where items are "removed" (become inaccessible) as they are selected, such as a person removing groceries from a shopping bag.
    /// However, item order is retained, and item inaccessibility can be reset (hence the "Magic" part of MagicBag).
    /// 
    /// This collection is useful for randomly picking items from a list without repeats, such as a function to pick songs from a playlist.
    /// </summary>
    public class MagicBag<T>
    {
        #region FIELDS

        /// <summary>
        /// Base collection used to store items in the magic bag.
        /// </summary>
        private OmniBuffer<T> baseCollection;

        #endregion
    }
}
