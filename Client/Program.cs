using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using var httpClient = new HttpClient();
const int BlockSize = 1024 * 1024; // 1 MB Blocks
const string CoordinatorUrl = "http://localhost:5000";

// --- WRITE PATH ---
async Task UploadFileAsync(string localPath, string dfsFileName)
{
    var fileBytes = await File.ReadAllBytesAsync(localPath);
    int totalBlocks = (int)Math.Ceiling((double)fileBytes.Length / BlockSize);

    // Ask coordinator for a storage plan
    var response = await httpClient.PostAsync($"{CoordinatorUrl}/files/allocate?fileName={dfsFileName}&blockCount={totalBlocks}", null);
    var planJson = await response.Content.ReadAsStringAsync();
    var assignments = JsonSerializer.Deserialize<BlockAssignment[]>(planJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    if (assignments is null)
    {
        Console.WriteLine("Failed to allocate blocks");
        return;
    }

    // Upload blocks to target nodes
    for (int i = 0; i < assignments.Length; i++)
    {
        int offset = i * BlockSize;
        int count = Math.Min(BlockSize, fileBytes.Length - offset);
        var blockData = new MemoryStream(fileBytes, offset, count);

        var uploadContent = new StreamContent(blockData);
        await httpClient.PostAsync($"{assignments[i].NodeUrl}/blocks/{assignments[i].BlockId}", uploadContent);
    }
    Console.WriteLine("Distributed upload complete.");
}

await UploadFileAsync("", "");

public record BlockAssignment(string BlockId, string NodeUrl);

