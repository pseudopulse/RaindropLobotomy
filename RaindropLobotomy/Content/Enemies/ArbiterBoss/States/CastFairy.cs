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
    public class CastFairy : BaseSkillState {
        public static string MuzzleName = "MuzzleL";
        public static float DamageCoefficient = 4f;
        public static GameObject Flash => ArbiterBoss.FairyMuzzleFlash;
        //
        private GameObject chargeInstance;
        private LineRenderer lr;
        private Vector3 forward;
        private bool fired = false;

        public override void OnEnter()
        {
            base.OnEnter();
    
            Debug.Log("playing fairy anim");
            PlayAnimation("Gesture, Override", "CastFairy");

            // GetModelAnimator().SetBool("shouldAimPitch", true);

            Transform muzzle = FindModelChild("SlashMuzzle");

            Vector3 pos = base.transform.position + (Vector3.up + (base.characterDirection.forward * 0.4f));
            Quaternion forward = Quaternion.LookRotation(base.characterDirection.forward) * Quaternion.AngleAxis(90, Vector3.up);
            Debug.Log("pos: " + pos);

            GameObject swing = GameObject.Instantiate(ArbiterBoss.ArbiterSlashEffect, pos, forward);
            // swing.GetComponent<ScaleParticleSystemDuration>().newDuration = 3f;

            /*swing = GameObject.Instantiate(Assets.GameObject.MercSwordFinisherSlash, muzzle.transform.position, muzzle.transform.rotation);
            swing.GetComponent<ScaleParticleSystemDuration>().newDuration = swing.GetComponent<ScaleParticleSystemDuration>().initialDuration;*/
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterMotor.moveDirection = Vector3.zero;
            
            characterBody.SetAimTimer(0.5f);

            base.characterBody.SetAimTimer(0.4f);

            if (base.fixedAge >= 0.7f && !fired) {
                Ray lrRay = base.GetAimRay();

                AkSoundEngine.PostEvent(Events.Play_MULT_m1_snipe_shoot, base.gameObject);

                for (float i = 3f; i < (Vector3.Distance(lrRay.origin, lrRay.GetPoint(70))); i += 5f) {
                    Vector3 pos = lrRay.origin + (lrRay.direction * i);
                    GameObject.Instantiate(ArbiterBoss.FairyTracerSlashEffect, pos, Quaternion.LookRotation(Random.onUnitSphere));
                }

                BulletAttack bulletAttack = new();
                bulletAttack.owner = base.gameObject;
                bulletAttack.weapon = base.characterBody.aimOriginTransform.gameObject;
                bulletAttack.origin = lrRay.origin;
                bulletAttack.aimVector = lrRay.direction;
                bulletAttack.minSpread = 0f;
                bulletAttack.maxDistance = 70f;
                bulletAttack.maxSpread = 0f;
                bulletAttack.damage = base.damageStat * 4f;
                bulletAttack.damageType = DamageType.NonLethal;
                bulletAttack.force = 4000f;
                bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
                bulletAttack.radius = 1f;
                bulletAttack.smartCollision = true;
                bulletAttack.tracerEffectPrefab = ArbiterBoss.FairyTracerEffect;
                bulletAttack.Fire();

                fired = true;
            }

            if (base.fixedAge >= 1.2f) {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            GetModelAnimator().SetBool("shouldAimPitch", false);
        }
    }
}