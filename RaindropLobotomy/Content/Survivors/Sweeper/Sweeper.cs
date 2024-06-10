using System;
using System.Linq;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2.CharacterAI;
using ThreeEyedGames;

#pragma warning disable

namespace RaindropLobotomy.Survivors.Sweeper {
    public class Sweeper : SurvivorBase
    {
        public override SurvivorDef Survivor => Load<SurvivorDef>("sdSweeper.asset");

        public override GameObject BodyPrefab => Survivor.bodyPrefab;

        public override GameObject MasterPrefab => null;
        public static DamageAPI.ModdedDamageType SmallLifesteal = DamageAPI.ReserveDamageType();
        public static DamageAPI.ModdedDamageType BigLifesteal = DamageAPI.ReserveDamageType();
        public static GameObject AcidSprayEffect;
        public static GameObject SwingEffectL;
        public static GameObject SwingEffectR;
        public static LazyIndex SweeperIndex = new("SweeperBody");
        public static LazyIndex SweeperAIIndex = new("SweeperAIBody");
        public static GameObject AcidProjectile;
        public static GameObject AcidGlob;
        private static GameObject AcidGlobGhost;
        public static SweeperConfig Config = new();

        public class SweeperConfig : ConfigClass
        {
            public override string Section => "Survivors :: A Sweeper";
            public bool DoRecoil => Option<bool>("Utility Recoil", "Should Scatter Fuel have backwards recoil?", true);
        }


        public static string[] numerics => new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};

        // TODO:
        // css animation
        // skill icons
        // lore

        public static string GenerateRandomName() {
            int length = Random.Range(8, 16);
            string str = "";

            for (int i = 0; i < length; i++) {
                str += numerics.GetRandom();
            }

            return str;
        }

        public override void Create()
        {
            base.Create();
            BodyPrefab.GetComponent<CameraTargetParams>().cameraParams = Assets.CharacterCameraParams.ccpStandard;
            BodyPrefab.AddComponent<SweeperAllyBehaviour>();
            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Assets.GameObject.GenericFootstepDust;
        }

