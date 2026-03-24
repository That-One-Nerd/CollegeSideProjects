using QRCoder;
using System.Collections;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace TOTPGenerator;

internal static class Program
{
    private static readonly byte[] secret = new byte[20];
    private static string totp32 = null!;
    private static string url = null!;
    private static HMACSHA1 hmac = null!;

    private static readonly int codeSize = 6;
    private static int code = 000_000;
    private static DateTimeOffset codeExpire = DateTimeOffset.MinValue;

    private static int sidebarPosition;

    public static async Task Main()
    {
        Console.WriteLine("\n  \e[90mGenerating TOTP...\e[0m");

        Random rand = new();
        rand.NextBytes(secret);

        hmac = new(secret);

        totp32 = Base32Encoding.ToString(secret);
        const string service = "your-service";
        const string email = "email@example.net";
        url = $"otpauth://totp/{service}:{email}?secret={totp32}&issuer=your-service";
        Console.Clear();

        Console.WriteLine($"""

              {"\e[38;5;214m"}TOTP Testing{"\e[0m"}

                {"\e[94m"}Secret Key:{"\e[97m"}     {totp32}{"\e[0m"}
                {"\e[94m"}Secret Key URL:{"\e[97m"} {url}{"\e[0m"}

            """);

        QRCodeData qr = QRCodeGenerator.GenerateQrCode(url, QRCodeGenerator.ECCLevel.Default);
        List<BitArray> qrData = qr.ModuleMatrix;

        int maxLen = 0;
        for (int r = 0; r < qrData.Count; r += 2)
        {
            bool isBottom = r + 1 >= qrData.Count;
            BitArray? dataTop = qrData[r],
                      dataBot = isBottom ? null : qrData[r + 1];
            Console.Write("    \e[30;107m");
            for (int c = 0; c < dataTop.Length; c++)
            {
                bool top = dataTop[c],
                     bot = dataBot?[c] ?? false;

                char toPrint;
                if (top)
                {
                    if (bot) toPrint = '█';
                    else     toPrint = '▀';
                }
                else
                {
                    if (bot) toPrint = '▄';
                    else     toPrint = ' ';
                }
                Console.Write(toPrint);
            }
            if (Console.CursorLeft > maxLen) maxLen = Console.CursorLeft;
            Console.WriteLine("\e[0m");
        }

        sidebarPosition = maxLen + 2;

        Viewer(CancellationToken.None);
        await Task.Delay(-1);
    }

    private static readonly int ValidationTime = 30;
    private static void GenerateCode(int digits)
    {
        // Generate a counter that increments every ValidationTime seconds.
        long seconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
             counter = seconds / ValidationTime;

        byte[] counterBytes = new byte[8];
        ((IBinaryInteger<long>)counter).WriteBigEndian(counterBytes);

        byte[] hash = hmac.ComputeHash(counterBytes);

        int offset = hash[^1] & 0xF;
        int hashNum = ((hash[offset] & 0x7F) << 24) |
                      ((hash[offset + 1] & 0xFF) << 16) |
                      ((hash[offset + 2] & 0xFF) << 8) |
                      ( hash[offset + 3] & 0xFF);

        // Using the secret and the current time, generate the totp.
        int mod = 1;
        for (int i = 0; i < digits; i++) mod *= 10;
        code = hashNum % mod;

        // Time to refresh.
        codeExpire = DateTimeOffset.UnixEpoch + TimeSpan.FromSeconds((counter + 1) * ValidationTime);
    }

    private static readonly string[][] LoadingSequence = [
        ["█▀▀▀▀█", "█     ", "█▄    "],
        ["█▀▀▀▀█", "█    ▀", "█     "],
        ["█▀▀▀▀█", "█    █", "▀     "],
        ["█▀▀▀▀█", "█    █", "     ▀"],
        ["█▀▀▀▀█", "▀    █", "     █"],
        ["█▀▀▀▀█", "     █", "    ▄█"],
        ["▀▀▀▀▀█", "     █", "   ▄▄█"],
        [" ▀▀▀▀█", "     █", "  ▄▄▄█"],
        ["  ▀▀▀█", "     █", " ▄▄▄▄█"],
        ["   ▀▀█", "     █", "▄▄▄▄▄█"],
        ["    ▀█", "     █", "█▄▄▄▄█"],
        ["     █", "▄    █", "█▄▄▄▄█"],
        ["     ▄", "█    █", "█▄▄▄▄█"],
        ["▄     ", "█    █", "█▄▄▄▄█"],
        ["█     ", "█    ▄", "█▄▄▄▄█"],
        ["█▀    ", "█     ", "█▄▄▄▄█"],
        ["█▀▀   ", "█     ", "█▄▄▄▄▄"],
        ["█▀▀▀  ", "█     ", "█▄▄▄▄ "],
        ["█▀▀▀▀ ", "█     ", "█▄▄▄  "],
        ["█▀▀▀▀▀", "█     ", "█▄▄   "]
    ];

    private static int barCounter = 0;
    private static async void Viewer(CancellationToken token)
    {
        Console.CursorVisible = false;
        Console.OutputEncoding = Encoding.Unicode;
        while (true)
        {
            if (codeExpire < DateTime.Now) GenerateCode(codeSize);

            Console.Write("\e[35m");
            Console.SetCursorPosition(sidebarPosition, 6);
            string[] loader = LoadingSequence[barCounter = (barCounter + 1) % LoadingSequence.Length];
            for (int i = 0; i < loader.Length; i++)
            {
                Console.SetCursorPosition(sidebarPosition, 6 + i);
                Console.Write(loader[i]);
            }
            Console.Write("\e[0m");

            string codeStr;
            if (codeSize == 8) codeStr = $"{code / 10000:0000} {code % 10000:0000}";  // 8-digit code.
            else if (codeSize == 6) codeStr = $"{code / 1000:000} {code % 1000:000}"; // 6-digit code.
            else codeStr = $"{code}";                                                 // Generic

            Console.SetCursorPosition(sidebarPosition, 10);
            Console.Write($"\e[90mCode: \e[94m{codeStr}\e[0m");

            double timeLeft = (codeExpire - DateTime.Now).TotalSeconds,
                   percentLeft = timeLeft / ValidationTime;

            Console.SetCursorPosition(sidebarPosition, 11);
            int barLength = Console.WindowWidth - sidebarPosition - 5,
                barActive = int.Max(0, (int)(barLength * percentLeft));
            StringBuilder bar = new("<");

            if (timeLeft > 10) bar.Append("\e[32m");
            else if (timeLeft > 5) bar.Append("\e[33m");
            else if (timeLeft > 0) bar.Append("\e[31m");
            bar.Append('=', barActive).Append(' ', barLength - barActive);
            bar.Append("\e[0m>");
            Console.Write(bar);

            await Task.Delay(1000 / 10, token);
        }
    }
}
