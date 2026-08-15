using RPGame.Inventory.Data;

namespace RPGame.Loot
{
    public readonly struct LootResult
    {
        public LootResult(ItemDefinition item, int amount)
        {
            Item = item;
            Amount = amount;
        }

        public ItemDefinition Item { get; }
        public int Amount { get; }
    }
}
