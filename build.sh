rm -rf RaindropLobotomy/bin
dotnet restore
dotnet build
cp RaindropLobotomy/enkephalin RaindropLobotomy/bin/Debug/netstandard2.1/
rm -rf ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/RaindropLobotomy/BepInEx/plugins/RaindropLobotomy
cp -r WwiseRL/* RaindropLobotomy/bin/Debug/netstandard2.1/
cp -r RaindropLobotomy/bin/Debug/netstandard2.1/  ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/RaindropLobotomy/BepInEx/plugins/RaindropLobotomy
rm ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/RaindropLobotomy/BepInEx/plugins/RaindropLobotomy/PaladinMod.dll
rm ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/RaindropLobotomy/BepInEx/plugins/RaindropLobotomy/Decalicious.dll
rm ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/RaindropLobotomy/BepInEx/plugins/RaindropLobotomy/Survariants.dll
rm ~/.config/r2modmanPlus-local/RiskOfRain2/profiles/RaindropLobotomy/BepInEx/plugins/RaindropLobotomy/Unity.Postprocessing.Runtime.dll

rm -rf RLBuild
mkdir RLBuild
cp -r WwiseRL/* RLBuild
cp manifest.json RLBuild
cp icon.png RLBuild
cp README.md RLBuild
cp RaindropLobotomy/enkephalin RLBuild
cp RaindropLobotomy/bin/Debug/netstandard2.1/RaindropLobotomy.dll RLBuild

rm RaindropLobotomy.zip
cd RLBuild
zip -r ../RaindropLobotomy.zip *