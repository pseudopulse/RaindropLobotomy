using System;
using System.Collections;
using System.Linq;

namespace RaindropLobotomy.EGO.Toolbot {
    public class Limit : BaseSkillState {
        public float DamageCoefficient = 8f;
        public int DashCount = 3;
        public string MuzzleName = "MuzzleCleanup";
        public string MechanimFreeze = "Split.winded";
        public string MechanimActive = "Hitbox.active";
        public string HitBoxName = "Special";
        //
        private OverlapAttack attack;
        private Animator animBlades;
        private GrinderPathPoint[] points;
        private List<GrinderPathPoint> line;
        private Vector3 target = Vector3.zero;
        private Vector3 velocity;
        private bool reachedTarget = false;
        private CameraTargetParams.CameraParamsOverrideHandle? handle = null;
        private Vector3 lastPos;
        private Timer updateLastPosTimer = new Timer(0.1f, false, true, false, true);

        public override void OnEnter()
        {
            base.OnEnter();
            
            attack = new();
            attack.damage = base.damageStat * DamageCoefficient;
            attack.attacker = base.gameObject;
            attack.hitBoxGroup = FindHitBoxGroup(HitBoxName);
            attack.damageType |= DamageType.Stun1s;
            attack.damageType |= DamageType.BleedOnHit;
            attack.teamIndex = base.GetTeam();
            attack.hitEffectPrefab = Assets.GameObject.SpurtImpBlood;
            attack.isCrit = base.RollCrit();

            UpdateBoxes();

            if (line.Count <= 0f) {
                outer.SetNextStateToMain();
                return;
            }

            base.gameObject.layer = LayerIndex.fakeActor.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();

            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            base.characterBody.StartCoroutine(CleanupDuty());

            FindModelChild("LimitTrail").gameObject.SetActive(true);

            handle = base.GetComponent<CameraTargetParams>().AddParamsOverride(new() {
                cameraParamsData = Assets.CharacterCameraParams.ccpStandardTall.data,
            }, 1f);

            Grinder.DecreaseCharge(base.characterBody, 10);

            base.characterMotor._gravityParameters.channeledAntiGravityGranterCount++;
            
            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.ArmorBoost);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }

        public void UpdateBoxes() {
            SphereSearch search = new();
            search.radius = 90f;
            search.origin = base.characterBody.corePosition;
            search.mask = LayerIndex.entityPrecise.mask;
            search.RefreshCandidates();
            search.OrderCandidatesByDistance();
            search.FilterCandidatesByDistinctHurtBoxEntities();
            search.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(GetTeam()));

            List<HurtBox> boxes = search.GetHurtBoxes().ToList();
            line = new();

            if (boxes.Count <= 0f) {
                outer.SetNextStateToMain();
                return;
            }

            foreach (HurtBox box in boxes) {
                line.Add(new(box.transform.position));
            }

            foreach (GrinderPathPoint point in line) {
                point.connected = LinkPoints(point, line);
                point.NumConnected = point.connected.Length;
            }

