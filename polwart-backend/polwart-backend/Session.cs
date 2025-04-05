using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Patch;
using Microsoft.AspNetCore.SignalR;
using polwart_backend.Requests;

namespace polwart_backend;

public class Session(int mapId, string root)
{
	public readonly int MapId = mapId;
	private readonly Dictionary<string, ISingleClientProxy> _clients = [];
	private readonly JsonNode _rootDocument = JsonNode.Parse(root)!;
	private readonly SortedList<long, Revision> _revisions = [];
	
	public event Action<Session>? Close;

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

	public void RegisterClient(string connectionId, ISingleClientProxy client)
	{
		_clients[connectionId] = client;
	}

	public void DisposeClient(string connectionId)
	{
		if (!_clients.ContainsKey(connectionId)) return;

		_clients.Remove(connectionId);
		
		if (_clients.Count <= 0)
			Close?.Invoke(this);
	}

	public IEnumerable<ISingleClientProxy> GetClientsConnections()
	{
		return _clients.Values;
	}

	public IEnumerable<string> GetClientsEndpoints()
	{
		return _clients.Keys;
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