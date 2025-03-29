using Microsoft.AspNetCore.SignalR;

namespace polwart_backend.Hubs;

public class NotificationHub : Hub
{
	public void Subscribe(int mapId)
	{
		Console.WriteLine("Subscribe signal");
		
		if (!G.SessionsController.ConnectNotifications(Context.ConnectionId, Clients.Caller, mapId))
			Clients.Caller.SendAsync("SubscribtionFailed");
	}
	
	public Task NotifyChanges()
	{
		Console.WriteLine("Notify signal");
		
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