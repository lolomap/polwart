using System.Reflection;
using Json.Patch;
using Microsoft.AspNetCore.SignalR;
using NUnit.Framework;
using polwart_backend;
using polwart_backend.Entities;
using polwart_backend.Requests;

namespace polwart_backend_tests;

public class SessionsControllerTests
{
	private SessionsController _sessionsController = null!;
	private Dictionary<int, Session> _sessionsPerMap = null!;
	private Dictionary<string, Session> _sessionsPerConnections = null!;
	
	[SetUp]
	public void Setup()
	{
		_sessionsController = new();

		_sessionsPerMap = (Dictionary<int, Session>)
			typeof(SessionsController).GetField("_sessionsPerMap", BindingFlags.NonPublic | BindingFlags.Instance)!
				.GetValue(_sessionsController)!;

		_sessionsPerConnections = (Dictionary<string, Session>)
			typeof(SessionsController).GetField("_sessionsPerConnections", BindingFlags.NonPublic | BindingFlags.Instance)!
				.GetValue(_sessionsController)!;
		
		Map mapInfo = new()
		{
			Id = 0,
			Editors = [0, 1],
			Content = "{\"id\": 0,\"layers\": [{\"timestampISO\": \"2025-03-27T12:45:45.951Z\",\"content\": []}],\"legend\": [],\"toRemoveValue\": 0, \"toRemoveArray\": [0], \"toCombineArray\":[]}"
		};
		
		Session session = new(mapInfo);
		_sessionsPerMap.Add(0, session);
		_sessionsPerConnections.Add("testConnection", session);
	}

	[Test]
	public void CreateSession()
    {
        Session? session = (Session?)
			typeof(SessionsController)
				.GetMethod("CreateSession", BindingFlags.NonPublic | BindingFlags.Instance)!
				.Invoke(_sessionsController, [new ConnectRequest {MapId = 123}, new Map
					{
						Id = 123,
						Name = "",
						IsPublic = true,
						BackgroundFormat = "",
						Editors = [0, 1],
						Content = "{}"
					}
				]);
        
        Assert.Multiple(() =>
        {
            Assert.That(session, Is.Not.Null);
            Assert.That(_sessionsPerMap.ContainsKey(123));
            Assert.That(_sessionsPerMap[123], Is.SameAs(session));
        });
    }
	[Test]
    public void CreateSessionAlreadyExists()
	{
		Session? session = (Session?)
			typeof(SessionsController)
				.GetMethod("CreateSession", BindingFlags.NonPublic | BindingFlags.Instance)!
				.Invoke(_sessionsController, [new ConnectRequest {MapId = 0}, new Map
					{
						Id = 0,
						Name = "",
						IsPublic = true,
						BackgroundFormat = "",
						Editors = [0, 1],
						Content = "{}"
					}
				]);
        
		Assert.That(session, Is.Null);
	}

	[Test]
	public async Task Connect()
	{
		Session? session = await _sessionsController.Connect(new() {MapId = 0});
		
		Assert.That(session, Is.SameAs(_sessionsPerMap[0]));
	}
	[Test]
	public async Task ConnectNewSession()
	{
		//TODO: Test Database Context
		Session? session = await _sessionsController.Connect(new() {MapId = 12});
		
		Assert.That(session, Is.SameAs(_sessionsPerMap[12]));
	}
	[Test]
	public async Task ConnectInvalid()
	{
		//TODO: Test Database Context
		Session? session = await _sessionsController.Connect(new() {MapId = -100});
		
		Assert.That(session, Is.Null);
	}

	[Test]
	public void ConnectNotifications()
    {
        const string connection = "testConnection2";
		bool result =
			_sessionsController.ConnectNotifications(connection, new TestClient(), 0);
		
        Assert.Multiple(() =>
        {
            Assert.That(result);
            Assert.That(_sessionsPerConnections.ContainsKey(connection));
            Assert.That(_sessionsPerConnections[connection], Is.SameAs(_sessionsPerMap[0]));
        });
    }

    [Test]
	public void ConnectNotificationsNoSession()
	{
		const string connection = "testConnection2";
		bool result =
			_sessionsController.ConnectNotifications(connection, new TestClient(), 123);
		
		Assert.That(!result);
	}
	[Test]
	public void ConnectNotificationsAlreadyExists()
	{
		const string connection = "testConnection";
		bool result =
			_sessionsController.ConnectNotifications(connection, new TestClient(), 123);
		
		Assert.That(!result);
	}

	[Test]
	public void DisconnectNotifications()
	{
		const string connection = "testConnection";
		_sessionsController.DisconnectNotifications(connection);
		
		Assert.That(!_sessionsPerConnections.ContainsKey(connection));
	}
	[Test]
	public void DisconnectNotificationsNoSession()
	{
		const string connection = "testConnection2";
		_sessionsController.DisconnectNotifications(connection);
		
		Assert.That(!_sessionsPerConnections.ContainsKey(connection));
	}

	[Test]
	public void Patch()
	{
		const string patchData = "[{\"op\": \"replace\",\"path\": \"/id\",\"value\": 0}]";
		bool result = _sessionsController.Patch(new() {MapId = 0, Patch = patchData, Timestamp = 0}, 1);
		
		Assert.That(result);
	}
	[Test]
	public void PatchNoSession()
	{
		const string patchData = "[{\"op\": \"replace\",\"path\": \"/id\",\"value\": 0}]";
		bool result = _sessionsController.Patch(new() {MapId = 1, Patch = patchData, Timestamp = 0}, 0);
		
		Assert.That(!result);
	}
	[Test]
	public void PatchNotEditor()
	{
		const string patchData = "[{\"op\": \"replace\",\"path\": \"/id\",\"value\": 0}]";
		bool result = _sessionsController.Patch(new() {MapId = 0, Patch = patchData, Timestamp = 0}, 2);
		
		Assert.That(!result);
	}

	[Test]
	public void CloseSession()
    {
	    //TODO: Test Database context
        typeof(SessionsController)
			.GetMethod("CloseSession", BindingFlags.NonPublic | BindingFlags.Instance)!
			.Invoke(_sessionsController, [_sessionsPerMap[0]]);
        
        Assert.Multiple(() =>
        {
            Assert.That(!_sessionsPerMap.ContainsKey(0));
            Assert.That(!_sessionsPerConnections.ContainsKey("testConnection"));
        });
    }

    [Test]
	public void CloseSessionInvalid()
	{
		Session session = new(new()
		{
			Id = -100,
			Editors = [90],
			Content = ""
		});
		
		//TODO: Test Database context
		
		Assert.Multiple(() =>
		{
			Assert.That(() => {
				typeof(SessionsController)
				.GetMethod("CloseSession", BindingFlags.NonPublic | BindingFlags.Instance)!
				.Invoke(_sessionsController, [session]);
			}, Throws.Nothing);
			Assert.That(!_sessionsPerMap.ContainsKey(-100));
		});
	}
}