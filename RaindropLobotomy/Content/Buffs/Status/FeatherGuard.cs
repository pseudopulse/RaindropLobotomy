using System;
using RoR2.Orbs;

namespace RaindropLobotomy.Buffs {
    public class FeatherGuard : BuffBase<FeatherGuard>
    {
        public override BuffDef Buff => Load<BuffDef>("bdFeatherGuard.asset");
        public static GameObject FeatherShieldEffect;

        public override void PostCreation()
        {
            FeatherShieldEffect = PrefabAPI.InstantiateClone(Paths.GameObject.BearVoidEffect, "FeatherGuardEffect");
            FeatherShieldEffect.GetComponentInChildren<MeshRenderer>().material = Paths.Material.matArtifactShellExplosionIndicator;

            On.RoR2.HealthComponent.TakeDamage += FeatherGuardBlock;
        }

        private void FeatherGuardBlock(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (self.body.HasBuff(BuffIndex) && damageInfo.attacker && damageInfo.procCoefficient > 0) {
                damageInfo.rejected = true;

                HurtBox[] boxes = MiscUtils.RetrieveNearbyTargets(self.body, 45f);

                foreach (HurtBox box in boxes) {
                    LightningOrb orb = new();
                    orb.lightningType = LightningOrb.LightningType.BeadDamage;
                    orb.bouncesRemaining = 0;
                    orb.canBounceOnSameTarget = false;
                    orb.targetsToFindPerBounce = 0;
                    orb.attacker = self.gameObject;
                    orb.damageValue = self.body.damage * 3f;
                    orb.origin = self.body.corePosition;
                    orb.AddModdedDamageType(Sin.SinReflectionType);
                    orb.isCrit = false;
                    orb.target = box;
                    
                    OrbManager.instance.AddOrb(orb);
                }

                EffectManager.SimpleEffect(HealthComponent.AssetReferences.damageRejectedPrefab, damageInfo.position, Quaternion.identity, true);
                AkSoundEngine.PostEvent(Events.Play_item_void_bear, self.gameObject);
            }

            orig(self, damageInfo);
        }
    }
}