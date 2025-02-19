namespace Ocelot.SnowCooking.objects
{
    public class DrugeffectTimeObject
    {
        public long time = SnowCookingPlugin.getCurrentTime();
        public string playerId;
        public DrugeffectTimeObject(string playerId)
        {
            this.playerId = playerId;
        }
    }
}
