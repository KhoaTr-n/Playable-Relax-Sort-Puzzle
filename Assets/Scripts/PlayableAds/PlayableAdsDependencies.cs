using System;

namespace Engine.ShelfPuzzle
{
    public enum ShelfType
    {
        Common, // Có 3 Slot chứa trong cùng 1 Layer
        Single, // Chỉ có thể lấy Item ra
    }

    public class ShelfPuzzleInputData
    {
        public ShelfType Type;
        public int[][] Data = Array.Empty<int[]>();
    }
}

namespace Strategy.Level
{
    public interface ILevelAnimationStep
    {
        /// Nếu trạng thái Animation nào không cho phép chen ngang vào giữa chừng thì return false 
        bool CanInterrupt { get; }

        void Enter();
        void Update(float dt);
        void Exit();
    }
}
