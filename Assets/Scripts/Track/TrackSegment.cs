using System.Collections.Generic;
using UnityEngine;

namespace DesertTreasureHunt
{
    public enum TrackTheme
    {
        SandstoneRuins,
        CanyonBridge,
        OasisWalkway,
        PharaohTemple
    }

    public class TrackSegment : MonoBehaviour
    {
        public TrackTheme theme = TrackTheme.SandstoneRuins;
        public float segmentLength = 30f;
        public float laneDistance = 2.5f;

        [Header("Spawn Roots")]
        public Transform itemsContainer;

        // ── Shared static materials ──
        private static Material whiteTileMat;
        private static Material sandMat;
        private static Material stoneMat;
        private static Material stoneRedStripeMat;
        private static Material woodPlankMat;
        private static Material palmBarkMat;
        private static Material palmLeafMat;
        private static Material shrubMat;
        private static Material goldMat;
        private static Material rubyMat;
        private static Material sapphireMat;
        private static Material ironSpikeMat;
        private static Material rockMat;
        private static Material boulderMat;
        private static Material logMat;
        private static Material guardMat;
        private static Material guardArmorMat;
        private static Material shieldMat;
        private static Material starMat;
        private static Material keyMat;
        private static Material chestMat;

        // ─────────────────────────────────────────────────────────────────
        public void InitializeSegment(TrackTheme segTheme, int segmentIndex, float difficultyScale)
        {
            this.theme = segTheme;

            if (itemsContainer == null)
            {
                GameObject c = new GameObject("ItemsContainer");
                c.transform.SetParent(transform, false);
                itemsContainer = c.transform;
            }

            InitMaterials();
            BuildTrackFloor();
            BuildDesertSides();
            BuildScenery();

            if (segmentIndex > 2)
                SpawnObstaclesAndCollectibles(difficultyScale);
            else
                SpawnWelcomeCoins();
        }

        // ─────────────────────────────────────────────────────────────────
        // MATERIALS
        // ─────────────────────────────────────────────────────────────────
        private void InitMaterials()
        {
            if (whiteTileMat != null) return;
            Shader s = Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Standard")
                    ?? Shader.Find("Diffuse");

            whiteTileMat      = Mat(s, "WhiteTile",       new Color(0.94f, 0.90f, 0.80f));
            sandMat           = Mat(s, "Sand",            new Color(0.90f, 0.76f, 0.48f));
            stoneMat          = Mat(s, "Stone",           new Color(0.78f, 0.66f, 0.50f));
            stoneRedStripeMat = Mat(s, "StoneRedStripe",  new Color(0.72f, 0.25f, 0.18f));
            woodPlankMat      = Mat(s, "WoodPlank",       new Color(0.50f, 0.33f, 0.17f));
            palmBarkMat       = Mat(s, "PalmBark",        new Color(0.33f, 0.22f, 0.12f));
            palmLeafMat       = Mat(s, "PalmLeaf",        new Color(0.26f, 0.60f, 0.20f));
            shrubMat          = Mat(s, "Shrub",           new Color(0.35f, 0.58f, 0.18f));
            goldMat           = Mat(s, "Gold",            new Color(1.00f, 0.84f, 0.00f));
            rubyMat           = Mat(s, "Ruby",            new Color(0.95f, 0.15f, 0.25f));
            sapphireMat       = Mat(s, "Sapphire",        new Color(0.15f, 0.45f, 0.95f));
            ironSpikeMat      = Mat(s, "IronSpike",       new Color(0.30f, 0.30f, 0.34f));
            rockMat           = Mat(s, "Rock",            new Color(0.68f, 0.58f, 0.42f));
            boulderMat        = Mat(s, "Boulder",         new Color(0.52f, 0.46f, 0.36f));
            logMat            = Mat(s, "Log",             new Color(0.46f, 0.30f, 0.16f));
            guardMat          = Mat(s, "Guard",           new Color(0.55f, 0.42f, 0.28f));
            guardArmorMat     = Mat(s, "GuardArmor",      new Color(0.70f, 0.60f, 0.30f));
            shieldMat         = Mat(s, "Shield",          new Color(0.20f, 0.50f, 0.90f));
            starMat           = Mat(s, "Star",            new Color(1.00f, 0.80f, 0.00f));
            keyMat            = Mat(s, "Key",             new Color(0.85f, 0.70f, 0.10f));
            chestMat          = Mat(s, "Chest",           new Color(0.55f, 0.36f, 0.14f));
        }

