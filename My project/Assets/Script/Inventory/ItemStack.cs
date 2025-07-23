[System.Serializable]
public struct ItemStack
{
    public int itemId;
    public int quantity;

    public ItemStack(int id, int qty)
    {
        itemId = id;
        quantity = qty;
    }
}
