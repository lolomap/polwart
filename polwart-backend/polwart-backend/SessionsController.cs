using System.Net;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using polwart_backend.Entities;
using polwart_backend.Requests;

namespace polwart_backend;

public class SessionsController
{
	private readonly Dictionary<int, Session> _sessionsPerMap = [];
	private readonly Dictionary<string, Session> _sessionsPerConnections = [];

	private Session CreateSession(ConnectRequest request, string mapData)
	{
		Session session = new(request.MapId, mapData);
		session.Close += CloseSession;
		_sessionsPerMap.Add(request.MapId, session);
		
		//TODO: use logging utilities
		Console.WriteLine($"OPEN SESSION FOR {request.MapId}");

		return session;
	}

	public IEnumerable<Revision> Connect(ConnectRequest request, string mapData)
	{
		if (!_sessionsPerMap.TryGetValue(request.MapId, out Session? session))
		{
			session = CreateSession(request, mapData);
		}

		return session.GetRevisions(0);
	}

	// TODO: Methods should return object with result, error and session
	public bool ConnectNotifications(string connectionId, ISingleClientProxy connection, int mapId)
	{
		if (!_sessionsPerMap.TryGetValue(mapId, out Session? session))
			return false; //TODO: Response error (missing session)

		if (!_sessionsPerConnections.TryAdd(connectionId, session))
			return false; //TODO: Response error (only one map at the moment for connection)

		session.RegisterClient(connectionId, connection);

		return true;
	}

	public void DisconnectNotifications(string connectionId)
	{
		_sessionsPerConnections.Remove(connectionId);
	}

	public bool Patch(PatchRequest request)
	{
		if (!_sessionsPerMap.TryGetValue(request.MapId, out Session? session))
			return false; //TODO: Response error (missing session)
		
		session.Patch(request);

		return true;
	}

	public Session? GetClientSession(string connectionId)
	{
		return _sessionsPerConnections.GetValueOrDefault(connectionId);
	}

	public Session? GetMapSession(int mapId)
	{
		return _sessionsPerMap.GetValueOrDefault(mapId);
	}

	private async void CloseSession(Session session)
	{
		await using ApplicationContext db = new();
		
		string updatedRoot = session.CombineRevisions();
		Map? map = await db.Maps.FirstOrDefaultAsync(x => x.Id == session.MapId);
		if (map == null) return;
		map.Content = updatedRoot;
		await db.SaveChangesAsync();
		
		_sessionsPerMap.Remove(session.MapId);
		foreach (string connectionId in session.GetClientsEndpoints())
		{
			_sessionsPerConnections.Remove(connectionId);
		}
		
		Console.WriteLine($"CLOSE SESSION FOR {session.MapId}");
	}
}