            line.OrderByDescending(x => x.NumConnected);
            // line.RemoveAll(x => x.NumConnected == 0);
            points = line.ToArray();
        }

        public override void OnExit()
        {
            base.OnExit();

            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();

            base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;

            FindModelChild("LimitTrail").gameObject.SetActive(false);

            if (NetworkServer.active)
			{
				base.characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
			}

            if (handle.HasValue) {
                base.GetComponent<CameraTargetParams>().RemoveParamsOverride(handle.Value);
            }

            base.characterMotor._gravityParameters.channeledAntiGravityGranterCount--;
        }

        public IEnumerator CleanupDuty() {
            // PlayAnimation();

            for (int i = 0; i < DashCount; i++) {
                
                if (base.skillLocator.special.stock < 1) {
                    goto end;
                }

                UpdateBoxes();
                points = line.ToArray();
                yield return new WaitForSeconds(0.2f);

                for (int j = 0; j < points.Length; j++) {
                    GrinderPathPoint segment = points[j];

                    /*if (!CheckLOS(base.transform.position, segment.origin)) {
                         continue;
                    }*/

                    int livingHurtboxes = 0;

                    Collider[] col = Physics.OverlapSphere(segment.origin, 5f, LayerIndex.entityPrecise.mask);

                    for (int k = 0; k < col.Length; k++) {
                        Collider collider = col[k];

                        if (collider.GetComponent<HurtBox>()) {
                            HurtBox hb = collider.GetComponent<HurtBox>();
                            if (hb.healthComponent && hb.healthComponent.alive) {
                                livingHurtboxes++;
                            }
                        }
                    }

                    if (livingHurtboxes == 0) {
                        continue;
                    }

                    if (Vector3.Distance(base.transform.position, segment.origin) >= 5f && !CheckLOS(base.transform.position, segment.origin)) {
                        continue;
                    }

                    target = segment.origin;
                    velocity = (target - base.transform.position).normalized;
                    reachedTarget = false;
                    attack.ResetIgnoredHealthComponents();
                    PlayAnimation();

                    GameObject.Instantiate(Grinder.MultiSlash, FindModelChild("Chest"));

                    while (!reachedTarget) {
                        yield return new WaitForEndOfFrame();
                    }

                    base.characterMotor.velocity = Vector3.zero;

                    velocity = Vector3.zero;

                    yield return new WaitForSeconds(0.1f);
                }

                base.skillLocator.special.DeductStock(1);
            }

            end:

            outer.SetNextStateToMain();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (updateLastPosTimer.Tick()) {
                if (Vector3.Distance(lastPos, base.transform.position) < 1f) {
                    reachedTarget = true;
                }

                lastPos = base.transform.position;
            }

            if (base.fixedAge >= 10f) {
                outer.SetNextStateToMain();
            }

            if (target != Vector3.zero && !reachedTarget) {
                base.characterMotor.velocity = velocity * 140f;
                base.characterDirection.forward = velocity;

                Vector3 relativePos = target - base.transform.position;

                if (Vector3.Dot(velocity, relativePos) < 0f) {
                    // base.characterMotor.velocity = Vector3.zero;
                    reachedTarget = true;
                }
            }
            
            if (reachedTarget) {
                base.characterMotor.velocity = Vector3.zero;
            }

            attack.Fire();
        }

        public void RandomTeleport(Vector3 targetPos) {
            Vector3 pos = targetPos + (Random.onUnitSphere * 2f);
            pos += Vector3.up * 2f;

            TeleportHelper.TeleportBody(base.characterBody, pos);

            EffectManager.SimpleEffect(Assets.GameObject.MercExposeConsumeEffect, pos, Quaternion.identity, true);
        }

        public void PlayAnimation() {
            animBlades = FindModelChild("Blades").GetComponent<Animator>();
            animBlades.SetFloat("Split.playbackRate", 2f);
            PlayAnimationOnAnimator(animBlades, "Weapon", "Slash", "Split.playbackRate", 0.4f);
            
            AkSoundEngine.PostEvent(Events.Play_bandit2_m2_slash, base.gameObject);
        }

        public bool CheckHasLOSToOtherBox(HurtBox box, List<HurtBox> others) {
            foreach (HurtBox other in others) {
                if (box == other) continue;

                if (CheckLOS(box.transform.position, other.transform.position)) {
                    return true;
                }
            }

            return false;
        }

        public bool CheckLOS(Vector3 origin, Vector3 target) {
            return !Physics.SphereCast(origin, 0.3f, (target - origin).normalized, out RaycastHit info, 30f, LayerIndex.world.mask);
        }

        public GrinderPathPoint[] LinkPoints(GrinderPathPoint point, List<GrinderPathPoint> points) {
            List<GrinderPathPoint> allPoints = points;
            List<GrinderPathPoint> linkedPoints = new();

            foreach (GrinderPathPoint point2 in points) {
                if (point2 == point) continue;

                if (CheckLOS(point.origin, point2.origin)) linkedPoints.Add(point2);
            }

            return linkedPoints.ToArray();
        }

        public class GrinderPathPoint {
            public Vector3 origin;
            public GrinderPathPoint[] connected;
            public int NumConnected;

            public GrinderPathPoint(Vector3 _origin) {
                origin = _origin;
            }
        }
    }
}