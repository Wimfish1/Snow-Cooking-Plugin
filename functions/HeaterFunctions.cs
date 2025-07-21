using JetBrains.Annotations;
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
    public class HeaterFunctions
    {
        private const short EffectKey = 27400;

        public static void OnGestureChanged(UnturnedPlayer player, EPlayerGesture gesture)
        {
            if (Physics.Raycast(player.Player.look.aim.position, player.Player.look.aim.forward, out RaycastHit raycastHit, 2, RayMasks.BARRICADE))
            {
                foreach (var heater in SnowCookingPlugin.Instance.heaterList.ToList())
                {
                    if (heater.Key == raycastHit.transform)
                    {
                        string status = "Toggle Stove (OFF)";
                        if (heater.Value.isActive)
                        {
                            status = "Toggle Stove (ON)";
                        }
                        
                        double prog = System.Math.Round((SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / 100.0) * heater.Value.progress, 1);
                        EffectManager.sendUIEffect(SnowCookingPlugin.Instance.Configuration.Instance.heaterUiId, EffectKey, player.SteamPlayer().transportConnection, true);
                        EffectManager.sendUIEffectText(EffectKey, player.SteamPlayer().transportConnection, true, "SnowCookingUI_ToggleText", status);
                        EffectManager.sendUIEffectText(EffectKey, player.SteamPlayer().transportConnection, true, "Temperature_Degrees", prog.ToString() + "°C");
                        
                        player.Player.enablePluginWidgetFlag(EPluginWidgetFlags.Modal);
                        if (SnowCookingPlugin.Instance.heaterUiOpened.ContainsKey(player.Id))
                        {
                            SnowCookingPlugin.Instance.heaterUiOpened.Remove(player.Id);
                        }
                        if (heater.Key == null)
                            break;
                        SnowCookingPlugin.Instance.heaterUiOpened.Add(player.Id, heater.Key.position);
                    }
                }
            }
        }
        //public static void OnPlayerUpdateGesture(UnturnedPlayer player, UnturnedPlayerEvents.PlayerGesture gesture)
        //{
        //    if (Physics.Raycast(player.Player.look.aim.position, player.Player.look.aim.forward, out RaycastHit raycastHit, 2, RayMasks.BARRICADE))
        //    {
        //        foreach (var heater in SnowCookingPlugin.Instance.heaterList.ToList())
        //        {
        //            if (heater.Key == raycastHit.transform)
        //            {
        //                string progressBar = "";
        //                string status = "<color=red>OFF</color>";
        //                if (heater.Value.isActive)
        //                {
        //                    status = "<color=green>ON</color>";
        //                }
        //                //Sets the color of the progress bar
        //                if ((SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / 100.0) * heater.Value.progress >= SnowCookingPlugin.Instance.Configuration.Instance.heaterTooHotDegree)
        //                {
        //                    progressBar += "<color=" + SnowCookingPlugin.Instance.Configuration.Instance.heaterTooHotColor + ">";
        //                }
        //                else if ((SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / 100.0) * heater.Value.progress >= SnowCookingPlugin.Instance.Configuration.Instance.heaterHotDegree)
        //                {
        //                    progressBar += "<color=" + SnowCookingPlugin.Instance.Configuration.Instance.heaterHotColor + ">";
        //                }
        //                else
        //                {
        //                    progressBar += "<color=" + SnowCookingPlugin.Instance.Configuration.Instance.heaterColdColor + ">";
        //                }
        //                //Color end
        //                for (int i = 0; i < heater.Value.progress; i += 5)
        //                {
        //                    progressBar += "█";
        //                }
        //                progressBar += "</color>";
        //                double prog = System.Math.Round((SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / 100.0) * heater.Value.progress, 1);
        //                EffectManager.sendUIEffect(SnowCookingPlugin.Instance.Configuration.Instance.heaterUiId, Convert.ToInt16(SnowCookingPlugin.Instance.Configuration.Instance.heaterUiId), player.CSteamID, false, SnowCookingPlugin.Instance.Translate("temperature_title"), prog.ToString() + SnowCookingPlugin.Instance.Translate("temperature_symbol"), progressBar, status);
        //                player.Player.enablePluginWidgetFlag(EPluginWidgetFlags.Modal);
        //                if (SnowCookingPlugin.Instance.heaterUiOpened.ContainsKey(player.Id))
        //                {
        //                    SnowCookingPlugin.Instance.heaterUiOpened.Remove(player.Id);
        //                }
        //                if (heater.Key == null)
        //                    break;
        //                SnowCookingPlugin.Instance.heaterUiOpened.Add(player.Id, heater.Key.position);
        //            }
        //        }
        //    }
        //}
        public static void Update()
        {
            double heatProg = SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / SnowCookingPlugin.Instance.Configuration.Instance.heatingDurationSecs;

            double coolProg = SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / SnowCookingPlugin.Instance.Configuration.Instance.coolingDurationSecs;

            try
            {
                foreach (var heater in SnowCookingPlugin.Instance.heaterList.ToList())
                {
                    if (heater.Key == null || heater.Value == null)
                        break;
                    int amountActiveGenerators = 0;
                    foreach (var Generator in PowerTool.checkGenerators(heater.Key.position, PowerTool.MAX_POWER_RANGE, ushort.MaxValue).ToList())
                    {
                        if (heater.Key == null || Generator.transform == null)
                            break;

                        /* old generator logic
                        if (!Generator.isPowered && Generator.wirerange >= (heater.Key.position - Generator.transform.position).magnitude || Generator.isPowered && Generator.fuel == 0 && Generator.wirerange >= (heater.Key.position - Generator.transform.position).magnitude)
                        {
                            
                        }*/
                        if (Generator.isPowered && Generator.wirerange >= (heater.Key.position - Generator.transform.position).magnitude && Generator.fuel > 0)
                        {
                            amountActiveGenerators++;
                        }
                    }
                    // new generator logic
                    if (amountActiveGenerators == 0)
                    {
                        if (heater.Value == null)
                            break;
                        if (heater.Value.isActive)
                        {
                            heater.Value.isActive = false;
                            foreach (var player in SnowCookingPlugin.Instance.heaterUiOpened.ToList())
                            {
                                if (heater.Key.position == null || player.Value == null)
                                    break;
                                if (player.Value == heater.Key.position)
                                {
                                    EffectManager.sendUIEffectText(Convert.ToInt16(SnowCookingPlugin.Instance.Configuration.Instance.heaterUiId), new CSteamID(ulong.Parse(player.Key)), false, "SnowCookingUI_ToggleText", "Toggle Stove (OFF)");
                                }
                            }
                        }
                    }
                    //
                }
            } catch (Exception ex)
            {
                Logger.Log(String.Format("EXCEPTION THROWN | ExNo 1 (Error message: {0})", ex.Message), ConsoleColor.Red);
                return;
            }

            try
            {
                foreach (var heater in SnowCookingPlugin.Instance.heaterList.ToList())
                {
                    if (heater.Key == null || heater.Value == null)
                        break;
                    if (heater.Value.isActive)
                    {
                        if (heater.Value.progress < 100.0)
                        {
                            if (heater.Value.progress + heatProg > 100.0)
                            {
                                heater.Value.progress = 100 - heatProg;
                            }
                            heater.Value.progress += heatProg;
                        }
                    }
                    else if (!heater.Value.isActive)
                    {
                        if (heater.Value.progress > 0)
                        {
                            heater.Value.progress -= coolProg;
                        }
                    }
                }
            } catch (Exception ex)
            {
                Logger.Log(String.Format("EXCEPTION THROWN | ExNo 2 (Error message: {0})", ex.Message), ConsoleColor.Red);
                return;
            }

            try
            {
                foreach (var heater in SnowCookingPlugin.Instance.heaterList.ToList())
                {
                    if (heater.Key == null || heater.Value == null)
                        break;
                    if (heater.Value.progress < 0 || heater.Value.progress > SnowCookingPlugin.Instance.Configuration.Instance.maxDegree)
                    {
                        return;
                    }
                    if (BarricadeManager.tryGetInfo(heater.Key, out byte x, out byte y, out ushort plant, out ushort index, out BarricadeRegion region))
                    {
                        double prog = System.Math.Round((SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / 100.0) * heater.Value.progress, 1);
                        string text = "";
                        if (prog >= SnowCookingPlugin.Instance.Configuration.Instance.heaterTooHotDegree)
                        {
                            text += "<color=" + SnowCookingPlugin.Instance.Configuration.Instance.heaterTooHotColor + ">";
                        }
                        else if (prog >= SnowCookingPlugin.Instance.Configuration.Instance.heaterHotDegree)
                        {
                            text += "<color=" + SnowCookingPlugin.Instance.Configuration.Instance.heaterHotColor + ">";
                        }
                        else
                        {
                            text += "<color=" + SnowCookingPlugin.Instance.Configuration.Instance.heaterColdColor + ">";
                        }
                        text += prog.ToString() + "</color>" + SnowCookingPlugin.Instance.Translate("temperature_symbol");
                        //BarricadeManager.instance.channel.send("tellUpdateSign", ESteamCall.ALL, ESteamPacket.UPDATE_UNRELIABLE_BUFFER, x, y, plant, index, text);
                        InteractableSign component = heater.Key.GetComponent<InteractableSign>();
                        BarricadeManager.ServerSetSignText(component, text);
                    }
                }
            } catch (Exception ex)
            {
                Logger.Log(String.Format("EXCEPTION THROWN | ExNo 3 (Error message: {0})", ex.Message), ConsoleColor.Red);
                return;
            }

            try {
                foreach (var player in Provider.clients.ToList())
                {
                    if (player == null)
                        break;
                    UnturnedPlayer uplayer = UnturnedPlayer.FromSteamPlayer(player);
                    foreach (var item in SnowCookingPlugin.Instance.heaterUiOpened.ToList())
                    {
                        foreach (var heater in SnowCookingPlugin.Instance.heaterList.ToList())
                        {
                            if (heater.Key == null || item.Value == null)
                                break;
                            if (item.Value == heater.Key.position)
                            {
                                double prog = System.Math.Round((SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / 100.0) * heater.Value.progress, 1);
                                if (prog > SnowCookingPlugin.Instance.Configuration.Instance.maxDegree)
                                {
                                    prog = SnowCookingPlugin.Instance.Configuration.Instance.maxDegree;
                                } else if (prog < 0)
                                {
                                    prog = 0;
                                }
                                EffectManager.sendUIEffectText(Convert.ToInt16(SnowCookingPlugin.Instance.Configuration.Instance.heaterUiId), uplayer.CSteamID, false, "Temperature_Degrees", prog.ToString() + SnowCookingPlugin.Instance.Translate("temperature_symbol"));
                            }
                        }
                    }
                }
            } catch(Exception ex) {
                Logger.Log(String.Format("EXCEPTION THROWN | ExNo 4 (Error message: {0})", ex.Message), ConsoleColor.Red);
                return;
            }
        }
    }
}
