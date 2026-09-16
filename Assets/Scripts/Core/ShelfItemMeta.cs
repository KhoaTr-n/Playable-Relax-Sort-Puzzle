namespace Core
{
    public class ShelfItemMeta
    {
        /* Unique Id của Item */
        public readonly int Id;

        /* Loại Item */
        public readonly int TypeId;

        /* Trong GameScene sẽ có nhiều Shelf */
        public readonly int ShelfId;

        /* Trong 1 Shelf thì có nhiều Layer */
        public readonly int LayerId;

        /* Trong 1 layer thì có 3 slot */
        public readonly int SlotId;

        public ShelfItemMeta(
            int shelfId,
            int layerId,
            int slotId,
            int typeId,
            int id
        )
        {
            ShelfId = shelfId;
            LayerId = layerId;
            SlotId = slotId;
            TypeId = typeId;
            Id = id;
        }

        public ShelfItemMeta Change(int shelfId, int layerId, int slotId)
        {
            return new ShelfItemMeta(shelfId, layerId, slotId, TypeId, Id);
        }
        
        public ShelfItemMeta Change(int typeId)
        {
            return new ShelfItemMeta(ShelfId, LayerId, SlotId, typeId, Id);
        }
    }
}