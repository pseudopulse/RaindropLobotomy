using System;

namespace RaindropLobotomy.EGO.Merc {
    public class Overthrow : CoolerBasicMeleeAttack
    {
        public override float BaseDuration => 0.4f;

        public override float DamageCoefficient => 3f;

        public override string HitboxName => "Uppercut";

        public override GameObject HitEffectPrefab => Paths.GameObject.ImpactMercSwing;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.05f;

        public override GameObject SwingEffectPrefab => BLMerc.Upslash;

        public override string MuzzleString => "WhirlwindAir";
        public override string MechanimHitboxParameter => "Sword.active";

        public override void OnEnter()
        {
            base.OnEnter();
            base.characterMotor.Motor.ForceUnground();
            base.characterMotor.velocity = Vector3.up * 36f + (base.characterDirection.forward * 16f);

            BLMerc.UpdateSwordplayState(base.characterBody, SwordplayState.Uppercut);
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.forceVector = Vector3.up * 3000f;
        }

        public override void PlayAnimation()
        {
            PlayCrossfade("FullBody, Override", "Uppercut", "Uppercut.playbackRate", duration, 0.1f);
            AkSoundEngine.PostEvent(Events.Play_merc_m2_uppercut, base.gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();
            PlayAnimation("FullBody, Override", "UppercutExit");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterBody.isSprinting = true;
        }

        public override void AuthorityOnFinish()
        {
            outer.SetNextState(new OverthrowSlam());
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }

    public class OverthrowSlam : CoolerBasicMeleeAttack
    {
        public override float BaseDuration => 1f;

        public override float DamageCoefficient => 8f;

        public override string HitboxName => "WhirlwindAir";

        public override GameObject HitEffectPrefab => Paths.GameObject.ImpactMercSwing;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.01f;

        public override GameObject SwingEffectPrefab => BLMerc.Radial;

        public override string MuzzleString => "WhirlwindAir";
        private bool hasExited = false;
        private float stopwatch = 0f;

        public override void OnEnter()
        {
            base.OnEnter();

            BLMerc.UpdateSwordplayState(base.characterBody, SwordplayState.Slam);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.fixedDeltaTime;

            if (!base.isGrounded) {
                base.characterMotor.velocity = Vector3.down * 36f + (base.characterDirection.forward * 36f);
                base.fixedAge = 0f;
            }

            if ((base.isGrounded || stopwatch >= 10f) && !hasExited) {
                hasExited = true;
                outer.SetNextStateToMain();
            }
        }

        public override void PlayAnimation()
        {
            base.PlayAnimation();
            PlayCrossfade("FullBody, Override", "WhirlwindAir", "Whirlwind.playbackRate", duration, 0.1f);
            AkSoundEngine.PostEvent(Events.Play_merc_sword_swing, base.gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();
            PlayAnimation("FullBody, Override", "WhirlwindAirExit");

            if (base.isAuthority) {
                BlastAttack attack = new();
                attack.attacker = base.gameObject;
                attack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                attack.baseDamage = base.damageStat * damageCoefficient;
                attack.crit = base.RollCrit();
                attack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                attack.radius = 7f;
                attack.position = base.transform.position;
                attack.teamIndex = base.GetTeam();
                attack.procCoefficient = 1f;
                attack.AddModdedDamageType(Buffs.Poise.Instance.GivePoise);

                EffectManager.SpawnEffect(Paths.GameObject.HermitCrabBombExplosion, new EffectData {
                    origin = attack.position,
                    scale = attack.radius
                }, true);

                attack.Fire();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}