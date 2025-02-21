using System;
using System.Collections;
using RaindropLobotomy.Buffs;

namespace RaindropLobotomy.EGO.Merc {
    public class ToClaimTheirBones_Exit : BaseState {
        public override void OnEnter()
        {
            base.OnEnter();

            if (NetworkServer.active) {
                base.characterBody.SetBuffCount(Unrelenting.Instance.Buff.buffIndex, 0);
                base.characterBody.SetBuffCount(RoR2Content.Buffs.HiddenInvincibility.buffIndex, 0);
            }

            outer.SetNextStateToMain();

            BLMerc.UpdateYieldingState(base.characterBody, false);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }

    public class ToClaimTheirBones_Transition : BaseState {
        public float duration;
        public EntityState next;
        private bool force = false;

        public ToClaimTheirBones_Transition(float duration, EntityState next, bool force = false) {
            this.duration = duration;
            this.next = next;
            this.force = force;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (force) base.characterMotor.velocity = Vector3.zero;

            if (base.fixedAge >= duration) {
                outer.SetNextState(next);
            }
        }
    }

    public class ToClaimTheirBones_3 : CoolerBasicMeleeAttack
    {
        public override float BaseDuration => 1f;

        public override float DamageCoefficient => 20f;

        public override string HitboxName => "WhirlwindAir";

        public override GameObject HitEffectPrefab => Paths.GameObject.ImpactMercSwing;

        public override float ProcCoefficient => 3f;

        public override float HitPauseDuration => 0.05f;

        public override GameObject SwingEffectPrefab => BLMerc.DarkRadial2;

        public override string MuzzleString => "WhirlwindAir";

        public override string MechanimHitboxParameter => "Sword.active";
        private float bonusDamage;
        private bool hasExited = false;
        private GameObject visuals;
        private Vector3 forward;
        private float stopwatch = 0f;

        public ToClaimTheirBones_3(float bonusDamage, GameObject visuals, Vector3 forward) {
            this.bonusDamage = bonusDamage;
            this.visuals = visuals;
            this.forward = forward;
        }

        public override void OnEnter()
        {
            damageCoefficient *= bonusDamage;
            base.OnEnter();

            if (!Physics.Raycast(base.transform.position, forward, 90f, LayerIndex.world.mask)) {
                forward = Vector3.down;
            }

            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
        }

