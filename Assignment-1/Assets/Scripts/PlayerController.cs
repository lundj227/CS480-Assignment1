using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
 private const int MaxJumpCount = 2;

 // Rigidbody of the player.
 private Rigidbody rb; 

 // Variable to keep track of collected "PickUp" objects.
 private int count;

 // Movement along X and Y axes.
 private float movementX;
 private float movementY;

 // Tracks how many jumps are still available before landing.
 private int jumpsRemaining = MaxJumpCount;

 // Keeps track of grounded colliders so the air jump only resets on landing.
 private readonly HashSet<Collider> groundContacts = new HashSet<Collider>();

 // True after the ball has fully left the ground since the last jump reset.
 private bool hasLeftGroundSinceReset;

 // Speed at which the player moves.
 public float speed = 0;

 // Upward impulse applied for each jump.
 [SerializeField] private float jumpForce = 7f;

 // Contact normals above this threshold count as ground.
 [SerializeField] private float groundNormalThreshold = 0.5f;

 // UI text component to display count of "PickUp" objects collected.
 public TextMeshProUGUI countText;

 // UI object to display winning text.
 public GameObject winTextObject;

 // Start is called before the first frame update.
 void Start()
    {
 // Get and store the Rigidbody component attached to the player.
        rb = GetComponent<Rigidbody>();

 // Initialize count to zero.
        count = 0;

 // Update the count display.
        SetCountText();

 // Initially set the win text to be inactive.
        winTextObject.SetActive(false);
    }
 
 // This function is called when a move input is detected.
 void OnMove(InputValue movementValue)
    {
 // Convert the input value into a Vector2 for movement.
        Vector2 movementVector = movementValue.Get<Vector2>();

 // Store the X and Y components of the movement.
        movementX = movementVector.x; 
        movementY = movementVector.y; 
    }

 // FixedUpdate is called once per fixed frame-rate frame.
 void FixedUpdate() 
    {
 // Create a 3D movement vector using the X and Y inputs.
        Vector3 movement = new Vector3 (movementX, 0.0f, movementY);

 // Apply force to the Rigidbody to move the player.
        rb.AddForce(movement * speed); 

 // Once the ball fully leaves the ground, the second jump becomes available.
        if (groundContacts.Count == 0)
        {
            hasLeftGroundSinceReset = true;
        }
    }

 void OnJump(InputValue jumpValue)
    {
        if (!jumpValue.isPressed)
        {
            return;
        }

        bool isGrounded = groundContacts.Count > 0;
        bool canGroundJump = isGrounded && jumpsRemaining == MaxJumpCount;
        bool canAirJump = !isGrounded && hasLeftGroundSinceReset && jumpsRemaining > 0;

        if (!canGroundJump && !canAirJump)
        {
            return;
        }

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        jumpsRemaining--;
    }

 
 void OnTriggerEnter(Collider other) 
    {
 // Check if the object the player collided with has the "PickUp" tag.
 if (other.gameObject.CompareTag("PickUp")) 
        {
 // Deactivate the collided object (making it disappear).
            other.gameObject.SetActive(false);

 // Increment the count of "PickUp" objects collected.
            count = count + 1;

 // Update the count display.
            SetCountText();
        }
    }

 // Function to update the displayed count of "PickUp" objects collected.
 void SetCountText() 
    {
 // Update the count text with the current count.
        countText.text = "Count: " + count.ToString();

 // Check if the count has reached or exceeded the win condition.
 if (count >= 12)
        {
// Display the win text.
            winTextObject.SetActive(true);
        }
    }

 void OnCollisionEnter(Collision collision)
    {
        RegisterGroundContact(collision);
    }

 void OnCollisionStay(Collision collision)
    {
        RegisterGroundContact(collision);
    }

 void OnCollisionExit(Collision collision)
    {
        groundContacts.Remove(collision.collider);
    }

 void RegisterGroundContact(Collision collision)
    {
        if (!HasGroundContact(collision))
        {
            return;
        }

        bool wasGrounded = groundContacts.Count > 0;
        groundContacts.Add(collision.collider);

        if (!wasGrounded && hasLeftGroundSinceReset)
        {
            jumpsRemaining = MaxJumpCount;
            hasLeftGroundSinceReset = false;
        }
    }

 bool HasGroundContact(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y >= groundNormalThreshold)
            {
                return true;
            }
        }

        return false;
    }
}
