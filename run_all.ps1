$projects = @(
	"C:\Users\Jeffery.Snyder\source\others\DFS\DataNode\DataNode.csproj"
	"C:\Users\Jeffery.Snyder\source\others\DFS\Coordinator\Coordinator.csproj"
	"C:\Users\Jeffery.Snyder\source\others\DFS\Client\Client.csproj"
)

# Launch each project in its own console window
foreach ($project in $projects) {
    Write-Host "Launching background window for: $project" -ForegroundColor Cyan
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run --project $project"
	Start-Sleep -Seconds 5
}

