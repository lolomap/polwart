using JetBrains.Annotations;
using Json.Patch;

namespace polwart_backend.Requests;

[PublicAPI]
public class PatchRequest
{
	public int MapId { get; set; }
	public long Timestamp { get; set; }
	public string Patch { get; set; } = "";
}