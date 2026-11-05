using UnityEngine;

namespace DesertTreasureHunt
{
    public enum ObstacleType
    {
        GroundSpikes,       // Spike trap — jump or dodge
        WoodenCrate,        // Crate — jump or dodge
        FallingBoulder,     // Drops from above — dodge sideways
        CollapsingBridge,   // Floor falls after 1s — keep moving!
        RollingLog,         // Rolls across path — time your jump
        DesertGuard,        // Enemy guard — must dodge lane
        SandstoneBlock,     // Legacy sandstone wall block
        OverheadBarrier     // Must slide under
    }

    public class Obstacle : MonoBehaviour
    {
        public ObstacleType obstacleType = ObstacleType.GroundSpikes;
        public int damage = 1;

        // ── Falling Boulder state ──
        private bool _isDropping  = false;
        private float _dropTimer  = 1.5f;  // Warn time before dropping
        private float _dropSpeed  = 18f;
        private float _startY;

        // ── Collapsing Bridge state ──
        private bool _hasStepped  = false;
        private float _collapseTimer = 0.9f;
        private Renderer[] _floorRenderers;

        // ── Rolling Log state ──
        private float _rollDir    = 1f;
        private float _rollSpeed  = 6f;
        private float _rollRange  = 2.5f;
        private float _rollOriginX;

        // ── Guard patrol ──
        private float _guardTimer = 0f;
        private float _guardBob   = 0f;

        private void Awake()
        {
            gameObject.tag = "Obstacle";
            int obsLayer = LayerMask.NameToLayer("Obstacle");
            if (obsLayer != -1) gameObject.layer = obsLayer;

            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            _startY = transform.position.y;
        }

        private void Start()
        {
            if (obstacleType == ObstacleType.RollingLog)
                _rollOriginX = transform.position.x;

            if (obstacleType == ObstacleType.CollapsingBridge)
                _floorRenderers = GetComponentsInChildren<Renderer>();
        }

        private void Update()
        {
            switch (obstacleType)
            {
                case ObstacleType.FallingBoulder:
                    UpdateFallingBoulder();
                    break;
                case ObstacleType.CollapsingBridge:
                    UpdateCollapsingBridge();
                    break;
                case ObstacleType.RollingLog:
                    UpdateRollingLog();
                    break;
                case ObstacleType.DesertGuard:
                    UpdateGuard();
                    break;
            }
        }

        // ─── Falling Boulder: hovers with shadow cue, then drops ───
        private void UpdateFallingBoulder()
        {
            if (!_isDropping)
            {
                // Hover + pulse warning
                float pulse = 0.3f + 0.2f * Mathf.Abs(Mathf.Sin(Time.time * 4f));
                transform.position = new Vector3(transform.position.x, _startY + pulse, transform.position.z);
                _dropTimer -= Time.deltaTime;
                if (_dropTimer <= 0f) _isDropping = true;
            }
            else
            {
                // Drop
                transform.position = new Vector3(
                    transform.position.x,
                    Mathf.Max(-0.3f, transform.position.y - _dropSpeed * Time.deltaTime),
                    transform.position.z
                );
                // Roll while falling
                transform.Rotate(Vector3.right, 200f * Time.deltaTime, Space.World);
            }
        }

        // ─── Collapsing Bridge: flashes red, then disappears ───
        private void UpdateCollapsingBridge()
        {
            if (!_hasStepped) return;
            _collapseTimer -= Time.deltaTime;

            // Flash warning
            if (_floorRenderers != null)
            {
                float flash = Mathf.Abs(Mathf.Sin(Time.time * 10f));
                foreach (var r in _floorRenderers)
                    if (r != null) r.material.color = Color.Lerp(new Color(0.85f, 0.72f, 0.50f), Color.red, flash);
            }

            if (_collapseTimer <= 0f)
            {
                // Sink into the ground
                transform.position += Vector3.down * 15f * Time.deltaTime;
                if (transform.position.y < -5f) Destroy(gameObject);
            }
        }

        // ─── Rolling Log: sweeps back and forth across the road ───
        private void UpdateRollingLog()
        {
            _guardTimer += Time.deltaTime * _rollSpeed;
            float xOffset = Mathf.Sin(_guardTimer) * _rollRange;
            transform.position = new Vector3(_rollOriginX + xOffset, transform.position.y, transform.position.z);
            // Spin the log as it rolls
            transform.Rotate(Vector3.forward, 250f * Time.deltaTime * _rollDir, Space.World);
            _rollDir = (xOffset > 0f) ? 1f : -1f;
        }

        // ─── Desert Guard: bobs up and down, spear jabs ───
        private void UpdateGuard()
        {
            _guardTimer += Time.deltaTime;
            _guardBob = Mathf.Sin(_guardTimer * 2f) * 0.08f;
            transform.position = new Vector3(transform.position.x, _guardBob, transform.position.z);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") && other.GetComponent<PlayerController>() == null) return;

            PlayerController pc = other.GetComponent<PlayerController>();
            PlayerHealth ph     = other.GetComponent<PlayerHealth>();

            // Slide clears overhead barriers
            if (obstacleType == ObstacleType.OverheadBarrier && pc != null && pc.isSliding)
            {
                if (GameManager.Instance != null) GameManager.Instance.score += 50;
                return;
            }

            // Jump clears rolling log
            if (obstacleType == ObstacleType.RollingLog && pc != null && pc.isJumping)
                return;

            // Trigger bridge collapse but don't deal damage immediately (flash first)
            if (obstacleType == ObstacleType.CollapsingBridge)
            {
                _hasStepped = true;
                return;  // Damage dealt when bridge finishes collapsing (player falls)
            }

            // Shield absorbs hit
            if (GameManager.Instance != null && GameManager.Instance.isShieldActive)
            {
                GameManager.Instance.isShieldActive = false;
                // Visual feedback could go here
                return;
            }

            if (ph != null) ph.TakeDamage(damage);
        }
    }
}
