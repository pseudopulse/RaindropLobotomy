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

namespace RaindropLobotomy.Enemies
{
    public abstract class EnemyBase<T> : EnemyBase where T : EnemyBase<T>
    {
        public static T Instance { get; private set; }

        public EnemyBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class EnemyBase
    {
        public DirectorCard card;
        public CharacterSpawnCard isc;
        public GameObject prefab;
        public GameObject prefabMaster;
        public CharacterBody body;
        public CharacterMaster master;
        public abstract string ConfigName { get; }
        public virtual void Create()
        {
            if (!Main.config.Bind<bool>(ConfigName, "Enabled", true, "Should this enemy appear in runs?").Value) {
                return;
            }

            LoadPrefabs();
            body = prefab.GetComponent<CharacterBody>();
            if (prefabMaster) {
                master = prefabMaster.GetComponent<CharacterMaster>();
            }
            Modify();
            AddSpawnCard();
            AddDirectorCard();
            PostCreation();

            if (typeof(Abnormality).IsAssignableFrom(this.GetType())) {
                AbnormalityManager.AddAbnormality(this as Abnormality);
            }
        }

        public abstract void LoadPrefabs();

        public virtual void Modify()
        {
        }

        public virtual void PostCreation()
        {
        }

        public virtual void AddSpawnCard()
        {
            isc = ScriptableObject.CreateInstance<CharacterSpawnCard>();
        }

        public virtual void AddDirectorCard()
        {
            card = new DirectorCard();
            card.spawnCard = isc;
        }

        public void RegisterEnemy(GameObject bodyPrefab, GameObject masterPrefab, List<DirectorAPI.Stage> stages = null, DirectorAPI.MonsterCategory category = DirectorAPI.MonsterCategory.BasicMonsters, bool all = false)
        {
            PrefabAPI.RegisterNetworkPrefab(bodyPrefab);
            if (masterPrefab) {
                PrefabAPI.RegisterNetworkPrefab(masterPrefab);
            }
            ContentAddition.AddBody(bodyPrefab);
            if (masterPrefab) {
                ContentAddition.AddMaster(masterPrefab);
            }
            if (stages != null)
            {
                foreach (DirectorAPI.Stage stage in stages)
                {
                    DirectorAPI.Helpers.AddNewMonsterToStage(card, category, stage);
                }
            }

            if (all)
            {
                DirectorAPI.Helpers.AddNewMonster(card, category);
            }
        }
    }
}