#define VERBOSE_RESPONSES
#undef VERBOSE_RESPONSES

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
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

app.MapPost("/map/connect", async (ConnectRequest request) =>
	{
		await using ApplicationContext db = new();

		Map? map = await db.Maps.FirstOrDefaultAsync(x => x.Id == request.MapId);
		if (map == null)
			return Results.NotFound();
		string json = map.Content;

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
			: Results.Json(session.GetRevisions(request.SinceTimestamp)
				.Select(x => JsonSerializer.Serialize(x.PatchData)));
	})
	.WithName("UpdateMap")
	.WithOpenApi();

app.MapPost("/map/create", async (CreateMapRequest request) =>
	{
		await using ApplicationContext db = new();
		Map map = new()
		{
			IsPublic = request.IsPublic,
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
		return Results.Ok();
	})
	.WithName("CreateMap")
	.WithOpenApi();

app.Run();