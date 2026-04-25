using System.Collections;
using System.Numerics;
using System.Text;

namespace Jbot;

public class EnumSet<T> : ISet<T> where T : struct, Enum
{
    private readonly ulong[] largeEntries;

    private readonly EnumSet<T>? parent;
    private readonly T[] possibleValues;

    private ulong entries;

    public EnumSet()
    {
        this.possibleValues = Enum.GetValues<T>();

        if (this.possibleValues.Length > 64)
        {
            this.largeEntries = new ulong[(this.possibleValues.Length + 63) / 64];
        }
        else
        {
            this.largeEntries = [];
        }

        this.IsReadOnly = false;
        this.parent = null;
    }

    private EnumSet(EnumSet<T> parent)
    {
        this.largeEntries = [];
        this.possibleValues = [];
        this.entries = 0;
        this.IsReadOnly = true;
        this.parent = parent;
    }

    public bool IsReadOnly { get; }

    public int Count
    {
        get
        {
            // delegation case
            if (this.IsReadOnly)
            {
                return this.parent!.Count;
            }

            // >64 entries case
            if (this.largeEntries.Length != 0)
            {
                return this.largeEntries.Sum(BitOperations.PopCount);
            }

            // short case
            return BitOperations.PopCount(this.entries);
        }
    }

    public bool Add(T item)
    {
        this.CheckReadOnly();

        bool alreadyHad = this.GetEntry(item);
        this.SetEntry(item);
        return !alreadyHad;
    }

    public void Clear()
    {
        this.CheckReadOnly();

        if (this.largeEntries.Length != 0)
        {
            Array.Clear(this.largeEntries);
        }
        else
        {
            this.entries = 0;
        }
    }

    public bool Contains(T item)
    {
        if (this.IsReadOnly)
        {
            return this.parent!.Contains(item);
        }

        return this.GetEntry(item);
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
            this.Remove(t);
        }
    }

    public IEnumerator<T> GetEnumerator() => new EnumSetEnumerator(this);

    public void IntersectWith(IEnumerable<T> other)
    {
        T[] enumerable = other as T[] ?? other.ToArray();

        foreach (T t in this)
        {
            if (!enumerable.Contains(t)) this.Remove(t);
        }
    }

    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        T[] set = other.ToArray();
        return this.IsSubsetOf(set) && !this.SetEquals(set);
    }

    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        T[] set = other.ToArray();
        return this.IsSupersetOf(set) && !this.SetEquals(set);
    }

    public bool IsSubsetOf(IEnumerable<T> other)
    {
        T[] set = other.ToArray();
        return this.All(set.Contains);
    }

    public bool IsSupersetOf(IEnumerable<T> other) => other.All(this.Contains);

    public bool Overlaps(IEnumerable<T> other) => other.Any(this.Contains);

    public bool Remove(T item)
    {
        this.CheckReadOnly();

        bool had = this.GetEntry(item);
        this.UnsetEntry(item);
        return had;
    }

    public bool SetEquals(IEnumerable<T> other)
    {
        T[] enumerable = other as T[] ?? other.ToArray();

        if (enumerable.Any(t => !this.Contains(t)))
        {
            return false;
        }

        return this.All(enumerable.Contains) && enumerable.All(this.Contains);
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

    void ICollection<T>.Add(T item) { this.Add(item); }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    private void SetEntry(T value)
    {
        int index = Array.IndexOf(this.possibleValues, value);
        if (index == -1) return;

        if (this.largeEntries.Length != 0)
        {
            int arrayIndex = index / 64;
            int bitIndex = index % 64;
            this.largeEntries[arrayIndex] |= 1UL << bitIndex;
        }
        else
        {
            this.entries |= 1UL << index;
        }
    }

    private bool GetEntry(T value)
    {
        int index = Array.IndexOf(this.possibleValues, value);
        if (index == -1) return false;

        if (this.largeEntries.Length != 0)
        {
            int arrayIndex = index / 64;
            int bitIndex = index % 64;
            return (this.largeEntries[arrayIndex] & (1UL << bitIndex)) != 0;
        }

        return (this.entries & (1UL << index)) != 0;
    }

    private void UnsetEntry(T value)
    {
        int index = Array.IndexOf(this.possibleValues, value);
        if (index == -1) return;

        if (this.largeEntries.Length != 0)
        {
            int arrayIndex = index / 64;
            int bitIndex = index % 64;
            this.largeEntries[arrayIndex] &= ~(1UL << bitIndex);
        }
        else
        {
            this.entries &= ~(1UL << index);
        }
    }

    private void CheckReadOnly()
    {
        if (this.IsReadOnly)
        {
            throw new NotSupportedException("Cannot modify a read-only set");
        }
    }

    public EnumSet<T> AsReadOnly()
    {
#pragma warning disable IDE0028 // Simplify collection initialization
        return new EnumSet<T>(this);
#pragma warning restore IDE0028 // Simplify collection initialization
    }

    /// <summary>
    ///     Creates a string representation of this object (a list of each individual entry).
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

    private class EnumSetEnumerator(EnumSet<T> reference) : IEnumerator<T>
    {
        private int index = -1;

        public T Current => this.index >= 0 && this.index < reference.possibleValues.Length
            ? reference.possibleValues[this.index]
            : default(T);

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {
            // nothing to do
        }

        public bool MoveNext()
        {
            while (++this.index < reference.possibleValues.Length)
            {
                if (reference.Contains(reference.possibleValues[this.index]))
                {
                    return true;
                }
            }

            return false;
        }

        public void Reset() { this.index = -1; }
    }
}
