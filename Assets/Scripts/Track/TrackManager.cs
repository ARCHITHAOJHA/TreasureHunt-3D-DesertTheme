using System.Collections.Generic;
using UnityEngine;

namespace DesertTreasureHunt
{
    public class TrackManager : MonoBehaviour
    {
        public static TrackManager Instance { get; private set; }

        [Header("Spawn Configuration")]
        public float segmentLength = 30f;
        public int initialSegmentsCount = 7;
        public float recycleDistanceBehindPlayer = 25f;

        [Header("References")]
        public Transform playerTransform;

        private List<TrackSegment> activeSegments = new List<TrackSegment>();
        private float nextSpawnZ = 0f;
        private int totalSegmentsSpawned = 0;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (playerTransform == null && PlayerController.Instance != null)
            {
                playerTransform = PlayerController.Instance.transform;
            }

            SpawnInitialTrack();
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                if (PlayerController.Instance != null) playerTransform = PlayerController.Instance.transform;
                return;
            }

            // Check if player has advanced enough to spawn next segment
            if (playerTransform.position.z + (segmentLength * 4f) > nextSpawnZ)
            {
                SpawnNextSegment();
            }

            // Check if oldest segment can be recycled
            if (activeSegments.Count > 0)
            {
                TrackSegment oldest = activeSegments[0];
                if (playerTransform.position.z - oldest.transform.position.z > (segmentLength + recycleDistanceBehindPlayer))
                {
                    activeSegments.RemoveAt(0);
                    Destroy(oldest.gameObject);
                }
            }
        }

        private void SpawnInitialTrack()
        {
            nextSpawnZ = -30f; // Start 30m behind player for a solid runway
            for (int i = 0; i < initialSegmentsCount; i++)
            {
                SpawnNextSegment();
            }
        }

        public void SpawnNextSegment()
        {
            GameObject segObj = new GameObject($"TrackSegment_{totalSegmentsSpawned}");
            segObj.transform.SetParent(transform);
            segObj.transform.position = new Vector3(0f, 0f, nextSpawnZ);

            TrackSegment seg = segObj.AddComponent<TrackSegment>();
            seg.segmentLength = segmentLength;

            // Cycle themes every 5 segments
            int themeIndex = (totalSegmentsSpawned / 5) % 4;
            TrackTheme theme = (TrackTheme)themeIndex;

            // Calculate difficulty scaling factor
            float diffScale = 1.0f;
            if (GameManager.Instance != null)
            {
                diffScale = GameManager.Instance.difficulty switch
                {
                    DifficultyLevel.Easy => 0.7f,
                    DifficultyLevel.Medium => 1.0f,
                    DifficultyLevel.Hard => 1.4f,
                    _ => 1.0f
                };
            }

            seg.InitializeSegment(theme, totalSegmentsSpawned, diffScale);

            activeSegments.Add(seg);
            nextSpawnZ += segmentLength;
            totalSegmentsSpawned++;
        }
    }
}
