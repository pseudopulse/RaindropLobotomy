/* using System;

namespace RaindropLobotomy.EGO.Mage {
    public class SeveringWave : BaseSkillState, SteppedSkillDef.IStepSetter {
        public float duration = 0.8f;
        private int _step;
        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Additive", "FireNovaBomb", "ChargeNovaBomb.playbackRate", duration * 2f);

            FireProjectile();

            duration /= base.attackSpeedStat;

            StartAimMode(0.2f);

            AkSoundEngine.PostEvent(Events.Play_mage_m1_shoot, base.gameObject);
            AkSoundEngine.PostEvent(Events.Play_engi_M2_throw, base.gameObject);
            AkSoundEngine.PostEvent(Events.Play_engi_M2_throw, base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            StartAimMode(0.2f);

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }
        }

        public void FireProjectile()
        {
            Vector3 rot = base.inputBank.aimDirection;
            rot = Quaternion.AngleAxis(_step % 2 == 0 ? -95f : 95f, -rot) * rot;

            FireProjectileInfo info = new();
            info.projectilePrefab = Arbitificer.FairyWave;
            info.position = base.inputBank.aimOrigin;
            info.rotation = Util.QuaternionSafeLookRotation(rot);
            info.damage = base.damageStat * 2.2f;
            info.owner = base.gameObject;

            ProjectileManager.instance.FireProjectile(info);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public void SetStep(int i)
        {
            _step = i;
        }
    }
}*/