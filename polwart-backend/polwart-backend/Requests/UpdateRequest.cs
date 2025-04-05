using JetBrains.Annotations;

namespace polwart_backend.Requests;

[PublicAPI]
public class UpdateRequest
{
	public int MapId { get; set; }
	public long SinceTimestamp { get; set; }
}