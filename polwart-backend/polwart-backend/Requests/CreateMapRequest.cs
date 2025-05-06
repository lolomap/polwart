using JetBrains.Annotations;

namespace polwart_backend.Requests;

[PublicAPI]
public class CreateMapRequest
{
	public bool IsPublic { get; set; }
	public string InitialTimestampISO { get; set; } = "";

	public string BackgroundFormat { get; set; } = "";
}