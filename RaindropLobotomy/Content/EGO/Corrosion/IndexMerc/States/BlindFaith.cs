using System;
using RaindropLobotomy.Buffs;

namespace RaindropLobotomy.EGO.Merc {
    public class BlindFaith : CoolerBasicMeleeAttack, SteppedSkillDef.IStepSetter
    {
        public bool paladinInstalled => base.characterBody.bodyIndex == IndexMerc.IndexPaladinBody;
        public override float BaseDuration => 0.6f;

        public override float DamageCoefficient => 2.6f;

        public override string HitboxName => paladinInstalled ? "Sword" : (step == 2 ? "SwordLarge" : "Sword");

        public override GameObject HitEffectPrefab => null;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.05f;

        public override GameObject SwingEffectPrefab => step == 2 ? Load<GameObject>("ErodedSlashBig.prefab") : Load<GameObject>("ErodedSlash.prefab");

        public override string MuzzleString => paladinInstalled ? ( step == 0 ? "SwingRight" : "SwingLeft") : (step == 0 ? "GroundLight1" : step == 1 ? "GroundLight2" : "GroundLight3");
        public override string MechanimHitboxParameter => paladinInstalled ? null : "Sword.active";
        private int step = 0;

        public void SetStep(int i)
        {
            step = i;
        }

        public override void PlayAnimation()
        {
            AkSoundEngine.PostEvent(Events.Play_acrid_R_infect, base.gameObject);
            if (paladinInstalled) {
                switch (step) {
                    case 0:
                        PlayCrossfade("Gesture, Override", "Slash1", "Slash.playbackRate", this.duration, 0.05f);
                        if (!this.animator.GetBool("isMoving") && this.animator.GetBool("isGrounded")) {
                            PlayCrossfade("FullBody, Override", "Slash1", "Slash.playbackRate", this.duration, 0.05f);
                        }
                        break;
                    case 1:
                        PlayCrossfade("Gesture, Override", "Slash2", "Slash.playbackRate", this.duration, 0.05f);
                        if (!this.animator.GetBool("isMoving") && this.animator.GetBool("isGrounded")) {
                            PlayCrossfade("FullBody, Override", "Slash2", "Slash.playbackRate", this.duration, 0.05f);
                        }
                        break;
                    case 2:
                        PlayCrossfade("Gesture, Override", "Slash1", "Slash.playbackRate", this.duration, 0.05f);
                        if (!this.animator.GetBool("isMoving") && this.animator.GetBool("isGrounded")) {
                            PlayCrossfade("FullBody, Override", "Slash1", "Slash.playbackRate", this.duration, 0.05f);
                        }
                        break;
                }
            }

            else {
                switch (step) {
                    case 0:
                        PlayAnimationMerc("GroundLight1");
                        break;
                    case 1:
                        PlayAnimationMerc("GroundLight2");
                        break;
                    case 2:
                        PlayAnimationMerc("GroundLight1");
                        break;
                }
            }
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
    
            if (step == 2) {
                overlapAttack.AddModdedDamageType(Erosion.InflictTwoErosion);
            }
            else {
                overlapAttack.AddModdedDamageType(Erosion.InflictErosion);
            }
        }

        public override void BeginMeleeAttackEffect()
        {
            base.BeginMeleeAttackEffect();

            if (paladinInstalled) {
                swingEffectInstance.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                // shift vfx by 90 degrees to compensate for different muzzle orientation
            }
        }

        private void PlayAnimationMerc(string animation) {
            string animationStateName = animation;
            bool @bool = animator.GetBool("isMoving");
            bool bool2 = animator.GetBool("isGrounded");
            if (!@bool && bool2)
            {
                PlayCrossfade("FullBody, Override", animationStateName, "GroundLight.playbackRate", duration, 0.05f);
                return;
            }
            PlayCrossfade("Gesture, Additive", animationStateName, "GroundLight.playbackRate", duration, 0.05f);
            PlayCrossfade("Gesture, Override", animationStateName, "GroundLight.playbackRate", duration, 0.05f);

            AkSoundEngine.PostEvent(Events.Play_merc_sword_swing, base.gameObject);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}