        private static Material Mat(Shader s, string name, Color c) =>
            new Material(s) { name = "Mat_" + name, color = c };

        // ─────────────────────────────────────────────────────────────────
        // TRACK FLOOR — white/cream stone tiles
        // ─────────────────────────────────────────────────────────────────
        private void BuildTrackFloor()
        {
            Material roadMat = (theme == TrackTheme.CanyonBridge) ? woodPlankMat : whiteTileMat;
            Prim(PrimitiveType.Cube, "RoadFloor", transform,
                new Vector3(0f, -0.25f, segmentLength * 0.5f),
                new Vector3(9f, 0.5f, segmentLength),
                roadMat, true);

            if (theme == TrackTheme.CanyonBridge) { CreateRopeRailing(-4.2f); CreateRopeRailing(4.2f); }
            else { CreateCurb(-4.5f); CreateCurb(4.5f); }
        }

        private void CreateCurb(float x)
        {
            Prim(PrimitiveType.Cube, "Curb", transform,
                new Vector3(x, 0.05f, segmentLength * 0.5f),
                new Vector3(0.5f, 0.3f, segmentLength),
                stoneMat, false);
        }

        private void CreateRopeRailing(float x)
        {
            for (float z = 2f; z < segmentLength; z += 6f)
                Prim(PrimitiveType.Cylinder, "RopePost", transform,
                    new Vector3(x, 0.5f, z), new Vector3(0.25f, 0.6f, 0.25f), woodPlankMat, false);
        }

        // ─────────────────────────────────────────────────────────────────
        // DESERT SIDES — golden sand + scattered rocks
        // ─────────────────────────────────────────────────────────────────
        private void BuildDesertSides()
        {
            foreach (float sx in new float[] { -14f, 14f })
                Prim(PrimitiveType.Cube, "DesertSand", transform,
                    new Vector3(sx, -0.26f, segmentLength * 0.5f),
                    new Vector3(20f, 0.5f, segmentLength),
                    sandMat, false);

            for (float z = 3f; z < segmentLength; z += Random.Range(5f, 9f))
                for (int sd = -1; sd <= 1; sd += 2)
                    if (Random.value < 0.4f)
                        CreateRock(new Vector3(sd * Random.Range(6f, 11f), 0f, z));
        }

        private void CreateRock(Vector3 pos)
        {
            float sz = Random.Range(0.4f, 1.1f);
            var r = Prim(PrimitiveType.Cube, "Rock", transform, pos,
                new Vector3(sz, sz * 0.7f, sz * 1.1f), rockMat, false);
            r.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }

        // ─────────────────────────────────────────────────────────────────
        // SCENERY — Egyptian buildings, palm trees, shrubs, pyramids
        // ─────────────────────────────────────────────────────────────────
        private void BuildScenery()
        {
            BuildEgyptianBuildingWalls();

            // Staggered thick palm trees on both sides
            for (float z = 4f; z < segmentLength; z += 10f)
            {
                CreateThickPalmTree(new Vector3(-8.5f, 0f, z + Random.Range(-1f, 1f)));
                CreateThickPalmTree(new Vector3(8.5f,  0f, z + 5f + Random.Range(-1f, 1f)));
            }

            // Green low-poly shrubs scattered between buildings
            for (float z = 2f; z < segmentLength; z += Random.Range(4f, 7f))
                for (int sd = -1; sd <= 1; sd += 2)
                    if (Random.value < 0.55f)
                        CreateLowPolyShrub(new Vector3(sd * Random.Range(5.5f, 10f), 0f, z));

            // Occasional distant pyramid
            if (Random.value < 0.5f)
            {
                float sd = (Random.value > 0.5f) ? 1f : -1f;
                var pyr = Prim(PrimitiveType.Cube, "Pyramid", transform,
                    new Vector3(sd * Random.Range(40f, 70f), 15f, segmentLength * 0.5f),
                    new Vector3(28f, 28f, 28f), stoneMat, false);
                pyr.transform.localRotation = Quaternion.Euler(45f, 45f, 0f);
            }
        }

