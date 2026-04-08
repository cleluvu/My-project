[System.Serializable]
public class InventorySaveData
{
    public int itemID;
    public int slotIndex;
    public int quantity;

    public InventorySaveData(int id, int index, int qty)
    {
        itemID = id;
        slotIndex = index;
        quantity = qty;
    }
}