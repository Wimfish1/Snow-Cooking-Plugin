using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Linq;

namespace Ocelot.SnowCooking.functions
{
    public class UiFunctions
    {
        public static void ButtonClick(Player player, string buttonName)
        {
            if (player == null)
                return;
            UnturnedPlayer uplayer = UnturnedPlayer.FromPlayer(player);
            if (buttonName == "cocaineplugin.exit")
            {
                if (uplayer == null)
                    return;
                uplayer.Player.disablePluginWidgetFlag(EPluginWidgetFlags.Modal);
                EffectManager.askEffectClearByID(SnowCookingPlugin.Instance.Configuration.Instance.heaterUiId, uplayer.CSteamID);
                if (SnowCookingPlugin.Instance.heaterUiOpened.ContainsKey(uplayer.Id))
                {
                    SnowCookingPlugin.Instance.heaterUiOpened.Remove(uplayer.Id);
                }
            }
            if (buttonName == "cocaineplugin.toggle")
            {
                foreach (var item in SnowCookingPlugin.Instance.heaterUiOpened.ToList())
                {
                    if (item.Key == null || uplayer == null)
                        break;
                    if (item.Key == uplayer.Id)
                    {
                        foreach (var heater in SnowCookingPlugin.Instance.heaterList.ToList())
                        {
                            if (heater.Key == null)
                                break;
                            foreach (var Generator in PowerTool.checkGenerators(heater.Key.position, PowerTool.MAX_POWER_RANGE, ushort.MaxValue))
                            {
                                if (Generator == null || heater.Key == null)
                                    break;
                                if (Generator.fuel > 0 && Generator.isPowered && Generator.wirerange >= (heater.Key.position - Generator.transform.position).magnitude)
                                {
                                    if (item.Key == null || item.Value == null || heater.Key == null)
                                        break;
                                    if (heater.Key.position == item.Value)
                                    {
                                        string status = "<color=green>ON</color>";
                                        if (heater.Value.isActive)
                                        {
                                            status = "<color=red>OFF</color>";
                                            heater.Value.isActive = false;
                                        }
                                        else
                                        {
                                            heater.Value.isActive = true;
                                        }
                                        if (uplayer == null)
                                            break;
                                        EffectManager.sendUIEffectText(Convert.ToInt16(SnowCookingPlugin.Instance.Configuration.Instance.heaterUiId), uplayer.CSteamID, false, "cocaineplugin.toggletext", status);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
