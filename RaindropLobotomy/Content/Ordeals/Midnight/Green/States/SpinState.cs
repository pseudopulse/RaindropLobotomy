using System;

namespace RaindropLobotomy.Ordeals.Midnight.Green {
    public class SpinState : BaseHelixWeapon {
        public Transform endPoint1;
        public Transform endPoint2;
        public GameObject laserInst1;
        public GameObject laserInst2;
        public GameObject laserInst3;
        public Transform endPointC;

        public override void OnEnter()
        {
            base.OnEnter();
            endPoint1 = InitializeLazer(muzzleL, out laserInst1);
            endPoint2 = InitializeLazer(muzzleR, out laserInst2);
            endPointC = InitializeLazer(muzzleV, out laserInst3);
            endPointC.transform.position = new(endPointC.position.x, 9000, endPointC.position.z);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            cannonL.localRotation = Quaternion.Euler(Vector3.Lerp(cannonL.rotation.eulerAngles, new(0f, 0f, 0f), Mathf.Clamp01(base.fixedAge)));
            cannonR.localRotation = Quaternion.Euler(Vector3.Lerp(cannonR.rotation.eulerAngles, new(0f, 180f, 0f), Mathf.Clamp01(base.fixedAge)));
            pivot.transform.Rotate(new Vector3(0, 0, 20) * Time.fixedDeltaTime, Space.Self);

            endPoint2.transform.position = new Ray(muzzleR.transform.position, -muzzleR.right).GetPoint(5000);
            endPoint1.transform.position = new Ray(muzzleL.transform.position, -muzzleL.right).GetPoint(5000);
        }
    }
}