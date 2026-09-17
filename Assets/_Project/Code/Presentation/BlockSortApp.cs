using System.Collections.Generic;
using BlockSort.Core;
using BlockSort.Gameplay;
using BlockSort.Levels;
using BlockSort.Meta;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace BlockSort.Presentation
{
    /// <summary>
    /// Builds the portrait hybrid-casual shell at runtime so the Boot scene can stay empty.
    /// </summary>
    public sealed class BlockSortApp : MonoBehaviour
    {
        static readonly Color Wood = new(0.28f, 0.12f, 0.08f);
        static readonly Color[] CubeColors =
        {
            new(0.91f, 0.25f, 0.30f),
            new(1f, 0.71f, 0.09f),
            new(0.64f, 0.29f, 0.89f),
            new(0.15f, 0.73f, 0.71f),
            new(0.55f, 0.80f, 0.21f),
            new(0.94f, 0.48f, 0.21f)
        };
        static readonly string[] CubeIcons = { "▲", "●", "★", "●", "◆", "♥" };

        [SerializeField] WorldDatabase worldDatabase;
        [SerializeField] TextAsset worldJson;

        Canvas _canvas;
        RectTransform _home;
        RectTransform _levels;
        RectTransform _game;
        RectTransform _complete;
        RectTransform _boardRoot;
        Text _homeCoins;
        Text _gameCoins;
        Text _levelsCoins;
        Text _nextLevel;
        Text _levelNumber;
        Text _moves;
        Text _undoCount;
        Text _completeLevel;
        Text _reward;
        BlockSortSession _session;
        LevelDefinition _current;
        int _level = 1;
        bool _extraUsed;
        readonly List<int> _cleared = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBoot()
        {
            if (FindFirstObjectByType<BlockSortApp>() != null)
            {
                return;
            }

            var root = new GameObject("BlockSortApp");
            DontDestroyOnLoad(root);
            root.AddComponent<BlockSortApp>();
        }

        void Awake()
        {
            foreach (var value in ProgressSave.ClearedLevels)
            {
                if (value > 0 && !_cleared.Contains(value))
                {
                    _cleared.Add(value);
                }
            }

            _level = ProgressSave.CurrentLevel;
            EnsureEventSystem();
            BuildUi();
            Show(_home);
        }

        WorldJson World()
        {
            if (worldJson == null)
            {
                worldJson = Resources.Load<TextAsset>("Levels/world-01");
            }

            return LevelJson.Parse(worldJson);
        }

        LevelDefinition Definition(int number)
        {
            if (worldDatabase != null)
            {
                var fromDb = worldDatabase.GetLevel(number);
                if (fromDb != null)
                {
                    return fromDb;
                }
            }

            var world = World();
            foreach (var record in world.levels)
            {
                if (record.number == number)
                {
                    return LevelJson.ToDefinition(record, world.worldId);
                }
            }

            return LevelJson.ToDefinition(world.levels[Mathf.Clamp(number - 1, 0, world.levels.Length - 1)], world.worldId);
        }

        void EnsureEventSystem()
        {
            var existing = FindFirstObjectByType<EventSystem>();
            if (existing == null)
            {
                var system = new GameObject("EventSystem");
                existing = system.AddComponent<EventSystem>();
            }

            if (existing.GetComponent<InputSystemUIInputModule>() == null)
            {
                existing.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }

        void BuildUi()
        {
            _canvas = new GameObject("Canvas").AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var raycaster = _canvas.gameObject.AddComponent<GraphicRaycaster>();
            raycaster.ignoreReversedGraphics = true;
            var scaler = _canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            _canvas.transform.SetParent(transform, false);
            var bg = Image("Background", _canvas.transform, new Color(0.16f, 0.07f, 0.05f));
            Stretch(bg.rectTransform);

            _home = Screen("Home");
            Header(_home, out _homeCoins);
            Label(_home, "SORT THE COLORS", 72, new Vector2(0, 420));
            _nextLevel = Label(_home, "PLAY LEVEL 1", 40, new Vector2(0, 80));
            Button(_home, "PLAY", new Vector2(0, -80), new Vector2(560, 140), () => StartLevel(_level));
            Button(_home, "LEVELS", new Vector2(0, -260), new Vector2(360, 90), ShowLevels);

            _levels = Screen("Levels");
            Header(_levels, out _levelsCoins);
            Label(_levels, "LEVELS", 56, new Vector2(0, 640));
            Button(_levels, "BACK", new Vector2(0, -740), new Vector2(280, 80), () => Show(_home));

            _game = Screen("Game");
            Header(_game, out _gameCoins, true);
            _levelNumber = Label(_game, "LEVEL 1", 42, new Vector2(0, 760));
            _moves = Label(_game, "0 MOVES", 28, new Vector2(0, 700));
            _boardRoot = Panel("Board", _game, new Vector2(0, 40), new Vector2(980, 1180), false);
            Button(_game, "UNDO", new Vector2(-220, -780), new Vector2(280, 90), Undo);
            _undoCount = Label(_game, "3", 24, new Vector2(-90, -730));
            Button(_game, "EXTRA", new Vector2(220, -780), new Vector2(280, 90), ExtraSlot);
            Button(_game, "HOME", new Vector2(0, -900), new Vector2(220, 70), () => Show(_home));

            _complete = Screen("Complete");
            Label(_complete, "COLOR PERFECT", 64, new Vector2(0, 240));
            _completeLevel = Label(_complete, "LEVEL 1", 36, new Vector2(0, 80));
            _reward = Label(_complete, "+30", 48, new Vector2(0, -40));
            Button(_complete, "NEXT", new Vector2(0, -240), new Vector2(480, 120), NextLevel);
        }

        void Header(RectTransform parent, out Text coins, bool compact = false)
        {
            coins = Label(parent, ProgressSave.Coins.ToString(), 32, new Vector2(420, compact ? 860 : 820));
        }

        void Show(RectTransform screen)
        {
            _home.gameObject.SetActive(screen == _home);
            _levels.gameObject.SetActive(screen == _levels);
            _game.gameObject.SetActive(screen == _game);
            _complete.gameObject.SetActive(screen == _complete);
            RefreshMeta();
            EventBus<GameState>.Raise(screen == _game ? GameState.Playing : GameState.MainMenu);
        }

        void RefreshMeta()
        {
            _homeCoins.text = ProgressSave.Coins.ToString();
            _gameCoins.text = ProgressSave.Coins.ToString();
            _levelsCoins.text = ProgressSave.Coins.ToString();
            _nextLevel.text = $"PLAY LEVEL {_level}";
        }

        void ShowLevels()
        {
            Show(_levels);
            foreach (Transform child in _levels)
            {
                if (child.name.StartsWith("LevelNode"))
                {
                    Destroy(child.gameObject);
                }
            }

            var world = World();
            for (var i = 0; i < world.levels.Length; i++)
            {
                var number = world.levels[i].number;
                var x = -280 + (i % 3) * 280;
                var y = 420 - (i / 3) * 180;
                var locked = number > _level;
                var label = locked ? "X" : _cleared.Contains(number) ? "OK" : number.ToString();
                var captured = number;
                Button(_levels, label, new Vector2(x, y), new Vector2(180, 140), () =>
                {
                    if (captured <= _level)
                    {
                        StartLevel(captured);
                    }
                }).name = $"LevelNode{number}";
            }
        }

        void StartLevel(int number)
        {
            _current = Definition(number);
            _level = number;
            _extraUsed = false;
            _session = new BlockSortSession(_current.CreateBoard());
            _session.SlotSelected += _ => RenderBoard();
            _session.SelectionCleared += RenderBoard;
            _session.MoveApplied += (_, _, _) => RenderBoard();
            _session.SlotCleared += _ => RenderBoard();
            _session.LevelWon += OnWin;
            _levelNumber.text = $"LEVEL {number}";
            Show(_game);
            RenderBoard();
        }

        void RenderBoard()
        {
            foreach (Transform child in _boardRoot)
            {
                Destroy(child.gameObject);
            }

            if (_session == null)
            {
                return;
            }

            _moves.text = $"{_session.MoveCount} MOVES";
            _undoCount.text = _session.UndoCharges.ToString();
            var count = _session.Board.Slots.Count;
            var columns = Mathf.Min(4, count);
            for (var i = 0; i < count; i++)
            {
                var column = i % columns;
                var row = i / columns;
                var x = (column - (columns - 1) * 0.5f) * 240;
                var y = 280 - row * 520;
                DrawSlot(i, new Vector2(x, y));
            }
        }

        void DrawSlot(int index, Vector2 position)
        {
            var slot = _session.Board.Slots[index];
            var selected = _session.SelectedSlot == index;
            var hit = Panel($"Slot{index}", _boardRoot, position, new Vector2(220, 500), true);
            var hitImage = hit.GetComponent<Image>();
            hitImage.color = new Color(1f, 1f, 1f, 0.01f);
            hitImage.alphaHitTestMinimumThreshold = 0f;
            var visual = Panel("Tube", hit, Vector2.zero, new Vector2(180, 460), false);
            visual.GetComponent<Image>().color = selected ? new Color(0.82f, 0.44f, 0.18f) : new Color(0.42f, 0.18f, 0.10f);
            visual.localScale = selected ? Vector3.one * 1.04f : Vector3.one;
            var lift = selected ? 22f : 0f;
            for (var c = 0; c < slot.Cubes.Count; c++)
            {
                var color = slot.Cubes[c];
                var block = Panel($"Cube{c}", visual, new Vector2(0, -170 + c * 95 + lift), new Vector2(150, 86), false);
                block.GetComponent<Image>().color = CubeColors[(int)color];
                var icon = Label(block, CubeIcons[(int)color], 40, Vector2.zero, new Vector2(150, 86));
                icon.color = new Color(1f, 0.96f, 0.85f);
            }

            var tap = hit.gameObject.AddComponent<TubeTapTarget>();
            tap.SlotIndex = index;
            tap.Session = _session;
            tap.Visual = visual;
        }

        void Undo()
        {
            if (_session != null && _session.Undo())
            {
                RenderBoard();
            }
        }

        void ExtraSlot()
        {
            if (_extraUsed || _session == null)
            {
                return;
            }

            _extraUsed = true;
            _session.TryAddExtraSlot();
            RenderBoard();
        }

        void OnWin()
        {
            if (!_cleared.Contains(_current.levelNumber))
            {
                _cleared.Add(_current.levelNumber);
            }

            ProgressSave.Coins += _current.reward;
            if (_current.levelNumber >= _level)
            {
                _level = _current.levelNumber + 1;
            }

            ProgressSave.CurrentLevel = _level;
            ProgressSave.ClearedLevels = _cleared.ToArray();
            ProgressSave.Flush();
            _completeLevel.text = $"LEVEL {_current.levelNumber}";
            _reward.text = $"+{_current.reward}";
            Show(_complete);
        }

        void NextLevel() => StartLevel(_level);

        RectTransform Screen(string name)
        {
            var panel = Panel(name, _canvas.transform, Vector2.zero, new Vector2(1080, 1920));
            Stretch(panel);
            panel.gameObject.SetActive(false);
            return panel;
        }

        static Image Image(string name, Transform parent, Color color, bool raycastTarget = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        static RectTransform Panel(string name, Transform parent, Vector2 anchored, Vector2 size, bool raycastTarget = false)
        {
            var image = Image(name, parent, Wood, raycastTarget);
            var rect = image.rectTransform;
            rect.sizeDelta = size;
            rect.anchoredPosition = anchored;
            return rect;
        }

        static Text Label(Transform parent, string value, int size, Vector2 anchored, Vector2? box = null)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.text = value;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 0.93f, 0.62f);
            text.fontSize = size;
            text.raycastTarget = false;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            var rect = text.rectTransform;
            rect.sizeDelta = box ?? new Vector2(900, 120);
            rect.anchoredPosition = anchored;
            return text;
        }

        static Button Button(Transform parent, string label, Vector2 anchored, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            var image = Image(label, parent, new Color(0.92f, 0.42f, 0.16f), true);
            var rect = image.rectTransform;
            rect.sizeDelta = size;
            rect.anchoredPosition = anchored;
            var button = image.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.onClick.AddListener(action);
            Label(image.transform, label, 36, Vector2.zero, size);
            return button;
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
