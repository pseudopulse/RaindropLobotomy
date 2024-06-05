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
    public class CastPillar : BaseSkillState {
        public static string MuzzleName = "MuzzleHand";
        public static float DamageCoefficient = 8f;
        //
        private Timer spawnPillar = new(0.8f, expires: true);
        private Vector3 forward;
        private Transform target;

        public override void OnEnter()
        {
            base.OnEnter();
    
            Debug.Log("playing fairy anim");
            
            GetModelAnimator().SetBool("isInShockwave", true);
            PlayAnimation("Gesture, Override", "CastShockwave", "CastShockwave.playbackRate", 3f);

            // GetModelAnimator().SetBool("doAltAimYaw", true);

            forward = base.characterDirection.forward;
        }

        public override void OnExit()
        {
            base.OnExit();
            GetModelAnimator().SetBool("isInShockwave", true);
            GetModelAnimator().SetBool("doAltAimYaw", false);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.characterMotor.moveDirection = Vector3.zero;

            if (!spawnPillar.expired) {
                base.characterDirection.forward = forward;
            }

            if (spawnPillar.Tick()) {
                GameObject.Instantiate(ArbiterBoss.FairyMuzzleFlash, FindModelChild("MuzzleHand"));

                FireProjectileInfo info = new();

                Transform point = FindModelChild("MuzzleHand");

                target = base.characterBody.master.aiComponents[0].currentEnemy.gameObject?.transform;

                if (!target) {
                    outer.SetNextStateToMain();
                    return;
                }
                
                info.projectilePrefab = ArbiterBoss.PillarProjectile;
                info.damage = base.damageStat * 7f;
                info.crit = base.RollCrit();
                info.owner = base.gameObject;
                info.position = point.transform.position + (point.transform.forward * 0.8f);
                info.rotation = Quaternion.identity;
                info.target = target.gameObject;

                ProjectileManager.instance.FireProjectile(info);
            }

            if (base.fixedAge >= 4.5f) {
                outer.SetNextStateToMain();
            }
        }
    }
}