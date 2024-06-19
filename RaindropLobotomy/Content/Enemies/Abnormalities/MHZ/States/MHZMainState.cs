using System;

namespace RaindropLobotomy.Enemies.MHZ {
    public class MHZMain : BaseState {
        public Timer interval = new(10f, false, true, false, true);
        public GameObject prefab => MHZ.FogEffect;

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (interval.Tick() && NetworkServer.active) {
                Vector3 pos = MiscUtils.GetRandomGroundNode(RoR2.Navigation.NodeFlags.None, RoR2.Navigation.NodeFlags.NoCharacterSpawn, HullMask.BeetleQueen);

                GameObject fog = GameObject.Instantiate(prefab, pos, Quaternion.identity);
                NetworkServer.Spawn(fog);
            }
        }
    }
}