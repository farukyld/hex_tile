using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileGenerator : MonoBehaviour
{
    public int seed = 123456;
    public bool debugTilePos;
    public bool debugCellPos;
    [Header("Biome Set Up")]
    public Transform cellPositionParent;
    public int biomeWidth;  // Width in number of cells
    public int biomeHeigth;  // Height in number of cells
    public float biomeSize;  // The expected edge length of a Voronoi cell
    [Range(min: 0f, max: .5f)] public float voronoiDistortionFactor = 0.5f;  // The range to shift the Voronoi points
    private Vector3[,] featurePoints;  // 2D array of Voronoi feature points


    [Header("Tile Grid Set Up")]
    public GameObject hexTilePrefab;
    public Transform tilePositionParent;
    public Transform tileParent;
    public int tileHeigth;
    public int tileWidth;
    public float tileSpacing;
    public float defaultTileProbability = .1f;

    private void Start()
    {
        Random.InitState(seed);

        GenerateVoronoiPoints();

        AssignInitialBiomes();

        GenerateGrid();

        ScatterDefaultTiles();

        AssignTileBiomeAppearances();

    }
    void GenerateVoronoiPoints()
    {
        featurePoints = new Vector3[biomeWidth, biomeHeigth];

        for (int x = 0; x < biomeWidth; x++)
        {
            for (int y = 0; y < biomeHeigth; y++)
            {
                // Calculate the base position for each point (center of each cell)
                Vector3 basePos = new Vector3(x * biomeSize, 0, y * biomeSize) - new Vector3(biomeWidth * biomeSize * 0.5f, 0, biomeHeigth * biomeSize * 0.5f) + new Vector3(biomeSize * .5f, 0, biomeSize * .5f); ;


                // If it's the four center points, don't distort them
                if ((x == biomeWidth / 2 || x == biomeWidth / 2 - 1) && (y == biomeHeigth / 2 || y == biomeHeigth / 2 - 1))
                {
                    featurePoints[x, y] = basePos;
                }
                else
                {
                    // Shift the position based on the distortion factor
                    Vector3 randomOffset = new Vector3(Random.Range(-voronoiDistortionFactor, voronoiDistortionFactor), 0, Random.Range(-voronoiDistortionFactor, voronoiDistortionFactor)) * biomeSize;
                    featurePoints[x, y] = basePos + randomOffset;
                }
                if (debugCellPos)
                {
                    var debug = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    debug.transform.position = featurePoints[x, y];
                    debug.transform.localScale = Vector3.one * 1f;
                    debug.transform.parent = cellPositionParent;
                    debug.name = "Cell Center (" + x + ", " + y + ")";
                }

            }
        }
    }


    private Vector2Int DetermineVoronoiCell(Vector3 position)
    {
        //!!!! --->> clamp the position inside the area allocated for featurepoints here.

        // Define the boundaries of the shifted area
        float minX = -biomeWidth * biomeSize * 0.5f;
        float maxX = biomeWidth * biomeSize * 0.5f;
        float minZ = -biomeHeigth * biomeSize * 0.5f;
        float maxZ = biomeHeigth * biomeSize * 0.5f;
        print(minX + ", "+ maxX + ", "+  minZ + ", "+ maxZ);
        print(position);

        // Clamp the position inside the shifted area allocated for feature points
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, minZ, maxZ);


        // Convert the continuous space into grid indices
        int x = Mathf.FloorToInt(position.x / biomeSize) + biomeWidth / 2; // Using size because it's the expected edge length of a Voronoi cell
        int y = Mathf.FloorToInt(position.z / biomeSize) + biomeHeigth / 2;

        print(x + ", " + y);
        // Find the neighboring voronoi points in the grid
        Vector2Int[] surroundingPoints = new Vector2Int[]
        {
        new Vector2Int(x, y),
        new Vector2Int(x+1, y),
        new Vector2Int(x-1, y),
        new Vector2Int(x, y+1),
        new Vector2Int(x, y-1),
        new Vector2Int(x+1, y+1),
        new Vector2Int(x-1, y-1),
        new Vector2Int(x-1, y+1),
        new Vector2Int(x+1, y-1)
        };

        float minDistance = float.MaxValue;
        Vector2Int closestCell = new Vector2Int(-1, -1);

        // Determine the closest voronoi point
        foreach (var point in surroundingPoints)
        {
            if (point.x >= 0 && point.x < biomeWidth && point.y >= 0 && point.y < biomeHeigth)
            {
                float dist = Vector3.Distance(position, featurePoints[point.x, point.y]);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestCell = point;
                }
            }
        }
        return closestCell;
    }



    private Biome[,] initialLocationBiomes;

    void AssignInitialBiomes()
    {
        initialLocationBiomes = new Biome[biomeWidth, biomeHeigth];
        List<Biome> centerBiomes = new List<Biome> { Biome.Terra, Biome.Water, Biome.Fire, Biome.Auro };

        for (int x = 0; x < biomeWidth; x++)
        {
            for (int y = 0; y < biomeHeigth; y++)
            {
                // If it's the four center points, assign them a unique biome
                if ((x == biomeWidth / 2 || x == biomeWidth / 2 - 1) && (y == biomeHeigth / 2 || y == biomeHeigth / 2 - 1))
                {
                    int randomIndex = Random.Range(0, centerBiomes.Count);
                    initialLocationBiomes[x, y] = centerBiomes[randomIndex];
                    centerBiomes.RemoveAt(randomIndex);  // Remove the biome to ensure uniqueness
                }
                else
                {
                    // For other points, assign a random biome (excluding the Default biome)
                    initialLocationBiomes[x, y] = (Biome)Random.Range(1, Enum.GetValues(typeof(Biome)).Length);
                }
            }
        }
    }





    TileInfo[,] hexTiles;

    void GenerateGrid()
    {
        hexTiles = new TileInfo[tileWidth, tileHeigth];

        for (int x = 0; x < tileWidth; x++)
        {
            for (int y = 0; y < tileHeigth; y++)
            {
                // Calculate the position for the hex tile
                float xPos = x * tileSpacing * 1.5f;
                float zPos = y * tileSpacing * Mathf.Sqrt(3);

                // If it's an odd column, offset it to fit hexagonal pattern
                if (x % 2 == 1)
                {
                    zPos += tileSpacing * Mathf.Sqrt(3) * 0.5f;
                }

                Vector3 pos = new Vector3(xPos, 0, zPos);
                pos -= new Vector3(tileWidth * tileSpacing * 0.75f, 0, tileHeigth * tileSpacing * Mathf.Sqrt(3) * 0.5f);

                var obj = Instantiate(hexTilePrefab, pos, Quaternion.identity, this.transform);
                obj.gameObject.name = "tile" + x + " " + y;

                var tileInfo = obj.GetComponent<TileInfo>();
                tileInfo.tileIndex = new Vector2Int(x, y);
                tileInfo.hexTilesReference = hexTiles;

                Vector2Int cellLocation = DetermineVoronoiCell(pos); // !!!! --->> this part may return outside of range index for now. 

                if (debugTilePos)
                {
                    var debug = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    debug.transform.parent = this.transform;
                    debug.transform.position = pos;
                    var m = debug.GetComponent<Renderer>().material;
                    m.color = Color.red;
                    debug.GetComponent<Renderer>().material = m;
                    debug.name = "tile position: (" + x + ", " + y + ")";
                    debug.transform.parent = tilePositionParent; 
                }
                tileInfo.cellLocation = cellLocation;
                tileInfo.tileType = initialLocationBiomes[cellLocation.x, cellLocation.y];
                tileInfo.transform.parent = tileParent;
                hexTiles[x, y] = tileInfo;
            }
        }

        // Extract neighbors for each tile
        for (int x = 0; x < tileWidth; x++)
        {
            for (int y = 0; y < tileHeigth; y++)
            {
                hexTiles[x, y].ExtractNeighbors();
            }
        }
    }




    public float level2StartingRadius = 40f; // Adjust these values to determine when each level starts.
    public float level3StartingRadius = 80f;
    public float fadeWidth = 15f; // The width of the fading region. Adjust this for a broader/narrower fade.

    public List<Material> lv1Materials;
    public List<Material> lv2Materials;
    public List<Material> lv3Materials;

    List<Material> GetMaterialsBasedOnDistance(float distance)
    {
        float distanceToLevel2Boundary = Math.Abs(distance - level2StartingRadius);
        float distanceToLevel3Boundary = Math.Abs(distance - level3StartingRadius);

        if (distanceToLevel2Boundary < fadeWidth)
        {
            // Probabilistic decision based on how close we are to the boundary
            float distanceToFadeBeginning = distance - (level2StartingRadius - fadeWidth);
            float fadeFactor = distanceToFadeBeginning / (fadeWidth * 2);
            return UnityEngine.Random.value > fadeFactor ? lv1Materials : lv2Materials;
        }
        else if (distanceToLevel3Boundary < fadeWidth)
        {
            float distanceToFadeBeginning = distance - (level3StartingRadius - fadeWidth);
            float fadeFactor = distanceToFadeBeginning / (fadeWidth * 2);
            return UnityEngine.Random.value > fadeFactor ? lv2Materials : lv3Materials;
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

    void ScatterDefaultTiles()
    {
        // 1. Scattering Default Type Tiles:
        foreach (var tile in hexTiles)
        {
            // Check if the tile should be converted to Default
            if (Random.value < defaultTileProbability)
            {
                tile.tileType = Biome.Default;
            }
        }

        // 2. Making the Middle Tile Default:
        hexTiles[tileWidth / 2, tileHeigth / 2].tileType = Biome.Default;
    }

    void AssignTileBiomeAppearances()
    {
        Vector3 center = Vector3.zero;

        for (int x = 0; x < tileWidth; x++)
        {
            for (int y = 0; y < tileHeigth; y++)
            {
                TileInfo tile = hexTiles[x, y];

                // Calculate the distance from the tile to the center
                float distance = Vector3.Distance(tile.transform.position, center);

                // Get the materials based on distance
                List<Material> materials = GetMaterialsBasedOnDistance(distance);


                print((int)tile.tileType);
                // Choose a specific material from the list based on the tile's biome. 
                Material assignedMaterial = materials[(int)tile.tileType];

                var doubleMaterial = new List<Material>() { assignedMaterial, assignedMaterial };
                tile.GetComponent<Renderer>().materials = doubleMaterial.ToArray();
            }
        }
    }


    public enum Biome { Default, Terra, Water, Fire, Auro }

}
