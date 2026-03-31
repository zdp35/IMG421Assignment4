using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LanternDrift.Water;
using LanternDrift.Boat;
using LanternDrift.Gameplay;

namespace LanternDrift.Bootstrap
{
    [DefaultExecutionOrder(-1000)]
    public class RuntimeBootstrap : MonoBehaviour
    {
        private bool built;

        private void Start()
        {
            if (built) return;
            built = true;
            BuildScene();
        }

        private void BuildScene()
        {
            Physics.gravity = new Vector3(0f, -9.81f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.12f, 0.15f, 0.13f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.016f;
            RenderSettings.fogColor = new Color(0.11f, 0.16f, 0.14f, 1f);
            Camera.main?.gameObject.SetActive(false);

            Material waterMat = MakeMat(new Color(0.11f, 0.17f, 0.15f, 1f));
            Material landMat = MakeMat(new Color(0.18f, 0.21f, 0.14f, 1f));
            Material lanternMat = MakeMat(new Color(1.00f, 0.67f, 0.24f, 1f));
            Material boatMat = MakeMat(new Color(0.24f, 0.20f, 0.12f, 1f));
            Material gatorMat = MakeMat(new Color(0.16f, 0.28f, 0.14f, 1f));
            Material sandbarMat = MakeMat(new Color(0.60f, 0.50f, 0.30f, 1f));

            var gameManager = new GameObject("GameManager").AddComponent<GameManager>();
            gameManager.baseRoundTime = 95f;
            gameManager.sinkThreshold = 100f;
            gameManager.sinkDecayPerSecond = 7f;
            gameManager.difficulty = GameManager.DifficultyMode.Medium;

            CreateWater(waterMat);
            CreateWaves();
            CreateTerrain(landMat, sandbarMat);
            BoatController boat = CreateBoat(boatMat);
            gameManager.playerBoat = boat;
            CreateCamera(boat.transform);
            CreateLanterns(lanternMat);
            CreateGators(gatorMat);
            CreateUI(gameManager);
        }

        private Material MakeMat(Color color)
        {
            var mat = CreateSafeMaterial(
                new[]
                {
                    "Standard",
                    "Universal Render Pipeline/Lit",
                    "Legacy Shaders/Diffuse",
                    "Unlit/Color",
                    "Sprites/Default"
                },
                color);
            return mat;
        }

        private Material CreateSafeMaterial(string[] shaderNames, Color color)
        {
            Shader shader = null;
            for (int i = 0; i < shaderNames.Length; i++)
            {
                shader = Shader.Find(shaderNames[i]);
                if (shader != null) break;
            }

            if (shader == null)
            {
                Debug.LogError("RuntimeBootstrap could not find a valid shader. Using disabled renderer material fallback.");
                return null;
            }

            var mat = new Material(shader);
            mat.color = color;
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.08f);
            return mat;
        }

        private void CreateWater(Material waterMat)
        {
            var water = new GameObject("Water");
            water.transform.position = Vector3.zero;
            var mf = water.AddComponent<MeshFilter>();
            var mr = water.AddComponent<MeshRenderer>();
            mr.sharedMaterial = waterMat;
            var dm = water.AddComponent<DynamicMesh>();
            dm.width = 130;
            dm.height = 130;
            dm.ApplyToMeshFilter(dm.GenerateGridMesh());
            water.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh;
            water.AddComponent<WaveApplier>();
        }

        private void CreateWaves()
        {
            CreateWave("Wave_A", new Vector3(0f, 0f, 0f), 1.35f, 1.10f, new Vector2(8f, 9f), 0f);
            CreateWave("Wave_B", new Vector3(14f, 0f, -10f), 1.00f, 1.35f, new Vector2(5.5f, 7f), 0f);
            CreateWave("Wave_C", new Vector3(-18f, 0f, 14f), 0.80f, 0.90f, new Vector2(11f, 8f), 26f);
            CreateWave("Wave_D", new Vector3(4f, 0f, 38f), 0.62f, 1.70f, new Vector2(4.5f, 5.5f), 20f);
        }

        private void CreateWave(string name, Vector3 pos, float height, float speed, Vector2 scale, float radius)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            var wave = go.AddComponent<Wave>();
            wave.maxHeight = height;
            wave.speed = speed;
            wave.scale = scale;
            wave.radius = radius;
        }

