using UnityEngine;

namespace DesertTreasureHunt
{
    public class PlayerVisuals : MonoBehaviour
    {
        [Header("Model Roots & Limbs")]
        public Transform modelRoot;
        public Transform headTransform;
        public Transform torsoTransform;
        public Transform leftArm;
        public Transform rightArm;
        public Transform leftLeg;
        public Transform rightLeg;

        [Header("Animation Parameters")]
        public float runAnimSpeed = 14f;
        public float legSwingAngle = 38f;
        public float armSwingAngle = 35f;
        public float runBobHeight = 0.08f;

        private float animTime = 0f;
        private float currentTilt = 0f;
        private float targetTilt = 0f;
        private bool isJumping = false;
        private bool isSliding = false;
        private bool isDead = false;

        private ParticleSystem dustParticles;

        private void Awake()
        {
            if (modelRoot == null) modelRoot = transform;
            EnsureAdventurerModel();
            CreateSandDustEffect();
        }

        private void Update()
        {
            if (isDead) return;

            // Bank / Tilt lerp
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * 10f);
            targetTilt = Mathf.Lerp(targetTilt, 0f, Time.deltaTime * 4f);

            if (isSliding)
            {
                ApplySlidePose();
            }
            else if (isJumping)
            {
                ApplyJumpPose();
            }
            else
            {
                ApplyRunAnimation();
            }

            modelRoot.localRotation = Quaternion.Euler(0f, 0f, currentTilt);
        }

