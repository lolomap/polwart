using System.Security.Cryptography;

namespace polwart_backend;

public class PasswordHelper
{
	private static bool ByteArraysEqual(byte[] a1, byte[] a2)
	{
		if (ReferenceEquals(a1, a2))
			return true;
        
		return a1.Length == a2.Length && ((ReadOnlySpan<byte>)a1).SequenceEqual(a2);
	}
	
	public static string HashPassword(string password)
	{
		byte[] salt;
		byte[] buffer2;
        using (Rfc2898DeriveBytes bytes = new(password, 0x10, 0x3e8, HashAlgorithmName.SHA256))
		{
			salt = bytes.Salt;
			buffer2 = bytes.GetBytes(0x20);
		}
		byte[] dst = new byte[0x31];
		Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
		Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
		return Convert.ToBase64String(dst);
	}
	
	public static bool VerifyHashedPassword(string hashedPassword, string password)
	{
		byte[] buffer4;
		byte[] src = Convert.FromBase64String(hashedPassword);
		if ((src.Length != 0x31) || (src[0] != 0))
		{
			return false;
		}
		byte[] dst = new byte[0x10];
		Buffer.BlockCopy(src, 1, dst, 0, 0x10);
		byte[] buffer3 = new byte[0x20];
		Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
		using (Rfc2898DeriveBytes bytes = new(password, dst, 0x3e8, HashAlgorithmName.SHA256))
		{
			buffer4 = bytes.GetBytes(0x20);
		}
		return ByteArraysEqual(buffer3, buffer4);
	}
}