using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LanternDrift.Boat;

namespace LanternDrift.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        public enum DifficultyMode
        {
            Easy,
            Medium,
            Hard
        }

        public static GameManager Instance { get; private set; }

        [Header("Gameplay")]
        public DifficultyMode difficulty = DifficultyMode.Medium;
        public float baseRoundTime = 95f;
        public float sinkThreshold = 100f;
        public float sinkDecayPerSecond = 7f;

        [Header("References")]
        public BoatController playerBoat;
        public Canvas mainCanvas;
        public GameObject titlePanel;
        public GameObject hudPanel;
        public GameObject endPanel;

        [Header("HUD")]
        public Text lanternText;
        public Text timerText;
        public Text sinkText;
        public Text endTitleText;
        public Text endBodyText;
        public Text difficultyText;

        public float TimeRemaining { get; private set; }
        public bool GameRunning { get; private set; }
        public float SinkMeter { get; private set; }

        private readonly List<LanternPickup> lanterns = new List<LanternPickup>();
        private const string BaseTitleBody = "Collect every lantern before time runs out.\nWASD / Arrow Keys to steer your swamp boat.";
        private int collectedLanterns;
        private bool titleRefreshPending;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            RegisterLanternsInScene();
            ShowTitleScreen();
        }

        private void Update()
        {
            if (!GameRunning)
            {
                if (titleRefreshPending)
                {
                    titleRefreshPending = false;
                    RefreshTitleDifficultyLabel();
                }
                return;
            }

            TimeRemaining -= Time.deltaTime;
            SinkMeter = Mathf.Max(0f, SinkMeter - sinkDecayPerSecond * Time.deltaTime);

            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                EndGame(false, "The swamp swallowed the last light.");
            }
            else if (SinkMeter >= sinkThreshold)
            {
                EndGame(false, "The alligators dragged you under.");
            }

            UpdateHud();
        }

        public void RegisterLantern(LanternPickup lantern)
        {
            if (lantern != null && !lanterns.Contains(lantern))
            {
                lanterns.Add(lantern);
            }
        }

        public void RegisterLanternsInScene()
        {
            lanterns.Clear();
            var sceneLanterns = FindObjectsOfType<LanternPickup>(true);
            for (int i = 0; i < sceneLanterns.Length; i++)
            {
                RegisterLantern(sceneLanterns[i]);
            }
        }

        public void StartGame()
        {
            collectedLanterns = 0;
            SinkMeter = 0f;
            TimeRemaining = GetRoundTimeForDifficulty();
            GameRunning = true;

            foreach (LanternPickup lantern in lanterns)
            {
                if (lantern != null)
                {
                    lantern.ResetPickup();
                }
            }

            if (playerBoat != null)
            {
                playerBoat.canControl = true;
                playerBoat.transform.position = new Vector3(0f, 1f, -38f);
                playerBoat.transform.rotation = Quaternion.identity;
                playerBoat.rb.velocity = Vector3.zero;
                playerBoat.rb.angularVelocity = Vector3.zero;
            }

            if (titlePanel != null) titlePanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(true);
            if (endPanel != null) endPanel.SetActive(false);
            UpdateHud();
        }

        public void CollectLantern(LanternPickup lantern)
        {
            if (!GameRunning || lantern == null || lantern.IsCollected)
            {
                return;
            }

            lantern.SetCollected();
            collectedLanterns++;
            UpdateHud();

            if (collectedLanterns >= lanterns.Count)
            {
                EndGame(true, "Every lantern now burns aboard your boat.");
            }
        }

        public void AddSink(float amount)
        {
            if (!GameRunning)
            {
                return;
            }

            SinkMeter += Mathf.Max(0f, amount);
        }

        public void ShowTitleScreen()
        {
            GameRunning = false;
            if (playerBoat != null)
            {
                playerBoat.canControl = false;
            }

            if (titlePanel != null) titlePanel.SetActive(true);
            if (hudPanel != null) hudPanel.SetActive(false);
            if (endPanel != null) endPanel.SetActive(false);
            titleRefreshPending = true;
        }

        public void SetDifficultyEasy()
        {
            difficulty = DifficultyMode.Easy;
            RefreshTitleDifficultyLabel();
        }

        public void SetDifficultyMedium()
        {
            difficulty = DifficultyMode.Medium;
            RefreshTitleDifficultyLabel();
        }

        public void SetDifficultyHard()
        {
            difficulty = DifficultyMode.Hard;
            RefreshTitleDifficultyLabel();
        }

        public void RestartLevel()
        {
            StartGame();
        }

        private void EndGame(bool won, string body)
        {
            GameRunning = false;
            if (playerBoat != null)
            {
                playerBoat.canControl = false;
            }

            if (hudPanel != null) hudPanel.SetActive(false);
            if (endPanel != null) endPanel.SetActive(true);
            if (endTitleText != null) endTitleText.text = won ? "Lantern Drift Complete" : "Run Ended";
            if (endBodyText != null)
            {
                endBodyText.text = won
                    ? $"{body}\nTime Remaining: {Mathf.CeilToInt(TimeRemaining)}s"
                    : body;
            }
        }

        private void UpdateHud()
        {
            if (lanternText != null)
            {
                lanternText.text = $"Lanterns: {collectedLanterns}/{lanterns.Count}";
            }

            if (timerText != null)
            {
                timerText.text = $"Time: {Mathf.CeilToInt(TimeRemaining)}";
            }

            if (sinkText != null)
            {
                sinkText.text = $"Sink: {Mathf.RoundToInt((SinkMeter / Mathf.Max(1f, sinkThreshold)) * 100f)}%";
            }
        }

        private void RefreshTitleDifficultyLabel()
        {
            if (difficultyText != null)
            {
                difficultyText.text = $"Difficulty: {difficulty}";
            }
        }

        private float GetRoundTimeForDifficulty()
        {
            switch (difficulty)
            {
                case DifficultyMode.Easy:
                    return baseRoundTime + 25f;
                case DifficultyMode.Hard:
                    return baseRoundTime - 18f;
                default:
                    return baseRoundTime;
            }
        }
    }
}
