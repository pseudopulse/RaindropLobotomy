using System;
using R2API.Networking;
using R2API.Networking.Interfaces;
using R2API.Utils;
using UnityEngine.TextCore;

namespace RaindropLobotomy.EGO.Merc {
    public class BLMerc : CorrosionBase<BLMerc>
    {
        public override string EGODisplayName => "Blade Lineage Mercenary";

        public override string Description => "We are not placing our stone here, then? Mm, then the tides drive us to resign.";

        public override SurvivorDef TargetSurvivorDef => Paths.SurvivorDef.Merc;

        public override UnlockableDef RequiredUnlock => null;

        public override Color Color => Color.blue;

        public override SurvivorDef Survivor => Load<SurvivorDef>("sdBLMerc.asset");

        public override GameObject BodyPrefab => Load<GameObject>("BLMercBody.prefab");

        public override GameObject MasterPrefab => null;
        //
        public static GameObject SlashEffect;
        public static GameObject Upslash;
        public static GameObject Radial;
        public static GameObject Spear;
        public static GameObject DarkSlash;
        public static GameObject DarkRadial;
        public static GameObject DarkRadial2;
        public static GameObject WarpTrail;
        public static GameObject DarkFlare;
        //
        private static Dictionary<CharacterBody, SwordplayState> SwordplayMap = new();
        private static Dictionary<CharacterBody, bool> YieldingMap = new();
        //
        public static SkillDef ToClaimTheirBones;
        public static DamageAPI.ModdedDamageType PoiseDamageBonus = DamageAPI.ReserveDamageType();

        public override void Modify()
        {
            base.Modify();

            BodyPrefab.GetComponent<CameraTargetParams>().cameraParams = Paths.CharacterCameraParams.ccpStandardMelee;
            
            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<Animator>().runtimeAnimatorController = Paths.RuntimeAnimatorController.animMerc;
            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet = Paths.ItemDisplayRuleSet.idrsMerc;
            Load<GameObject>("BLMercDisplay.prefab").GetComponentInChildren<Animator>().runtimeAnimatorController = Paths.RuntimeAnimatorController.animMercDisplay;
            BodyPrefab.GetComponent<CharacterBody>()._defaultCrosshairPrefab = Paths.GameObject.MercBody.GetComponent<CharacterBody>().defaultCrosshairPrefab;
            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Paths.GameObject.GenericFootstepDust;

            SlashEffect = Load<GameObject>("BLMercSlash.prefab");
            Upslash = Load<GameObject>("BLMercUpslash.prefab");
            Radial = Load<GameObject>("BLSlashRadial.prefab");
            Spear = Load<GameObject>("BLMercThrust.prefab");
            DarkSlash = Load<GameObject>("BL_DarkSlashBig.prefab");
            DarkRadial = Load<GameObject>("BL_DarkRadial.prefab");
            DarkRadial2 = Load<GameObject>("BL_DarkRadial_2.prefab");
            WarpTrail = Load<GameObject>("BL_WarpTrail.prefab");
            DarkFlare = Load<GameObject>("BL_BladeShing.prefab");

            ToClaimTheirBones = Load<SkillDef>("BL_ToClaimTheirBones.asset");

            On.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += BoneClaiming;
            On.RoR2.CharacterModel.UpdateOverlays += BoneClaiming2;
            On.RoR2.HealthComponent.TakeDamageProcess += PoiseCritBonus;

            NetworkingAPI.RegisterMessageType<SyncTCTB>();
        }

        private void PoiseCritBonus(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.crit && damageInfo.attacker && damageInfo.attacker.GetComponent<CharacterBody>() && damageInfo.HasModdedDamageType(PoiseDamageBonus)) {
                CharacterBody characterBody = damageInfo.attacker.GetComponent<CharacterBody>();
                int poiseCount = characterBody.GetBuffCount(Buffs.Poise.Instance.Buff);
                poiseCount = Mathf.Clamp(poiseCount, 0, 20);
                damageInfo.damage *= 1f + ((poiseCount / 4f) * 0.1f);
            }

            orig(self, damageInfo);
        }

