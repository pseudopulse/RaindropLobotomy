using System;

namespace RaindropLobotomy.EGO.Commando {
    public class Lament: BaseSkillState, SteppedSkillDef.IStepSetter
    {
        private float DamageCoefficient = 2.5f;
        private float ShotsPerSecond = 1f;
        private GameObject ImpactWhite => Load<GameObject>("LamentImpactWhite.prefab");
        private GameObject ImpactBlack => Load<GameObject>("LamentImpactBlack.prefab");
        private float Duration => 1f / ShotsPerSecond;
        //
        private int pistol;
        private float duration;
        public void SetStep(int i)
        {
            pistol = i;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            if (!SolemnLament.config.ReplaceSoundEffects) {
                if (pistol % 2 == 0) {
                    AkSoundEngine.PostEvent("Play_butterflyShot_black", base.gameObject);
                }
                else {
                    AkSoundEngine.PostEvent("Play_butterflyShot_white", base.gameObject);
                }
            }
            else {
                AkSoundEngine.PostEvent(Events.Play_wCrit, base.gameObject);
            }

            if (pistol % 2 == 0)
            {
                PlayAnimation("Gesture Additive, Left", "FirePistol, Left");
                FireBullet("MuzzleLeft");
            }
            else
            {
                PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
                FireBullet("MuzzleRight");
            }

            duration = Duration / base.attackSpeedStat;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterBody.SetAimTimer(0.2f);

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public void FireBullet(string muzzle) {
            EffectManager.SimpleMuzzleFlash(muzzle == "MuzzleLeft" ? SolemnLament.LamentMuzzleFlashBlack : SolemnLament.LamentMuzzleFlashWhite, base.gameObject, muzzle, false);

            if (isAuthority) {
                BulletAttack bulletAttack = new();
                bulletAttack.owner = base.gameObject;
                bulletAttack.weapon = base.gameObject;
                bulletAttack.origin = base.inputBank.aimOrigin;
                bulletAttack.aimVector = base.inputBank.aimDirection;
                bulletAttack.minSpread = 0f;
                bulletAttack.maxSpread = 0f;
                bulletAttack.damage = base.damageStat * DamageCoefficient;
                bulletAttack.force = 40f;
                bulletAttack.tracerEffectPrefab = muzzle == "MuzzleLeft" ? SolemnLament.LamentTracerBlack : SolemnLament.LamentTracerWhite;
                bulletAttack.muzzleName = muzzle;
                bulletAttack.hitEffectPrefab = muzzle == "MuzzleLeft" ? ImpactBlack : ImpactWhite;
                bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
                bulletAttack.radius = 0.1f;
                bulletAttack.smartCollision = true;
                bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                bulletAttack.damageType = DamageTypeCombo.GenericPrimary;
                bulletAttack.AddModdedDamageType(SolemnLament.StackingLament);
                bulletAttack.Fire();
            }
        }
    }
}