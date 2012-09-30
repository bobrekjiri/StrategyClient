namespace StrategyClient
{
    class TileInfo
    {
        public TileType Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Population { get; set; }
        public VillageState State { get; set; }
        public string Owner { get; set; }
    }
}