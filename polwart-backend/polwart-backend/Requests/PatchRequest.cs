using JetBrains.Annotations;

namespace polwart_backend.Requests;

[PublicAPI]
public class PatchRequest
{
	public long Timestamp { get; set; }
	public string Patch { get; set; } = "";
}