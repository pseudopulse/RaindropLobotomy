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
using RaindropLobotomy.Buffs;

namespace RaindropLobotomy.Enemies.ArbiterBoss {
    public class CastFairy : BaseSkillState {
        public static string MuzzleName = "MuzzleHand";
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
            PlayAnimation("Gesture, Override", "CastFairy", "CastFairy.playbackRate", 0.7f);

            // GetModelAnimator().SetBool("shouldAimPitch", true);

            Transform muzzle = FindModelChild("MuzzleSlash");

            Debug.Log(muzzle.transform.position);

            /*Vector3 pos = base.transform.position + (Vector3.up + (base.characterDirection.forward * 0.4f));
            Quaternion forward = Quaternion.LookRotation(base.characterDirection.forward) * Quaternion.AngleAxis(180, Vector3.up);
            Debug.Log("pos: " + pos);*/

            GameObject swing = GameObject.Instantiate(ArbiterBoss.ArbiterSlashEffect, muzzle);
            Debug.Log(swing.transform.position);
            swing.transform.forward *= -1f;
            swing.GetComponent<ScaleParticleSystemDuration>().newDuration = 3f;
            Debug.Log(swing.transform.position);

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

            if (base.fixedAge >= 0.5f && !fired) {
                Ray lrRay = base.GetAimRay();

                AkSoundEngine.PostEvent(Events.Play_MULT_m1_snipe_shoot, base.gameObject);

                for (float i = 3f; i < (Vector3.Distance(lrRay.origin, lrRay.GetPoint(70))); i += 5f) {
                    Vector3 pos = lrRay.origin + (lrRay.direction * i);
                    GameObject.Instantiate(ArbiterBoss.FairyTracerSlashEffect, pos, Quaternion.LookRotation(Random.onUnitSphere));
                }

                BulletAttack bulletAttack = new();
                bulletAttack.owner = base.gameObject;
                bulletAttack.weapon = base.gameObject;
                bulletAttack.origin = lrRay.origin;
                bulletAttack.aimVector = lrRay.direction;
                bulletAttack.minSpread = 0f;
                bulletAttack.maxDistance = 50f;
                bulletAttack.maxSpread = 0f;
                bulletAttack.damage = base.damageStat * 4f;
                bulletAttack.AddModdedDamageType(Fairy.FairyOnHit);
                bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
                bulletAttack.radius = 0.4f;
                bulletAttack.smartCollision = false;
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
        }
    }
}