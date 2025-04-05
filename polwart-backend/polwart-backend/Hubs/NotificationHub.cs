using Microsoft.AspNetCore.SignalR;

namespace polwart_backend.Hubs;

public class NotificationHub : Hub
{
	public void Subscribe(int mapId)
	{
		if (!G.SessionsController.ConnectNotifications(Context, Clients.Caller, mapId))
			Clients.Caller.SendAsync("SubscribtionFailed");
	}
	
	public Task NotifyChanges()
	{
		Session? session = G.SessionsController.GetClientSession(Context.ConnectionId);
		if (session == null)
		{
			return Task.CompletedTask;
		}
		
		List<Task> tasks = [];

		foreach (ISingleClientProxy client in session.GetClients())
		{
			tasks.Add(client.SendAsync("RevisionsUpdate"));
		}
		
		return Task.WhenAll(tasks);
	}
}