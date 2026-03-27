using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string connectionString = "Server=.;Database=TestDb;User Id=sa;Password=YourPassword123";

var apiKey = "sk-1234567890-secret-key";

app.MapGet("/apikey", () => apiKey);

app.MapGet("/user", async (HttpRequest request) =>
{
    string name = request.Query["name"];

    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();

    string query = $"SELECT * FROM Users WHERE Name = '{name}'";

    var cmd = new SqlCommand(query, conn);
    var reader = await cmd.ExecuteReaderAsync();

    var results = new List<string>();
    while (await reader.ReadAsync())
    {
        results.Add(reader["Name"].ToString());
    }

    return Results.Ok(results);
});


app.MapPost("/deserialize", async (HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    var obj = JsonSerializer.Deserialize<object>(body);

    return Results.Ok(obj);
});

app.MapGet("/admin", () =>
{
    return Results.Ok("Sensitive admin data");
});

app.MapGet("/file", (string path) =>
{
    var content = File.ReadAllText(path);

    return Results.Ok(content);
});

app.MapGet("/login", (string user, string pass) =>
{
    if (user == "admin" && pass == "1234")
        return Results.Ok("Logged in");

    return Results.Unauthorized();
});

app.MapGet("/redirect", (string url) =>
{
    return Results.Redirect(url);
});

app.MapGet("/jwt", (string token) =>
{
    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var jwt = handler.ReadJwtToken(token);

    return Results.Ok(jwt.Claims);
});

app.MapPost("/login2", (string user, string password, ILogger<Program> logger) =>
{
    logger.LogInformation($"User {user} logged in with password {password}");

    return Results.Ok();
});

app.MapGet("/fetch", async (string url) =>
{
    using var client = new HttpClient();

    var data = await client.GetStringAsync(url);

    return Results.Ok(data);
});

app.MapPost("/invoke", (string typeName, string methodName) =>
{
    var type = Type.GetType(typeName);
    var method = type?.GetMethod(methodName);

    var result = method?.Invoke(null, null);

    return Results.Ok(result);
});

app.MapPost("/temp", (string content) =>
{
    var path = "/tmp/data.txt";

    File.WriteAllText(path, content);

    return Results.Ok();
});

app.MapGet("/error", () =>
{
    try
    {
        throw new Exception("Sensitive internal error");
    }
    catch (Exception ex)
    {
        return Results.Ok(ex.ToString());
    }
});

app.MapPost("/create-user", ([FromBody] User user) =>
{
    return Results.Ok(user);
});

int balance = 1000;

app.MapPost("/withdraw", (int amount) =>
{
    if (balance >= amount)
    {
        balance -= amount;
    }

    return Results.Ok(balance);
});

app.Run();

public class User
{
    public string Username { get; set; }
    public bool IsAdmin { get; set; }
}
