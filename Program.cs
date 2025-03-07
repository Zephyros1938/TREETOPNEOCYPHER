using System;
using System.Linq;
using System.Text;

public class Program
{
    static bool copyToClipBoard = false;
    public static int Main(string[] args)
    {
        //var u = TREETOPNEO.Encryption.CascadeRotate16InUint(0b0101001100001111u);
        //Console.WriteLine("{0:B16}, {1:B16}", u, TREETOPNEO.Encryption.CascadeRotate16InUint(u));

        if (args.Contains("-c") || args.Contains("--clipboard"))
        {
            copyToClipBoard = true;
        }
        // Display help if requested.
        if (args.Contains("-h") || args.Contains("--help"))
        {
            Help();
            return 0;
        }

        // Check if the correct number of arguments (pairs) is provided.
        if (args.Contains("-s") || args.Contains("--string") || args.Contains("-k") || args.Contains("--key"))
        {
            string? key = null;
            string? text = null;
            //Console.WriteLine("Must provide a Key and a String to encrypt.");
            // Process arguments in pairs.
            for (int i = 0; i < 4; i += 2)
            {
                string option = args[i];
                string value = args[i + 1];

                if (option == "-k" || option == "--key")
                {
                    if (key != null)
                    {
                        Console.WriteLine("Key has already been provided.");
                        return -1;
                    }
                    key = value;
                }
                else if (option == "-s" || option == "--string")
                {
                    if (text != null)
                    {
                        Console.WriteLine("String has already been provided.");
                        return -1;
                    }
                    text = value;
                }
                else
                {
                    Console.WriteLine($"Unrecognized option: {option}");
                    return -1;
                }
            }

            // Check if both key and text have been set.
            if (key == null || text == null)
            {
                Console.WriteLine("Both key and string must be provided.");
                return -1;
            }

            //Console.WriteLine($"DATA          :\n\tTEXT: {text}\n\tKEY : {key}");
            string encrypted = TREETOPNEO.Encryption.Encrypt(text, key);
            string decrypted = TREETOPNEO.Encryption.Decrypt(encrypted, key);
            byte[] encryptedBytes = Encoding.UTF8.GetBytes(encrypted);
            Console.WriteLine(key.Length%3);
            Console.WriteLine($"ENCRYPTED DATA:\n\tENCR: {encrypted}\n\tDECR: {decrypted}\n\tBYTE: {string.Join("", encryptedBytes.Select(b => string.Format("{0:X2}", b)))}");
            return 0;
        }
        if (args.Contains("-t") || args.Contains("--test"))
        {
            int testCount = 10;// = 200;
            if (args.Length == 2)
            {
                if (int.TryParse(args[1], out int res))
                {
                    testCount = res;
                }
            }
            for (int i = 0; i < testCount; i++)
            {
                string initString = TREETOPNEO.Rand.RandomString(50);
                string key = TREETOPNEO.Rand.RandomString(50);
                string encrypted = TREETOPNEO.Encryption.Encrypt(initString, key);
                string decrypted = TREETOPNEO.Encryption.Decrypt(encrypted, key);
                Console.WriteLine($"{TREETOPNEO.SringOperations.PadBoth($" TEST No.{i + 1} ", 50)}\n\tKEY : {key}\n\tTEXT: {initString}\n\tENCR: {encrypted}\n\tDECR: {decrypted}");
                Console.Write($"\n\tSUCCESS? ");
                if (decrypted == initString)
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                }
                Console.Write("  ");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write('\n');
            }
        }

        return 0;

    }

    public static void Help()
    {
        Console.WriteLine(
"""
NAME
    TREETOPNEOCYPHER

SYNOPSIS
    TREETOPNEOCYPHER [OPTION] 

DESCRIPTION
    An application that is used by @TREETOPNEO to encrypt their Twitter/X messages.
    May function incorrectly on Windows Powershell. It is recommended to use the Windows Command Prompt.

OPTIONS
    -k, --key[=KEY]
        Provides the KEY value to the encryptor.
    
    -s, --string[=STRING]
        Provides the STRING value to the encryptor.
    
    -t, --test [COUNT]
        Automated test, checks for lossless decryption. Optionally specify the number of tests (default is 10).
    
    -h, --help
        Displays this help message.

EXAMPLES
    TREETOPNEOCYPHER -k mykey -s "Hello World"
        Encrypts the string "Hello World" using the key "mykey".

    TREETOPNEOCYPHER -t 5
        Runs 5 automated tests to check for lossless decryption.
""");
    }
}

