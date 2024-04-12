using System;

namespace RaindropLobotomy.Enemies.SingingMachine {
    public class MusicState : BaseState {
        public enum SM_MusicType {
            Low,
            Moderate,
            OhShitWhiteNightBreached
        }

        public SM_MusicType type;
        private float totalDuration {
            get {
                return type switch {
                    SM_MusicType.Low => 20f,
                    SM_MusicType.Moderate => 40f,
                    SM_MusicType.OhShitWhiteNightBreached => 99999f
                };
            }
        }

        private GameObject prefab => Load<GameObject>("SingingMachineBuffWard_T1.prefab");
        private GameObject instance;
        private SingingMachineMain mainState;
        private float stopwatch = 0f;

        public override void OnEnter()
        {
            base.OnEnter();

            mainState = (EntityStateMachine.FindByCustomName(gameObject, "Body").state as SingingMachineMain);

            mainState.disallowLidStateChange = true;
            mainState.lidState = SingingMachineMain.SingingMachineLidState.Closed;

            if (!NetworkServer.active) {
                return;
            }

            instance = GameObject.Instantiate(prefab, base.gameObject.transform.position, Quaternion.identity);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= totalDuration) {
                outer.SetNextStateToMain();
                return;
            }

            stopwatch += Time.fixedDeltaTime;

            if (stopwatch >= 0.1f) {
                stopwatch = 0f;
                // EffectManager.SpawnEffect()
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            GameObject.Destroy(instance);

            mainState.disallowLidStateChange = false;
            mainState.lidState = SingingMachineMain.SingingMachineLidState.Open;
        }
    }
}