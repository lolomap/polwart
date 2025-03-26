using polwart_backend;
using polwart_backend.Requests;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/map/connect", (ConnectRequest request) =>
	{
		// Get data from database
		string json = "{\"foo\": \"bar\"}";

		G.SessionsController.Connect(request, json);
	})
	.WithName("ConnectMap")
	.WithOpenApi();

app.Run();