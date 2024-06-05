using System;
using RoR2.CharacterAI;

namespace RaindropLobotomy.Ordeals.Midnight.Green {
    public class BeamState : BaseHelixWeapon {
        public GameObject laserInst1;
        public Transform endPointC;

        private GameObject chargeEffect;
        private GameObject telegraphEffect;
        private Timer chargeUpTimer = new(5f);
        private Transform particleSystem;
        private LineRenderer beam;
        private float mult => Mathf.Lerp(0f, 1f, chargeUpTimer.cur / 5f);
        private float targetBeamWidth = 0f;

        //

        // private GameObject indicator;
        private GameObject beam2;
        //
        private bool summonedDeathRay = false;
        private BaseAI ai;
        private Vector3 position;
        private Transform target {
            get {
                return ai.currentEnemy.gameObject?.transform ?? null;
            }
        }

        private LaserPattern currentPattern;
        private float belowMapPos = -300f;
        private Timer plasmaDepositInterval = new(0.045f, expires: true, resetOnExpire: true);
        private Timer bullet = new(1f / 10, false, true, false, true);
        private float damageCoeff = 6f / 10;
        private Transform endpointD;

        private class LaserPattern {
            Vector3[] points;
            int currentPoint;
            public bool isDone;
            public float speed;
            private float belowMapPos = -300f;

            public LaserPattern(Vector3[] targetPoints) {
                points = targetPoints;
                currentPoint = 0;
                isDone = false;
            }

            public Vector3 Update(Vector3 current) {
                if (currentPoint > points.Length - 1) {
                    Debug.Log("Points have all been iterated, marking this pattern as done.");
                    currentPoint = points.Length - 1;
                    isDone = true;
                }

                Vector3 output = points[currentPoint];
                output.y = belowMapPos;

                if (Vector3.Distance(current, output) < 1f) {
                    currentPoint++;
                }

                return output;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            // endPoint1 = InitializeLazer(muzzleL, out laserInst1);
            // endPoint2 = InitializeLazer(muzzleR, out laserInst2);
            endPointC = InitializeLazer(muzzleV, out laserInst1);
            laserInst1.transform.parent = null;
            laserInst1.transform.up = Vector3.up;
            endPointC.transform.position = new(endPointC.position.x, 9000, endPointC.position.z);

            beam = laserInst1.FindComponent<LineRenderer>("Beam");
            targetBeamWidth = beam.startWidth;
            particleSystem = laserInst1.FindParticle("Rings").transform;

            //

            ai = characterBody.master.aiComponents[0];

            // indicator = GameObject.Instantiate(LastHelix.IndicatorPrefab, GetRandomPositionIgnoreNodegraph(base.transform.position, 30f, 60f), Quaternion.identity);
            // indicator.transform.up = Vector3.up;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!chargeUpTimer.Tick()) {
                Debug.Log("rizzing up that charge timer");

                Debug.Log(mult);

                float width = targetBeamWidth * mult;
                beam.startWidth = width;
                beam.endWidth = width;

                particleSystem.localScale = Vector3.one * mult;
            }
            else if (!summonedDeathRay) {
                Vector3 random = GetRandomPositionIgnoreNodegraph(base.transform.position, 40f, 70f);

                beam2 = GameObject.Instantiate(LastHelix.LaserPrefab, new(random.x, belowMapPos, random.z), Quaternion.identity);
                beam2.transform.up = Vector3.up;
                beam2.transform.Find("LaserEnd").localPosition += Vector3.up * 1200;
                endpointD = beam2.transform.Find("LaserEnd");

                summonedDeathRay = true;

                ai.FindEnemyHurtBox(4000f, true, false);
            }

            if ((currentPattern == null || currentPattern.isDone) && summonedDeathRay) {
                Vector3[] nodes = LastHelixLaserPatterns.GetSpiralPointSet(GetRandomPositionIgnoreNodegraph(base.transform.position, 40f, 240f), 140f, 35f, 3);

                beam2.transform.position = nodes[0];

                currentPattern = new(nodes)
                {
                    speed = 140f
                };
            }

            if (bullet.Tick() && summonedDeathRay) {
                GetBulletAttack(muzzleV, Vector3.up, base.damageStat * damageCoeff, 3f);
                GetBulletAttack(endpointD, Vector3.down, base.damageStat * damageCoeff, 3f);
            }


            if (currentPattern != null && !currentPattern.isDone && summonedDeathRay) {
                Vector3 output = currentPattern.Update(beam2.transform.position);

                Vector3 next = Vector3.MoveTowards(beam2.transform.position, output, currentPattern.speed * Time.fixedDeltaTime);

                beam2.transform.position = next;

                if (plasmaDepositInterval.Tick()) {
                    Vector3 plasmaTrailRaycastPoint = next + (Vector3.up * 900f);

                    RaycastHit[] hits = Physics.SphereCastAll(plasmaTrailRaycastPoint, 0.2f, Vector3.down, 4000f, LayerIndex.world.mask);

                    foreach (RaycastHit hit in hits) {
                        GameObject prefab = GameObject.Instantiate(LastHelix.PlasmaPrefab, hit.point, Quaternion.identity);
                        PlasmaDamage plasma = prefab.GetComponent<PlasmaDamage>();
                        plasma.owner = base.characterBody;
                        plasma.damagePerSecond = base.damageStat * 6f;
                    }
                }
            }
        }

        public Vector3 GetRandomPositionIgnoreNodegraph(Vector3 origin, float min, float max, int attempts = 0) {
            Vector3 dirVec = Vector3.zero;

            // generate a random forward vector
            while (dirVec == Vector3.zero) {
                dirVec = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
            }

            // decide how far along the direction point should be
            float dist = Random.Range(min, max);

            Vector3 pos = ValidatePosition(origin + (dirVec * dist));

            if (pos == base.transform.position && attempts < 20) {
                // invalid spot, reroll (up to a limit)
                return GetRandomPositionIgnoreNodegraph(origin, min, max, attempts + 1);
            }
            
            return pos;
        }

        public Vector3 ValidatePosition(Vector3 position) {
            Vector3 pos = position;

            if (Physics.Raycast(pos + (Vector3.up * 10f), Vector3.down, out RaycastHit hit, 200f, LayerIndex.world.mask)) {
                return hit.point;
            }

            return base.transform.position;
        }
    }
}