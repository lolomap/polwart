using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Patch;
using polwart_backend.Requests;

namespace polwart_backend;

public class Session(int mapId, string root)
{
	private int _mapId = mapId;
	private readonly JsonNode _rootDocument = JsonNode.Parse(root)!;
	private readonly SortedList<long, Revision> _revisions = [];

	public void Patch(PatchRequest request)
	{
		JsonPatch? data = JsonSerializer.Deserialize<JsonPatch>(request.Patch);
		if (data == null) return;

		Revision revision = new(this, data, request.Timestamp);
		_revisions.Add(request.Timestamp, revision);
	}

	public IEnumerable<Revision> GetRevisions(long sinceTimestamp)
	{
		return _revisions.Values.Where(x => x.Timestamp >= sinceTimestamp);
	}

	public void SendNotifications()
	{
		
	}

	public string CombineRevisions()
	{
		JsonNode document = _rootDocument.DeepClone();
		
		for (int i = 0; i < _revisions.Count; i++)
		{
			_revisions[i].PatchData.Apply(document);
		}

		return JsonSerializer.Serialize(document);
	}
}