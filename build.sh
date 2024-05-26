rm -rf RaindropLobotomy/bin
dotnet restore
dotnet build
cp RaindropLobotomy/enkephalin RaindropLobotomy/bin/Debug/netstandard2.0/
rm -rf ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/RaindropLobotomy/BepInEx/plugins/RaindropLobotomy
cp -r WwiseRL/* RaindropLobotomy/bin/Debug/netstandard2.0/
cp -r RaindropLobotomy/bin/Debug/netstandard2.0/  ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/RaindropLobotomy/BepInEx/plugins/RaindropLobotomy

rm -rf RLBuild
mkdir RLBuild
cp -r WwiseRL/* RLBuild
cp manifest.json RLBuild
cp README.md RLBuild
cp RaindropLobotomy/enkephalin RLBuild
cp RaindropLobotomy/bin/Debug/netstandard2.0/RaindropLobotomy.dll RLBuild

rm RaindropLobotomy.zip
cd RLBuild
zip -r ../RaindropLobotomy.zip *