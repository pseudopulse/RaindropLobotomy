using System;
using System.Collections;
using System.Linq;
using EntityStates.ArtifactShell;
using EntityStates.Interactables.MSObelisk;

namespace RaindropLobotomy.EGO.Commando {
    public class EternalRest : BaseSkillState {
        public float totalDuration = 3f;
        public int totalShots = 16;
        public float damageCoeff = 2f;
        public List<HurtBox> hurtBoxes = new();
        private GameObject ImpactWhite => Load<GameObject>("LamentImpactWhite.prefab");
        private GameObject ImpactBlack => Load<GameObject>("LamentImpactBlack.prefab");
        private bool firedAtLeastOnce = false;
        private float shotDelay;
        private Transform rotateObject;

        public override void OnEnter()
        {
            base.OnEnter();


            BullseyeSearch search = new();
            search.searchOrigin = base.GetAimRay().origin;
            search.searchDirection = base.GetAimRay().direction;
            search.filterByLoS = true;
            search.filterByDistinctEntity = true;
            search.maxDistanceFilter = 120f;
            search.maxAngleFilter = 180f;
            search.teamMaskFilter = TeamMask.GetUnprotectedTeams(base.GetTeam());
            search.RefreshCandidates();
            hurtBoxes = search.GetResults().Where(x => x.healthComponent && x.healthComponent.body.HasBuff(Buffs.Sealed.Instance.Buff)).ToList();

            totalShots = (int)(totalShots * base.attackSpeedStat);

            shotDelay = totalDuration / totalShots;

            if (!UpdateHurtboxes()) {
                outer.SetNextStateToMain();
            }

            base.characterBody.StartCoroutine(FireBullets());

            rotateObject = base.transform.Find("RotateObject");
        }

        public IEnumerator FireBullets() {
            for (int i = 0; i < 8; i++) {
                for (int j = 1; j <= 2; j++) {
                    yield return new WaitForSeconds(shotDelay);

                    if (!SolemnLament.config.ReplaceSoundEffects) {
                        if (j % 2 == 0) {
                            AkSoundEngine.PostEvent("Play_butterflyShot_black", base.gameObject);
                        }
                        else {
                            AkSoundEngine.PostEvent("Play_butterflyShot_white", base.gameObject);
                        }
                    }
                    else {
                        AkSoundEngine.PostEvent(Events.Play_wCrit, base.gameObject);
                    }


                    if (!UpdateHurtboxes()) {
                        goto end;
                    }

                    foreach (HurtBox box in hurtBoxes) {
                        FireBullet(j, box);
                    }
                }
            }

            end:
            outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();

            if (firedAtLeastOnce) {
                base.skillLocator.special.DeductStock(1);
            }
        }

        public void FireBullet(int pistol, HurtBox target) {
            if (pistol % 2 == 0)
            {
                PlayAnimation("Gesture Additive, Left", "FirePistol, Left");
                FireBullet("MuzzleLeft", target.transform.position);
            }
            else
            {
                PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
                FireBullet("MuzzleRight", target.transform.position);
            }
        }

        public void FireBullet(string muzzle, Vector3 target) {
            EffectManager.SimpleMuzzleFlash(muzzle == "MuzzleLeft" ? SolemnLament.LamentMuzzleFlashBlack : SolemnLament.LamentMuzzleFlashWhite, base.gameObject, muzzle, false);

            if (isAuthority) {
                BulletAttack bulletAttack = new();
                bulletAttack.owner = base.gameObject;
                bulletAttack.weapon = base.gameObject;
                bulletAttack.origin = base.inputBank.aimOrigin;
                bulletAttack.aimVector = (target - base.inputBank.aimOrigin).normalized;
                bulletAttack.minSpread = 0f;
                bulletAttack.maxSpread = 0f;
                bulletAttack.damage = base.damageStat * damageCoeff;
                bulletAttack.force = 40f;
                bulletAttack.tracerEffectPrefab = muzzle == "MuzzleLeft" ? SolemnLament.LamentTracerBlack : SolemnLament.LamentTracerWhite;
                bulletAttack.muzzleName = muzzle;
                bulletAttack.hitEffectPrefab = muzzle == "MuzzleLeft" ? ImpactBlack : ImpactWhite;
                bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
                bulletAttack.radius = 0.1f;
                bulletAttack.smartCollision = true;
                bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                bulletAttack.damageType |= DamageType.Stun1s;
                bulletAttack.Fire();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            rotateObject.Rotate(new Vector3(0, 2400f, 0) * Time.fixedDeltaTime);
            base.characterDirection.forward = rotateObject.forward;
        }

        public bool UpdateHurtboxes() {
            hurtBoxes.RemoveAll(x => x == null);

            return hurtBoxes.Count > 0;
        }
    }
}