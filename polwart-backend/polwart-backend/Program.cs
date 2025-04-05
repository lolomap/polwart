#define VERBOSE_RESPONSES

using System.Text.Json;
using polwart_backend;
using polwart_backend.Hubs;
using polwart_backend.Requests;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(
		policy  =>
		{
			policy
				.AllowAnyOrigin()
				.AllowAnyHeader()
				.AllowAnyMethod()
				.AllowCredentials()
				.WithOrigins("http://localhost:5173");
		});
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapHub<NotificationHub>("/notification");

//TODO: make requests async

app.MapPost("/map/connect", (ConnectRequest request) =>
	{
		//TODO: Get data from database. Don't forget to make request async for db usage
		string json = "{\n\t\"id\": 123,\n\t\"layers\": [\n\t\t{\n\t\t\t\"timestampISO\": \"2025-03-27T12:45:45.951Z\",\n\t\t\t\"content\": []\n\t\t}\n\t],\n\t\"legend\": []\n}";

		IEnumerable<Revision> revisions = G.SessionsController.Connect(request, json);

		return Results.Json(
			new
			{
				Root = JsonSerializer.Deserialize<Dictionary<string, object>>(json),
				Revisions = revisions.Select(x => JsonSerializer.Serialize(x.PatchData))
			},
			contentType: "application/json", statusCode: 200);
	})
	.WithName("ConnectMap")
	.WithOpenApi();

app.MapPatch("/map/patch", (PatchRequest request) =>
	{
		if (!G.SessionsController.Patch(request))
		{
			return Results.NotFound();
		}
		#if VERBOSE_RESPONSES
		else
		{
			return Results.Json(G.SessionsController.GetMapSession(request.MapId)!.CombineRevisions(),
				contentType: "application/json", statusCode: 200);
		}
		#endif
	})
	.WithName("PatchMap")
	.WithOpenApi();

app.MapPost("/map/update", (UpdateRequest request) =>
	{
		Session? session = G.SessionsController.GetMapSession(request.MapId);
		return session == null
			? Results.NotFound()
			: Results.Json(session.GetRevisions(request.SinceTimestamp)
				.Select(x => JsonSerializer.Serialize(x.PatchData)));
	})
	.WithName("UpdateMap")
	.WithOpenApi();

app.Run();