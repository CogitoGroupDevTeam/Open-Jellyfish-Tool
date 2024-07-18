
# Determine which OS we're building for, based on the first parameter
if($args[0] -eq "windows") {
	$buildFolder = "build"
    $buildTarget = "win-x64"
}elseif ($args[0] -eq "linux") {
	$buildFolder = "build"
    $buildTarget = "linux-x64"
} else {
    Write-Host "Invalid OS specified"
    Write-Host "Please provide one of following as the first parameter:"
    Write-Host "  linux"
    Write-Host "  windows"
    exit 1
}

# Fetch the annotated (NOT lightweight) tag for the current commit
$tagName = git describe --tags
if ($LastExitCode -ne 0) {
    Write-Host "Build failed - no tag on current commit"
    exit 1
}

# Process the semvar formatted tag (e.g. v1.0.3-rc1 or v2.4.0.1)
$metabuild = $tagName.split("-") 
$versions = $metabuild[0].split(".")
$versions[0] -match '[0-9]+' | Out-Null
$vMajor = $matches[0]
$versions[1] -match '[0-9]+' | Out-Null
$vMinor = $matches[0]
$versions[2] -match '[0-9]+' | Out-Null
$vPatch = $matches[0]
$metabuild[1] -match '[0-9]+' | Out-Null
$vMeta = $matches[0]

Write-Host "Building Version: ${vMajor}.${vMinor}.${vPatch}.${vMeta}"

if ($vMajor -eq "" -or $vMinor -eq "" -or $vPatch -eq "") {
    Write-Host "Invalid version tag: ${tagName}"
}

$VersionMajor = if ($null -eq [int]$vMajor) { 0 } else { [int]$vMajor }
$VersionMinor = if ($null -eq [int]$vMinor) { 0 } else { [int]$vMinor }
$VersionPatch = if ($null -eq [int]$vPatch) { 0 } else { [int]$vPatch }
$VersionMeta =  if ($null -eq [int]$vMeta) { 0 } else { [int]$vMeta }

# Apply version number to csproj
$CSProj = "./OpenJellyfishTool.csproj"
[xml]$CSProjContents = Get-Content -Path $CSProj
$CSProjContents.Project.PropertyGroup.Version = "${VersionMajor}.${VersionMinor}.${VersionPatch}"
$CSProjContents.Project.PropertyGroup.AssemblyVersion = "${VersionMajor}.${VersionMinor}.${VersionPatch}.${VersionMeta}"
$CSProjContents.Save($CSProj)

dotnet publish OpenJellyfishTool.csproj -c Release -r $buildTarget -o $buildFolder -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --self-contained true

# Move artifacts
Remove-Item "${buildFolder}\*.pdb"
Copy-Item -Path "README.md" -Destination "${buildFolder}"

# Build completed
Write-Host "Successfully built for $buildTarget in $buildFolder"
