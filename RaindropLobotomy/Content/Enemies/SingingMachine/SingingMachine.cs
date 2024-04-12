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

namespace RaindropLobotomy.Enemies.SingingMachine
{
    public class SingingMachine : EnemyBase<SingingMachine>
    {
        public static BuffDef BewitchedDebuff;
        public override void LoadPrefabs()
        {
            prefab = Load<GameObject>("SingingMachineBody.prefab");
            prefabMaster = Load<GameObject>("SingingMachineMaster.prefab");

            BewitchedDebuff = Load<BuffDef>("SM_BewitchedBuff.asset");

            RegisterEnemy(prefab, prefabMaster);

            LanguageAPI.Add("RL_SINGINGMACHINE_NAME", "Singing Machine");
            LanguageAPI.Add("RL_SINGINGMACHINE_LORE", "");
        }
    }
}