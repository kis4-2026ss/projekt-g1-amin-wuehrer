using UnityEngine;

public class EndlessFloor : MonoBehaviour
{
    private Transform player;
    private float length;

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        length = transform.localScale.x;
    }

    void Update()
    {
        if (player == null) return;

        // Move the floor ahead of the player if they get close to the end
        // For a simple prototype, we just jump the floor forward.
        // A better way is to use two segments and swap them.
        if (player.position.x > transform.position.x + length * 0.25f)
        {
            transform.position = new Vector3(player.position.x + length * 0.25f, transform.position.y, transform.position.z);
        }
    }
}
