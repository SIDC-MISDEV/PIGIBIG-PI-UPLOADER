using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PIGIBIG_PI_UPLOADER
{
    class ACryptoServiceProvider
    {
        private static string Key = "CesInadDrewMonetteRuelArleneCarl";
        //   private static string Key = "dofkrfaosrdedofkrfaosrdedofkrfao";
        private static string IV = "lenardaquino1234";
        char pad = 'x';

        public string Encrypt(string text, string iv)
        {
            byte[] plaintextbytes = System.Text.ASCIIEncoding.ASCII.GetBytes(text);
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.Key = System.Text.ASCIIEncoding.ASCII.GetBytes(Key);
            //aes.IV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV);
            aes.IV = System.Text.ASCIIEncoding.ASCII.GetBytes(iv.PadRight(16, pad));
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;
            ICryptoTransform crypto = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] encrypted = crypto.TransformFinalBlock(plaintextbytes, 0, plaintextbytes.Length);
            crypto.Dispose();
            return Convert.ToBase64String(encrypted);
        }

        // public static string Decrypt(string encrypted, string _key)
        public string Decrypt(string encrypted, string iv)
        {
            try
            {

                byte[] encryptedbytes = Convert.FromBase64String(encrypted);
                AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.Key = System.Text.ASCIIEncoding.ASCII.GetBytes(Key);
                //aes.IV = System.Text.ASCIIEncoding.ASCII.GetBytes(IV);
                aes.IV = System.Text.ASCIIEncoding.ASCII.GetBytes(iv.PadRight(16, pad));
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;
                ICryptoTransform crypto = aes.CreateDecryptor(aes.Key, aes.IV);
                byte[] secret = crypto.TransformFinalBlock(encryptedbytes, 0, encryptedbytes.Length);
                crypto.Dispose();
                return System.Text.ASCIIEncoding.ASCII.GetString(secret);
            }
            catch (Exception er)
            {
                //return null;
                return er.Message;
            }

        }
    }
}
