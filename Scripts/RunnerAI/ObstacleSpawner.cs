using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public SentisObstacleGenerator sentisGen;
    public MLAgentsValidationManager mlValidation;

    public float nextSpawnX = 10f;
    [SerializeField] private float spawnBuffer = 40f; // Distance ahead of player to spawn
    [SerializeField] private float despawnBuffer = 20f; // Distance behind player to despawn
    private GameObject player;

    private bool wasCountingDown = true;
    private List<GameObject> activeObstacles = new List<GameObject>();

    private void Awake()
    {
        Debug.Log("ObstacleSpawner Awake called");
        player = GameObject.FindWithTag("Player");
        
        // Ensure no stale obstacles from previous editor runs remain in the scene
        var existingObstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (var ob in existingObstacles) DestroyImmediate(ob);
        
        // Initialize nextSpawnX to be far away at start
        nextSpawnX = 100f; 
    }

    private void Start()
    {
        Debug.Log("ObstacleSpawner Start called");
        if (player == null) player = GameObject.FindWithTag("Player");
        if (player == null) Debug.LogError("ObstacleSpawner: Player still NOT FOUND in Start");
        else Debug.Log("ObstacleSpawner: Player initialized in Awake/Start");
    }

    private void Update()
    {
        if (GameState.Instance == null) return;
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player == null) return;
        }

        if (GameState.Instance.isCountingDown)
        {
            // Reset nextSpawnX and clear any pre-spawned objects to ensure a clean start
            nextSpawnX = player.transform.position.x + spawnBuffer;
            if (wasCountingDown == false) // Transitioning back to countdown (e.g. restart)
            {
                ClearAllObstacles();
            }
            wasCountingDown = true;
            return;
        }

        // Just finished countdown? Ensure we have a clean starting point
        if (wasCountingDown)
        {
            wasCountingDown = false;
            Debug.Log("Countdown ended. Starting spawns at X: " + nextSpawnX);
        }

        // Limit to 1 spawn per frame to prevent heavy physics spikes
        // Ensure we always have obstacles queued up outside the FOV
        if (nextSpawnX - player.transform.position.x < spawnBuffer + 20f)
        {
            TrySpawnObstacle();
        }

        CleanupObstacles();
    }

    private void CleanupObstacles()
    {
        // Remove obstacles that have passed the player and are off-screen
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] == null)
            {
                activeObstacles.RemoveAt(i);
                continue;
            }

            if (player.transform.position.x - activeObstacles[i].transform.position.x > despawnBuffer)
            {
                Destroy(activeObstacles[i]);
                activeObstacles.RemoveAt(i);
            }
        }
    }

    private enum PatternType { Single, Tunnel, Stacked, Cluster, Sequence, Slalom }
    private PatternType lastPattern = PatternType.Single;
    private int samePatternCount = 0;

    private void TrySpawnObstacle()
    {
        GameState.DifficultyLevel difficulty = GameState.Instance.currentDifficulty;
        PatternType nextType;

        // Forced variation logic: if we had the same pattern 2 times, pick a different one
        int maxAttempts = 10;
        do
        {
            float rand = Random.value;
            // Weights adjust based on difficulty
            float tunnelChance = 0.15f;
            float stackedChance = 0.15f;
            float clusterChance = 0.10f;
            float sequenceChance = 0.15f;
            float slalomChance = 0.15f;

            if (difficulty == GameState.DifficultyLevel.Medium)
            {
                tunnelChance = 0.20f;
                stackedChance = 0.15f;
                clusterChance = 0.15f;
                slalomChance = 0.20f;
            }
            else if (difficulty == GameState.DifficultyLevel.Hard)
            {
                tunnelChance = 0.25f;
                stackedChance = 0.20f;
                clusterChance = 0.15f;
                slalomChance = 0.25f;
            }
            else if (difficulty == GameState.DifficultyLevel.Impossible)
            {
                tunnelChance = 0.30f;
                stackedChance = 0.25f;
                clusterChance = 0.15f;
                slalomChance = 0.20f;
            }

            if (rand < tunnelChance) nextType = PatternType.Tunnel;
            else if (rand < tunnelChance + stackedChance) nextType = PatternType.Stacked;
            else if (rand < tunnelChance + stackedChance + clusterChance) nextType = PatternType.Cluster;
            else if (rand < tunnelChance + stackedChance + clusterChance + sequenceChance) nextType = PatternType.Sequence;
            else if (rand < tunnelChance + stackedChance + clusterChance + sequenceChance + slalomChance) nextType = PatternType.Slalom;
            else nextType = PatternType.Single;

            maxAttempts--;
        } while (nextType == lastPattern && samePatternCount >= 1 && maxAttempts > 0);

        if (nextType == lastPattern) samePatternCount++;
        else
        {
            lastPattern = nextType;
            samePatternCount = 1;
        }

        switch (nextType)
        {
            case PatternType.Tunnel: SpawnTunnel(); break;
            case PatternType.Stacked: SpawnStacked(); break;
            case PatternType.Cluster: SpawnCluster(); break;
            case PatternType.Sequence: SpawnCloseSequence(); break;
            case PatternType.Slalom: SpawnSlalom(); break;
            default: SpawnSingle(); break;
        }
    }

    private void SpawnSlalom()
    {
        int count = Random.Range(4, 7);
        float lastY = Random.Range(-3f, 7f);

        List<ObstacleParameters> slalom = new List<ObstacleParameters>();
        for (int i = 0; i < count; i++)
        {
            // Alternate Y position significantly
            float targetY = (lastY > 2f) ? Random.Range(-4.5f, -1f) : Random.Range(5f, 9.5f);
            float w = Random.Range(4f, 8f);
            float h = Random.Range(4f, 8f);
            float dist = Random.Range(18f, 25f);
            
            slalom.Add(new ObstacleParameters(w, h, dist, targetY));
            lastY = targetY;
        }

        if (mlValidation.ValidateObstacleAsync(slalom.ToArray()))
        {
            foreach (var p in slalom) Spawn(p, true);
        }
        else
        {
            nextSpawnX += 15f;
        }
    }

    private float lastSingleY = 0;

    private void SpawnSingle()
    {
        ObstacleParameters data = sentisGen.GenerateObstacleData();
        
        // Force variety in Y position for single obstacles
        // If last was high, go low, and vice versa
        if (lastSingleY > 3f) data.yPosition = Random.Range(-4.5f, 1f);
        else if (lastSingleY < 1f) data.yPosition = Random.Range(5f, 9.5f);
        else data.yPosition = (Random.value > 0.5f) ? Random.Range(6f, 9.5f) : Random.Range(-4.5f, -1f);
        
        lastSingleY = data.yPosition;

        if (mlValidation.ValidateObstacleAsync(data))
        {
            Spawn(data);
        }
        else
        {
            nextSpawnX += 3f;
        }
    }

    private void SpawnTunnel()
    {
        ObstacleParameters data = sentisGen.GenerateObstacleData();
        
        float tunnelWidth = data.width;
        float obstacleHeight = Random.Range(6f, 10f); 
        float gapSize = Random.Range(7.5f, 9.5f);
        float centerY = Random.Range(-2f, 3.5f);

        ObstacleParameters top = new ObstacleParameters(tunnelWidth, obstacleHeight, 0, centerY + (obstacleHeight * 0.5f + gapSize * 0.5f));
        // Use full distance for the next pattern to avoid overlap
        ObstacleParameters bottom = new ObstacleParameters(tunnelWidth, obstacleHeight, data.distanceToNext, centerY - (obstacleHeight * 0.5f + gapSize * 0.5f));

        if (mlValidation.ValidateObstacleAsync(new ObstacleParameters[] { top, bottom }))
        {
            Spawn(top, false);
            Spawn(bottom, true);
        }
        else
        {
            nextSpawnX += 8f;
        }
    }

    private void SpawnStacked()
    {
        ObstacleParameters data = sentisGen.GenerateObstacleData();
        
        float w = data.width;
        float h = data.height * 0.8f;
        float baseY = Random.Range(-3f, 1f);
        
        ObstacleParameters btm = new ObstacleParameters(w, h, 0, baseY);
        ObstacleParameters top = new ObstacleParameters(w, h, data.distanceToNext, baseY + h + Random.Range(7.5f, 10.5f));

        if (mlValidation.ValidateObstacleAsync(new ObstacleParameters[] { btm, top }))
        {
            Spawn(btm, false);
            Spawn(top, true);
        }
        else
        {
            nextSpawnX += 8f;
        }
    }

    private void SpawnCluster()
    {
        int count = Random.Range(6, 12); // Significantly more stones
        List<ObstacleParameters> cluster = new List<ObstacleParameters>();

        for (int i = 0; i < count; i++)
        {
            float w = Random.Range(2.5f, 6.5f);
            float h = Random.Range(2.5f, 6.5f);
            float dist = Random.Range(7f, 16f); 
            // Wider spread from near floor to near ceiling
            float y = Random.Range(-4.5f, 9.5f); 
            cluster.Add(new ObstacleParameters(w, h, dist, y));
        }

        if (mlValidation.ValidateObstacleAsync(cluster.ToArray()))
        {
            foreach (var p in cluster)
            {
                Spawn(p, true);
            }
        }
        else
        {
            nextSpawnX += 12f;
        }
    }

    private void SpawnCloseSequence()
    {
        ObstacleParameters data1 = sentisGen.GenerateObstacleData();
        data1.distanceToNext = Random.Range(10f, 15f); // Increased distance
        
        ObstacleParameters data2 = sentisGen.GenerateObstacleData();

        if (mlValidation.ValidateObstacleAsync(new ObstacleParameters[] { data1, data2 }))
        {
            Spawn(data1);
            Spawn(data2);
        }
        else
        {
            nextSpawnX += 8f;
        }
    }

    private void ClearAllObstacles()
    {
        foreach (var ob in activeObstacles)
        {
            if (ob != null) Destroy(ob);
        }
        activeObstacles.Clear();
    }

    private void Spawn(ObstacleParameters data, bool incrementX = true)
    {
        // Adjust distance based on difficulty to increase density
        float densityMult = 0.8f; // Global reduction
        if (GameState.Instance != null)
        {
            switch(GameState.Instance.currentDifficulty)
            {
                case GameState.DifficultyLevel.Easy: densityMult = 0.85f; break;
                case GameState.DifficultyLevel.Medium: densityMult = 0.7f; break;
                case GameState.DifficultyLevel.Hard: densityMult = 0.55f; break;
                case GameState.DifficultyLevel.Impossible: densityMult = 0.45f; break;
            }
        }

        // Use the Y position from parameters for mid-air spawning
        GameObject go = Instantiate(obstaclePrefab, new Vector3(nextSpawnX, data.yPosition, -0.1f), Quaternion.identity);
        
        // Tag immediately to ensure collision detection picks it up
        go.tag = "Obstacle"; 
        activeObstacles.Add(go);
        
        // IMPORTANT: Use DestroyImmediate to ensure the old collider is gone 
        // before the procedural one is added and processed by physics.
        var colliders = go.GetComponents<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            DestroyImmediate(colliders[i]);
        }

        // Add procedural shape generator
        var polyGen = go.AddComponent<ProceduralPolygonGenerator>();
        polyGen.GenerateRandomShape(data.width, data.height);
        
        if (incrementX)
        {
            nextSpawnX += data.distanceToNext * densityMult;
        }
    }
}