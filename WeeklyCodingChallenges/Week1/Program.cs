namespace WeeklyCodingChallenge.Week1;

public static class Program
{
    public static void Main()
    {
        using StreamReader reader = new("testinput.txt");
        string input = reader.ReadToEnd();
        Console.WriteLine(Calculate(input));
    }

    private static int Calculate(string val)
    {
        int result = 0;
        for (int i = 0; i < val.Length; i++)
        {
            char c1 = val[i],
                 c2 = val[(i + 1) % val.Length];
            if (c1 == c2) result += int.Parse(c1.ToString());
        }
        return result;
    }
}
