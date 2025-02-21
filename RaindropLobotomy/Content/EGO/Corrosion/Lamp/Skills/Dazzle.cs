using System;
using PaladinMod.Modules;

namespace RaindropLobotomy.EGO.Mage {
    public class Dazzle : BaseSkillState {
        public float duration = 0.8f;
        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Additive", "FireNovaBomb", "ChargeNovaBomb.playbackRate", 1f);

            DazzleTarget();
            StartAimMode(0.2f);

            AkSoundEngine.PostEvent(Events.Play_chef_skill2_boosted_fireball_explode, base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            StartAimMode(0.2f);

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }
        }

        public void DazzleTarget()
        {
            LampTargetTracker tracker = GetComponent<LampTargetTracker>();

            CharacterBody body = tracker.target.GetComponent<HurtBox>().healthComponent.body;

            DamageInfo damage = new();
            damage.damage = body.HasBuff(Buffs.Enchanted.BuffIndex) ? base.damageStat * 40f : base.damageStat * 4f;
            damage.damageType = DamageType.Stun1s;
            damage.crit = base.RollCrit();
            damage.damageColorIndex = body.HasBuff(Buffs.Enchanted.BuffIndex) ? DamageColorIndex.WeakPoint : DamageColorIndex.Default;
            damage.attacker = base.gameObject;
            damage.position = body.corePosition;
            damage.procCoefficient = 1f;
            damage.damageType = DamageTypeCombo.GenericSecondary;
            damage.AddModdedDamageType(EGOLamp.LightDamage);

            GlobalEventManager.instance.OnHitAll(damage, body.gameObject);
            GlobalEventManager.instance.OnHitEnemy(damage, body.gameObject);
            body.healthComponent.TakeDamage(damage);

            if (body.HasBuff(Buffs.Enchanted.BuffIndex)) {
                EffectManager.SpawnEffect(Paths.GameObject.ChildTrackingSparkBallExplosion, new EffectData {
                    scale = body.bestFitRadius * 2f * 3f,
                    origin = damage.position
                }, true);

                body.SetBuffCount(Buffs.Enchanted.BuffIndex, 0);
            }
            else {
                EffectManager.SpawnEffect(Paths.GameObject.BoostedSearFireballProjectileExplosionVFX, new EffectData {
                    scale = body.bestFitRadius * 1.5f,
                    origin = damage.position
                }, true);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}