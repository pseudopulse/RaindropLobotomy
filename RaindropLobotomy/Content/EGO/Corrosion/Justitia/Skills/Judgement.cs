using System;

namespace RaindropLobotomy.EGO.FalseSon {
    public class Judgement : CoolerBasicMeleeAttack, SteppedSkillDef.IStepSetter
    {
        public override float BaseDuration => 0.8f;

        public override float DamageCoefficient => 2.2f;

        public override string HitboxName => "Club";

        public override GameObject HitEffectPrefab => Paths.GameObject.OmniImpactVFXSlashMerc;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.1f;

        public override GameObject SwingEffectPrefab => Load<GameObject>("JustitiaSlash.prefab");

        public override string MuzzleString => _step % 2 == 0 ? "SwingRight" : "SwingLeft";
        public override Func<bool> AlternateActiveParameter => () => {
            return base.fixedAge >= base.duration * 0.3f;
        };
        private int _step;

        void SteppedSkillDef.IStepSetter.SetStep(int i)
        {
            this._step = i;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void PlayAnimation()
        {
            AkSoundEngine.PostEvent(_step % 2 == 0 ? "Play_justitia_slash2" : "Play_justitia_slash1", base.gameObject);
            string animationStateName = (_step % 2 == 0 ? "SwingClubRight" : "SwingClubLeft");
            float num = Mathf.Max(duration, 0.2f);
            PlayCrossfade("Gesture, Additive", animationStateName, "SwingClub.playbackRate", num, 0.1f);
            PlayCrossfade("Gesture, Override", animationStateName, "SwingClub.playbackRate", num, 0.1f);

            if (base.isAuthority) {
                FireProjectileInfo info = new();
                info.projectilePrefab = EGOJustitia.AffectedByAnyDebuff(base.characterBody) ? EGOJustitia.AirSlashSinProjectile : EGOJustitia.AirSlashProjectile;
                info.damage = overlapAttack.damage;
                info.crit = overlapAttack.isCrit;
                info.position = base.characterBody.corePosition;
                info.rotation = Util.QuaternionSafeLookRotation(inputBank.aimDirection);
                info.owner = base.gameObject;

                ProjectileManager.instance.FireProjectile(info);
            }
        }
    }
}