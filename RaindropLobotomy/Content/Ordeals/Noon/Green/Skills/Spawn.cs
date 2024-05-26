using System;

namespace RaindropLobotomy.Ordeals.Noon.Green {
    public class Spawn : BaseState {
        public override void OnEnter()
        {
            base.OnEnter();

            GameObject pod = GameObject.Instantiate(base.characterBody.preferredPodPrefab, base.transform.position, base.transform.rotation);
            pod.GetComponent<VehicleSeat>().AssignPassenger(base.gameObject);
            NetworkServer.Spawn(pod);

            outer.SetNextStateToMain();
        }
    }
}