using UnityEngine;
using LanternDrift.Water;
using LanternDrift.Boat;

namespace LanternDrift.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class LanternPickup : MonoBehaviour
    {
        public Light lanternLight;
        public Renderer[] renderers;
        public ParticleSystem collectBurst;
        public bool IsCollected { get; private set; }

        private Color[] originalColors;

        private void Awake()
        {
            CacheReferencesAndColors();
        }

        private void Start()
        {
            GameManager.Instance?.RegisterLantern(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsCollected)
            {
                return;
            }

            BoatController boat = other.GetComponentInParent<BoatController>();
            if (boat == null)
            {
                return;
            }

            GameManager.Instance?.CollectLantern(this);
        }

        public void SetCollected()
        {
            CacheReferencesAndColors();
            IsCollected = true;

            if (lanternLight != null)
            {
                lanternLight.enabled = false;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material.HasProperty("_Color"))
                {
                    Color c = renderers[i].material.color;
                    c.a = 0.15f;
                    renderers[i].material.color = c;
                }
            }

            if (collectBurst != null)
            {
                collectBurst.Play();
            }
        }

        public void ResetPickup()
        {
            CacheReferencesAndColors();
            IsCollected = false;

            if (lanternLight != null)
            {
                lanternLight.enabled = true;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material.HasProperty("_Color"))
                {
                    Color restored = i < originalColors.Length ? originalColors[i] : renderers[i].material.color;
                    restored.a = 1f;
                    renderers[i].material.color = restored;
                }
            }
        }

        private void CacheReferencesAndColors()
        {
            if (lanternLight == null)
            {
                lanternLight = GetComponentInChildren<Light>(true);
            }

            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>(true);
            }

            if (renderers == null)
            {
                renderers = new Renderer[0];
            }

            if (originalColors == null || originalColors.Length != renderers.Length)
            {
                originalColors = new Color[renderers.Length];
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null && renderers[i].material.HasProperty("_Color"))
                    {
                        originalColors[i] = renderers[i].material.color;
                    }
                    else
                    {
                        originalColors[i] = Color.white;
                    }
                }
            }
        }
    }
}
