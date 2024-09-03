using System;

namespace RaindropLobotomy.EGO.Merc {
    public class Flurry : BaseSkillState {
        public OverlapAttack attack;
        public bool hasFired = false;
        public Vector3 forwardLock;
        public LineRenderer chargeRenderer;
        public bool hasHitTarget = false;
        
        public override void OnEnter()
        {
            base.OnEnter();

            chargeRenderer = GameObject.Instantiate(Load<GameObject>("HandChargeBeam"), FindModelChild("MuzzleCharge")).GetComponent<LineRenderer>();

            attack = new();
            attack.hitBoxGroup = FindHitBoxGroup("Hitbox");
            attack.damage = base.damageStat * 7f;
            attack.pushAwayForce = 4000f;
            attack.attacker = base.gameObject;
            attack.hitEffectPrefab = Paths.GameObject.OmniImpactVFXLoader;
            attack.teamIndex = base.GetTeam();
            attack.procCoefficient = 1f;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!hasFired) {
                base.StartAimMode(0.2f);

                base.rigidbody.velocity = Vector3.zero;

                chargeRenderer.widthMultiplier = 1f - (base.fixedAge / 1f);

                base.rigidbody.rotation = Util.QuaternionSafeLookRotation(base.inputBank.aimDirection);

                Vector3 endPos = base.GetAimRay().GetPoint(90f);

                if (Physics.Raycast(base.transform.position, base.GetAimRay().direction, out RaycastHit info, 400f, LayerIndex.world.mask)) {
                    endPos = info.point;
                }

                chargeRenderer.SetPosition(0, chargeRenderer.transform.position);
                chargeRenderer.SetPosition(1, endPos);

                if (base.fixedAge >= 1f) {
                    hasFired = true;

                    forwardLock = base.GetAimRay().direction;
                    
                    Destroy(chargeRenderer.gameObject);
                    base.fixedAge = 0f;
                }
            }
            else {

                base.rigidbody.rotation = Util.QuaternionSafeLookRotation(forwardLock);

                if (!hasHitTarget) {
                    bool temp = attack.Fire();
                    if (temp) {
                        hasHitTarget = temp;
                        base.rigidbody.velocity = Vector3.zero;
                        EffectManager.SpawnEffect(Paths.GameObject.HermitCrabBombExplosion, new EffectData {
                            origin = base.transform.position,
                            scale = 5f
                        }, false);
                    }
                }

                base.rigidbody.velocity = forwardLock * 90f;

                if (base.fixedAge >= 0.5f) {
                    outer.SetNextStateToMain();
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}