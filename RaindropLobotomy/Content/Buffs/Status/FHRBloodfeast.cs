using System;
using RaindropLobotomy.Enemies.FHR;
using RoR2.Orbs;

namespace RaindropLobotomy.Buffs {
    public class FHRBloodfeast : BuffBase<FHRBloodfeast>
    {
        public override BuffDef Buff => Load<BuffDef>("bdFHRBloodfeast.asset");
        public static GameObject OrbEffect;

        public override void PostCreation()
        {
            OrbEffect = Paths.GameObject.InfusionOrbEffect;

            On.RoR2.HealthComponent.TakeDamage += HandleBleedTaken;
            On.RoR2.CharacterBody.AddTimedBuff_BuffIndex_float += RejectOnFHR;
        }

        // dont allow FHR to inflict bloodfeast on itself
        private void RejectOnFHR(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffIndex_float orig, CharacterBody self, BuffIndex buffIndex, float duration)
        {
            if (buffIndex == Buff.buffIndex && self.bodyIndex == FHR.FHRIndex) {
                return;
            }

            orig(self, buffIndex, duration);
        }

        private void HandleBleedTaken(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);

            bool isBleed = damageInfo.damageType.damageType.HasFlag(DamageType.DoT) && damageInfo.damageColorIndex == DamageColorIndex.Bleed;

            if (isBleed && self.body.HasBuff(Buff)) {
                CharacterBody target = FHR.GetNearbyRecipient(self.body.corePosition);

                if (target) {
                    BloodfeastOrb orb = new();
                    orb.target = target.mainHurtBox;
                    orb.origin = self.body.corePosition;

                    OrbManager.instance.AddOrb(orb);
                }
            }
        }

        public class BloodfeastOrb : Orb {
            public CharacterBody body;

            public override void Begin()
            {
                duration = distanceToTarget / 50f;

                EffectData data = new() {
                    origin = origin,
                    genericFloat = duration
                };

                data.SetHurtBoxReference(target);

                EffectManager.SpawnEffect(OrbEffect, data, true);

                HurtBox box = target.GetComponent<HurtBox>();

                if (box && box.healthComponent) {
                    body = box.healthComponent.body;
                }
            }

            public override void OnArrival()
            {
                if (body) {
                    body.healthComponent.HealFraction(0.005f, default);
                    body.AddTimedBuff(BloodBoost.BuffIndex, 8.5f);
                }
            }
        }
    }
}