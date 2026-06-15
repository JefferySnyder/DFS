using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

void CreateNode(int port)
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddHttpClient();
    builder.Services.AddSingleton(new SharedState(port));
    builder.Services.AddHostedService<TimedRequestService>();
    var app = builder.Build();

    // Ensure the local block directory exists
    const string StorageDir = "./BlockData";
    Directory.CreateDirectory(StorageDir);

    // 1. Write a raw block to disk
    app.MapPost("/blocks/{blockId}", async (string blockId, HttpRequest request) =>
    {
        var filePath = Path.Combine(StorageDir, blockId);
        using var fileStream = File.Create(filePath);
        await request.Body.CopyToAsync(fileStream);
        return Results.Ok(new { Message = $"Block {blockId} stored successfully." });
    });

    // 2. Read a raw block from disk
    app.MapGet("/blocks/{blockId}", async (string blockId) =>
    {
        var filePath = Path.Combine(StorageDir, blockId);
        if (!File.Exists(filePath)) return Results.NotFound();
        
        var bytes = await File.ReadAllBytesAsync(filePath);
        return Results.Bytes(bytes, "application/octet-stream");
    });

    app.RunAsync($"http://localhost:{port}"); // Run other nodes on 5002, 5003, etc.
}

const int NODE_COUNT = 3;

for (int i = 1; i <= NODE_COUNT; i++)
{
    CreateNode(5000 + i);
}
Console.ReadKey(true);