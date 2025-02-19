using Ocelot.SnowCooking.functions;
using Ocelot.SnowCooking.objects;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Enumerations;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Ocelot.SnowCooking
{
    public class SnowCookingPlugin : RocketPlugin<SnowCookingConfiguration>
    {
        public static SnowCookingPlugin Instance;
        public const string VERSION = "1.2.14";

        private int Frame = 0;
        private double timer = 0;
        public Dictionary<Transform, HeaterObject> heaterList = new Dictionary<Transform, HeaterObject>();
        public Dictionary<Transform, PanObject> panList = new Dictionary<Transform, PanObject>();
        public Dictionary<string, Vector3> heaterUiOpened = new Dictionary<string, Vector3>();
        public Dictionary<Transform, PanFilledObject> panFilledList = new Dictionary<Transform, PanFilledObject>();
        public Dictionary<Transform, PanPowderObject> panPowderList = new Dictionary<Transform, PanPowderObject>();

        public List<Transform> cocaLeavesList = new List<Transform>();
        public List<Transform> dryingLampList = new List<Transform>();
        public List<DrugeffectTimeObject> drugeffectPlayersList = new List<DrugeffectTimeObject>();
        protected override void Load()
        {
            Instance = this;
            Logger.Log("SnowCookingPlugin v" + VERSION + " by Ocelot loaded! Enjoy! :)", ConsoleColor.Yellow);

            BarricadeManager.onDeployBarricadeRequested += BarricadeDeployed;
            //UnturnedPlayerEvents.OnPlayerUpdateGesture += OnPlayerUpdateGesture;
            PlayerAnimator.OnGestureChanged_Global += OnGestureChanged;
            EffectManager.onEffectButtonClicked += ButtonClick;
            UseableConsumeable.onConsumePerformed += ConsumeAction;
            BarricadeManager.onSalvageBarricadeRequested += BarricadeSalvaged;
            BarricadeManager.onDamageBarricadeRequested += BarricadeDamaged;
            U.Events.OnPlayerDisconnected += PlayerDisconnected;
            BarricadeManager.onModifySignRequested += OnModifySign;
            if (Level.isLoaded)
            {
                BarricadeFunctions.AddExistingBarricades(1);
            }
            else
            {
                Level.onLevelLoaded += BarricadeFunctions.AddExistingBarricades;
            }
        }

        private void OnGestureChanged(PlayerAnimator arg1, EPlayerGesture gesture)
        {
            if (gesture == EPlayerGesture.PUNCH_LEFT || gesture == EPlayerGesture.PUNCH_RIGHT)
            {
                HeaterFunctions.OnGestureChanged(UnturnedPlayer.FromPlayer(arg1.player), gesture);
                PanFunctions.OnGestureChanged(UnturnedPlayer.FromPlayer(arg1.player), gesture);
            }
        }

        private void BarricadeDamaged(CSteamID instigatorSteamID, Transform barricadeTransform, ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            BarricadeFunctions.BarricadeDamaged(barricadeTransform, pendingTotalDamage);
        }

        private void OnModifySign(CSteamID instigator, InteractableSign sign, ref string text, ref bool shouldAllow)
        {
            foreach (var heater in heaterList)
            {
                if (heater.Key == sign.transform)
                {
                    shouldAllow = false;
                    ChatManager.serverSendMessage(Translate("cocaine_sign_edit"), Color.white, null, UnturnedPlayer.FromCSteamID(instigator).SteamPlayer(), EChatMode.SAY, Configuration.Instance.iconImageUrl, true);
                }
            }
        }

        private void BarricadeSalvaged(CSteamID steamID, byte x, byte y, ushort plant, ushort index, ref bool shouldAllow)
        {
            BarricadeFunctions.BarricadeSalvaged(steamID, x, y, plant, index);
        }

        protected override void Unload()
        {
            BarricadeManager.onDeployBarricadeRequested -= BarricadeDeployed;
            //UnturnedPlayerEvents.OnPlayerUpdateGesture -= OnPlayerUpdateGesture;
            PlayerAnimator.OnGestureChanged_Global -= OnGestureChanged;
            EffectManager.onEffectButtonClicked -= ButtonClick;
            UseableConsumeable.onConsumePerformed -= ConsumeAction;
            BarricadeManager.onSalvageBarricadeRequested -= BarricadeSalvaged;
            U.Events.OnPlayerDisconnected -= PlayerDisconnected;
        }

        private void PlayerDisconnected(UnturnedPlayer player)
        {
            if (heaterUiOpened.ContainsKey(player.Id))
            {
                heaterUiOpened.Remove(player.Id);
            }
        }

        private void ConsumeAction(Player instigatingPlayer, ItemConsumeableAsset consumeableAsset)
        {
            CocaineBagFunctions.ConsumeAction(instigatingPlayer, consumeableAsset);
        }

        private void ButtonClick(Player player, string buttonName)
        {
            UiFunctions.ButtonClick(player, buttonName);
        }

        //private void OnPlayerUpdateGesture(UnturnedPlayer player, UnturnedPlayerEvents.PlayerGesture gesture)
        //{
        //    if (gesture == UnturnedPlayerEvents.PlayerGesture.PunchLeft || gesture == UnturnedPlayerEvents.PlayerGesture.PunchRight)
        //    {
        //        HeaterFunctions.OnPlayerUpdateGesture(player, gesture);
        //        PanFunctions.OnPlayerUpdateGesture(player, gesture);
        //    }
        //}

        private void BarricadeDeployed(Barricade barricade, ItemBarricadeAsset asset, Transform hit, ref Vector3 point, ref float angle_x, ref float angle_y, ref float angle_z, ref ulong owner, ref ulong group, ref bool shouldAllow)
        {
            Vector3 pos = point;
            ulong ownerPan = owner;
            ulong groupPan = group;
            float angle_x_pan = angle_x;
            float angle_y_pan = angle_y;
            float angle_z_pan = angle_z;
            BarricadeFunctions.BarricadeDeployed(barricade, asset, hit, pos, angle_x_pan, angle_y_pan, angle_z_pan, ownerPan, groupPan);
        }

        

        public override TranslationList DefaultTranslations => new TranslationList
        {
            {"temperature_symbol", " °C" },
            {"temperature_title", "Temperature" },
            {"cocaine_sign_edit", "<color=#00B92C>[SnowCooking]</color> Sorry, but you are <color=#ff3c19>not allowed</color> to edit this sign." }
        };

        private void Update()
        {
            Frame++;
            if (Frame % 5 != 0) return; // BRICHT METHODE AB WENN DER FRAME NICHT DURCH 5 TEILBAR IST
            // DO STUFF EVERY GAME FRAME E.G 60/s
            if (getCurrentTime() - timer >= 1)
            {
                timer = getCurrentTime();

                HeaterFunctions.Update();
                PanFunctions.Update();
                DryinglampFunctions.Update();
                CocaineBagFunctions.Update();
            }
        }

        public Dictionary<Vector3, Transform> GetAllObjects()
        {
            Dictionary<Vector3, Transform> objectsOnMap = new Dictionary<Vector3, Transform>();
            foreach (var region in BarricadeManager.regions)
            {
                foreach (var drop in region.drops)
                {
                    if (!objectsOnMap.ContainsKey(drop.model.position))
                    {
                        objectsOnMap.Add(drop.model.position, drop.model);
                    }
                }
            }
            return objectsOnMap;
        }

        public Transform GetPlacedObjectTransform(Vector3 objectPosition)
        {
            Dictionary<Vector3, Transform> objectsOnMap = new Dictionary<Vector3, Transform>();
            objectsOnMap = GetAllObjects();

            foreach (var mapObject in objectsOnMap.ToList())
            {
                if (mapObject.Key == objectPosition)
                {
                    return mapObject.Value;
                }
            }
            return null; //Never happens
        }

        //FOR SIGN
        public Dictionary<Vector3, InteractableSign> GetAllSigns()
        {
            Dictionary<Vector3, InteractableSign> objectsOnMap = new Dictionary<Vector3, InteractableSign>();
            foreach (var region in BarricadeManager.regions)
            {
                foreach (var drop in region.drops)
                {
                    if (!objectsOnMap.ContainsKey(drop.model.position))
                    {
                        InteractableSign sign = (InteractableSign)drop.interactable;
                        objectsOnMap.Add(drop.model.position, sign);
                    }
                }
            }
            return objectsOnMap;
        }

        public InteractableSign GetPlacedObjectSign(Vector3 objectPosition)
        {
            Dictionary<Vector3, InteractableSign> objectsOnMap = new Dictionary<Vector3, InteractableSign>();
            objectsOnMap = GetAllSigns();

            foreach (var mapObject in objectsOnMap.ToList())
            {
                if (mapObject.Key == objectPosition)
                {
                    return mapObject.Value;
                }
            }
            return null; //Never happens
        }

        public BarricadeData getBarricadeDataAtPosition(Vector3 position)
        {
            foreach (var region in BarricadeManager.regions)
            {
                foreach (var b in region.barricades)
                {
                    if (b.point == position) return b;
                }
            }
            return null;
        }

        public static Int32 getCurrentTime()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public void Wait(float seconds, System.Action action)
        {
            StartCoroutine(_wait(seconds, action));
        }
        IEnumerator _wait(float time, System.Action callback)
        {
            yield return new WaitForSeconds(time);
            callback();
        }
    }
}