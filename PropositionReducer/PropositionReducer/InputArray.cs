using System.Collections;
using System.Text;

namespace PropositionReducer;

public class InputArray(char[] names) : IEnumerable<KeyValuePair<char, bool>>
{
    private readonly char[] names = names;
    private readonly BitArray values = new(names.Length);

    public bool this[char name]
    {
        get => Get(name);
        set => Set(name, value);
    }

    public IEnumerator<KeyValuePair<char, bool>> GetEnumerator()
    {
        for (int i = 0; i < names.Length; i++) yield return new(names[i], values[i]);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Get(char name)
    {
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i] == name) return values[i];
        }
        throw new ArgumentOutOfRangeException(nameof(name));
    }
    public void Set(char name, bool value)
    {
        for (int i = 0; i < names.Length; i++)
        {
            if (names[i] == name)
            {
                values[i] = value;
                return;
            }
        }
        throw new ArgumentOutOfRangeException(nameof(name));
    }

    public void SetAll(BitArray values)
    {
        if (this.values.Length != values.Length) throw new ArgumentOutOfRangeException(nameof(values));
        for (int i = 0; i < values.Length; i++) this.values[i] = values[i];
    }
    public void SetAll(bool[] values)
    {
        if (this.values.Length != values.Length) throw new ArgumentOutOfRangeException(nameof(values));
        for (int i = 0; i < values.Length; i++) this.values[i] = values[i];
    }

    public override string ToString()
    {
        StringBuilder result = new("[");
        for (int i = 0; i < names.Length; i++)
        {
            result.Append($" {names[i]}={(values[i] ? "T" : "F")}");
            if (i < names.Length - 1) result.Append(',');
        }
        return result.Append(" ]").ToString();
    }
    public string NamesToString()
    {
        StringBuilder result = new("[");
        for (int i = 0; i < names.Length; i++)
        {
            result.Append($" {names[i]}");
            if (i < names.Length - 1) result.Append(',');
        }
        return result.Append(" ]").ToString();
    }
}