        private void ApplyRunAnimation()
        {
            float speedFactor = (GameManager.Instance != null) ? GameManager.Instance.currentSpeed / 12f : 1f;
            animTime += Time.deltaTime * runAnimSpeed * speedFactor;

            float legAngle = Mathf.Sin(animTime) * legSwingAngle;
            float armAngle = -Mathf.Sin(animTime) * armSwingAngle;
            float bob = Mathf.Abs(Mathf.Sin(animTime * 2f)) * runBobHeight;

            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(legAngle, 0f, 0f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(-legAngle, 0f, 0f);
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(armAngle, 0f, 10f);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(-armAngle, 0f, -10f);

            if (torsoTransform != null)
            {
                torsoTransform.localPosition = new Vector3(0f, 0.75f + bob, 0f);
            }

            if (dustParticles != null && !dustParticles.isPlaying)
            {
                dustParticles.Play();
            }
        }

        private void ApplyJumpPose()
        {
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(-30f, 0f, -5f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(-20f, 0f, 5f);
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(60f, 0f, 25f);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(60f, 0f, -25f);

            if (dustParticles != null && dustParticles.isPlaying)
            {
                dustParticles.Stop();
            }
        }

        private void ApplySlidePose()
        {
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(75f, 0f, 0f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(75f, 0f, 0f);
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(-45f, 0f, 30f);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(-45f, 0f, -30f);
            if (torsoTransform != null) torsoTransform.localRotation = Quaternion.Euler(-35f, 0f, 0f);
        }

        public void OnLaneSwitch(int direction)
        {
            targetTilt = -direction * 18f;
        }

        public void OnJump()
        {
            isJumping = true;
            Invoke(nameof(ResetJump), 0.7f);
        }

        private void ResetJump()
        {
            isJumping = false;
        }

        public void OnSlide(bool sliding)
        {
            isSliding = sliding;
            if (!sliding && torsoTransform != null)
            {
                torsoTransform.localRotation = Quaternion.identity;
                torsoTransform.localPosition = new Vector3(0f, 0.75f, 0f);
            }
        }

        public void OnDeath()
        {
            isDead = true;
            modelRoot.localRotation = Quaternion.Euler(90f, Random.Range(-30f, 30f), 0f);
            modelRoot.localPosition = new Vector3(0f, 0.2f, 0f);
            if (dustParticles != null) dustParticles.Stop();
        }

        public void SetVisible(bool visible)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.enabled = visible;
            }
        }

        private void CreateSandDustEffect()
        {
            GameObject dustObj = new GameObject("SandDustTrail");
            dustObj.transform.SetParent(transform);
            dustObj.transform.localPosition = new Vector3(0f, 0.1f, -0.4f);

            dustParticles = dustObj.AddComponent<ParticleSystem>();
            var main = dustParticles.main;
            main.startColor = new Color(0.92f, 0.78f, 0.55f, 0.6f);
            main.startSize = 0.25f;
            main.startLifetime = 0.4f;
            main.startSpeed = 1.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = dustParticles.emission;
            emission.rateOverTime = 25f;

            var shape = dustParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            var renderer = dustParticles.GetComponent<ParticleSystemRenderer>();
            Shader s = Shader.Find("Mobile/Particles/Alpha Blended") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
            if (s != null)
            {
                renderer.material = new Material(s);
            }
        }

        private void EnsureAdventurerModel()
        {
            // If adventurer parts don't exist yet, build procedural stylized 3D explorer!
            if (torsoTransform == null)
            {
                Material sandClothesMat = CreateColorMaterial("Mat_KhakiShirt", new Color(0.76f, 0.65f, 0.48f));
                Material leatherMat = CreateColorMaterial("Mat_LeatherBrown", new Color(0.42f, 0.26f, 0.15f));
                Material skinMat = CreateColorMaterial("Mat_Skin", new Color(0.95f, 0.77f, 0.62f));
                Material pantsMat = CreateColorMaterial("Mat_CargoPants", new Color(0.35f, 0.28f, 0.20f));

                // 1. Torso
                GameObject torsoObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                torsoObj.name = "Torso";
                torsoObj.transform.SetParent(modelRoot, false);
                torsoObj.transform.localPosition = new Vector3(0f, 0.75f, 0f);
                torsoObj.transform.localScale = new Vector3(0.55f, 0.6f, 0.32f);
                torsoObj.GetComponent<Renderer>().material = sandClothesMat;
                Destroy(torsoObj.GetComponent<Collider>());
                torsoTransform = torsoObj.transform;

                // Satchel / sling bag across chest
                GameObject satchel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                satchel.name = "Satchel";
                satchel.transform.SetParent(torsoTransform, false);
                satchel.transform.localPosition = new Vector3(0.28f, -0.1f, 0.1f);
                satchel.transform.localRotation = Quaternion.Euler(0f, 15f, 15f);
                satchel.transform.localScale = new Vector3(0.3f, 0.4f, 0.25f);
                satchel.GetComponent<Renderer>().material = leatherMat;
                Destroy(satchel.GetComponent<Collider>());

                // 2. Head
                GameObject headObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                headObj.name = "Head";
                headObj.transform.SetParent(torsoTransform, false);
                headObj.transform.localPosition = new Vector3(0f, 0.52f, 0f);
                headObj.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
                headObj.GetComponent<Renderer>().material = skinMat;
                Destroy(headObj.GetComponent<Collider>());
                headTransform = headObj.transform;

                // Explorer Fedora Hat Crown
                GameObject hatCrown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                hatCrown.name = "HatCrown";
                hatCrown.transform.SetParent(headTransform, false);
                hatCrown.transform.localPosition = new Vector3(0f, 0.45f, 0.02f);
                hatCrown.transform.localScale = new Vector3(0.7f, 0.35f, 0.75f);
                hatCrown.GetComponent<Renderer>().material = leatherMat;
                Destroy(hatCrown.GetComponent<Collider>());

                // Explorer Hat Brim
                GameObject hatBrim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                hatBrim.name = "HatBrim";
                hatBrim.transform.SetParent(headTransform, false);
                hatBrim.transform.localPosition = new Vector3(0f, 0.32f, 0.02f);
                hatBrim.transform.localScale = new Vector3(1.25f, 0.04f, 1.3f);
                hatBrim.GetComponent<Renderer>().material = leatherMat;
                Destroy(hatBrim.GetComponent<Collider>());

                // 3. Arms
                leftArm = CreateLimb("LeftArm", modelRoot, new Vector3(-0.38f, 0.85f, 0f), new Vector3(0.18f, 0.55f, 0.18f), sandClothesMat);
                rightArm = CreateLimb("RightArm", modelRoot, new Vector3(0.38f, 0.85f, 0f), new Vector3(0.18f, 0.55f, 0.18f), sandClothesMat);

                // 4. Legs
                leftLeg = CreateLimb("LeftLeg", modelRoot, new Vector3(-0.16f, 0.45f, 0f), new Vector3(0.2f, 0.6f, 0.2f), pantsMat);
                rightLeg = CreateLimb("RightLeg", modelRoot, new Vector3(0.16f, 0.45f, 0f), new Vector3(0.2f, 0.6f, 0.2f), pantsMat);

                // Boots
                CreateLimb("LeftBoot", leftLeg, new Vector3(0f, -0.4f, 0.05f), new Vector3(1.1f, 0.4f, 1.4f), leatherMat);
                CreateLimb("RightBoot", rightLeg, new Vector3(0f, -0.4f, 0.05f), new Vector3(1.1f, 0.4f, 1.4f), leatherMat);
            }
        }

        private Transform CreateLimb(string name, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject limb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            limb.name = name;
            limb.transform.SetParent(parent, false);
            limb.transform.localPosition = localPos;
            limb.transform.localScale = localScale;
            limb.GetComponent<Renderer>().material = mat;
            Destroy(limb.GetComponent<Collider>());
            return limb.transform;
        }

        private Material CreateColorMaterial(string name, Color color)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
            Material m = new Material(s);
            m.name = name;
            m.color = color;
            return m;
        }
    }
}
