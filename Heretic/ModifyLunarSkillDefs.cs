using HereticMod.Components;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace HereticMod
{
    //This modifies the Lunar Skills to function properly at 0 stacks of their corresponding item.
    public class ModifyLunarSkillDefs
    {
        private static bool initialized = false;

        public static void Init()
        {
            if (initialized) return;
            initialized = true;
            
            //SKILLSLOT.CHARACTERBODY.INVENTORY NEEDS TO BE NULLCHECKED
            SetupPrimary();
            SetupSecondary();
            SetupUtility();
            SetupSpecial();
        }

        private static void SetupPrimary()
        {
            if (HereticPlugin.visionsAttackSpeed)
            {
                LunarPrimaryReplacementSkill visionsDef = Addressables.LoadAssetAsync<LunarPrimaryReplacementSkill>("RoR2/Base/LunarSkillReplacements/LunarPrimaryReplacement.asset").WaitForCompletion();
                visionsDef.attackSpeedBuffsRestockSpeed = true;
                visionsDef.attackSpeedBuffsRestockSpeed_Multiplier = 1f;

            }

            //LunarPrimaryReplacement overrides the attackSpeedBuffsRestockSpeed stat
            On.RoR2.Skills.LunarPrimaryReplacementSkill.GetRechargeInterval += LunarPrimaryReplacementSkill_GetRechargeInterval;

            On.RoR2.Skills.LunarPrimaryReplacementSkill.GetMaxStock += (orig, self, skillSlot) =>
            {
                int maxStock = self.baseMaxStock;
                if (skillSlot && skillSlot.characterBody && skillSlot.characterBody.inventory)
                {
                    maxStock = Math.Max(orig(self, skillSlot), maxStock);
                }
                return maxStock;
            };
        }

        private static float LunarPrimaryReplacementSkill_GetRechargeInterval(On.RoR2.Skills.LunarPrimaryReplacementSkill.orig_GetRechargeInterval orig, LunarPrimaryReplacementSkill self, RoR2.GenericSkill skillSlot)
        {
            //Fix cooldown at 0 stacks
            float interval = self.baseRechargeInterval;
            if (skillSlot && skillSlot.characterBody && skillSlot.characterBody.inventory)
            {
                interval = Mathf.Max(orig(self, skillSlot), interval);
            }

            //Apply attack speed scaling if enabled
            if (self.attackSpeedBuffsRestockSpeed && skillSlot)
            {
                float num = skillSlot.characterBody.attackSpeed - skillSlot.characterBody.baseAttackSpeed;
                num *= self.attackSpeedBuffsRestockSpeed_Multiplier;
                num += 1f;
                if (num < 0.5f)
                {
                    num = 0.5f;
                }
                interval /= num;
            }

            return interval;
        }
        private static void SetupSecondary()
        {
            On.RoR2.Skills.LunarSecondaryReplacementSkill.GetRechargeInterval += (orig, self, skillSlot) =>
            {
                float interval = self.baseRechargeInterval;
                if (skillSlot && skillSlot.characterBody && skillSlot.characterBody.inventory)
                {
                    interval = Mathf.Max(orig(self, skillSlot), interval);
                }
                return interval;
            };

            IL.RoR2.GlobalEventManager.ProcessHitEnemy += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(MoveType.After,
                         x => x.MatchLdsfld(typeof(RoR2.RoR2Content.Items), "LunarSecondaryReplacement"),
                         x => x.MatchCallvirt<RoR2.Inventory>("GetItemCount")
                         );
                c.EmitDelegate<Func<int, int>>(itemCount =>
                {
                    if (itemCount <= 0)
                    {
                        itemCount = 1;
                    }

                    return itemCount;
                });
            };
        }

        private static void SetupUtility()
        {
            IL.EntityStates.GhostUtilitySkillState.OnEnter += (il) =>
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(MoveType.After,
                         x => x.MatchLdsfld(typeof(RoR2.RoR2Content.Items), "LunarUtilityReplacement"),
                         x => x.MatchCallvirt<RoR2.Inventory>("GetItemCount")
                         );
                c.EmitDelegate<Func<int, int>>(itemCount =>
               {
                   if (itemCount <= 0) itemCount = 1;
                   return itemCount;
               });
            };
        }

        //Spaghetti code
        private static void SetupSpecial()
        {
            On.RoR2.Skills.LunarDetonatorSkill.GetRechargeInterval += (orig, self, skillSlot) =>
            {
                float interval = self.baseRechargeInterval;
                if (skillSlot && skillSlot.characterBody && skillSlot.characterBody.inventory)
                {
                    interval = Mathf.Max(orig(self, skillSlot), interval);
                }
                return interval;
            };

            //Incredibly jank.
            On.RoR2.Skills.LunarDetonatorSkill.OnAssigned += (orig, self, skillSlot) =>
            {
                if (skillSlot && skillSlot.characterBody && skillSlot.characterBody.skillLocator && skillSlot.characterBody.skillLocator.allSkills == null)
                {
                    AssignLunarDetonator ald = skillSlot.characterBody.gameObject.GetComponent<AssignLunarDetonator>();
                    if (!ald) ald = skillSlot.characterBody.gameObject.AddComponent<AssignLunarDetonator>();
                    ald.cb = skillSlot.characterBody;
                    ald.skill = self;
                    ald.skillSlot = skillSlot;
                    return null;
                }
                else
                {
                    return orig(self, skillSlot);
                }
            };

            //Fixes a nullref that sometimes shows up.
            On.RoR2.Skills.LunarDetonatorSkill.OnUnassigned += (orig, self, skillSlot) =>
            {
                if (skillSlot && (RoR2.Skills.LunarDetonatorSkill.InstanceData)skillSlot.skillInstanceData != null)
                {
                    orig(self, skillSlot);
                }
            };
        }
    }
}
