using System;
using R2API.Networking.Interfaces;
using RaindropLobotomy.Buffs;

namespace RaindropLobotomy.EGO.Merc {
    public class YieldMyFlesh : CoolerBasicMeleeAttack {
        private float damageTaken = 0f;

        public override float BaseDuration => 1.6f;

        public override float DamageCoefficient => 4f;

        public override string HitboxName => "Sword";

        public override GameObject HitEffectPrefab => Assets.GameObject.ImpactMercSwing;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.05f;

        public override GameObject SwingEffectPrefab => BLMerc.SlashEffect;

        public override string MuzzleString => "GroundLight1";
        private Transform attacker;

        public override void OnEnter()
        {
            base.mecanimHitboxActiveParameter = "Sword.active";

            base.OnEnter();

            if (NetworkServer.active) {
                On.RoR2.HealthComponent.TakeDamage += ReceiveDamage;
                characterBody.AddBuff(Buffs.Unrelenting.Instance.Buff);
            }

            // damageTaken = 100f;
            // attacker = base.gameObject.transform;
            // Process();
        }

        private void ReceiveDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            if (self == characterBody.healthComponent && !damageInfo.damageType.HasFlag(DamageType.FallDamage) && damageInfo.attacker) {
                damageTaken = damageInfo.damage;
                attacker = damageInfo.attacker.transform;

                if (base.isAuthority) {
                    ReceiveInfo(attacker.gameObject, damageTaken);
                }
                else {
                    new SyncTCTB(base.gameObject, attacker.gameObject, damageTaken).Send(R2API.Networking.NetworkDestination.Clients);
                }
            }

            orig(self, damageInfo);
        }

        public void ReceiveInfo(GameObject attacker, float damageDealt) {
            damageTaken = damageDealt;
            this.attacker = attacker.transform;
            Process();
        }

        public override void OnExit()
        {
            base.OnExit();
            if (NetworkServer.active) {
                On.RoR2.HealthComponent.TakeDamage -= ReceiveDamage;
            }
            characterBody.RemoveBuff(Buffs.Unrelenting.Instance.Buff);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public void Process() {
            if (damageTaken > 0f && attacker) {
                AkSoundEngine.PostEvent(Events.Play_merc_shift_slice, base.gameObject);
                AkSoundEngine.PostEvent(Events.Play_merc_shift_slice, base.gameObject);
                AkSoundEngine.PostEvent(Events.Play_merc_shift_slice, base.gameObject);

                EffectManager.SpawnEffect(Assets.GameObject.MercExposeConsumeEffect, new EffectData {
                    origin = base.transform.position,
                    scale = 3f
                }, false);

                // base.characterBody.skillLocator.special.SetSkillOverride(base.gameObject, BLMerc.ToClaimTheirBones, GenericSkill.SkillOverridePriority.Contextual);

                int count = base.characterBody.GetBuffCount(Poise.Instance.Buff);
                count = Mathf.Clamp(count, 0, 20);
                float bonusDamage = 1f + (count * 0.1f);

                base.characterBody.SetBuffCount(Poise.Instance.Buff.buffIndex, 0);

                base.characterBody.SetBuffCount(Unrelenting.Instance.Buff.buffIndex, 0);
                base.characterBody.SetBuffCount(RoR2Content.Buffs.HiddenInvincibility.buffIndex, 1);

                if (base.isAuthority) {
                    outer.SetNextState(new ToClaimTheirBones_Transition(0f, new ToClaimTheirBones_1(bonusDamage, attacker)));
                }
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