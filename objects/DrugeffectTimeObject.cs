namespace Ocelot.SnowCooking.objects
{
    public class DrugeffectTimeObject
    {
        public long time = SnowCookingPlugin.GetCurrentTime();
        public string playerId;
        public DrugeffectTimeObject(string playerId)
        {
            this.playerId = playerId;
        }
    }
}
