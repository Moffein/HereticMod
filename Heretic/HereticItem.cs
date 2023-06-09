using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.Networking;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

namespace HereticMod
{
    class HereticItem
    {
        public static ItemDef HereticStatBonusItem;

        private static bool initialized = false;
        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            HereticStatBonusItem = ScriptableObject.CreateInstance<ItemDef>();
            HereticStatBonusItem.canRemove = false;
            HereticStatBonusItem.name = "MoffeinHereticStatBonusItem";
            HereticStatBonusItem.deprecatedTier = ItemTier.NoTier;
            HereticStatBonusItem.descriptionToken = "MOFFEINHERETIC_STATBONUSITEM_DESC";
            HereticStatBonusItem.nameToken = "MOFFEINHERETIC_STATBONUSITEM_NAME";
            HereticStatBonusItem.pickupToken = "MOFFEINHERETIC_STATBONUSITEM_PICKUP";
            HereticStatBonusItem.hidden = false;
            HereticStatBonusItem.pickupIconSprite = Assets.assetBundle.LoadAsset<Sprite>("texHeresyItemIcon");
            HereticStatBonusItem.tags = new[]
            {
                ItemTag.WorldUnique,
                ItemTag.BrotherBlacklist,
                ItemTag.CannotSteal,
                ItemTag.CannotDuplicate
            };
            ItemDisplayRule[] idr = new ItemDisplayRule[0];
            ItemAPI.Add(new CustomItem(HereticStatBonusItem, idr));

            RecalculateStatsAPI.GetStatCoefficients += (sender, args) =>
            {
                if (sender.inventory && sender.inventory.GetItemCount(HereticItem.HereticStatBonusItem) > 0)
                {
                    bool shieldOnly = sender.HasBuff(RoR2Content.Buffs.AffixLunar) || sender.inventory.GetItemCount(RoR2Content.Items.ShieldOnly) > 0;
                    float adjustedLevel = sender.level - 1f;
                    if (!shieldOnly)
                    {
                        args.baseRegenAdd -= sender.baseRegen + sender.levelRegen * adjustedLevel;//Negate base regen.
                    }

                    args.healthMultAdd += 3f;

                    args.baseDamageAdd += 0.5f * (sender.baseDamage + sender.levelDamage * adjustedLevel);
                }
            };

            //Place regen penalty here to make it ignore difficulty
            IL.RoR2.HealthComponent.ServerFixedUpdate += (il) =>
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After,
                     x => x.MatchLdfld(typeof(HealthComponent), "regenAccumulator")
                    ))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<float, HealthComponent, float>>((regenAccumulator, self) =>
                    {
                        if (self.body.inventory && self.body.inventory.GetItemCount(HereticItem.HereticStatBonusItem) > 0)
                        {
                            bool shieldOnly = self.body.HasBuff(RoR2Content.Buffs.AffixLunar) || self.body.inventory.GetItemCount(RoR2Content.Items.ShieldOnly) > 0;
                            if (!shieldOnly)
                            {
                                float regen = Time.fixedDeltaTime * (6f + 1.2f * (self.body.level - 1f));
                                regen /= self.body.cursePenalty;

                                regenAccumulator -= regen;
                            }
                        }
                        return regenAccumulator;
                    });
                }
                else
                {
                    UnityEngine.Debug.LogError("RiskyMod: ModifyRegenAccumulator IL Hook failed");
                }
            };

            if (HereticPlugin.giveHereticItem) On.RoR2.CharacterMaster.OnInventoryChanged += GiveHereticItem;
        }

        private static void GiveHereticItem(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self)
        {
            bool isHeretic = false;
            CharacterBody cb = self.GetBody();
            if (cb && cb.bodyIndex == HereticPlugin.HereticBodyIndex) isHeretic = true;

            orig(self);

            if (NetworkServer.active
                && self.inventory
                && self.inventory.GetItemCount(RoR2Content.Items.LunarPrimaryReplacement.itemIndex) > 0
                && self.inventory.GetItemCount(RoR2Content.Items.LunarSecondaryReplacement.itemIndex) > 0
                && self.inventory.GetItemCount(RoR2Content.Items.LunarSpecialReplacement.itemIndex) > 0
                && self.inventory.GetItemCount(RoR2Content.Items.LunarUtilityReplacement.itemIndex) > 0
                && self.inventory.GetItemCount(HereticItem.HereticStatBonusItem) <= 0)
            {
                self.inventory.GiveItem(HereticItem.HereticStatBonusItem);

                if (isHeretic)
                {
                    EffectManager.SimpleEffect(EntityStates.Heretic.SpawnState.effectPrefab, cb.corePosition, Quaternion.identity, true);
                }
            }
        }
    }
}