        private void BuildEgyptianBuildingWalls()
        {
            for (float z = 0f; z < segmentLength; z += 15f)
            {
                float bLen = Mathf.Min(14f, segmentLength - z);
                if (bLen < 4f) continue;
                float zC = z + bLen * 0.5f;
                BuildEgyptianBlock(new Vector3(-13f, 3f, zC), bLen, 6f);
                BuildEgyptianBlock(new Vector3( 13f, 3f, zC), bLen, 6f);
            }
        }

        private void BuildEgyptianBlock(Vector3 center, float length, float height)
        {
            // Stone body
            var w = Prim(PrimitiveType.Cube, "EgyptWall", transform, center,
                new Vector3(6f, height, length), stoneMat, false);

            // Red stripe near top
            Prim(PrimitiveType.Cube, "RedStripe", transform,
                new Vector3(center.x, center.y + height * 0.5f - 0.9f, center.z),
                new Vector3(6.05f, 0.9f, length + 0.05f), stoneRedStripeMat, false);

            // Second thinner stripe
            Prim(PrimitiveType.Cube, "RedStripe2", transform,
                new Vector3(center.x, center.y + height * 0.5f - 2.1f, center.z),
                new Vector3(6.05f, 0.4f, length + 0.05f), stoneRedStripeMat, false);

            // Roof overhang cap
            Prim(PrimitiveType.Cube, "RoofCap", transform,
                new Vector3(center.x, center.y + height * 0.5f + 0.3f, center.z),
                new Vector3(6.6f, 0.6f, length + 0.4f), stoneMat, false);
        }

        private void CreateThickPalmTree(Vector3 pos)
        {
            var palm = new GameObject("PalmTree");
            palm.transform.SetParent(transform, false);
            palm.transform.localPosition = pos;
            float th = Random.Range(5f, 7.5f);

            // Thick dark Cylinder trunk
            var trunk = Prim(PrimitiveType.Cylinder, "Trunk", palm.transform,
                new Vector3(0f, th * 0.5f, 0f), new Vector3(0.8f, th * 0.5f, 0.8f), palmBarkMat, false);
            trunk.transform.localRotation =
                Quaternion.Euler(Random.Range(-6f, 6f), 0f, Random.Range(-4f, 4f));

            // 3-cluster blocky green fronds
            for (int i = 0; i < 3; i++)
            {
                float ang = i * 120f;
                var frond = Prim(PrimitiveType.Cube, "Frond", palm.transform,
                    new Vector3(Mathf.Sin(ang * Mathf.Deg2Rad) * 0.9f, th + 0.5f, Mathf.Cos(ang * Mathf.Deg2Rad) * 0.9f),
                    new Vector3(2.0f, 1.0f, 3.0f), palmLeafMat, false);
                frond.transform.localRotation = Quaternion.Euler(Random.Range(-20f, 20f), ang, 0f);
            }
            Prim(PrimitiveType.Cube, "Crown", palm.transform,
                new Vector3(0f, th + 0.8f, 0f), new Vector3(1.6f, 1.0f, 1.6f), palmLeafMat, false);
        }

        private void CreateLowPolyShrub(Vector3 pos)
        {
            float sz = Random.Range(0.5f, 1.1f);
            for (int i = 0; i < 2; i++)
            {
                float thisScale = sz * (i == 0 ? 1.0f : 0.6f);
                var b = Prim(PrimitiveType.Cube, "Shrub", transform,
                    new Vector3(pos.x + (i == 1 ? Random.Range(-0.3f, 0.3f) : 0f),
                                pos.y + sz * 0.5f + i * sz * 0.4f, pos.z),
                    new Vector3(thisScale * 1.2f, thisScale * 0.8f, thisScale * 1.2f),
                    shrubMat, false);
                b.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 90f), 0f);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // OBSTACLE & COLLECTIBLE SPAWNING
        // ─────────────────────────────────────────────────────────────────
        private void SpawnWelcomeCoins()
        {
            for (float z = 8f; z < segmentLength - 5f; z += 2.5f)
                SpawnCoin(new Vector3(0f, 0.8f, z));
        }