        private void BoneClaiming2(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
        {
            orig(self);

            AddOverlay(CharacterModel.doppelgangerMaterial, IsBoning(self.body) || (self.body && self.body.inventory && self.body.inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0));

            void AddOverlay(Material overlayMaterial, bool condition)
            {
                if (self.activeOverlayCount < CharacterModel.maxOverlays && condition)
                {
                    self.currentOverlays[self.activeOverlayCount++] = overlayMaterial;
                }
            }
        }

        private void BoneClaiming(On.RoR2.CharacterBody.orig_UpdateAllTemporaryVisualEffects orig, CharacterBody self)
        {
            orig(self);

            self.UpdateSingleTemporaryVisualEffect(ref self.doppelgangerEffectInstance, CharacterBody.AssetReferences.doppelgangerEffectPrefab, self.bestFitRadius, IsBoning(self) || ((self.inventory != null && (self.inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0))), "Head");
        }

        private bool IsBoning(CharacterBody body) {
            if (!body || !YieldingMap.ContainsKey(body)) return false;

            return YieldingMap[body];
        }

        public override void SetupLanguage()
        {
            base.SetupLanguage();

            "RL_BLMERC_NAME".Add("Blade Lineage Mercenary");


            "RL_BLMERC_PASSIVE_NAME".Add("Swordplay of the Homeland");
            "RL_BLMERC_PASSIVE_DESC".Add("Gain <style=cIsUtility>Poise</style> when using unique attacks without consecutive repeats.");

            "RL_BLMERC_PRIMARY_NAME".Add("Draw of the Sword");
            "RL_BLMERC_PRIMARY_DESC".Add("Slash forward for <style=cIsDamage>260% damage</style>. Critical hits inflict <style=cDeath>Bleed</style>.");

            "RL_BLMERC_SECONDARY_NAME".Add("Acupuncture");
            "RL_BLMERC_SECONDARY_DESC".Add("Thrust forward for <style=cIsDamage>400% damage</style>. Deal bonus <style=cIsHealth>critical damage</style> equal to <style=cIsUtility>Poise</style>.");

            "RL_BLMERC_UTILITY_NAME".Add("Overthrow");
            "RL_BLMERC_UTILITY_DESC".Add("Leap up and slam down for <style=cIsDamage>800% damage</style>. Gain <style=cIsUtility>Poise</style> for every target hit.");

            "RL_BLMERC_SPECIAL_NAME".Add("Yield My Flesh");
            "RL_BLMERC_SPECIAL_DESC".Add("Slash for <style=cIsDamage>400% damage</style>. If you take damage during this skill, retaliate with <style=cDeath>To Claim Their Bones</style>.");

            "RL_KEYWORD_POISE".Add(
                """
                <style=cKeywordName>Poise</style> Gain <style=cIsDamage>+5%</style> <style=cIsHealth>critical strike chance</style> for each stack of <style=cIsUtility>Poise</style>. Lose 1 <style=cIsUtility>Poise</style> when dealing a critical hit.
                """
            );

            "RL_KEYWORD_TCTB".Add(
                """
                <style=cKeywordName>To Claim Their Bones</style> Consume all <style=cIsUtility>Poise</style> and convert it into up to <style=cIsDamage>+200%</style> damage for this skill.

                Quickly reposition and slash <style=cIsDamage>8 times</style>, dealing <style=cIsDamage>300% damage</style> each time. Inflict <style=cDeath>Bleed</style> on every hit.
                <style=cIsUtility>Crippling.</style> Slice upwards for <style=cIsDamage>1000%</style> damage, and then slam downwards, dealing <style=cIsDamage>3000% damage</style> on impact.

                You <style=cIsUtility>cannot be hit</style> during these attacks.
                """
            );
        }

        public static void UpdateYieldingState(CharacterBody body, bool newState) {
            if (!YieldingMap.ContainsKey(body)) {
                YieldingMap.Add(body, newState);
            }

            YieldingMap[body] = newState;

            body.RecalculateStats();
        }

        public static void UpdateSwordplayState(CharacterBody body, SwordplayState newState) {
            if (!SwordplayMap.ContainsKey(body)) {
                SwordplayMap.Add(body, newState);
            }


            if (newState != SwordplayMap[body]) {
                if (NetworkServer.active) {
                    int count = body.GetBuffCount(Buffs.Poise.Instance.Buff);
                    body.SetBuffCount(Buffs.Poise.Instance.Buff.buffIndex, Mathf.Clamp(count + 1, 0, 20));
                }

                EffectManager.SimpleEffect(Paths.GameObject.LunarRerollEffect, body.corePosition, Quaternion.identity, false);
                // AkSoundEngine.PostEvent(Events.Play_lunar_reroller_activate, body.gameObject);
            }

            SwordplayMap[body] = newState;
        }
        
    }

    public class SyncTCTB : INetMessage
    {
        public GameObject applyTo;
        private NetworkInstanceId _applyTo;
        public GameObject target;
        private NetworkInstanceId _target;
        private float damage;
        public void Deserialize(NetworkReader reader)
        {
            _applyTo = reader.ReadNetworkId();
            _target = reader.ReadNetworkId();

            applyTo = Util.FindNetworkObject(_applyTo);
            target = Util.FindNetworkObject(_target);

            damage = (float)reader.ReadDouble();
        }

        public void OnReceived()
        {
            EntityStateMachine machine = EntityStateMachine.FindByCustomName(applyTo, "Body");

            if (machine.state is YieldMyFlesh) {
                (machine.state as YieldMyFlesh).ReceiveInfo(target, damage);
            }
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(applyTo.GetComponent<NetworkIdentity>().netId);
            writer.Write(target.GetComponent<NetworkIdentity>().netId);
            writer.Write((double)damage);
        }

        public SyncTCTB() {

        }

        public SyncTCTB(GameObject applyTo, GameObject target, float damage) {
            this.applyTo = applyTo;
            this.target = target;
            this.damage = damage;
        }
    }

    public enum SwordplayState {
        Slash,
        BigSlash,
        Thrust,
        Slam,
        Uppercut
    }
}