rm -rf RaindropLobotomy/bin
dotnet restore
dotnet build
cp RaindropLobotomy/enkephalin RaindropLobotomy/bin/Debug/netstandard2.0/
rm -rf ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/RaindropLobotomy/BepInEx/plugins/RaindropLobotomy
cp -r RaindropLobotomy/bin/Debug/netstandard2.0/  ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/RaindropLobotomy/BepInEx/plugins/RaindropLobotomy