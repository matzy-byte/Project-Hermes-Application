dotnet publish .\backend\backend.csproj -c Release -o .\bundle\backend
dotnet publish .\shared\shared.csproj -c Release -o .\bundle\shared
"C:\Program Files\Godot_mono\Godot_v4.6.1-stable_mono_win64.exe" --headless --export-release "Windows Desktop" --path ".\frontend"
