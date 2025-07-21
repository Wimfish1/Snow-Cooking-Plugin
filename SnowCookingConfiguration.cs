using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Ocelot.SnowCooking
{
    public class SnowCookingConfiguration : IRocketPluginConfiguration
    {
        // CONFIG VARIABLES
        public string iconImageUrl;
        public ushort panId;
        public ushort panFilledId;
        public ushort cocaLeavesId;
        public ushort ashesId;
        public ushort dryingLampId;
        public ushort panPowderId;
        public ushort snowBagId;
        public uint heatingDurationSecs;
        public uint coolingDurationSecs;
        public uint dryingDurationSecs;
        public uint cookingDurationSecs;

        public int snowBagsMin;
        public int snowBagsMax;

        public double maxDegree;

        public string heaterColdColor;
        public string heaterHotColor;
        public int heaterHotDegree;
        public string heaterTooHotColor;
        public int heaterTooHotDegree;

        public bool UseDrugEffectSpeed;
        public ushort DrugEffectSpeedMultiplier;
        public bool UseDrugEffectJump;
        public ushort DrugEffectJumpMultiplier;
        public int DrugEffectDurationSecs;

        public ushort cocaLeavesBurnedEffectId;

        [XmlArrayItem(ElementName = "heaterId")]
        public List<ushort> heaterIds;
        public ushort heaterUiId;

        public void LoadDefaults()
        {
            iconImageUrl = "https://i.imgur.com/xDgvtP5.png";
            panId = 10203;
            panFilledId = 10204;
            panPowderId = 10208;
            cocaLeavesId = 10202;
            ashesId = 10206;
            dryingLampId = 10207;
            snowBagId = 10209;
            heatingDurationSecs = 40;
            coolingDurationSecs = 80;
            cookingDurationSecs = 30;
            dryingDurationSecs = 5;
            maxDegree = 200;
            heaterColdColor = "#33B8FF";
            heaterHotColor = "#FC620A";
            heaterTooHotColor = "#ff3c19";
            heaterHotDegree = 50;
            heaterTooHotDegree = 150;
            snowBagsMin = 1;
            snowBagsMax = 3;
            UseDrugEffectSpeed = true;
            DrugEffectSpeedMultiplier = 2;
            UseDrugEffectJump = true;
            DrugEffectJumpMultiplier = 2;
            DrugEffectDurationSecs = 30;
            cocaLeavesBurnedEffectId = 119;
            heaterIds = new List<ushort>() { 10205 };
            heaterUiId = 10110;
        }
    }
}
