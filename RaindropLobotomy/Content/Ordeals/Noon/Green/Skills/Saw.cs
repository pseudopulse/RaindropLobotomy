using System;
using Rewired.ComponentControls.Effects;

namespace RaindropLobotomy.Ordeals.Noon.Green {
    public class Saw : CoolerBasicMeleeAttack
    {
        public override float BaseDuration => 3f;

        public override float DamageCoefficient => 6f / hitRate;

        public override string HitboxName => "Saw";

        public override GameObject HitEffectPrefab => Paths.GameObject.OmniImpactVFX;

        public override float ProcCoefficient => 0.4f;

        public override float HitPauseDuration => 0f;

        public override GameObject SwingEffectPrefab => null;

        public override string MuzzleString => "Sawblade";

        private bool defensive = false;
        private float stopwatch = 0f;
        private int hitRate = 10;
        private float delay => 1f / hitRate;
        private Transform saw;
        private GameObject sawblade;
        private Vector3 forward;

        public override void PlayAnimation()
        {
    
            defensive = GetModelAnimator().GetBool("isDefensive");

            GetModelAnimator().SetBool("isSawing", true);
            PlayAnimation("Gesture, Override", defensive ? "Defensive Saw" : "Saw", "Standard.playbackRate", 1f);

            AkSoundEngine.PostEvent(Events.Play_MULT_m1_sawblade_start, base.gameObject);

            saw = FindModelChild("Sawblade");
            saw.GetComponent<RotateAroundAxis>().enabled = true;

            sawblade = GameObject.Instantiate(Paths.GameObject.ToolbotBuzzsawEffectLoop, FindModelChild("SawMuzzle"));
            sawblade.transform.localRotation = Quaternion.Euler(90, 0, 0);

            forward = characterDirection.forward;
        }

        public override void OnExit()
        {
            base.OnExit();
            GetModelAnimator().SetBool("isSawing", false);
            AkSoundEngine.PostEvent(Events.Play_MULT_m1_sawblade_stop, base.gameObject);
            saw.GetComponent<RotateAroundAxis>().enabled = false;
            Destroy(sawblade);
        }

        public override void AuthorityFixedUpdate()
        {
            base.AuthorityFixedUpdate();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.characterMotor._moveDirection = Vector3.zero;
            base.characterDirection.forward = forward;

            stopwatch += Time.fixedDeltaTime;

            if (stopwatch >= delay) {
                stopwatch = 0f;
                overlapAttack.ResetIgnoredHealthComponents();
            }
        }
    }
}