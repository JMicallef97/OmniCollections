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
    /// The collection can be traversed "omnidirectionally": like a list (forwards or backwards), like a stack, or a queue (without
    /// fully removing items). The start point ("0 index") of the collection can be arbitrarily assigned.
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
                return computeIndex(collectionStartIndex + baseCollection.Count - 1);
            }
        }

        #endregion

        public OmniBuffer()
        {
            this.baseCollection = new List<T>();
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
                // check if index provided is less than 0
                if (index < 0)
                {
                    // index is out of range, throw exception
                    throw new IndexOutOfRangeException();
                }

                switch (this.isBufferReversed)
                {
                    case false:
                        // -buffer is not reversed. Traverse buffer normally.
                        // larger indices result in values that approach the start index "from the left" (wrapping around from the end of the collection back to the start)
                        return baseCollection[computeIndex(collectionStartIndex + index)];

                    case true:
                        // -buffer has been reversed. Traverse buffer backwards.
                        // -larger indices should result in values that approach the start index "from the right"
                        // -to begin, check result of subtracting desired index from collection start index
                        if (collectionStartIndex - index < 0)
                        {
                            // fell off left edge of collection (resulted in an index less than 0)
                            // -wrap index back around into the collection's range by adding Count -1 to it
                            return baseCollection[computeIndex((collectionStartIndex - index) + (Count))]; // count - 1
                        }
                        else
                        {
                            // within bounds
                            return baseCollection[computeIndex(collectionStartIndex - index)];
                        }

                    default:
                        return default(T);
                }
            }

            set
            {
                baseCollection[computeIndex(index)] = value;
            }
        }

        /// <summary>
        /// Converts a "circular buffer" index (potentially larger than the base collection size) to a normal list index ("base collection index")
        /// to allow retrieval of an item from the base collection list.
        /// </summary>
        /// <param name="circularIndex">Index to convert. If less than 0, or it is greater than or equal to base collection count, the function will throw an exception.</param>
        /// <returns>Index which corresponds to a location within the base collection list.</returns>
        private int computeIndex(int circularIndex)
        {
            return (circularIndex % Count);

            //switch (this.bufferMode)
            //{
            //    case OmniBufferMode.List:
            //        // finally, compute base collection index by applying modulo (base list count) to "wrap"
            //        // circular index around
            //        return (circularIndex % Count);

            //    case OmniBufferMode.List_Reversed:
            //        // larger indices should return values closer to the start index
            //        //return ((collectionStartIndex + baseCollection.Count - 1) - (circularIndex % Count) % Count);
            //        return ((collectionStartIndex + baseCollection.Count - 1) - (circularIndex % Count)) + collectionStartIndex;

            //    default:
            //        return -1;
            //}
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
            if (baseCollection.Count == 0)
            {
                baseCollection.Add(Item);
            }
            else
            {
                // inserts item after EndIndex
                baseCollection.Insert(EndIndex+1, Item);
            }
        }

        /// <summary>
        /// Removes an item from this OmniBuffer instance.
        /// </summary>
        /// <param name="Item">The item to remove.</param>
        public void Remove(T Item)
        {
            // remove item from base collection
            baseCollection.Remove(Item);
        }

        /// <summary>
        /// Removes an item from this OmniBuffer instance at the specified index.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            // remove item from base collection at specified index (input is expected to be a circular index)
            baseCollection.RemoveAt(computeIndex(index));
        }

        #endregion

        // functions relating to overall buffer properties
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

        #endregion
    }
}
