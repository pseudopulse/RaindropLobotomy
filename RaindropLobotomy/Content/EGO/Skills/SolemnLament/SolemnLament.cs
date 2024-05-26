using RoR2;
using R2API;

namespace RaindropLobotomy.EGO {
    public class SolemnLament : EGOSkillBase
    {
        public override SkillDef SkillDef => Load<SteppedSkillDef>("sdSolemnLament.asset");
        public override SurvivorDef Survivor => Assets.SurvivorDef.Commando;

        public override SkillSlot Slot => SkillSlot.Primary;
        public override UnlockableDef Unlock => null;
        public GameObject lament;
        public Material matLamentWhite;
        public Material matLamentBlack;
        private Vector3 gunScale = new(4f, 4f, 4f);
        public static BuffDef SealStack;
        public static BuffDef Seal;
        public static DamageAPI.ModdedDamageType LamentType = DamageAPI.ReserveDamageType();

        public override void OnSkillChangeUpdate(CharacterModel model, bool equipped)
        {
            RendererStore store = GetRendererStore(model);
            if (!equipped) {
                Debug.Log("reloading from store");
                store.noUpdateIndices = new int[0];
                ReloadFromStore(model.baseRendererInfos[0], store[0]);
                ReloadFromStore(model.baseRendererInfos[1], store[1]);
            }
            else {
                Debug.Log("loading gun replacements");
                store.noUpdateIndices = new int[] { 0, 1 };
                model.baseRendererInfos[0].defaultMaterial = matLamentBlack;
                model.baseRendererInfos[0].renderer.material = matLamentBlack;
                model.baseRendererInfos[0].renderer.GetComponent<MeshFilter>().mesh = lament.GetComponent<MeshFilter>().mesh;
                model.baseRendererInfos[0].renderer.transform.localScale = gunScale;
                model.baseRendererInfos[0].renderer.transform.localRotation = Quaternion.Euler(0f, -270f, 180f);

                model.baseRendererInfos[1].defaultMaterial = matLamentWhite;
                model.baseRendererInfos[1].renderer.material = matLamentWhite;
                model.baseRendererInfos[1].renderer.GetComponent<MeshFilter>().mesh = lament.GetComponent<MeshFilter>().mesh;
                model.baseRendererInfos[1].renderer.transform.localScale = gunScale;
                model.baseRendererInfos[1].renderer.transform.localRotation = Quaternion.Euler(0f, 270f, 180f);
            }
        }

        public override void CreateLanguage()
        {
            lament = Load<GameObject>("SolemnLament.prefab");
            matLamentBlack = Load<Material>("matLamentBlack.mat");
            matLamentWhite = Load<Material>("matLamentWhite.mat");
            Seal = Load<BuffDef>("bdSealed.asset");
            SealStack = Load<BuffDef>("bdSealStack.asset");

            ContentAddition.AddEffect(Load<GameObject>("LamentImpactWhite.prefab"));
            ContentAddition.AddEffect(Load<GameObject>("LamentImpactBlack.prefab"));
            ContentAddition.AddBuffDef(Seal);
            ContentAddition.AddBuffDef(SealStack);
            
            LanguageAPI.Add("RL_KEYWORD_LAMENT", "Lament");
            LanguageAPI.Add("RL_SOLEMNLAMENT_NAME", "Solemn Lament");
            LanguageAPI.Add("RL_SOLEMNLAMENT_DESC", "Fire slow, alternating shots for <style=cIsDamage>145% damage</style> that <style=cIsUtility>seal</style> targets. Deals additional damage against <style=cIsUtility>fully sealed</style> targets.");

            GlobalEventManager.onServerDamageDealt += HandleSeal;
        }

        private void HandleSeal(DamageReport report)
        {
            if (!report.victimBody) return;

            bool isAlreadySealed = report.victimBody.HasBuff(Seal);
            if (isAlreadySealed) return;

            if (report.damageInfo.HasModdedDamageType(LamentType)) {
                report.victimBody.AddTimedBuff(SealStack, 10f);

                if (report.victimBody.GetBuffCount(SealStack) >= 5) {
                    report.victimBody.SetBuffCount(SealStack.buffIndex, 0);
                    report.victimBody.AddTimedBuff(Seal, 3f);
                    EffectManager.SimpleEffect(Assets.GameObject.OmniImpactExecute, report.damageInfo.position, Quaternion.identity, false);
                    
                    SetStateOnHurt hurt = report.victimBody.GetComponent<SetStateOnHurt>();
                    if (hurt) {
                        hurt.SetStun(3f);
                    }
                }
            }
        }

        public override void SetupRendererStore(RendererStore store, CharacterModel model)
        {
            FillStore(store, model);
        }
    }

    public class LamentState : BaseSkillState, SteppedSkillDef.IStepSetter
    {
        private float DamageCoefficient = 1.4f;
        private float DamageCoefficientSealedTarget = 5.7f;
        private float ShotsPerSecond = 1.5f;
        private GameObject ImpactWhite => Load<GameObject>("LamentImpactWhite.prefab");
        private GameObject ImpactBlack => Load<GameObject>("LamentImpactBlack.prefab");
        private float Duration => 1f / ShotsPerSecond;
        //
        private int pistol;
        private float duration;
        public void SetStep(int i)
        {
            pistol = i;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            if (pistol % 2 == 0)
            {
                PlayAnimation("Gesture Additive, Left", "FirePistol, Left");
                FireBullet("MuzzleLeft");
                AkSoundEngine.PostEvent("Play_butterflyShot_black", base.gameObject);
            }
            else
            {
                PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
                FireBullet("MuzzleRight");
                AkSoundEngine.PostEvent("Play_butterflyShot_white", base.gameObject);
            }

            duration = Duration / base.attackSpeedStat;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterBody.SetAimTimer(0.2f);

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public void FireBullet(string muzzle) {
            EffectManager.SimpleMuzzleFlash(Assets.GameObject.MuzzleflashBarrage, base.gameObject, muzzle, false);

            if (isAuthority) {
                BulletAttack bulletAttack = new();
                bulletAttack.owner = base.gameObject;
                bulletAttack.weapon = base.gameObject;
                bulletAttack.origin = base.inputBank.aimOrigin;
                bulletAttack.aimVector = base.inputBank.aimDirection;
                bulletAttack.minSpread = 0f;
                bulletAttack.maxSpread = base.characterBody.spreadBloomAngle;
                bulletAttack.damage = base.damageStat;
                bulletAttack.force = 40f;
                bulletAttack.tracerEffectPrefab = Assets.GameObject.TracerBandit2Rifle;
                bulletAttack.muzzleName = muzzle;
                bulletAttack.hitEffectPrefab = muzzle == "MuzzleLeft" ? ImpactBlack : ImpactWhite;
                bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
                bulletAttack.radius = 0.1f;
                bulletAttack.smartCollision = true;
                bulletAttack.AddModdedDamageType(SolemnLament.LamentType);
                bulletAttack.hitCallback = (BulletAttack attack, ref BulletAttack.BulletHit hit) => {
                    float coefficient = DamageCoefficient;

                    if (hit.hitHurtBox && hit.hitHurtBox.healthComponent) {
                        CharacterBody body = hit.hitHurtBox.healthComponent.body;

                        if (body.HasBuff(SolemnLament.Seal)) {
                            coefficient = DamageCoefficientSealedTarget;
                            attack.damageColorIndex = DamageColorIndex.Void;
                        }
                    }

                    attack.damage *= coefficient;
                    return BulletAttack.defaultHitCallback(attack, ref hit);
                };
                bulletAttack.Fire();
            }
        }
    }
}