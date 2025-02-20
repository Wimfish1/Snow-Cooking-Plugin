using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Linq;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Ocelot.SnowCooking.functions
{
    public class HeaterFunctions
    {
        public static void OnGestureChanged(UnturnedPlayer player, EPlayerGesture gesture)
        {
            if (!Physics.Raycast(player.Player.look.aim.position, player.Player.look.aim.forward,
                    out var raycastHit, 2, RayMasks.BARRICADE)) return;
            var status = "<color=red>OFF</color>";
            foreach (var heater in SnowCookingPlugin.Instance.heaterList.ToList())
            {
                if (heater.Key != raycastHit.transform) continue;
                var progressBar = "";
                if (heater.Value.isActive)
                {
                    status = "<color=green>ON</color>";
                }

                switch ((SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / 100.0) * heater.Value.progress >= SnowCookingPlugin.Instance.Configuration.Instance.heaterTooHotDegree)
                {
                    //Sets the color of the progress bar
                    case true:
                        progressBar += "<color=" + SnowCookingPlugin.Instance.Configuration.Instance.heaterTooHotColor + ">";
                        break;
                    default:
                    {
                        if ((SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / 100.0) * heater.Value.progress >= SnowCookingPlugin.Instance.Configuration.Instance.heaterHotDegree)
                        {
                            progressBar += "<color=" + SnowCookingPlugin.Instance.Configuration.Instance.heaterHotColor + ">";
                        }
                        else
                        {
                            progressBar += "<color=" + SnowCookingPlugin.Instance.Configuration.Instance.heaterColdColor + ">";
                        }

                        break;
                    }
                }
                //Color end
                for (int i = 0; i < heater.Value.progress; i += 5)
                {
                    progressBar += "█";
                }
                progressBar += "</color>";
                var prog = System.Math.Round((SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / 100.0) * heater.Value.progress, 1);
                EffectManager.sendUIEffect(SnowCookingPlugin.Instance.Configuration.Instance.heaterUiId, Convert.ToInt16(SnowCookingPlugin.Instance.Configuration.Instance.heaterUiId), player.CSteamID, false, SnowCookingPlugin.Instance.Translate("temperature_title"), prog.ToString() + SnowCookingPlugin.Instance.Translate("temperature_symbol"), progressBar, status);
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
        
        public static void Update()
        {
            var heatProg = SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / SnowCookingPlugin.Instance.Configuration.Instance.heatingDurationSecs;

            var coolProg = SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / SnowCookingPlugin.Instance.Configuration.Instance.coolingDurationSecs;

            try
            {
                foreach (var heater in SnowCookingPlugin.Instance.heaterList.ToList())
                {
                    if (!heater.Key || heater.Value == null)
                        break;
                    var amountActiveGenerators = 0;
                    foreach (var Generator in PowerTool.checkGenerators(heater.Key.position, PowerTool.MAX_POWER_RANGE, ushort.MaxValue).ToList())
                    {
                        if (!heater.Key || !Generator.transform)
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
                    if (amountActiveGenerators != 0) continue;
                    if (heater.Value == null)
                        break;
                    if (!heater.Value.isActive) continue;
                    heater.Value.isActive = false;
                    foreach (var player in SnowCookingPlugin.Instance.heaterUiOpened.ToList())
                    {
                        if (player.Value == heater.Key.position)
                        {
                            EffectManager.sendUIEffectText(Convert.ToInt16(SnowCookingPlugin.Instance.Configuration.Instance.heaterUiId), new CSteamID(ulong.Parse(player.Key)), false, "cocaineplugin.toggletext", "<color=red>OFF</color>");
                        }
                    }
                    //
                }
            } catch (Exception ex)
            {
                Logger.Log($"EXCEPTION THROWN | ExNo 1 (Error message: {ex.Message})", ConsoleColor.Red);
                return;
            }

            try
            {
                foreach (var heater in SnowCookingPlugin.Instance.heaterList.ToList())
                {
                    if (!heater.Key || heater.Value == null)
                        break;
                    switch (heater.Value.isActive)
                    {
                        case true:
                        {
                            if (heater.Value.progress < 100.0)
                            {
                                if (heater.Value.progress + heatProg > 100.0)
                                {
                                    heater.Value.progress = 100 - heatProg;
                                }
                                heater.Value.progress += heatProg;
                            }

                            break;
                        }
                        case false:
                        {
                            if (heater.Value.progress > 0)
                            {
                                heater.Value.progress -= coolProg;
                            }

                            break;
                        }
                    }
                }
            } catch (Exception ex)
            {
                Logger.Log($"EXCEPTION THROWN | ExNo 2 (Error message: {ex.Message})", ConsoleColor.Red);
                return;
            }

            try
            {
                foreach (var heater in SnowCookingPlugin.Instance.heaterList.ToList())
                {
                    if (!heater.Key || heater.Value == null)
                        break;
                    if (heater.Value.progress < 0 || heater.Value.progress > SnowCookingPlugin.Instance.Configuration.Instance.maxDegree)
                    {
                        return;
                    }

                    if (!BarricadeManager.tryGetInfo(heater.Key, out byte x, out byte y, out ushort plant,
                            out ushort index, out BarricadeRegion region)) continue;
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
            } catch (Exception ex)
            {
                Logger.Log($"EXCEPTION THROWN | ExNo 3 (Error message: {ex.Message})", ConsoleColor.Red);
                return;
            }

            try {
                foreach (var player in Provider.clients.ToList())
                {
                    if (player == null)
                        break;
                    var uplayer = UnturnedPlayer.FromSteamPlayer(player);
                    foreach (var item in SnowCookingPlugin.Instance.heaterUiOpened.ToList())
                    {
                        foreach (var heater in SnowCookingPlugin.Instance.heaterList.ToList())
                        {
                            if (!heater.Key)
                                break;
                            if (item.Value != heater.Key.position) continue;
                            string progressBar = "";
                            //Sets the color of the progress bar
                            if ((SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / 100.0) * heater.Value.progress >= SnowCookingPlugin.Instance.Configuration.Instance.heaterTooHotDegree)
                            {
                                progressBar += "<color=" + SnowCookingPlugin.Instance.Configuration.Instance.heaterTooHotColor + ">";
                            }
                            else if ((SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / 100.0) * heater.Value.progress >= SnowCookingPlugin.Instance.Configuration.Instance.heaterHotDegree)
                            {
                                progressBar += "<color=" + SnowCookingPlugin.Instance.Configuration.Instance.heaterHotColor + ">";
                            }
                            else
                            {
                                progressBar += "<color=" + SnowCookingPlugin.Instance.Configuration.Instance.heaterColdColor + ">";
                            }
                            //Color end
                            for (int i = 0; i < heater.Value.progress; i += 5)
                            {
                                progressBar += "█";
                            }
                            progressBar += "</color>";
                            double prog = System.Math.Round((SnowCookingPlugin.Instance.Configuration.Instance.maxDegree / 100.0) * heater.Value.progress, 1);
                            if (prog > SnowCookingPlugin.Instance.Configuration.Instance.maxDegree)
                            {
                                prog = SnowCookingPlugin.Instance.Configuration.Instance.maxDegree;
                            } else if (prog < 0)
                            {
                                prog = 0;
                            }
                            EffectManager.sendUIEffectText(Convert.ToInt16(SnowCookingPlugin.Instance.Configuration.Instance.heaterUiId), uplayer.CSteamID, false, "cocaineplugin.tempprogress", progressBar);
                            EffectManager.sendUIEffectText(Convert.ToInt16(SnowCookingPlugin.Instance.Configuration.Instance.heaterUiId), uplayer.CSteamID, false, "cocaineplugin.tempdeg", prog.ToString() + SnowCookingPlugin.Instance.Translate("temperature_symbol"));
                        }
                    }
                }
            } catch(Exception ex) {
                Logger.Log($"EXCEPTION THROWN | ExNo 4 (Error message: {ex.Message})", ConsoleColor.Red);
                return;
            }
        }
    }
}