        public override void SetupLanguage()
        {
            base.SetupLanguage();

            LanguageAPI.Add("RL_SWEEPER_NAME", "A Sweeper");
            LanguageAPI.Add("RL_SWEEPER_SUB", "A Sweeper");

            LanguageAPI.Add("RL_SWEEPER_PRIM_NAME", "Corpse Cleanup");
            LanguageAPI.Add("RL_SWEEPER_PRIM_DESC", "<style=cIsUtility>Melting.</style> Slash horizontally, dealing <style=cIsDamage>160% damage</style>. Heal for <style=cIsHealing>20%</style> of damage dealt.");

            LanguageAPI.Add("RL_SWEEPER_SEC_NAME", "Trash Disposal");
            LanguageAPI.Add("RL_SWEEPER_SEC_DESC", "Slam downward, dealing <style=cIsDamage>440% damage</style> and spraying <style=cDeath>heated fuel</style> for <style=cIsDamage>2x140% damage</style>.");

            LanguageAPI.Add("RL_SWEEPER_UTIL_NAME", "Scatter Fuel");
            LanguageAPI.Add("RL_SWEEPER_UTIL_DESC", "<style=cIsDamage>Ignite.</style> Scatter a large burst of <style=cDeath>heated fuel</style> from your tank for <style=cIsDamage>6x80% damage</style>.");

            LanguageAPI.Add("RL_SWEEPER_SPEC_NAME", "333... 1973..");
            LanguageAPI.Add("RL_SWEEPER_SPEC_DESC", "Rally nearby Sweepers, causing them to <style=cIsUtility>target enemies you strike</style>. Give 1 <style=cIsDamage>Persistence</style> to all nearby Sweepers and yourself");

            LanguageAPI.Add("RL_SWEEPER_PASSIVE_NAME", "4126.. 6826");
            LanguageAPI.Add("RL_SWEEPER_PASSIVE_DESC", "Allied <style=cIsUtility>Sweepers</style> will periodically spawn. Up to <style=cIsDamage>4</style> ally Sweepers may exist at once. Sweepers share your items.");

            LanguageAPI.Add("RL_KEYWORD_MELTING", 
                "<style=cKeywordName>Melting</style>On kill, melt the target into a molten puddle dealing 300% damage per second over 5 seconds."
            );

            LanguageAPI.Add("RL_KEYWORD_PERSISTANCE",
                "<style=cKeywordName>Persistence</style>Gain 10% attack speed, 10% movement speed, and 20 armor per stack of Persistence you have. Stacks of Persistence will be consumed to negate damage that would take you below 25% health."
            );


            AcidSprayEffect = PrefabAPI.InstantiateClone(Assets.GameObject.AcidLarvaLeapExplosion, "SweeperAcidImpact");
            AcidSprayEffect.FindParticle("Billboard, Directional").material = Assets.Material.matBloodHumanLarge;
            AcidSprayEffect.FindParticle("Billboard, Big Splash").gameObject.SetActive(false);
            AcidSprayEffect.FindParticle("Billboard, Splash").material = Assets.Material.matBloodSiphon;
            AcidSprayEffect.FindParticle("Billboard, Dots").material = Assets.Material.matBloodGeneric;
            AcidSprayEffect.FindComponent<Light>("Point light").color = new Color32(207, 59, 46, 255);
            ContentAddition.AddEffect(AcidSprayEffect);

            SwingEffectL = Load<GameObject>("SweeperSlashLeft.prefab");
            SwingEffectR = Load<GameObject>("SweeperSlashRight.prefab");

            On.RoR2.CharacterBody.GetDisplayName += OverrideAllyNames;
            On.RoR2.GlobalEventManager.OnHitEnemy += Heal;
            On.RoR2.GlobalEventManager.OnCharacterDeath += OnSweeperKill;

            AcidProjectile = PrefabAPI.InstantiateClone(Assets.GameObject.BeetleQueenAcid, "SweeperMoltenCorpse");
            Material newMat = Object.Instantiate(Assets.Material.matBeetleQueenAcidDecal);
            newMat.SetColor("_Color", new Color32(203, 7, 0, 255));
            newMat.SetVector("_CutoffScroll", new(5, 5, -5, -5));
            newMat.SetFloat("_AlphaBoost", 0.44f);
            newMat.SetTexture("_RemapTex", Assets.Texture2D.texRampTeslaCoil);

            AcidProjectile.FindParticle("Gas").gameObject.SetActive(false);
            AcidProjectile.FindComponent<Light>("Point Light").color = new Color32(255, 0, 7, 255);
            AcidProjectile.FindParticle("Spittle").material = Assets.Material.matBloodSiphon;
            AcidProjectile.FindComponent<MeshRenderer>("Decal").material = newMat;
            AcidProjectile.FindComponent<Decal>("Decal").Material = newMat;

            AcidProjectile.GetComponent<ProjectileDotZone>().lifetime = 5f;
            AcidProjectile.GetComponent<ProjectileDotZone>().damageCoefficient = 1f;

            ContentAddition.AddProjectile(AcidProjectile);


            AcidGlob = PrefabAPI.InstantiateClone(Assets.GameObject.CrocoSpit, "AcidGlobProjectile");
            AcidGlobGhost = PrefabAPI.InstantiateClone(Assets.GameObject.CrocoSpitGhost, "AcidGlobGhost");

            AcidGlob.GetComponent<ProjectileController>().ghostPrefab = AcidGlobGhost;
            AcidGlob.GetComponent<Rigidbody>().useGravity = true;

            AcidGlob.GetComponent<ProjectileDamage>().damageType = DamageType.IgniteOnHit;

            AcidGlob.GetComponent<ProjectileImpactExplosion>().explosionEffect = Assets.GameObject.BleedOnHitAndExplodeExplosion;
            AcidGlob.GetComponent<ProjectileImpactExplosion>().impactEffect = null;

            ContentAddition.AddProjectile(AcidGlob);

            AcidGlobGhost.FindParticle("Flashes").material = Assets.Material.matMageFirebolt;
            AcidGlobGhost.FindParticle("Goo, WS").material = Load<Material>("matSweeperSlash.mat");
            AcidGlobGhost.FindParticle("Goo Drippings").material = Assets.Material.matBloodSiphon;
            AcidGlobGhost.FindComponent<TrailRenderer>("Trail").material = Load<Material>("matSweeperSlash.mat");

            NetworkingAPI.RegisterMessageType<SyncNameOverride>();
        }

        private void OnSweeperKill(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);

