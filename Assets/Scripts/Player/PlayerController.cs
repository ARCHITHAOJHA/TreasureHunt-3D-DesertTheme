
using UnityEngine;

namespace DesertTreasureHunt
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Lane Settings")]
        public float laneDistance = 2.5f;
        public float laneChangeSpeed = 16f;
        public int currentLane = 1; // 0 = Left, 1 = Center, 2 = Right

        [Header("Jump & Gravity")]
        public float jumpForce = 9.5f;
        public float gravity = 28f;
        private float verticalVelocity = 0f;

        // Accessible by Obstacle.cs
        public bool isJumping { get; private set; } = false;

        [Header("Slide Settings")]
        public float slideDuration = 0.75f;
        public bool isSliding = false;
        private float slideTimer = 0f;
        private float originalHeight;
        private Vector3 originalCenter;

        [Header("Components")]
        private CharacterController controller;
        public PlayerVisuals visuals;
        public PlayerHealth health;

        [Header("Input Handling")]
        private Vector2 touchStartPos;
        private bool isSwiping = false;
        private float minSwipeDistance = 35f;

        private float spawnGraceTimer = 2.0f;

        private void Awake()
        {
            Instance = this;

            controller = GetComponent<CharacterController>();

            originalHeight = controller.height;
            originalCenter = controller.center;

            if (visuals == null)
                visuals = GetComponentInChildren<PlayerVisuals>();

            if (health == null)
                health = GetComponent<PlayerHealth>();
        }

        private void Update()
        {
            if (GameManager.Instance == null ||
                GameManager.Instance.currentState != GameState.Playing)
            {
                return;
            }

            HandleInput();
            HandleSlide();
            MovePlayer();
            CheckFallDeath();
        }

        private void HandleInput()
        {
            // Keyboard / Input System controls
            if (DesertInput.GetLeft())
            {
                MoveLane(-1);
            }
            else if (DesertInput.GetRight())
            {
                MoveLane(1);
            }

            if (DesertInput.GetJump() &&
                controller.isGrounded &&
                !isSliding)
            {
                Jump();
            }

            if (DesertInput.GetSlide() &&
                controller.isGrounded &&
                !isSliding)
            {
                StartSlide();
            }

            // Mobile / Touch / Mouse swipe controls
            Vector2 pointerPos;

            if (DesertInput.GetPointerDown(out pointerPos))
            {
                touchStartPos = pointerPos;
                isSwiping = true;
            }
            else if (DesertInput.GetPointerUp(out pointerPos) && isSwiping)
            {
                Vector2 delta = pointerPos - touchStartPos;

                if (delta.magnitude >= minSwipeDistance)
                {
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        // Horizontal swipe
                        if (delta.x > 0)
                            MoveLane(1);
                        else
                            MoveLane(-1);
                    }
                    else
                    {
                        // Vertical swipe
                        if (delta.y > 0 &&
                            controller.isGrounded &&
                            !isSliding)
                        {
                            Jump();
                        }
                        else if (delta.y < 0 &&
                                 controller.isGrounded &&
                                 !isSliding)
                        {
                            StartSlide();
                        }
                    }
                }

                isSwiping = false;
            }
        }

        public void MoveLane(int direction)
        {
            int targetLane = Mathf.Clamp(currentLane + direction, 0, 2);

            if (targetLane != currentLane)
            {
                currentLane = targetLane;

                if (visuals != null)
                    visuals.OnLaneSwitch(direction);
            }
        }

        public void Jump()
        {
            if (!controller.isGrounded || isSliding)
                return;

            verticalVelocity = jumpForce;
            isJumping = true;

            if (visuals != null)
                visuals.OnJump();

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayJump();
        }

        public void StartSlide()
        {
            if (!controller.isGrounded || isSliding)
                return;

            isSliding = true;
            slideTimer = slideDuration;

            controller.height = originalHeight * 0.45f;

            controller.center = new Vector3(
                originalCenter.x,
                originalHeight * 0.225f,
                originalCenter.z
            );

            if (visuals != null)
                visuals.OnSlide(true);

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySlide();
        }

        private void HandleSlide()
        {
            if (!isSliding)
                return;

            slideTimer -= Time.deltaTime;

            if (slideTimer <= 0f)
            {
                StopSlide();
            }
        }

        public void StopSlide()
        {
            if (!isSliding)
                return;

            isSliding = false;

            controller.height = originalHeight;
            controller.center = originalCenter;

            if (visuals != null)
                visuals.OnSlide(false);
        }

        private void MovePlayer()
        {
            float forwardSpeed = GameManager.Instance.currentSpeed;

            // Lane movement
            float targetX = (currentLane - 1) * laneDistance;

            float newX = Mathf.MoveTowards(
                transform.position.x,
                targetX,
                laneChangeSpeed * Time.deltaTime
            );

            // Gravity and vertical movement
            if (controller.isGrounded)
            {
                if (verticalVelocity < 0f)
                    verticalVelocity = -1.5f;

                // Player has landed
                if (isJumping && verticalVelocity < 0f)
                {
                    isJumping = false;
                }
            }
            else
            {
                verticalVelocity -= gravity * Time.deltaTime;
            }

            Vector3 moveVector = new Vector3(
                newX - transform.position.x,
                verticalVelocity * Time.deltaTime,
                forwardSpeed * Time.deltaTime
            );

            controller.Move(moveVector);

            // Check landing after movement
            if (controller.isGrounded && verticalVelocity <= 0f)
            {
                isJumping = false;
            }
        }

        private void CheckFallDeath()
        {
            if (spawnGraceTimer > 0f)
            {
                spawnGraceTimer -= Time.deltaTime;

                if (transform.position.y < 0f)
                {
                    transform.position = new Vector3(
                        transform.position.x,
                        0.1f,
                        transform.position.z
                    );

                    verticalVelocity = 0f;
                    isJumping = false;
                }

                return;
            }

            if (transform.position.y < -5.0f)
            {
                isJumping = false;

                if (health != null)
                {
                    health.DieInstantly();
                }
                else if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver();
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
