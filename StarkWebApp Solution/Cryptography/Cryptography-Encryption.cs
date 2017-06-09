void Main()
{
	Base64StringCryptographyService cryptographyService = new Base64StringCryptographyService();

	#region encryption example

        string originalMessage = "the password is: cow";
        string secretMessage = cryptographyService.EncryptForThisComputer(originalMessage);
        Console.WriteLine("Secret message, safe to put on a post-it on your monitor: " + secretMessage);

    #endregion
}

// Define other methods and classes here

public class Base64StringCryptographyService
{
	public string GenerateRandomString()
	{
		const int length = 128;
		string code = null;
		using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
		{
			byte[] tokenData = new byte[length];
			rng.GetBytes(tokenData);

			// largest result
			//byte[] tokenData = Enumerable.Repeat<byte>(255, bufferSize).ToArray();
			//tokenData[bufferSize-1] = 254;

			// smallest result
			//byte[] tokenData = Enumerable.Repeat<byte>(0, bufferSize).ToArray();

			code = Convert.ToBase64String(tokenData);
		}
		return code.PadLeft(length, '0').Substring(0, length);
	}

	public string EncryptForThisComputer(string plaintext, string purpose = "unspecific")
	{
		byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
		byte[] cipherBytes = MachineKey.Protect(bytes, purpose);
		string encodedCipherBytes = Convert.ToBase64String(cipherBytes);
		return encodedCipherBytes;
	}

	public bool TryDecryptForThisComputer(string encodedCiphertext, out string plaintext)
	{
		return TryDecryptForThisComputer(encodedCiphertext, "unspecific", out plaintext);
	}

	public bool TryDecryptForThisComputer(string encodedCiphertext, string purpose, out string plaintext)
	{
		byte[] cipherBytes;

		try
		{
			cipherBytes = Convert.FromBase64String(encodedCiphertext);
		}
		catch (FormatException)
		{
			plaintext = null;
			return false;
		}

		try
		{
			byte[] bytes = MachineKey.Unprotect(cipherBytes, purpose);
			plaintext = Encoding.UTF8.GetString(bytes);

			return true;
		}
		catch (CryptographicException)
		{
			plaintext = null;
			return false;
		}
	}

	// From https://gist.github.com/cmatskas/faee04c7b78afae065e1#file-pbkdf2dotnetsample-cs%23file-pbkdf2dotnetsample-cs .
	public string Hash(string original, string salt, int iterations = 1)
	{
		const int hashByteSize = 20; // to match the size of the PBKDF2-HMAC-SHA-1 hash

		byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

		Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(original, saltBytes);
		pbkdf2.IterationCount = iterations;
		byte[] bytes = pbkdf2.GetBytes(hashByteSize);

		return Convert.ToBase64String(bytes);

	}

	// From https://gist.github.com/cmatskas/faee04c7b78afae065e1#file-pbkdf2dotnetsample-cs%23file-pbkdf2dotnetsample-cs .
	public static bool SlowEquals(string x, string y)
	{
		byte[] a = Convert.FromBase64String(x);
		byte[] b = Convert.FromBase64String(y);

		uint diff = (uint)a.Length ^ (uint)b.Length;
		for (int i = 0; i < a.Length && i < b.Length; i++)
		{
			diff |= (uint)(a[i] ^ b[i]);
		}
		return diff == 0;
	}
}
