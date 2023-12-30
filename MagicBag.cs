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
        /// Omni buffer of ints, which represent the bagStateID when the item was grabbed last.
        /// </summary>
        private OmniBuffer<int> itemGrabbedIDs;

        /// <summary>
        /// When trying to grab an item from the bag, this value is checked against the item's
        /// itemGrabbedID value. If the value is less than bagStateID, the item is grabbed and
        /// its itemGrabbedID value is assigned bagStateID.
        /// 
        /// The purpose of this value is to ensure that items grabbed before the bag state was reset
        /// can be grabbed after the reset, without having to manually iterate through the list and
        /// reset every single item's itemGrabbedID.
        /// </summary>
        private int bagStateID;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public MagicBag()
        {
            // initialize variables
            this.baseCollection = new OmniBuffer<T>();
            this.itemGrabbedIDs = new OmniBuffer<int>();
            this.bagStateID = 1;
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
            // check if itemGrabbedID of item being requested is less than the current bag state ID
            // (meaning it hasn't been grabbed from the bag after the last time the bag was reset)
            if (this.itemGrabbedIDs[itemIndex] < bagStateID)
            {
                // item hasn't been pulled from bag yet
                // -pull item from bag
                bagItem = this.baseCollection[itemIndex];

                // -update itemGrabbedID for current item to be that of bagStateID (to prevent item
                //  from being pulled again before the bag state is reset)
                this.itemGrabbedIDs[itemIndex] = bagStateID;

                // return true since item was able to be pulled from bag
                return true;
            }

            // item was pulled from bag previously
            // -return default value and false
            bagItem = default(T);
            return false;
        }

        /// <summary>
        /// Resets the state of the bag to before any items were grabbed, making all items in the bag available to grab again.
        /// </summary>
        public void resetBagState()
        {
            // increment the bag state ID
            bagStateID++;

            // check if bag state ID is the max value of an int
            if (bagStateID == Int32.MaxValue)
            {
                // need to go through flag list and reset all items (since can't increment bagStateID anymore)
                for (int m = 0; m < itemGrabbedIDs.Count; m++)
                {
                    itemGrabbedIDs[m] = 0;
                }

                // reset bag state ID to 1
                bagStateID = 1;
            }
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
            // add itemGrabbedID for item
            this.itemGrabbedIDs.Add(0);
        }

        /// <summary>
        /// Removes an item from the magic bag. Returns a boolean value indicating if item was not
        /// in list or item was unsuccessfully removed.
        /// </summary>
        /// <param name="Item">The item to add.</param>
        public bool Remove(T Item)
        {
            // check if base collection contains item
            if (this.baseCollection.Contains(Item))
            {
                // remove item's itemGrabbedID
                this.itemGrabbedIDs.RemoveAt(this.baseCollection.IndexOf(Item));
                // remove item
                this.baseCollection.Remove(Item);

                // return true since item was successfully removed
                return true;
            }
            else
            {
                // item wasn't in list
                return false;
            }
        }

        /// <summary>
        /// Removes an item from the magic bag at the specified index.
        /// </summary>
        /// <param name="Item">The index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            // perform bounds check
            if (index >= 0 && index < this.itemGrabbedIDs.Count)
            {
                // remove item's itemGrabbedID
                this.itemGrabbedIDs.RemoveAt(index);
                // remove item
                this.baseCollection.RemoveAt(index);
            }
            else
            {
                // throw out of range exception
                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// Retrieves the index of an item contained in this MagicBag. If the MagicBag does't contain the item,
        /// the function returns -1.
        /// </summary>
        /// <param name="Item">The item whose index is to be retrieved.</param>
        /// <returns></returns>
        public bool Contains(T Item)
        {
            return this.baseCollection.Contains(Item);
        }

        #endregion
    }
}
