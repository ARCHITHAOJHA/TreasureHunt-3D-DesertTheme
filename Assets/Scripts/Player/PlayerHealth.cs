using System.Collections;
using UnityEngine;

namespace DesertTreasureHunt
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        public int maxHearts = 3;
        public int currentHearts = 3;

        [Header("Invulnerability")]
        public float invulnerabilityTime = 1.5f;
        private bool isInvulnerable = false;
        private float spawnProtectionTimer = 2.0f;

        private PlayerVisuals visuals;

        private void Awake()
        {
            visuals = GetComponentInChildren<PlayerVisuals>();
        }

        private void Start()
        {
            currentHearts = maxHearts;
            UpdateHealthUI();
        }

        private void Update()
        {
            if (spawnProtectionTimer > 0f)
            {
                spawnProtectionTimer -= Time.deltaTime;
            }
        }

        public void TakeDamage(int damage = 1)
        {
            if (isInvulnerable || spawnProtectionTimer > 0f) return;

            // Shield check
            if (GameManager.Instance != null && GameManager.Instance.isShieldActive)
            {
                GameManager.Instance.isShieldActive = false;
                if (SoundManager.Instance != null) SoundManager.Instance.PlayPowerUp();
                StartCoroutine(InvulnerabilityRoutine(0.8f));
                return;
            }

            currentHearts -= damage;
            UpdateHealthUI();

            if (SoundManager.Instance != null) SoundManager.Instance.PlayHurt();

            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.TriggerShake(0.3f, 0.4f);
            }

            if (currentHearts <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(InvulnerabilityRoutine(invulnerabilityTime));
            }
        }

        public void Heal(int hearts = 1)
        {
            currentHearts = Mathf.Min(currentHearts + hearts, maxHearts);
            UpdateHealthUI();
        }

        public void DieInstantly()
        {
            currentHearts = 0;
            UpdateHealthUI();
            Die();
        }

        private void Die()
        {
            if (visuals != null) visuals.OnDeath();
            if (SoundManager.Instance != null) SoundManager.Instance.PlayGameOver();
            if (GameManager.Instance != null) GameManager.Instance.GameOver();
        }

        private IEnumerator InvulnerabilityRoutine(float duration)
        {
            isInvulnerable = true;
            float elapsed = 0f;
            float blinkRate = 0.1f;

            while (elapsed < duration)
            {
                if (visuals != null) visuals.SetVisible(false);
                yield return new WaitForSeconds(blinkRate);
                if (visuals != null) visuals.SetVisible(true);
                yield return new WaitForSeconds(blinkRate);
                elapsed += blinkRate * 2f;
            }

            if (visuals != null) visuals.SetVisible(true);
            isInvulnerable = false;
        }

        private void UpdateHealthUI()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHearts(currentHearts, maxHearts);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Obstacle") || other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                TakeDamage(1);
            }
            else if (other.CompareTag("KillZone"))
            {
                DieInstantly();
            }
        }
    }
}
