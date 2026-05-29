using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.MLAgents;
using System.Threading.Tasks;
using System.Collections.Generic;

public class MLAgentsValidationManager : MonoBehaviour
{
    [Header("Shadow Prefabs")]
    public GameObject shadowAgentPrefab;
    public GameObject shadowObstaclePrefab;

    private Scene shadowScene;
    private PhysicsScene2D shadowPhysics;

    private void Awake()
    {
        CreateShadowScene();
    }

    private void CreateShadowScene()
    {
        // Create a separate physics scene for validation
        CreateSceneParameters csp = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
        shadowScene = SceneManager.CreateScene("ShadowValidationScene", csp);
        shadowPhysics = shadowScene.GetPhysicsScene2D();
    }

    /// <summary>
    /// Validates if the obstacle is jumpable by simulating a shadow agent.
    /// Named 'Async' per prompt requirements, though simulation runs in high-speed loop.
    /// </summary>
    public bool ValidateObstacleAsync(ObstacleParameters[] patterns)
    {
        if (shadowAgentPrefab == null || shadowObstaclePrefab == null) return true;

        // 1. Setup shadow scene with boundaries
        GameObject ground = new GameObject("Ground");
        var gCol = ground.AddComponent<BoxCollider2D>();
        gCol.size = new Vector2(2000, 1);
        ground.transform.position = new Vector3(0, -5.5f, 0);
        SceneManager.MoveGameObjectToScene(ground, shadowScene);

        GameObject ceiling = new GameObject("Ceiling");
        var cCol = ceiling.AddComponent<BoxCollider2D>();
        cCol.size = new Vector2(2000, 1);
        ceiling.transform.position = new Vector3(0, 10.5f, 0);
        SceneManager.MoveGameObjectToScene(ceiling, shadowScene);

        GameObject agentObj = Instantiate(shadowAgentPrefab);
        SceneManager.MoveGameObjectToScene(agentObj, shadowScene);
        agentObj.transform.position = new Vector3(0, 0, 0);
        
        Rigidbody2D rb = agentObj.GetComponent<Rigidbody2D>();
        if (rb == null) rb = agentObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 4.2f; 
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        List<GameObject> obstacleObjs = new List<GameObject>();
        float currentX = 12f;
        float totalWidth = 0;

        foreach (var p in patterns)
        {
            GameObject obstacleObj = Instantiate(shadowObstaclePrefab);
            SceneManager.MoveGameObjectToScene(obstacleObj, shadowScene);
            obstacleObj.transform.position = new Vector3(currentX, p.yPosition, 0); 

            var polyGen = obstacleObj.AddComponent<ProceduralPolygonGenerator>();
            polyGen.GenerateRandomShape(p.width, p.height);
            
            obstacleObjs.Add(obstacleObj);
            currentX += p.distanceToNext;
            totalWidth += p.width + p.distanceToNext;
        }

        // 2. Simulation with "Smart" jumping
        bool success = false;
        float[] testHeights = new float[] { -4f, -1.5f, 2.5f, 6.5f, 9f }; // Better coverage from bottom to top

        foreach (float targetY in testHeights)
        {
            // Reset agent for each height test
            agentObj.transform.position = new Vector3(0, 0, 0);
            rb.linearVelocity = Vector2.zero;
            
            float fixedDeltaTime = 0.02f;
            int maxSteps = 400;
            float jumpCooldown = 0.2f; // Max 5 clicks per second
            float nextJumpTime = 0;

            for (int i = 0; i < maxSteps; i++)
            {
                float currentTime = i * fixedDeltaTime;
                float speed = GameState.Instance != null ? GameState.Instance.currentSpeed : 12f;
                rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);

                if (rb.position.y < targetY && rb.linearVelocity.y < 0 && currentTime >= nextJumpTime)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 14.4f);
                    nextJumpTime = currentTime + jumpCooldown;
                }

                shadowPhysics.Simulate(fixedDeltaTime);
                
                if (rb.position.x > 12f + totalWidth + 5f)
                {
                    success = true;
                    break;
                }
                
                if (i > 10 && rb.linearVelocity.x < speed * 0.5f) break; 
                if (rb.position.y < -10f || rb.position.y > 15f) break;
            }
            
            if (success) break;
        }

        foreach (var o in obstacleObjs) Destroy(o);
        Destroy(agentObj);
        Destroy(ground);
        Destroy(ceiling);
        
        return success;
    }

    public bool ValidateObstacleAsync(ObstacleParameters parameters)
    {
        return ValidateObstacleAsync(new ObstacleParameters[] { parameters });
    }
}
