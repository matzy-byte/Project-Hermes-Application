dotnet publish .\backend\backend.csproj -c Release -o .\bundle\backend
dotnet publish .\shared\shared.csproj -c Release -o .\bundle\shared
godot --headless --export-release "Windows Desktop" --path ".\frontend"
