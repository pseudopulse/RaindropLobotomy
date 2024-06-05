using System;

namespace RaindropLobotomy.Ordeals.Midnight.Green {
    public class BaseHelixWeapon : BaseState {
        public Transform muzzleL;
        public Transform muzzleR;
        public Transform cannonL;
        public Transform cannonR;
        public Transform pivot;
        public Transform muzzleV;

        public override void OnEnter()
        {
            base.OnEnter();
            muzzleL = FindModelChild("Muzzle_L");
            muzzleR = FindModelChild("Muzzle_R");
            muzzleV = FindModelChild("Muzzle_V");
            cannonL = FindModelChild("CannonL");
            cannonR = FindModelChild("CannonR");
            pivot = FindModelChild("Pivot");
        }

        public Transform InitializeLazer(Transform muzzle, out GameObject inst) {
            inst = GameObject.Instantiate(LastHelix.LaserPrefab, muzzle);
            return inst.transform.Find("LaserEnd");
        }

        public void PointTowards(Transform cannon, Vector3 position) {
            Vector3 norm = (position - cannon.transform.position).normalized;
            cannon.localRotation = Quaternion.Euler(0, norm.y, 0);
        }

        public BulletAttack GetBulletAttack(Transform muzzle, Vector3 forward, float damage, float radius) {
            BulletAttack attack = new();
            attack.damage = damage;
            attack.origin = muzzle.transform.position;
            attack.aimVector = forward;
            attack.falloffModel = BulletAttack.FalloffModel.None;
            attack.owner = base.gameObject;
            attack.procCoefficient = 0f;
            attack.stopperMask = LayerIndex.noCollision.mask;
            attack.radius = radius;
            attack.smartCollision = false;
            attack.maxDistance = 4000f;
            
            return attack;
        }
    }
}