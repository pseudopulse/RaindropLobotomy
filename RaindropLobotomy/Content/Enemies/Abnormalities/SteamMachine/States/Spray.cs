using System;

namespace RaindropLobotomy.Enemies.SteamMachine {
    public class Spray : BaseState {
        private GameObject sprayInstance;
        private Transform muzzle;
        private Timer sprayTimer = new(0.2f);
        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Gesture, Override", "Spray", "Spray.playbackRate", 2.6f);
            muzzle = FindModelChild("Nozzle");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.characterBody.SetAimTimer(1f);

            if (base.fixedAge <= 0.5f) return;
            if (base.fixedAge >= 2.3f) { outer.SetNextStateToMain(); return; }

            if (!sprayInstance) {
                sprayInstance = GameObject.Instantiate(Assets.GameObject.DroneFlamethrowerEffect, muzzle);
                sprayInstance.transform.forward = -muzzle.forward;
                sprayInstance.transform.localScale = new(0.01f, 0.01f, 0.01f);
                sprayInstance.transform.GetComponent<DynamicBone>().enabled = false;
                Transform bone1 = sprayInstance.transform.Find("Bone1");
                bone1.GetComponent<LineRenderer>().sharedMaterials = new Material[] {
                    Assets.Material.matEnvSteam, Assets.Material.matEnvSteam, Assets.Material.matEnvSteam, Assets.Material.matEnvSteam, Assets.Material.matEnvSteam, Assets.Material.matEnvSteam, Assets.Material.matEnvSteam, Assets.Material.matEnvSteam, Assets.Material.matEnvSteam,
                };
                bone1.transform.localPosition = new(0f, 0f, -2f);
                bone1.Find("Bone2").transform.localPosition = new(0f, 0f, 20f);
                sprayInstance.transform.Find("Billboard").gameObject.SetActive(false);
                sprayInstance.transform.Find("Point Light").gameObject.SetActive(false);
            }


            if (sprayTimer.Tick()) {
                BulletAttack attack = new();
                attack.damage = base.damageStat * 4f;
                attack.aimVector = -muzzle.transform.forward;
                attack.weapon = muzzle.gameObject;
                attack.owner = base.gameObject;
                attack.falloffModel = BulletAttack.FalloffModel.None;
                attack.isCrit = base.RollCrit();
                attack.stopperMask = LayerIndex.noCollision.mask;
                attack.origin = muzzle.transform.position;
                attack.procCoefficient = 1f;
                attack.radius = 1f;
                attack.smartCollision = true;
                attack.maxDistance = 40f;
                attack.muzzleName = "Nozzle";
                
                attack.Fire();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (sprayInstance) Destroy(sprayInstance);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}