using UnityEngine;

namespace DesertTreasureHunt
{
    public enum CollectibleType
    {
        Coin,
        GemRed,
        GemBlue,
        Key
    }

    public class Collectible : MonoBehaviour
    {
        public CollectibleType type = CollectibleType.Coin;
        public int pointValue = 10;
        public float rotateSpeed = 120f;
        public float bobSpeed = 3f;
        public float bobHeight = 0.15f;

        private Vector3 startPos;
        private bool isCollected = false;

        private void Start()
        {
            startPos = transform.position;
        }

        private void Update()
        {
            if (isCollected) return;

            // Rotation & Bobbing
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
            float bobOffset = Mathf.Sin((Time.time + transform.position.z * 0.5f) * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, startPos.y + bobOffset, transform.position.z);

            // Magnet Attraction (primarily for coins and gems)
            if (GameManager.Instance != null && GameManager.Instance.isMagnetActive && PlayerController.Instance != null)
            {
                float distToPlayer = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
                if (distToPlayer < 12f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, PlayerController.Instance.transform.position + Vector3.up * 0.8f, 22f * Time.deltaTime);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;

            if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
            {
                Collect();
            }
        }

        public void Collect()
        {
            isCollected = true;

            if (GameManager.Instance != null)
            {
                switch (type)
                {
                    case CollectibleType.Coin:
                        GameManager.Instance.AddCoins(1);
                        if (SoundManager.Instance != null) SoundManager.Instance.PlayCoin();
                        break;
                    case CollectibleType.GemRed:
                        GameManager.Instance.AddGem(50, "Ruby");
                        if (SoundManager.Instance != null) SoundManager.Instance.PlayGem();
                        break;
                    case CollectibleType.GemBlue:
                        GameManager.Instance.AddGem(100, "Sapphire");
                        if (SoundManager.Instance != null) SoundManager.Instance.PlayGem();
                        break;
                    case CollectibleType.Key:
                        GameManager.Instance.AddKey();
                        if (SoundManager.Instance != null) SoundManager.Instance.PlayKey();
                        break;
                }
            }

            SpawnPickupSparkles();
            Destroy(gameObject);
        }

        private void SpawnPickupSparkles()
        {
            GameObject fx = new GameObject("PickupFX");
            fx.transform.position = transform.position;

            ParticleSystem ps = fx.AddComponent<ParticleSystem>();
            var main = ps.main;
            Color fxColor = type switch
            {
                CollectibleType.Coin => new Color(1f, 0.84f, 0f),
                CollectibleType.GemRed => new Color(1f, 0.2f, 0.3f),
                CollectibleType.GemBlue => new Color(0.2f, 0.6f, 1f),
                CollectibleType.Key => new Color(1f, 0.95f, 0.4f),
                _ => Color.yellow
            };
            main.startColor = fxColor;
            main.startSize = 0.25f;
            main.startSpeed = 3f;
            main.startLifetime = 0.5f;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 18) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var rend = ps.GetComponent<ParticleSystemRenderer>();
            Shader s = Shader.Find("Mobile/Particles/Additive") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
            if (s != null) rend.material = new Material(s);

            Destroy(fx, 0.6f);
        }
    }
}
