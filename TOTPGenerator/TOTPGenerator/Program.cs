using QRCoder;
using System.Collections;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace TOTPGenerator;

// Hi Orion!
// 
// Here's the TOTP generator prototype. Of course it's written in C# but I'm
// fairly certain that it wouldn't be very difficult to port to Java. There are
// four tricky things that need to be done:
// 
// - Compute HMAC SHA1 on Java (doable, from my googling)
//   https://stackoverflow.com/questions/6312544/hmac-sha1-how-to-do-it-properly-in-java
//
// - Write long in big-endian notation (worst case scenario we can reverse the
//   bytes ourselves)
//
// - Convert a number to base-32 (doable with a dependency, maybe also doable
//   without)
//   https://stackoverflow.com/questions/21515479/encode-string-to-base32-string-in-java
//
// - Generate a QR code in Angular (haven't googled, but I'm certain it can be
//   done)
//
// Check around if you'd like. It's really not super difficult. I've added
// comments to hopefully make it easier to parse.

internal static class Program
{
    private static readonly byte[] secret = new byte[20]; // 20-byte private key, shared via QR
    private static string totp32 = null!;                 // base32-formatted private key.
    private static string url = null!;                    // url-formatted private key.
    private static HMACSHA1 hmac = null!;                 // HMAC SHA1. the backbone.

    private static readonly int codeSize = 6;             // how many digits to make our code.
    private static int code = 000_000;                    // live update for the code.
    private static DateTimeOffset codeExpire = DateTimeOffset.MinValue; // how long till refresh.

    public static async Task Main()
    {
        Console.WriteLine("\n  \e[90mTOTP Generation Test...\e[0m");

        // Generate our secret. The secret key is expected to be
        // any 20-byte combination. When sent as a qr code, it follows
        // the format of:
        // otpauth://totp/service:email?secret=KEY_AS_BASE32&issuer=your-service
        // however: it can be shortened to as little as:
        // otpauth://totp?secret=KEY_AS_BASE32
        Random rand = new();
        rand.NextBytes(secret);

        // Prepare the HMAC SHA1 algorithm
        hmac = new(secret);

        Viewer(CancellationToken.None);
        await Task.Delay(-1);
    }

    // There's a lot of variables that can be tweaked,
    // but that very likely never will be.
    // - ValidationTime is how often the code refreshes, but it's standard to be 30 seconds.
    // - The digits can be customized, but it's basically always 6.
    private static readonly int ValidationTime = 30;
    private static void GenerateCode(int digits)
    {
        // Generate a counter that increments every ValidationTime seconds.
        long seconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
             counter = seconds / ValidationTime;

        // Convert the counter to 8 bytes in big-endian notation.
        byte[] counterBytes = new byte[8];
        ((IBinaryInteger<long>)counter).WriteBigEndian(counterBytes);

        // Compute the hash using the private key previously generated.
        byte[] hash = hmac.ComputeHash(counterBytes);

        // Weird. I found this part through a different online implementation.
        // Compute the 32-bit output based on picking a section of the computed
        // hash.
        int offset = hash[^1] & 0xF;
        int hashNum = ((hash[offset]     & 0x7F) << 24) |
                      ((hash[offset + 1] & 0xFF) << 16) |
                      ((hash[offset + 2] & 0xFF) <<  8) |
                      ( hash[offset + 3] & 0xFF);

        // Using the secret and the current time, generate the totp.
        int mod = 1;
        for (int i = 0; i < digits; i++) mod *= 10;
        code = hashNum % mod;

        // Time to refresh.
        codeExpire = DateTimeOffset.UnixEpoch + TimeSpan.FromSeconds((counter + 1) * ValidationTime);
    }

    // And that's it! Not too bad, I'd say.
    // Probably doable to port to our app.







    // The rest of the code below this point is for the visualizer.
    // Don't bother to look at it lol.

    #region Visualizer
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

    private static int sidebarPosition;
    private static int barCounter = 0;
    private static async void Viewer(CancellationToken token)
    {
        Console.CursorVisible = false;
        Console.OutputEncoding = Encoding.Unicode;

        totp32 = Base32Encoding.ToString(secret);
        const string service = "your-service";
        const string email = "email@example.net";
        url = $"otpauth://totp/{service}:{email}?secret={totp32}&issuer=your-service";
        url = $"otpauth://totp?secret={totp32}";
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
                    else toPrint = '▀';
                }
                else
                {
                    if (bot) toPrint = '▄';
                    else toPrint = ' ';
                }
                Console.Write(toPrint);
            }
            if (Console.CursorLeft > maxLen) maxLen = Console.CursorLeft;
            Console.WriteLine("\e[0m");
        }

        sidebarPosition = maxLen + 2;

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
    #endregion
}
