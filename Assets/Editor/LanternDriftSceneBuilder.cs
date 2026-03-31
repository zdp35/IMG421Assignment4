using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LanternDrift.Boat;
using LanternDrift.Gameplay;
using LanternDrift.Water;
using LanternDrift.UI;

namespace LanternDrift.EditorTools
{
    public static class LanternDriftSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/LanternDrift_Greybox.unity";

        [MenuItem("Lantern Drift/Build Greybox Scene")]
        public static void BuildGreyboxScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "LanternDrift_Greybox";

            SetupLightingAndFog();

            GameObject managerObject = new GameObject("GameManager");
            GameManager gameManager = managerObject.AddComponent<GameManager>();
            gameManager.difficulty = GameManager.DifficultyMode.Medium;
            gameManager.baseRoundTime = 95f;
            gameManager.sinkThreshold = 100f;
            gameManager.sinkDecayPerSecond = 6f;

            CreateRiverBanks();
            GameObject water = CreateWater();
            BoatController boat = CreateBoat();
            CreateCamera(boat.transform);
            CreateLanterns();
            CreateSandbars();
            CreateGators();
            BuildUI(gameManager, boat);

            gameManager.playerBoat = boat;

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Lantern Drift", "Greybox scene built and saved to Assets/Scenes/LanternDrift_Greybox.unity", "OK");
        }

        private static void SetupLightingAndFog()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.03f, 0.06f, 0.045f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.03f, 0.08f, 0.06f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.02f;

            Light directional = Object.FindObjectOfType<Light>();
            if (directional != null)
            {
                directional.type = LightType.Directional;
                directional.intensity = 0.18f;
                directional.color = new Color(0.3f, 0.45f, 0.35f);
                directional.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
            }

            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.backgroundColor = new Color(0.01f, 0.04f, 0.03f);
                mainCam.clearFlags = CameraClearFlags.SolidColor;
            }
        }

        private static GameObject CreateWater()
        {
            GameObject water = new GameObject("Water");
            water.transform.position = Vector3.zero;
            MeshRenderer renderer = water.AddComponent<MeshRenderer>();
            MeshFilter filter = water.AddComponent<MeshFilter>();
            DynamicMesh dynamicMesh = water.AddComponent<DynamicMesh>();
            dynamicMesh.width = 120;
            dynamicMesh.height = 120;
            dynamicMesh.ApplyToMeshFilter(dynamicMesh.GenerateGridMesh());
            water.AddComponent<WaveApplier>();

            Material waterMat = CreateSafeMaterial(
                new[] { "Standard", "Universal Render Pipeline/Lit", "Legacy Shaders/Diffuse", "Unlit/Color", "Sprites/Default" },
                new Color(0.06f, 0.16f, 0.12f, 1f));
            if (waterMat != null && waterMat.HasProperty("_Glossiness")) waterMat.SetFloat("_Glossiness", 0.1f);
            renderer.sharedMaterial = waterMat;

            BoxCollider waterCollider = water.AddComponent<BoxCollider>();
            waterCollider.size = new Vector3(120f, 4f, 120f);
            waterCollider.center = new Vector3(0f, -1.5f, 0f);

            CreateWave("MainWaveA", new Vector3(0f, 0f, 0f), 0.42f, 0.82f, new Vector2(8f, 12f));
            CreateWave("MainWaveB", new Vector3(14f, 0f, -8f), 0.28f, 1.18f, new Vector2(5f, 7f));
            CreateWave("MainWaveC", new Vector3(-20f, 0f, 12f), 0.22f, 1.55f, new Vector2(3f, 4f));
            return water;
        }

        private static void CreateWave(string name, Vector3 position, float maxHeight, float speed, Vector2 scale)
        {
            GameObject waveObj = new GameObject(name);
            waveObj.transform.position = position;
            Wave wave = waveObj.AddComponent<Wave>();
            wave.maxHeight = maxHeight;
            wave.speed = speed;
            wave.scale = scale;
        }

        private static BoatController CreateBoat()
        {
            GameObject boatRoot = new GameObject("PlayerBoat");
            boatRoot.transform.position = new Vector3(0f, 1f, -38f);

            Rigidbody rb = boatRoot.AddComponent<Rigidbody>();
            rb.mass = 18f;
            rb.drag = 1.2f;
            rb.angularDrag = 2.4f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            BoatStatus status = boatRoot.AddComponent<BoatStatus>();
            BoatController controller = boatRoot.AddComponent<BoatController>();
            controller.rb = rb;
            controller.status = status;
            controller.forwardForce = 22f;
            controller.reverseForce = 12f;
            controller.turnTorque = 14.5f;
            controller.pivotTurnTorque = 24f;
            controller.maxForwardSpeed = 5.8f;
            controller.maxReverseSpeed = 2.7f;
            controller.canControl = false;
            controller.moveWaterDrag = 1.0f;
            controller.idleWaterDrag = 1.45f;
            controller.angularDrag = 2.8f;
            controller.throttleResponse = 5.0f;
            controller.throttleDirectionChangeResponse = 10.0f;
            controller.steeringResponse = 4.8f;
            controller.steeringDirectionChangeResponse = 8.5f;
            controller.steeringReleaseResponse = 7.0f;
            controller.minTurnAuthority = 0.45f;
            controller.maxTurnAuthority = 1.75f;
            controller.lowSpeedTurnAuthority = 1.25f;
            controller.reverseTurnAuthority = 1.15f;
            controller.sideSlipDamping = 4.2f;
            controller.reverseBrakeAssist = 7.0f;

            GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hull.name = "Hull";
            hull.transform.SetParent(boatRoot.transform, false);
            hull.transform.localScale = new Vector3(1.9f, 0.45f, 3.7f);
            hull.transform.localPosition = new Vector3(0f, 0f, 0f);
            hull.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.18f, 0.16f, 0.11f));
            Object.DestroyImmediate(hull.GetComponent<BoxCollider>());

            GameObject deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.name = "DeckCabin";
            deck.transform.SetParent(boatRoot.transform, false);
            deck.transform.localScale = new Vector3(1.2f, 0.5f, 1.2f);
            deck.transform.localPosition = new Vector3(0f, 0.45f, -0.2f);
            deck.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.24f, 0.22f, 0.16f));
            Object.DestroyImmediate(deck.GetComponent<BoxCollider>());

            BoxCollider rootCollider = boatRoot.AddComponent<BoxCollider>();
            rootCollider.size = new Vector3(1.8f, 1f, 3.5f);
            rootCollider.center = new Vector3(0f, 0.35f, 0f);

            CreateBuoy(boatRoot.transform, rb, new Vector3(-0.7f, -0.15f, -1.2f));
            CreateBuoy(boatRoot.transform, rb, new Vector3(0.7f, -0.15f, -1.2f));
            CreateBuoy(boatRoot.transform, rb, new Vector3(-0.7f, -0.15f, 1.2f));
            CreateBuoy(boatRoot.transform, rb, new Vector3(0.7f, -0.15f, 1.2f));

            GameObject lampObj = new GameObject("BoatLamp");
            lampObj.transform.SetParent(boatRoot.transform, false);
            lampObj.transform.localPosition = new Vector3(0f, 1.2f, 0.8f);
            Light lamp = lampObj.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.range = 24f;
            lamp.intensity = 5.8f;
            lamp.color = new Color(1f, 0.73f, 0.35f);
            controller.boatLamp = lamp;

            CreateWakeEffects(boatRoot.transform, controller);
            return controller;
        }

        private static void CreateBuoy(Transform parent, Rigidbody rb, Vector3 localPos)
        {
            GameObject buoyObj = new GameObject("Buoy");
            buoyObj.transform.SetParent(parent, false);
            buoyObj.transform.localPosition = localPos;
            Buoy buoy = buoyObj.AddComponent<Buoy>();
            buoy.targetRigidbody = rb;
            buoy.buoyancy = 1.35f;
        }

        private static void CreateWakeEffects(Transform boatRoot, BoatController controller)
        {
            GameObject wakeRoot = new GameObject("WakeEffects");
            wakeRoot.transform.SetParent(boatRoot, false);
            wakeRoot.transform.localPosition = new Vector3(0f, 0.1f, -2.1f);

            GameObject bubbleObj = new GameObject("BubbleParticles");
            bubbleObj.transform.SetParent(wakeRoot.transform, false);
            ParticleSystem particles = bubbleObj.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 0.9f;
            main.startSpeed = 0.8f;
            main.startSize = 0.22f;
            main.gravityModifier = -0.05f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 120;
            var emission = particles.emission;
            emission.rateOverTime = 0f;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 10f;
            shape.radius = 0.25f;
            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(new Color(0.55f, 1f, 0.72f), 0f), new GradientColorKey(new Color(0.16f, 0.82f, 0.42f), 1f) },
                new[] { new GradientAlphaKey(0.35f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = gradient;

            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            Material particleMat = CreateSafeMaterial(
                new[] { "Universal Render Pipeline/Particles/Unlit", "Particles/Standard Unlit", "Unlit/Color", "Sprites/Default", "Legacy Shaders/Particles/Alpha Blended" },
                new Color(0.42f, 1f, 0.62f, 0.55f));
            renderer.sharedMaterial = particleMat;

            GameObject trailObj = new GameObject("WakeTrail");
            trailObj.transform.SetParent(wakeRoot.transform, false);
            trailObj.transform.localPosition = Vector3.zero;
            TrailRenderer trail = trailObj.AddComponent<TrailRenderer>();
            trail.time = 0f;
            trail.startWidth = 1.1f;
            trail.endWidth = 0.15f;
            trail.alignment = LineAlignment.View;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;
            Material trailMat = CreateSafeMaterial(
                new[] { "Sprites/Default", "Unlit/Color", "Legacy Shaders/Particles/Alpha Blended" },
                new Color(0.38f, 1f, 0.62f, 0.5f));
            trail.sharedMaterial = trailMat;

            BoatWakeEffects effects = wakeRoot.AddComponent<BoatWakeEffects>();
            effects.boat = controller;
            effects.bubbleParticles = particles;
            effects.wakeTrail = trail;
        }

        private static void CreateCamera(Transform target)
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            TopDownCameraFollow follow = mainCam.gameObject.AddComponent<TopDownCameraFollow>();
            follow.target = target;
            follow.offset = new Vector3(0f, 24f, -8f);
            follow.rotateWithBoat = false;
            mainCam.transform.position = target.position + follow.offset;
            mainCam.transform.rotation = Quaternion.Euler(68f, 0f, 0f);
        }

        private static void CreateRiverBanks()
        {
            GameObject terrain = new GameObject("SwampTerrain");

            GameObject baseGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
            baseGround.name = "BaseGround";
            baseGround.transform.SetParent(terrain.transform, false);
            baseGround.transform.localScale = new Vector3(12f, 1f, 12f);
            baseGround.transform.position = new Vector3(0f, -0.6f, 0f);
            baseGround.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.08f, 0.1f, 0.06f));

            Vector3[] bankPositions =
            {
                new Vector3(-12f, 0.2f, -30f), new Vector3(12f, 0.2f, -30f),
                new Vector3(-14f, 0.2f, -10f), new Vector3(14f, 0.2f, -10f),
                new Vector3(-18f, 0.2f, 8f), new Vector3(18f, 0.2f, 8f),
                new Vector3(-16f, 0.2f, 28f), new Vector3(16f, 0.2f, 28f),
                new Vector3(-5f, 0.2f, 2f), new Vector3(5f, 0.2f, 2f),
                new Vector3(-7f, 0.2f, 18f), new Vector3(7f, 0.2f, 18f)
            };

            Vector3[] bankScales =
            {
                new Vector3(8f, 1.5f, 20f), new Vector3(8f, 1.5f, 20f),
                new Vector3(10f, 1.5f, 18f), new Vector3(10f, 1.5f, 18f),
                new Vector3(12f, 1.5f, 18f), new Vector3(12f, 1.5f, 18f),
                new Vector3(10f, 1.5f, 18f), new Vector3(10f, 1.5f, 18f),
                new Vector3(3f, 1.5f, 12f), new Vector3(3f, 1.5f, 12f),
                new Vector3(4f, 1.5f, 10f), new Vector3(4f, 1.5f, 10f)
            };

            for (int i = 0; i < bankPositions.Length; i++)
            {
                GameObject bank = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bank.name = $"Bank_{i:00}";
                bank.transform.SetParent(terrain.transform, false);
                bank.transform.position = bankPositions[i];
                bank.transform.localScale = bankScales[i];
                bank.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.14f, 0.12f, 0.07f));
            }

            for (int i = 0; i < 26; i++)
            {
                CreateReedCluster(terrain.transform, new Vector3(Random.Range(-20f, 20f), 0f, Random.Range(-45f, 45f)));
            }
        }

        private static void CreateReedCluster(Transform parent, Vector3 position)
        {
            GameObject cluster = new GameObject("ReedCluster");
            cluster.transform.SetParent(parent, false);
            cluster.transform.position = position;
            for (int i = 0; i < 4; i++)
            {
                GameObject reed = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                reed.transform.SetParent(cluster.transform, false);
                reed.transform.localScale = new Vector3(0.07f, Random.Range(0.8f, 1.4f), 0.07f);
                reed.transform.localPosition = new Vector3(Random.Range(-0.7f, 0.7f), 0.4f, Random.Range(-0.7f, 0.7f));
                reed.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.17f, 0.24f, 0.12f));
                Object.DestroyImmediate(reed.GetComponent<CapsuleCollider>());
            }
        }

        private static void CreateLanterns()
        {
            Vector3[] positions =
            {
                new Vector3(0f, 0.6f, -25f),
                new Vector3(0f, 0.6f, -14f),
                new Vector3(-9f, 0.6f, -5f),
                new Vector3(9f, 0.6f, -1f),
                new Vector3(-13f, 0.6f, 14f),
                new Vector3(13f, 0.6f, 15f),
                new Vector3(-10f, 0.6f, 31f),
                new Vector3(0f, 0.6f, 41f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject lantern = new GameObject($"Lantern_{i + 1:00}");
                lantern.transform.position = positions[i];
                lantern.AddComponent<WaterFloatFollower>().heightOffset = 0.65f;

                GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                baseObj.transform.SetParent(lantern.transform, false);
                baseObj.transform.localScale = new Vector3(0.35f, 0.25f, 0.35f);
                baseObj.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.28f, 0.17f, 0.05f));
                Object.DestroyImmediate(baseObj.GetComponent<CapsuleCollider>());

                GameObject glowObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                glowObj.transform.SetParent(lantern.transform, false);
                glowObj.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                glowObj.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
                glowObj.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(1f, 0.74f, 0.35f));
                Object.DestroyImmediate(glowObj.GetComponent<SphereCollider>());

                Light light = lantern.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 20f;
                light.intensity = 6.0f;
                light.color = new Color(1f, 0.72f, 0.33f);

                SphereCollider trigger = lantern.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = 1.3f;

                LanternPickup pickup = lantern.AddComponent<LanternPickup>();
                pickup.lanternLight = light;
                pickup.renderers = lantern.GetComponentsInChildren<Renderer>();
            }
        }

        private static void CreateSandbars()
        {
            Vector3[] positions =
            {
                new Vector3(-3f, 0.08f, -1f),
                new Vector3(6f, 0.08f, 15f),
                new Vector3(-8f, 0.08f, 20f),
                new Vector3(0f, 0.08f, 31f)
            };

            Vector3[] scales =
            {
                new Vector3(4.2f, 0.3f, 7f),
                new Vector3(5.2f, 0.3f, 5.2f),
                new Vector3(4.2f, 0.3f, 4.2f),
                new Vector3(6.5f, 0.3f, 4.8f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject sandbar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sandbar.name = $"Sandbar_{i + 1:00}";
                sandbar.transform.position = positions[i];
                sandbar.transform.localScale = scales[i];
                sandbar.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.35f, 0.29f, 0.14f));
                Collider col = sandbar.GetComponent<Collider>();
                col.isTrigger = true;
                sandbar.AddComponent<SandbarZone>();
            }
        }

        private static void CreateGators()
        {
            Vector3[] positions =
            {
                new Vector3(9f, 0.1f, -12f),
                new Vector3(-10f, 0.1f, 7f),
                new Vector3(11f, 0.1f, 19f),
                new Vector3(-6f, 0.1f, 34f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject gator = new GameObject($"Gator_{i + 1:00}");
                gator.transform.position = positions[i];

                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.transform.SetParent(gator.transform, false);
                body.transform.localScale = new Vector3(0.9f, 0.35f, 2.1f);
                body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                body.GetComponent<Renderer>().sharedMaterial = CreateMaterial(new Color(0.09f, 0.18f, 0.08f));
                Object.DestroyImmediate(body.GetComponent<CapsuleCollider>());

                SphereCollider detect = gator.AddComponent<SphereCollider>();
                detect.isTrigger = true;
                detect.radius = 11f;

                GatorHazard hazard = gator.AddComponent<GatorHazard>();
                hazard.detectionRange = 11f;
                hazard.attackRange = 4.2f;
                hazard.moveSpeed = 2f + i * 0.15f;
                hazard.dangerPerSecond = 15f + i * 1.5f;
            }
        }

        private static void BuildUI(GameManager gameManager, BoatController boat)
        {
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            GameObject canvasObject = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);
            gameManager.mainCanvas = canvas;

            GameObject titlePanel = CreatePanel(canvas.transform, "TitlePanel", new Color(0f, 0f, 0f, 0.6f));
            CreateText(titlePanel.transform, "Title", "Lantern Drift", 74, new Vector2(0.5f, 0.7f), new Vector2(900f, 120f));
            CreateText(titlePanel.transform, "Subtitle", "Collect every lantern before the swamp takes you.", 28, new Vector2(0.5f, 0.6f), new Vector2(1100f, 80f));
            CreateText(titlePanel.transform, "Controls", "W/S thrust   A/D turn", 24, new Vector2(0.5f, 0.52f), new Vector2(700f, 60f));

            CreateButton(titlePanel.transform, "PlayButton", "Play", new Vector2(0.5f, 0.40f), UIButtonAction.ActionType.Play);
            CreateButton(titlePanel.transform, "QuitButton", "Quit", new Vector2(0.5f, 0.31f), UIButtonAction.ActionType.Quit);

            GameObject hudPanel = CreatePanel(canvas.transform, "HUDPanel", new Color(0f, 0f, 0f, 0f));
            hudPanel.SetActive(false);
            Text lanternText = CreateText(hudPanel.transform, "LanternText", "Lanterns: 0/0", 28, new Vector2(0.12f, 0.95f), new Vector2(320f, 50f));
            Text timerText = CreateText(hudPanel.transform, "TimerText", "Time: 0", 28, new Vector2(0.5f, 0.95f), new Vector2(240f, 50f));
            Text sinkText = CreateText(hudPanel.transform, "SinkText", "Sink: 0%", 28, new Vector2(0.88f, 0.95f), new Vector2(260f, 50f));

            GameObject endPanel = CreatePanel(canvas.transform, "EndPanel", new Color(0f, 0f, 0f, 0.68f));
            endPanel.SetActive(false);
            Text endTitle = CreateText(endPanel.transform, "EndTitle", "Run Ended", 66, new Vector2(0.5f, 0.66f), new Vector2(900f, 100f));
            Text endBody = CreateText(endPanel.transform, "EndBody", "", 28, new Vector2(0.5f, 0.54f), new Vector2(1000f, 160f));
            CreateButton(endPanel.transform, "RestartButton", "Restart", new Vector2(0.5f, 0.38f), UIButtonAction.ActionType.Restart);
            CreateButton(endPanel.transform, "BackButton", "Title", new Vector2(0.5f, 0.29f), UIButtonAction.ActionType.BackToTitle);

            gameManager.titlePanel = titlePanel;
            gameManager.hudPanel = hudPanel;
            gameManager.endPanel = endPanel;
            gameManager.lanternText = lanternText;
            gameManager.timerText = timerText;
            gameManager.sinkText = sinkText;
            gameManager.endTitleText = endTitle;
            gameManager.endBodyText = endBody;
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize, Vector2 anchor, Vector2 size)
        {
            GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(parent, false);
            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            Text text = textObj.GetComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.95f, 0.87f, 0.68f);
            return text;
        }

        private static void CreateButton(Transform parent, string name, string label, Vector2 anchor, UIButtonAction.ActionType actionType)
        {
            GameObject buttonObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UIButtonAction));
            buttonObj.transform.SetParent(parent, false);
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(260f, 72f);
            rect.anchoredPosition = Vector2.zero;
            Image image = buttonObj.GetComponent<Image>();
            image.color = new Color(0.18f, 0.22f, 0.17f, 0.92f);
            buttonObj.GetComponent<UIButtonAction>().actionType = actionType;
            Text text = CreateText(buttonObj.transform, "Label", label, 28, new Vector2(0.5f, 0.5f), new Vector2(240f, 60f));
            text.color = new Color(0.98f, 0.92f, 0.82f);
        }

        private static Material CreateMaterial(Color color)
        {
            Material mat = CreateSafeMaterial(
                new[]
                {
                    "Standard",
                    "Universal Render Pipeline/Lit",
                    "Legacy Shaders/Diffuse",
                    "Unlit/Color",
                    "Sprites/Default"
                },
                color);
            if (mat != null && mat.HasProperty("_Glossiness"))
            {
                mat.SetFloat("_Glossiness", 0.08f);
            }
            return mat;
        }

        private static Material CreateSafeMaterial(string[] shaderNames, Color color)
        {
            Shader shader = null;
            for (int i = 0; i < shaderNames.Length; i++)
            {
                shader = Shader.Find(shaderNames[i]);
                if (shader != null) break;
            }

            if (shader == null)
            {
                Debug.LogError("LanternDriftSceneBuilder could not find a valid shader for material creation.");
                return null;
            }

            Material mat = new Material(shader);
            mat.color = color;
            return mat;
        }
    }
}
