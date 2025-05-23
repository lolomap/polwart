using System.Reflection;
using Json.Patch;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using NUnit.Framework;
using polwart_backend;
using polwart_backend.Entities;

namespace polwart_backend_tests;

public class SessionTests
{
	private Session _session = null!;
	private SortedList<long, Revision> _sessionRevisions = null!;
	private Dictionary<string, ISingleClientProxy> _sessionClients = null!;
	
	[SetUp]
	public void Setup()
	{
		Map mapInfo = new()
		{
			Id = 0,
			Content = "{\"id\": 0,\"layers\": [{\"timestampISO\": \"2025-03-27T12:45:45.951Z\",\"content\": []}],\"legend\": [],\"toRemoveValue\": 0, \"toRemoveArray\": [0], \"toCombineArray\":[]}"
		};
		const string patchData = "[{\"op\": \"add\",\"path\": \"/toCombineArray/-\",\"value\": 123}]";
		
		_session = new(mapInfo);
		
		Revision r1 = new(_session, JsonSerializer.Deserialize<JsonPatch>(patchData)!, 0);
		Revision r2 = new(_session, JsonSerializer.Deserialize<JsonPatch>(patchData)!, 100);
		Revision r3 = new(_session, JsonSerializer.Deserialize<JsonPatch>(patchData)!, 200);

		_sessionRevisions = (SortedList<long, Revision>)
			typeof(Session).GetField("_revisions", BindingFlags.NonPublic | BindingFlags.Instance)!
				.GetValue(_session)!;
		_sessionRevisions.Add(r1.Timestamp, r1);
		_sessionRevisions.Add(r2.Timestamp, r2);
		_sessionRevisions.Add(r3.Timestamp, r3);

		_sessionClients = (Dictionary<string, ISingleClientProxy>)
			typeof(Session).GetField("_clients", BindingFlags.NonPublic | BindingFlags.Instance)!
				.GetValue(_session)!;
		_sessionClients.Add("testConnection", new TestClient());
	}

	[Test]
	public void PatchAdd()
    {
        const string patchData = "[{\"op\": \"add\",\"path\": \"/\",\"value\": {\"testProperty\": 0}}]";
		_session.Patch(new()
		{
			MapId = 0,
			Timestamp = 300,
			Patch = patchData
		});

		Revision lastRevision = _sessionRevisions.Last().Value;
		PatchOperation operation = lastRevision.PatchData.Operations[0];
        Assert.Multiple(() =>
        {
            Assert.That(operation.Op, Is.EqualTo(OperationType.Add));
            Assert.That(operation.Path.ToString(), Is.EqualTo("/"));
            Assert.That(operation.Value!.ToJsonString(), Is.EqualTo("{\"testProperty\":0}"));
        });
    }
    [Test]
	public void PatchRemove()
	{
		const string patchData = "[{\"op\": \"remove\",\"path\": \"/toRemoveValue\"}]";
		_session.Patch(new()
		{
			MapId = 0,
			Timestamp = 400,
			Patch = patchData
		});

		Revision lastRevision = _sessionRevisions.Last().Value;
		PatchOperation operation = lastRevision.PatchData.Operations[0];
		Assert.Multiple(() =>
		{
			Assert.That(operation.Op, Is.EqualTo(OperationType.Remove));
			Assert.That(operation.Path.ToString(), Is.EqualTo("/toRemoveValue"));
		});
	}
	[Test]
	public void PatchReplace()
	{
		const string patchData = "[{\"op\": \"replace\",\"path\": \"/id\",\"value\": 0}]";
		_session.Patch(new()
		{
			MapId = 0,
			Timestamp = 500,
			Patch = patchData
		});

		Revision lastRevision = _sessionRevisions.Last().Value;
		PatchOperation operation = lastRevision.PatchData.Operations[0];
		Assert.Multiple(() =>
		{
			Assert.That(operation.Op, Is.EqualTo(OperationType.Replace));
			Assert.That(operation.Path.ToString(), Is.EqualTo("/id"));
			Assert.That(operation.Value!.ToJsonString(), Is.EqualTo("0"));
		});
	}
	[Test]
	public void PatchAddArray()
	{
		const string patchData = "[{\"op\": \"add\",\"path\": \"/legend/-\",\"value\": {\"testProperty\": 0}}]";
		_session.Patch(new()
		{
			MapId = 0,
			Timestamp = 600,
			Patch = patchData
		});

		Revision lastRevision = _sessionRevisions.Last().Value;
		PatchOperation operation = lastRevision.PatchData.Operations[0];
		Assert.Multiple(() =>
		{
			Assert.That(operation.Op, Is.EqualTo(OperationType.Add));
			Assert.That(operation.Path.ToString(), Is.EqualTo("/legend/-"));
			Assert.That(operation.Value!.ToJsonString(), Is.EqualTo("{\"testProperty\":0}"));
		});
	}
	[Test]
	public void PatchReplaceArray()
	{
		const string patchData = "[{\"op\": \"replace\",\"path\": \"/layers/0\",\"value\": {\"timestampISO\": \"2025-03-27T12:45:45.951Z\",\"content\": []}}]";
		_session.Patch(new()
		{
			MapId = 0,
			Timestamp = 700,
			Patch = patchData
		});

		Revision lastRevision = _sessionRevisions.Last().Value;
		PatchOperation operation = lastRevision.PatchData.Operations[0];
		Assert.Multiple(() =>
		{
			Assert.That(operation.Op, Is.EqualTo(OperationType.Replace));
			Assert.That(operation.Path.ToString(), Is.EqualTo("/layers/0"));
			Assert.That(operation.Value!.ToJsonString(), Is.EqualTo("{\"timestampISO\":\"2025-03-27T12:45:45.951Z\",\"content\":[]}"));
		});
	}
	[Test]
	public void PatchRemoveArray()
	{
		const string patchData = "[{\"op\": \"remove\",\"path\": \"/toRemoveArray/0\"}]";
		_session.Patch(new()
		{
			MapId = 0,
			Timestamp = 800,
			Patch = patchData
		});

		Revision lastRevision = _sessionRevisions.Last().Value;
		PatchOperation operation = lastRevision.PatchData.Operations[0];
		Assert.Multiple(() =>
		{
			Assert.That(operation.Op, Is.EqualTo(OperationType.Remove));
			Assert.That(operation.Path.ToString(), Is.EqualTo("/toRemoveArray/0"));
		});
	}
	[Test]
	public void PatchLate()
	{
		const string patchData = "[{\"op\": \"replace\",\"path\": \"/id\",\"value\": 0}]";
		
		_session.Patch(new()
		{
			MapId = 0,
			Timestamp = -1000,
			Patch = patchData
		});

		Revision firstRevision = _sessionRevisions.First().Value;
		PatchOperation operation = firstRevision.PatchData.Operations[0];
		Assert.Multiple(() =>
		{
			Assert.That(operation.Op, Is.EqualTo(OperationType.Replace));
			Assert.That(operation.Path.ToString(), Is.EqualTo("/id"));
			Assert.That(operation.Value!.ToJsonString(), Is.EqualTo("0"));
		});
	}
	[Test]
	public void PatchInvalid()
	{
		const string patchData = "[{\"op\": \"repl";
		
		int oldRevisionsCount = _sessionRevisions.Count;
		
		_session.Patch(new()
		{
			MapId = 0,
			Timestamp = -1000,
			Patch = patchData
		});

		int newRevisionsCount = _sessionRevisions.Count;
		
		Assert.That(oldRevisionsCount, Is.EqualTo(newRevisionsCount));
	}

