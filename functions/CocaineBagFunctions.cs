using JetBrains.Annotations;
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

namespace Ocelot.SnowCooking.functions
{
    public class CocaineBagFunctions
    {
        public static void ConsumeAction(Player instigatingPlayer, ItemConsumeableAsset consumeableAsset)
        {
            UnturnedPlayer player = UnturnedPlayer.FromPlayer(instigatingPlayer);
            if (consumeableAsset.id == SnowCookingPlugin.Instance.Configuration.Instance.snowBagId)
            {
                SnowCookingPlugin.Instance.drugeffectPlayersList.Add(new DrugeffectTimeObject(player.Id));
                if (SnowCookingPlugin.Instance.Configuration.Instance.UseDrugEffectSpeed)
                {
                    player.Player.movement.sendPluginSpeedMultiplier(SnowCookingPlugin.Instance.Configuration.Instance.DrugEffectSpeedMultiplier);
                }
                if (SnowCookingPlugin.Instance.Configuration.Instance.UseDrugEffectJump)
                {
                    player.Player.movement.sendPluginJumpMultiplier(SnowCookingPlugin.Instance.Configuration.Instance.DrugEffectJumpMultiplier);
                }
            }
        }

        public static void Update()
        {
            foreach (var drugeffect in SnowCookingPlugin.Instance.drugeffectPlayersList.ToList())
            {
                if (SnowCookingPlugin.getCurrentTime() - drugeffect.time >= SnowCookingPlugin.Instance.Configuration.Instance.DrugEffectDurationSecs)
                {
                    SnowCookingPlugin.Instance.drugeffectPlayersList.Remove(drugeffect);
                    UnturnedPlayer player = UnturnedPlayer.FromCSteamID(new CSteamID(ulong.Parse(drugeffect.playerId)));
                    if (SnowCookingPlugin.Instance.Configuration.Instance.UseDrugEffectSpeed)
                    {
                        player.Player.movement.sendPluginSpeedMultiplier(1);
                    }
                    if (SnowCookingPlugin.Instance.Configuration.Instance.UseDrugEffectJump)
                    {
                        player.Player.movement.sendPluginJumpMultiplier(1);
                    }
                }
            }
        }
    }
}
