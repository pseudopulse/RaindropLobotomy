using System;

namespace RaindropLobotomy.Ordeals.Midnight.Green {
    public class SpinState : BaseHelixWeapon {
        public Transform endPoint1;
        public Transform endPoint2;
        public GameObject laserInst1;
        public GameObject laserInst2;
        private Timer chargeUpTimer = new(5f);
        //
        private Transform particleSystem;
        private LineRenderer beam;
        private float mult => Mathf.Lerp(0f, 1f, chargeUpTimer.cur / 5f);
        private float targetBeamWidth = 0f;

        private Timer bulletTimer = new(1f / 10, false, true, false, true);
        private float damageCoeff = 6f / 10;

        //

        private Transform particleSystem2;
        private LineRenderer beam2;

        public override void OnEnter()
        {
            base.OnEnter();
            endPoint1 = InitializeLazer(muzzleL, out laserInst1);
            endPoint2 = InitializeLazer(muzzleR, out laserInst2);

            endPoint1.transform.up = -muzzleL.right;
            endPoint2.transform.up = -muzzleR.right;

            beam = laserInst1.FindComponent<LineRenderer>("Beam");
            targetBeamWidth = beam.startWidth;
            particleSystem = laserInst1.FindParticle("Rings").transform;
            particleSystem.gameObject.SetActive(false);

            beam2 = laserInst2.FindComponent<LineRenderer>("Beam");
            particleSystem2 = laserInst2.FindParticle("Rings").transform;
            particleSystem2.gameObject.SetActive(false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            endPoint2.transform.position = new Ray(muzzleR.transform.position, -muzzleR.right).GetPoint(5000);
            endPoint1.transform.position = new Ray(muzzleL.transform.position, -muzzleL.right).GetPoint(5000);

            if (!chargeUpTimer.Tick()) {
                float width = targetBeamWidth * mult;
                beam.startWidth = width;
                beam.endWidth = width;

                beam2.startWidth = width;
                beam2.endWidth = width;

                return;
            }

            if (bulletTimer.Tick()) {
                GetBulletAttack(muzzleL, -muzzleL.right, base.damageStat * damageCoeff, 3f).Fire();
                GetBulletAttack(muzzleR, -muzzleR.right, base.damageStat * damageCoeff, 3f).Fire();
            }

            cannonL.localRotation = Quaternion.Euler(Vector3.Lerp(cannonL.localRotation.eulerAngles, new(0f, 0f, 0f), Mathf.Clamp01((base.fixedAge - 5f) / 2f)));
            cannonR.localRotation = Quaternion.Euler(Vector3.Lerp(cannonR.localRotation.eulerAngles, new(0f, 180f, 0f), Mathf.Clamp01((base.fixedAge - 5f) / 2f)));
            pivot.transform.Rotate(new Vector3(0, 0, 20) * Time.fixedDeltaTime, Space.Self);
        }
    }
}