using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzer.StockClasses
{
    public class StockSortedDictionary
    {
        private const int BUCKET_SIZE = 1000;
        private StockDailyValue[] values;
        private DateTime[] keys;
        private int allocatedSize;
        private int size;

        public StockSortedDictionary()
        {
            values = new StockDailyValue[BUCKET_SIZE];
            keys = new DateTime[BUCKET_SIZE];
            allocatedSize = BUCKET_SIZE;
            size = 0;
        }

        //
        // Summary:
        //     Adds an element with the specified key and value into the System.Collections.Generic.SortedDictionary`2.
        //
        // Parameters:
        //   key:
        //     The key of the element to add.
        //
        //   value:
        //     The value of the element to add. The value can be null for reference types.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     key is null.
        //
        //   T:System.ArgumentException:
        //     An element with the same key already exists in the System.Collections.Generic.SortedDictionary`2.
        public void Add(DateTime key, StockDailyValue value)
        {
            if (size > 0 && key < keys[size - 1])
            {
                throw new ArgumentException("Date must be greater than last key");
            }
            if (size == allocatedSize)
            {
                allocatedSize += BUCKET_SIZE;
                var tmpValues = new StockDailyValue[allocatedSize];
                var tmpKeys = new DateTime[allocatedSize];
                for (int i = 0; i < size; i++)
                {
                    tmpValues[i] = values[i];
                    tmpKeys[i] = keys[i];
                }
                this.keys = tmpKeys;
                this.values = tmpValues;
            }
            keys[size] = key;
            values[size] = value;
            size++;
        }

        public void InitRange(IEnumerable<StockDailyValue> dailyValues)
        {
            allocatedSize = dailyValues.Count();
            values = dailyValues.ToArray();
            keys = dailyValues.Select(dv => dv.DATE).ToArray();
            size = allocatedSize;
        }

        //
        // Summary:
        //     Gets or sets the value associated with the specified key.
        //
        // Parameters:
        //   key:
        //     The key of the value to get or set.
        //
        // Returns:
        //     The value associated with the specified key. If the specified key is not found,
        //     a get operation throws a System.Collections.Generic.KeyNotFoundException, and
        //     a set operation creates a new element with the specified key.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     key is null.
        //
        //   T:System.Collections.Generic.KeyNotFoundException:
        //     The property is retrieved and key does not exist in the collection.
        public StockDailyValue this[DateTime key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException();
                int index = IndexOf(key);
                if (index == -1)
                    throw new KeyNotFoundException();
                return values[index];
            }
        }


        public int IndexOf(DateTime date)
        {
            if (size == 0)
            {
                return -1;
            }
            if (date < keys[0]) { return -1; }
            if (date > keys[size - 1]) { return -1; }
            return IndexOfRec(date, 0, size - 1);
        }
        private int IndexOfRec(DateTime date, int startIndex, int endIndex)
        {
            if (startIndex < endIndex)
            {
                if (keys[startIndex] == date)
                {
                    return startIndex;
                }
                if (keys[endIndex] == date)
                {
                    return endIndex;
                }
                int midIndex = (startIndex + endIndex) / 2;
                int comp = date.CompareTo(keys[midIndex]);
                if (comp == 0)
                {
                    return midIndex;
                }
                else if (comp < 0)
                {// 
                    return IndexOfRec(date, startIndex + 1, midIndex - 1);
                }
                else
                {
                    return IndexOfRec(date, midIndex + 1, endIndex - 1);
                }
            }
            else
            {
                if (startIndex == endIndex && keys[startIndex] == date)
                {
                    return startIndex;
                }
                return -1;
            }
        }


        //
        // Summary:
        //     Gets a collection containing the values in the System.Collections.Generic.SortedDictionary`2.
        //
        // Returns:
        //     A System.Collections.Generic.SortedDictionary`2.ValueCollection containing the
        //     values in the System.Collections.Generic.SortedDictionary`2.
        public IEnumerable<StockDailyValue> Values
        {
            get
            {
                return this.values.Take(size);
            }
        }
        //
        // Summary:
        //     Gets a collection containing the keys in the System.Collections.Generic.SortedDictionary`2.
        //
        // Returns:
        //     A System.Collections.Generic.SortedDictionary`2.KeyCollection containing the
        //     keys in the System.Collections.Generic.SortedDictionary`2.
        public IEnumerable<DateTime> Keys
        {
            get
            {
                return this.keys.Take(size);
            }
        }
        //
        // Summary:
        //     Gets the number of key/value pairs contained in the System.Collections.Generic.SortedDictionary`2.
        //
        // Returns:
        //     The number of key/value pairs contained in the System.Collections.Generic.SortedDictionary`2.
        public int Count => size;

        //
        // Summary:
        //     Removes all elements from the System.Collections.Generic.SortedDictionary`2.
        public void Clear()
        {
            values = new StockDailyValue[BUCKET_SIZE];
            keys = new DateTime[BUCKET_SIZE];
            allocatedSize = BUCKET_SIZE;
            size = 0;
        }

        //
        // Summary:
        //     Removes the element with the specified key from the System.Collections.Generic.SortedDictionary`2.
        //
        // Parameters:
        //   key:
        //     The key of the element to remove.
        //
        // Returns:
        //     true if the element is successfully removed; otherwise, false. This method also
        //     returns false if key is not found in the System.Collections.Generic.SortedDictionary`2.
        //
        public bool RemoveLast()
        {
            if (size > 0)
            {
                size--;
                return true;
            }
            else
            {
                return false;
            }
        }

        //
        // Summary:
        //     Determines whether the System.Collections.Generic.SortedDictionary`2 contains
        //     an element with the specified key.
        //
        // Parameters:
        //   key:
        //     The key to locate in the System.Collections.Generic.SortedDictionary`2.
        //
        // Returns:
        //     true if the System.Collections.Generic.SortedDictionary`2 contains an element
        //     with the specified key; otherwise, false.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     key is null.
        public bool ContainsKey(DateTime key)
        {
            return IndexOf(key) >= 0;
        }


    }
}
