using System;
using UnityEngine.Rendering;

namespace RaindropLobotomy.Survivors.Sweeper {
    public class SprayFuel : BaseSkillState {
        public Animator anim;
        public float duration;
        public float damageCoeff = 5f;
        public bool hasShot = false;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = 1f / base.attackSpeedStat;
            PlayAnimation("FullBody, Override", "Spray", "Generic.playbackRate", duration);
            anim = GetModelAnimator();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }

            // base.characterMotor.velocity = base.characterMotor.velocity.Nullify(true, false, true);

            StartAimMode(0.5f);

            if (anim.GetFloat("ejectSpray") >= 0.8f && !hasShot && base.isAuthority) {
                hasShot = true;

                Transform muzzle = FindModelChild("MuzzleBacktank");
                // EffectManager.SimpleEffect(Sweeper.AcidSprayEffect, muzzle.transform.position, Quaternion.LookRotation(muzzle.transform.up, muzzle.transform.forward), false);
                bool crit = base.RollCrit();
                for (int i = 0; i < 6; i++) {
                    FireProjectileInfo info = new();
                    info.damage = base.damageStat * 0.8f;
                    info.position = muzzle.position;
                    info.rotation = Quaternion.LookRotation(Util.ApplySpread(base.GetAimRay().direction, -6f, 6f, 1f, 1f));
                    info.projectilePrefab = Sweeper.AcidGlob;
                    info.crit = crit;
                    info.owner = base.gameObject;
                    info.damageTypeOverride = DamageTypeCombo.GenericUtility | DamageType.IgniteOnHit;
                    
                    ProjectileManager.instance.FireProjectile(info);
                }

                if (Sweeper.Config.DoRecoil) {
                    base.characterMotor.Motor.ForceUnground();
                    base.characterMotor.velocity = -base.inputBank.aimDirection * 26f;
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}