            if (damageReport.attackerBody && IsSweeper(damageReport.attackerBody)) {
                FireProjectileInfo info = new();
                info.owner = damageReport.attacker;
                info.damage = damageReport.attackerBody.damage * 3f;
                info.crit = false;
                info.position = damageReport.victimBody.footPosition;
                info.projectilePrefab = AcidProjectile;
                info.rotation = Quaternion.LookRotation(Vector3.up);

                if (damageReport.victimBody && damageReport.victimBody.modelLocator && damageReport.victimBody.modelLocator.modelTransform) {
                    GameObject.Destroy(damageReport.victimBody.modelLocator.modelTransform);
                }

                ProjectileManager.instance.FireProjectile(info);
            }
        }

        public class SyncNameOverride : INetMessage
        {
            public GameObject applyTo;
            private NetworkInstanceId _applyTo;
            public string nameReplacement;
            public void Deserialize(NetworkReader reader)
            {
                _applyTo = reader.ReadNetworkId();
                nameReplacement = reader.ReadString();

                applyTo = Util.FindNetworkObject(_applyTo);
            }

            public void OnReceived()
            {
                // Debug.Log("name override received: " + nameReplacement);
                // applyTo.AddComponent<AllyNameOverride>().OverrideName = nameReplacement;
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(applyTo.GetComponent<NetworkIdentity>().netId);
                writer.Write(nameReplacement);
            }

            public SyncNameOverride() {

            }

            public SyncNameOverride(GameObject applyTo, string str) {
                this.applyTo = applyTo;
                this.nameReplacement = str;
            }
        }

        private bool IsSweeper(CharacterBody body) {
            return body.bodyIndex == SweeperIndex || body.bodyIndex == SweeperAIIndex;
        }

        private string OverrideAllyNames(On.RoR2.CharacterBody.orig_GetDisplayName orig, CharacterBody self)
        {
            if (self.GetComponent<AllyNameOverride>()) {
                return self.GetComponent<AllyNameOverride>().OverrideName;
            }

            return orig(self);
        }

        private void Heal(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            if (damageInfo.HasModdedDamageType(SmallLifesteal) && damageInfo.attacker) {
                HealthComponent attackerHealth = damageInfo.attacker.GetComponent<HealthComponent>();

                if (attackerHealth) attackerHealth.Heal(damageInfo.damage * 0.05f, new(), true);
            }

            if (damageInfo.HasModdedDamageType(BigLifesteal) && damageInfo.attacker) {
                HealthComponent attackerHealth = damageInfo.attacker.GetComponent<HealthComponent>();

                if (attackerHealth) attackerHealth.Heal(damageInfo.damage * 0.15f, new(), true);
            }
        }

        public static List<CharacterBody> GetSweepersInRange(float range, Vector3 pivot) {
            return CharacterBody.readOnlyInstancesList.Where(x => Vector3.Distance(pivot, x.corePosition) <= range 
            && (x.bodyIndex == SweeperAIIndex || x.bodyIndex == SweeperIndex)).ToList();
        }

        public static List<CharacterMaster> GetSweepersOwnedByPlayer(CharacterMaster owner) {
            return CharacterMaster.readOnlyInstancesList.Where(x => x.minionOwnership.ownerMaster == owner 
            && x.GetBody()
            && x.GetBody().bodyIndex == SweeperAIIndex).ToList();
        }

        public static void ForceSweeperTargets(CharacterMaster owner, CharacterBody target) {
            List<CharacterMaster> allies = GetSweepersOwnedByPlayer(owner);

            foreach (CharacterMaster master in allies) {
                BaseAI ai = master.GetComponent<BaseAI>();
                ai.currentEnemy.gameObject = target.gameObject;
            }
        }


        public class SweeperAllyBehaviour : MonoBehaviour {
            public Timer timer = new(60f, false, true, false, true);
            public CharacterBody self;
            public CharacterMaster master;
            public void Start() {
                timer.cur = 50f;
                self = GetComponent<CharacterBody>();
                master = self.master;
            }

            public void FixedUpdate() {
                if (!NetworkServer.active) return;

                if (timer.Tick()) {
                    Vector3[] positions = MiscUtils.GetSafePositionsWithinDistance(base.transform.position, 45f);

                    if (positions.Length == 1 || !IsSpawnAllowed()) {
                        return;
                    }

                    Vector3 pos = positions.GetRandom(Run.instance.spawnRng);

                    MasterSummon summon = new();
                    summon.ignoreTeamMemberLimit = true;
                    summon.masterPrefab = Ordeals.Noon.Indigo.Sweeper.cscSweeper.prefab;
                    summon.position = pos;
                    summon.rotation = Quaternion.identity;
                    summon.summonerBodyObject = base.gameObject;
                    summon.useAmbientLevel = false;
                    summon.teamIndexOverride = TeamIndex.Player;
                    summon.preSpawnSetupCallback = (master) => {
                        master.onBodyStart += (body) => {
                            AllyNameOverride nameOverride = body.AddComponent<AllyNameOverride>();
                            nameOverride.OverrideName = GenerateRandomName();
                            Debug.Log("syncing name override");
                            new SyncNameOverride(body.gameObject, nameOverride.OverrideName).Send(NetworkDestination.Clients);
                        };
                    };
                    summon.inventoryToCopy = master.inventory;
                    summon.Perform();
                }
            }

            public bool IsSpawnAllowed() {
                int totalSweepersWeOwn = 0;

                foreach (CharacterMaster cMaster in CharacterMaster.readOnlyInstancesList) {
                    MinionOwnership ownership = cMaster.minionOwnership;

                    if (ownership && ownership.ownerMaster == master && cMaster.bodyPrefab == Ordeals.Noon.Indigo.Sweeper.BodyPrefab) {
                        totalSweepersWeOwn++;
                    }
                }

                return totalSweepersWeOwn < 4;
            }
        }

        public class AllyNameOverride : MonoBehaviour {
            public string OverrideName;
        }
    }
}