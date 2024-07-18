using System;
using System.Linq;
using Newtonsoft.Json.Utilities;
using RaindropLobotomy.EGO.Commando;

namespace RaindropLobotomy.Buffs {
    public class Sealed : BuffBase<Sealed>
    {
        public override BuffDef Buff => Load<BuffDef>("bdLamentSeal.asset");

        public override void PostCreation()
        {
            On.RoR2.GenericSkill.IsReady += Seal;
            RecalculateStatsAPI.GetStatCoefficients += HandleStun;
            On.RoR2.GlobalEventManager.OnHitEnemy += OnHitEnemy;
        }

        private void OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            if (damageInfo.HasModdedDamageType(SolemnLament.Seal) && victim && victim.GetComponent<CharacterBody>().GetBuffCount(Buff) < 4) {
                victim.GetComponent<CharacterBody>().AddTimedBuff(Buff, victim.GetComponent<CharacterBody>().isChampion ? 7.5f : 15f, 4);
            }
        }

        private void HandleStun(CharacterBody sender, StatHookEventArgs args)
        {
            int count = sender.GetBuffCount(Buff);

            if (count >= 4) {
                SetStateOnHurt hurt = sender.GetComponent<SetStateOnHurt>();

                if (hurt && hurt.canBeStunned) {
                    hurt.SetStun(10f);
                }
            }
        }

        private bool Seal(On.RoR2.GenericSkill.orig_IsReady orig, GenericSkill self)
        {
            int buffCount = self.characterBody.GetBuffCount(Buff.buffIndex);

            if (buffCount > 0) {
                SkillLocator locator = self.characterBody.skillLocator;

                if (Sealable(buffCount, self, new GenericSkill[] {
                    locator.primary, locator.secondary, locator.utility, locator.special
                })) {
                    return false;
                }
            }

            return orig(self);
        }

        private bool Sealable(int count, GenericSkill skill, GenericSkill[] slots) {
            slots = slots.Where(x => x != null).ToArray();

            if (skill == null) return false;

            int total = slots.Length;
            int index = Array.IndexOf(slots, skill);

            return index <= count;
        }
    }
}