using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    public GameObject hexTilePrefab; // Drag and drop your hex tile prefab here in inspector
    public int width = 10; // Number of tiles in x-direction
    public int height = 10; // Number of tiles in z-direction
    public float tileSpacing = 1.0f; // Gap between tiles, you can adjust this based on your hexagon's size
    public List<Material> materials;
    private void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                // Calculate the position for the hex tile
                float xPos = x * tileSpacing * 1.5f;
                float zPos = z * tileSpacing * Mathf.Sqrt(3);
                
                // If it's an odd column, offset it to fit hexagonal pattern
                if (x % 2 == 1)
                {
                    zPos += tileSpacing * Mathf.Sqrt(3) * 0.5f;
                }

                Vector3 pos = new Vector3(xPos, 0, zPos);
                var obj = Instantiate(hexTilePrefab, pos, Quaternion.identity, this.transform);
                var list = new List<Material>();
                list.Add(materials.RandomElement());
                list.Add(list[0]);
                obj.GetComponent<Renderer>().materials = list.ToArray();
                obj.gameObject.name = "tile" + x + " " + z;
            }
        }
    }
}
