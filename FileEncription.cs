using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VWA;

// AusRufs:
//	fileEncription.EncryptFile("SConfig.txt", "SConfig1.txt", "YourPassword");
//fileEncription.DecryptFile("SConfig.txt", "decrypted.txt", "YourPassword");  
//fileEncryption.DecryptFile("output.txt", "decrypted.txt", "YourPassword");

//string s = fileEncription.DecryptFileToString("SConfig.txt", "YourPassword"); ;

namespace VWA
{
	public class FileEncription
	{
		public void EncryptFile(string inputFile, string outputFile, string password)
		{
			byte[] salt = Encoding.UTF8.GetBytes("SaltValue");
			using (Aes aes = Aes.Create())
			{

				var keyGenerator = new Rfc2898DeriveBytes(password, salt);
				aes.Key = keyGenerator.GetBytes(32);
				aes.IV = keyGenerator.GetBytes(16);


				using (FileStream inputFileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
				using (FileStream outputFileStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
				{

					outputFileStream.Write(aes.IV, 0, aes.IV.Length);

					using (CryptoStream cryptoStream = new CryptoStream(outputFileStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
					{
						inputFileStream.CopyTo(cryptoStream);
					}
				}
			}
		}

		public void DecryptFile(string inputFile, string outputFile, string password)
		{
			byte[] salt = Encoding.UTF8.GetBytes("SaltValue");
			using (Aes aes = Aes.Create())
			{
				using (FileStream inputFileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
				{

					byte[] iv = new byte[16];
					inputFileStream.Read(iv, 0, iv.Length);


					var keyGenerator = new Rfc2898DeriveBytes(password, salt);
					aes.Key = keyGenerator.GetBytes(32);
					aes.IV = iv;


					using (FileStream outputFileStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
					using (CryptoStream cryptoStream = new CryptoStream(inputFileStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
					{
						cryptoStream.CopyTo(outputFileStream);
					}
				}
			}
		}
		public string DecryptFileToString(string inputFile, string password)
		{
			byte[] salt = Encoding.UTF8.GetBytes("SaltValue");
			using (Aes aes = Aes.Create())
			{
				using (FileStream inputFileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
				{

					byte[] iv = new byte[16];
					inputFileStream.Read(iv, 0, iv.Length);


					var keyGenerator = new Rfc2898DeriveBytes(password, salt);
					aes.Key = keyGenerator.GetBytes(32);
					aes.IV = iv;


					using (CryptoStream cryptoStream = new CryptoStream(inputFileStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
					using (MemoryStream memoryStream = new MemoryStream())
					{
						cryptoStream.CopyTo(memoryStream);
						byte[] decryptedBytes = memoryStream.ToArray();


						return Encoding.UTF8.GetString(decryptedBytes);
					}
				}
			}
		}

		public SecureString ToSecureString(string plainText)
		{
			var secureString = new SecureString();
			foreach (char c in plainText)
			{
				secureString.AppendChar(c);
			}
			secureString.MakeReadOnly();
			return secureString;
		}
		public string ConvertToPlainString(SecureString secureString)
		{
			if (secureString == null)
				throw new ArgumentNullException(nameof(secureString));

			IntPtr unmanagedString = IntPtr.Zero;
			try
			{
				unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
				return Marshal.PtrToStringUni(unmanagedString);
			}
			finally
			{
				Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString); // Wyczyść pamięć
			}
		}
	}
	}

