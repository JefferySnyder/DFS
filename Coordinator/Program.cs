using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Metadata store: Maps FileName -> List of Block IDs
var FileMetadata = new Dictionary<string, List<string>>();

var StorageNodes = new List<string>();

// Request allocation layout for a new file
app.MapPost("/files/allocate", ([FromQuery] string fileName, [FromQuery] int blockCount) =>
{
    if (FileMetadata.ContainsKey(fileName)) return Results.BadRequest("File already exists.");
    if (StorageNodes.Count == 0) return Results.Problem(detail: "No DataNodes available", statusCode: 500);

    var assignedBlocks = new List<BlockAssignment>();

    for (int i = 0; i < blockCount; i++)
    {
        string blockId = Guid.NewGuid().ToString() + $"#{i}";

        // Round-robin selection of storage nodes
        string targetNode = StorageNodes[i % StorageNodes.Count];

        assignedBlocks.Add(new BlockAssignment(blockId, targetNode));
        
        if (!FileMetadata.ContainsKey(fileName)) FileMetadata[fileName] = new List<string>();
        FileMetadata[fileName].Add(blockId);
    }

    return Results.Ok(assignedBlocks);
});

// Lookup layout for reading an existing file
app.MapGet("/files/lookup", ([FromQuery] string fileName) =>
{
    if (!FileMetadata.TryGetValue(fileName, out var blockIds)) return Results.NotFound();
    if (StorageNodes.Count == 0) return Results.Problem(detail: "No DataNodes available", statusCode: 500);

    var retrievedBlocks = new List<BlockAssignment>();
    for (int i = 0; i < blockIds.Count; i++)
    {
        retrievedBlocks.Add(new BlockAssignment(blockIds[i], StorageNodes[i % StorageNodes.Count]));
    }
    return Results.Ok(retrievedBlocks); // Returns the ordered list of blocks to fetch
});

app.MapGet("ping/{port}", (int port) =>
{
    var node = $"http://localhost:{port}";
    if (!StorageNodes.Contains(node))
    {
        StorageNodes.Add(node);
        Console.WriteLine($"Added {node}");
    }
});

app.Run("http://localhost:5000");

public record BlockAssignment(string BlockId, string NodeUrl);