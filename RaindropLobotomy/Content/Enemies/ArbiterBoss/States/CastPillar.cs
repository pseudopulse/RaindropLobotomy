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
        public static string MuzzleName = "MuzzleL";
        public static float DamageCoefficient = 8f;
        //
        private Timer spawnPillar = new(1.4f, expires: true);
        private Vector3 forward;

        public override void OnEnter()
        {
            base.OnEnter();
    
            Debug.Log("playing fairy anim");
            PlayAnimation("Gesture, Override", "CastFairy");

            GetModelAnimator().SetBool("doAltAimYaw", true);

            forward = base.characterDirection.forward;
        }

        public override void OnExit()
        {
            base.OnExit();
            GetModelAnimator().SetBool("doAltAimYaw", false);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.characterMotor.moveDirection = Vector3.zero;

            if (!spawnPillar.expired) {
                base.characterDirection.forward = forward;
            }

            if (spawnPillar.Tick()) {
                GameObject.Instantiate(ArbiterBoss.FairyMuzzleFlash, FindModelChild("MuzzleL"));

                FireProjectileInfo info = new();

                Transform point = FindModelChild("MuzzleL");
                
                info.projectilePrefab = ArbiterBoss.PillarProjectile;
                info.damage = base.damageStat * 7f;
                info.crit = base.RollCrit();
                info.owner = base.gameObject;
                info.position = point.transform.position + (point.transform.forward * 0.8f);
                info.rotation = Util.QuaternionSafeLookRotation(point.transform.forward);

                ProjectileManager.instance.FireProjectile(info);
            }

            if (base.fixedAge >= 4.5f) {
                outer.SetNextStateToMain();
            }
        }
    }
}