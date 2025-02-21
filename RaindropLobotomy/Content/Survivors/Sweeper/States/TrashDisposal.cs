using System;

namespace RaindropLobotomy.Survivors.Sweeper {
    public class TrashDisposal : CoolerBasicMeleeAttack
    {
        public override float BaseDuration => 1f;

        public override float DamageCoefficient => 4.4f;

        public override string HitboxName => "Sweep";

        public override GameObject HitEffectPrefab => Paths.GameObject.SpurtGenericBlood;

        public override float ProcCoefficient => 1;

        public override float HitPauseDuration => 0.05f;

        public override GameObject SwingEffectPrefab => null;

        public override string MuzzleString => "MuzzleSweep";
        public override string MechanimHitboxParameter => "slamBegun";
        public bool hasSlammed = false;
        private bool canExit = true;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Override", "Slam", "Generic.playbackRate", duration);

            // AkSoundEngine.PostEvent(Events.Play_loader_R_variant_whooshDown, base.gameObject);
            
            characterMotor.velocity = characterMotor.velocity.Nullify(true, false, true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            // Debug.Log(animator.GetFloat("slamContact"));

            if (!base.isGrounded && animator.GetFloat("slamBegun") > 0.5f) {
                animator.SetFloat("Slam.playbackRate", 0f);
                canExit = false;
                base.fixedAge -= Time.fixedDeltaTime;
            }
            else {
                animator.SetFloat("Slam.playbackRate", 1f);
                canExit = true;
            }

            if (animator.GetFloat("slamContact") > 0.5f && !hasSlammed) {
                hasSlammed = true;

                Vector3 pos = modelLocator.modelTransform.position + (characterDirection.forward * 2.23f);

                EffectManager.SimpleEffect(Sweeper.AcidSprayEffect, pos, Quaternion.identity, false);
                AkSoundEngine.PostEvent(Events.Play_acid_larva_attack1_explo, base.gameObject);

                BulletAttack attack = new();
                attack.owner = base.gameObject;
                attack.maxDistance = 24f;
                attack.bulletCount = 1;
                attack.radius = 4f;
                attack.smartCollision = true;
                attack.damage = 1.4f * damageStat;
                attack.falloffModel = BulletAttack.FalloffModel.None;
                attack.damageType = DamageTypeCombo.GenericSecondary | DamageType.IgniteOnHit;
                attack.origin = base.transform.position;
                attack.aimVector = Vector3.Reflect((pos - characterBody.corePosition).normalized, Vector3.up);
                EffectManager.SimpleEffect(Sweeper.AcidSprayEffect, pos, Quaternion.LookRotation(characterDirection.forward, attack.aimVector), false);
                attack.isCrit = base.RollCrit();
                if (base.isAuthority) {
                    attack.Fire();
                    attack.aimVector = base.characterDirection.forward;
                    attack.Fire();
                }
            }

            base.duration = canExit ? base.baseDuration / attackSpeedStat : 9999999f;
        }
    }
}