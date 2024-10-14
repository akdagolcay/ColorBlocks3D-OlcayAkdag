using System.Collections.Generic;

namespace Managers.LevelSystem
{
    public class LevelConfig
    {
        public int MoveLimit;
        public int RowCount;
        public int ColCount;
        public List<CellInfo> CellInfo;
        public List<MovableInfo> MovableInfo;
        public List<ExitInfo> ExitInfo;
    }

    public class CellInfo
    {
        public int Row;
        public int Col;
    }

    public class MovableInfo
    {
        public int Row;
        public int Col;
        public List<int> Direction;
        public int Length;
        public int Colors;
    }

    public class ExitInfo
    {
        public int Row;
        public int Col;
        public int Direction;
        public int Colors;
    }
}