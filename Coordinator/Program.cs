using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<CoorShared>();
builder.Services.AddHostedService<NodeCheckService>();
var app = builder.Build();

// Metadata store: Maps FileName -> List of Block IDs
var FileMetadata = new Dictionary<string, List<string>>();

// Request allocation layout for a new file
app.MapPost("/files/allocate", ([FromQuery] string fileName, [FromQuery] int blockCount, CoorShared shared) =>
{
    if (FileMetadata.ContainsKey(fileName)) return Results.BadRequest("File already exists.");
    if (shared.StorageNodes.Count == 0) return Results.Problem(detail: "No DataNodes available", statusCode: 500);

    var assignedBlocks = new List<BlockAssignment>();

    for (int i = 0; i < blockCount; i++)
    {
        string blockId = Guid.NewGuid().ToString() + $"#{i}";

        // Round-robin selection of storage nodes
        string targetNode = shared.StorageNodes[i % shared.StorageNodes.Count].Name;

        assignedBlocks.Add(new BlockAssignment(blockId, targetNode));
        
        if (!FileMetadata.ContainsKey(fileName)) FileMetadata[fileName] = new List<string>();
        FileMetadata[fileName].Add(blockId);
    }

    return Results.Ok(assignedBlocks);
});

// Lookup layout for reading an existing file
app.MapGet("/files/lookup", ([FromQuery] string fileName, CoorShared shared) =>
{
    if (!FileMetadata.TryGetValue(fileName, out var blockIds)) return Results.NotFound();
    if (shared.StorageNodes.Count == 0) return Results.Problem(detail: "No DataNodes available", statusCode: 500);

    var retrievedBlocks = new List<BlockAssignment>();
    for (int i = 0; i < blockIds.Count; i++)
    {
        retrievedBlocks.Add(new BlockAssignment(blockIds[i], shared.StorageNodes[i % shared.StorageNodes.Count].Name));
    }
    return Results.Ok(retrievedBlocks); // Returns the ordered list of blocks to fetch
});

app.MapGet("ping/{port}", (int port, CoorShared shared) =>
{
    var nodeName = $"http://localhost:{port}";
    var nodeLifetime = new NodeLifetime(nodeName, DateTime.Now);

    var nodeIdx = shared.StorageNodes.FindIndex(x => x.Name == nodeName);
    if (nodeIdx == -1)
    {
        shared.StorageNodes.Add(nodeLifetime);
        Console.WriteLine($"Added {nodeLifetime}");
    }
    else 
    { 
        shared.StorageNodes[nodeIdx] = nodeLifetime;
        Console.WriteLine($"Updated {nodeLifetime}");
    }
});

app.Run("http://localhost:5000");

public record BlockAssignment(string BlockId, string NodeUrl);
public record NodeLifetime(string Name, DateTime Age);