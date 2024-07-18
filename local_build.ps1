
# --self-contained false means the binary requires the runtime, but is much lighter weight.
(dotnet publish OpenJellyfishTool.csproj -c Release -r linux-x64 --self-contained false -o publish)
if ($LastExitCode -eq 0) {
    Write-Host "Linux x64 built successfully"    
}
else {
    return
}
(dotnet publish OpenJellyfishTool.csproj -c Release -r win-x64 --self-contained false -o publish)
if ($LastExitCode -eq 0) {
    Write-Host "Win x64 built successfully"    
}
else {
    return
}
