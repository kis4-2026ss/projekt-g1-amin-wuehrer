using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(PolygonCollider2D))]
public class ProceduralPolygonGenerator : MonoBehaviour
{
    public void GenerateRandomShape(float width, float height)
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer.sharedMaterial == null)
        {
            renderer.sharedMaterial = Resources.Load<Material>("MeteoriteTexture");
            // Fallback if not in Resources
            if (renderer.sharedMaterial == null)
            {
                #if UNITY_EDITOR
                renderer.sharedMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/MeteoriteTexture.mat");
                #endif
            }
        }

        int vertexCount = UnityEngine.Random.Range(5, 12);
        Vector2[] points = new Vector2[vertexCount];
        
        // Larger meteorites are smoother (more vertices, less radius variation)
        // Smaller meteorites are spikier
        float spikiness = (width + height > 15f) ? 0.2f : 0.6f;
        
        for (int i = 0; i < vertexCount; i++)
        {
            float angle = i * Mathf.PI * 2 / vertexCount;
            float r = UnityEngine.Random.Range(1.0f - spikiness, 1.0f);
            points[i] = new Vector2(Mathf.Cos(angle) * r * width * 0.5f, Mathf.Sin(angle) * r * height * 0.5f);
        }

        // Apply to Collider
        GetComponent<PolygonCollider2D>().points = points;

        // Create Mesh
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[vertexCount + 1];
        Vector2[] uvs = new Vector2[vertexCount + 1];
        
        vertices[0] = Vector3.zero; // Center point
        uvs[0] = new Vector2(0.5f, 0.5f);
        
        for (int i = 0; i < vertexCount; i++)
        {
            vertices[i + 1] = points[i];
            uvs[i + 1] = new Vector2(points[i].x / width + 0.5f, points[i].y / height + 0.5f);
        }

        int[] triangles = new int[vertexCount * 3];
        for (int i = 0; i < vertexCount; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = (i + 1 == vertexCount) ? 1 : i + 2;
            triangles[i * 3 + 2] = i + 1;
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
