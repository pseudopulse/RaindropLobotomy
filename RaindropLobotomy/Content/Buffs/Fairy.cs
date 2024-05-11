using System;

namespace RaindropLobotomy.Buffs {
    public class Fairy : BuffBase<Fairy>
    {
        public override BuffDef Buff => Load<BuffDef>("bdFairy.asset");
        public static float DamageCoeffToDetonate = 5f;
        public static DamageAPI.ModdedDamageType FairyOnHit = DamageAPI.ReserveDamageType();
        public static DamageAPI.ModdedDamageType TripleFairyOnHit = DamageAPI.ReserveDamageType();

        public override void PostCreation()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += UpdateFairy;
        }

        private void UpdateFairy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo info, GameObject victim)
        {
            if (info.attacker) {
                CharacterBody body = info.attacker.GetComponent<CharacterBody>();

                if (body && body.HasBuff(Buff) && info.procChainMask.mask == 0) {
                    FairyDamageController controller = body.GetComponent<FairyDamageController>();

                    if (controller) controller.AcculumatedBaseDamage += info.damage / body.damage;
                }
            }

            orig(self, info, victim);

            if ((info.HasModdedDamageType(FairyOnHit) || info.HasModdedDamageType(TripleFairyOnHit)) && victim) {
                CharacterBody body = victim.GetComponent<CharacterBody>();

                if (body) {
                    int count = info.HasModdedDamageType(TripleFairyOnHit) ? 3 : 1;

                    for (int i = 0; i < count; i++) {
                        body.AddTimedBuff(Buff, 10f);
                    }

                    if (!body.GetComponent<FairyDamageController>()) {
                        FairyDamageController fairyController = body.AddComponent<FairyDamageController>();
                        fairyController.body = body;
                    }
                }
            }
        }
    }

    public class FairyDamageController : MonoBehaviour {
        public float AcculumatedBaseDamage = 0f;
        public CharacterBody body;

        public void FixedUpdate() {
            if (!body.HasBuff(Fairy.Instance.Buff)) {
                Destroy(this);
                return;
            }

            if (AcculumatedBaseDamage >= Fairy.DamageCoeffToDetonate) {
                int times = Mathf.FloorToInt(AcculumatedBaseDamage / Fairy.DamageCoeffToDetonate);
                AcculumatedBaseDamage = 0f;

                DetonateFairyStacks(times);
            }
        }

        public void DetonateFairyStacks(int times) {
            int fairyStacks = body.GetBuffCount(Fairy.Instance.Buff);

            DamageInfo damage = new();
            damage.attacker = null;
            damage.crit = false;
            damage.procCoefficient = 0;
            damage.damage = body.damage * fairyStacks * times;
            damage.damageColorIndex = DamageColorIndex.Fragile;
            damage.position = body.corePosition;
            
            body.healthComponent.TakeDamage(damage);

            GameObject effect = GameObject.Instantiate(Enemies.ArbiterBoss.ArbiterBoss.FairyTracerSlashEffect, body.corePosition, Quaternion.LookRotation(Random.onUnitSphere));
            effect.transform.localScale *= 2f;

            AkSoundEngine.PostEvent(Events.Play_item_void_bleedOnHit_explo, base.gameObject);
        }
    }
}