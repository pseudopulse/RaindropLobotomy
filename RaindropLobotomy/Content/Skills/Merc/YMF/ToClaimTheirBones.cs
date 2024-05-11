using System;

namespace RaindropLobotomy.Skills.Merc {
    public class ToClaimTheirBones : CoolerBasicMeleeAttack
    {
        public override float BaseDuration => 0.6f;

        public override float DamageCoefficient => damageCoeff;

        public override string HitboxName => "Sword";

        public override GameObject HitEffectPrefab => Assets.GameObject.ImpactMercSwing;

        public override float ProcCoefficient => 2f;

        public override float HitPauseDuration => 0.5f;

        public override GameObject SwingEffectPrefab => Assets.GameObject.MercSwordFinisherSlash;

        public override string MuzzleString => "GroundLight1";

        private float hitBoxScale = 1f;
        private float damageCoeff;

        public override void OnEnter()
        {
            base.mecanimHitboxActiveParameter = "Sword.active";
            
            int resentment = base.characterBody.GetBuffCount(Buffs.Resentment.Instance.Buff);

            hitBoxScale = Util.Remap(resentment, 0, 100, 1.7f, 6f);
            damageCoeff = Util.Remap(resentment, 0, 100, 4f, 46f);

            base.OnEnter();

            base.hitBoxGroup.hitBoxes[0].transform.localScale *= hitBoxScale;
        }

        public override void OnExit()
        {
            base.OnExit();
            base.hitBoxGroup.hitBoxes[0].transform.localScale /= hitBoxScale;
            base.characterBody.SetBuffCount(Buffs.Resentment.Instance.Buff.buffIndex, 0);
            base.characterBody.skillLocator.special.UnsetSkillOverride(base.gameObject, YieldMyFlesh.ToClaimTheirBones, GenericSkill.SkillOverridePriority.Contextual);
        }

        public override void BeginMeleeAttackEffect()
        {
            base.BeginMeleeAttackEffect();

            foreach (ParticleSystem system in swingEffectInstance.GetComponentsInChildren<ParticleSystem>()) {
                if (!system.name.Contains("SwingTrail")) {
                    continue;
                }
                ParticleSystem.MainModule main = system.main;
                main.scalingMode = ParticleSystemScalingMode.Local;
                system.transform.localScale *= hitBoxScale;
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
            }
            else
            {
                PlayCrossfade("Gesture, Additive", animationStateName, "GroundLight.playbackRate", duration, 0.05f);
                PlayCrossfade("Gesture, Override", animationStateName, "GroundLight.playbackRate", duration, 0.05f);
            }

            AkSoundEngine.PostEvent(Events.Play_merc_m1_hard_swing, base.gameObject);
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.damageType |= DamageType.ApplyMercExpose;
            overlapAttack.damageType |= DamageType.BonusToLowHealth;
            overlapAttack.damageType |= DamageType.CrippleOnHit;
            overlapAttack.damageType |= DamageType.SlowOnHit;
            overlapAttack.damageType |= DamageType.Stun1s;
        }
    }
}