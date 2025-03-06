using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace TREETOPNEO;

public static class Encryption
{
    public static string Encrypt(string text, string key)
    {
        GetKeyShift(key, out uint keyShift, out uint keyShift2, out uint keyShift3);
        char[] textBytes = text.ToCharArray();
        string encryptedText = new(BitOp(textBytes, keyShift));
        textBytes = encryptedText.ToCharArray();
        encryptedText = new(BitOpLayer2(textBytes, keyShift, keyShift2, keyShift3));
        return encryptedText;
    }
    public static string Decrypt(string text, string key)
    {
        GetKeyShift(key, out uint keyShift, out uint keyShift2, out uint keyShift3);
        char[] textBytes = text.ToCharArray();
        string decryptedText = new(BitOp(textBytes, keyShift));
        textBytes = decryptedText.ToCharArray();
        decryptedText = new(BitOpLayer2(textBytes, keyShift, keyShift2, keyShift3));
        return decryptedText;
    }

    public static char[] BitOp(char[] textBytes, uint key)
    {
        char[] operatedBytes = new char[textBytes.Length];
        for (int i = 0; i < textBytes.Length; i++)
        {
            // Incorporate both the key and the index (i) into the operation.
            // XOR is self-inverse: a ^ key ^ key = a.
            uint value = textBytes[i];
            uint temp = (uint)(value ^ key ^ i);

            // Rotate left by 8 bits (for a 16-bit char this swaps the high and low bytes).
            // For 16-bit values, a rotation by 8 is self-inverse.
            uint rotated = Rotate16Bit(temp);

            // Apply the same XOR combination after the rotation.
            operatedBytes[i] = (char)(rotated ^ key ^ i);
        }
        return operatedBytes;
    }

    public static char[] BitOpLayer2(char[] textBytes, uint key, uint key2, uint key3)
    {
        char[] operatedBytes = new char[textBytes.Length];
        for (int i = 0; i < textBytes.Length; i++)
        {
            uint value = textBytes[i];
            uint temp = (uint)(value ^ key ^ i ^ key2 ^ key3);

            uint rotated = Rotate16Bit4S(temp);
            rotated = Rotate16Bit(temp);

            operatedBytes[i] = (char)(rotated ^ key ^ i ^ key2 ^ key3);
        }
        return operatedBytes;
    }

    public static uint Rotate16Bit(uint val)
    {
        return ((val << 8) | (val >> 8)) & 0xFFFF;
    }

    public static uint Rotate16Bit4S(uint val)
    {
        uint mask = 0xF0F0;              // the bits we want to rotate
        uint notMask = ~mask;           // the bits we want to leave intact

        // Extract the bits we intend to rotate.
        uint extracted = val & mask;

        // Rotate only the extracted bits by 8 positions,
        // but note: you need to be careful to only move bits within the positions allowed by the mask.
        // One approach is to shift them and then reposition them according to the mask's layout.
        // Here we assume an 8-bit rotation is valid if the mask’s bits are laid out in an 8-bit periodic pattern.
        uint rotatedExtracted = ((extracted << 8) | (extracted >> 8)) & mask;

        // Combine the rotated subset with the bits that were not part of the mask.
        return rotatedExtracted | (val & notMask);
    }

    public static uint Rotate16Bit2S(uint val)
    {
        uint mask = 0xF000;              // the bits we want to rotate
        uint notMask = ~mask;           // the bits we want to leave intact

        // Extract the bits we intend to rotate.
        uint extracted = val & mask;

        // Rotate only the extracted bits by 8 positions,
        // but note: you need to be careful to only move bits within the positions allowed by the mask.
        // One approach is to shift them and then reposition them according to the mask's layout.
        // Here we assume an 8-bit rotation is valid if the mask’s bits are laid out in an 8-bit periodic pattern.
        uint rotatedExtracted = ((extracted << 8) | (extracted >> 8)) & mask;

        // Combine the rotated subset with the bits that were not part of the mask.
        return rotatedExtracted | (val & notMask);
    }

    public static uint Rotate8Bit(uint val)
    {
        return ((val << 4) | (val >> 4)) & 0xFF;
    }

    public static uint Rotate4Bit(uint val)
    {
        return ((val << 2) | (val >> 2)) & 0xF;
    }

    public static void GetKeyShift(string key, out uint keyShift, out uint keyShift2, out uint keyShift3)
    {
        keyShift = 0;
        keyShift2 = 0;
        keyShift3 = 0;
        foreach (char c in key.ToCharArray())
        {
            keyShift += (ushort)c;
            keyShift2 += (ushort)c | keyShift;
            keyShift3 = keyShift3 & (keyShift | keyShift2!) ^ keyShift | keyShift3;
        }
    }
}

public static class Rand
{
    private static Random r = new();
    public static string charset = "`1234567890-=~!@#$%^&*()_+qwertyuiop[]\\QWERTYUIOP{}|asdfghjkl;'ASDFGHJKL:\"zxcvbnm,./ZXCVBNM<>?";

    public static string RandomString(int length)
    {
        return new string(Enumerable.Repeat(charset, length)
        .Select(s => s[r.Next(s.Length)]).ToArray());
    }
}

public static class SringOperations
{
    public static string PadBoth(string source, int length)
    {
        int spaces = length - source.Length;
        int padLeft = spaces / 2 + source.Length;
        return source.PadLeft(padLeft, '-').PadRight(length, '-');

    }
}

public static class OperatingSystem
{
    public static bool IsWindows() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool IsMacOS() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static bool IsLinux() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
}

public static class Clipboard
{
    public static void Copy(string val)
    {
        if (OperatingSystem.IsWindows())
        {
            $"echo {val} | clip".Bat();
        }

        if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
        {
            $"echo \"{val}\" | pbcopy".Bash();
        }
    }
}

public static class Shell
{
    public static string Bash(this string cmd)
    {
        var escapedArgs = cmd.Replace("\"", "\\\"");
        string result = Run("/bin/bash", $"-c \"{escapedArgs}\"");
        return result;
    }

    public static string Bat(this string cmd)
    {
        var escapedArgs = cmd.Replace("\"", "\\\"");
        string result = Run("cmd.exe", $"/c \"{escapedArgs}\"");
        return result;
    }

    private static string Run(string filename, string arguments)
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = false,
            }
        };
        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return result;
    }
}
