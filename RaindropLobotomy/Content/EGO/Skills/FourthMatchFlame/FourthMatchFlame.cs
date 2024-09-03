/*using RoR2;
using R2API;
using R2API.Utils;
using EntityStates.Huntress;

namespace RaindropLobotomy.EGO {
    public class FourthMatchFlame : EGOSkillBase
    {
        public override SkillDef SkillDef => Load<SkillDef>("sdFMF.asset");
        public override SurvivorDef Survivor => Paths.SurvivorDef.Railgunner;

        public override SkillSlot Slot => SkillSlot.Special;
        public override UnlockableDef Unlock => null;
        private Vector3 gunScale = new(1f, 1f, 1f);
        private GameObject fmf;
        private Material matFMF;
        public static SkillDef CancelFMF;
        public static SkillDef ChargeFMF;
        public static GameObject TracerFMF;
        public static GameObject BeamFMF;

        public override void OnSkillChangeUpdate(CharacterModel model, bool equipped)
        {
            RendererStore store = GetRendererStore(model);
            // Debug.Log("reloading: "+ equipped);
            if (!equipped) {
                // Debug.Log("reloading from store");
                store.noUpdateIndices = new int[0];
                model.baseRendererInfos[4].renderer.gameObject.SetActive(true);

                Transform stock = model.GetComponent<ChildLocator>().FindChild("GunStock");
                Transform fmf = stock.Find("mdlFourthMatchFlame");

                if (fmf) {
                    GameObject.Destroy(fmf.gameObject);
                }
            }
            else {
                // Debug.Log("loading gun replacements");
                store.noUpdateIndices = new int[] { 4 };
                model.baseRendererInfos[4].renderer.gameObject.SetActive(false);

                Transform stock = model.GetComponent<ChildLocator>().FindChild("GunStock");
                Transform fmf = stock.Find("mdlFourthMatchFlame");

                if (!fmf) {
                    GameObject mdl = GameObject.Instantiate(this.fmf, stock);
                    mdl.gameObject.name = "mdlFourthMatchFlame";
                    mdl.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    mdl.transform.localPosition = new(0.05f, -0.7f, 0f);
                    mdl.transform.localScale = new(0.3f, 0.3f, 0.3f);
                }
            }
        }

        public override void CreateLanguage()
        {
            fmf = Load<GameObject>("mdlFourthMatchFlame.prefab");
            matFMF = Load<Material>("matFourthMatchFlame.mat");
            LanguageAPI.Add("RL_EGO_FMF_SKILL_NAME", "Fourth Match Flame");
            LanguageAPI.Add("RL_EGO_FMF_SKILL_DESC", "<style=cIsDamage>Ignite.</style> Charge up a <style=cIsUtility>piercing</style> flame blast for <style=cIsDamage>1000%-4444% damage</style>. <style=cDeath>Take ramping self-damage as you charge it</style>.");
            LanguageAPI.Add("RL_EGO_FMF_SKILL_CANCEL_NAME", "Abort");
            LanguageAPI.Add("RL_EGO_FMF_SKILL_CANCEL_DESC", "Cancel Fourth Match Flame.");

            TracerFMF = Load<GameObject>("TracerMatchFlame.prefab");
            ContentAddition.AddEffect(TracerFMF);

            CancelFMF = Load<SkillDef>("sdFMF_Cancel.asset");
            CancelFMF.icon = Paths.SkillDef.EngiCancelTargetingDummy.icon;
            ChargeFMF = Load<SkillDef>("sdFMF_Fire.asset");

            BeamFMF = Load<GameObject>("MatchFlameCharge.prefab");
        }


        public override void SetupRendererStore(RendererStore store, CharacterModel model)
        {
            FillStore(store, model);
        }
    }

    public class ChargeFMF : BaseSkillState {
        public float DamageCoeffMin = 10f;
        public float DamageCoeffMax = 44.44f;
        public float SelfDamagePerTick = 0.02f;
        private float TicksPerSecond = 5;
        private float TickDelay = 1f / 5;
        private float ChargeTime = 5f;
        private float stopwatch = 0f;
        private GameObject chargePrefab;
        private GameObject laser;
        private LineRenderer beam;
        private float mult => base.fixedAge / 5f;
        private float width => 0.5f * mult;
        private Transform muzzle;

        public override void OnEnter() {
            base.OnEnter();

            AkSoundEngine.PostEvent(Events.Play_item_use_molotov_fire_loop, base.gameObject);

            laser = GameObject.Instantiate(FourthMatchFlame.BeamFMF, base.transform.position, Quaternion.identity);
            beam = laser.GetComponent<LineRenderer>();

            // Debug.Log("beam?: " + beam);

            muzzle = FindModelChild("MuzzleSniper");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            StartAimMode(2f);

            // Debug.Log("beam?: " + beam);
            // Debug.Log("muzzle?: " + muzzle);

            beam.SetPosition(0, muzzle.transform.position);
            beam.SetPosition(1, GetAimRay().GetPoint(40f));
            beam.startWidth = width;
            beam.endWidth = width;

            base.characterBody.isSprinting = false;

            if (base.isAuthority && ((!inputBank.skill1.down && base.fixedAge >= 0.5f) || base.fixedAge >= 5f)) {
                outer.SetNextState(new FireFMF(Util.Remap(base.fixedAge, 0f, 5f, DamageCoeffMin, DamageCoeffMax)));
            }

            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= TickDelay && NetworkServer.active) {
                stopwatch = 0f;

                DamageInfo damage = new();
                damage.damage = base.healthComponent.fullCombinedHealth * (SelfDamagePerTick);
                damage.damageColorIndex = DamageColorIndex.Item;
                damage.position = base.characterBody.corePosition;

                base.healthComponent.TakeDamage(damage);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            AkSoundEngine.PostEvent(Events.Stop_item_use_molotov_fire_loop, base.gameObject);
            Destroy(laser);
        }
    }

    public class ActivateFMF : BaseSkillState {
        public override void OnEnter()
        {
            base.OnEnter();
            skillLocator.primary.SetSkillOverride(base.gameObject, FourthMatchFlame.ChargeFMF, GenericSkill.SkillOverridePriority.Contextual);
            skillLocator.secondary.SetSkillOverride(base.gameObject, Paths.SkillDef.CaptainCancelDummy, GenericSkill.SkillOverridePriority.Contextual);
            skillLocator.special.SetSkillOverride(base.gameObject, FourthMatchFlame.CancelFMF, GenericSkill.SkillOverridePriority.Contextual);
            outer.SetNextStateToMain();
        }
    }

    public class FireFMF : BaseSkillState {
        public float DamageCoefficient;
        public float duration = 1f;

        public FireFMF(float DamageCoefficient) {
            this.DamageCoefficient = DamageCoefficient;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Ray aimRay = GetAimRay();

            // PlayAnimation(heavy.animationLayerName, heavy.animationStateName, heavy.animationPlaybackRateParam, 1f);

            BulletAttack attack = new();
            attack.damage = base.damageStat * DamageCoefficient;
            attack.radius = 2f;
            attack.maxDistance = 40f;
            attack.sniper = true;
            attack.origin = aimRay.origin;
            attack.aimVector = aimRay.direction;
            attack.muzzleName = "MuzzleSniper";
            attack.procCoefficient = 1f;
            attack.hitEffectPrefab = Paths.GameObject.ImpactWispEmber;
            attack.damageType |= DamageType.IgniteOnHit;
            attack.owner = base.gameObject;
            attack.stopperMask = LayerIndex.noCollision.mask;
            attack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
            attack.tracerEffectPrefab = FourthMatchFlame.TracerFMF;

            attack.Fire();

            AkSoundEngine.PostEvent(Events.Play_wisp_attack_fire, base.gameObject);

            EffectManager.SimpleMuzzleFlash(Paths.GameObject.MuzzleflashFireMeatBall, base.gameObject, "MuzzleSniper", false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            skillLocator.primary.UnsetSkillOverride(base.gameObject, FourthMatchFlame.ChargeFMF, GenericSkill.SkillOverridePriority.Contextual);
            skillLocator.secondary.UnsetSkillOverride(base.gameObject, Paths.SkillDef.CaptainCancelDummy, GenericSkill.SkillOverridePriority.Contextual);
            skillLocator.special.UnsetSkillOverride(base.gameObject, FourthMatchFlame.CancelFMF, GenericSkill.SkillOverridePriority.Contextual);
        }
    }

    public class CancelFMF : BaseSkillState {
        public override void OnEnter()
        {
            base.OnEnter();
            skillLocator.primary.UnsetSkillOverride(base.gameObject, FourthMatchFlame.ChargeFMF, GenericSkill.SkillOverridePriority.Contextual);
            skillLocator.secondary.UnsetSkillOverride(base.gameObject, Paths.SkillDef.CaptainCancelDummy, GenericSkill.SkillOverridePriority.Contextual);
            skillLocator.special.UnsetSkillOverride(base.gameObject, FourthMatchFlame.CancelFMF, GenericSkill.SkillOverridePriority.Contextual);

            outer.SetNextStateToMain();
        }
    }
}*/