        private void SpawnObstaclesAndCollectibles(float diffScale)
        {
            var avail = new List<int> { 0, 1, 2 };
            int obsCount = (Random.value < 0.4f * diffScale) ? 2 : 1;

            for (int i = 0; i < obsCount; i++)
            {
                if (avail.Count <= 1) break;
                int idx  = Random.Range(0, avail.Count);
                int lane = avail[idx];
                avail.RemoveAt(idx);
                float lX = (lane - 1) * laneDistance;
                float zP = Random.Range(10f, segmentLength - 8f);
                SpawnRandomObstacle(lX, zP);
            }

            int   safe  = avail[0];
            float safeX = (safe - 1) * laneDistance;

            float roll = Random.value;
            if      (roll < 0.40f) SpawnCoinLine(safeX);
            else if (roll < 0.60f) SpawnGemPattern(safeX);
            else if (roll < 0.72f) SpawnTreasureChest(new Vector3(safeX, 0f, 15f));
            else if (roll < 0.82f) SpawnKey(new Vector3(safeX, 1.2f, 15f));
            else                   SpawnPowerUpOrb(new Vector3(safeX, 1.2f, 15f));
        }

        // ── Pick a random obstacle from all 6 types ──
        private void SpawnRandomObstacle(float x, float z)
        {
            int roll = Random.Range(0, 6);
            switch (roll)
            {
                case 0: CreateSpikeTrap(new Vector3(x, 0.15f, z));        break;
                case 1: CreateWoodenCrate(new Vector3(x, 0.65f, z));       break;
                case 2: CreateFallingBoulder(new Vector3(x, 7f, z));       break;
                case 3: CreateCollapsingBridge(new Vector3(x, -0.1f, z)); break;
                case 4: CreateRollingLog(new Vector3(x, 0.5f, z));         break;
                case 5: CreateDesertGuard(new Vector3(x, 0f, z));          break;
            }
        }

        // ── 1. Spike Trap ──
        private void CreateSpikeTrap(Vector3 pos)
        {
            var trap = new GameObject("SpikeTrap");
            trap.transform.SetParent(itemsContainer, false);
            trap.transform.localPosition = pos;
            var obs = trap.AddComponent<Obstacle>();
            obs.obstacleType = ObstacleType.GroundSpikes;
            var col = trap.AddComponent<BoxCollider>();
            col.isTrigger = true; col.size = new Vector3(1.8f, 0.7f, 1.8f);

            Prim(PrimitiveType.Cube, "Plate", trap.transform,
                new Vector3(0f, 0.05f, 0f), new Vector3(1.8f, 0.1f, 1.8f), stoneMat, false);
            for (int sx = -1; sx <= 1; sx++)
                for (int sz = -1; sz <= 1; sz++)
                    Prim(PrimitiveType.Cylinder, "Spike", trap.transform,
                        new Vector3(sx * 0.5f, 0.35f, sz * 0.5f),
                        new Vector3(0.12f, 0.35f, 0.12f), ironSpikeMat, false);
        }

        // ── 2. Wooden Crate ──
        private void CreateWoodenCrate(Vector3 pos)
        {
            var crate = Prim(PrimitiveType.Cube, "WoodenCrate", itemsContainer,
                pos, new Vector3(1.5f, 1.5f, 1.5f), woodPlankMat, true);
            var obs = crate.AddComponent<Obstacle>();
            obs.obstacleType = ObstacleType.WoodenCrate;

            // Cross brace lines on faces (thin dark strips)
            for (int i = 0; i < 2; i++)
            {
                var strip = Prim(PrimitiveType.Cube, "CrateBrace", crate.transform,
                    new Vector3(0f, 0f, 0.51f + i * 0.02f),
                    new Vector3(1.0f, 0.08f, 0.02f), ironSpikeMat, false);
                strip.transform.localRotation = Quaternion.Euler(0f, 0f, (i == 0) ? 45f : -45f);
            }
        }

        // ── 3. Falling Boulder ──
        private void CreateFallingBoulder(Vector3 pos)
        {
            var boulder = Prim(PrimitiveType.Sphere, "FallingBoulder", itemsContainer,
                pos, new Vector3(1.6f, 1.6f, 1.6f), boulderMat, true);
            var obs = boulder.AddComponent<Obstacle>();
            obs.obstacleType = ObstacleType.FallingBoulder;

            // Shadow indicator on the ground beneath
            var shadow = Prim(PrimitiveType.Cylinder, "BoulderShadow", itemsContainer,
                new Vector3(pos.x, 0.01f, pos.z), new Vector3(1.4f, 0.02f, 1.4f), ironSpikeMat, false);

            // Deactivate shadow collider
            Destroy(shadow.GetComponent<Collider>());
        }