	[Test]
	public void GetRevisionsMid()
    {
        List<Revision> revisions = _session.GetRevisions(50).ToList();
        
        Assert.Multiple(() =>
        {
	        Assert.That(revisions, Has.Count.EqualTo(2));
            Assert.That(revisions[0].Timestamp, Is.EqualTo(100));
            Assert.That(revisions[1].Timestamp, Is.EqualTo(200));
        });
    }
	[Test]
	public void GetRevisionsLess()
	{
		List<Revision> revisions = _session.GetRevisions(-1000).ToList();
        
		Assert.Multiple(() =>
		{
			Assert.That(revisions, Has.Count.EqualTo(3));
			Assert.That(revisions[0].Timestamp, Is.EqualTo(0));
			Assert.That(revisions[1].Timestamp, Is.EqualTo(100));
			Assert.That(revisions[2].Timestamp, Is.EqualTo(200));
		});
	}
	[Test]
	public void GetRevisionsGreater()
	{
		List<Revision> revisions = _session.GetRevisions(1000).ToList();
        
		Assert.That(revisions, Has.Count.EqualTo(0));
	}

	[Test]
	public void RegisterClient()
    {
        const string connection = "testConnection2";
		TestClient client = new();
		_session.RegisterClient(connection, client);
        Assert.Multiple(() =>
        {
            Assert.That(_sessionClients.ContainsKey(connection));
            Assert.That(_sessionClients[connection], Is.SameAs(client));
        });
    }

    [Test]
	public void RegisterClientConnectionExists()
	{
		const string connection = "testConnection";
		TestClient client = new();
		_session.RegisterClient(connection, client);
		Assert.Multiple(() =>
		{
			Assert.That(_sessionClients.ContainsKey(connection));
			Assert.That(_sessionClients[connection], Is.SameAs(client));
		});
	}
	[Test]
	public void RegisterClientClientExists()
	{
		const string connection = "testConnection2";
		TestClient client = (TestClient) _sessionClients["testConnection"];
		_session.RegisterClient(connection, client);
		Assert.Multiple(() =>
		{
			Assert.That(_sessionClients.ContainsKey(connection));
			Assert.That(_sessionClients[connection], Is.SameAs(client));
		});
	}

	[Test]
	public void DisposeClient()
	{
		const string connection = "testConnection";
		_session.DisposeClient(connection);
		Assert.That(!_sessionClients.ContainsKey(connection));
	}
	[Test]
	public void DisposeClientLast()
    {
        const string connection = "testConnection";
		bool checkSignal = false;
		_session.Close += _ => { checkSignal = true; };
		
		_session.DisposeClient(connection);
        Assert.Multiple(() =>
        {
            Assert.That(!_sessionClients.ContainsKey(connection));
            Assert.That(checkSignal);
        });
    }

    [Test]
	public void DisposeClientInvalid()
	{
		const string connection = "abcd";
		_session.DisposeClient(connection);
		Assert.That(!_sessionClients.ContainsKey(connection));
	}

	[Test]
	public void CombineRevisions()
	{
		string combined = _session.CombineRevisions();
		
		Assert.That(combined, Is.EqualTo("{\"id\":0,\"layers\":[{\"timestampISO\":\"2025-03-27T12:45:45.951Z\",\"content\":[]}],\"legend\":[],\"toRemoveValue\":0,\"toRemoveArray\":[0],\"toCombineArray\":[123,123,123]}"));
	}
	[Test]
	public void CombineRevisionsEmpty()
	{
		_sessionRevisions.Clear();
		string combined = _session.CombineRevisions();
		
		Assert.That(combined, Is.EqualTo("{\"id\":0,\"layers\":[{\"timestampISO\":\"2025-03-27T12:45:45.951Z\",\"content\":[]}],\"legend\":[],\"toRemoveValue\":0,\"toRemoveArray\":[0],\"toCombineArray\":[]}"));
	}
}