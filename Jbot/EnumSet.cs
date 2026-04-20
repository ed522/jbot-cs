using System.Collections;
using System.Numerics;
using System.Text;

namespace Jbot;

public class EnumSet<T> : ISet<T> where T : struct, Enum
{
    private class EnumSetEnumerator : IEnumerator<T>
    {
        public T Current => (index >= 0 && index < reference.possibleValues.Length) 
            ? reference.possibleValues[index] 
            : default;

        private readonly EnumSet<T> reference;
        object IEnumerator.Current => Current;
        private int index = -1;

        public EnumSetEnumerator(EnumSet<T> reference)
        {
            this.reference = reference;
        }

        public void Dispose()
        {
            // nothing to do
        }

        public bool MoveNext()
        {
            while (++index < reference.possibleValues.Length)
            {
                if (reference.Contains(reference.possibleValues[index]))
                    return true;
            }
            return false;
        }

        public void Reset()
        {
            index = -1;
        }
    }

    private ulong entries;
    private ulong[] largeEntries;
    private T[] possibleValues;

    private readonly EnumSet<T>? parent;
    public bool IsReadOnly { get; private set; }
    public int Count
    {
        get
        {
            if (!IsReadOnly)
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
            else
            {
                return parent!.Count;
            }
        }
    }

    private void SetEntry(T value)
    {
        int index = Array.IndexOf(possibleValues, value);
        if (index == -1) return;

        if (largeEntries.Length != 0)
        {
            int arrayIndex = index / 64;
            int bitIndex = index % 64;
            largeEntries[arrayIndex] |= (1UL << bitIndex);
        }
        else
        {
            entries |= (1UL << index);
        }
    }

    private bool GetEntry(T value)
    {
        int index = Array.IndexOf(possibleValues, value);
        if (index == -1) return false;

        if (largeEntries.Length != 0)
        {
            int arrayIndex = index / 64;
            int bitIndex = index % 64;
            return (largeEntries[arrayIndex] & (1UL << bitIndex)) != 0;
        }
        else
        {
            return (entries & (1UL << index)) != 0;
        }
    }

    private void UnsetEntry(T value)
    {
        int index = Array.IndexOf(possibleValues, value);
        if (index == -1) return;

        if (largeEntries.Length != 0)
        {
            int arrayIndex = index / 64;
            int bitIndex = index % 64;
            largeEntries[arrayIndex] &= ~(1UL << bitIndex);
        }
        else
        {
            entries &= ~(1UL << index);
        }
    }

    public EnumSet()
    {
        possibleValues = Enum.GetValues<T>();

        if (possibleValues.Length > 64)
            largeEntries = new ulong[(possibleValues.Length + 63) / 64];
        else 
            largeEntries = [];

        IsReadOnly = false;
        parent = null;
    }
    private EnumSet(EnumSet<T> parent)
    {
        this.largeEntries = [];
        this.possibleValues = [];
        this.entries = 0;
        this.IsReadOnly = true;
        this.parent = parent;
    }

    public bool Add(T item)
    {
        if (!IsReadOnly)
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
        if (IsReadOnly) throw new NotSupportedException("Set is read-only.");
        
        if (largeEntries.Length != 0)
        {
            Array.Clear(largeEntries);
        }
        else
        {
            entries = 0;
        }
    }

    public bool Contains(T item)
    {
        if (!IsReadOnly) return GetEntry(item);
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
        if (!IsReadOnly)
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
        #pragma warning disable IDE0028 // Simplify collection initialization
        return new EnumSet<T>(this);
        #pragma warning restore IDE0028 // Simplify collection initialization
    }

    /// <summary>
    /// Creates a string representation of this object (a list of each individual entry).
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        StringBuilder builder = new();
        builder.Append('{');

        foreach (T t in this)
        {
            builder.Append(t.ToString());
            builder.Append(',');
        }
        builder[^1] = '}'; // replace last comma
        return builder.ToString();
    }
    
}