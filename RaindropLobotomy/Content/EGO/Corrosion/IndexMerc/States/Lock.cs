using System;

namespace RaindropLobotomy.EGO.Merc {
    public class Lock : BaseSkillState {
        public bool paladinInstalled => base.characterBody.bodyIndex == IndexMerc.IndexPaladinBody;
        public float damageCoefficientBase = 2.5f;
        public override void OnEnter()
        {
            base.OnEnter();
            AkSoundEngine.PostEvent(Events.Play_lunar_reroller_activate, base.gameObject);

            FireProjectileInfo info = new();
            info.projectilePrefab = IndexMerc.LockingBolt;
            info.damage = base.damageStat;
            info.crit = base.RollCrit();
            info.rotation = Util.QuaternionSafeLookRotation(base.inputBank.aimDirection);
            info.position = base.inputBank.aimOrigin;
            info.owner = base.gameObject;

            if (base.isAuthority) {
                ProjectileManager.instance.FireProjectile(info);
            }

            outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}