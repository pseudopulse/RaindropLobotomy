using System;
using System.Collections;
using System.Linq;
using R2API.Utils;

namespace RaindropLobotomy.EGO.Bandit {
    public class FloodingBullets : BaseState {
        public string PortalMuzzle = "PortalMuzzle";
        public string BulletMuzzle = "MuzzleShotgun";
        public GameObject PortalPrefab => EGOMagicBullet.PortalPrefab;
        public GameObject BulletPrefab => EGOMagicBullet.BulletPrefab;
        public float DamageCoefficient = 11f;
        //
        private Animator animator;
        private AimAnimator aimAnimator;
        private MagicBulletPortal portal;
        private List<GameObject> toDestroy = new();
        private GameObject pp;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            aimAnimator = animator.GetComponent<AimAnimator>();

            // PlayAnimation("Gesture, Additive", "FireMainWeapon", "FireMainWeapon.playbackRate", 2f);

            aimAnimator.enabled = false;
            animator.SetFloat("aimYawCycle", 0.5f);
            animator.SetFloat("aimPitchCycle", 0.5f);

            StartAimMode(0.1f);

            base.characterBody.StartCoroutine(ProcessBullets());
            base.characterBody.AddBuff(RoR2Content.Buffs.ElephantArmorBoost);
            // pp = GameObject.Instantiate(EGOMagicBullet.FloodingBulletsPP);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public IEnumerator ProcessBullets() {
            GameObject portalInst = GameObject.Instantiate(PortalPrefab, FindModelChild(PortalMuzzle));
            portal = portalInst.GetComponent<MagicBulletPortal>();
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < 3; i++) {
                BulletAttack attack = GetBullet();
                
                List<HurtBox> targets = GetTargets();

                if (targets.Count >= 3) {
                    targets = targets.GetRange(0, 3);
                }

                if (targets.Count == 0) continue;

                if (EGOMagicBullet.config.UseVanillaSounds) {
                    AkSoundEngine.PostEvent(Events.Play_mage_m2_shoot, base.gameObject);
                }
                else {
                    AkSoundEngine.PostEvent("Play_fruitloop_portal", base.gameObject);
                }

                foreach (HurtBox box in targets) {
                    Vector3 position = box.transform.position + Vector3.up * 3f;

                    for (int k = 0; k < 50; k++) {
                        position = (Random.onUnitSphere * 5) + box.transform.position;

                        if (!Physics.SphereCast(position, 2f, (box.transform.position - position).normalized, out RaycastHit _, Vector3.Distance(box.transform.position, position) - 0.5f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore)) {
                            break;
                        }
                    }

                    Vector3 vector = (box.transform.position - position).normalized;
                    Vector3 position2 = new Ray(position, vector).GetPoint(10);

                    if (Physics.Raycast(position, vector, out RaycastHit info, 10f, LayerIndex.world.mask)) {
                        position2 = info.point + (-vector * 2f);
                    }

                    GameObject inst1 = GameObject.Instantiate(PortalPrefab, position, Quaternion.LookRotation(vector));
                    inst1.transform.localScale *= 2f;
                    inst1.GetComponent<MagicBulletPortal>().aimTarget = box.transform;

                    portal.outputPortals.Add(inst1.GetComponent<MagicBulletPortal>());

                    toDestroy.Add(inst1);
                }

                yield return new WaitForSeconds(1f);

                if (isAuthority) {
                    attack.Fire();
                    portal.FireBullet(attack);
                }

                if (EGOMagicBullet.config.UseVanillaSounds) {
                    AkSoundEngine.PostEvent(Events.Play_bandit2_m1_rifle, base.gameObject);
                }
                else {
                    AkSoundEngine.PostEvent("Play_fruitloop_shot", base.gameObject);
                }

                // PlayAnimation("Gesture, Additive", "FireMainWeapon", "FireMainWeapon.playbackRate", 0.2f);

                bool lastAmmo = EGOMagicBullet.GiveAmmo(characterBody);

                yield return new WaitForSeconds(0.5f);

                for (int j = 0; j < toDestroy.Count; j++) {
                    GameObject.Destroy(toDestroy[j]);
                }

                toDestroy.Clear();
                portal.outputPortals.Clear();

                bool allowed = IsAllowedToContinue();

                if (lastAmmo) {
                    break;
                }

                if (!allowed) {
                    break;
                }

                yield return new WaitForSeconds(0.6f);
            }

            outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnExit()
        {
            base.OnExit();
            aimAnimator.enabled = true;
            GameObject.Destroy(portal.gameObject);
            GameObject.Destroy(pp);

            for (int i = 0; i < toDestroy.Count; i++) {
                GameObject.Destroy(toDestroy[i]);
            }
            base.characterBody.RemoveBuff(RoR2Content.Buffs.ElephantArmorBoost);
        }

        public bool IsAllowedToContinue() {
            base.skillLocator.special.DeductStock(1);

            return base.skillLocator.special.stock > 0;
        }

        public BulletAttack GetBullet() {
            Transform muzzle = FindModelChild(BulletMuzzle);
            float distance = Vector3.Distance(muzzle.position, portal.transform.position);

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
            attack.radius = 1f;
            attack.smartCollision = true;
            attack.maxDistance = distance;

            return attack;
        }

        public List<HurtBox> GetTargets() {
            SphereSearch search = new();
            search.origin = base.transform.position;
            search.mask = LayerIndex.entityPrecise.mask;
            search.radius = 60f;
            search.RefreshCandidates();
            search.FilterCandidatesByDistinctHurtBoxEntities();
            search.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(GetTeam()));
            
            return search.GetHurtBoxes().Where(x => 
                true
            ).ToList();
        }
    }
}