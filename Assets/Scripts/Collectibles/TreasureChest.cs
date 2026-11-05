using System.Collections;
using UnityEngine;

namespace DesertTreasureHunt
{
    public class TreasureChest : MonoBehaviour
    {
        [Header("Chest Parts")]
        public Transform chestLid;
        public Transform chestBase;

        [Header("Rewards")]
        public int bonusScore = 500;
        public int bonusCoins = 25;

        private bool isOpen = false;

        private void Awake()
        {
            if (chestBase == null)
            {
                BuildProceduralChest();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isOpen) return;

            if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
            {
                OpenChest();
            }
        }

        public void OpenChest()
        {
            isOpen = true;

            if (SoundManager.Instance != null) SoundManager.Instance.PlayChest();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.score += bonusScore * GameManager.Instance.scoreMultiplier;
                GameManager.Instance.AddCoins(bonusCoins);

                // Award random powerup!
                int roll = Random.Range(0, 4);
                switch (roll)
                {
                    case 0:
                        GameManager.Instance.ActivateMagnet(12f);
                        break;
                    case 1:
                        GameManager.Instance.ActivateShield();
                        break;
                    case 2:
                        GameManager.Instance.ActivateMultiplier(2, 12f);
                        break;
                    case 3:
                        GameManager.Instance.ActivateSprint(6f);
                        break;
                }
            }

            StartCoroutine(AnimateLidOpen());
            SpawnChestFX();
        }

        private IEnumerator AnimateLidOpen()
        {
            float elapsed = 0f;
            float duration = 0.35f;
            Quaternion startRot = chestLid.localRotation;
            Quaternion targetRot = Quaternion.Euler(-70f, 0f, 0f);

            while (elapsed < duration)
            {
                chestLid.localRotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            chestLid.localRotation = targetRot;
        }

        private void SpawnChestFX()
        {
            GameObject fx = new GameObject("ChestOpenFX");
            fx.transform.position = transform.position + Vector3.up * 0.6f;

            ParticleSystem ps = fx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(1f, 0.85f, 0.2f, 1f);
            main.startSize = 0.4f;
            main.startSpeed = 4.5f;
            main.startLifetime = 0.8f;

            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 35) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 35f;

            var rend = ps.GetComponent<ParticleSystemRenderer>();
            Shader s = Shader.Find("Mobile/Particles/Additive") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
            if (s != null) rend.material = new Material(s);

            Destroy(fx, 1.2f);
        }

        private void BuildProceduralChest()
        {
            Material woodMat = CreateMat("Mat_ChestWood", new Color(0.45f, 0.26f, 0.12f));
            Material goldTrimMat = CreateMat("Mat_ChestGold", new Color(0.95f, 0.78f, 0.18f));

            // Chest Base
            GameObject bObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bObj.name = "ChestBase";
            bObj.transform.SetParent(transform, false);
            bObj.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            bObj.transform.localScale = new Vector3(1.1f, 0.5f, 0.8f);
            bObj.GetComponent<Renderer>().material = woodMat;
            chestBase = bObj.transform;

            // Chest Lid Root (Hinged at back edge)
            GameObject lidRoot = new GameObject("ChestLidRoot");
            lidRoot.transform.SetParent(transform, false);
            lidRoot.transform.localPosition = new Vector3(0f, 0.5f, -0.4f);
            chestLid = lidRoot.transform;

            // Lid Mesh
            GameObject lidMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lidMesh.name = "LidMesh";
            lidMesh.transform.SetParent(lidRoot.transform, false);
            lidMesh.transform.localPosition = new Vector3(0f, 0.15f, 0.4f);
            lidMesh.transform.localScale = new Vector3(1.15f, 0.3f, 0.85f);
            lidMesh.GetComponent<Renderer>().material = woodMat;
            Destroy(lidMesh.GetComponent<Collider>());

            // Gold Lock Clasp
            GameObject lockClasp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lockClasp.name = "GoldLock";
            lockClasp.transform.SetParent(bObj.transform, false);
            lockClasp.transform.localPosition = new Vector3(0f, 0.2f, 0.52f);
            lockClasp.transform.localScale = new Vector3(0.2f, 0.25f, 0.08f);
            lockClasp.GetComponent<Renderer>().material = goldTrimMat;
            Destroy(lockClasp.GetComponent<Collider>());

            // Trigger Collider
            BoxCollider col = gameObject.GetComponent<BoxCollider>();
            if (col == null) col = gameObject.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, 0.5f, 0f);
            col.size = new Vector3(1.4f, 1.2f, 1.4f);
        }

        private Material CreateMat(string name, Color color)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
            Material m = new Material(s);
            m.name = name;
            m.color = color;
            return m;
        }
    }
}
