using Json.Patch;

namespace polwart_backend;

public class Revision(Session session, JsonPatch patch, long timestamp)
{
	public Session Session { get; private set; } = session;
	public readonly long Timestamp = timestamp;
	public readonly JsonPatch PatchData = patch;
}