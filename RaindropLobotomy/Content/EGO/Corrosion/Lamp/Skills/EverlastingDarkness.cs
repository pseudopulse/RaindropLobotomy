using System;

namespace RaindropLobotomy.EGO.Mage {
    public class EverlastingDarkness : BaseSkillState {
        private bool playedAnim = false;
        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Additive", "ChargeNovaBomb", "ChargeNovaBomb.playbackRate", 2f);

            FireProjectileInfo info = new();
            info.projectilePrefab = EGOLamp.LampAreaPassive;
            info.position = base.characterBody.corePosition;
            info.rotation = Quaternion.identity;
            info.damage = base.damageStat * 2.4f;
            info.owner = base.gameObject;

            ProjectileManager.instance.FireProjectile(info);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= 5f) {
                outer.SetNextStateToMain();
            }

            if (base.fixedAge >= 2f && !playedAnim) {
                playedAnim = true;
                PlayAnimation("Gesture, Additive", "FireNovaBomb", "ChargeNovaBomb.playbackRate", 1f);
            }
        }
    }
}