        // ── 4. Collapsing Bridge ──
        private void CreateCollapsingBridge(Vector3 pos)
        {
            var bridge = new GameObject("CollapsingBridge");
            bridge.transform.SetParent(itemsContainer, false);
            bridge.transform.localPosition = pos;
            var obs = bridge.AddComponent<Obstacle>();
            obs.obstacleType = ObstacleType.CollapsingBridge;
            var col = bridge.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(9f, 0.4f, 4f);

            // Bridge planks
            for (int p = 0; p < 5; p++)
            {
                Prim(PrimitiveType.Cube, "Plank", bridge.transform,
                    new Vector3(0f, 0f, -1.6f + p * 0.8f),
                    new Vector3(8f, 0.25f, 0.65f), woodPlankMat, false);
            }
            // Side rails
            for (int sd = -1; sd <= 1; sd += 2)
                Prim(PrimitiveType.Cube, "Rail", bridge.transform,
                    new Vector3(sd * 4.0f, 0.4f, 0f), new Vector3(0.2f, 0.4f, 4.2f), woodPlankMat, false);
        }

        // ── 5. Rolling Log ──
        private void CreateRollingLog(Vector3 pos)
        {
            var log = Prim(PrimitiveType.Cylinder, "RollingLog", itemsContainer,
                pos, new Vector3(0.9f, 4.5f, 0.9f), logMat, true);
            log.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // Lie on its side
            var obs = log.AddComponent<Obstacle>();
            obs.obstacleType = ObstacleType.RollingLog;
        }

        // ── 6. Desert Guard ──
        private void CreateDesertGuard(Vector3 pos)
        {
            var guard = new GameObject("DesertGuard");
            guard.transform.SetParent(itemsContainer, false);
            guard.transform.localPosition = pos;
            var obs = guard.AddComponent<Obstacle>();
            obs.obstacleType = ObstacleType.DesertGuard;
            var col = guard.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size  = new Vector3(1.2f, 2.4f, 1.0f);
            col.center = new Vector3(0f, 1.2f, 0f);

            // Body
            Prim(PrimitiveType.Cube, "GuardBody", guard.transform,
                new Vector3(0f, 1.1f, 0f), new Vector3(0.9f, 1.1f, 0.65f), guardMat, false);
            // Head
            Prim(PrimitiveType.Cube, "GuardHead", guard.transform,
                new Vector3(0f, 2.1f, 0f), new Vector3(0.75f, 0.75f, 0.75f), guardMat, false);
            // Helmet
            Prim(PrimitiveType.Cube, "GuardHelmet", guard.transform,
                new Vector3(0f, 2.5f, 0f), new Vector3(0.85f, 0.4f, 0.85f), guardArmorMat, false);
            // Armor chestplate
            Prim(PrimitiveType.Cube, "GuardArmor", guard.transform,
                new Vector3(0f, 1.1f, 0.33f), new Vector3(0.88f, 0.9f, 0.08f), guardArmorMat, false);
            // Spear shaft
            var spear = Prim(PrimitiveType.Cylinder, "SpearShaft", guard.transform,
                new Vector3(0.55f, 1.5f, 0f), new Vector3(0.07f, 1.6f, 0.07f), logMat, false);
            spear.transform.localRotation = Quaternion.Euler(0f, 0f, 10f);
            // Spear tip
            Prim(PrimitiveType.Cylinder, "SpearTip", guard.transform,
                new Vector3(0.65f, 3.1f, 0f), new Vector3(0.1f, 0.3f, 0.1f), ironSpikeMat, false);
            // Legs
            for (int lg = -1; lg <= 1; lg += 2)
                Prim(PrimitiveType.Cube, "GuardLeg", guard.transform,
                    new Vector3(lg * 0.22f, 0.3f, 0f), new Vector3(0.3f, 0.6f, 0.5f), guardMat, false);
        }

        // ─────────────────────────────────────────────────────────────────
        // COLLECTIBLES
        // ─────────────────────────────────────────────────────────────────
        private void SpawnCoinLine(float x)
        {
            for (float z = 6f; z < segmentLength - 6f; z += 2.2f)
                SpawnCoin(new Vector3(x, 0.8f, z));
        }

