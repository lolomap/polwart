using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace polwart_backend;

public static class AuthOptions
{
	public const string ISSUER = "PolwartServer";
	public const string AUDIENCE = "newdaysteam.ru";
	private static readonly string _key = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!;
	public static SymmetricSecurityKey GetSymmetricSecurityKey() => new(Encoding.UTF8.GetBytes(_key));
}