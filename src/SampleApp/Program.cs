var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () =>
{
    Console.WriteLine("GET / - Hello World endpoint called");
    return "Hello World!";
});

app.MapGet("/api/test", () =>
{
    Console.WriteLine("GET /api/test - Test endpoint called");
    return new { Message = "Test successful", Timestamp = DateTime.UtcNow };
});

app.MapPost("/api/echo", (EchoRequest request) =>
{
    Console.WriteLine($"POST /api/echo - Received: {request.Message}");
    return new EchoResponse(request.Message, DateTime.UtcNow);
});

app.Run();

public record EchoRequest(string Message);
public record EchoResponse(string Echo, DateTime Timestamp);

// Make Program accessible for WebApplicationFactory
public partial class Program { }
