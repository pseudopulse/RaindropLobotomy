using System;

namespace RaindropLobotomy.Skills.Merc {
    public class YieldMyFleshState : CoolerBasicMeleeAttack {
        private float damageTaken = 0f;

        public override float BaseDuration => 1.6f;

        public override float DamageCoefficient => 4f;

        public override string HitboxName => "Sword";

        public override GameObject HitEffectPrefab => Paths.GameObject.ImpactMercSwing;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.05f;

        public override GameObject SwingEffectPrefab => Paths.GameObject.MercSwordSlash;

        public override string MuzzleString => "GroundLight1";
        private Transform attacker;

        public override void OnEnter()
        {
            base.mecanimHitboxActiveParameter = "Sword.active";

            base.OnEnter();

            On.RoR2.HealthComponent.TakeDamageProcess += ReceiveDamage;
            characterBody.AddBuff(Buffs.Unrelenting.Instance.Buff);
        }

        private void ReceiveDamage(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            if (self == characterBody.healthComponent && !damageInfo.damageType.damageType.HasFlag(DamageType.FallDamage) && damageInfo.attacker) {
                damageInfo.damage *= 2f;
                damageTaken = damageInfo.damage;
                attacker = damageInfo.attacker.transform;
                Process();
            }

            orig(self, damageInfo);
        }

        public override void OnExit()
        {
            base.OnExit();
            On.RoR2.HealthComponent.TakeDamageProcess -= ReceiveDamage;
            characterBody.RemoveBuff(Buffs.Unrelenting.Instance.Buff);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public void Process() {
            if (damageTaken > 0f && attacker) {
                float percentageTaken = damageTaken / healthComponent.fullCombinedHealth;

                characterBody.SetBuffCount(Buffs.Resentment.Instance.Buff.buffIndex, (int)(percentageTaken * 100f));


                AkSoundEngine.PostEvent(Events.Play_merc_shift_slice, base.gameObject);
                AkSoundEngine.PostEvent(Events.Play_merc_shift_slice, base.gameObject);
                AkSoundEngine.PostEvent(Events.Play_merc_shift_slice, base.gameObject);

                EffectManager.SpawnEffect(Paths.GameObject.MercExposeConsumeEffect, new EffectData {
                    origin = base.transform.position,
                    scale = 3f
                }, true);

                outer.SetNextState(new ToClaimTheirBones(attacker));
            }
        }

        public override void PlayAnimation()
        {
            string animationStateName = "GroundLight1";
            bool @bool = animator.GetBool("isMoving");
            bool bool2 = animator.GetBool("isGrounded");
            if (!@bool && bool2)
            {
                PlayCrossfade("FullBody, Override", animationStateName, "GroundLight.playbackRate", duration, 0.05f);
                return;
            }
            PlayCrossfade("Gesture, Additive", animationStateName, "GroundLight.playbackRate", duration, 0.05f);
            PlayCrossfade("Gesture, Override", animationStateName, "GroundLight.playbackRate", duration, 0.05f);
        }
    }
}