using System;
using System.Linq;
using EntityStates.AI;
using RoR2.CharacterAI;

namespace RaindropLobotomy.Enemies.SingingMachine {
    public class BewitchedState : BaseState {
        public SingingMachineMain mainState;
        public GameObject enemy;
        public BaseAI enemyAI;
        public CharacterBody enemyBody;
        private bool didWeExitEarly = false;
        private GameObject indicatorPrefab => Load<GameObject>("BewitchedIndicator.prefab");
        private GameObject indicator;
        private bool begunJump = false;
        private bool crushing = false;
        private EntityStateMachine aiESM;
        private float stopwatch = 0f;
        private AISkillDriver targetDriver;
        private BaseAI ai => enemyAI;

        public override void OnEnter()
        {
            base.OnEnter();

            mainState = (EntityStateMachine.FindByCustomName(gameObject, "Body").state as SingingMachineMain);

            SphereSearch search = new();
            search.radius = 50f;
            search.origin = base.transform.position;
            search.mask = LayerIndex.entityPrecise.mask;
            search.RefreshCandidates();
            search.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(TeamIndex.Player));
            search.FilterCandidatesByDistinctHurtBoxEntities();
            List<HurtBox> box = search.GetHurtBoxes().OrderBy(x => x.healthComponent.health).Where(x => !x.healthComponent.body.isBoss && x.healthComponent.body.moveSpeed > 0 && !x.healthComponent.body.isFlying && !x.healthComponent.body.isPlayerControlled).ToList();

            if (box.Count > 0) {
                enemy = box[0].healthComponent.gameObject;
            }

            if (!enemy) {
                didWeExitEarly = true;
                outer.SetNextStateToMain();
                return;
            }

            enemyBody = enemy.GetComponent<CharacterBody>();
            enemyAI = enemyBody.master?.GetComponent<BaseAI>() ?? null;
            
            if (!enemyAI || !enemyBody.GetComponent<CharacterMotor>()) {
                didWeExitEarly = true;
                outer.SetNextStateToMain();
                return;
            }

            targetDriver = enemyAI.gameObject.AddComponent<AISkillDriver>();
            targetDriver.customName = "ChaseInteractable";
            targetDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            targetDriver.moveTargetType = AISkillDriver.TargetType.Custom;
            targetDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            targetDriver.maxDistance = Mathf.Infinity;
            targetDriver.minDistance = 0f;
            targetDriver.skillSlot = SkillSlot.None;
            targetDriver.resetCurrentEnemyOnNextDriverSelection = true;
            targetDriver.shouldSprint = true;
            targetDriver.requireSkillReady = false;
            targetDriver.selectionRequiresAimTarget = false;
            targetDriver.selectionRequiresOnGround = false;
            targetDriver.activationRequiresTargetLoS = false;
            targetDriver.activationRequiresAimTargetLoS = false;
            targetDriver.activationRequiresAimConfirmation = false;
            List<AISkillDriver> drivers = enemyAI.skillDrivers.ToList();
            drivers.Add(targetDriver);
            enemyAI.skillDrivers = drivers.ToArray();

            aiESM = enemyAI.GetComponent<EntityStateMachine>();

            // Debug.Log("Bewitching enemy: " + enemy);

            indicator = GameObject.Instantiate(indicatorPrefab, enemyBody.transform);
            indicator.transform.position = new(enemyBody.corePosition.x, enemyBody.corePosition.y + (enemyBody.radius) + 3f, enemyBody.corePosition.z);

            EffectManager.SimpleEffect(Paths.GameObject.OmniImpactExecute, indicator.transform.position, Quaternion.identity, false);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if ((base.fixedAge >= 30f || !enemy || !ai) && !crushing) {
                if (indicator) Destroy(indicator);
                outer.SetNextStateToMain();
                return;
            }

            if (!ai) {
                return;
            }

            if (!NetworkServer.active) return;

            stopwatch += Time.fixedDeltaTime;

            ai.customTarget.gameObject = base.gameObject;

            if (ai.customTarget.gameObject) {
                ai.customTarget.Update();
                ai.SetGoalPosition(ai.customTarget.gameObject.transform.position);
                ai.localNavigator.Update(Time.fixedDeltaTime);

                ai.skillDriverEvaluation = new BaseAI.SkillDriverEvaluation {
                    dominantSkillDriver = targetDriver,
                    aimTarget = ai.customTarget,
                    target = ai.customTarget
                };
            }

            if (Vector3.Distance(enemy.transform.position, base.transform.position) < 20f) {
                mainState.UpdateLidState(SingingMachineMain.SingingMachineLidState.Open);
                mainState.disallowLidStateChange = true;
            }

            if (Vector3.Distance(enemy.transform.position, base.transform.position) < 5f && !begunJump) {
                begunJump = true;

                // Debug.Log("jumping");

                float ySpeed = Trajectory.CalculateInitialYSpeed(0.7f, ((base.transform.position.y + 3f) - enemyBody.corePosition.y));
                float xOff = (base.transform.position.x - enemyBody.corePosition.x);
                float zOff = (base.transform.position.z - enemyBody.corePosition.z);

                Vector3 velocity = new(xOff / 0.7f, ySpeed, zOff / 0.7f);

                enemyBody.GetComponent<CharacterMotor>().velocity = velocity;
                enemyBody.GetComponent<CharacterMotor>().disableAirControlUntilCollision = true;
                enemyBody.GetComponent<CharacterMotor>().Motor.ForceUnground();

                // Debug.Log(enemyBody.GetComponent<CharacterMotor>().velocity);
            }

            if (Vector3.Distance(new(enemy.transform.position.x, 0, enemy.transform.position.z), new(base.transform.position.x, 0, base.transform.position.z)) < 0.6f && !crushing) {
                crushing = true;
                // Debug.Log("CRUSHING!");
                enemyBody.GetComponent<CharacterMotor>().enabled = false;
                enemyBody.GetComponent<CharacterBody>().baseMoveSpeed = 0f;
                enemyBody.GetComponent<CharacterBody>().moveSpeed = 0f;
                mainState.disallowLidStateChange = false;
                mainState.UpdateLidState(SingingMachineMain.SingingMachineLidState.Closed);
                mainState.disallowLidStateChange = true;
                enemyBody.healthComponent.Suicide();
            }

            if (crushing && (!enemy || !enemyBody.healthComponent.alive)) {
                outer.SetNextState(new MusicState() { type = MusicState.SM_MusicType.Low });
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (indicator) {
                GameObject.Destroy(indicator);
            }
            skillLocator.secondary.rechargeStopwatch = didWeExitEarly ? 2f : 40f;
        }
    }
}