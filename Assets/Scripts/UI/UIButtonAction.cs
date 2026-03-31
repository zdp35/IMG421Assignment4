using UnityEngine;
using UnityEngine.UI;
using LanternDrift.Gameplay;

namespace LanternDrift.UI
{
    [RequireComponent(typeof(Button))]
    public class UIButtonAction : MonoBehaviour
    {
        public enum ActionType
        {
            Play,
            Restart,
            BackToTitle,
            Quit
        }

        public ActionType actionType;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(HandleClick);
        }

        private void HandleClick()
        {
            switch (actionType)
            {
                case ActionType.Play:
                    GameManager.Instance?.StartGame();
                    break;
                case ActionType.Restart:
                    GameManager.Instance?.RestartLevel();
                    break;
                case ActionType.BackToTitle:
                    GameManager.Instance?.ShowTitleScreen();
                    break;
                case ActionType.Quit:
                    Application.Quit();
                    break;
            }
        }
    }
}
