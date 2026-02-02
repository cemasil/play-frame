using System.Collections.Generic;
using UnityEngine;

namespace PlayFrame.MiniGames.Match3
{
    public class Match3Grid
    {
        private Gem[,] grid;
        private int width;
        private int height;

        public Match3Grid(int width, int height)
        {
            this.width = width;
            this.height = height;
            grid = new Gem[width, height];
        }

        public void SetGem(int x, int y, Gem gem)
        {
            if (IsValidPosition(x, y))
            {
                grid[x, y] = gem;
                gem.SetPosition(x, y);
            }
        }

        public Gem GetGem(int x, int y)
        {
            if (IsValidPosition(x, y))
                return grid[x, y];

            return null;
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public void SwapGems(Gem gem1, Gem gem2)
        {
            int x1 = gem1.X;
            int y1 = gem1.Y;
            int x2 = gem2.X;
            int y2 = gem2.Y;

            grid[x1, y1] = gem2;
            grid[x2, y2] = gem1;

            gem1.SetPosition(x2, y2);
            gem2.SetPosition(x1, y1);
        }

        public List<Gem> FindMatchesForGems(Gem gem1, Gem gem2)
        {
            List<Gem> matches = new List<Gem>();

            // Check matches around gem1
            AddMatchesAtPosition(gem1.X, gem1.Y, matches);

            // Check matches around gem2
            AddMatchesAtPosition(gem2.X, gem2.Y, matches);

            return matches;
        }

        private void AddMatchesAtPosition(int x, int y, List<Gem> matches)
        {
            Gem centerGem = grid[x, y];
            if (centerGem == null) return;

            int horizontalCount = 1;
            int leftX = x - 1;
            int rightX = x + 1;

            while (leftX >= 0 && grid[leftX, y] != null && grid[leftX, y].ColorIndex == centerGem.ColorIndex)
            {
                horizontalCount++;
                leftX--;
            }

            while (rightX < width && grid[rightX, y] != null && grid[rightX, y].ColorIndex == centerGem.ColorIndex)
            {
                horizontalCount++;
                rightX++;
            }

            if (horizontalCount >= 3)
            {
                for (int i = leftX + 1; i < rightX; i++)
                {
                    if (!matches.Contains(grid[i, y]))
                        matches.Add(grid[i, y]);
                }
            }

            int verticalCount = 1;
            int topY = y - 1;
            int bottomY = y + 1;

            while (topY >= 0 && grid[x, topY] != null && grid[x, topY].ColorIndex == centerGem.ColorIndex)
            {
                verticalCount++;
                topY--;
            }

            while (bottomY < height && grid[x, bottomY] != null && grid[x, bottomY].ColorIndex == centerGem.ColorIndex)
            {
                verticalCount++;
                bottomY++;
            }

            if (verticalCount >= 3)
            {
                for (int i = topY + 1; i < bottomY; i++)
                {
                    if (!matches.Contains(grid[x, i]))
                        matches.Add(grid[x, i]);
                }
            }
        }

        public void RemoveGem(Gem gem)
        {
            if (IsValidPosition(gem.X, gem.Y))
            {
                grid[gem.X, gem.Y] = null;
            }
        }
    }
}
