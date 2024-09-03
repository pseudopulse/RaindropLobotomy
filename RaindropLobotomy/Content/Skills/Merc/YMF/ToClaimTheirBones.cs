using System;
using System.Collections;
using RaindropLobotomy.Survivors.Sweeper;

namespace RaindropLobotomy.Skills.Merc {
    public class ToClaimTheirBones : BaseSkillState
    {
        private OverlapAttack attack;
        private Animator animator;
        private float damageCoeff;
        private HitBoxGroup hitBoxGroup;
        private float hitBoxScale;
        private Transform target;
        private Vector3 lastTargetPosition;
        private bool wasFlying = false;
        private bool alreadySpawned = false;
        private GameObject swingEffectInstance;
        private string rizz = "GroundLight1";
        private GameObject swingEffectPrefab => Paths.GameObject.MercSwordFinisherSlash;
        public ToClaimTheirBones(Transform enemy) {
            target = enemy;
            lastTargetPosition = target.position;
        }
        public override void OnEnter()
        {
            int resentment = base.characterBody.GetBuffCount(Buffs.Resentment.Instance.Buff);
            characterBody.AddBuff(Buffs.Unrelenting.Instance.Buff);

            hitBoxScale = Util.Remap(resentment, 0, 100, 1.7f, 6f);
            damageCoeff = Util.Remap(resentment, 0, 100, 4f / 4, 46f / 4);

            hitBoxGroup = FindHitBoxGroup("Sword");

            attack = new();
            attack.damage = damageCoeff;
            attack.attacker = base.gameObject;
            attack.hitBoxGroup = hitBoxGroup;
            attack.damageType |= DamageType.ApplyMercExpose;
            attack.damageType |= DamageType.BonusToLowHealth;
            attack.damageType |= DamageType.CrippleOnHit;
            attack.damageType |= DamageType.SlowOnHit;
            attack.damageType |= DamageType.Stun1s;
            attack.teamIndex = base.GetTeam();
            attack.hitEffectPrefab = Paths.GameObject.OmniImpactVFXSlashMerc;
            attack.isCrit = base.RollCrit();
            attack.procCoefficient = 1.5f;
            attack.AddModdedDamageType(Sweeper.BigLifesteal);

            base.OnEnter();

            hitBoxGroup.hitBoxes[0].transform.localScale *= hitBoxScale;

            base.characterBody.StartCoroutine(NahIdLose());

            animator = GetModelAnimator();
        }

        public IEnumerator NahIdLose() {
            yield return new WaitForSeconds(0.35f);
            RandomTeleport();
            yield return new WaitForSeconds(0.3f);
            PlayAnimation("GroundLight1", 0.4f);
            rizz = "GroundLight1";
            yield return new WaitForSeconds(0.4f);
            
            RandomTeleport();
            yield return new WaitForSeconds(0.3f);
            PlayAnimation("GroundLight2", 0.4f);
            rizz = "GroundLight2";
            yield return new WaitForSeconds(0.4f);

            RandomTeleport();
            yield return new WaitForSeconds(0.3f);
            PlayAnimation("GroundLight1", 0.4f);
            rizz = "GroundLight1";
            yield return new WaitForSeconds(0.4f);

            RandomTeleport();
            yield return new WaitForSeconds(0.3f);
            PlayAnimation("GroundLight2", 0.4f);
            rizz = "GroundLight2";

            yield return new WaitForSeconds(1f);
            outer.SetNextStateToMain();
        }

        public void RandomTeleport() {
            Vector3 pos = lastTargetPosition + (Random.onUnitSphere * 4f);
            pos += Vector3.up * 2f;

            TeleportHelper.TeleportBody(base.characterBody, pos);

            EffectManager.SimpleEffect(Paths.GameObject.MercExposeConsumeEffect, pos, Quaternion.identity, true);
        }

        public override void OnExit()
        {
            base.OnExit();
            hitBoxGroup.hitBoxes[0].transform.localScale /= hitBoxScale;
            characterBody.RemoveBuff(Buffs.Unrelenting.Instance.Buff);
            characterBody.SetBuffCount(Buffs.Resentment.Instance.Buff.buffIndex, 0);
            if (swingEffectInstance) {
                GameObject.Destroy(swingEffectInstance);
            }
        }

        public override void FixedUpdate() {
            base.FixedUpdate();
            
            if (animator.GetFloat("Sword.active") >= 0.5f) {
                BeginMeleeAttackEffect(rizz);
                if (base.isAuthority) attack.Fire();
            }

            if (!base.isAuthority) return;

            if (target) {
                lastTargetPosition = target.position;
            }

            base.characterMotor.velocity = Vector3.zero;
            base.characterDirection.forward = (lastTargetPosition - base.transform.position).normalized;
        }

        public void PlayAnimation(string animationStateName, float duration)
        {
            if (swingEffectInstance) {
                GameObject.Destroy(swingEffectInstance);
            }
            attack.ResetIgnoredHealthComponents();
            alreadySpawned = false;
            bool @bool = animator.GetBool("isMoving");
            bool bool2 = animator.GetBool("isGrounded");
            if (!@bool && bool2)
            {
                PlayCrossfade("FullBody, Override", animationStateName, "GroundLight.playbackRate", duration, 0.05f);
            }
            else
            {
                PlayCrossfade("Gesture, Additive", animationStateName, "GroundLight.playbackRate", duration, 0.05f);
                PlayCrossfade("Gesture, Override", animationStateName, "GroundLight.playbackRate", duration, 0.05f);
            }

            AkSoundEngine.PostEvent(animationStateName == "GroundLight1" ? Events.Play_merc_m1_hard_swing : Events.Play_merc_sword_swing, base.gameObject);
        }

        protected virtual void BeginMeleeAttackEffect(string muzzle)
        {
            if (alreadySpawned) {
                return;
            }

            Transform transform = FindModelChild(muzzle);
            
            swingEffectInstance = Object.Instantiate(swingEffectPrefab, transform);
            ScaleParticleSystemDuration component = swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
            
            component.newDuration = component.initialDuration;
            

            alreadySpawned = true;
        }
    }
}