        public override void OnExit()
        {
            base.OnExit();

            base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            
            PlayAnimation("FullBody, Override", "WhirlwindAirExit");

            if (NetworkServer.active) {
                base.characterBody.RemoveBuff(Unrelenting.Instance.Buff);
                base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            BLMerc.UpdateYieldingState(base.characterBody, false);

            if (base.isAuthority) {
                BlastAttack attack = new();
                attack.attacker = base.gameObject;
                attack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                attack.baseDamage = base.damageStat * damageCoefficient;
                attack.crit = base.RollCrit();
                attack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                attack.radius = 30f;
                attack.position = base.transform.position;
                attack.teamIndex = base.GetTeam();
                attack.procCoefficient = 3f;
                attack.baseDamage *= bonusDamage;
                attack.damageType |= DamageType.CrippleOnHit;

                EffectManager.SpawnEffect(Paths.GameObject.HermitCrabBombExplosion, new EffectData {
                    origin = attack.position,
                    scale = attack.radius
                }, true);

                attack.Fire();
            }
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.damageType = DamageTypeCombo.GenericSpecial;
            overlapAttack.damageType |= DamageType.CrippleOnHit;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.fixedDeltaTime;

            if (!base.isGrounded) {
                base.characterMotor.velocity = forward * 200f;
                base.fixedAge = 0f;
            }

            if ((base.isGrounded || stopwatch >= 10f) && !hasExited) {
                hasExited = true;
                base.characterMotor.velocity = Vector3.zero;
                GameObject.Destroy(visuals);
                AuthorityOnFinish();
            }
        }

        public override void PlayAnimation()
        {
            PlayCrossfade("FullBody, Override", "WhirlwindAir", "Whirlwind.playbackRate", duration, 0.1f);
            AkSoundEngine.PostEvent(Events.Play_merc_sword_swing, base.gameObject);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

        public override void AuthorityOnFinish()
        {
            outer.SetNextState(new ToClaimTheirBones_Transition(1f, new ToClaimTheirBones_Exit()));
        }
    }

    public class ToClaimTheirBones_2 : CoolerBasicMeleeAttack
    {
        public override float BaseDuration => 0.4f;

        public override float DamageCoefficient => 15f;

        public override string HitboxName => "WhirlwindAir";

        public override GameObject HitEffectPrefab => Paths.GameObject.ImpactMercSwing;

        public override float ProcCoefficient => 2f;

        public override float HitPauseDuration => 0.05f;

        public override GameObject SwingEffectPrefab => BLMerc.Upslash;

        public override string MuzzleString => "WhirlwindAir";

        public override string MechanimHitboxParameter => "Sword.active";
        private float bonusDamage;
        private GameObject visuals;
        private Vector3 attackerPosition;
        public ToClaimTheirBones_2(float bonusDamage, GameObject visuals, Vector3 attackerPosition) {
            this.bonusDamage = bonusDamage;
            this.attackerPosition = attackerPosition;
            this.visuals = visuals;
        }

        public override void OnEnter()
        {
            damageCoefficient *= bonusDamage;
            base.OnEnter();

            base.characterMotor.Motor.ForceUnground();
            base.characterMotor.velocity = Vector3.up * 86f + (base.characterDirection.forward * 8f);
        }

        public override void OnExit()
        {
            base.OnExit();
            PlayAnimation("FullBody, Override", "UppercutExit");
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.forceVector = Vector3.up * 3000f;
            overlapAttack.damageType = DamageTypeCombo.GenericSpecial;
            overlapAttack.damageType |= DamageType.CrippleOnHit;
        }

        public override void PlayAnimation()
        {
            PlayCrossfade("FullBody, Override", "Uppercut", "Uppercut.playbackRate", duration, 0.1f);
            AkSoundEngine.PostEvent(Events.Play_merc_m2_uppercut, base.gameObject);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

        public override void AuthorityOnFinish()
        {
            // GameObject.Instantiate(BLMerc.DarkFlare, visuals.transform);
            outer.SetNextState(new ToClaimTheirBones_Transition(0.3f, new ToClaimTheirBones_3(bonusDamage, visuals, (attackerPosition - base.transform.position).normalized), true));
        }
    }

    public class ToClaimTheirBones_1 : BaseSkillState
    {
        public string HitboxName = "SwordLarge";
        public GameObject SwingEffectPrefab => BLMerc.DarkSlash;
        public string MuzzleString => "GroundLight1";
        public string MechanimHitboxParameter => "Sword.active";
        private float bonusDamage;
        private Vector3 forward;
        private Transform attacker;
        private Vector3 teleportPosition;
        private GameObject visuals;
        private Animator animator;
        private OverlapAttack attack;
        private Vector3 lastTargetPosition;
        private string rizz;
        private bool alreadySpawned;
        private GameObject swingEffectInstance;

        public ToClaimTheirBones_1(float bonusDamage, Transform attacker) {
            this.bonusDamage = bonusDamage;
            this.attacker = attacker;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            base.characterMotor.Motor.ForceUnground();
            forward = Random.onUnitSphere;
            teleportPosition = base.transform.position;

            BLMerc.UpdateYieldingState(base.characterBody, true);

            visuals = GameObject.Instantiate(BLMerc.WarpTrail, FindModelChild("WhirlwindAir"));
            animator = GetModelAnimator();

            HitBoxGroup hitBoxGroup = FindHitBoxGroup(HitboxName);

            Debug.Log(bonusDamage);

            attack = new();
            attack.damage = base.damageStat * 3f * bonusDamage;
            attack.attacker = base.gameObject;
            attack.hitBoxGroup = hitBoxGroup;
            attack.damageType = DamageTypeCombo.GenericSpecial;
            attack.damageType |= DamageType.Stun1s;
            attack.damageType |= DamageType.BleedOnHit;
            attack.teamIndex = base.GetTeam();
            attack.hitEffectPrefab = Paths.GameObject.OmniImpactVFXSlashMerc;
            attack.isCrit = base.RollCrit();
            attack.procCoefficient = 1.5f;

            base.characterBody.StartCoroutine(NahIdLose());
        }

        public IEnumerator NahIdLose() {
            yield return new WaitForSeconds(0.35f);
            RandomTeleport();
            yield return new WaitForSeconds(0.1f);
            PlayAnimation("GroundLight1", 0.3f);
            rizz = "GroundLight1";
            yield return new WaitForSeconds(0.3f);
            
            RandomTeleport();
            yield return new WaitForSeconds(0.1f);
            PlayAnimation("GroundLight2", 0.3f);
            rizz = "GroundLight2";
            yield return new WaitForSeconds(0.3f);

            RandomTeleport();
            yield return new WaitForSeconds(0.1f);
            PlayAnimation("GroundLight1", 0.3f);
            rizz = "GroundLight1";
            yield return new WaitForSeconds(0.3f);

            RandomTeleport();
            yield return new WaitForSeconds(0.1f);
            PlayAnimation("GroundLight2", 0.3f);
            rizz = "GroundLight2";
            
            yield return new WaitForSeconds(0.3f);

            RandomTeleport();
            yield return new WaitForSeconds(0.1f);
            PlayAnimation("GroundLight1", 0.3f);
            rizz = "GroundLight1";
            yield return new WaitForSeconds(0.3f);
            
            RandomTeleport();
            yield return new WaitForSeconds(0.1f);
            PlayAnimation("GroundLight2", 0.3f);
            rizz = "GroundLight2";
            yield return new WaitForSeconds(0.3f);

            RandomTeleport();
            yield return new WaitForSeconds(0.1f);
            PlayAnimation("GroundLight1", 0.3f);
            rizz = "GroundLight1";
            yield return new WaitForSeconds(0.3f);

            RandomTeleport();
            yield return new WaitForSeconds(0.1f);
            PlayAnimation("GroundLight2", 0.3f);
            rizz = "GroundLight2";

            yield return new WaitForSeconds(0.3f);
            
            AuthorityOnFinish();
        }

        protected virtual void BeginMeleeAttackEffect(string muzzle)
        {
            if (alreadySpawned) {
                return;
            }

            Transform transform = FindModelChild(muzzle);
            
            swingEffectInstance = Object.Instantiate(BLMerc.DarkSlash, transform);
            ScaleParticleSystemDuration component = swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
            
            component.newDuration = component.initialDuration;
            

            alreadySpawned = true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            
            base.characterBody.isSprinting = true;
            // base.characterMotor.velocity = base.characterDirection.forward * 10f;

            if (attacker) {
                lastTargetPosition = attacker.transform.position;
            }

            if (animator.GetFloat("Sword.active") >= 0.5f) {
                BeginMeleeAttackEffect(rizz);
                if (base.isAuthority) attack.Fire();
            }

            if (!base.isAuthority) return;

            base.characterMotor.velocity = Vector3.zero;
            base.characterDirection.forward = (lastTargetPosition - base.transform.position).normalized;
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
        }

        public void PlayAnimation(string state, float duration)
        {
            alreadySpawned = false;
            string animationStateName = state;
            bool @bool = animator.GetBool("isMoving");
            bool bool2 = animator.GetBool("isGrounded");
            if (!@bool && bool2)
            {
                PlayCrossfade("FullBody, Override", animationStateName, "GroundLight.playbackRate", duration, 0.05f);
                return;
            }
            PlayCrossfade("Gesture, Additive", animationStateName, "GroundLight.playbackRate", duration, 0.05f);
            PlayCrossfade("Gesture, Override", animationStateName, "GroundLight.playbackRate", duration, 0.05f);

            AkSoundEngine.PostEvent(Events.Play_merc_sword_swing, base.gameObject);

            attack.ResetIgnoredHealthComponents();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

        public void AuthorityOnFinish()
        {
            TeleportHelper.TeleportBody(base.characterBody, teleportPosition);

            EffectManager.SimpleEffect(Paths.GameObject.MercExposeConsumeEffect, teleportPosition, Quaternion.identity, true);
            outer.SetNextState(new ToClaimTheirBones_Transition(0.2f, new ToClaimTheirBones_2(bonusDamage, visuals, lastTargetPosition)));
        }
    }
}