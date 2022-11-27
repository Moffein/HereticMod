using BepInEx;
using BepInEx.Configuration;
using HereticMod.Components;
using R2API;
using R2API.Utils;
using RiskOfOptions;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HereticMod
{
    [BepInDependency("com.rune580.riskofoptions")]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Moffein.Heretic", "Heretic", "1.1.5")]
    [R2API.Utils.R2APISubmoduleDependency(nameof(RecalculateStatsAPI), nameof(ContentAddition), nameof(ItemAPI), nameof(PrefabAPI), nameof(LoadoutAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class HereticPlugin : BaseUnityPlugin
    {
        public static bool fixTypos = true;
        public static bool visionsAttackSpeed = true;
        public static bool giveHereticItem = true;
        public static float sortPosition = 17f;
        public static ConfigEntry<KeyboardShortcut> squawkButton;
        public static bool forceUnlock;

        public static PluginInfo pluginInfo;

        public static BodyIndex HereticBodyIndex;
        public static GameObject HereticBodyObject;
        public static SurvivorDef HereticSurvivorDef;

        public void Awake()
        {
            HereticBodyObject = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Heretic/HereticBody.prefab").WaitForCompletion();
            ReadConfig();

            pluginInfo = Info;
            Assets.Init();
            Tokens.Init();

            Skins.InitSkins(HereticBodyObject);
            ModifyStats(HereticBodyObject.GetComponent<CharacterBody>());

            ModifySurvivorDef();
            ModifyLunarSkillDefs.Init();
            SkillSetup.Init();
            SquawkController.Init();

            HereticItem.Init();

            On.RoR2.CameraRigController.OnEnable += DisableLobbyFade;
            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();
                HereticBodyIndex = BodyCatalog.FindBodyIndex("HereticBody");
            };
        }

        public static void DisableLobbyFade(On.RoR2.CameraRigController.orig_OnEnable orig, CameraRigController self)
        {
            SceneDef sd = RoR2.SceneCatalog.GetSceneDefForCurrentScene();
            if (sd && sd.baseSceneName.Equals("lobby"))
            {
                self.enableFading = false;
            }
            orig(self);
        }

        private void ModifyStats(CharacterBody cb)
        {
            cb.baseMaxHealth = 110f;
            cb.levelMaxHealth = 33f;

            cb.baseDamage = 12f;
            cb.levelDamage = 2.4f;

            cb.baseRegen = 1f;
            cb.levelRegen = 0.2f;
        }

        private void ModifySurvivorDef()
        {
            HereticSurvivorDef = Addressables.LoadAssetAsync<SurvivorDef>("RoR2/Base/Heretic/Heretic.asset").WaitForCompletion();
            HereticSurvivorDef.hidden = false;

            UnlockableDef hereticUnlock = ScriptableObject.CreateInstance<UnlockableDef>();
            hereticUnlock.cachedName = "Survivors.MoffeinHeretic";
            hereticUnlock.nameToken = "ACHIEVEMENT_MOFFEINHERETIC_UNLOCK_NAME";
            hereticUnlock.achievementIcon = Assets.assetBundle.LoadAsset<Sprite>("texHereticUnlock.png");
            ContentAddition.AddUnlockableDef(hereticUnlock);

            if (!HereticPlugin.forceUnlock)
            {
                HereticSurvivorDef.unlockableDef = hereticUnlock;
            }

            GameObject HereticDisplayPrefab = HereticBodyObject.GetComponent<ModelLocator>().modelTransform.gameObject.InstantiateClone("MoffeinHereticDisplay", false);
            HereticDisplayPrefab.transform.localScale *= 0.6f;
            HereticSurvivorDef.displayPrefab = HereticDisplayPrefab;
            HereticDisplayPrefab.AddComponent<MenuAnimComponent>();

            HereticSurvivorDef.desiredSortPosition = sortPosition;

            //Hopefully hooking this will make it work in MP
            On.RoR2.SurvivorMannequins.SurvivorMannequinSlotController.RebuildMannequinInstance += (orig, self) =>
            {
                orig(self);
                if (self.currentSurvivorDef == HereticSurvivorDef)
                {
                    MenuAnimComponent mac = self.mannequinInstanceTransform.gameObject.GetComponent<MenuAnimComponent>();
                    if (mac) mac.Play();
                }
            };
        }

        private void ReadConfig()
        {
            HereticPlugin.forceUnlock = Config.Bind("Unlock", "Force Unlock", false, "Unlocks Heretic by default.").Value;

            HereticPlugin.visionsAttackSpeed = Config.Bind("Gameplay", "Visions of Heresy Attack Speed", true, "Reload speed of Visions of Heresy scales with Attack Speed instead of Cooldown.").Value;
            HereticPlugin.giveHereticItem = Config.Bind("Gameplay", "Enable Mark of Heresy", true, "Collecting all 4 Heresy items gives you the Mark of Heresy.").Value;

            HereticPlugin.fixTypos = Config.Bind("General", "Fix Skill Descriptions", true, "Fixes a typo with Hooks of Heresy and adds color-coding to Essence of Heresy.").Value;
            HereticPlugin.sortPosition = Config.Bind("General", "Character Select Sort Position", 17f, "Determines which spot this survivor will take in the Character Select menu.").Value;    //set to 14 to go after captain
            HereticPlugin.squawkButton = Config.Bind("General", "Squawk Button", KeyboardShortcut.Empty, "Press this button to squawk.");
            ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(HereticPlugin.squawkButton));
        }
    }
}
