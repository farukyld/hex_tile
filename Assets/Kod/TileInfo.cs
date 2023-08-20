using UnityEngine;
using System.Collections.Generic;

public class ChunkBilgi : MonoBehaviour
{
    public Vector2Int cellLocation; // Biome location in 2D array
    public Vector2Int tileIndex;    // Tile location in HexTiles 2D array
    public Element element;
    public ChunkBilgi[,] hexTilesReference;
    public List<ChunkBilgi> neighboringTiles;

    public void ExtractNeighbors()
    {
        neighboringTiles = new List<ChunkBilgi>();

        // Top and Bottom in the same column
        AddNeighbor(tileIndex.x, tileIndex.y - 1);
        AddNeighbor(tileIndex.x, tileIndex.y + 1);

        // Depending on if the current column is odd or even
        if (tileIndex.x % 2 == 0)
        {
            AddNeighbor(tileIndex.x - 1, tileIndex.y);
            AddNeighbor(tileIndex.x + 1, tileIndex.y);
            AddNeighbor(tileIndex.x - 1, tileIndex.y - 1);
            AddNeighbor(tileIndex.x + 1, tileIndex.y - 1);
        }
        else
        {
            AddNeighbor(tileIndex.x - 1, tileIndex.y);
            AddNeighbor(tileIndex.x + 1, tileIndex.y);
            AddNeighbor(tileIndex.x - 1, tileIndex.y + 1);
            AddNeighbor(tileIndex.x + 1, tileIndex.y + 1);
        }
    }

    private void AddNeighbor(int x, int y)
    {
        if (x >= 0 && x < hexTilesReference.GetLength(0) && y >= 0 && y < hexTilesReference.GetLength(1))
        {
            neighboringTiles.Add(hexTilesReference[x, y]);
        }
    }
}
