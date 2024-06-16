using System;

namespace RaindropLobotomy.EGO.Commando {
    public class GuidingHand : BaseSkillState {
        public float duration = 0.5f;
        public float damageCoefficient = 2.5f;
        public override void OnEnter()
        {
            base.OnEnter();

            duration /= base.attackSpeedStat;

            PlayAnimation("Gesture, Additive", "ThrowGrenade", "FireFMJ.playbackRate", duration);
			PlayAnimation("Gesture, Override", "ThrowGrenade", "FireFMJ.playbackRate", duration);

            FireProjectileInfo info = new();

            info.crit = base.RollCrit();
            info.damage = base.damageStat * damageCoefficient;
            info.position = base.GetAimRay().origin;
            info.owner = base.gameObject;
            
            AkSoundEngine.PostEvent(Events.Play_merc_R_slicingBlades_throw, base.gameObject);
            
            info.projectilePrefab = SolemnLament.GuidingHandBlack;
            info.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(base.GetAimRay().direction, 0f, 0f, 1f, 1f, -10, 0f));

            if (base.isAuthority) ProjectileManager.instance.FireProjectile(info);


            info.projectilePrefab = SolemnLament.GuidingHandWhite;
            info.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(base.GetAimRay().direction, 0f, 0f, 1f, 1f, 10, 0f));

            if (base.isAuthority) ProjectileManager.instance.FireProjectile(info);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}