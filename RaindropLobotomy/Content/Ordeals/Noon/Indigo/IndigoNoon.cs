using System;
using System.Linq;

namespace RaindropLobotomy.Ordeals.Noon.Indigo {
    public class GreenIndigo : OrdealBase
    {
        public override OrdealLevel OrdealLevel => OrdealLevel.NOON;

        public override string Name => "The Sweepers";

        public override string Subtitle => "When night falls in the Backstreets, they will come.";

        public override string RiskTitle => "Ordeal of Indigo Noon";

        public override Color32 Color => new(75, 27, 196, 255);

        public override void OnSpawnOrdeal(RoR2.Stage stage)
        {
            for (int i = 0; i < 3; i++) {
                PlayerCharacterMasterController[] masters = PlayerCharacterMasterController.instances.Where(x => x.body && x.body.healthComponent.alive).ToArray();

                PlayerCharacterMasterController master = masters.GetRandom();

                for (int j = 0; j < 3; j++) {
                    DirectorPlacementRule rule = new();
                    rule.maxDistance = 20;
                    rule.placementMode = DirectorPlacementRule.PlacementMode.NearestNode;
                    rule.position = PickSpawnPosition(master.body.corePosition, 40f, 80f);

                    DirectorSpawnRequest spawnReq = new(Sweeper.cscSweeper, rule, Run.instance.spawnRng);
                    spawnReq.teamIndexOverride = TeamIndex.Monster;
                    spawnReq.onSpawnedServer = (res) => {
                        if (res.success) {
                            CharacterMaster cMaster = res.spawnedInstance.GetComponent<CharacterMaster>();
                            cMaster.inventory.GiveItem(RoR2Content.Items.UseAmbientLevel);
                        }
                    };

                    DirectorCore.instance.TrySpawnObject(spawnReq);
                }
            }
        }
    }
}