using UnityEngine;

public class PlayerController : MonoBehaviour {
    [Header("Player Settings")]
        public float MovementSpeed = 5f;
        public Rigidbody2D rb2D;
        public int PlayerTemp;
        public GameObject EscapeMenu;
    [Header("Animation Settings")]
        public Animator PlayerAnimator;
    [Space]
        public GameObject World;
        WorldGeneratorV3 WorldGenScript;

    Vector2 Movement;

    void Start() {
    }

    void Update() {

        Movement.x = Input.GetAxisRaw("Horizontal");
        Movement.y = Input.GetAxisRaw("Vertical");
        if (Input.GetKeyDown("escape")) {
            EscapeMenu.SetActive(!EscapeMenu.activeSelf);
        }

        //Calc player temp and effects related
        //CalcPlayerTemp(); i said i cant sry

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

    void CalcPlayerTemp() {
        //no i cant sry.
        if (WorldGenScript.CurrentBiomeTemp < 0) {
            PlayerTemp = WorldGenScript.CurrentBiomeTemp*2 - WorldGenScript.CurrentBiomeTemp/3;
        } else if (WorldGenScript.CurrentBiomeTemp >= 0) {
            PlayerTemp = 36 - WorldGenScript.CurrentBiomeTemp / 3;
            //PlayerTemp = 36 + (((WorldGenScript.CurrentBiomeTemp / 2) * WorldGenScript.CurrentBiomeTemp) / 36);
        }
        //

    }
}
