using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using System;
using System.Collections.Generic;
using EntityStates;
using RoR2.ExpansionManagement;
using Unity;
using HarmonyLib;
using RoR2.CharacterAI;
using System.Collections;

namespace RaindropLobotomy.Enemies.ArbiterBoss {
    public class CastShockwave : BaseSkillState {
        public static float DamageCoefficient = 20f;
        public static float InitialDuration = 6f;
        public static int HitCount = 3;
        public static float HitDelay = 1.5f;
        public static Vector3 TeleRad = new(30f, 30f, 30f);
        public static Vector3 ChargeRad = new(4f, 4f, 4f);
        //
        public static GameObject ChargeEffectPrefab => ArbiterBoss.ShockwaveChargeEffect;
        public static GameObject ShockwaveEffectPrefab => ArbiterBoss.ShockwaveEffect;
        public static GameObject TelegraphPrefab => ArbiterBoss.ShockwaveTelegraphEffect;
        //
        private Animator animator;
        private Timer attackBegin = new(3f, expires: true, trueOnExpire: true);
        private Transform hand;
        private bool begunShockwaves;
        private GameObject currentTelegraphFX;
        private GameObject chargeEffect;
        private Vector3 forward;

        public override void OnEnter()
        {
            base.OnEnter();
            
            animator = GetModelAnimator();
            animator.SetBool("isInShockwave", true);
            PlayAnimation("Gesture, Override", "CastShockwave", "CastShockwave.playbackRate", attackBegin.duration);

            AkSoundEngine.PostEvent(Events.Play_moonBrother_phase4_itemSuck_start, base.gameObject);

            forward = characterDirection.forward;

            hand = FindModelChild("MuzzleL");
            chargeEffect = GameObject.Instantiate(ChargeEffectPrefab, hand);
            chargeEffect.transform.localPosition = Vector3.zero;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterMotor.moveDirection = Vector3.zero;
            characterDirection.forward = forward;

            if (!attackBegin.Tick()) {
                chargeEffect.transform.localPosition += new Vector3(0f, 0f, (3f / attackBegin.duration ) * Time.fixedDeltaTime);
                chargeEffect.transform.localScale += (ChargeRad / attackBegin.duration) * Time.fixedDeltaTime;
                return;
            }

            characterMotor.velocity = Vector3.zero;

            if (currentTelegraphFX) {
                currentTelegraphFX.transform.localScale += (TeleRad * 2f / HitDelay) * Time.fixedDeltaTime;
            }

            if (!begunShockwaves) {
                begunShockwaves = true;
                base.characterBody.StartCoroutine(PerformShockwaves());
            }
        }

        public IEnumerator PerformShockwaves() {
            for (int i = 0; i < HitCount; i++) {
                currentTelegraphFX = GameObject.Instantiate(TelegraphPrefab, chargeEffect.transform.position, Quaternion.identity);
                yield return new WaitForSeconds(HitDelay);
                ProcessHit();
                if (i == HitCount - 1) {
                    chargeEffect.gameObject.SetActive(false);
                }
                yield return new WaitForSeconds(HitDelay / 3f + 0.5f);
            }

            outer.SetNextStateToMain();

            yield return null;
        }

        public void ProcessHit() {
            GameObject.Destroy(currentTelegraphFX);
            
            GameObject shockwave = GameObject.Instantiate(ShockwaveEffectPrefab, chargeEffect.transform.position, Quaternion.identity);
            shockwave.transform.localScale = new(30f, 30f, 30f);

            BlastAttack attack = new();
            attack.position = chargeEffect.transform.position;
            attack.radius = 30f;
            attack.crit = base.RollCrit();
            attack.baseDamage = base.damageStat * DamageCoefficient;
            attack.attacker = base.gameObject;
            attack.attackerFiltering = AttackerFiltering.NeverHitSelf;
            attack.losType = BlastAttack.LoSType.NearestHit;
            attack.falloffModel = BlastAttack.FalloffModel.None;
            attack.procCoefficient = 1f;
            attack.teamIndex = base.GetTeam();
            
            attack.Fire();

            AkSoundEngine.PostEvent(Events.Play_moonBrother_phaseJump_shockwave_single, base.gameObject);
            AkSoundEngine.PostEvent(Events.Play_moonBrother_phaseJump_shockwave_single, base.gameObject);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

        public override void OnExit()
        {
            base.OnExit();
            animator.SetBool("isInShockwave", false);
            AkSoundEngine.PostEvent(Events.Play_moonBrother_phase4_itemSuck_end, base.gameObject);
            GameObject.Destroy(chargeEffect);
        }
    }
}