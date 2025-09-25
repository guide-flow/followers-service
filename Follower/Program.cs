using DotNetEnv;
using Follower;
using Follower.Startup;
using Neo4j.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwagger();

if (builder.Environment.IsDevelopment())
{
    Env.Load();
}

builder.Services.ConfigureNeo4j(builder.Configuration);
builder.Services.ConfigureAuth(builder.Configuration);

var app = builder.Build();

app.MapGet("/health/db", async (IDriver driver) =>
{
    try
    {
        await using var session = driver.AsyncSession(o => o.WithDefaultAccessMode(AccessMode.Read));
        var cursor = await session.RunAsync("RETURN 1 AS ok");
        await cursor.SingleAsync();
        return Results.Ok(new { neo4j = "up" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 503);
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("_allowDevClients");
app.UseAuthorization();

app.MapControllers();

app.Run();
