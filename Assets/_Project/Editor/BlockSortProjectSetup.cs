using System.IO;
using BlockSort.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace BlockSort.EditorTools
{
    public static class BlockSortProjectSetup
    {
        const string WorldJson = "Assets/_Project/Resources/Levels/world-01.json";
        const string WorldFolder = "Assets/_Project/Data/Levels/World01";
        const string CatalogPath = "Assets/_Project/Data/Levels/World01/World01Catalog.asset";
        const string DatabasePath = "Assets/_Project/Data/Levels/WorldDatabase.asset";
        const string BootScene = "Assets/_Project/Scenes/Boot.unity";
        const string MetaScene = "Assets/_Project/Scenes/Meta.unity";
        const string GameScene = "Assets/_Project/Scenes/Game.unity";
        const string PrefKey = "BlockSort.ProjectSetup.v3";

        [InitializeOnLoadMethod]
        static void AutoRun()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorPrefs.GetBool(PrefKey, false))
                {
                    Setup();
                }
            };
        }

        [MenuItem("Tools/Block Sort/Complete Project Setup")]
        public static void Setup()
        {
            Directory.CreateDirectory("Assets/_Project/Data/Levels/World01");
            Directory.CreateDirectory("Assets/_Project/Scenes");
            Directory.CreateDirectory("Assets/_Project/Settings");
            Directory.CreateDirectory("Assets/_Project/Prefabs");
            Directory.CreateDirectory("Assets/_Project/Art/UI");
            Directory.CreateDirectory("Assets/_Project/Audio/SFX");
            BakeWorldFromJson();
            EnsureScenes();
            ConfigurePlayer();
            TryAssignUrp();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorPrefs.SetBool(PrefKey, true);
            Debug.Log("Block Sort: project folders, World 01 levels, and Boot/Meta/Game scenes are ready. Press Play on Boot.");
        }

        [MenuItem("Tools/Block Sort/Bake 12 Hand Levels From JSON")]
        public static void BakeWorldFromJson()
        {
            var json = AssetDatabase.LoadAssetAtPath<TextAsset>(WorldJson);
            if (json == null)
            {
                Debug.LogWarning("Missing " + WorldJson);
                return;
            }

            var world = LevelJson.Parse(json);
            var catalog = LoadOrCreate<LevelCatalog>(CatalogPath);
            catalog.worldId = world.worldId;
            catalog.displayName = world.displayName;
            catalog.levels = new LevelDefinition[world.levels.Length];
            for (var i = 0; i < world.levels.Length; i++)
            {
                var path = $"{WorldFolder}/Level_{world.levels[i].number:00}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
                var baked = LevelJson.ToDefinition(world.levels[i], world.worldId);
                if (existing == null)
                {
                    AssetDatabase.CreateAsset(baked, path);
                    existing = baked;
                }
                else
                {
                    EditorUtility.CopySerialized(baked, existing);
                    Object.DestroyImmediate(baked);
                }

                catalog.levels[i] = existing;
            }

            EditorUtility.SetDirty(catalog);
            var database = LoadOrCreate<WorldDatabase>(DatabasePath);
            database.worlds = new[] { catalog };
            EditorUtility.SetDirty(database);
        }

        [MenuItem("Tools/Block Sort/Generate Extra Practice Levels (13-30)")]
        public static void GeneratePractice()
        {
            Directory.CreateDirectory("Assets/_Project/Data/Levels/World01/Generated");
            for (var n = 13; n <= 30; n++)
            {
                var definition = LevelGenerator.Build(n, 4530 + n);
                var path = $"Assets/_Project/Data/Levels/World01/Generated/Level_{n:00}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
                if (existing == null)
                {
                    AssetDatabase.CreateAsset(definition, path);
                }
                else
                {
                    EditorUtility.CopySerialized(definition, existing);
                    Object.DestroyImmediate(definition);
                }
            }

            AssetDatabase.SaveAssets();
        }

        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        static void EnsureScenes()
        {
            CreateSceneIfMissing(BootScene, "Boot");
            CreateSceneIfMissing(MetaScene, "Meta");
            CreateSceneIfMissing(GameScene, "Game");
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootScene, true),
                new EditorBuildSettingsScene(MetaScene, true),
                new EditorBuildSettingsScene(GameScene, true)
            };
        }

        static void CreateSceneIfMissing(string path, string name)
        {
            if (File.Exists(path))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = name;
            if (name == "Boot" && Object.FindFirstObjectByType<BlockSort.Presentation.BlockSortApp>() == null)
            {
                var go = new GameObject("BlockSortApp");
                go.AddComponent<BlockSort.Presentation.BlockSortApp>();
            }

            EditorSceneManager.SaveScene(scene, path);
        }

        static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "Block Sort Studio";
            PlayerSettings.productName = "Block Sort";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.blocksort.game");
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.blocksort.game");
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1);
        }

        static void TryAssignUrp()
        {
            var pipelineType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");
            var rendererType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalRendererData, Unity.RenderPipelines.Universal.Runtime");
            if (pipelineType == null || rendererType == null)
            {
                return;
            }

            var rendererPath = "Assets/_Project/Settings/URP-Renderer.asset";
            var pipelinePath = "Assets/_Project/Settings/URP-Pipeline.asset";
            var renderer = AssetDatabase.LoadAssetAtPath<ScriptableObject>(rendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance(rendererType);
                AssetDatabase.CreateAsset(renderer, rendererPath);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                var created = ScriptableObject.CreateInstance(pipelineType);
                AssetDatabase.CreateAsset(created, pipelinePath);
                pipeline = created as RenderPipelineAsset;
            }

            if (pipeline != null && renderer != null)
            {
                var serialized = new SerializedObject(pipeline);
                var rendererList = serialized.FindProperty("m_RendererDataList");
                rendererList.arraySize = 1;
                rendererList.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
                var defaultIndex = serialized.FindProperty("m_DefaultRendererIndex");
                if (defaultIndex != null)
                {
                    defaultIndex.intValue = 0;
                }

                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(pipeline);
                GraphicsSettings.defaultRenderPipeline = pipeline;
                QualitySettings.renderPipeline = pipeline;
            }
        }
    }
}
