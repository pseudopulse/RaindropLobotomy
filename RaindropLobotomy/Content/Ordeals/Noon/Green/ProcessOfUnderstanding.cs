using System;
using RaindropLobotomy.Enemies;

namespace RaindropLobotomy.Ordeals.Noon.Green {
    public class ProcessOfUnderstanding : EnemyBase<ProcessOfUnderstanding>
    {
        public override void LoadPrefabs()
        {
            prefab = Load<GameObject>("GreenNoonBody.prefab");
            prefabMaster = Load<GameObject>("GreenNoonMaster.prefab");

            RegisterEnemy(prefab, prefabMaster);

            LanguageAPI.Add("RL_UNDERSTANDING_NAME", "Process Of Understanding");
            LanguageAPI.Add("RL_UNDERSTANDING_SUB", "Ordeal of Green Noon");
        }
    }
}