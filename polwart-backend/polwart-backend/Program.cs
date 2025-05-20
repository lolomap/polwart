#define VERBOSE_RESPONSES
#undef VERBOSE_RESPONSES

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using polwart_backend;
using polwart_backend.Entities;
using polwart_backend.Hubs;
using polwart_backend.Requests;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("Bearer", new()
	{
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.Http,
		Scheme = "Bearer"
	});
	options.AddSecurityRequirement(new()
	{
		{
			new()
			{
				Reference = new()
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			Array.Empty<string>()
		}
	});
});
builder.Services.AddSignalR();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = AuthOptions.ISSUER,
			ValidateAudience = true,
			ValidAudience = AuthOptions.AUDIENCE,
			ValidateLifetime = true,
			IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
			ValidateIssuerSigningKey = true,
		};
	});

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
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/notification");

app.MapPost("/register", async (RegisterRequest request) =>
	{
		await using ApplicationContext db = new();

		User? existedUser = await db.Users.FirstOrDefaultAsync(x => x.Login == request.Login);
		if (existedUser != null)
		{
			return Results.Conflict();
		}

		User user = new()
		{
			Login = request.Login,
			Password = PasswordHelper.HashPassword(request.Password)
		};

		await db.Users.AddAsync(user);
		await db.SaveChangesAsync();

		List<Claim> claims = [
			new(JwtRegisteredClaimNames.Name, user.Login),
			new(JwtRegisteredClaimNames.Sub, user.Id.ToString())
		];
		JwtSecurityToken jwt = new(
			issuer: AuthOptions.ISSUER,
			audience: AuthOptions.AUDIENCE,
			claims: claims,
			expires: DateTime.UtcNow.Add(TimeSpan.FromDays(1)),
			signingCredentials: new(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

		return Results.Json(new {JWT = new JwtSecurityTokenHandler().WriteToken(jwt)},
			contentType: "application/json", statusCode: 200);
	})
	.WithName("Register")
	.WithOpenApi();

app.MapPost("/login", async (LoginRequest request) =>
	{
		await using ApplicationContext db = new();

		User? user = await db.Users.FirstOrDefaultAsync(x => x.Login == request.Login);
		if (user == null) return Results.NotFound();

		if (!PasswordHelper.VerifyHashedPassword(user.Password, request.Password))
			return Results.Unauthorized();

		List<Claim> claims = [
			new(JwtRegisteredClaimNames.Name, user.Login),
			new(JwtRegisteredClaimNames.Sub, user.Id.ToString())
		];
		JwtSecurityToken jwt = new(
			issuer: AuthOptions.ISSUER,
			audience: AuthOptions.AUDIENCE,
			claims: claims,
			expires: DateTime.UtcNow.Add(TimeSpan.FromDays(1)),
			signingCredentials: new(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

		return Results.Json(new {JWT = new JwtSecurityTokenHandler().WriteToken(jwt)},
			contentType: "application/json", statusCode: 200);
	})
	.WithName("Login")
	.WithOpenApi();

app.MapPost("/map/connect", async (ConnectRequest request,  HttpContext context) =>
	{
		Session? session = await G.SessionsController.Connect(request);
		if (session == null)
			return Results.NotFound();

		if (!session.MapInfo.IsPublic)
		{
			int userId = Convert.ToInt32(
				context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
			if (!(session.MapInfo.Editors ?? []).Contains(userId))
				return Results.Unauthorized();
		}

		return Results.Json(
			new
			{
				session.MapInfo,
				Root = JsonSerializer.Deserialize<Dictionary<string, object>>(session.MapInfo.Content),
				Revisions = session.GetRevisions(0).Select(x => JsonSerializer.Serialize(x.PatchData))
			},
			contentType: "application/json", statusCode: 200);
	})
	.WithName("ConnectMap")
	.WithOpenApi();

app.MapPatch("/map/patch", [Authorize] (PatchRequest request, HttpContext context) =>
	{
		if (!G.SessionsController.Patch(request, Convert.ToInt32(
			    context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value)
		    ))
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

app.MapPost("/map/create", [Authorize] async (CreateMapRequest request, HttpContext context) =>
	{
		int userId = Convert.ToInt32(
			context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
		
		await using ApplicationContext db = new();
		Map map = new()
		{
			IsPublic = request.IsPublic,
			Editors = [userId],
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

app.MapPost("/map/changeEditors", [Authorize] async (ChangeEditorsRequest request, HttpContext context) =>
	{
		Session? session = G.SessionsController.GetMapSession(request.MapId);
		if (session == null)
			return Results.NotFound();
		
		int userId = Convert.ToInt32(
			context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
		
		// Map creator is always first in editors list 
		if ((session.MapInfo.Editors ?? []).FindIndex(x => x == userId) != 0)
			return Results.Unauthorized();
		if (request.Editors.Count < 1 || request.Editors[0] != userId)
			return Results.BadRequest();
			
		await using ApplicationContext db = new();
		Map? map = db.Maps.FirstOrDefault(x => x.Id == request.MapId);
		if (map == null)
			return Results.NotFound();

		map.Editors = request.Editors;
		await db.SaveChangesAsync();
		return Results.Ok();
	})
	.WithName("ChangeEditorsMap")
	.WithOpenApi();

app.Run();