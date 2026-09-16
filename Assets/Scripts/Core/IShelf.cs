using Engine.ShelfPuzzle;
using Game;

namespace Core
{
    public interface IShelf
    {
        /**
         * Shelf ID
         */
        int Id { get; }

        /**
         * [
         *      [slot_0, slot_1, slot_2], // layer_0
         *      [slot_0, slot_1, slot_2], // layer_1
         * ]
         */
        int[][] ExportData();

        void ImportData(int id, ShelfPuzzleInputData input);
    }
}