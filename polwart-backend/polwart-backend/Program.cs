#define VERBOSE_RESPONSES
#undef VERBOSE_RESPONSES

using System.Text.Json;
using polwart_backend;
using polwart_backend.Entities;
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
				.WithOrigins(Environment.GetEnvironmentVariable("FRONTEND_ENDPOINT") ?? "*")
				.AllowAnyHeader()
				.AllowAnyMethod()
				.AllowCredentials();
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

app.MapPost("/map/connect", async (ConnectRequest request) =>
	{
		(Map? map, Session ? session) = await G.SessionsController.Connect(request);
		if (session == null || map == null)
			return Results.NotFound();

		return Results.Json(
			new
			{
				Map = map,
				Root = JsonSerializer.Deserialize<Dictionary<string, object>>(session.RootJson),
				Revisions = session.GetRevisions(0).Select(x => JsonSerializer.Serialize(x.PatchData))
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
		#else
		return Results.Ok();
		#endif
	})
	.WithName("PatchMap")
	.WithOpenApi();

app.MapPost("/map/update", (UpdateRequest request) =>
	{
		Session? session = G.SessionsController.GetMapSession(request.MapId);
		return session == null
			? Results.NotFound()
			: Results.Json(
				session.GetRevisions(request.SinceTimestamp)
					.Select(x => JsonSerializer.Serialize(x.PatchData)),
				contentType: "application/json", statusCode: 200);
	})
	.WithName("UpdateMap")
	.WithOpenApi();

app.MapPost("/map/create", async (CreateMapRequest request) =>
	{
		await using ApplicationContext db = new();
		Map map = new()
		{
			IsPublic = request.IsPublic,
			BackgroundFormat = request.BackgroundFormat,
			Content = JsonSerializer.Serialize(new
			{
				legend = (object[]) [],
				layers = (object[]) [
					new
					{
						content = (object[]) [],
						timestampISO = request.InitialTimestampISO
					}
				]
			})
		};

		await db.Maps.AddAsync(map);
		await db.SaveChangesAsync();
		return Results.Json(new {MapId = map.Id},
			contentType: "application/json", statusCode: 200);
	})
	.WithName("CreateMap")
	.WithOpenApi();

app.Run();