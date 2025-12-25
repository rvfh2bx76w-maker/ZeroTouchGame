public readonly struct ItemStack
{
    public ItemDef Def { get; }
    public int Qty { get; }

    public ItemStack(ItemDef def, int qty)
    {
        Def = def;
        Qty = qty;
    }
}
