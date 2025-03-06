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

            uint rotated = CascadeRotate16InUint(temp);

            operatedBytes[i] = (char)(rotated ^ key ^ i ^ key2 ^ key3);
        }
        return operatedBytes;
    }

    public static uint Rotate16Bit(uint val)
    {
        return ((val << 8) | (val >> 8)) & 0xFFFFu;
    }

    public static uint Rotate16Bit4S(uint val)
    {
        uint mask = 0xF0F0u;              // the bits we want to rotate
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

    public static ushort CascadeRotate(ushort x)
    {
        // Stage 1: Swap the two bytes (rotate by 8 bits).
        // This is an involution: doing it twice returns the original.
        ushort swappedBytes = (ushort)(((x & 0x00FF) << 8) | ((x & 0xFF00) >> 8));

        // Stage 2: For each 8-bit half, swap the nibbles (rotate by 4 bits).
        // Process the high and low bytes independently.
        byte high = (byte)((swappedBytes >> 8) & 0xFF);
        byte low = (byte)(swappedBytes & 0xFF);
        byte rotatedHigh = (byte)(((high << 4) | (high >> 4)) & 0xFF);
        byte rotatedLow = (byte)(((low << 4) | (low >> 4)) & 0xFF);

        ushort afterNibbleSwap = (ushort)((rotatedHigh << 8) | rotatedLow);

        // Stage 3 (Optional): For each 4-bit nibble, swap its two 2-bit groups (rotate by 2 bits).
        // There are 4 nibbles in a 16-bit number.
        ushort result = 0;
        for (int nibbleIndex = 0; nibbleIndex < 4; nibbleIndex++)
        {
            // Extract nibble
            ushort nibble = (ushort)((afterNibbleSwap >> (nibbleIndex * 4)) & 0xF);
            // Swap the two 2-bit groups in the nibble.
            // (A 4-bit rotation by 2 is its own inverse.)
            nibble = (ushort)(((nibble << 2) | (nibble >> 2)) & 0xF);
            // Put the transformed nibble back into its place.
            result |= (ushort)(nibble << (nibbleIndex * 4));
        }

        return result;
    }

    public static uint CascadeRotate16InUint(uint val)
    {
        // Constrain the input to 16 bits.
        ushort lower16 = (ushort)(val & 0xFFFF);
        ushort transformed = CascadeRotate(lower16);
        // Reassemble, preserving any upper bits if needed.
        return (val & 0xFFFF0000) | transformed;
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