        private void CreateTerrain(Material landMat, Material sandbarMat)
        {
            // Outer banks
            CreateBank(new Vector3(0f, -0.7f, 65f), new Vector3(140f, 1f, 14f), landMat);
            CreateBank(new Vector3(0f, -0.7f, -65f), new Vector3(140f, 1f, 14f), landMat);
            CreateBank(new Vector3(-65f, -0.7f, 0f), new Vector3(14f, 1f, 140f), landMat);
            CreateBank(new Vector3(65f, -0.7f, 0f), new Vector3(14f, 1f, 140f), landMat);

            // Main river walls
            CreateBank(new Vector3(-18f, -0.2f, -25f), new Vector3(22f, 1f, 70f), landMat);
            CreateBank(new Vector3(18f, -0.2f, -25f), new Vector3(22f, 1f, 70f), landMat);
            // Middle islands to create branching that rejoins
            CreateBank(new Vector3(0f, -0.2f, -2f), new Vector3(14f, 1f, 24f), landMat);
            CreateBank(new Vector3(-7f, -0.2f, 25f), new Vector3(18f, 1f, 18f), landMat);
            CreateBank(new Vector3(10f, -0.2f, 28f), new Vector3(16f, 1f, 16f), landMat);
            CreateBank(new Vector3(0f, -0.2f, 48f), new Vector3(10f, 1f, 10f), landMat);

            // Decorative stumps / edges
            for (int i = -2; i <= 2; i++)
            {
                CreateCylinder(new Vector3(-27f, -0.2f, i * 18f), new Vector3(4f, 2f, 4f), landMat);
                CreateCylinder(new Vector3(27f, -0.2f, i * 18f + 6f), new Vector3(4f, 2.3f, 4f), landMat);
            }

            // Raised sandbar walls / islands
            CreateSandbar(new Vector3(-10f, 0.62f, 2f), new Vector3(7f, 1.45f, 5f), sandbarMat);
            CreateSandbar(new Vector3(13f, 0.62f, 18f), new Vector3(6f, 1.45f, 6f), sandbarMat);
            CreateSandbar(new Vector3(-12f, 0.62f, 36f), new Vector3(8f, 1.45f, 5f), sandbarMat);
            CreateSandbar(new Vector3(0f, 0.62f, 56f), new Vector3(7f, 1.45f, 5f), sandbarMat);
        }

