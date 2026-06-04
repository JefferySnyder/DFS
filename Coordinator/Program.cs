using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Metadata store: Maps FileName -> List of Block IDs
var FileMetadata = new Dictionary<string, List<string>>();

// List of available active storage nodes
var StorageNodes = new List<string> { "http://localhost:5001", "http://localhost:5002" };

// Request allocation layout for a new file
app.MapPost("/files/allocate", ([FromQuery] string fileName, [FromQuery] int blockCount) =>
{
    if (FileMetadata.ContainsKey(fileName)) return Results.BadRequest("File already exists.");

    var assignedBlocks = new List<BlockAssignment>();

    for (int i = 0; i < blockCount; i++)
    {
        string blockId = Guid.NewGuid().ToString();

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
    if (!FileMetadata.TryGetValue(fileName, out var blocks)) return Results.NotFound();
    return Results.Ok(blocks); // Returns the ordered list of blocks to fetch
});

app.Run("http://localhost:5000");

public record BlockAssignment(string BlockId, string NodeUrl);