#define VERBOSE_RESPONSES

using System.Text.Json;
using polwart_backend;
using polwart_backend.Hubs;
using polwart_backend.Requests;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

const string allowSpecificOrigins = "AllowSpecificOrigins";
builder.Services.AddCors(options =>
{
	options.AddPolicy(name: allowSpecificOrigins,
		policy  =>
		{
			policy.WithOrigins("http://example.com",
				"http://www.contoso.com");
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
app.UseCors(allowSpecificOrigins);

app.MapHub<NotificationHub>("/notification");

app.MapPost("/map/connect", (ConnectRequest request) =>
	{
		//TODO: Get data from database. Don't forget to make request async for db usage
		string json = "{\n\t\"id\": 123,\n\t\"layers\": [\n\t\t{\n\t\t\t\"timestampISO\": \"2025-03-27T12:45:45.951Z\",\n\t\t\t\"content\": []\n\t\t}\n\t],\n\t\"legend\": []\n}";

		IEnumerable<Revision> revisions = G.SessionsController.Connect(request, json);

		return Results.Json(new {Root = JsonSerializer.Deserialize<Dictionary<string, object>>(json), Revisions = revisions},
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

app.Run();