using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BlockSort.Presentation
{
    public sealed class GameJuice : MonoBehaviour
    {
        AudioSource _sfx;
        AudioSource _music;
        RectTransform _fxRoot;
        readonly Dictionary<string, AudioClip> _clips = new();
        readonly Dictionary<string, Sprite> _sprites = new();

        public static GameJuice Create(Transform owner, RectTransform canvas)
        {
            var juice = owner.gameObject.AddComponent<GameJuice>();
            juice.Build(canvas);
            return juice;
        }

        void Build(RectTransform canvas)
        {
            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.spatialBlend = 0f;
            _music = gameObject.AddComponent<AudioSource>();
            _music.playOnAwake = false;
            _music.loop = true;
            _music.spatialBlend = 0f;
            _music.volume = 0.16f;
            LoadClip("tap", "Audio/SFX/tap");
            LoadClip("pour", "Audio/SFX/pour");
            LoadClip("clear", "Audio/SFX/clear");
            LoadClip("undo", "Audio/SFX/undo");
            LoadClip("win", "Audio/SFX/win");
            var loop = Resources.Load<AudioClip>("Audio/Music/workshop_loop");
            if (loop != null)
            {
                _music.clip = loop;
                _music.Play();
            }

            var fx = new GameObject("JuiceFx", typeof(RectTransform));
            _fxRoot = fx.GetComponent<RectTransform>();
            _fxRoot.SetParent(canvas, false);
            Stretch(_fxRoot);
            var group = fx.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        public Sprite Sprite(string resourceName)
        {
            if (_sprites.TryGetValue(resourceName, out var cached) && cached != null)
            {
                return cached;
            }

            var texture = Resources.Load<Texture2D>(resourceName);
            if (texture == null)
            {
                return null;
            }

            var sprite = UnityEngine.Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            _sprites[resourceName] = sprite;
            return sprite;
        }

        public void Play(string id)
        {
            if (_clips.TryGetValue(id, out var clip) && clip != null)
            {
                _sfx.PlayOneShot(clip);
            }
        }

        public void Burst(Vector2 anchoredPosition, Color color, int count = 18)
        {
            StartCoroutine(BurstRoutine(anchoredPosition, color, count));
        }

        IEnumerator BurstRoutine(Vector2 anchoredPosition, Color color, int count)
        {
            var dot = Sprite("Art/particle_dot");
            var bits = new List<(RectTransform rect, Vector2 velocity)>();
            for (var i = 0; i < count; i++)
            {
                var image = new GameObject("Spark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<Image>();
                image.raycastTarget = false;
                image.sprite = dot;
                image.color = Color.Lerp(color, Color.white, Random.value * 0.35f);
                var rect = image.rectTransform;
                rect.SetParent(_fxRoot, false);
                rect.sizeDelta = Vector2.one * Random.Range(18f, 34f);
                rect.anchoredPosition = anchoredPosition;
                var angle = Random.Range(0f, Mathf.PI * 2f);
                var speed = Random.Range(420f, 980f);
                bits.Add((rect, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed));
            }

            var time = 0f;
            const float duration = 0.55f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                var delta = Time.unscaledDeltaTime;
                var fade = 1f - time / duration;
                for (var i = 0; i < bits.Count; i++)
                {
                    var (rect, velocity) = bits[i];
                    if (rect == null)
                    {
                        continue;
                    }

                    velocity += new Vector2(0f, -1600f) * delta;
                    rect.anchoredPosition += velocity * delta;
                    rect.localRotation = Quaternion.Euler(0f, 0f, time * 360f);
                    var image = rect.GetComponent<Image>();
                    var tint = image.color;
                    tint.a = fade;
                    image.color = tint;
                    bits[i] = (rect, velocity);
                }

                yield return null;
            }

            foreach (var (rect, _) in bits)
            {
                if (rect != null)
                {
                    Destroy(rect.gameObject);
                }
            }
        }

        void LoadClip(string id, string path)
        {
            var clip = Resources.Load<AudioClip>(path);
            if (clip != null)
            {
                _clips[id] = clip;
            }
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
