using System;
using RaindropLobotomy.Survivors.Sweeper;

namespace RaindropLobotomy.Buffs {
    public class Persistence : BuffBase<Persistence>
    {
        public override BuffDef Buff => Load<BuffDef>("bdPersistence.asset");

        public override void PostCreation()
        {
            RecalculateStatsAPI.GetStatCoefficients += AddPersistanceBuffs;
            On.RoR2.HealthComponent.TakeDamageProcess += DamageNull;
            On.RoR2.GlobalEventManager.OnHitEnemy += RallySweepers;
            On.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += WarCry;
        }

        private void WarCry(On.RoR2.CharacterBody.orig_UpdateAllTemporaryVisualEffects orig, CharacterBody self)
        {
            orig(self);

            self.UpdateSingleTemporaryVisualEffect(ref self.teamWarCryEffectInstance, CharacterBody.AssetReferences.teamWarCryEffectPrefab, self.bestFitRadius, self.HasBuff(Buff) || self.HasBuff(RoR2Content.Buffs.TeamWarCry), "HeadCenter");
        }

        private void RallySweepers(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            if (damageInfo.attacker && victim && victim.GetComponent<CharacterBody>()) {
                CharacterBody attacker = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attacker && attacker.bodyIndex == Sweeper.SweeperIndex && attacker.HasBuff(Buff)) {
                    Sweeper.ForceSweeperTargets(attacker.master, victim.GetComponent<CharacterBody>());
                }
            }
        }

        private void DamageNull(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            int persistenceCount = self.body.GetBuffCount(Buff);

            if (persistenceCount > 0) {
                float current = self.health + self.shield + self.barrier;
                float armor = self.body.armor;
                float armorMult = ((armor >= 0f) ? (1f - armor / (armor + 100f)) : (2f - 100f / (100f - armor)));

                float damage = damageInfo.damage * armorMult;

                if ((current - damage) <= 0f) {
                    damageInfo.rejected = true;

                    EffectData effectData3 = new EffectData
                    {
                        origin = damageInfo.position,
                        rotation = Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : Random.onUnitSphere)
                    };
                    EffectManager.SpawnEffect(HealthComponent.AssetReferences.captainBodyArmorBlockEffectPrefab, effectData3, transmit: true);

                    self.body.SetBuffCount(Buff.buffIndex, persistenceCount - 1);
                }
            }

            orig(self, damageInfo);
        }

        private void AddPersistanceBuffs(CharacterBody sender, StatHookEventArgs args)
        {
            int persistenceCount = sender.GetBuffCount(Buff);

            if (persistenceCount > 0) {
                float atkSpeed = 0.1f * persistenceCount;
                float armor = persistenceCount * 20;
                float moveSpeed = 0.05f * persistenceCount;

                args.armorAdd += armor;
                args.attackSpeedMultAdd += atkSpeed;
                args.moveSpeedMultAdd += moveSpeed;
            }
        }
    }
}