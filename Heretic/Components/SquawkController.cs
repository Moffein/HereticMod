using RoR2;
using UnityEngine.Networking;
using UnityEngine;
using BepInEx.Configuration;

namespace HereticMod.Components
{
    public class SquawkController : NetworkBehaviour
    {
        private static bool initialized = false;
        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            HereticPlugin.HereticBodyObject.AddComponent<SquawkController>();

            //Need NetworkBehavior because SimpleSoundEffect doesnt seem to be able to be parented to a GameObject, which ends up sounding weird when in motion.
            /*SquawkController.squawk = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            SquawkController.squawk.eventName = "Play_heretic_squawk";
            R2API.ContentAddition.AddNetworkSoundEventDef(SquawkController.squawk);*/
        }

        public static float baseCooldown = 0.3f;
        public static NetworkSoundEventDef squawk;
        private bool wasPressed = false;
        private float cooldownStopwatch = 0f;

        //Taken from https://github.com/ToastedOven/CustomEmotesAPI/blob/main/CustomEmotesAPI/CustomEmotesAPI/CustomEmotesAPI.cs
        private static bool GetKeyPressed(ConfigEntry<KeyboardShortcut> entry)
        {
            foreach (var item in entry.Value.Modifiers)
            {
                if (!Input.GetKey(item))
                {
                    return false;
                }
            }
            return Input.GetKeyDown(entry.Value.MainKey);
        }


        public void Update()
        {
            if (this.hasAuthority)
            {
                if (cooldownStopwatch <= 0f)
                {
                    if (GetKeyPressed(HereticPlugin.squawkButton))
                    {
                        if (!wasPressed)
                        {
                            //EffectManager.SimpleSoundEffect(SquawkController.squawk.index, base.transform.position, true);
                            Util.PlaySound("Play_heretic_squawk", base.gameObject);
                            CmdSquawk();
                            cooldownStopwatch = 0.15f;
                        }
                        wasPressed = true;
                    }
                    else
                    {
                        wasPressed = false;
                    }
                }
                else
                {
                    cooldownStopwatch -= Time.deltaTime;
                }
            }
        }

        [Command]
        public void CmdSquawk()
        {
            RpcSquawk();
        }

        [ClientRpc]
        private void RpcSquawk()
        {
            if (!this.hasAuthority)
            {
                Util.PlaySound("Play_heretic_squawk", base.gameObject);
            }
        }
    }
}
