using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float jumpForce = 14.4f; // Increased from 12 by 20%
    
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float maxTiltAngle = 45f;
    [SerializeField] private float angleOffset = -90f; // Added to fix sprites that face Up by default
    
    private Rigidbody2D rb;
private InputAction jumpAction;
    private bool isDead = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpAction = InputSystem.actions.FindAction("Jump");
        
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 4.2f;
    }

    private void Update()
    {
        if (isDead || GameState.Instance == null || GameState.Instance.isCountingDown) return;

        if (jumpAction != null && jumpAction.WasPressedThisFrame())
        {
            Jump();
        }

        RotateShip();
    }

    private void RotateShip()
    {
        // Calculate angle based on velocity
        float targetAngle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        
        // Clamp the angle
        targetAngle = Mathf.Clamp(targetAngle, -maxTiltAngle, maxTiltAngle);

        // Apply offset (e.g., -90 if the sprite faces Up by default)
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle + angleOffset);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (isDead || GameState.Instance == null || GameState.Instance.isCountingDown)
        {
            if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(GameState.Instance.currentSpeed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("Player Died!");
        Invoke(nameof(RestartLevel), 1f);
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
