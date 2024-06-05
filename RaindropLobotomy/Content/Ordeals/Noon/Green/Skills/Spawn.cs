using System;

namespace RaindropLobotomy.Ordeals.Noon.Green {
    public class Spawn : BaseState {
        private GameObject pod;
        private SurvivorPodController pC;
        private VehicleSeat vS;
        private GameObject model;
        public override void OnEnter()
        {
            base.OnEnter();

            Debug.Log("toggling model for: " + modelLocator.modelTransform.GetChild(0).gameObject);
            model = base.modelLocator.modelTransform.gameObject;

            // KillYourself(false);

            if (NetworkServer.active) {
                pod = GameObject.Instantiate(base.characterBody.preferredPodPrefab, base.transform.position, base.transform.rotation);
                pod.GetComponent<VehicleSeat>().AssignPassenger(base.gameObject);
                vS = pod.GetComponent<VehicleSeat>();
                pC = pod.GetComponent<SurvivorPodController>();
                vS.onPassengerExit += Ejected;
                NetworkServer.Spawn(pod);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public void KillYourself(bool kys) {
            foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true)) {
                Debug.Log("killing your elf: " + kys);
                renderer.enabled = kys;
            }
        }

        public void Ejected(GameObject passenger) {
            outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}