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

namespace RaindropLobotomy.Enemies.Fragment
{
    public class UniverseFragment : EnemyBase<UniverseFragment>
    {
        public static BuffDef BewitchedDebuff;
        public override void LoadPrefabs()
        {
            prefab = Load<GameObject>("FragmentBody.prefab");
            // prefabMaster = Load<GameObject>("SingingMachineMaster.prefab");

            RegisterEnemy(prefab, prefabMaster);
        }
    }
}