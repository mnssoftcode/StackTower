using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using StackTower.Core;
using StackTower.Gameplay;
using StackTower.UI;
using StackTower.Audio;
using StackTower.Platform;

namespace StackTower.Editor
{
    /// <summary>
    /// Automated builder that sets up textures, materials, prefabs, scenes,
    /// build settings, and player settings for the Stack Tower MVP.
    /// </summary>
    [InitializeOnLoad]
    public static class StackTowerSceneBuilder
    {
        private const string RootPath = "Assets/_Project/StackTower";
        private const string ArtPath = RootPath + "/Art";
        private const string PrefabPath = RootPath + "/Prefabs";
        private const string ScenesPath = RootPath + "/Scenes";

        private const string MainMenuScenePath = ScenesPath + "/MainMenu.unity";
        private const string GameplayScenePath = ScenesPath + "/Gameplay.unity";

        static StackTowerSceneBuilder()
        {
            EditorApplication.delayCall += CheckAndAutoBuild;
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        [InitializeOnLoadMethod]
        public static void AutoBuildOnReload()
        {
            CheckAndAutoBuild();
        }

        private static void CheckAndAutoBuild()
        {
            if (!File.Exists(MainMenuScenePath) || !File.Exists(GameplayScenePath))
            {
                Debug.Log("[StackTower] Scenes missing on start. Automatically generating complete MVP scenes and prefabs...");
                BuildCompleteGame();
            }
        }

        [MenuItem("Stack Tower/Build Complete Game (Scenes, Prefabs, Build Settings)")]
        public static void BuildCompleteGame()
        {
            try
            {
                EnsureFolders();
                ConfigureSpriteImporters();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                GameObject blockPrefab = CreateBlockPrefab();
                GameObject offcutPrefab = CreateOffcutPrefab();

                BuildGameplayScene(blockPrefab, offcutPrefab);
                BuildMainMenuScene();

                ConfigureBuildSettings();
                ConfigurePlayerSettings();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("<color=green>[StackTower] Complete Game Build SUCCEEDED!</color> Scenes, Prefabs, and Settings are ready.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[StackTower] Build failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        [MenuItem("Stack Tower/Build macOS Universal App (.app)")]
        public static void BuildStandalonePlayer()
        {
            string buildPath = "Builds/macOS/StackTower.app";
            string buildDir = Path.GetDirectoryName(buildPath);
            if (!Directory.Exists(buildDir))
            {
                Directory.CreateDirectory(buildDir);
            }

            // Ensure Universal architecture: Intel 64-bit + Apple Silicon (M-series)
            PlayerSettings.SetArchitecture(UnityEditor.Build.NamedBuildTarget.Standalone, 0);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { MainMenuScenePath, GameplayScenePath },
                locationPathName = buildPath,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[StackTower] macOS Universal Build completed with status: {report.summary.result}, errors: {report.summary.totalErrors}");
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                EditorUtility.DisplayDialog("Stack Tower", "Universal macOS App built successfully!\n\nSupports both Intel and Apple Silicon (M1/M2/M3/M4) Macs.\n\nTo package into a .dmg, click 'Stack Tower > Package .app into DMG' or run './create_dmg.sh'.", "OK");
            }
        }

        [MenuItem("Stack Tower/Package .app into DMG")]
        public static void PackageDmgFromMenu()
        {
            string scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "create_dmg.sh");
            if (!File.Exists(scriptPath))
            {
                Debug.LogError("create_dmg.sh script not found!");
                return;
            }

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"\"{scriptPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var proc = System.Diagnostics.Process.Start(psi))
            {
                string stdout = proc.StandardOutput.ReadToEnd();
                string stderr = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                if (proc.ExitCode == 0)
                {
                    Debug.Log($"<color=green>[StackTower DMG]</color> DMG created successfully!\n{stdout}");
                    EditorUtility.DisplayDialog("Stack Tower DMG", "DMG package created successfully!\nLocated at: Builds/macOS/StackTower.dmg", "OK");
                }
                else
                {
                    Debug.LogError($"[StackTower DMG] Failed: {stderr}\n{stdout}");
                    EditorUtility.DisplayDialog("Stack Tower DMG Error", $"DMG creation failed:\n{stderr}", "OK");
                }
            }
        }

        private static void EnsureFolders()
        {
            string[] folders = { RootPath, ArtPath, PrefabPath, ScenesPath, RootPath + "/Materials", RootPath + "/Audio" };
            foreach (var folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
                    string leaf = Path.GetFileName(folder);
                    AssetDatabase.CreateFolder(parent, leaf);
                }
            }
        }

        private static void ConfigureSpriteImporters()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { ArtPath });
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;

                string filename = Path.GetFileName(path);
                if (filename == "pill_button.png")
                {
                    importer.spriteBorder = new Vector4(44, 44, 44, 44);
                }
                else if (filename == "card_bg.png")
                {
                    importer.spriteBorder = new Vector4(28, 28, 28, 28);
                }
                else if (filename == "block_bevel.png")
                {
                    importer.spriteBorder = new Vector4(10, 12, 10, 14);
                }
                else if (filename == "drop_guide.png")
                {
                    importer.spriteBorder = new Vector4(8, 8, 8, 8);
                }
                else if (filename == "sky_gradient.png")
                {
                    importer.wrapMode = TextureWrapMode.Clamp;
                }