        private void CreateBank(Vector3 pos, Vector3 scale, Material mat)
        {
            var bank = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bank.name = "Bank";
            bank.transform.position = pos;
            bank.transform.localScale = scale;
            bank.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private void CreateCylinder(Vector3 pos, Vector3 scale, Material mat)
        {
            var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.name = "Stump";
            cyl.transform.position = pos;
            cyl.transform.localScale = scale;
            cyl.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private void CreateSandbar(Vector3 pos, Vector3 scale, Material mat)
        {
            var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = "SandbarWall";
            bar.transform.position = pos;
            bar.transform.localScale = scale;
            bar.GetComponent<Renderer>().sharedMaterial = mat;
            var col = bar.GetComponent<Collider>();
            col.isTrigger = false;
        }

        private BoatController CreateBoat(Material boatMat)
        {
            var root = new GameObject("PlayerBoat");
            root.transform.position = new Vector3(0f, 0.7f, -38f);

            var rb = root.AddComponent<Rigidbody>();
            rb.mass = 14f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var status = root.AddComponent<BoatStatus>();
            var controller = root.AddComponent<BoatController>();
            controller.rb = rb;
            controller.status = status;
            controller.forwardForce = 21f;
            controller.reverseForce = 12.0f;
            controller.turnTorque = 14f;
            controller.pivotTurnTorque = 25f;
            controller.maxForwardSpeed = 5.6f;
            controller.maxReverseSpeed = 2.6f;
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

            var hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hull.name = "Hull";
            hull.transform.SetParent(root.transform, false);
            hull.transform.localScale = new Vector3(1.8f, 0.4f, 3.4f);
            hull.transform.localPosition = Vector3.zero;
            hull.GetComponent<Renderer>().sharedMaterial = boatMat;
            Object.Destroy(hull.GetComponent<BoxCollider>());
            var boatCollider = root.AddComponent<BoxCollider>();
            boatCollider.size = new Vector3(1.8f, 0.6f, 3.4f);
            boatCollider.center = Vector3.zero;

            var cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabin";
            cabin.transform.SetParent(root.transform, false);
            cabin.transform.localScale = new Vector3(1.1f, 0.5f, 1.2f);
            cabin.transform.localPosition = new Vector3(0f, 0.45f, -0.15f);
            cabin.GetComponent<Renderer>().sharedMaterial = boatMat;
            Object.Destroy(cabin.GetComponent<BoxCollider>());

            var lampGo = new GameObject("BoatLamp");
            lampGo.transform.SetParent(root.transform, false);
            lampGo.transform.localPosition = new Vector3(0f, 0.75f, 0.9f);
            var light = lampGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 24f;
            light.intensity = 5.8f;
            light.color = new Color(1f, 0.72f, 0.35f, 1f);
            controller.boatLamp = light;

            CreateBuoy(root.transform, rb, new Vector3(-0.65f, -0.3f, -1.1f));
            CreateBuoy(root.transform, rb, new Vector3(0.65f, -0.3f, -1.1f));
            CreateBuoy(root.transform, rb, new Vector3(-0.65f, -0.3f, 1.1f));
            CreateBuoy(root.transform, rb, new Vector3(0.65f, -0.3f, 1.1f));

            var wake = new GameObject("WakeEffects");
            wake.transform.SetParent(root.transform, false);
            wake.transform.localPosition = new Vector3(0f, 0.02f, -1.85f);
            var wakeFx = wake.AddComponent<BoatWakeEffects>();
            wakeFx.boat = controller;
            wakeFx.bubbleParticles = CreateBubbleParticles(wake.transform);
            wakeFx.wakeTrail = CreateWakeTrail(wake.transform);

            return controller;
        }

        private void CreateBuoy(Transform parent, Rigidbody rb, Vector3 localPos)
        {
            var b = new GameObject("Buoy");
            b.transform.SetParent(parent, false);
            b.transform.localPosition = localPos;
            var buoy = b.AddComponent<Buoy>();
            buoy.targetRigidbody = rb;
            buoy.buoyancy = 1.4f;
            buoy.waterDrag = 0.14f;
            buoy.waterAngularDrag = 0.08f;
        }

        private ParticleSystem CreateBubbleParticles(Transform parent)
        {
            var go = new GameObject("BubbleTrail");
            go.transform.SetParent(parent, false);
            go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.6f;
            main.startSpeed = 0.9f;
            main.startSize = 0.18f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 300;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 10f;
            shape.radius = 0.25f;
            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.55f, 1f, 0.72f), 0f),
                    new GradientColorKey(new Color(0.16f, 0.82f, 0.42f), 1f)
                },
                new[] { new GradientAlphaKey(0.75f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            var particleMat = CreateSafeMaterial(
                new[]
                {
                    "Universal Render Pipeline/Particles/Unlit",
                    "Particles/Standard Unlit",
                    "Unlit/Color",
                    "Sprites/Default",
                    "Legacy Shaders/Particles/Alpha Blended"
                },
                new Color(0.42f, 1f, 0.62f, 0.55f));
            renderer.sharedMaterial = particleMat;
            return ps;
        }

        private TrailRenderer CreateWakeTrail(Transform parent)
        {
            var go = new GameObject("WakeTrail");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            var tr = go.AddComponent<TrailRenderer>();
            tr.alignment = LineAlignment.View;
            tr.time = 0f;
            tr.startWidth = 0.35f;
            tr.endWidth = 0.02f;
            var mat = CreateSafeMaterial(
                new[] { "Sprites/Default", "Unlit/Color", "Legacy Shaders/Particles/Alpha Blended" },
                new Color(0.38f, 1f, 0.62f, 0.5f));
            tr.sharedMaterial = mat;
            return tr;
        }

        private void CreateCamera(Transform target)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.025f, 0.04f, 0.035f, 1f);
            camGo.transform.position = new Vector3(0f, 5.5f, -47f);
            camGo.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
            var follow = camGo.AddComponent<TopDownCameraFollow>();
            follow.target = target;
            follow.offset = new Vector3(0f, 5.5f, -9.5f);
            follow.lookTargetOffset = new Vector3(0f, 1.3f, 5.5f);
            follow.positionSmoothTime = 0.12f;
            follow.rotationLerpSpeed = 8f;
            follow.rotateWithBoat = true;
        }

        private void CreateLanterns(Material lanternMat)
        {
            Vector3[] candidatePoints = new[]
            {
                new Vector3(0f, 0.45f, -28f),
                new Vector3(0f, 0.45f, -18f),
                new Vector3(-12f, 0.45f, -18f),
                new Vector3(12f, 0.45f, -18f),
                new Vector3(-13f, 0.45f, -6f),
                new Vector3(13f, 0.45f, -4f),
                new Vector3(-16f, 0.45f, 12f),
                new Vector3(16f, 0.45f, 13f),
                new Vector3(-22f, 0.45f, 24f),
                new Vector3(22f, 0.45f, 26f),
                new Vector3(-21f, 0.45f, 34f),
                new Vector3(21f, 0.45f, 36f),
                new Vector3(-10f, 0.45f, 42f),
                new Vector3(11f, 0.45f, 44f),
                new Vector3(0f, 0.45f, 58f),
                new Vector3(-14f, 0.45f, 57f),
                new Vector3(14f, 0.45f, 57f),
            };

            int spawned = 0;
            for (int i = 0; i < candidatePoints.Length && spawned < 8; i++)
            {
                if (!IsLanternSpotClear(candidatePoints[i], 2.1f))
                {
                    continue;
                }

                var root = new GameObject($"Lantern_{spawned + 1}");
                root.transform.position = candidatePoints[i];
                var trigger = root.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = 1.35f;
                var pickup = root.AddComponent<LanternPickup>();
                root.AddComponent<WaterFloatFollower>().heightOffset = 0.38f;

                var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                body.transform.SetParent(root.transform, false);
                body.transform.localScale = new Vector3(0.32f, 0.28f, 0.32f);
                body.GetComponent<Renderer>().sharedMaterial = lanternMat;
                Object.Destroy(body.GetComponent<Collider>());

                var glow = new GameObject("Light");
                glow.transform.SetParent(root.transform, false);
                var light = glow.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 20f;
                light.intensity = 6.4f;
                light.color = new Color(1f, 0.72f, 0.30f, 1f);
                pickup.lanternLight = light;
                pickup.renderers = new[] { body.GetComponent<Renderer>() };
                spawned++;
            }
        }

        private bool IsLanternSpotClear(Vector3 position, float clearRadius)
        {
            Collider[] hits = Physics.OverlapSphere(position + Vector3.up * 0.35f, clearRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider hit = hits[i];
                if (hit == null || hit.isTrigger)
                {
                    continue;
                }

                if (hit.GetComponent<DynamicMesh>() != null || hit.GetComponentInParent<DynamicMesh>() != null)
                {
                    continue;
                }

                if (hit.gameObject.name == "Water")
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private void CreateGators(Material gatorMat)
        {
            Vector3[] points = new[]
            {
                new Vector3(8f, 0.2f, -18f),
                new Vector3(-16f, 0.2f, 10f),
                new Vector3(17f, 0.2f, 24f),
                new Vector3(-4f, 0.2f, 50f),
            };

            for (int i = 0; i < points.Length; i++)
            {
                var root = new GameObject($"Gator_{i+1}");
                root.transform.position = points[i];
                var sphere = root.AddComponent<SphereCollider>();
                sphere.isTrigger = true;
                sphere.radius = 7.2f;
                var haz = root.AddComponent<GatorHazard>();
                haz.detectionRange = 7.2f;
                haz.attackRange = 3.1f;
                haz.moveSpeed = 1.05f;
                haz.lungeTurnSpeed = 2.8f;
                haz.slowMultiplier = 0.88f;
                haz.turnMultiplier = 0.94f;
                haz.escapeSpeedThreshold = 1.15f;
                haz.dangerPerSecond = 9f;

                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.transform.SetParent(root.transform, false);
                body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                body.transform.localScale = new Vector3(1.0f, 0.5f, 2.1f);
                body.GetComponent<Renderer>().sharedMaterial = gatorMat;
                Object.Destroy(body.GetComponent<Collider>());
            }
        }

        private void CreateUI(GameManager gm)
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            gm.mainCanvas = canvas;

            if (FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            gm.titlePanel = CreatePanel(canvas.transform, "TitlePanel", new Color(0f,0f,0f,0.58f));
            CreateAnchoredText(gm.titlePanel.transform, "Title", "Lantern Drift", 76, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 220f), new Vector2(900f, 120f));
            CreateAnchoredText(gm.titlePanel.transform, "Body", "Skim the blackwater, collect every lantern, and avoid the things beneath.\nUse WASD or Arrow Keys to steer.", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(1200f, 140f));
            gm.difficultyText = CreateAnchoredText(gm.titlePanel.transform, "DifficultyLabel", "Difficulty: Medium", 32, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(520f, 50f));

            var easyBtn = CreateButton(gm.titlePanel.transform, "EasyButton", "Easy", new Vector2(-240f, -70f), new Vector2(180f, 64f));
            easyBtn.onClick.AddListener(() => gm.SetDifficultyEasy());
            var medBtn = CreateButton(gm.titlePanel.transform, "MediumButton", "Medium", new Vector2(0f, -70f), new Vector2(180f, 64f));
            medBtn.onClick.AddListener(() => gm.SetDifficultyMedium());
            var hardBtn = CreateButton(gm.titlePanel.transform, "HardButton", "Hard", new Vector2(240f, -70f), new Vector2(180f, 64f));
            hardBtn.onClick.AddListener(() => gm.SetDifficultyHard());

            var playBtn = CreateButton(gm.titlePanel.transform, "PlayButton", "Play", new Vector2(0f, -180f), new Vector2(300f, 84f));
            playBtn.onClick.AddListener(() => gm.StartGame());

            gm.hudPanel = new GameObject("HudPanel", typeof(RectTransform));
            gm.hudPanel.transform.SetParent(canvas.transform, false);
            var hudRt = gm.hudPanel.GetComponent<RectTransform>();
            hudRt.anchorMin = Vector2.zero;
            hudRt.anchorMax = Vector2.one;
            hudRt.offsetMin = Vector2.zero;
            hudRt.offsetMax = Vector2.zero;
            gm.timerText = CreateAnchoredText(gm.hudPanel.transform, "TimerText", "Time: 0", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(300f, 42f));
            gm.lanternText = CreateAnchoredText(gm.hudPanel.transform, "LanternText", "Lanterns: 0/0", 28, TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -66f), new Vector2(360f, 42f));
            gm.sinkText = CreateAnchoredText(gm.hudPanel.transform, "SinkText", "Sink: 0%", 28, TextAnchor.MiddleRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-34f, -28f), new Vector2(360f, 42f));

            gm.endPanel = CreatePanel(canvas.transform, "EndPanel", new Color(0f,0f,0f,0.62f));
            gm.endTitleText = CreateAnchoredText(gm.endPanel.transform, "EndTitle", "Run Ended", 62, TextAnchor.MiddleCenter, new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0f, 140f), new Vector2(900f, 100f));
            gm.endBodyText = CreateAnchoredText(gm.endPanel.transform, "EndBody", "", 30, TextAnchor.MiddleCenter, new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0f, 35f), new Vector2(1100f, 160f));
            var restartBtn = CreateButton(gm.endPanel.transform, "RestartButton", "Restart", new Vector2(-150f, -90f), new Vector2(220f, 70f));
            restartBtn.onClick.AddListener(() => gm.RestartLevel());
            var titleBtn = CreateButton(gm.endPanel.transform, "TitleButton", "Back To Title", new Vector2(150f, -90f), new Vector2(260f, 70f));
            titleBtn.onClick.AddListener(() => gm.ShowTitleScreen());

            gm.SetDifficultyMedium();
        }

        private GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor textAnchor, Vector2 anchoredPos, Vector2 size, bool rightAnchor=false)
        {
            Vector2 uiAnchor = rightAnchor ? new Vector2(1f, 1f) : new Vector2(0.5f, 1f);
            TextAnchor alignment = rightAnchor ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
            return CreateAnchoredText(parent, name, content, fontSize, alignment, uiAnchor, uiAnchor, anchoredPos, size);
        }

        private Text CreateAnchoredText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            var txt = go.GetComponent<Text>();
            txt.text = content;
            txt.alignment = alignment;
            txt.fontSize = fontSize;
            txt.color = new Color(0.95f, 0.88f, 0.72f, 1f);
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return txt;
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            go.GetComponent<Image>().color = new Color(0.22f, 0.17f, 0.09f, 0.92f);
            var button = go.GetComponent<Button>();
            var txt = CreateText(go.transform, "Label", label, 28, TextAnchor.MiddleCenter, Vector2.zero, size);
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.offsetMin = Vector2.zero;
            txt.rectTransform.offsetMax = Vector2.zero;
            return button;
        }
    }
}
