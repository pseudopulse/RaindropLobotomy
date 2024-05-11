using System;

namespace RaindropLobotomy.EGO.Viend {
    public class Hello : BaseSkillState {
        public static float DamageCoefficient = 2.75f;
        public static float BaseDuration = 0.6f;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = BaseDuration / attackSpeedStat;

            PlayAnimation("LeftArm, Override", "FireHandBeam", "HandBeam.playbackRate", duration);

            Ray aimRay = GetAimRay();

            AddRecoil(-1f, -2f, -0.5f, 0.5f);
            StartAimMode(aimRay);
            AkSoundEngine.PostEvent(Events.Play_mage_m2_iceSpear_impact, base.gameObject);

            EffectManager.SimpleMuzzleFlash(Assets.GameObject.Muzzleflash1, base.gameObject, "MuzzleHandBeam", transmit: false);
            
            if (base.isAuthority)
            {
                BulletAttack bulletAttack = new();
                bulletAttack.owner = base.gameObject;
                bulletAttack.weapon = base.gameObject;
                bulletAttack.origin = aimRay.origin;
                bulletAttack.aimVector = aimRay.direction;
                bulletAttack.muzzleName = "MuzzleHandBeam";
                bulletAttack.maxDistance = 40f;
                bulletAttack.minSpread = 0f;
                bulletAttack.maxSpread = base.characterBody.spreadBloomAngle;
                bulletAttack.radius = 0.5f;
                bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                bulletAttack.smartCollision = true;
                bulletAttack.damage = DamageCoefficient * base.damageStat;
                bulletAttack.procCoefficient = 1f;
                bulletAttack.force = 800f;
                bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
                bulletAttack.tracerEffectPrefab = EGOMimicry.TracerHello;
                bulletAttack.hitEffectPrefab = Assets.GameObject.OmniImpactVFXHuntress;
                Debug.Log("firing bullet");
                bulletAttack.Fire();
            }
            
            base.characterBody.AddSpreadBloom(0.2f);
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
            return InterruptPriority.PrioritySkill;
        }
    }
}