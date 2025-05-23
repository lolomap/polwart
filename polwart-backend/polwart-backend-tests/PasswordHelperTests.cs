using System.Reflection;
using NUnit.Framework;
using polwart_backend;

namespace polwart_backend_tests;

public class PasswordHelperTests
{
	[Test]
	public void ByteArraysEqual()
	{
		PasswordHelper helper = new();
		bool result = (bool) typeof(PasswordHelper)
			.GetMethod("ByteArraysEqual", BindingFlags.NonPublic | BindingFlags.Static)!
			.Invoke(helper, [new byte[] {0x00, 0x01, 0x02}, new byte[] {0x00, 0x01, 0x02}])!;
		
		Assert.That(result);
	}

	[Test]
	public void ByteArraysEqualSameReference()
	{
		PasswordHelper helper = new();
		byte[] arr = [0x00, 0x01, 0x02];
		bool result = (bool) typeof(PasswordHelper)
			.GetMethod("ByteArraysEqual", BindingFlags.NonPublic | BindingFlags.Static)!
			.Invoke(helper, [arr, arr])!;
		
		Assert.That(result);
	}
	
	[Test]
	public void ByteArraysEqualDifferentLength()
	{
		PasswordHelper helper = new();
		bool result = (bool) typeof(PasswordHelper)
			.GetMethod("ByteArraysEqual", BindingFlags.NonPublic | BindingFlags.Static)!
			.Invoke(helper, [new byte[] {0x00, 0x01}, new byte[] {0x00, 0x01, 0x02}])!;
		
		Assert.That(!result);
	}
	
	[Test]
	public void ByteArraysEqualDifferentContent()
	{
		PasswordHelper helper = new();
		bool result = (bool) typeof(PasswordHelper)
			.GetMethod("ByteArraysEqual", BindingFlags.NonPublic | BindingFlags.Static)!
			.Invoke(helper, [new byte[] {0x00, 0x01, 0x02}, new byte[] {0x00, 0x01, 0x03}])!;
		
		Assert.That(!result);
	}

	[Test]
	public void VerifyHashedPassword()
	{
		string hash = PasswordHelper.HashPassword("PASSWORD");
		Assert.That(PasswordHelper.VerifyHashedPassword(hash, "PASSWORD"));
	}
	
	[Test]
	public void VerifyHashedPasswordInvalid()
	{
		string hash = PasswordHelper.HashPassword("PASSWORD");
		Assert.That(!PasswordHelper.VerifyHashedPassword(hash, "qwerty123"));
	}
	
	[Test]
	public void VerifyHashedPasswordNoHash()
	{
		Assert.That(!PasswordHelper.VerifyHashedPassword("PASSWORD", "PASSWORD"));
	}
}