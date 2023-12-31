using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmniCollections
{
    #region ENUMS

    public enum OmniBufferMode
    {
        /// <summary>
        /// Traverse an omni buffer as a list, in ascending order (that is, larger indices return items closer to the end of the base collection).
        /// </summary>
        List,

        /// <summary>
        /// Traverse an omni buffer as a list, in reverse (descending) order (that is, larger indices return items closer to the start of the base collection).
        /// </summary>
        List_Reversed,

        ///// <summary>
        ///// Traverse an omni buffer as a bag. That is, every time an item is retrieved it is removed from the list (as if removing an item from a bag full of items).
        ///// </summary>
        //Bag,
    }

    #endregion

    /// <summary>
    /// A collection which wraps around a List collection to represent a "circular" buffer; that is, indices accessed
    /// which are larger than the collection size will be "wrapped around" to fit within the collection.
    /// 
    /// The collection can be traversed "omnidirectionally": forwards as well as backwards (without incurring the overhead of
    /// actually reversing the items in the list). The start point ("0 index") of the collection can be anywhere within the list. 
    /// An OmniBuffer can be used as a list or simply act as a wrapper around an existing list (see the "WrapList" function).
    /// </summary>
    public class OmniBuffer<T>
    {
        #region FIELDS

        /// <summary>
        /// The collection of items stored in this OmniBuffer instance, in a list
        /// </summary>
        private List<T> baseCollection = new List<T>();

        /// <summary>
        /// Represents the starting index ("index 0") of the collection. May be any number from 0 to
        /// the base collection item count - 1.
        /// </summary>
        private int collectionStartIndex;
        
        /// <summary>
        /// Flag used to determine if buffer has been reversed (that is, traversing the buffer with larger indices will return items whose indices
        /// approach the start index of the pre-reversed buffer).
        /// </summary>
        private bool isBufferReversed;

        /// <summary>
        /// Flag used to determine if OmniBuffer is read only or not.
        /// </summary>
        private bool isReadOnly;

        #endregion

        #region PROPERTIES

        public int Count
        {
            get
            {
                return baseCollection.Count;
            }
        }

        /// <summary>
        /// Gets or sets the starting index ([0]) of the collection. Must be greater than or equal to 0, and less than the number of items
        /// in the buffer.
        /// </summary>
        public int StartIndex
        {
            get
            {
                return collectionStartIndex;
            }
            set
            {
                // ensure that value is within bounds (0 to base collection count)
                if (value >= 0 && value < Count)
                {
                    // assign value
                    this.collectionStartIndex = value;
                }
                else
                {
                    // value is out of range; throw exception
                    throw new Exception("New start index ('" + value.ToString() + "') is out of range. Value must be between 0 and Count-1 (the number of items contained within this OmniBuffer instance).");
                }
            }
        }

        /// <summary>
        /// Gets the index of the last item in the collection, based on the current value of StartIndex.
        /// </summary>
        public int EndIndex
        {
            get
            {
                return computeIndex(collectionStartIndex + Count - 1);
            }
        }

        /// <summary>
        /// Gets or sets a flag indicating if this OmniBuffer instance is read-only or not.
        /// If set to true, item assignments and functions related to item addition/deletion will throw an exception.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }

            set
            {
                this.isReadOnly = value;
            }
        }

        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        public OmniBuffer()
        {
            // initialize instance variables
            this.baseCollection = new List<T>();
            this.collectionStartIndex = 0;
            this.isBufferReversed = false;
            this.isReadOnly = false;
        }

        /// <summary>
        /// Creates an OmniBuffer object with a deep copy of a list (baseList)
        /// To populate an OmniBuffer object with a shallow copy (reference to a list object),
        /// use the default constructor and then call the WrapList function in the variable instance,
        /// passing in a reference to the list to make a shallow copy of.
        /// </summary>
        /// <param name="baseList"></param>
        public OmniBuffer(List<T> baseList)
        {
            // initialize instance variables
            this.baseCollection = baseList.ToList(); // create a deep copy of the list provided.
            this.collectionStartIndex = 0;
            this.isBufferReversed = false;
        }

        /// <summary>
        /// Creates an OmniBuffer object with a deep copy of an array (baseArray).
        /// </summary>
        /// <param name="baseArray"></param>
        public OmniBuffer(T[] baseArray)
        {
            // initialize instance variables
            this.baseCollection = baseArray.ToList(); // create a deep copy of the list provided.
            this.collectionStartIndex = 0;
            this.isBufferReversed = false;
        }

        // items related to index/indexing of base collection
        #region INDEX RELATED FUNCTIONS

        // collection indexer
        public T this[int index]
        {
            get
            {
                //// check if index provided is less than 0
                //if (index < 0)
                //{
                //    // index is out of range, throw exception
                //    throw new IndexOutOfRangeException();
                //}

                // wrap negative indices around ("turn clock hands backward")
                while (index < 0)
                {
                    index += Count;
                }

                return baseCollection[computeIndex(index)];
            }

            set
            {
                // check if read only flag is set or not
                if (!isReadOnly)
                {
                    // collection isn't read only; update value in list
                    baseCollection[computeIndex(index)] = value;
                }
                else
                {
                    throw new Exception("Collection is read-only; cannot update items.");
                }
            }
        }

        /// <summary>
        /// Converts a "circular buffer" index (potentially larger than the base collection size) to a normal list index ("base collection index")
        /// to allow retrieval of an item from the base collection list.
        /// </summary>
        /// <param name="circularIndex">Index to convert. If less than 0, or it is greater than or equal to base collection count, the function will throw an exception.</param>
        /// <returns>Index which corresponds to a location within the base collection list.</returns>
        private int computeIndex(int index)
        {
            if (!isBufferReversed)
            {
                // -buffer is not reversed. Traverse buffer normally.
                // larger indices result in values that approach the start index "from the left" (wrapping around from the end of the base collection back to the start)
                return (collectionStartIndex + index) % Count;
            }
            else
            {
                // -buffer has been reversed. Traverse buffer backwards.
                // -larger indices should result in values that approach the start index "from the right" (wrapping around from the start of the base collection back to the end)
                // -because of the peridocity involved when using the modulo operator, indices smaller than
                //  the collection start index repeat.
                // -can get values at smaller indices with larger (increasing) indices by adding the base list count
                //  to the start index (to complete the next "cycle") and subtracting the index provided to "move the
                //  clock hands back" to the desired index.

                // Example: Given the list "1,2,3,4,5", index 4 is the start index (contains the element '5') and
                // the list is reversed. The index provided by the indexer is 1.
                // 1. Add the list count to the start index: 4 + 5 = 9
                // 2. Subtract the index provided by the indexer: 9 - 1 = 8
                // 3. Apply the modulo operator with the base collection count to the result, to move the
                //     result back into the range of the base collection (0 to base collection count): 8 % 5 = 3
                // 
                // -The index 3 is one less than the start index, which was 4, which is what was expected.

                return (collectionStartIndex + (Count - index)) % Count;
            }
        }

        /// <summary>
        /// Returns the first item (item located at StartIndex) in this OmniBuffer.
        /// </summary>
        /// <returns></returns>
        public T First()
        {
            return baseCollection[collectionStartIndex];
        }

        /// <summary>
        /// Returns the last item (item located at EndIndex) in this OmniBuffer.
        /// </summary>
        /// <returns></returns>
        public T Last()
        {
            return baseCollection[EndIndex];
        }

        #endregion

        // list related functions such as add, remove, etc
        #region LIST I/O FUNCTIONS

        /// <summary>
        /// Add an item to this OmniBuffer instance.
        /// </summary>
        /// <param name="Item">The item to add.</param>
        public void Add(T Item)
        {
            if (!isReadOnly)
            {
                if (baseCollection.Count == 0)
                {
                    baseCollection.Add(Item);
                }
                else
                {
                    // inserts item after EndIndex
                    baseCollection.Insert(EndIndex + 1, Item);
                }
            }
            else
            {
                throw new Exception("Collection is read-only; cannot add items.");
            }
        }

        /// <summary>
        /// Removes an item from this OmniBuffer instance.
        /// </summary>
        /// <param name="Item">The item to remove.</param>
        public void Remove(T Item)
        {
            if (!isReadOnly)
            {
                // remove item from base collection
                baseCollection.Remove(Item);
            }
            else
            {
                throw new Exception("Collection is read-only; cannot remove items.");
            }
        }

        /// <summary>
        /// Removes an item from this OmniBuffer instance at the specified index.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            if (!isReadOnly)
            {
                // remove item from base collection at specified index (input is expected to be a circular index)
                baseCollection.RemoveAt(computeIndex(index));
            }
            else
            {
                throw new Exception("Collection is read-only; cannot remove items.");
            }
        }

        /// <summary>
        /// Check if OmniBuffer contains an item, returns a boolean value indicating if item is contained.
        /// </summary>
        /// <param name="Item">The item to search for.</param>
        /// <returns></returns>
        public bool Contains(T Item)
        {
            return this.baseCollection.Contains(Item);
        }

        /// <summary>
        /// Retrieves the index of an item contained in this OmniBuffer. If the OmniBuffer does't contain the item,
        /// the function returns -1.
        /// </summary>
        /// <param name="Item">The item whose index is to be retrieved.</param>
        /// <returns></returns>
        public int IndexOf(T Item)
        {
            // check if base collection contains an item
            if (this.baseCollection.Contains(Item))
            {
                // return item's index
                return computeIndex(this.baseCollection.IndexOf(Item));
            }
            else
            {
                // return -1 since base collection doesn't contain an item
                return -1;
            }
        }

        #endregion

        // functions relating to buffer-unique properties
        #region BUFFER RELATED FUNCTIONS

        /// <summary>
        /// Reverses the order of the items in the buffer.
        /// </summary>
        public void ReverseBuffer()
        {
            // check if buffer was reversed
            if (!this.isBufferReversed)
            {
                // Buffer hasn't been reversed.
                // -To reverse buffer, move start index to the index of the last element.
                // -in a circular collection, the last element is always the element just in front of the start element (index of start element - 1)
                this.collectionStartIndex -= 1;

                // -check if subtracting start index by 1 will result in a fall off the left edge of the collection (that is, result in a value less than 0)
                if (this.collectionStartIndex - 1 < 0) // < 0
                {
                    // because collection is circular, negative values should wrap back around to the end of the collection
                    // (like turning clock hands back 1 hour. Turning a clock hand by -1 hours from 12 o clock results in the clock reading 11 o clock)
                    // -to wrap index around, add the list's count
                    this.collectionStartIndex += Count; // +1
                }
            }
            else if (this.isBufferReversed)
            {
                // Buffer has been reversed.
                // -To revert buffer ("un-reverse it"), move start index to the index of the last element.
                // The last element in a reversed circular collection is the element just behind the start element (index of start element + 1)
                this.collectionStartIndex += 1;

                // -check if add start index by 1 will result in a fall off the right edge of the collection (that is, result in a value greater than the base collection's count)
                if (this.collectionStartIndex + 1 >= Count)
                {
                    // because collection is circular, values larger than the collection size should wrap back around to the start of the collection
                    // (like turning clock hands forward 1 hour. Turning a clock hand by 1 hour past 12 o clock results in the clock reading 1 o clock)
                    // -to wrap index around, subtract the collection's count
                    this.collectionStartIndex -= Count;
                }
            }

            // update "buffer reversed" flag to indicate current state
            this.isBufferReversed = !this.isBufferReversed;
        }

        /// <summary>
        /// Assigns a reference (shallow copy) of baseList to this OmniBuffer instance's
        /// base collection. Useful for quickly gaining access to OmniBuffer features without
        /// incurring the overhead of copying items into the OmniBuffer.
        /// 
        /// This function will reset StartIndex to 0 and IsReadOnly to false.
        /// </summary>
        /// <param name="baseList"></param>
        public void WrapList(List<T> baseList)
        {
            // assign reference to baseList to baseCollection
            this.baseCollection = baseList;
            // update instance variables
            this.collectionStartIndex = 0;
            this.isBufferReversed = false;
            this.isReadOnly = false;
        }

        #endregion
    }
}
