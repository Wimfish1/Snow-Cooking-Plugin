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
            var uplayer = UnturnedPlayer.FromPlayer(player);
            switch (buttonName)
            {
                case "cocaineplugin.exit" when uplayer == null:
                    return;
                case "cocaineplugin.exit":
                {
                    uplayer.Player.disablePluginWidgetFlag(EPluginWidgetFlags.Modal);
                    EffectManager.askEffectClearByID(SnowCookingPlugin.Instance.Configuration.Instance.heaterUiId, uplayer.CSteamID);
                    if (SnowCookingPlugin.Instance.heaterUiOpened.ContainsKey(uplayer.Id))
                    {
                        SnowCookingPlugin.Instance.heaterUiOpened.Remove(uplayer.Id);
                    }

                    break;
                }
                case "cocaineplugin.toggle":
                {
                    for (var index = 0; index < SnowCookingPlugin.Instance.heaterUiOpened.ToList().Count; index++)
                    {
                        var item = SnowCookingPlugin.Instance.heaterUiOpened.ToList()[index];
                        if (item.Key == null || uplayer == null)
                            break;
                        if (item.Key != uplayer.Id) continue;
                        for (var i = 0; i < SnowCookingPlugin.Instance.heaterList.ToList().Count; i++)
                        {
                            var heater = SnowCookingPlugin.Instance.heaterList.ToList()[i];
                            if (heater.Key == null)
                                break;
                            var list = PowerTool.checkGenerators(heater.Key.position,
                                         PowerTool.MAX_POWER_RANGE, ushort.MaxValue);
                            for (var index1 = 0; index1 < list.Count; index1++)
                            {
                                var generator = list[index1];
                                if (generator == null || heater.Key == null)
                                    break;
                                if (generator.fuel <= 0 || !generator.isPowered || !(generator.wirerange >=
                                        (heater.Key.position - generator.transform.position).magnitude)) continue;
                                if (item.Key == null || heater.Key == null)
                                    break;
                                if (heater.Key.position != item.Value) continue;
                                var status = "<color=green>ON</color>";
                                if (heater.Value.isActive)
                                {
                                    status = "<color=red>OFF</color>";
                                    heater.Value.isActive = false;
                                }
                                else
                                {
                                    heater.Value.isActive = true;
                                }

                                EffectManager.sendUIEffectText(
                                    Convert.ToInt16(SnowCookingPlugin.Instance.Configuration.Instance
                                        .heaterUiId), uplayer.CSteamID, false, "cocaineplugin.toggletext",
                                    status);
                            }
                        }
                    }

                    break;
                }
            }
        }
    }
}
