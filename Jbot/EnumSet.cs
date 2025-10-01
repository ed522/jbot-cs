using System.Collections;
using System.Numerics;

namespace Jbot;

public class EnumSet<T> : ISet<T> where T : struct, Enum
{

    private class EnumSetEnumerator : IEnumerator<T>
    {
        // undefined if index == -1 -> past end of list
        public T Current => index != -1 ? reference.possibleValues[index] : default;
        private readonly EnumSet<T> reference;
        object IEnumerator.Current => Current;
        private int index = 0;

        public EnumSetEnumerator(EnumSet<T> reference)
        {
            this.reference = reference;
            // scan through to first entry
            while (!reference.Contains(reference.possibleValues[index])) index++;
        }

        public void Dispose()
        {
            // nothing to do
        }

        public bool MoveNext()
        {
            try
            {
                do index++;
                while (!reference.Contains(reference.possibleValues[index]));
                return true;
            }
            catch (IndexOutOfRangeException)
            {
                index = -1;
                return false;
            }
        }

        public void Reset()
        {
            index = 0;
            while (!reference.Contains(reference.possibleValues[index])) index++;
        }
    }

    private ulong entries;
    private ulong[] largeEntries;
    private T[] possibleValues;

    private readonly bool isReadOnly;
    private readonly EnumSet<T>? parent;

    private void SetEntry(T value)
    {
        // find the value
        int index = possibleValues
            .AsParallel()
            .Where(t => t.Equals(value))
            .Index().First().Index;

        // check which one to use
        if (largeEntries.Length != 0)
        {
            // Large array created, so we should use it.
            // Get offsets and shift a value in
            int arrayIndex = index / sizeof(ulong); // rounding down
            int bitIndex = index % sizeof(ulong);
            largeEntries[arrayIndex] |= (ushort)(0x1 << bitIndex);
        }
        else
        {
            // Use the single integer.
            entries |= (ushort)(0x1 << index);
        }
    }
    private bool GetEntry(T value)
    {
        // find the value
        int index = possibleValues
            .AsParallel()
            .Where(t => t.Equals(value))
            .Index().First().Index;

        if (largeEntries.Length != 0)
        {
            // as above
            int arrayIndex = index / sizeof(ulong); // rounding down
            int bitIndex = index % sizeof(ulong);
            return (largeEntries[arrayIndex] & (ushort)(0x1 << bitIndex)) != 0;
        }
        else
        {
            return (entries & (ushort)(0x1 << index)) != 0;
        }
    }
    private void UnsetEntry(T value)
    {
        // find the value
        int index = possibleValues
            .AsParallel()
            .Where(t => t.Equals(value))
            .Index().First().Index;

        // check which one to use
        if (largeEntries.Length != 0)
        {
            int arrayIndex = index / sizeof(ulong); // rounding down
            int bitIndex = index % sizeof(ulong);
            // mask out
            largeEntries[arrayIndex] &= (ushort)~(0x1 << bitIndex);
        }
        else
        {
            // mask out the entry to remove
            entries &= (ushort)~(0x1 << index);
        }
    }
    public EnumSet()
    {
        // Look up all of the values we could hold to speed up some computation
        possibleValues = Enum.GetValues<T>();

        // If there are 64 or fewer entries, we can use a single ulong.
        // If there are more than that, we need the array.

        // If (len) is greater than ulong size (* 8, bytes to bits), use the largeEntries array.
        if (possibleValues.Length > sizeof(ulong) * 8)
            largeEntries = new ulong[possibleValues.Length / (sizeof(ulong) * 8)];
        // Else make it 0 entries long.
        else largeEntries = [];

        // normal set
        isReadOnly = false;
        parent = null;
    }
    private EnumSet(EnumSet<T> parent)
    {
        this.largeEntries = [];
        this.possibleValues = [];
        this.entries = 0;
        this.isReadOnly = true;
        this.parent = parent;
    }

    public int Count
    {
        get
        {
            if (!isReadOnly)
            {
                // check word size
                if (nuint.MaxValue == 0xFF_FF_FF_FF)
                {
                    if (largeEntries.Length == 0)
                        return BitOperations.PopCount(entries & 0xFFFFFFFF)
                            + BitOperations.PopCount((entries >> 32) & 0xFFFFFFFF);
                    else
                    {
                        int val = 0;
                        foreach (ulong i in largeEntries)
                        {
                            val += BitOperations.PopCount(i & 0xFFFFFFFF)
                                + BitOperations.PopCount((i >> 32) & 0xFFFFFFFF);
                        }
                        return val;
                    }
                }
                else
                {
                    if (largeEntries.Length == 0)
                    {
                        return BitOperations.PopCount(entries);
                    }
                    else
                    {
                        int val = 0;
                        foreach (ulong i in largeEntries)
                        {
                            val += BitOperations.PopCount(i);
                        }
                        return val;
                    }
                }
            }
            else
            {
                return parent!.Count;
            }
        }
    }

    public bool IsReadOnly => false;

    public bool Add(T item)
    {
        if (!isReadOnly)
        {
            bool alreadyHad = GetEntry(item);
            SetEntry(item);
            return !alreadyHad;
        }
        else
        {
            return parent!.Add(item);
        }
    }

    public void Clear()
    {
        if (isReadOnly)
        {
            if (largeEntries.Length != 0)
            {
                int len = largeEntries.Length;
                largeEntries = new ulong[len];
            }
            else
            {
                entries = 0;
            }
        }
        else
        {
            parent!.Clear();
        }
    }

    public bool Contains(T item)
    {
        if (!isReadOnly) return GetEntry(item);
        else return parent!.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        int i = arrayIndex;
        foreach (T t in this)
        {
            array[i] = t;
            i++;
        }
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        foreach (T t in other)
        {
            Remove(t);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new EnumSetEnumerator(this);
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        foreach (T t in this)
        {
            if (!other.Contains(t)) this.Remove(t);
        }
    }

    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        return IsSubsetOf(other) && !this.SetEquals(other);
    }

    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        return IsSupersetOf(other) && !this.SetEquals(other);
    }

    public bool IsSubsetOf(IEnumerable<T> other)
    {
        foreach (T t in this)
        {
            if (!other.Contains(t)) return false;
        }
        return true;
    }

    public bool IsSupersetOf(IEnumerable<T> other)
    {
        foreach (T t in other)
        {
            if (!this.Contains(t)) return false;
        }
        return true;
    }

    public bool Overlaps(IEnumerable<T> other)
    {
        foreach (T t in this)
        {
            if (other.Contains(t)) return true;
        }
        return false;
    }

    public bool Remove(T item)
    {
        if (!isReadOnly)
        {
            bool had = this.GetEntry(item);
            this.UnsetEntry(item);
            return had;
        }
        else
        {
            return parent!.Remove(item);
        }
    }

    public bool SetEquals(IEnumerable<T> other)
    {
        foreach (T t in other)
        {
            if (!this.Contains(t)) return false;
        }
        foreach (T t in this)
        {
            if (!other.Contains(t)) return false;
        }
        return true;
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        foreach (T t in other)
        {
            if (!this.Add(t)) this.Remove(t);
        }
    }

    public void UnionWith(IEnumerable<T> other)
    {
        foreach (T t in other)
        {
            this.Add(t);
        }
    }

    void ICollection<T>.Add(T item)
    {
        this.Add(item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public EnumSet<T> AsReadOnly()
    {
        // turns out, it does change semantics
        // this constructor is specific
        #pragma warning disable IDE0028 // Simplify collection initialization
        return new EnumSet<T>(this);
        #pragma warning restore IDE0028 // Simplify collection initialization
    }
    
}