using System;
using System.Linq;
using RoR2;

namespace RaindropLobotomy.EGO.Bandit {
    public class MagicBullet : BaseSkillState {
        public string PortalMuzzle = "PortalMuzzle";
        public string BulletMuzzle = "MuzzleShotgun";
        public GameObject PortalPrefab => EGOMagicBullet.PortalPrefab;
        public GameObject BulletPrefab => EGOMagicBullet.BulletPrefab;
        public float DamageCoefficient = 4.8f;
        //
        private GameObject portal1;
        private GameObject portal2;
        private MagicBulletPortal mbp1;
        private bool firedBullet = false;
        private bool shouldConsumeAmmo = false;

        public override void OnEnter()
        {
            base.OnEnter();

            Ray aimRay = base.GetAimRay();
            
            portal1 = GameObject.Instantiate(PortalPrefab, FindModelChild(PortalMuzzle));
            // portal1.GetComponent<BoxCollider>().size = new(7, 7, 1.5f);
            portal2 = GameObject.Instantiate(PortalPrefab, aimRay.GetPoint(5), Util.QuaternionSafeLookRotation(aimRay.direction));

            mbp1 = portal1.GetComponent<MagicBulletPortal>();
            MagicBulletPortal mbp2 = portal2.GetComponent<MagicBulletPortal>();
            mbp1.outputPortals.Add(mbp2);
            mbp2.isOutput = true;


            MagicBulletTargeter targeter = base.characterBody.GetComponent<MagicBulletTargeter>();

            HurtBox box = targeter.target?.GetComponent<HurtBox>() ?? null;

            if (box) {
                shouldConsumeAmmo = true;

                Vector3 position = box.transform.position + Vector3.up * 3f;

                for (int i = 0; i < 50; i++) {
                    position = (Random.onUnitSphere * 5) + box.transform.position;

                    if (!Physics.SphereCast(position, 2f, (box.transform.position - position).normalized, out RaycastHit _, Vector3.Distance(box.transform.position, position) - 0.5f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore)) {
                        break;
                    }
                }

                mbp2.transform.position = position;
                mbp2.aimTarget = box.transform;
            }

            if (EGOMagicBullet.config.UseVanillaSounds) {
                AkSoundEngine.PostEvent(Events.Play_mage_m2_shoot, base.gameObject);
            }
            else {
                AkSoundEngine.PostEvent("Play_fruitloop_portal", base.gameObject);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            StartAimMode(0.1f);

            if (base.fixedAge >= 0.5f && !firedBullet) {
                firedBullet = true;

                Vector3 position = portal1.transform.position;
                Quaternion rotation = portal1.transform.rotation;
                portal1.transform.parent = null;
                portal1.transform.position = position;
                portal1.transform.rotation = rotation;

                PlayAnimation("Gesture, Additive", "FireMainWeapon", "FireMainWeapon.playbackRate", 0.2f);


                Transform muzzle = FindModelChild(BulletMuzzle);
                float distance = Vector3.Distance(muzzle.position, portal1.transform.position);

                if (EGOMagicBullet.config.UseVanillaSounds) {
                    AkSoundEngine.PostEvent(Events.Play_bandit2_m1_rifle, base.gameObject);
                }
                else {
                    AkSoundEngine.PostEvent("Play_fruitloop_shot", base.gameObject);
                }

                BulletAttack attack = new();
                attack.damage = base.damageStat * DamageCoefficient;
                attack.aimVector = muzzle.transform.forward;
                attack.weapon = muzzle.gameObject;
                attack.owner = base.gameObject;
                attack.falloffModel = BulletAttack.FalloffModel.None;
                attack.isCrit = base.RollCrit();
                attack.stopperMask = LayerIndex.noCollision.mask;
                attack.origin = muzzle.transform.position;
                attack.tracerEffectPrefab = EGOMagicBullet.TracerPrefab;
                attack.procCoefficient = 1f;
                attack.radius = 0.5f;
                attack.smartCollision = true;
                attack.maxDistance = distance;

                if (shouldConsumeAmmo) {
                    EGOMagicBullet.GiveAmmo(characterBody);
                }
                
                if (isAuthority) {
                    attack.Fire();
                    mbp1.FireBullet(attack);
                }
            }

            if (base.fixedAge >= 1.2f) {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnExit()
        {
            base.OnExit();

            GameObject.Destroy(portal1);
            GameObject.Destroy(portal2);
        }
    }
}