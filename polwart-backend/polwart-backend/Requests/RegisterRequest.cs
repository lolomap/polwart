using JetBrains.Annotations;

namespace polwart_backend.Requests;

[PublicAPI]
public class RegisterRequest
{
	public string Login { get; set; } = "";
	public string Password { get; set; } = "";
}