

namespace CompliaShield.DataExchange.PgpHelper
{
    using Sdk.Cryptography.Encryption;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("CompliaShield PGP encryption helper...");
            Console.WriteLine();
            Console.WriteLine("Licensed under open source MIT License");
            Console.WriteLine();


            if (args == null || args.Count() <= 2)
            {
                Console.WriteLine("Invalid arguments...");
                OutputHelp();
                Thread.Sleep(5000);
                return;
            }

            string extensionLimit = null;

            for (int i = 0; i < args.Count(); i++)
            {
                if (args[i] == "--ext")
                {
                    try
                    {
                        extensionLimit = args[i + 1];
                        if (!extensionLimit.StartsWith("."))
                        {
                            extensionLimit = "." + extensionLimit;
                        }
                    }
                    catch { }
                }
            }

          
            // get the file attributes for file or directory
            try
            {
                Uri uri;
                if (!Uri.TryCreate(args[0], UriKind.RelativeOrAbsolute, out uri))
                {
                    throw new ArgumentException($"Argument 1 is not a valid URL. Value was: '{args[0]}'");
                }

                Console.WriteLine("PGP Key URL: " + uri.ToString());

                var keyAsString = GetPgpCertificateAsync(uri).GetAwaiter().GetResult();
                var list = new List<string>();
                FileAttributes attr = File.GetAttributes(args[1]);

                Console.WriteLine("Path: " + args[1]);
                
                bool deleteSource = false;
                if (args.Contains("/deletesource"))
                {
                    deleteSource = true;
                }
                Console.WriteLine("deletesource: " + deleteSource.ToString().ToLower());

                bool overwrite = false;
                if (args.Contains("/overwrite"))
                {
                    overwrite = true;
                }
                Console.WriteLine("overwrite: " + overwrite.ToString().ToLower());


                if (attr.HasFlag(FileAttributes.Directory))
                {
                    var di = new DirectoryInfo(args[1]);
                    foreach (var file in di.GetFiles())
                    {
                        if (file.Extension != null && file.Extension.ToLower() == ".gpg")
                        {
                            continue;
                        }
                        if (extensionLimit != null && file.Extension != extensionLimit)
                        {
                            continue;
                        }
                        PgpEncrypt(keyAsString, file, deleteSource, overwrite);
                        list.Add(file.Name);
                        Console.WriteLine($"Processed '{file.Name}'");
                    }
                }
                else
                {
                    var file = new FileInfo(args[1]);
                    PgpEncrypt(keyAsString, file, deleteSource, overwrite);
                    list.Add(file.Name);
                    Console.WriteLine($"Processed '{file.Name}'");
                }
                Console.WriteLine();
                Console.WriteLine("Completed encrypting...");
                Console.WriteLine($"{list.Count:N0} files processed.");
                Thread.Sleep(3000);
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: " + ex.GetType().Name);
                Console.WriteLine("Message: " + ex.Message);
                Console.WriteLine("-----BEGIN EXCEPTION-----");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("-----END EXCEPTION-----");
            }

        }

        static void OutputHelp()
        {
            Console.WriteLine("\tArgument 1: The URL of the PGP public key to encrypt the data.");
            Console.WriteLine("\tArgument 2: The path to the file or directory of items you wish to encrypt.");
            Console.WriteLine("\t--ext: If present, limits the extensions of files to be encrypted when specifying a directory. Use '--ext .txt'");
            Console.WriteLine("\tArgument '/deletesource': The value instructs the program to delete the source file when complete.");
            Console.WriteLine("\tArgument '/overwrite': The value instructs the program to delete the source file when complete.");
        }

        private static async Task<string> GetPgpCertificateAsync(Uri uri)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage httpResponseMessage;
                httpResponseMessage = await client.GetAsync(uri.ToString());

                // if no OK, throw an exception
                if (!httpResponseMessage.IsSuccessStatusCode || httpResponseMessage.StatusCode == HttpStatusCode.NoContent)
                {
                    throw new ArgumentException($"Could not get PGP public key from URL '{uri.ToString()}'.");
                }

                return await httpResponseMessage.Content.ReadAsStringAsync();
            }
        }

        public static void PgpEncrypt(string pgpPublicKey, FileInfo fi, bool deleteSource, bool overwrite)
        {
            var newFile = new FileInfo(fi.FullName + ".gpg");
            if (newFile.Exists && !overwrite)
            {
                return;
            }

            // continue processing the file
            byte[] encOutput = null;
            try
            {
                encOutput = PgpEncryptor.EncryptAes256(File.ReadAllBytes(fi.FullName), fi.Name, GenerateStreamFromString(pgpPublicKey), withIntegrityCheck: true, armor: false, compress: true);
            }
            catch (Exception ex)
            {
                // encryption exception
                throw new InvalidOperationException($"Could not use PGP public key to encrypt data.\n{ex.GetType().FullName}: {ex.Message}", ex);
            }
            File.WriteAllBytes(newFile.FullName, encOutput);
            if (deleteSource)
            {
                fi.Delete();
            }
        }

        private static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


        private static byte[] StreamToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
