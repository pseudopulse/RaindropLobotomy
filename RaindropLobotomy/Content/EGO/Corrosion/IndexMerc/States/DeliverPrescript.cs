using System;

namespace RaindropLobotomy.EGO.Merc {
    public class DeliverPrescript : BaseSkillState {
        public override void OnEnter()
        {
            base.OnEnter();

            base.skillLocator.primary.SetSkillOverride(this, IndexMerc.Attack, GenericSkill.SkillOverridePriority.Contextual);
            base.skillLocator.secondary.SetSkillOverride(this, IndexMerc.Defense, GenericSkill.SkillOverridePriority.Contextual);
            base.skillLocator.utility.SetSkillOverride(this, IndexMerc.Evade, GenericSkill.SkillOverridePriority.Contextual);
            base.skillLocator.special.SetSkillOverride(this, IndexMerc.Energize, GenericSkill.SkillOverridePriority.Contextual);
        }

        public override void Update()
        {
            base.Update();

            if (base.fixedAge <= 0.2f) {
                return;
            }

            if (base.inputBank.skill1.justPressed) {
                FirePrescript(Buffs.PrescriptAttack.Instance.Buff, 7.5f, Load<GameObject>("PrescriptAttackEffect.prefab"));
                return;
            }

            if (base.inputBank.skill2.justPressed) {
                FirePrescript(Buffs.PrescriptDefense.Instance.Buff, 10f, Load<GameObject>("PrescriptDefenseEffect.prefab"));
                return;
            }

            if (base.inputBank.skill3.justPressed) {
                FirePrescript(Buffs.PrescriptEvasion.Instance.Buff, 7.5f, Load<GameObject>("PrescriptEvadeEffect.prefab"));
                return;
            }

            if (base.inputBank.skill4.justPressed) {
                FirePrescript(Buffs.PrescriptEnergized.Instance.Buff, 10f, Load<GameObject>("PrescriptEnergizeEffect.prefab"));
                return;
            }
        }

        public void FirePrescript(BuffDef buff, float time, GameObject effect) {
            IndexMerc.IndexPrescriptTargeter prescriptTargeter = GetComponent<IndexMerc.IndexPrescriptTargeter>();
            CharacterBody buffTarget = base.characterBody;

            if (prescriptTargeter.target) {
                buffTarget = prescriptTargeter.target.GetComponent<HurtBox>().healthComponent.body;
            }

            buffTarget.AddTimedBuff(buff, time, 1);

            GameObject obj = GameObject.Instantiate(effect, buffTarget.corePosition + (Vector3.up * buffTarget.bestFitRadius), Quaternion.identity);
            obj.transform.parent = buffTarget.transform;

            AkSoundEngine.PostEvent(Events.Play_lunar_reroller_activate, buffTarget.gameObject);

            EffectManager.SpawnEffect(Assets.GameObject.ExplosionLunarSun, new EffectData {
                origin = buffTarget.corePosition,
                scale = buffTarget.bestFitRadius * 2f
            }, false);

            outer.SetNextStateToMain();
        }
        public override void OnExit()
        {
            base.OnExit();

            base.skillLocator.primary.UnsetSkillOverride(this, IndexMerc.Attack, GenericSkill.SkillOverridePriority.Contextual);
            base.skillLocator.secondary.UnsetSkillOverride(this, IndexMerc.Defense, GenericSkill.SkillOverridePriority.Contextual);
            base.skillLocator.utility.UnsetSkillOverride(this, IndexMerc.Evade, GenericSkill.SkillOverridePriority.Contextual);
            base.skillLocator.special.UnsetSkillOverride(this, IndexMerc.Energize, GenericSkill.SkillOverridePriority.Contextual);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}