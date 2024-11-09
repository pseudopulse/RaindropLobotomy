using System;

namespace RaindropLobotomy.EGO.Mage {
    public class EternalLamp : BaseSkillState {
        public float duration = 0.8f;
        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Additive", "FireNovaBomb", "ChargeNovaBomb.playbackRate", 1f);

            FireProjectile();

            duration /= base.attackSpeedStat;

            StartAimMode(0.2f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }
        }

        public void FireProjectile()
        {
            LampTargetTracker tracker = GetComponent<LampTargetTracker>();

            FireProjectileInfo info = new();
            info.projectilePrefab = EGOLamp.LampSeekerBolt;
            info.position = base.inputBank.aimOrigin;
            info.rotation = Util.QuaternionSafeLookRotation(inputBank.aimDirection);
            info.damage = base.damageStat * 2.4f;
            info.owner = base.gameObject;
            if (tracker.target) {
                info.target = tracker.target.gameObject;
            }

            ProjectileManager.instance.FireProjectile(info);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}