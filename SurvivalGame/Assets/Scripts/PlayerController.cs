using UnityEngine;

public class PlayerController : MonoBehaviour {
    [Header("Player Settings")]
        public float MovementSpeed = 5f;
        public Rigidbody2D rb2D;
    [Header("Animation Settings")]
        public Animator PlayerAnimator;
    [Space]
        public GameObject World;
        WorldGeneratorV3 WorldGenScript;

    Vector2 Movement;

    void Start() {
        WorldGenScript = World.GetComponent<WorldGeneratorV3>();
    }

    void Update() {

        //Get Input
        if (WorldGenScript.IsWorldComplete) {
            Movement.x = Input.GetAxisRaw("Horizontal");
            Movement.y = Input.GetAxisRaw("Vertical");
        }

    }

    private void FixedUpdate() {

        ManagePlayerAnimation();

    }

    void ManagePlayerAnimation() {
        rb2D.MovePosition(rb2D.position + Movement.normalized * MovementSpeed * Time.deltaTime);

        bool isIdle = Movement.x == 0 && Movement.y == 0;
        PlayerAnimator.SetBool("IsWalking", !isIdle);
        PlayerAnimator.SetBool("IsIdle", isIdle);

        if (Movement.y < 0) {
            PlayerAnimator.SetBool("Left", false);
            PlayerAnimator.SetBool("Right", false);
            PlayerAnimator.SetBool("Back", false);
            PlayerAnimator.SetBool("Front", true);
        }
        if (Movement.y > 0) {
            PlayerAnimator.SetBool("Left", false);
            PlayerAnimator.SetBool("Right", false);
            PlayerAnimator.SetBool("Back", true);
            PlayerAnimator.SetBool("Front", false);
        }
        if (Movement.x < 0) {
            PlayerAnimator.SetBool("Left", true);
            PlayerAnimator.SetBool("Right", false);
            PlayerAnimator.SetBool("Back", false);
            PlayerAnimator.SetBool("Front", false);
        }
        if (Movement.x > 0) {
            PlayerAnimator.SetBool("Left", false);
            PlayerAnimator.SetBool("Right", true);
            PlayerAnimator.SetBool("Back", false);
            PlayerAnimator.SetBool("Front", false);
        }
    }
}
