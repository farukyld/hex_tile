using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    public float noiseScale = 0.1f; // Adjust for smoother or more jagged transitions
    public float defaultThreshold = 0.05f; // Value under which a tile is set as default (unexplored)

    public float level2StartingRadius = 20f; // Adjust these values to determine when each level starts.
    public float level3StartingRadius = 40f;
    public float fadeWidth = 5f; // The width of the fading region. Adjust this for a broader/narrower fade.

    public GameObject hexTilePrefab; // Drag and drop your hex tile prefab here in inspector
    public int width = 10; // Number of tiles in x-direction
    public int height = 10; // Number of tiles in z-direction
    public float tileSpacing = 1.0f; // Gap between tiles, you can adjust this based on your hexagon's size
    public List<Material> lv1Materials;
    public List<Material> lv2Materials;
    public List<Material> lv3Materials;



    private void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        Vector3 center = new Vector3(width * tileSpacing * 0.75f, 0, height * tileSpacing * Mathf.Sqrt(3) * 0.5f); // Approximate center


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
                obj.gameObject.name = "tile" + x + " " + z;

                float distanceToCenter = Vector3.Distance(pos, center);
                List<Material> currentMaterials = GetMaterialsBasedOnDistance(distanceToCenter);



                Biome biome = GetBiomeForPosition(xPos, zPos); 
                Material tileMaterial = currentMaterials[(int)biome];

                var list = new List<Material>() { tileMaterial, tileMaterial };
                obj.GetComponent<Renderer>().materials = list.ToArray();
            }
        }
    }

    List<Material> GetMaterialsBasedOnDistance(float distance)
    {
        float distanceToLevel2Boundary = Math.Abs(distance - level2StartingRadius);
        float distanceToLevel3Boundary = Math.Abs(distance - level3StartingRadius);

        if (distanceToLevel2Boundary < fadeWidth)
        {
            // Probabilistic decision based on how close we are to the boundary
            float fadeFactor = distanceToLevel2Boundary / fadeWidth;
            return UnityEngine.Random.value < fadeFactor ? lv1Materials : lv2Materials;
        }
        else if (distanceToLevel3Boundary < fadeWidth)
        {
            float fadeFactor = distanceToLevel3Boundary / fadeWidth;
            return UnityEngine.Random.value < fadeFactor ? lv2Materials : lv3Materials;
        }
        else if (distance < level2StartingRadius)
        {
            return lv1Materials;
        }
        else if (distance < level3StartingRadius)
        {
            return lv2Materials;
        }
        else
        {
            return lv3Materials;
        }
    }


    Biome GetBiomeForPosition(float x, float z)
    {
        float noiseValue = Mathf.PerlinNoise(x * noiseScale, z * noiseScale);

        if (noiseValue < defaultThreshold) return Biome.Default;

        float increment = 1f / (Enum.GetValues(typeof(Biome)).Length - 1);
        int biomeIndex = Mathf.Clamp(Mathf.FloorToInt(noiseValue / increment), 1, lv1Materials.Count - 1);

        return (Biome)biomeIndex;
    }


    public enum Biome { Default, Terra, Water, Fire, Auro }

}
