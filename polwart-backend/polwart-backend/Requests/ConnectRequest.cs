using JetBrains.Annotations;

namespace polwart_backend.Requests;

[PublicAPI]
public class ConnectRequest 
{
	public int MapId { get; set; }
}