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
        /// <summary>
        /// Omni buffer of flags, indicating if an item is accessible or not.
        /// </summary>
        private OmniBuffer<FlagPtr> itemAccessibilityFlags;

        /// <summary>
        /// An flag that indicates whether items have been accessed or not. When an item is accessed,
        /// the index corresponding to the item (in itemAccessibilityFlags collection) is assigned a reference
        /// to this object.
        /// 
        /// When bag state is reset (items become accessible again), this itemAccessedFlag is set to null and then
        /// re-initialized. As a result, all items in itemAccessibilityFlags will point to null (items that haven't ever
        /// been accessed in addition to items who have been accessed and assigned a reference to this field, since the
        /// reference points to an object that is null).
        /// </summary>
        private FlagPtr itemAccessedFlag;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public MagicBag()
        {
            // initialize variables
            this.baseCollection = new OmniBuffer<T>();
            this.itemAccessibilityFlags = new OmniBuffer<FlagPtr>();
            this.itemAccessedFlag = new FlagPtr(true);
        }

        // Functions that relate to bag-specific functionality (pulling items out of bag, etc)
        #region BAG RELATED FUNCTIONS

        /// <summary>
        /// Attempts to grab an item from the bag at the specified index. If the item is still accessible (hasn't been
        /// pulled from the bag yet) the function will return true and bagItem will be assigned the value of (or reference to) the item.
        /// If the item is not accessible, the function will return false and bagItem will be populated with the default value of the
        /// type of item the bag is holding.
        /// </summary>
        /// <param name="itemIndex">The index of the item to access</param>
        /// <param name="bagItem">The value (or reference to) of the item being grabbed from the bag.</param>
        /// <returns>Flag indicating whether item is accessible or not.</returns>
        public bool GrabItem(int itemIndex, out T bagItem)
        {
            // check if flag has been assigned
            if (this.itemAccessibilityFlags[itemIndex] == null)
            {
                // item hasn't been pulled from bag yet
                // -pull item from bag
                bagItem = this.baseCollection[itemIndex];

                // -update item's flag in itemAccessibilityFlags by assigning a reference to itemAccessedFlag
                this.itemAccessibilityFlags[itemIndex] = this.itemAccessedFlag;

                // return true since item was able to be pulled from bag
                return true;
            }
            else
            {
                // check if flag value is true or not
                if (!this.itemAccessibilityFlags[itemIndex].flagValue)
                {
                    // item hasn't been grabbed from bag yet, or was grabbed before the bag was reset.
                    // -pull item from bag
                    bagItem = this.baseCollection[itemIndex];

                    // -update item's flag in itemAccessibilityFlags by assigning a reference to itemAccessedFlag
                    this.itemAccessibilityFlags[itemIndex] = this.itemAccessedFlag;

                    // return true since item was able to be pulled from bag
                    return true;
                }
                // otherwise item was previously grabbed from bag
            }

            // item was pulled from bag previously
            // -return default value and false
            bagItem = default(T);
            return false;
        }

        public void resetBagState()
        {
            // reset flag state of item accessed flag (so items that were grabbed
            // before the bag was reset, and still retain a reference to this instance
            // of the itemAccessedFlag) will be able to be grabbed after the reset)
            this.itemAccessedFlag.flagValue = false;
            // break reference to previous instance of itemAccessedFlag, so items that
            // were grabbed (and assigned the flag) won't be affected by the reset
            this.itemAccessedFlag = null;

            // create new instance of flag, assign it the value of true (to prevent
            // items grabbed from bag being able to be grabbed more than once)
            this.itemAccessedFlag = new FlagPtr(true);
        }

        #endregion

        // functions related to collection I/O (adding items, removing items, etc)
        #region COLLECTION I/O FUNCTIONS

        /// <summary>
        /// Adds an item to the magic bag.
        /// </summary>
        /// <param name="Item">The item to add.</param>
        public void Add(T Item)
        {
            // add item to base collection
            this.baseCollection.Add(Item);
            // add accessibility flag for item
            this.itemAccessibilityFlags.Add(null);
        }

        #endregion
    }

    public class FlagPtr
    {
        // constants
        public bool flagValue;

        public FlagPtr(bool Value)
        {
            this.flagValue = Value;
        }
    }
}
