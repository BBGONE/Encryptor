using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CryptUtils
{
    public static class Encryptor
    {
        public static async Task Encrypt(string pass, string src, string outputDir)
        {
            byte[] iv = new byte[16];

            var cryptoRng = RandomNumberGenerator.Create();
            cryptoRng.GetBytes(iv);
            var rfc = new Rfc2898DeriveBytes(pass, iv);
            byte[] key = rfc.GetBytes(16);

            DateTime fileDate = File.GetCreationTimeUtc(src);
            long srcDate = fileDate.ToBinary();
            string dest = Path.Combine(outputDir, Path.ChangeExtension(Path.GetFileName(src),"bin"));

            // Console.WriteLine($"{src}   {fileDate}");

            try
            {
                using (Stream destf = File.Create(dest))
                {
                    await destf.WriteAsync(iv);

                    byte[] encryptedName = EncryptString(key, iv, Path.GetFileName(src));
                    await WriteLenBytes(destf, encryptedName);

                    byte[] encryptedDate = EncryptString(key, iv, srcDate.ToString());
                    await WriteLenBytes(destf, encryptedDate);

                    using (var srcFile = File.OpenRead(src))
                    using (Aes algorithm = Aes.Create())
                    using (ICryptoTransform encryptor = algorithm.CreateEncryptor(key, iv))
                    using (Stream c = new CryptoStream(destf, encryptor, CryptoStreamMode.Write))
                    {
                        await srcFile.CopyToAsync(c);
                    }
                }
            }
            catch(Exception)
            {
                if (File.Exists(dest))
                    File.Delete(dest);

                throw;
            }
        }

        public static async Task Decrypt(string pass, string src, string outputDir)
        {
            byte[] iv = new byte[16];
            string dest = null;

            try
            {
                using (Stream srcFile = File.OpenRead(src))
                {
                    await srcFile.ReadAsync(iv);

                    var rfc = new Rfc2898DeriveBytes(pass, iv);
                    byte[] key = rfc.GetBytes(16);

                    byte[] namebytes = await ReadLenBytes(srcFile);
                    string name = DecryptString(key, iv, namebytes);

                    byte[] dateBytes = await ReadLenBytes(srcFile);
                    string date = DecryptString(key, iv, dateBytes);
                    DateTime fileDate = DateTime.FromBinary(long.Parse(date));
                    DateTime.SpecifyKind(fileDate, DateTimeKind.Utc);
                    
                    // Console.WriteLine($"{src}   {fileDate}");

                    dest = Path.Combine(outputDir, name);

                    using (Stream destf = File.Create(dest))
                    using (Aes algorithm = Aes.Create())
                    using (ICryptoTransform decryptor = algorithm.CreateDecryptor(key, iv))
                    using (Stream c = new CryptoStream(srcFile, decryptor, CryptoStreamMode.Read))
                    {
                        await c.CopyToAsync(destf);
                    }

                    File.SetCreationTimeUtc(dest, fileDate);
                    File.SetLastWriteTimeUtc(dest, fileDate);
                    File.SetLastAccessTimeUtc(dest, fileDate);
                }
            }
            catch (Exception)
            {
                if (!string.IsNullOrEmpty(dest) && File.Exists(dest))
                    File.Delete(dest);

                throw;
            }
        }

        static async Task WriteLenBytes(Stream stream, byte[] bytes)
        {
            int len = bytes.Length;
            byte[] lenbytes = new byte[1] { (byte)len };
            await stream.WriteAsync(lenbytes);
            await stream.WriteAsync(bytes);
        }

        static async Task<byte[]> ReadLenBytes(Stream stream)
        {
            byte[] lenbytes = new byte[1];
            await stream.ReadAsync(lenbytes);
            int len = lenbytes[0];

            byte[] bytes = new byte[len];
            await stream.ReadAsync(bytes);
            return bytes;
        }

        static byte[] EncryptString(byte[] key, byte[] iv, string name)
        {
            MemoryStream mem = new MemoryStream();

            using (Aes algorithm = Aes.Create())
            {
                using (ICryptoTransform encryptor = algorithm.CreateEncryptor(key, iv))
                {
                    using (Stream c = new CryptoStream(mem, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter w = new StreamWriter(c))
                        w.Write(name);
                }
            }

            return mem.ToArray();
        }

        static string DecryptString(byte[] key, byte[] iv, byte[] bytes)
        {
            using (Aes algorithm = Aes.Create())
            {
                using (ICryptoTransform decryptor = algorithm.CreateDecryptor(key, iv))
                using (Stream f = new MemoryStream(bytes))
                using (Stream c = new CryptoStream(f, decryptor, CryptoStreamMode.Read))
                using (StreamReader r = new StreamReader(c))
                    return r.ReadToEnd();
            }
        }
    }
}
