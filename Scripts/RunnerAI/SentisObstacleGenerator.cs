using UnityEngine;
using Unity.InferenceEngine;

public class SentisObstacleGenerator : MonoBehaviour
{
    public ModelAsset modelAsset;
    private Model runtimeModel;
    private Worker worker;

    private void Start()
    {
        if (modelAsset != null)
        {
            runtimeModel = ModelLoader.Load(modelAsset);
            // In Unity 6 Inference Engine, use 'new Worker'
            worker = new Worker(runtimeModel, BackendType.GPUCompute);
        }
    }

    public ObstacleParameters GenerateObstacleData()
    {
        // Fallback data if no model is assigned or worker fails
        if (worker == null || GameState.Instance == null)
        {
            float difficultyMult = 1f;
            float minGap = 4f; 

            switch(GameState.Instance.currentDifficulty)
            {
                case GameState.DifficultyLevel.Easy: 
                    difficultyMult = 1.0f; 
                    minGap = 8.5f; 
                    break;
                case GameState.DifficultyLevel.Medium: 
                    difficultyMult = 1.4f; 
                    minGap = 6.5f; 
                    break;
                case GameState.DifficultyLevel.Hard: 
                    difficultyMult = 1.8f; 
                    minGap = 5.0f; 
                    break;
                case GameState.DifficultyLevel.Impossible: 
                    difficultyMult = 2.4f; 
                    minGap = 4.0f; 
                    break;
            }

            // Variety Logic: Roll for "Type" of meteorite size
            float sizeRoll = UnityEngine.Random.value;
            float width, height;

            if (sizeRoll < 0.2f) // Small/Micro
            {
                width = UnityEngine.Random.Range(1.2f, 4f);
                height = UnityEngine.Random.Range(1.2f, 4f);
            }
            else if (sizeRoll > 0.75f && GameState.Instance.currentDifficulty != GameState.DifficultyLevel.Easy) // Huge/Giant
            {
                width = UnityEngine.Random.Range(12f, 22f) * (difficultyMult * 0.75f);
                height = UnityEngine.Random.Range(12f, 22f) * (difficultyMult * 0.75f);
            }
            else // Standard
            {
                width = UnityEngine.Random.Range(4f, 9f) * difficultyMult;
                height = UnityEngine.Random.Range(4f, 9f) * difficultyMult;
            }

            // Clamp height to ensure passage is physically possible
            float maxHeight = 16f - minGap; 
            height = Mathf.Min(height, maxHeight);
            
            // Tightened distances for a fuller level
            float minDistance = (GameState.Instance.currentSpeed * 0.45f) + 4f; 
            float distance = Mathf.Max(UnityEngine.Random.Range(10f, 22f) / (difficultyMult * 0.8f), minDistance);
            
            // Reduced jitter to keep patterns tighter
            distance *= UnityEngine.Random.Range(0.85f, 1.15f);

            // True random Y distribution
            float y = UnityEngine.Random.Range(-4.8f, 9.8f);

            return new ObstacleParameters(width, height, distance, y);
        }

        // Input: [currentSpeed, difficultyMetric]
        float[] inputData = new float[] { GameState.Instance.currentSpeed, GameState.Instance.difficultyMetric };
        using Tensor<float> inputTensor = new Tensor<float>(new TensorShape(1, 2), inputData);
        
        worker.Schedule(inputTensor);

        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;
        float[] outputData = outputTensor.DownloadToArray();

        // Safety Clamping: Increased maximums for AI-driven sizes
        float w = Mathf.Clamp(outputData[0] * UnityEngine.Random.Range(0.8f, 1.5f), 2f, 35f);
        float h = Mathf.Clamp(outputData[1] * UnityEngine.Random.Range(0.8f, 1.5f), 2f, 14f);
        float d = Mathf.Clamp(outputData[2], 12f, 60f);
        float y_ai = Mathf.Clamp(outputData[3] + UnityEngine.Random.Range(-2f, 2f), -5f, 10f);

        return new ObstacleParameters(w, h, d, y_ai);
}

    private void OnDisable()
    {
        worker?.Dispose();
    }
}