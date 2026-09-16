using System.Collections.Generic;
using UnityEngine;

namespace PlayableAds
{
    /// <summary>
    /// Quản lý grid 2D cho Playable Ads.
    /// row 0 = bottom, row tăng lên trên.
    /// Khi shelf bị clear, CollapseColumn dồn shelf phía trên xuống (gravity).
    /// </summary>
    public class PlayableAdsGridData
    {
        private readonly int[,] _grid; // [col, row] → shelfId, -1 = empty
        private readonly Vector2[,] _cellPositions; // vị trí world-space cố định cho mỗi ô
        private readonly Dictionary<int, (int col, int row)> _shelfCells; // shelfId → vị trí hiện tại
        private readonly int _columnCount;
        private readonly int _rowCount;

        public int ColumnCount => _columnCount;
        public int RowCount => _rowCount;

        public PlayableAdsGridData(int columnCount, int rowCount)
        {
            _columnCount = columnCount;
            _rowCount = rowCount;
            _grid = new int[columnCount, rowCount];
            _cellPositions = new Vector2[columnCount, rowCount];
            _shelfCells = new Dictionary<int, (int col, int row)>();

            for (var c = 0; c < columnCount; c++)
            for (var r = 0; r < rowCount; r++)
                _grid[c, r] = -1;
        }

        /// Đăng ký shelf vào grid. Gọi khi setup level.
        public void RegisterShelf(int shelfId, int col, int row, Vector2 worldPosition)
        {
            _grid[col, row] = shelfId;
            _cellPositions[col, row] = worldPosition;
            _shelfCells[shelfId] = (col, row);
        }

        /// Lấy vị trí (col, row) của shelf trong grid.
        public (int col, int row) GetShelfCell(int shelfId)
        {
            return _shelfCells[shelfId];
        }

        /// Gọi khi shelf bị clear. Return danh sách shelf cần animate slide xuống.
        public List<SlideData> OnShelfCleared(int shelfId)
        {
            if (!_shelfCells.TryGetValue(shelfId, out var cell))
                return new List<SlideData>();

            _grid[cell.col, cell.row] = -1;
            _shelfCells.Remove(shelfId);
            return CollapseColumn(cell.col);
        }

        /// Gravity-style collapse: dồn tất cả shelf trong cột xuống lấp ô trống.
        private List<SlideData> CollapseColumn(int col)
        {
            var slides = new List<SlideData>();
            var writeRow = 0;

            for (var readRow = 0; readRow < _rowCount; readRow++)
            {
                var shelfId = _grid[col, readRow];
                if (shelfId < 0) continue;

                if (writeRow != readRow)
                {
                    slides.Add(new SlideData
                    {
                        ShelfId = shelfId,
                        FromPosition = _cellPositions[col, readRow],
                        ToPosition = _cellPositions[col, writeRow],
                        ToRow = writeRow
                    });

                    _grid[col, writeRow] = shelfId;
                    _grid[col, readRow] = -1;
                    _shelfCells[shelfId] = (col, writeRow);
                }

                writeRow++;
            }

            return slides;
        }
    }

    public struct SlideData
    {
        public int ShelfId;
        public Vector2 FromPosition;
        public Vector2 ToPosition;
        public int ToRow;
    }
}
