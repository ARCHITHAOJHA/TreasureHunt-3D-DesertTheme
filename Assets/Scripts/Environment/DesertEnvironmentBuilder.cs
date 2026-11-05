using UnityEngine;

namespace DesertTreasureHunt
{
    public class DesertEnvironmentBuilder : MonoBehaviour
    {
        private void Awake()
        {
            SetupEgyptianAtmosphere();
            BuildSphinxFarBackdrop();
            BuildAmbientSandParticles();
        }

        private void SetupEgyptianAtmosphere()
        {
            // Warm Egyptian golden-amber directional sun
            Light dirLight = FindFirstObjectByType<Light>();
            if (dirLight == null)
            {
                GameObject lightObj = new GameObject("EgyptianSun");
                dirLight = lightObj.AddComponent<Light>();
                dirLight.type = LightType.Directional;
            }
            dirLight.color = new Color(1.0f, 0.90f, 0.65f);   // Warm amber sun
            dirLight.intensity = 1.5f;
            dirLight.transform.rotation = Quaternion.Euler(38f, -20f, 0f);
            dirLight.shadows = LightShadows.Soft;

            // Sky-matching warm sandy haze fog (matches the image)
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.97f, 0.84f, 0.60f);  // Sandy golden haze
            RenderSettings.fogDensity = 0.009f;

            // Warm ambient from sky
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.90f, 0.76f, 0.52f);

