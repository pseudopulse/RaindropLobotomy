using System;

namespace RaindropLobotomy.Enemies.SteamMachine {
    public class Spray : BaseState {
        private GameObject sprayInstance;
        private Transform muzzle;
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