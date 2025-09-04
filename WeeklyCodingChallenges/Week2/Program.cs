namespace WeeklyCodingChallenge.Week2;

public static class Program
{
    public static void Main()
    {
        using StreamReader reader = new("testinput.txt");
        string contents = reader.ReadToEnd();
        Console.WriteLine(CalculateValue(contents));
    }

    public static int CalculateValue(string str)
    {
        int sum = 0;

        int start = 0, comma, end;
        while ((start = str.IndexOf("mul(", start)) != -1)
        {
            comma = str.IndexOf(',', start);
            if (comma == -1) break;

            end = str.IndexOf(')', comma);
            if (end == -1) break;

            string firstStr = str[(start + 4)..comma],
                   secondStr = str[(comma + 1)..end];

            start += 4; // Could be optimized.

            if (!int.TryParse(firstStr, out int num1) ||
                !int.TryParse(secondStr, out int num2)) continue;

            sum += num1 * num2;
        }

        return sum;
    }
}
