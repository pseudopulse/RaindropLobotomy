using System;

namespace RaindropLobotomy.Enemies.Fragment {
    public class Penetrate : CoolerBasicMeleeAttack
    {
        public override float BaseDuration => 1.2f;

        public override float DamageCoefficient => 4f;

        public override string HitboxName => "StabHitbox";

        public override GameObject HitEffectPrefab => Assets.GameObject.VoidImpactEffect;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0f;

        public override GameObject SwingEffectPrefab => null;

        public override string MuzzleString => "MuzzleTendril";
        private bool spawnedEffect = false;

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterMotor.velocity = Vector3.zero;

            if (base.fixedAge >= 0.4f && !spawnedEffect) {
                spawnedEffect = true;

                GameObject.Instantiate(UniverseFragment.SpearThrust, FindModelChild("SpearMuzzle"));
            }
        }

        public override void PlayAnimation()
        {
            PlayAnimation("Gesture, Override", "Penetrate", "Penetrate.playbackRate", 1f);
            AkSoundEngine.PostEvent("Play_fragment_stab", base.gameObject);
        }
    }
}