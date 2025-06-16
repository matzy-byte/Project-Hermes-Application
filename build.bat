dotnet publish .\backend\backend.csproj -c Release -o .\bundle\backend
godot --headless --export-release "Windows Desktop" --path ".\frontend"
