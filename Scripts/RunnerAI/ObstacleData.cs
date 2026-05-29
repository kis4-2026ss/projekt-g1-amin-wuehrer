using UnityEngine;

[System.Serializable]
public struct ObstacleParameters
{
    public float width;
    public float height;
    public float distanceToNext;
    public float yPosition; // Added for mid-air spawning

    public ObstacleParameters(float w, float h, float d, float y)
    {
        width = w;
        height = h;
        distanceToNext = d;
        yPosition = y;
    }
}
