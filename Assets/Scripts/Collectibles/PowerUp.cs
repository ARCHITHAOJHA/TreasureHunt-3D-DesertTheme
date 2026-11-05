using UnityEngine;

namespace DesertTreasureHunt
{
    public enum PowerUpType
    {
        Magnet,         // Coin Magnet  — attracts coins nearby
        Shield,         // Shield       — protects from one hit
        SpeedBoost,     // Speed Boost  — increase running speed
        Multiplier,     // Score Multiplier — 2× score for limited time
        KeyFinder,      // Key Finder   — reveals / highlights nearby keys
        TreasureBonus   // Treasure Bonus — bonus score at game over
    }

    public class PowerUp : MonoBehaviour
    {
        public PowerUpType powerUpType = PowerUpType.Magnet;
        public float duration = 10f;
        public float rotateSpeed = 90f;

        private void Update()
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
            float bob = Mathf.Sin(Time.time * 3f) * 0.1f;
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + bob * Time.deltaTime,
                transform.position.z);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
            {
                ApplyPowerUp();
                Destroy(gameObject);
            }
        }

        private void ApplyPowerUp()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayPowerUp();
            if (GameManager.Instance == null) return;

            switch (powerUpType)
            {
                case PowerUpType.Magnet:
                    GameManager.Instance.ActivateMagnet(duration);
                    break;
                case PowerUpType.Shield:
                    GameManager.Instance.ActivateShield();
                    break;
                case PowerUpType.SpeedBoost:
                    GameManager.Instance.ActivateSprint(duration > 0 ? duration : 6f);
                    break;
                case PowerUpType.Multiplier:
                    GameManager.Instance.ActivateMultiplier(2, duration);
                    break;
                case PowerUpType.KeyFinder:
                    GameManager.Instance.ActivateKeyFinder(duration > 0 ? duration : 12f);
                    break;
                case PowerUpType.TreasureBonus:
                    GameManager.Instance.ActivateTreasureBonus();
                    break;
            }
        }
    }
}
