using Microsoft.Data.SqlClient;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string connectionString = "Server=.;Database=TestDb;User Id=sa;Password=YourPassword123";

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

app.MapPost("/run", (string cmd) =>
{
    var process = new System.Diagnostics.Process();
    process.StartInfo.FileName = "cmd.exe";
    process.StartInfo.Arguments = "/c " + cmd;
    process.Start();

    return Results.Ok("Command executed");
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

app.Run();
