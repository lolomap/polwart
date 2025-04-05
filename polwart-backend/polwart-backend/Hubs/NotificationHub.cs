using Microsoft.AspNetCore.SignalR;

namespace polwart_backend.Hubs;

public class NotificationHub : Hub
{
	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		Session? session = G.SessionsController.GetClientSession(Context.ConnectionId);
		G.SessionsController.DisconnectNotifications(Context.ConnectionId);
		session?.DisposeClient(Context.ConnectionId);	
		
		await base.OnDisconnectedAsync(exception);
	}

	public void Subscribe(int mapId)
	{
		if (!G.SessionsController.ConnectNotifications(Context.ConnectionId, Clients.Caller, mapId))
		{
			Clients.Caller.SendAsync("SubscribtionFailed");
		}
	}
	
	public Task NotifyChanges()
	{
		Session? session = G.SessionsController.GetClientSession(Context.ConnectionId);
		if (session == null)
			return Task.CompletedTask;
		
		List<Task> tasks = [];

		foreach (ISingleClientProxy client in session.GetClientsConnections())
		{
			tasks.Add(client.SendAsync("RevisionsUpdate"));
		}
		
		return Task.WhenAll(tasks);
	}
}