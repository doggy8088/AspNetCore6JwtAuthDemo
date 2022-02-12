using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<JwtHelpers>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapPost("/signin", (LoginViewModel login, JwtHelpers jwt) =>
    {
        if (ValidateUser(login))
        {
            var token = jwt.GenerateToken(login.Username);
            return Results.Ok(new { token });
        }
        else
        {
            return Results.BadRequest();
        }
    })
    .WithName("SignIn")
    .AllowAnonymous();

app.MapGet("/claims", (ClaimsPrincipal user) =>
    {
        return Results.Ok(user.Claims.Select(p => new { p.Type, p.Value }));
    })
    .WithName("Claims")
    .RequireAuthorization();

app.MapGet("/username", (ClaimsPrincipal user) =>
    {
        return Results.Ok(user.Identity?.Name);
    })
    .WithName("Username")
    .RequireAuthorization();

app.MapGet("/jwtid", (ClaimsPrincipal user) =>
    {
        return Results.Ok(user.Claims.FirstOrDefault(p => p.Type == "jti")?.Value);
    })
    .WithName("JwtId")
    .RequireAuthorization();

bool ValidateUser(LoginViewModel login)
{
    return true;
}

app.Run();

record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record LoginViewModel(string Username, string Password);
