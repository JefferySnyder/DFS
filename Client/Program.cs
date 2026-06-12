using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using var httpClient = new HttpClient();
//const int BlockSize = 1024 * 1024; // 1 MB Blocks
const int BlockSize = 1024; // 1 KB Blocks
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

async Task RetrieveFileAsync(string dfsFileName, string storePath)
{
    // ask coordinator for block IDs associated with file
    var response = await httpClient.GetAsync($"{CoordinatorUrl}/files/lookup?fileName={dfsFileName}");
    var planJson = await response.Content.ReadAsStringAsync();
    var blockIds = JsonSerializer.Deserialize<string[]>(planJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    if (blockIds is null)
    {
        Console.WriteLine("Failed to retrieve block IDs");
        return;
    }

    const string ReadDataNodeUrl = "http://localhost:5001";
    // Retrieve data from nodes
    for (int i = 0; i < blockIds.Length; i++)
    {
        var filePath = Path.Combine(storePath, blockIds[i]);
        using var downloadStream = await httpClient.GetStreamAsync($"{ReadDataNodeUrl}/blocks/{blockIds[i]}");
        using var fileStream = File.Create(filePath);
        await downloadStream.CopyToAsync(fileStream);
    }
    Console.WriteLine("Distributed donwload complete.");
}

const string LocalPath = "C:/Users/Jeffery.Snyder/Documents/output.txt";
const string DfsFileName = "output.txt";
const string StorePath = "C:/Users/Jeffery.Snyder/Documents/output";

await UploadFileAsync(LocalPath, DfsFileName);

await RetrieveFileAsync(DfsFileName, StorePath);
public record BlockAssignment(string BlockId, string NodeUrl);

