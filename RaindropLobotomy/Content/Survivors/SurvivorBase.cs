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
using KinematicCharacterController;

namespace RaindropLobotomy.Survivors
{
    public abstract class SurvivorBase<T> : SurvivorBase where T : SurvivorBase<T>
    {
        public static T Instance { get; private set; }

        public SurvivorBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class SurvivorBase
    {
        public abstract SurvivorDef Survivor { get; }
        public abstract GameObject BodyPrefab { get; }
        public abstract GameObject MasterPrefab { get; }
        
        public virtual void Create()
        {
            if (BodyPrefab) ContentAddition.AddBody(BodyPrefab);
            if (MasterPrefab) ContentAddition.AddMaster(MasterPrefab);
            if (Survivor) ContentAddition.AddSurvivorDef(Survivor);

            if (BodyPrefab) {
                KinematicCharacterMotor motor = BodyPrefab.GetComponent<KinematicCharacterMotor>();
                if (motor) {
                    motor.playerCharacter = true;
                }
            }

            Modify();
            SetupLanguage();
        }

        public virtual void Modify()
        {
        }

        public virtual void SetupLanguage()
        {
        }
    }
}