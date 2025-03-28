﻿using Microsoft.AspNetCore.SignalR;
using polwart_backend.Requests;

namespace polwart_backend;

public class SessionsController
{
	private readonly Dictionary<int, Session> _sessionsPerMap = [];
	private readonly Dictionary<string, Session> _sessionsPerConnections = [];

	private Session CreateSession(ConnectRequest request, string mapData)
	{
		Session session = new(request.MapId, mapData);
		_sessionsPerMap.Add(request.MapId, session);

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
		
		session.RegisterClient(connection);

		return true;
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
	
	public void CloseSession(int id)
	{
		_sessionsPerMap.Remove(id);
	}
}