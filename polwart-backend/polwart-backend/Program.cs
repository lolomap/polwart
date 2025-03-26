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
		string json = "{\"foo\": \"bar\"}";

		G.SessionsController.Connect(request, json);
	})
	.WithName("ConnectMap")
	.WithOpenApi();

app.Run();