        private void SpawnGemPattern(float x)
        {
            SpawnGem(new Vector3(x, 1.0f, 10f), (Random.value > 0.5f) ? CollectibleType.GemRed : CollectibleType.GemBlue);
            SpawnCoin(new Vector3(x, 0.8f, 14f));
            SpawnCoin(new Vector3(x, 0.8f, 17f));
            SpawnGem(new Vector3(x, 1.0f, 21f), CollectibleType.GemBlue);
        }

        private void SpawnCoin(Vector3 pos)
        {
            var coin = Prim(PrimitiveType.Cylinder, "Coin", itemsContainer,
                pos, new Vector3(0.65f, 0.08f, 0.65f), goldMat, true);
            coin.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var c = coin.AddComponent<Collectible>();
            c.type = CollectibleType.Coin; c.pointValue = 10;
            coin.GetComponent<Collider>().isTrigger = true;
        }

        private void SpawnGem(Vector3 pos, CollectibleType gemType)
        {
            var gem = Prim(PrimitiveType.Sphere, gemType == CollectibleType.GemRed ? "RubyGem" : "SapphireGem",
                itemsContainer, pos, new Vector3(0.7f, 0.9f, 0.7f),
                gemType == CollectibleType.GemRed ? rubyMat : sapphireMat, true);
            var c = gem.AddComponent<Collectible>();
            c.type = gemType; c.pointValue = gemType == CollectibleType.GemRed ? 50 : 100;
            gem.GetComponent<Collider>().isTrigger = true;
        }

        private void SpawnKey(Vector3 pos)
        {
            var key = Prim(PrimitiveType.Cylinder, "AncientKey", itemsContainer,
                pos, new Vector3(0.18f, 0.6f, 0.18f), keyMat, true);
            key.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            var c = key.AddComponent<Collectible>();
            c.type = CollectibleType.Key; c.pointValue = 250;
            key.GetComponent<Collider>().isTrigger = true;
        }

        private void SpawnTreasureChest(Vector3 pos)
        {
            var chest = new GameObject("TreasureChest");
            chest.transform.SetParent(itemsContainer, false);
            chest.transform.localPosition = pos;
            chest.AddComponent<TreasureChest>();
        }

        // ── 6 Power-Up Orbs with distinct shapes & colors ──
        private void SpawnPowerUpOrb(Vector3 pos)
        {
            PowerUpType[] allTypes = {
                PowerUpType.Magnet, PowerUpType.Shield, PowerUpType.SpeedBoost,
                PowerUpType.Multiplier, PowerUpType.KeyFinder, PowerUpType.TreasureBonus
            };
            PowerUpType pType = allTypes[Random.Range(0, allTypes.Length)];

            PrimitiveType shape;
            Material mat;
            switch (pType)
            {
                case PowerUpType.Magnet:        shape = PrimitiveType.Cube;     mat = goldMat;     break;
                case PowerUpType.Shield:        shape = PrimitiveType.Cube;     mat = shieldMat;   break;
                case PowerUpType.SpeedBoost:    shape = PrimitiveType.Sphere;   mat = rubyMat;     break;
                case PowerUpType.Multiplier:    shape = PrimitiveType.Sphere;   mat = starMat;     break;
                case PowerUpType.KeyFinder:     shape = PrimitiveType.Cylinder; mat = keyMat;      break;
                default:                        shape = PrimitiveType.Cube;     mat = chestMat;    break;
            }

            var orb = Prim(shape, "PowerUp_" + pType, itemsContainer,
                pos, new Vector3(0.9f, 0.9f, 0.9f), mat, true);
            var pu = orb.AddComponent<PowerUp>();
            pu.powerUpType = pType;
            pu.duration = 10f;
            orb.GetComponent<Collider>().isTrigger = true;
        }

        // ─────────────────────────────────────────────────────────────────
        // HELPER: create primitive, optionally keep collider
        // ─────────────────────────────────────────────────────────────────
        private static GameObject Prim(PrimitiveType type, string name, Transform parent,
            Vector3 localPos, Vector3 scale, Material mat, bool keepCollider)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale    = scale;
            go.GetComponent<Renderer>().material = mat;
            if (!keepCollider) Destroy(go.GetComponent<Collider>());
            return go;
        }
    }
}
