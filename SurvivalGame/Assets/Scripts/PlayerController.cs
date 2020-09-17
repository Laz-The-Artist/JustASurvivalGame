using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public float MovementSpeed = 5f;
    public Rigidbody2D rb2D;
    [Header("Animation Settings")]
    public Animator PlayerAnimator;
    [Space]
    public GameObject World;
    WorldGenerator WorldGenScript;

    Vector2 Movement;

    void Start()
    {
        WorldGenScript = World.GetComponent<WorldGenerator>();
    }

    void Update()
    {
        if (WorldGenScript.IsWorldReady == true) {
            Movement.x = Input.GetAxisRaw("Horizontal");
            Movement.y = Input.GetAxisRaw("Vertical");
        }
    }

    private void FixedUpdate() {
        rb2D.MovePosition(rb2D.position + Movement * MovementSpeed * Time.deltaTime);
        if (Movement.x == -1) {
            PlayerAnimator.SetBool("Left", true);
            PlayerAnimator.SetBool("Right", false);
            PlayerAnimator.SetBool("Back", false);
            PlayerAnimator.SetBool("Front", false);
        } else if (Movement.x == 1) {
            PlayerAnimator.SetBool("Left", false);
            PlayerAnimator.SetBool("Right", true);
            PlayerAnimator.SetBool("Back", false);
            PlayerAnimator.SetBool("Front", false);
        } else if (Movement.y == -1) {
            PlayerAnimator.SetBool("Left", false);
            PlayerAnimator.SetBool("Right", false);
            PlayerAnimator.SetBool("Back", false);
            PlayerAnimator.SetBool("Front", true);
        } else if (Movement.y == 1) {
            PlayerAnimator.SetBool("Left", false);
            PlayerAnimator.SetBool("Right", false);
            PlayerAnimator.SetBool("Back", true);
            PlayerAnimator.SetBool("Front", false);
        }

    }
}
