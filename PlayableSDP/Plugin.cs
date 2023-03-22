using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;
using RoR2.ExpansionManagement;

namespace PlayableSDP {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    
    public class PlayableSDP : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "pseudopulseName";
        public const string PluginName = "PlayableSDP";
        public const string PluginVersion = "1.0.0";

        public static AssetBundle bundle;
        public static BepInEx.Logging.ManualLogSource ModLogger;

        // assets
        public static GameObject SDPBody;
        public static SurvivorDef sdSDP;

        public void Awake() {
            // assetbundle loading 
            bundle = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("PlayableSDP.dll", "sdpbundle"));

            // set logger
            ModLogger = Logger;

            LoadAssets();
            ModifyAssets();
            SetupLanguage();
            PrefabAPI.RegisterNetworkPrefab(SDPBody);
            ContentAddition.AddBody(SDPBody);
            ContentAddition.AddSurvivorDef(sdSDP);
        }

        public void LoadAssets() {
            SDPBody = bundle.LoadAsset<GameObject>("SDPBody.prefab");
            sdSDP = bundle.LoadAsset<SurvivorDef>("sdSDP.asset");
        }

        public void ModifyAssets() {
            ExpansionDef sotv = Utils.Paths.ExpansionDef.DLC1.Load<ExpansionDef>();

            ExpansionRequirementComponent req = SDPBody.GetComponent<ExpansionRequirementComponent>();
            req.requiredExpansion = sotv;

            SkillLocator locator = SDPBody.GetComponent<SkillLocator>();
            SetupSkill(locator.primary);
            SetupSkill(locator.secondary);
            SetupSkill(locator.utility);
            SetupSkill(locator.special);

            SDPBody.AddComponent<FreeWinFriday>();
        }

        public void SetupLanguage() {
            "SDP_BODY_NAME".Add("Spare Drone Parts");
            "SDP_BODY_SUBTITLE".Add("C Tier");
            "SDP_PASSIVE_NAME".Add("Free Win Friday");
            "SDP_PASSIVE_DESC".Add("<style=cIsUtility>Spare Drone Parts</style> instantly wins the run. <style=cDeath>Spare Drone Parts' C tier status prevents it from moving</style>.");
            "SDP_BODY_DESC".Add(
                """
                Spare Drone Parts gets a C. Your drones are meant to draw aggro, not deal damage. This item doesn't change that. While it makes your drone's damage much, much higher, when compared to your overall damage it really is no competition. Plus, if Colonel Droneman dies he wont respawn until the next stage, making the overall usefulness of Spare Drone Parts pretty limited.
                """
            );
        }

        private void SetupSkill(GenericSkill slot) {
            SkillDef disconnected = Utils.Paths.SkillDef.CaptainSkillDisconnected.Load<SkillDef>();
            SkillFamily family = ScriptableObject.CreateInstance<SkillFamily>();
            (family as ScriptableObject).name = slot.skillName + "Family";
            family.variants = new SkillFamily.Variant[] {
                new SkillFamily.Variant {
                    skillDef = disconnected,
                    unlockableDef = null,
                    viewableNode = new(disconnected.skillNameToken, false, null)
                }
            };
            slot._skillFamily = family;
            ContentAddition.AddSkillFamily(family);
        }

        private class FreeWinFriday : MonoBehaviour {
            public float stopwatch = 0f;

            private void FixedUpdate() {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch >= 4f && Run.instance && NetworkServer.active) {
                    Run.instance.BeginGameOver(RoR2Content.GameEndings.PrismaticTrialEnding);
                }
            }
        }
    }
}