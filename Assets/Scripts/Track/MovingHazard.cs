using UnityEngine;

namespace DesertTreasureHunt
{
    public class MovingHazard : MonoBehaviour
    {
        public enum MovementStyle
        {
            SwingPendulum,
            PatrolLane,
            SpinningSpike
        }

        public MovementStyle style = MovementStyle.SwingPendulum;
        public float speed = 2.5f;
        public float swingAngle = 45f;
        public float patrolWidth = 3f;

        private Vector3 initialPos;
        private Quaternion initialRot;

        private void Start()
        {
            initialPos = transform.position;
            initialRot = transform.rotation;
        }

        private void Update()
        {
            switch (style)
            {
                case MovementStyle.SwingPendulum:
                    float angle = Mathf.Sin(Time.time * speed) * swingAngle;
                    transform.rotation = initialRot * Quaternion.Euler(0f, 0f, angle);
                    break;

                case MovementStyle.PatrolLane:
                    float x = Mathf.Sin(Time.time * speed) * patrolWidth;
                    transform.position = new Vector3(initialPos.x + x, transform.position.y, transform.position.z);
                    break;

                case MovementStyle.SpinningSpike:
                    transform.Rotate(Vector3.forward, 180f * speed * Time.deltaTime, Space.Self);
                    break;
            }
        }
    }
}
