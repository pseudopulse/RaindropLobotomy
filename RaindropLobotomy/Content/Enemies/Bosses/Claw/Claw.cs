using System;

namespace RaindropLobotomy.Enemies.Claw {
    public class Claw : EnemyBase<Claw>
    {
        public override void LoadPrefabs()
        {
            prefab = Load<GameObject>("ClawBody.prefab");
            // prefabMaster = Load<GameObject>("BinahMaster.prefab");

            RegisterEnemy(prefab, prefabMaster);

            "RL_CLAW_NAME".Add("A Claw");
            "RL_CLAW_SUB".Add("Executioner of the Claw");
        }
    }
}