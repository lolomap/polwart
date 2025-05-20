using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using polwart_backend.Entities;
using polwart_backend.Requests;

namespace polwart_backend;

public class SessionsController
{
	private readonly Dictionary<int, Session> _sessionsPerMap = [];
	private readonly Dictionary<string, Session> _sessionsPerConnections = [];

	private Session CreateSession(ConnectRequest request, Map map)
	{
		Session session = new(map);
		session.Close += CloseSession;
		_sessionsPerMap.Add(request.MapId, session);
		
		//TODO: use logging utilities
		Console.WriteLine($"OPEN SESSION FOR {request.MapId}");

		return session;
	}

	public async Task<Session?> Connect(ConnectRequest request)
	{
		if (!_sessionsPerMap.TryGetValue(request.MapId, out Session? session))
		{
			await using ApplicationContext db = new();

			Map? map = await db.Maps.FirstOrDefaultAsync(x => x.Id == request.MapId);
			if (map == null)
				return default;
			
			session = CreateSession(request, map);
		}

		return session;
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

	public bool Patch(PatchRequest request, int userId)
	{
		if (!_sessionsPerMap.TryGetValue(request.MapId, out Session? session))
			return false; //TODO: Response error (missing session)

		if (!(session.MapInfo.Editors ?? []).Contains(userId))
			return false; //TODO: Response error (unauthorized)
		
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

	private void CloseSession(Session session)
	{
		using ApplicationContext db = new();
		
		string updatedRoot = session.CombineRevisions();
		Map? map = db.Maps.FirstOrDefault(x => x.Id == session.MapInfo.Id);
		if (map == null) return;
		map.Content = updatedRoot;
		db.SaveChanges();
		
		_sessionsPerMap.Remove(session.MapInfo.Id);
		foreach (string connectionId in session.GetClientsEndpoints())
		{
			_sessionsPerConnections.Remove(connectionId);
		}
		
		Console.WriteLine($"CLOSE SESSION FOR {session.MapInfo.Id}");
	}
}