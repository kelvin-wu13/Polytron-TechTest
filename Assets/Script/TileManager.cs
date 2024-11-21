using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class TileManager : MonoBehaviour
{
    [Header("Tile Prefabs")]
    public GameObject[] tilePrefabs; // Array of 7 different tile types
    
    [Header("Spawn Prefabs")]
    public GameObject treePrefab;
    public GameObject housePrefab;
    
    [Header("UI Elements")]
    public TMP_Text scoreText;

    [Header("Grid Settings")]
    public int gridSize = 8;

    [Header("Layer Settings")]
    public int interactableLayer = 6;  // Layer 6: Interactable
    public int fullLayer = 7;           // Layer 7: Full

    // Internal tracking
    private GameObject[,] tileGrid;
    private int currentScore = 0;

    void Start()
    {
        // Initialize the grid
        tileGrid = new GameObject[gridSize, gridSize];
        GenerateRandomGrid();
        
        // Start tree planting coroutine
        StartCoroutine(PlantTreesRoutine());
        
        // Update score text
        UpdateScoreText();
    }

    void GenerateRandomGrid()
    {
        // Tracking to ensure at least one of each tile type
        bool[] tileTypeUsed = new bool[tilePrefabs.Length];
        
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                // Randomly select a tile, ensuring each type is used at least once
                int tileIndex;
                do
                {
                    tileIndex = Random.Range(0, tilePrefabs.Length);
                } while (tileTypeUsed[tileIndex] && !AllTileTypesUsed(tileTypeUsed));

                tileTypeUsed[tileIndex] = true;

                // Instantiate the tile
                GameObject newTile = Instantiate(
                    tilePrefabs[tileIndex], 
                    new Vector3(x, 0, y), 
                    Quaternion.identity
                );
                
                // Set initial layer to Interactable
                newTile.layer = interactableLayer;
                
                // Store in grid
                tileGrid[x, y] = newTile;
            }
        }
    }

    bool AllTileTypesUsed(bool[] usedTypes)
    {
        foreach (bool used in usedTypes)
        {
            if (!used) return false;
        }
        return true;
    }

    IEnumerator PlantTreesRoutine()
    {
        int treeCount = 0;
        while (HasEmptyDirtTileForTree() && treeCount < gridSize)
        {
            yield return new WaitForSeconds(1f);
            if(PlantTreeOnDirtTile())
            {
                treeCount++;
            }
        }
        Debug.Log("Tree is finished planting");
    }

    bool HasEmptyDirtTileForTree()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject tile = tileGrid[x, y];
                if (IsDirtTile(tile) && tile.layer == interactableLayer)
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool PlantTreeOnDirtTile()
    {
        List<Vector3> emptyDirtPositions = new List<Vector3>();
        List<GameObject> availableTiles = new List<GameObject>();

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject tile = tileGrid[x, y];
                if (IsDirtTile(tile) && tile.layer == interactableLayer)
                {
                    emptyDirtPositions.Add(tile.transform.position + Vector3.up * 0.07f);
                    availableTiles.Add(tile);
                }
            }
        }

        if (emptyDirtPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, emptyDirtPositions.Count);
            Vector3 randomPos = emptyDirtPositions[randomIndex];
            
            // Instantiate tree
            Instantiate(treePrefab, randomPos, Quaternion.identity);
            
            // Change tile layer to Full
            availableTiles[randomIndex].layer = fullLayer;
            
            return true;
        }
        return false;
    }

    void Update()
    {
        // Handle house spawning on mouse click
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                SpawnHouseOnTile(hit.collider.gameObject);
            }
        }
    }

    void SpawnHouseOnTile(GameObject tile)
    {
        // Check if tile is Dirt or Desert and is on Interactable layer
        if ((IsDirtTile(tile) || IsDesertTile(tile)) && tile.layer == interactableLayer)
        {
            Vector3 housePosition = tile.transform.position + Vector3.up * 0.123f;
            Instantiate(housePrefab, housePosition, Quaternion.identity);

            // Change tile layer to Full
            tile.layer = fullLayer;

            // Calculate and update score
            UpdateScore(tile);
        }
    }

    void UpdateScore(GameObject tile)
    {
        if (IsDirtTile(tile))
        {
            currentScore += 10;
        }
        else if (IsDesertTile(tile))
        {
            currentScore += 2;
        }
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }

    // Tile type check helpers
    bool IsDirtTile(GameObject tile)
    {
        return tile.name.Contains("Dirt") || tile.name.Contains("dirt");
    }

    bool IsDesertTile(GameObject tile)
    {
        return tile.name.Contains("Desert") || tile.name.Contains("desert");
    }
}