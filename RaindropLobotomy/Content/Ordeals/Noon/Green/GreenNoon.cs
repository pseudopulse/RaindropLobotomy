using System;
using System.Linq;

namespace RaindropLobotomy.Ordeals.Noon.Green {
    public class GreenNoon : OrdealBase
    {
        public override OrdealLevel OrdealLevel => OrdealLevel.NOON;

        public override string Name => "Process of Understanding";

        public override string Subtitle => "In the end, they were bound to life. We existed only to express despair and ire.";

        public override string RiskTitle => "Ordeal of Green Noon";

        public override Color32 Color => new(23, 240, 0, 255);

        public override void OnSpawnOrdeal(RoR2.Stage stage)
        {
            for (int i = 0; i < 3; i++) {
                PlayerCharacterMasterController[] masters = PlayerCharacterMasterController.instances.Where(x => x.body && x.body.healthComponent.alive).ToArray();

                PlayerCharacterMasterController master = masters.GetRandom();

                for (int j = 0; j < 2; j++) {
                    DirectorPlacementRule rule = new();
                    rule.maxDistance = 20;
                    rule.placementMode = DirectorPlacementRule.PlacementMode.NearestNode;
                    rule.position = PickSpawnPosition(master.body.corePosition, 20f, 50f);

                    DirectorSpawnRequest spawnReq = new(Load<CharacterSpawnCard>("cscGreenNoon.asset"), rule, Run.instance.spawnRng);
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