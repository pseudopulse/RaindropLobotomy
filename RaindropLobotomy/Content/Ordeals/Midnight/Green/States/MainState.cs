using System;

namespace RaindropLobotomy.Ordeals.Midnight.Green {
    public class MainState : GenericCharacterMain {
        private GameObject laserInst;
        public override void OnEnter()
        {
            base.OnEnter();
            Transform p = FindModelChild("MuzzleVert");
            laserInst = GameObject.Instantiate(LastHelix.LaserPrefab, p.transform.position, Quaternion.identity);
            laserInst.transform.Find("LaserEnd").transform.position = new(p.transform.position.x, 4000f, p.transform.position.z);
        }
    }
}