                importer.SaveAndReimport();
            }
        }

        private static Sprite LoadSprite(string filename)
        {
            string path = $"{ArtPath}/{filename}";
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static TMP_FontAsset LoadDefaultFont()
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (font == null)
            {
                font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }
            return font;
        }

        private static GameObject CreateBlockPrefab()
        {
            string prefabFilePath = $"{PrefabPath}/Block.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabFilePath);
            if (existing != null) return existing;

            GameObject go = new GameObject("Block");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("block_bevel.png");
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.sortingOrder = 5;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            MovingBlock mb = go.AddComponent<MovingBlock>();
            SerializedObject mbSo = new SerializedObject(mb);
            mbSo.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            mbSo.ApplyModifiedProperties();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabFilePath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateOffcutPrefab()
        {
            string prefabFilePath = $"{PrefabPath}/Offcut.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabFilePath);
            if (existing != null) return existing;

            GameObject go = new GameObject("Offcut");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("block_bevel.png");
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.sortingOrder = 4;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3.5f;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            BlockPiece piece = go.AddComponent<BlockPiece>();
            SerializedObject pieceSo = new SerializedObject(piece);
            pieceSo.FindProperty("_spriteRenderer").objectReferenceValue = sr;
            pieceSo.FindProperty("_rigidbody2D").objectReferenceValue = rb;
            pieceSo.ApplyModifiedProperties();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabFilePath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void BuildGameplayScene(GameObject blockPrefab, GameObject offcutPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.04f, 0.13f); // #0d0a21
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<CameraController>();

            // 2. Background Scenery
            CreateBackgroundScenery(camGo.transform);

            // 3. Managers
            GameObject gmGo = new GameObject("GameManager");
            GameManager gm = gmGo.AddComponent<GameManager>();

            GameObject audioGo = new GameObject("AudioManager");
            audioGo.AddComponent<AudioManager>();

            GameObject inputGo = new GameObject("PrimaryInput");
            inputGo.AddComponent<PrimaryInput>();

            // 4. Tower Controller & Drop Indicator
            GameObject tcGo = new GameObject("TowerController");
            TowerController tc = tcGo.AddComponent<TowerController>();

            GameObject dropGuideGo = new GameObject("DropIndicator");
            dropGuideGo.transform.SetParent(tcGo.transform);
            SpriteRenderer guideSr = dropGuideGo.AddComponent<SpriteRenderer>();
            guideSr.sprite = LoadSprite("drop_guide.png");
            guideSr.drawMode = SpriteDrawMode.Sliced;
            guideSr.sortingOrder = 2;
            dropGuideGo.SetActive(false);

            // Serialized fields for TowerController
            SerializedObject tcSo = new SerializedObject(tc);
            tcSo.FindProperty("_blockPrefab").objectReferenceValue = blockPrefab;
            tcSo.FindProperty("_offcutPrefab").objectReferenceValue = offcutPrefab;
            tcSo.FindProperty("_dropIndicator").objectReferenceValue = guideSr;
            tcSo.ApplyModifiedProperties();

            // 5. Canvas & UI
            BuildGameplayCanvas(gmGo);

            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.04f, 0.13f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            camGo.AddComponent<AudioListener>();

            // Background
            CreateBackgroundScenery(camGo.transform);

            // Audio Manager
            GameObject audioGo = new GameObject("AudioManager");
            audioGo.AddComponent<AudioManager>();

            // UI Canvas
            BuildMainMenuCanvas();

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        private static void CreateBackgroundScenery(Transform camTransform)
        {
            GameObject bgRoot = new GameObject("BackgroundScenery");
            BackgroundController bgCtrl = bgRoot.AddComponent<BackgroundController>();

            // 1. Sky Gradient
            GameObject sky = new GameObject("SkyGradient");
            sky.transform.SetParent(bgRoot.transform);
            sky.transform.localPosition = new Vector3(0f, 0f, 0f);
            sky.transform.localScale = new Vector3(12f, 22f, 1f);
            SpriteRenderer skySr = sky.AddComponent<SpriteRenderer>();
            skySr.sprite = LoadSprite("sky_gradient.png");
            skySr.sortingOrder = -20;

            // 2. Stars
            GameObject starfield = new GameObject("Starfield");
            starfield.transform.SetParent(bgRoot.transform);
            starfield.transform.localPosition = Vector3.zero;
            Sprite starSprite = LoadSprite("star_sparkle.png");

            float[] starPositions = {
                -2.5f, 4.2f, 0.35f,
                -1.2f, 3.5f, 0.25f,
                0.8f, 4.5f, 0.30f,
                2.2f, 3.8f, 0.40f,
                -2.0f, 1.8f, 0.22f,
                1.5f, 2.2f, 0.28f,
                -0.5f, 2.9f, 0.32f,
                2.6f, 1.2f, 0.20f
            };

            for (int i = 0; i < starPositions.Length; i += 3)
            {
                GameObject s = new GameObject($"Star_{i / 3}");
                s.transform.SetParent(starfield.transform);
                s.transform.localPosition = new Vector3(starPositions[i], starPositions[i + 1], 0f);
                float sc = starPositions[i + 2];
                s.transform.localScale = new Vector3(sc, sc, 1f);
                SpriteRenderer ssr = s.AddComponent<SpriteRenderer>();
                ssr.sprite = starSprite;
                ssr.color = new Color(1f, 1f, 1f, 0.85f);
                ssr.sortingOrder = -15;
            }

            // 3. Mountains Back
            GameObject mtnBack = new GameObject("MountainBack");
            mtnBack.transform.SetParent(bgRoot.transform);
            mtnBack.transform.localPosition = new Vector3(0f, -0.8f, 0f);
            mtnBack.transform.localScale = new Vector3(9.5f, 4.8f, 1f);
            SpriteRenderer mbSr = mtnBack.AddComponent<SpriteRenderer>();
            mbSr.sprite = LoadSprite("mountain_silhouette_back.png");
            mbSr.sortingOrder = -12;

            // 4. Mountains Mid
            GameObject mtnMid = new GameObject("MountainMid");
            mtnMid.transform.SetParent(bgRoot.transform);
            mtnMid.transform.localPosition = new Vector3(0f, -1.8f, 0f);
            mtnMid.transform.localScale = new Vector3(9.5f, 4.2f, 1f);
            SpriteRenderer mmSr = mtnMid.AddComponent<SpriteRenderer>();
            mmSr.sprite = LoadSprite("mountain_silhouette_mid.png");
            mmSr.sortingOrder = -10;

            // 5. Trees Silhouette
            GameObject trees = new GameObject("TreeSilhouettes");
            trees.transform.SetParent(bgRoot.transform);
            trees.transform.localPosition = new Vector3(0f, -3.2f, 0f);
            trees.transform.localScale = new Vector3(9.5f, 2.5f, 1f);
            SpriteRenderer trSr = trees.AddComponent<SpriteRenderer>();
            trSr.sprite = LoadSprite("tree_silhouettes.png");
            trSr.sortingOrder = -8;
        }

        private static void BuildGameplayCanvas(GameObject gmGo)
        {
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            CanvasScaler cs = canvasGo.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080f, 1920f);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // Event System
            EnsureEventSystem();

            // Safe Area Container
            GameObject safeAreaGo = new GameObject("SafeAreaContainer");
            safeAreaGo.transform.SetParent(canvasGo.transform, false);
            RectTransform saRt = safeAreaGo.AddComponent<RectTransform>();
            saRt.anchorMin = Vector2.zero;
            saRt.anchorMax = Vector2.one;
            saRt.sizeDelta = Vector2.zero;
            safeAreaGo.AddComponent<SafeAreaFitter>();

            GameplayUI ui = canvasGo.AddComponent<GameplayUI>();
            TMP_FontAsset defaultFont = LoadDefaultFont();

            // 1. Current Score
            GameObject scoreGo = new GameObject("CurrentScoreText");
            scoreGo.transform.SetParent(safeAreaGo.transform, false);
            RectTransform scoreRt = scoreGo.AddComponent<RectTransform>();
            scoreRt.anchorMin = new Vector2(0.5f, 1f);
            scoreRt.anchorMax = new Vector2(0.5f, 1f);
            scoreRt.pivot = new Vector2(0.5f, 1f);
            scoreRt.anchoredPosition = new Vector2(0f, -130f);
            scoreRt.sizeDelta = new Vector2(500f, 160f);
            TextMeshProUGUI scoreTmp = scoreGo.AddComponent<TextMeshProUGUI>();
            scoreTmp.font = defaultFont;
            scoreTmp.text = "0";
            scoreTmp.fontSize = 140;
            scoreTmp.fontStyle = FontStyles.Bold;
            scoreTmp.alignment = TextAlignmentOptions.Center;
            scoreTmp.color = Color.white;

            // 2. Score Delta (+1 / +2)
            GameObject deltaGo = new GameObject("ScoreDeltaText");
            deltaGo.transform.SetParent(safeAreaGo.transform, false);
            RectTransform deltaRt = deltaGo.AddComponent<RectTransform>();
            deltaRt.anchorMin = new Vector2(0.5f, 1f);
            deltaRt.anchorMax = new Vector2(0.5f, 1f);
            deltaRt.pivot = new Vector2(0.5f, 1f);
            deltaRt.anchoredPosition = new Vector2(0f, -280f);
            deltaRt.sizeDelta = new Vector2(250f, 70f);
            TextMeshProUGUI deltaTmp = deltaGo.AddComponent<TextMeshProUGUI>();
            deltaTmp.font = defaultFont;
            deltaTmp.text = "+1";
            deltaTmp.fontSize = 52;
            deltaTmp.fontStyle = FontStyles.Bold;
            deltaTmp.alignment = TextAlignmentOptions.Center;
            deltaTmp.color = new Color(1f, 0.9f, 0.3f);
            deltaGo.SetActive(false);

            // 3. Perfect Banner Text
            GameObject perfectGo = new GameObject("PerfectFeedbackText");
            perfectGo.transform.SetParent(safeAreaGo.transform, false);
            RectTransform perfRt = perfectGo.AddComponent<RectTransform>();
            perfRt.anchorMin = new Vector2(0.5f, 1f);
            perfRt.anchorMax = new Vector2(0.5f, 1f);
            perfRt.pivot = new Vector2(0.5f, 1f);
            perfRt.anchoredPosition = new Vector2(0f, -360f);
            perfRt.sizeDelta = new Vector2(600f, 90f);
            TextMeshProUGUI perfTmp = perfectGo.AddComponent<TextMeshProUGUI>();
            perfTmp.font = defaultFont;
            perfTmp.text = "PERFECT!";
            perfTmp.fontSize = 72;
            perfTmp.fontStyle = FontStyles.Bold;
            perfTmp.alignment = TextAlignmentOptions.Center;
            perfTmp.color = new Color(0.2f, 1f, 0.65f);
            perfectGo.SetActive(false);

            // 4. Best Score Pill (Top Right)
            GameObject bestPillGo = new GameObject("BestScorePill");
            bestPillGo.transform.SetParent(safeAreaGo.transform, false);
            RectTransform bpRt = bestPillGo.AddComponent<RectTransform>();
            bpRt.anchorMin = new Vector2(1f, 1f);
            bpRt.anchorMax = new Vector2(1f, 1f);
            bpRt.pivot = new Vector2(1f, 1f);
            bpRt.anchoredPosition = new Vector2(-40f, -50f);
            bpRt.sizeDelta = new Vector2(320f, 82f);
            Image bpImg = bestPillGo.AddComponent<Image>();
            bpImg.sprite = LoadSprite("card_bg.png");
            bpImg.type = Image.Type.Sliced;
            bpImg.color = new Color(0.2f, 0.15f, 0.35f, 0.85f);

            // Crown icon (larger, prominent)
            GameObject crownGo = new GameObject("CrownIcon");
            crownGo.transform.SetParent(bestPillGo.transform, false);
            RectTransform crRt = crownGo.AddComponent<RectTransform>();
            crRt.anchorMin = new Vector2(0f, 0.5f);
            crRt.anchorMax = new Vector2(0f, 0.5f);
            crRt.pivot = new Vector2(0f, 0.5f);
            crRt.anchoredPosition = new Vector2(24f, 0f);
            crRt.sizeDelta = new Vector2(56f, 56f);
            Image crImg = crownGo.AddComponent<Image>();
            crImg.sprite = LoadSprite("crown_icon.png");

            // Best Score Text
            GameObject bpTextGo = new GameObject("BestScoreText");
            bpTextGo.transform.SetParent(bestPillGo.transform, false);
            RectTransform bptRt = bpTextGo.AddComponent<RectTransform>();
            bptRt.anchorMin = Vector2.zero;
            bptRt.anchorMax = Vector2.one;
            bptRt.offsetMin = new Vector2(90f, 0f);
            bptRt.offsetMax = new Vector2(-16f, 0f);
            TextMeshProUGUI bptTmp = bpTextGo.AddComponent<TextMeshProUGUI>();
            bptTmp.font = defaultFont;
            bptTmp.text = "Best: 0";
            bptTmp.fontSize = 36;
            bptTmp.fontStyle = FontStyles.Bold;
            bptTmp.alignment = TextAlignmentOptions.MidlineLeft;
            bptTmp.color = Color.white;

            // 5. Tap Hint
            GameObject hintGo = new GameObject("TapHintText");
            hintGo.transform.SetParent(safeAreaGo.transform, false);
            RectTransform hintRt = hintGo.AddComponent<RectTransform>();
            hintRt.anchorMin = new Vector2(0.5f, 0.5f);
            hintRt.anchorMax = new Vector2(0.5f, 0.5f);
            hintRt.pivot = new Vector2(0.5f, 0.5f);
            hintRt.anchoredPosition = new Vector2(0f, 150f);
            hintRt.sizeDelta = new Vector2(500f, 60f);
            TextMeshProUGUI hintTmp = hintGo.AddComponent<TextMeshProUGUI>();
            hintTmp.font = defaultFont;
            hintTmp.text = "TAP TO DROP";
            hintTmp.fontSize = 38;
            hintTmp.fontStyle = FontStyles.Bold;
            hintTmp.alignment = TextAlignmentOptions.Center;
            hintTmp.color = new Color(0.75f, 0.72f, 0.90f, 0.85f);
            hintTmp.alignment = TextAlignmentOptions.Center;
            hintTmp.color = new Color(0.75f, 0.72f, 0.90f, 0.85f);

            // 6. Game Over Modal Overlay
            GameObject gameOverGo = new GameObject("GameOverOverlay");
            gameOverGo.transform.SetParent(canvasGo.transform, false);
            RectTransform goRt = gameOverGo.AddComponent<RectTransform>();
            goRt.anchorMin = Vector2.zero;
            goRt.anchorMax = Vector2.one;
            goRt.sizeDelta = Vector2.zero;
            CanvasGroup goCg = gameOverGo.AddComponent<CanvasGroup>();

            // Dimmed background
            Image dimImg = gameOverGo.AddComponent<Image>();
            dimImg.color = new Color(0.06f, 0.04f, 0.14f, 0.75f);

            // Dialog Card
            GameObject cardGo = new GameObject("GameOverCard");
            cardGo.transform.SetParent(gameOverGo.transform, false);
            RectTransform cardRt = cardGo.AddComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.anchoredPosition = new Vector2(0f, 40f);
            cardRt.sizeDelta = new Vector2(740f, 880f);
            Image cardImg = cardGo.AddComponent<Image>();
            cardImg.sprite = LoadSprite("card_bg.png");
            cardImg.type = Image.Type.Sliced;
            cardImg.color = new Color(0.14f, 0.10f, 0.28f, 0.92f);

            // Title: GAME OVER
            GameObject titleGo = new GameObject("GameOverTitle");
            titleGo.transform.SetParent(cardGo.transform, false);
            RectTransform titRt = titleGo.AddComponent<RectTransform>();
            titRt.anchorMin = new Vector2(0.5f, 1f);
            titRt.anchorMax = new Vector2(0.5f, 1f);
            titRt.pivot = new Vector2(0.5f, 1f);
            titRt.anchoredPosition = new Vector2(0f, -60f);
            titRt.sizeDelta = new Vector2(600f, 80f);
            TextMeshProUGUI titTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titTmp.font = defaultFont;
            titTmp.text = "Game Over";
            titTmp.fontSize = 72;
            titTmp.fontStyle = FontStyles.Bold;
            titTmp.alignment = TextAlignmentOptions.Center;
            titTmp.color = Color.white;

            // Best Score Badge in Card
            GameObject goBestPill = new GameObject("GameOverBestBadge");
            goBestPill.transform.SetParent(cardGo.transform, false);
            RectTransform goBpRt = goBestPill.AddComponent<RectTransform>();
            goBpRt.anchorMin = new Vector2(0.5f, 1f);
            goBpRt.anchorMax = new Vector2(0.5f, 1f);
            goBpRt.pivot = new Vector2(0.5f, 1f);
            goBpRt.anchoredPosition = new Vector2(0f, -160f);
            goBpRt.sizeDelta = new Vector2(340f, 84f);
            Image goBpImg = goBestPill.AddComponent<Image>();
            goBpImg.sprite = LoadSprite("pill_button.png");
            goBpImg.type = Image.Type.Sliced;
            goBpImg.color = new Color(0.25f, 0.18f, 0.45f, 0.85f);

            GameObject goCrown = new GameObject("Crown");
            goCrown.transform.SetParent(goBestPill.transform, false);
            RectTransform gcrRt = goCrown.AddComponent<RectTransform>();
            gcrRt.anchorMin = new Vector2(0f, 0.5f);
            gcrRt.anchorMax = new Vector2(0f, 0.5f);
            gcrRt.pivot = new Vector2(0f, 0.5f);
            gcrRt.anchoredPosition = new Vector2(24f, 0f);
            gcrRt.sizeDelta = new Vector2(56f, 56f);
            Image gcrImg = goCrown.AddComponent<Image>();
            gcrImg.sprite = LoadSprite("crown_icon.png");

            GameObject goBestTextGo = new GameObject("Text");
            goBestTextGo.transform.SetParent(goBestPill.transform, false);
            RectTransform gbtRt = goBestTextGo.AddComponent<RectTransform>();
            gbtRt.anchorMin = Vector2.zero;
            gbtRt.anchorMax = Vector2.one;
            gbtRt.offsetMin = new Vector2(90f, 0f);
            gbtRt.offsetMax = new Vector2(-15f, 0f);
            TextMeshProUGUI goBestTmp = goBestTextGo.AddComponent<TextMeshProUGUI>();
            goBestTmp.font = defaultFont;
            goBestTmp.text = "Best: 0";
            goBestTmp.fontSize = 36;
            goBestTmp.fontStyle = FontStyles.Bold;
            goBestTmp.alignment = TextAlignmentOptions.MidlineLeft;
            goBestTmp.color = Color.white;

            // New Best Badge
            GameObject newBestGo = new GameObject("NewBestBadge");
            newBestGo.transform.SetParent(cardGo.transform, false);
            RectTransform nbRt = newBestGo.AddComponent<RectTransform>();
            nbRt.anchorMin = new Vector2(0.5f, 1f);
            nbRt.anchorMax = new Vector2(0.5f, 1f);
            nbRt.pivot = new Vector2(0.5f, 1f);
            nbRt.anchoredPosition = new Vector2(0f, -235f);
            nbRt.sizeDelta = new Vector2(260f, 40f);
            TextMeshProUGUI nbTmp = newBestGo.AddComponent<TextMeshProUGUI>();
            nbTmp.font = defaultFont;
            nbTmp.text = "★ NEW BEST! ★";
            nbTmp.fontSize = 28;
            nbTmp.fontStyle = FontStyles.Bold;
            nbTmp.alignment = TextAlignmentOptions.Center;
            nbTmp.color = new Color(1f, 0.85f, 0.2f);
            newBestGo.SetActive(false);

            // Score Inner Card
            GameObject innerScoreBox = new GameObject("ScoreCardBox");
            innerScoreBox.transform.SetParent(cardGo.transform, false);
            RectTransform isbRt = innerScoreBox.AddComponent<RectTransform>();
            isbRt.anchorMin = new Vector2(0.5f, 0.5f);
            isbRt.anchorMax = new Vector2(0.5f, 0.5f);
            isbRt.pivot = new Vector2(0.5f, 0.5f);
            isbRt.anchoredPosition = new Vector2(0f, 40f);
            isbRt.sizeDelta = new Vector2(560f, 220f);
            Image isbImg = innerScoreBox.AddComponent<Image>();
            isbImg.sprite = LoadSprite("card_bg.png");
            isbImg.type = Image.Type.Sliced;
            isbImg.color = new Color(0.10f, 0.08f, 0.22f, 0.95f);

            GameObject yourScoreLbl = new GameObject("YourScoreLabel");
            yourScoreLbl.transform.SetParent(innerScoreBox.transform, false);
            RectTransform yslRt = yourScoreLbl.AddComponent<RectTransform>();
            yslRt.anchorMin = new Vector2(0.5f, 1f);
            yslRt.anchorMax = new Vector2(0.5f, 1f);
            yslRt.pivot = new Vector2(0.5f, 1f);
            yslRt.anchoredPosition = new Vector2(0f, -24f);
            yslRt.sizeDelta = new Vector2(400f, 40f);
            TextMeshProUGUI yslTmp = yourScoreLbl.AddComponent<TextMeshProUGUI>();
            yslTmp.font = defaultFont;
            yslTmp.text = "YOUR SCORE";
            yslTmp.fontSize = 28;
            yslTmp.fontStyle = FontStyles.Bold;
            yslTmp.alignment = TextAlignmentOptions.Center;
            yslTmp.color = new Color(0.65f, 0.60f, 0.82f);

            GameObject finalScoreGo = new GameObject("FinalScoreText");
            finalScoreGo.transform.SetParent(innerScoreBox.transform, false);
            RectTransform fsRt = finalScoreGo.AddComponent<RectTransform>();
            fsRt.anchorMin = new Vector2(0.5f, 0f);
            fsRt.anchorMax = new Vector2(0.5f, 0f);
            fsRt.pivot = new Vector2(0.5f, 0f);
            fsRt.anchoredPosition = new Vector2(0f, 25f);
            fsRt.sizeDelta = new Vector2(400f, 120f);
            TextMeshProUGUI fsTmp = finalScoreGo.AddComponent<TextMeshProUGUI>();
            fsTmp.font = defaultFont;
            fsTmp.text = "0";
            fsTmp.fontSize = 110;
            fsTmp.fontStyle = FontStyles.Bold;
            fsTmp.alignment = TextAlignmentOptions.Center;
            fsTmp.color = Color.white;

            // Restart Button
            GameObject restartBtnGo = new GameObject("RestartButton");
            restartBtnGo.transform.SetParent(cardGo.transform, false);
            RectTransform rstRt = restartBtnGo.AddComponent<RectTransform>();
            rstRt.anchorMin = new Vector2(0.5f, 0f);
            rstRt.anchorMax = new Vector2(0.5f, 0f);
            rstRt.pivot = new Vector2(0.5f, 0f);
            rstRt.anchoredPosition = new Vector2(0f, 160f);
            rstRt.sizeDelta = new Vector2(560f, 100f);
            Image rstImg = restartBtnGo.AddComponent<Image>();
            rstImg.sprite = LoadSprite("pill_button.png");
            rstImg.type = Image.Type.Sliced;
            Button rstBtn = restartBtnGo.AddComponent<Button>();

            GameObject rstTxtGo = new GameObject("Text");
            rstTxtGo.transform.SetParent(restartBtnGo.transform, false);
            RectTransform rsttRt = rstTxtGo.AddComponent<RectTransform>();
            rsttRt.anchorMin = Vector2.zero;
            rsttRt.anchorMax = Vector2.one;
            rsttRt.sizeDelta = Vector2.zero;
            TextMeshProUGUI rsttTmp = rstTxtGo.AddComponent<TextMeshProUGUI>();
            rsttTmp.font = defaultFont;
            rsttTmp.text = "Restart ⟳";
            rsttTmp.fontSize = 46;
            rsttTmp.fontStyle = FontStyles.Bold;
            rsttTmp.alignment = TextAlignmentOptions.Center;
            rsttTmp.color = Color.white;

            // Main Menu Button
            GameObject menuBtnGo = new GameObject("MainMenuButton");
            menuBtnGo.transform.SetParent(cardGo.transform, false);
            RectTransform mmRt = menuBtnGo.AddComponent<RectTransform>();
            mmRt.anchorMin = new Vector2(0.5f, 0f);
            mmRt.anchorMax = new Vector2(0.5f, 0f);
            mmRt.pivot = new Vector2(0.5f, 0f);
            mmRt.anchoredPosition = new Vector2(0f, 45f);
            mmRt.sizeDelta = new Vector2(560f, 90f);
            Image mmImg = menuBtnGo.AddComponent<Image>();
            mmImg.sprite = LoadSprite("card_bg.png");
            mmImg.type = Image.Type.Sliced;
            mmImg.color = new Color(0.22f, 0.16f, 0.40f, 0.90f);
            Button mmBtn = menuBtnGo.AddComponent<Button>();

            GameObject mmTxtGo = new GameObject("Text");
            mmTxtGo.transform.SetParent(menuBtnGo.transform, false);
            RectTransform mmtRt = mmTxtGo.AddComponent<RectTransform>();
            mmtRt.anchorMin = Vector2.zero;
            mmtRt.anchorMax = Vector2.one;
            mmtRt.sizeDelta = Vector2.zero;
            TextMeshProUGUI mmtTmp = mmTxtGo.AddComponent<TextMeshProUGUI>();
            mmtTmp.font = defaultFont;
            mmtTmp.text = "Main Menu";
            mmtTmp.fontSize = 38;
            mmtTmp.fontStyle = FontStyles.Bold;
            mmtTmp.alignment = TextAlignmentOptions.Center;
            mmtTmp.color = new Color(0.9f, 0.88f, 0.98f);

            gameOverGo.SetActive(false);

            // Wire serialized fields to GameplayUI
            SerializedObject uiSo = new SerializedObject(ui);
            uiSo.FindProperty("_currentScoreText").objectReferenceValue = scoreTmp;
            uiSo.FindProperty("_hudBestScoreText").objectReferenceValue = bptTmp;
            uiSo.FindProperty("_tapHintText").objectReferenceValue = hintTmp;
            uiSo.FindProperty("_perfectFeedbackText").objectReferenceValue = perfTmp;
            uiSo.FindProperty("_scoreDeltaText").objectReferenceValue = deltaTmp;

            uiSo.FindProperty("_gameOverPanel").objectReferenceValue = gameOverGo;
            uiSo.FindProperty("_gameOverCanvasGroup").objectReferenceValue = goCg;
            uiSo.FindProperty("_finalScoreText").objectReferenceValue = fsTmp;
            uiSo.FindProperty("_gameOverBestScoreText").objectReferenceValue = goBestTmp;
            uiSo.FindProperty("_newBestBadge").objectReferenceValue = newBestGo;
            uiSo.FindProperty("_restartButton").objectReferenceValue = rstBtn;
            uiSo.FindProperty("_mainMenuButton").objectReferenceValue = mmBtn;
            uiSo.ApplyModifiedProperties();
        }

        private static void BuildMainMenuCanvas()
        {
            GameObject canvasGo = new GameObject("Canvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            CanvasScaler cs = canvasGo.GetComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1080f, 1920f);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            EnsureEventSystem();

            MainMenuUI mmUi = canvasGo.AddComponent<MainMenuUI>();
            TMP_FontAsset defaultFont = LoadDefaultFont();

            // Safe Area Container
            GameObject safeAreaGo = new GameObject("SafeAreaContainer");
            safeAreaGo.transform.SetParent(canvasGo.transform, false);
            RectTransform saRt = safeAreaGo.AddComponent<RectTransform>();
            saRt.anchorMin = Vector2.zero;
            saRt.anchorMax = Vector2.one;
            saRt.sizeDelta = Vector2.zero;
            safeAreaGo.AddComponent<SafeAreaFitter>();

            // 1. Isometric Logo Stack
            GameObject logoGo = new GameObject("LogoStack");
            logoGo.transform.SetParent(safeAreaGo.transform, false);
            RectTransform logoRt = logoGo.AddComponent<RectTransform>();
            logoRt.anchorMin = new Vector2(0.5f, 0.5f);
            logoRt.anchorMax = new Vector2(0.5f, 0.5f);
            logoRt.pivot = new Vector2(0.5f, 0.5f);
            logoRt.anchoredPosition = new Vector2(0f, 380f);
            logoRt.sizeDelta = new Vector2(160f, 160f);
            Image logoImg = logoGo.AddComponent<Image>();
            logoImg.sprite = LoadSprite("app_logo_stack.png");

            // 2. Title: STACK TOWER
            GameObject titleGo = new GameObject("TitleText");
            titleGo.transform.SetParent(safeAreaGo.transform, false);
            RectTransform titRt = titleGo.AddComponent<RectTransform>();
            titRt.anchorMin = new Vector2(0.5f, 0.5f);
            titRt.anchorMax = new Vector2(0.5f, 0.5f);
            titRt.pivot = new Vector2(0.5f, 0.5f);
            titRt.anchoredPosition = new Vector2(0f, 210f);
            titRt.sizeDelta = new Vector2(800f, 180f);
            TextMeshProUGUI titTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titTmp.font = defaultFont;
            titTmp.text = "STACK\nTOWER";
            titTmp.fontSize = 92;
            titTmp.fontStyle = FontStyles.Bold;
            titTmp.lineSpacing = -18f;
            titTmp.alignment = TextAlignmentOptions.Center;
            titTmp.color = Color.white;

            // 3. Instruction
            GameObject instGo = new GameObject("InstructionText");
            instGo.transform.SetParent(safeAreaGo.transform, false);
            RectTransform insRt = instGo.AddComponent<RectTransform>();
            insRt.anchorMin = new Vector2(0.5f, 0.5f);
            insRt.anchorMax = new Vector2(0.5f, 0.5f);
            insRt.pivot = new Vector2(0.5f, 0.5f);
            insRt.anchoredPosition = new Vector2(0f, 90f);
            insRt.sizeDelta = new Vector2(700f, 60f);
            TextMeshProUGUI insTmp = instGo.AddComponent<TextMeshProUGUI>();
            insTmp.font = defaultFont;
            insTmp.text = "Tap to stack. Build as high as you can.";
            insTmp.fontSize = 32;
            insTmp.alignment = TextAlignmentOptions.Center;
            insTmp.color = new Color(0.72f, 0.68f, 0.88f);

            // 4. Best Score Pill (larger, prominent crown)
            GameObject bestPillGo = new GameObject("BestScorePill");
            bestPillGo.transform.SetParent(safeAreaGo.transform, false);
            RectTransform bpRt = bestPillGo.AddComponent<RectTransform>();
            bpRt.anchorMin = new Vector2(0.5f, 0.5f);
            bpRt.anchorMax = new Vector2(0.5f, 0.5f);
            bpRt.pivot = new Vector2(0.5f, 0.5f);
            bpRt.anchoredPosition = new Vector2(0f, -30f);
            bpRt.sizeDelta = new Vector2(380f, 92f);
            Image bpImg = bestPillGo.AddComponent<Image>();
            bpImg.sprite = LoadSprite("card_bg.png");
            bpImg.type = Image.Type.Sliced;
            bpImg.color = new Color(0.22f, 0.16f, 0.40f, 0.85f);

            GameObject crownGo = new GameObject("Crown");
            crownGo.transform.SetParent(bestPillGo.transform, false);
            RectTransform crRt = crownGo.AddComponent<RectTransform>();
            crRt.anchorMin = new Vector2(0f, 0.5f);
            crRt.anchorMax = new Vector2(0f, 0.5f);
            crRt.pivot = new Vector2(0f, 0.5f);
            crRt.anchoredPosition = new Vector2(28f, 0f);
            crRt.sizeDelta = new Vector2(62f, 62f);
            Image crImg = crownGo.AddComponent<Image>();
            crImg.sprite = LoadSprite("crown_icon.png");

            GameObject bestTextGo = new GameObject("BestText");
            bestTextGo.transform.SetParent(bestPillGo.transform, false);
            RectTransform btRt = bestTextGo.AddComponent<RectTransform>();
            btRt.anchorMin = Vector2.zero;
            btRt.anchorMax = Vector2.one;
            btRt.offsetMin = new Vector2(104f, 0f);
            btRt.offsetMax = new Vector2(-20f, 0f);
            TextMeshProUGUI btTmp = bestTextGo.AddComponent<TextMeshProUGUI>();
            btTmp.font = defaultFont;
            btTmp.text = "Best: 0";
            btTmp.fontSize = 40;
            btTmp.fontStyle = FontStyles.Bold;
            btTmp.alignment = TextAlignmentOptions.MidlineLeft;
            btTmp.color = Color.white;

            // 5. Play Button
            GameObject playBtnGo = new GameObject("PlayButton");
            playBtnGo.transform.SetParent(safeAreaGo.transform, false);
            RectTransform playRt = playBtnGo.AddComponent<RectTransform>();
            playRt.anchorMin = new Vector2(0.5f, 0.5f);
            playRt.anchorMax = new Vector2(0.5f, 0.5f);
            playRt.pivot = new Vector2(0.5f, 0.5f);
            playRt.anchoredPosition = new Vector2(0f, -220f);
            playRt.sizeDelta = new Vector2(440f, 116f);
            Image playImg = playBtnGo.AddComponent<Image>();
            playImg.sprite = LoadSprite("pill_button.png");
            playImg.type = Image.Type.Sliced;
            Button playBtn = playBtnGo.AddComponent<Button>();

            GameObject playTxtGo = new GameObject("Text");
            playTxtGo.transform.SetParent(playBtnGo.transform, false);
            RectTransform ptxtRt = playTxtGo.AddComponent<RectTransform>();
            ptxtRt.anchorMin = Vector2.zero;
            ptxtRt.anchorMax = Vector2.one;
            ptxtRt.sizeDelta = Vector2.zero;
            TextMeshProUGUI ptxtTmp = playTxtGo.AddComponent<TextMeshProUGUI>();
            ptxtTmp.font = defaultFont;
            ptxtTmp.text = "Play ▶";
            ptxtTmp.fontSize = 54;
            ptxtTmp.fontStyle = FontStyles.Bold;
            ptxtTmp.alignment = TextAlignmentOptions.Center;
            ptxtTmp.color = Color.white;
            ptxtTmp.color = Color.white;

            // Wire serialized fields to MainMenuUI
            SerializedObject mmSo = new SerializedObject(mmUi);
            mmSo.FindProperty("_playButton").objectReferenceValue = playBtn;
            mmSo.FindProperty("_bestScoreText").objectReferenceValue = btTmp;
            mmSo.FindProperty("_titleText").objectReferenceValue = titTmp;
            mmSo.FindProperty("_instructionText").objectReferenceValue = insTmp;
            mmSo.ApplyModifiedProperties();
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(GameplayScenePath, true)
            };
            EditorBuildSettings.scenes = scenes;
        }

        [MenuItem("Stack Tower/Apply App Icon to Project")]
        public static void ApplyAppIcon()
        {
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ArtPath}/app_icon.png");
            if (icon != null)
            {
                PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Unknown, new[] { icon }, IconKind.Application);
                PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Standalone, new[] { icon }, IconKind.Application);
                PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Android, new[] { icon }, IconKind.Application);
                PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.iOS, new[] { icon }, IconKind.Application);
                AssetDatabase.SaveAssets();
                Debug.Log("<color=green>[StackTower]</color> App icon successfully applied across all platforms (Default, Standalone macOS, Android, iOS)!");
                EditorUtility.DisplayDialog("Stack Tower", "App Icon applied successfully to all platforms (Default, macOS, Android, iOS)!", "OK");
            }
            else
            {
                Debug.LogError("Could not find app_icon.png in Art folder!");
            }
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.productName = "Stack Tower";
            PlayerSettings.companyName = "Antigravity Games";

            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ArtPath}/app_icon.png");
            if (icon != null)
            {
                PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Unknown, new[] { icon }, IconKind.Application);
                PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Standalone, new[] { icon }, IconKind.Application);
                PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Android, new[] { icon }, IconKind.Application);
                PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.iOS, new[] { icon }, IconKind.Application);
            }
        }
    }
}
