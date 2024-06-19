using System;
using RaindropLobotomy.Enemies;

namespace RaindropLobotomy.Ordeals.Noon.Green {
    public class ProcessOfUnderstanding : EnemyBase<ProcessOfUnderstanding>
    {
        public LazyIndex noon = new("GreenNoonBody");
        public static DamageAPI.ModdedDamageType NoonInvulnFrontal = DamageAPI.ReserveDamageType();

        public override string ConfigName => "Process of Understanding";

        public override void LoadPrefabs()
        {
            prefab = Load<GameObject>("GreenNoonBody.prefab");
            prefabMaster = Load<GameObject>("GreenNoonMaster.prefab");

            prefab.GetComponent<CharacterBody>().preferredPodPrefab = Assets.GameObject.RoboCratePod;

            RegisterEnemy(prefab, prefabMaster);

            LanguageAPI.Add("RL_UNDERSTANDING_NAME", "Process Of Understanding");
            LanguageAPI.Add("RL_UNDERSTANDING_SUB", "Ordeal of Green Noon");

            On.RoR2.DamageInfo.ModifyDamageInfo += (orig, self, modifier) => {
                orig(self, modifier);

                if (modifier == HurtBox.DamageModifier.Barrier) {
                    self.AddModdedDamageType(NoonInvulnFrontal);
                }
            };

            On.RoR2.HealthComponent.TakeDamage += (orig, self, info) => {
                if (self.body.bodyIndex == noon && info.HasModdedDamageType(NoonInvulnFrontal)) {
                    info.rejected = true;

                    EffectManager.SpawnEffect(RoR2.HealthComponent.AssetReferences.damageRejectedPrefab, new EffectData
                    {
                        origin = info.position
                    }, transmit: true);
                }

                orig(self, info);
            };

            On.EntityStates.BasicMeleeAttack.AuthorityTriggerHitPause += (orig, self) => {
                if (self.GetType() == typeof(Saw)) return;

                orig(self);
            };
        }
    }
}