using polwart_backend.Requests;

namespace polwart_backend;

public class SessionsController
{
	private readonly Dictionary<int, Session> _sessions = [];

	private Session CreateSession(ConnectRequest request, string mapData)
	{
		Session session = new(request.MapId, mapData);
		_sessions.Add(request.MapId, session);

		return session;
	}

	public void Connect(ConnectRequest request, string mapData)
	{
		if (!_sessions.TryGetValue(request.MapId, out Session? session))
		{
			session = CreateSession(request, mapData);
		}
	}

	public void CloseSession(int id)
	{
		_sessions.Remove(id);
	}
}