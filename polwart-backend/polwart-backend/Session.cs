using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Patch;
using Microsoft.AspNetCore.SignalR;
using polwart_backend.Requests;

namespace polwart_backend;

public class Session(int mapId, string root)
{
	private int _mapId = mapId;
	private readonly HashSet<ISingleClientProxy> _clients = [];
	private readonly JsonNode _rootDocument = JsonNode.Parse(root)!;
	private readonly SortedList<long, Revision> _revisions = [];

	public void Patch(PatchRequest request)
	{
		JsonPatch? data = JsonSerializer.Deserialize<JsonPatch>(request.Patch);
		if (data == null) return;

		Revision revision = new(this, data, request.Timestamp);
		_revisions.TryAdd(request.Timestamp, revision);
	}

	public IEnumerable<Revision> GetRevisions(long sinceTimestamp)
	{
		return _revisions.Values.Where(x => x.Timestamp >= sinceTimestamp);
	}

	public void RegisterClient(ISingleClientProxy client)
	{
		_clients.Add(client);
	}

	public IEnumerable<ISingleClientProxy> GetClients()
	{
		return _clients;
	}

	public string CombineRevisions()
	{
		JsonNode? document = _rootDocument.DeepClone();

		foreach (Revision revision in _revisions.Values)
		{
			PatchResult patch = revision.PatchData.Apply(document);
			if (patch.IsSuccess)
				document = patch.Result;
		}

		return JsonSerializer.Serialize(document);
	}
}