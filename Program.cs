using CryptUtils.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CryptUtils
{
    class Program
    {
   
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("CryptUtils");
            Console.WriteLine();

            Arguments arguments = new Arguments(args);
            arguments.Process();
            arguments.CheckValidOptions(new string[] { "-d", "--dir", "-o", "--out", "-p", "--password", "-h", "--help" });

            Func<IOptions, Task<int>> encryptor = async opts =>
            {
                Console.WriteLine($"Encrypting directory {opts.DIR} ...");
                Console.WriteLine();

                await Task.Yield();

                string outputDir = opts.OUTPUTDIR ?? Path.Combine(opts.DIR, "!ENCRYPTED");

                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                foreach (string file in Directory.EnumerateFiles(opts.DIR))
                {
                    await Encryptor.Encrypt(opts.Password, file, outputDir);
                }

                Console.WriteLine($"Directory is encrypted");

                return 0;
            };

            Func<IOptions, Task<int>> decryptor = async opts =>
            {
                Console.WriteLine($"Decrypting directory {opts.DIR} ...");
                Console.WriteLine();

                await Task.Yield();

                string outputDir = opts.OUTPUTDIR ?? Path.Combine(opts.DIR, "!DECRYPTED");

                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                foreach (string file in Directory.EnumerateFiles(opts.DIR))
                {
                    await Encryptor.Decrypt(opts.Password, file, outputDir);
                }

                Console.WriteLine($"Directory is decrypted");

                return 0;
            };

            if (arguments.Errors.Any())
            {
                await Error(arguments.Errors);
            }
            else if (arguments.Options.Count == 0 || arguments.GetOption("-h", "--help") != null)
            {
                await Usage();
            }
            else if (arguments.Command == "encrypt")
            {
                EncryptOptions encryptOptions = new EncryptOptions() { DIR = arguments.GetOption("-d", "--dir")?.Trim(' ', '"'), OUTPUTDIR = arguments.GetOption("-o", "--out")?.Trim(' ', '"'), Password = arguments.GetOption("-p", "--password") };

                List<string> errors = new List<string>();

                if (string.IsNullOrEmpty(encryptOptions.DIR))
                {
                    errors.Add("The required option --dir is not provided");
                }
                else if (!Directory.Exists(encryptOptions.DIR))
                {
                    errors.Add($"The directory {encryptOptions.DIR} does not exist");
                }

                if (string.IsNullOrEmpty(encryptOptions.Password))
                {
                    errors.Add("The required option --password is not provided");
                }
                else if (encryptOptions.Password?.Length < 6)
                {
                    errors.Add("The password is too weak");
                }

                if (errors.Any())
                {
                    await Error(errors);
                }
                else
                {
                    await encryptor(encryptOptions);
                }
            }
            else if (arguments.Command == "decrypt")
            {
                DecryptOptions decryptOptions = new DecryptOptions() { DIR = arguments.GetOption("-d", "--dir")?.Trim(' ', '"'), OUTPUTDIR = arguments.GetOption("-o", "--out")?.Trim(' ', '"'), Password = arguments.GetOption("-p", "--password") };

                List<string> errors = new List<string>();

                if (string.IsNullOrEmpty(decryptOptions.DIR))
                {
                    errors.Add("The required option --dir is not provided");
                }
                else if (!Directory.Exists(decryptOptions.DIR))
                {
                    errors.Add($"The directory {decryptOptions.DIR} does not exist");
                }

                if (string.IsNullOrEmpty(decryptOptions.Password))
                {
                    errors.Add("The required option --password is not provided");
                }

                if (errors.Any())
                {
                    await Error(errors);
                }
                else
                {
                    await decryptor(decryptOptions);
                }
            }
            else
            {
                await Error(new string[] { "Invalid arguments" });
            }



            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();

            async Task<int> Error(IEnumerable<string> errs)
            {
                await Task.CompletedTask;

                Console.WriteLine("ERROR:");

                foreach (var err in errs)
                {
                    Console.WriteLine(err.ToString());
                }

                Console.WriteLine();

                await Usage();
                return 0;
            }

            async Task<int> Usage()
            {
                await Task.CompletedTask;

                Console.WriteLine("USAGE: [command?] [options]");
                Console.WriteLine();

                Console.WriteLine("COMMANDS:");
                Console.WriteLine("encrypt \t default, for encrypting files in the directory");
                Console.WriteLine("decrypt \t for decrypting files in the directory");
                Console.WriteLine();
                Console.WriteLine("OPTIONS:");
                Console.WriteLine("-d, --dir \t required, Directory to encrypt or decrypt");
                Console.WriteLine("-p, --password \t required, Password for encryption or decryption");
                Console.WriteLine("-o, --out \t optional, Output directory for results");
                Console.WriteLine();

                Console.WriteLine("EXAMPLES:");
                Console.WriteLine("CryptUtils.exe encrypt --dir c:\\DIR -p qwertyuiop --out c:\\OUTDIR");
                Console.WriteLine("CryptUtils.exe decrypt --dir c:\\DIR -p qwertyuiop --out c:\\OUTDIR");

                return 0;
            }
            
      
            return 0;
        }
        
    }
}
