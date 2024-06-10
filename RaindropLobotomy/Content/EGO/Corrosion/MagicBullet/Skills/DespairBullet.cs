using System;
using System.Collections;

namespace RaindropLobotomy.EGO.Bandit {
    public class DespairBullet : BaseSkillState {
        public string PortalMuzzle = "PortalMuzzle";
        public string BulletMuzzle = "MuzzleShotgun";
        public GameObject PortalPrefab => EGOMagicBullet.PortalPrefab;
        public GameObject BulletPrefab => EGOMagicBullet.BulletPrefab;
        public float DamageCoefficient = 0.1f;
        //
        private Animator animator;
        private AimAnimator aimAnimator;
        private MagicBulletPortal portal;
        private MagicBulletPortal output;

        public override void OnEnter()
        {
            base.OnEnter();

            animator = GetModelAnimator();
            aimAnimator = animator.GetComponent<AimAnimator>();

            // PlayAnimation("Gesture, Additive", "FireMainWeapon", "FireMainWeapon.playbackRate", 2f);

            StartAimMode(0.1f);

            base.characterBody.StartCoroutine(ProcessBullet());
        }

        public IEnumerator ProcessBullet() {
            GameObject portalInst = GameObject.Instantiate(PortalPrefab, FindModelChild(PortalMuzzle));
            portal = portalInst.GetComponent<MagicBulletPortal>();
            AkSoundEngine.PostEvent("Play_fruitloop_portal", base.gameObject);

            GameObject portalInst2 = GameObject.Instantiate(PortalPrefab, FindModelChild("DespairPortal"));
            output = portalInst2.GetComponent<MagicBulletPortal>();

            portal.outputPortals.Add(output);
            output.isOutput = true;
            
            yield return new WaitForSeconds(0.8f);

            BulletAttack attack = GetBullet();

            if (base.isAuthority) {
                attack.Fire();
                portal.FireBullet(attack);
            }

            AkSoundEngine.PostEvent("Play_fruitloop_shot", base.gameObject);


            yield return new WaitForSeconds(0.5f);

            outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();
            GameObject.Destroy(portal.gameObject);
            GameObject.Destroy(output.gameObject);
            EGOMagicBullet.SpendAmmo(characterBody);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            StartAimMode(0.1f);
        }

        public BulletAttack GetBullet() {
            Transform muzzle = FindModelChild(BulletMuzzle);
            float distance = Vector3.Distance(muzzle.position, portal.transform.position);

            BulletAttack attack = new();
            attack.damage = base.healthComponent.fullCombinedHealth * DamageCoefficient;
            attack.aimVector = muzzle.transform.forward;
            attack.weapon = muzzle.gameObject;
            attack.owner = null;
            attack.falloffModel = BulletAttack.FalloffModel.None;
            attack.isCrit = base.RollCrit();
            attack.stopperMask = LayerIndex.noCollision.mask;
            attack.origin = muzzle.transform.position;
            attack.tracerEffectPrefab = EGOMagicBullet.TracerPrefab;
            attack.procCoefficient = 1f;
            attack.radius = 2f;
            attack.smartCollision = true;
            attack.maxDistance = distance;
            attack.damageType |= DamageType.NonLethal;
            attack.AddModdedDamageType(EGOMagicBullet.DespairDamage);

            return attack;
        }
    }
}