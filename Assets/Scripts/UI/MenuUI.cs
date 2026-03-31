using UnityEngine;
using LanternDrift.Gameplay;

namespace LanternDrift.UI
{
    public class MenuUI : MonoBehaviour
    {
        public void Play()
        {
            GameManager.Instance?.StartGame();
        }

        public void Restart()
        {
            GameManager.Instance?.RestartLevel();
        }

        public void BackToTitle()
        {
            GameManager.Instance?.ShowTitleScreen();
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