            // Camera background = sandy golden sky
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.97f, 0.84f, 0.60f);
            }
        }

        private void BuildSphinxFarBackdrop()
        {
            // Place sphinx statue far ahead as a visual landmark (like in the image)
            GameObject sphinxRoot = new GameObject("SphinxBackdrop");
            sphinxRoot.transform.position = new Vector3(0f, 0f, 200f);

            Shader s = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
            Material stoneMat = new Material(s) { color = new Color(0.82f, 0.72f, 0.55f) };
            Material darkStoneMat = new Material(s) { color = new Color(0.60f, 0.52f, 0.40f) };
            Material goldMat = new Material(s) { color = new Color(0.95f, 0.78f, 0.35f) };

            // Sphinx body (lion body)
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "SphinxBody";
            body.transform.SetParent(sphinxRoot.transform, false);
            body.transform.localPosition = new Vector3(0f, 3f, 0f);
            body.transform.localScale = new Vector3(10f, 6f, 18f);
            body.GetComponent<Renderer>().material = stoneMat;
            Destroy(body.GetComponent<Collider>());

            // Sphinx head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "SphinxHead";
            head.transform.SetParent(sphinxRoot.transform, false);
            head.transform.localPosition = new Vector3(0f, 9f, 7f);
            head.transform.localScale = new Vector3(7f, 7f, 6f);
            head.GetComponent<Renderer>().material = stoneMat;
            Destroy(head.GetComponent<Collider>());

            // Nemes headdress (striped headcloth)
            GameObject headdress = GameObject.CreatePrimitive(PrimitiveType.Cube);
            headdress.name = "SphinxHeaddress";
            headdress.transform.SetParent(sphinxRoot.transform, false);
            headdress.transform.localPosition = new Vector3(0f, 11f, 7f);
            headdress.transform.localScale = new Vector3(9f, 3f, 8f);
            headdress.GetComponent<Renderer>().material = goldMat;
            Destroy(headdress.GetComponent<Collider>());

            // Face details - eyes (dark recesses)
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Cube);
                eye.name = "SphinxEye";
                eye.transform.SetParent(sphinxRoot.transform, false);
                eye.transform.localPosition = new Vector3(side * 2.0f, 9.5f, 10.5f);
                eye.transform.localScale = new Vector3(1.5f, 0.8f, 0.4f);
                eye.GetComponent<Renderer>().material = darkStoneMat;
                Destroy(eye.GetComponent<Collider>());
            }

            // Beard chin extension
            GameObject beard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beard.name = "SphinxBeard";
            beard.transform.SetParent(sphinxRoot.transform, false);
            beard.transform.localPosition = new Vector3(0f, 6f, 10f);
            beard.transform.localScale = new Vector3(2.5f, 4f, 1.5f);
            beard.GetComponent<Renderer>().material = goldMat;
            Destroy(beard.GetComponent<Collider>());

            // Base plinth
            GameObject plinth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plinth.name = "SphinxPlinth";
            plinth.transform.SetParent(sphinxRoot.transform, false);
            plinth.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            plinth.transform.localScale = new Vector3(14f, 1f, 22f);
            plinth.GetComponent<Renderer>().material = darkStoneMat;
            Destroy(plinth.GetComponent<Collider>());

            // Two Anubis guardian statues flanking the sphinx (as in image)
            BuildAnubisStatue(sphinxRoot.transform, new Vector3(-12f, 0f, -4f), stoneMat, darkStoneMat);
            BuildAnubisStatue(sphinxRoot.transform, new Vector3(12f, 0f, -4f), stoneMat, darkStoneMat);

            // Background mountain ridges (low-poly cliffs in the image)
            for (int i = -2; i <= 2; i++)
            {
                float mountainX = i * 40f + Random.Range(-10f, 10f);
                float mountainH = Random.Range(25f, 45f);
                BuildLowPolyMountain(new Vector3(mountainX, 0f, 240f), mountainH, stoneMat);
            }
        }

        private void BuildAnubisStatue(Transform parent, Vector3 localPos, Material stoneMat, Material darkMat)
        {
            GameObject statRoot = new GameObject("AnubisStatue");
            statRoot.transform.SetParent(parent, false);
            statRoot.transform.localPosition = localPos;

            // Body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(statRoot.transform, false);
            body.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            body.transform.localScale = new Vector3(2.5f, 5f, 2f);
            body.GetComponent<Renderer>().material = stoneMat;
            Destroy(body.GetComponent<Collider>());

            // Jackal head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(statRoot.transform, false);
            head.transform.localPosition = new Vector3(0f, 6f, 0.3f);
            head.transform.localScale = new Vector3(2f, 2.5f, 3f);
            head.GetComponent<Renderer>().material = darkMat;
            Destroy(head.GetComponent<Collider>());

            // Pointy jackal ears
            for (int s = -1; s <= 1; s += 2)
            {
                GameObject ear = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ear.transform.SetParent(statRoot.transform, false);
                ear.transform.localPosition = new Vector3(s * 0.7f, 7.8f, 0.3f);
                ear.transform.localRotation = Quaternion.Euler(0f, 0f, s * -20f);
                ear.transform.localScale = new Vector3(0.5f, 1.6f, 0.5f);
                ear.GetComponent<Renderer>().material = darkMat;
                Destroy(ear.GetComponent<Collider>());
            }
        }

        private void BuildLowPolyMountain(Vector3 worldPos, float height, Material mat)
        {
            GameObject mtn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mtn.name = "BackdropMountain";
            mtn.transform.position = worldPos;
            mtn.transform.localRotation = Quaternion.Euler(45f, Random.Range(0f, 360f), 0f);
            mtn.transform.localScale = new Vector3(height * 1.4f, height, height * 1.4f);
            mtn.GetComponent<Renderer>().material = mat;
            Destroy(mtn.GetComponent<Collider>());
        }

        private void BuildAmbientSandParticles()
        {
            // Drifting sand haze particles to match the golden atmosphere
            GameObject sandObj = new GameObject("DesertSandHaze");
            sandObj.transform.SetParent(transform);
            sandObj.transform.position = new Vector3(0f, 4f, 30f);

            ParticleSystem ps = sandObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0.97f, 0.82f, 0.55f, 0.18f);
            main.startSize = 0.3f;
            main.startSpeed = 5f;
            main.startLifetime = 5f;
            main.maxParticles = 400;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 70f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(35f, 10f, 60f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-3f);
            vel.z = new ParticleSystem.MinMaxCurve(4f);

            var rend = ps.GetComponent<ParticleSystemRenderer>();
            Shader s = Shader.Find("Mobile/Particles/Alpha Blended") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
            if (s != null) rend.material = new Material(s);
        }
    }
}
