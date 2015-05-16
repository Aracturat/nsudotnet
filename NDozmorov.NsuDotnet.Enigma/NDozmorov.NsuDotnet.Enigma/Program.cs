using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace NDozmorov.NsuDotnet.Enigma
{
    internal class Program
    {
        private delegate void CryptFunc(Stream inStream, Stream outStream, SymmetricAlgorithm alg);

        private static void Main(string[] args)
        {
            Dictionary<String, SymmetricAlgorithm> algorithms = new Dictionary<string, SymmetricAlgorithm>();
            algorithms["DES"] = new DESCryptoServiceProvider();
            algorithms["AES"] = new AesCryptoServiceProvider();
            algorithms["RC2"] = new RC2CryptoServiceProvider();
            algorithms["RIJNDAEL"] = new RijndaelManaged();

            try
            {
                string dir = args[0];
                string inputFilename = args[1];
                string algorithmName = args[2];
                SymmetricAlgorithm algorythm = algorithms[algorithmName.ToUpper()];

                string keyFilename;
                string outputFilename;
                CryptFunc func;
                 
                switch (dir.ToUpper())
                {
                    case "ENCRYPT":
                        outputFilename = args[3];
                        algorythm.GenerateIV();
                        algorythm.GenerateKey();

                        keyFilename = String.Format("{0}.key.txt", inputFilename);
                        WriteKeyFile(keyFilename, algorythm);
                        func = EncryptBytes;
                        break;

                    case "DECRYPT":
                        keyFilename = args[3];
                        outputFilename = args[4];
                        
                        ReadKeyFile(keyFilename, ref algorythm);
                        func = DecryptBytes;
                        break;

                    default:
                        throw new Exception(String.Format("Wrong argument: {0}", dir));
                }

                using (var inStream = new FileStream(inputFilename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    using (var outStream = new FileStream(outputFilename, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        outStream.SetLength(0);
                       
                        func.Invoke(inStream, outStream, algorythm);
                    }
                }
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine("Algorythm not found!");
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("Wrong number of parameters");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void WriteKeyFile(string keyFilename, SymmetricAlgorithm algorithm)
        {
            using (var keyStream = new FileStream(keyFilename, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var keyWriter = new StreamWriter(keyStream))
                {
                    keyWriter.WriteLine(Convert.ToBase64String(algorithm.IV));
                    keyWriter.WriteLine(Convert.ToBase64String(algorithm.Key));
                }
            }
        }

        private static void ReadKeyFile(string keyFilename, ref SymmetricAlgorithm algorithm)
        {
            if (!File.Exists(keyFilename))
            {
                throw new Exception("Key file not found");
            }

            using (var keyStream = new FileStream(keyFilename, FileMode.Open, FileAccess.Read))
            {
                using (var keyReader = new StreamReader(keyStream))
                {
                    string IV = keyReader.ReadLine();
                    string Key = keyReader.ReadLine();

                    if (IV.Length == 0 || Key.Length == 0)
                    {
                        throw new Exception("Wrong Key file");
                    }

                    algorithm.IV = Convert.FromBase64String(IV);
                    algorithm.Key = Convert.FromBase64String(Key);
                }
            }
        }
        
        private static void EncryptBytes(Stream inStream, Stream outStream, SymmetricAlgorithm alg)
        {
            ICryptoTransform encryptor = alg.CreateEncryptor(alg.Key, alg.IV);

            using (var encStream = new CryptoStream(outStream, encryptor, CryptoStreamMode.Write))
            {
                inStream.CopyTo(encStream);
            }
        }

        private static void DecryptBytes(Stream inStream, Stream outStream, SymmetricAlgorithm alg)
        {
            ICryptoTransform decryptor = alg.CreateDecryptor(alg.Key, alg.IV);
            
            using (var decrStream = new CryptoStream(inStream, decryptor, CryptoStreamMode.Read))
            {
                decrStream.CopyTo(outStream);
            }
        }
    }
}
