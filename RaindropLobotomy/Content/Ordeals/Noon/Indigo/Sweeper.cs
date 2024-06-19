using System;
using RaindropLobotomy.Enemies;

namespace RaindropLobotomy.Ordeals.Noon.Indigo {
    public class Sweeper : EnemyBase<Sweeper>
    {
        public static CharacterSpawnCard cscSweeper;
        public static GameObject BodyPrefab;

        public override string ConfigName => "A Sweeper (Enemy)";

        public override void LoadPrefabs()
        {
            prefab = Load<GameObject>("SweeperAIBody.prefab");
            prefabMaster = Load<GameObject>("SweeperAIMaster.prefab");

            BodyPrefab = prefab;
            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Assets.GameObject.GenericFootstepDust;

            RegisterEnemy(prefab, prefabMaster);

            cscSweeper = Load<CharacterSpawnCard>("cscSweeper.asset");
        }
    }
}