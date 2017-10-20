/* Radical Forge Copyright (c) 2017 All Rights Reserved
   </copyright>
   <author>Frederic Babord</author>
   <date>15th June 2017</date>
   <summary>The main blockout editor window logic and visual apperance</summary>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;


namespace RadicalForge.Blockout
{
    public enum BlockoutAxis
    {
        X = 0,
        Y,
        Z
    }

    public class BlockoutEditorWindow : EditorWindow
    {
        public static BlockoutEditorWindow Instance { get; private set; }

        // Add a blockout window option to the window dropdown
        [MenuItem("Window/Blockout/Editor %&b", false, 10)]
        public static void Init()
        {
            AssetDatabase.Refresh();
            if (!isVisible)
            {
                // Get existing open window or if none, make a new one:
                var window = (BlockoutEditorWindow) GetWindow(typeof(BlockoutEditorWindow));
                window.maxSize = new Vector2(4000, 4000);
                window.minSize = new Vector2(405, 100);
                window.Show();
            }
        }

        /// <summary>
        /// Show Blockout helper window
        /// </summary>
        [MenuItem("Window/Blockout/Block Helper", false, 11)]
        public static void ToggleSuggestions()
        {
            DisplaySuggestedAssets = !DisplaySuggestedAssets;
            if (DisplaySuggestedAssets)
            {
                assetHelper = BlockoutBlockHelper.Init();
            }
            else
            {
                BlockoutBlockHelper.Hide();
                assetHelper = null;
            }
        }

        /// <summary>
        /// Open URL to show online docs
        /// </summary>
        [MenuItem("Window/Blockout/Documentation", false, 55)]
        public static void OpenDocumentation()
        {
            Application.OpenURL("http://blockout.radicalforge.com/");
        }

        /// <summary>
        /// Open URL to show online docs
        /// </summary>
        [MenuItem("Window/Blockout/Tutorials", false, 56)]
        public static void OpennTutorials()
        {
            Application.OpenURL("https://www.youtube.com/playlist?list=PLpMc62-aOz2wt7DG-R6Dm87V1pyj9SCFH");
        }

        /// <summary>
        /// Open URL to submit feedback lke html mailto
        /// </summary>
        [MenuItem("Window/Blockout/Submit Feedback or Bug Report", false, 57)]
        public static void SubmitBugReport()
        {
            Application.OpenURL("mailto:support@radicalforge.com?subject=Blockout");
        }

        /// <summary>
        /// Init window properties
        /// </summary>
        public void OnEnable()
        {
            Instance = this;
            isVisible = true;
            AssetDatabase.Refresh();
            icon = Resources.Load(
                                  EditorGUIUtility.isProSkin
                                      ? "Blockout/UI_Icons/Blockout_Icon_Light"
                                      : "Blockout/UI_Icons/Blockout_Icon_Dark",
                                  typeof(Texture2D)) as Texture2D;
            titleContent = new GUIContent("Blockout", icon);
            LoadTextureResources(this);
            LoadThemes(this);
            LoadGUIContentResources();
            logoSkin = (GUISkin) Resources.Load(
                                                EditorGUIUtility.isProSkin
                                                    ? "Blockout/UI_Icons/BlockoutEditorSkinLight"
                                                    : "Blockout/UI_Icons/BlockoutEditorSkin", typeof(GUISkin));

             
            TryLoadSceneDefinitions();

            BlockoutEditorHelper.Awake();

            if (DisplaySuggestedAssets)
                assetHelper = BlockoutBlockHelper.Init();
        }

        /// <summary>
        ///     Called when the window is destroyed for cleanup
        /// </summary>
        private void OnDisable()
        {
            Instance = null;
            if (BlockoutBlockHelper.visible)
                assetHelper.Close();
            isVisible = false;
            BlockoutEditorHelper.Destroy();
        }

        /// <summary>
        /// Load all of the scene definitions internally
        /// </summary>
        private void TryLoadSceneDefinitions()
        {
            currentSetting = -1;
            sceneSettings = Resources.LoadAll<BlockoutSceneSettings>("").ToList();
            for (var i = 0; i < sceneSettings.Count; ++i)
                if (SceneManager.GetActiveScene().name == sceneSettings[i].sceneName)
                {
                    var target = i;
                    LoadSceneDefinition(target);
                }
            if (currentSetting < 0)
            {
                var newSetting = CreateInstance<BlockoutSceneSettings>();
                newSetting.sceneName = SceneManager.GetActiveScene().name;
                newSetting.assetDictionary = new List<AssetDefinition>();
                newSetting.cameraAnchor = new List<CameraAnchor>();

                Directory.CreateDirectory("Assets/Blockout/Editor/Resources/Blockout/SceneDefinitions");
                AssetDatabase.CreateAsset(newSetting,
                                          "Assets/Blockout/Editor/Resources/Blockout/SceneDefinitions/" +
                                          typeof(BlockoutSceneSettings).Name + "_" +
                                          newSetting.sceneName + ".asset");
                sceneSettings.Add(newSetting);
                currentSetting = sceneSettings.FindIndex(x => x.sceneName == newSetting.name);
                LoadSceneDefinition(currentSetting);
            }

            root = null;
            walls = null;
            floor = null;
            dynamic = null;
            foliage = null;
            trim = null;
            particles = null;
            triggers = null;
            cameras = null;
            comments = null;
        }

        /// <summary>
        /// Load a current scene definition thats already been globally loaded
        /// </summary>
        /// <param name="setting">The scene setting ID</param>
        private void LoadSceneDefinition(int setting)
        {
            currentSetting = setting;
            if (currentSetting > sceneSettings.Count)
            {
                currentSetting = -1;
                Debug.LogError(
                               "BLOCKOUT :: (INTERNAL) Unable to load scene definition. Please Close and reopen any Blockout window");
                return;
            }
            if (currentSetting < 0)
                return;
            currentMaterialTheme = sceneSettings[setting].currentTheme;
            if (currentMaterialTheme < 0)
                currentPallet = PalletType.User;
            else
                currentPallet = PalletType.Preset;

            currentGirdTexture = CurrentSceneSetting.currentTexture;
        }
        
        public void Update()
        {
            // Scene check
            currentActiveScene = SceneManager.GetActiveScene();
            if (currentActiveScene != previousActiveScene)
            {
                TryLoadSceneDefinitions();
                previousActiveScene = currentActiveScene;
            }


            // Apply current material theme to newly spawned blockout assets
            if (Selection.gameObjects.Length > 0 && root)
            {
                var selected = Selection.gameObjects.ToList();
                foreach (var x in selected)
                    if (!AssetDatabase.Contains(x.gameObject))
                    {
                        var helper = x.GetComponent<BlockoutHelper>();
                        if (helper)
                        {
                            var idx = Undo.GetCurrentGroup();
                            if (helper.ReapplyMaterialTheme)
                            {
                                if (CurrentSceneSetting.assetDictionary
                                                       .Find(y => y.assetName == helper.gameObject.name) != null)
                                {
                                    CurrentSceneSetting.assetDictionary
                                                       .Find(y => y.assetName == helper.gameObject.name)
                                                       .assetQuantity++;
                                }
                                else
                                {
                                    var def = new AssetDefinition();
                                    def.assetName = helper.gameObject.name;
                                    if (def.assetName.Contains("("))
                                    {
                                        var startIDX = def.assetName.IndexOf("(", StringComparison.Ordinal);
                                        def.assetName = def.assetName.Substring(0, startIDX);
                                    }
                                    def.assetQuantity = 1;
                                    CurrentSceneSetting.assetDictionary.Add(def);
                                    EditorUtility.SetDirty(CurrentSceneSetting);
                                    AssetDatabase.SaveAssets();
                                }

                                ApplyCurrentTheme();
                                helper.ReapplyMaterialTheme = false;
                            }

                            Undo.CollapseUndoOperations(idx);
                            break;
                        }
                    }
            }

            // Scene view comment toggle check
            if (previousShowCommentsBox != showCommentsBox)
            {
                previousShowCommentsBox = showCommentsBox;
                CommentBoxSceneGUI.GlobalNotes = GlobalNotes;
                if (showCommentsBox)
                    CommentBoxSceneGUI.Enable();
                else
                    CommentBoxSceneGUI.Disable();

                if (SceneView.lastActiveSceneView)
                    SceneView.lastActiveSceneView.Repaint();
            }
            if (CommentBoxSceneGUI.ShowCommentInfoInternal != CommentBoxSceneGUI.ShowCommentInfo)
            {
                CommentBoxSceneGUI.GlobalNotes = GlobalNotes;
                if (SceneView.currentDrawingSceneView)
                    SceneView.currentDrawingSceneView.Repaint();
                else if (SceneView.lastActiveSceneView)
                    SceneView.lastActiveSceneView.Repaint();
                CommentBoxSceneGUI.ShowCommentInfoInternal = CommentBoxSceneGUI.ShowCommentInfo;
            }

            // Auto snapping position
            if (doSnapPosition
                && !EditorApplication.isPlaying
                && Selection.transforms.Length > 0
                && Selection.transforms[0].position != prevPosition)
                if ((Selection.transforms[0].position - prevPosition).magnitude > snapValue)
                {
                    prevPosition = Selection.transforms[0].position;
                    previousObjectMain = currentObjectMain;
                    currentObjectMain = Selection.transforms[0].gameObject;
                }
                else if (previousObjectMain != null)
                {
                    SnapUpdate();
                    prevPosition = Selection.transforms[0].position;
                }

            // Auto snapping scale
            if (doSnapPosition
                && !EditorApplication.isPlaying
                && Selection.transforms.Length > 0
                && Selection.transforms[0].lossyScale != prevScale)
                if ((Selection.transforms[0].lossyScale - prevScale).magnitude > snapValue)
                {
                    prevScale = Selection.transforms[0].lossyScale;
                    previousObjectMain = currentObjectMain;
                    currentObjectMain = Selection.transforms[0].gameObject;
                }
                else if (previousObjectMain != null)
                {
                    ScaleSnapUpdate();
                    prevScale = Selection.transforms[0].lossyScale;
                }
            
            // Ensure valid scene camera
            if (!sceneCamera || sceneCamera != Camera.current)
                sceneCamera = Camera.current;

            // Repaint window is a area / pin comment is selected
            if (Selection.activeGameObject)
            {
                if (Selection.activeGameObject.GetComponentInParent<BlockoutPinGizmo>())
                {
                    selectedNote = Selection.activeGameObject.GetComponentInParent<BlockoutPinGizmo>()
                                            .GetComponent<Notepad>();
                    repaint = true;
                }
                else if (Selection.activeGameObject.GetComponent<BlockoutSceneViewCubeGizmo>())
                {
                    selectedNote = Selection.activeGameObject.GetComponent<BlockoutSceneViewCubeGizmo>()
                                            .GetComponent<Notepad>();
                    repaint = true;
                }
                else if (Selection.activeGameObject.GetComponent<Notepad>())
                {
                    selectedNote = Selection.activeGameObject.GetComponent<Notepad>();
                    repaint = true;
                }
                else
                {
                    selectedNote = null;
                    repaint = true;
                }
            }
            else
            {
                selectedNote = null;
                repaint = true;
            }

            // Set text colour if chanaged
            if (sceneCommentTextColor != previousSceneCommentTextColor)
            {
                CommentBoxSceneGUI.textColor = sceneCommentTextColor;
                previousSceneCommentTextColor = sceneCommentTextColor;
            }

            // Trigger validations
            currentTriggerSelectionCount = Selection.gameObjects.ToList().Where(x => x.GetComponent<BlockoutTrigger>())
                                                    .Select(s => s.GetComponent<BlockoutTrigger>()).Count();
            if (currentTriggerSelectionCount > 0)
                targetInitTrigger = Selection.gameObjects.ToList().Where(x => x.GetComponent<BlockoutTrigger>())
                                             .Select(s => s.GetComponent<BlockoutTrigger>()).ToList()[0];
            else
                targetInitTrigger = null;

            if (targetInitTrigger != previousTargetInitTrigger)
            {
                previousTargetInitTrigger = targetInitTrigger;
                repaint = true;
            }

            if (currentTriggerSelectionCount != previousTriggerSelectionCount)
            {
                previousTriggerSelectionCount = currentTriggerSelectionCount;
                repaint = true;
            }

            // potentially Redundant?
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                if (Physics.Raycast(SceneView.lastActiveSceneView.camera.ScreenPointToRay(Input.mousePosition),
                                    out hit))
                    if (hit.collider.gameObject.GetComponent<BlockoutSceneViewCubeGizmo>() ||
                        hit.collider.gameObject.GetComponent<Notepad>())
                        Selection.activeGameObject = hit.collider.gameObject;
            }

            // force window repaint
            if (repaint)
            {
                Repaint();
                repaint = false;
            }
        }

        /// <summary>
        ///     Snaps the transform position to the grid snap distance
        /// </summary>
        private void SnapUpdate()
        {
            foreach (var transform in Selection.transforms)
            {
                var p = transform.transform.position;
                p.x = Round(p.x);
                p.y = Round(p.y);
                p.z = Round(p.z);
                transform.transform.position = p;
            }
        }

        /// <summary>
        ///     Snaps the transform scale to the grid snap distance
        /// </summary>
        private void ScaleSnapUpdate()
        {
            foreach (var transform in Selection.transforms)
            {
                var s = transform.transform.lossyScale;
                s.x = Round(s.x);
                s.y = Round(s.y);
                s.z = Round(s.z);
                if (snapValue > s.x && s.x >= 0)
                    s.x = snapValue;
                if (0 > s.x && s.x >= -snapValue)
                    s.x = -snapValue;

                if (snapValue > s.y && s.y >= 0)
                    s.y = snapValue;
                if (0 > s.y && s.y >= -snapValue)
                    s.y = -snapValue;

                if (snapValue > s.z && s.z >= 0)
                    s.z = snapValue;
                if (0 > s.z && s.z >= -snapValue)
                    s.z = -snapValue;

                transform.transform.SetGlobalScale(s);
            }
        }

        /// <summary>
        ///     Rounds the specified float. Used in snapping
        /// </summary>
        /// <param name="input">The float to round.</param>
        /// <returns></returns>
        private float Round(float input)
        {
            return snapValue * Mathf.Round(input / snapValue);
        }

        /// <summary>
        /// Finds the first child object with a specific name
        /// </summary>
        /// <param name="fromGameObject">The target parent to check</param>
        /// <param name="withName">The target name</param>
        /// <returns>An array of child objects that match the name</returns>
        private GameObject GetChildGameObject(GameObject fromGameObject, string withName)
        {
            var ts = fromGameObject.GetComponentsInChildren<Transform>();
            foreach (var t in ts) if (t.gameObject.name == withName) return t.gameObject;
            return null;
        }

        /// <summary>
        /// Find child objects with a specific name
        /// </summary>
        /// <param name="fromGameObject">The target parent to check</param>
        /// <param name="withName">The target name</param>
        /// <returns>An array of child objects that match the name</returns>
        private GameObject[] GetChildGameObjects(GameObject fromGameObject, string withName)
        {
            var ts = fromGameObject.GetComponentsInChildren<Transform>();
            var targets = new List<GameObject>();
            foreach (var t in ts) if (t.gameObject.name == withName) targets.Add(t.gameObject);
            return targets.ToArray();
        }

        /// <summary>
        /// Same as find obejct opf type, but only for the active scene
        /// </summary>
        /// <typeparam name="T">Object type to search for</typeparam>
        /// <returns>An array of objetcs</returns>
        private T[] FindObjectsOfTypeInActiveScene<T>()
        {
            var targetSections = new List<T>();
            var goArray = currentActiveScene.GetRootGameObjects();
            foreach (var t in goArray) targetSections.AddRange(t.GetComponentsInChildren<T>());

            return targetSections.ToArray();
        }

        /// <summary>
        ///     Finds the blockout hierarchy.
        /// </summary>
        /// <returns>Always true as automatically repopulated if missing objects, but only false if there is no root object</returns>
        private bool FindHeirachy()
        {
            var goArray = currentActiveScene.GetRootGameObjects();
            GameObject rootObj = null;
            for (var i = 0; i < goArray.Length; i++)
            {
                rootObj = GetChildGameObject(goArray[i], "Blockout");
                if (rootObj != null)
                    break;
            }
            if (rootObj)
            {
                root = rootObj.transform;
                commentInGame = root.GetComponent<BlockoutCommentInGameGUI>();
                GlobalNotes = root.GetComponent<Notepad>();
                if (!GlobalNotes)
                    GlobalNotes = root.gameObject.AddComponent<Notepad>();

                commentInGame.GlobalNotes = GlobalNotes;
                CommentBoxSceneGUI.GlobalNotes = GlobalNotes;

                var missingSections = new List<string>();

                var targetObj = root.GetComponentsInChildren<BlockoutSection>()
                                    .Where(x => x.Section == SectionID.Floors).Select(x => x.transform).ToArray();
                if (targetObj.Length > 0)
                    floor = targetObj[0];
                else
                    missingSections.Add("Floors");

                targetObj = root.GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Walls)
                                .Select(x => x.transform).ToArray();
                if (targetObj.Length > 0)
                    walls = targetObj[0];
                else
                    missingSections.Add("Walls");

                targetObj = root.GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Trim)
                                .Select(x => x.transform).ToArray();
                if (targetObj.Length > 0)
                    trim = targetObj[0];
                else
                    missingSections.Add("Trim");

                targetObj = root.GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Dynamic)
                                .Select(x => x.transform).ToArray();
                if (targetObj.Length > 0)
                    dynamic = targetObj[0];
                else
                    missingSections.Add("Dynamic");

                targetObj = root.GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Foliage)
                                .Select(x => x.transform).ToArray();
                if (targetObj.Length > 0)
                    foliage = targetObj[0];
                else
                    missingSections.Add("Foliage");

                targetObj = root.GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Particles)
                                .Select(x => x.transform).ToArray();
                if (targetObj.Length > 0)
                    particles = targetObj[0];
                else
                    missingSections.Add("Particles");

                targetObj = root.GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Triggers)
                                .Select(x => x.transform).ToArray();
                if (targetObj.Length > 0)
                    triggers = targetObj[0];
                else
                    missingSections.Add("Triggers");

                targetObj = root.GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Cameras)
                                .Select(x => x.transform).ToArray();
                if (targetObj.Length > 0)
                    cameras = targetObj[0];
                else
                    missingSections.Add("Cameras");

                targetObj = root.GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Comments)
                                .Select(x => x.transform).ToArray();
                if (targetObj.Length > 0)
                    comments = targetObj[0];
                else
                    missingSections.Add("Comments");

                if (missingSections.Count > 0)
                    RegenHeirachy(missingSections);
                noHierarchyPresent = false;
                return true;
            }
            noHierarchyPresent = true;
            return false;
        }

        /// <summary>
        ///     Regens the hierarchy with missing object.
        /// </summary>
        /// <param name="missingObjects">The missing objects.</param>
        private void RegenHeirachy(IList<string> missingObjects)
        {
            foreach (var missingObject in missingObjects)
            {
                var newObj = new GameObject(missingObject);
                newObj.transform.SetParent(root);
                switch (missingObject)
                {
                    case "Walls":
                        walls = newObj.AddComponent<BlockoutSection>().transform;
                        walls.GetComponent<BlockoutSection>().Section = SectionID.Walls;
                        break;
                    case "Floors":
                        floor = newObj.AddComponent<BlockoutSection>().transform;
                        floor.GetComponent<BlockoutSection>().Section = SectionID.Floors;
                        break;
                    case "Trim":
                        trim = newObj.AddComponent<BlockoutSection>().transform;
                        trim.GetComponent<BlockoutSection>().Section = SectionID.Trim;
                        break;
                    case "Dynamic":
                        dynamic = newObj.AddComponent<BlockoutSection>().transform;
                        dynamic.GetComponent<BlockoutSection>().Section = SectionID.Dynamic;
                        break;
                    case "Foliage":
                        foliage = newObj.AddComponent<BlockoutSection>().transform;
                        foliage.GetComponent<BlockoutSection>().Section = SectionID.Foliage;
                        break;
                    case "Particles":
                        particles = newObj.AddComponent<BlockoutSection>().transform;
                        particles.GetComponent<BlockoutSection>().Section = SectionID.Particles;
                        break;
                    case "Triggers":
                        triggers = newObj.AddComponent<BlockoutSection>().transform;
                        triggers.GetComponent<BlockoutSection>().Section = SectionID.Triggers;
                        break;
                    case "Cameras":
                        cameras = newObj.AddComponent<BlockoutSection>().transform;
                        cameras.GetComponent<BlockoutSection>().Section = SectionID.Cameras;
                        break;
                    case "Comments":
                        comments = newObj.AddComponent<BlockoutSection>().transform;
                        comments.GetComponent<BlockoutSection>().Section = SectionID.Comments;
                        break;
                    default:
                        Debug.LogError("BLOCKOUT :: (INTERNAL)" + missingObject + " NOT VALID!");
                        break;
                }
            }
            FindHeirachy();
        }

        /// <summary>
        ///     Pings and object(s) in the project window. It will open a project window if one doesnt exist
        /// </summary>
        /// <param name="asset1">First Asset name to try (no extension)</param>
        /// <param name="asset2">Seccond Asset name to try (no extension)</param>
        private void PingAssetInProjectWindow(string asset1, string asset2)
        {
            EditorApplication.ExecuteMenuItem("Window/Project");

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var guids = AssetDatabase.FindAssets(asset1 + " t:prefab", null);
            if (guids.Length == 0)
                guids = AssetDatabase.FindAssets(asset2 + " t:prefab", null);
            if (guids.Length > 0)
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        /// <summary>
        /// Draw the main editor window
        /// </summary>
        private void OnGUI()
        {
            isVisible = true;
            globalScrollPosition = GUILayout.BeginScrollView(globalScrollPosition);

            #region Header

            GUILayout.Space(10);

            if (!logoSkin)
                logoSkin = (GUISkin) Resources.Load("Blockout/UI_Icons/BlockoutEditorSkin", typeof(GUISkin));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (noHierarchyPresent)
            {
                GUILayout.Box(!selectedNote && !noHierarchyPresent ? blockoutAssetSubheader : logo,
                              logoSkin.GetStyle("Texture"), GUILayout.Width(375), GUILayout.Height(90));
            }
            else
            {
                if (selectedNote)
                    if (GUILayout.Button(backIcon, GUILayout.Height(23), GUILayout.Width(23)))
                        Selection.activeGameObject = null;
                GUILayout.Box(
                              !selectedNote && !noHierarchyPresent
                                  ? blockoutAssetSubheader
                                  : selectedNote
                                      ? commentsHeader
                                      : logo, logoSkin.GetStyle("Texture"),
                              selectedNote ? GUILayout.MaxWidth(345) : GUILayout.MaxWidth(385), GUILayout.Height(30));
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (!root)
            {
                if (!currentActiveScene.IsValid())
                {
                    GUILayout.EndHorizontal();
                    GUILayout.EndScrollView();
                    return;
                }

                var goArray = currentActiveScene.GetRootGameObjects();
                GameObject rootObj = null;
                for (var i = 0; i < goArray.Length; i++)
                {
                    rootObj = GetChildGameObject(goArray[i], "Blockout");
                    if (rootObj != null)
                        break;
                }
                if (rootObj)
                {
                    if (!FindHeirachy())
                        Debug.LogError(
                                       "BLOCKOUT :: Unable to find blockout hierarchy...I think you misspelled something");
                    else
                        noHierarchyPresent = false;
                }
                else
                {
                    noHierarchyPresent = true;


                    GUILayout.BeginVertical(GUILayout.Width(390));
					GUILayout.Space(10);
                    // Create the blockout hierarchy with floors, walls, trim, dynaminc and foliage gameobejcs all children of a blockout root objct
                    if (GUILayout.Button(createHierarchyLabel, GUILayout.Width(390),
                                         GUILayout.Height(EditorGUIUtility.singleLineHeight * 6)))
                        if (!FindHeirachy())
                            if (EditorSceneManager.SaveOpenScenes())
                            {
                                Undo.IncrementCurrentGroup();
                                Undo.SetCurrentGroupName("Create Blockout Hierarchy");
                                var undoGroupIdx = Undo.GetCurrentGroup();
                                root = new GameObject("Blockout").GetComponent<Transform>();
                                Undo.RegisterCreatedObjectUndo(root.gameObject, "");
                                commentInGame = root.gameObject.AddComponent<BlockoutCommentInGameGUI>();
                                GlobalNotes = root.gameObject.AddComponent<Notepad>();
                                commentInGame.GlobalNotes = GlobalNotes;
                                var section = root.gameObject.AddComponent<BlockoutSection>();
                                section.Section = SectionID.Root;
                                CreateBlockoutSubHeirachyWithRoot(root);
                                FindHeirachy();
                                Undo.CollapseUndoOperations(undoGroupIdx);
                                TryLoadSceneDefinitions();
                                CommentBoxSceneGUI.GlobalNotes = GlobalNotes;
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("Blockout Editor Error",
                                                            "You need to have a SAVED scene before using the Blockout system",
                                                            "OK");
                            }
                    GUILayout.Space(5);
                    GUILayout.Label(introLabel);
                    GUILayout.Space(5);


                    GUILayout.Label(exampleScenes, GUILayout.Width(390), GUILayout.Height(EditorGUIUtility.singleLineHeight * 2));
                    if (GUILayout.Button(allAssetsButton, GUILayout.Width(390),
                                         GUILayout.Height(EditorGUIUtility.singleLineHeight * 3 + 8)))
                        EditorSceneManager.OpenScene("Assets/Blockout/Examples/All Assets.unity");

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(fpsExampleLabel, GUILayout.Width(193), GUILayout.Height(125)))
                        EditorSceneManager.OpenScene("Assets/Blockout/Examples/FPS.unity");
                    if (GUILayout.Button(rollerballExampleLabel, GUILayout.Width(193), GUILayout.Height(125)))
                        EditorSceneManager.OpenScene("Assets/Blockout/Examples/Rollerball.unity");
                    //if (GUILayout.Button(patformerLabel, GUILayout.Width(128), GUILayout.Height(128)))
                    //    EditorSceneManager.OpenScene("Assets/Blockout/Examples/2D.unity");
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);


                    GUILayout.Label("Keyboard Shortcuts");
                    GUILayout.BeginVertical();
                    if (Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Ctrl + ALT + B", EditorStyles.boldLabel, GUILayout.Width(152));
                        GUILayout.Label("Show The Blockout Window");
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Cmd + ALT + B", EditorStyles.boldLabel, GUILayout.Width(152));
                        GUILayout.Label("Show / Hide The Blockout Window");
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Alt + S", EditorStyles.boldLabel, GUILayout.Width(152));
                    GUILayout.Label("Toggle Auto Grid Snapping");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Alt + C", EditorStyles.boldLabel, GUILayout.Width(152));
                    GUILayout.Label("Toggle Comments");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Alt + Z", EditorStyles.boldLabel, GUILayout.Width(152));
                    GUILayout.Label("Decrease Grid Snapping Value");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Alt + X", EditorStyles.boldLabel, GUILayout.Width(152));
                    GUILayout.Label("Increase Grid Snapping Value");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("End", EditorStyles.boldLabel, GUILayout.Width(152));
                    GUILayout.Label("Snap to ground (-Y)");
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("G", EditorStyles.boldLabel, GUILayout.Width(152));
                    GUILayout.Label("Jump To Selected Prefab In Project");
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();

                    GUILayout.Space(5);



                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(openDocumentationLabel, GUILayout.Width(193), GUILayout.Height(EditorGUIUtility.singleLineHeight * 3 + 8)))
                        OpenDocumentation();
                    if (GUILayout.Button(openTutorialsLabel, GUILayout.Width(193), GUILayout.Height(EditorGUIUtility.singleLineHeight * 3 + 8)))
                        OpennTutorials();
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    if (GUILayout.Button(feedbackLabel, GUILayout.Width(390), GUILayout.Height(EditorGUIUtility.singleLineHeight * 3 + 8)))
                        SubmitBugReport();

                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Box(radforgeLogo, logoSkin.GetStyle("Texture"), GUILayout.Width(150),
                                  GUILayout.Height(45));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }
            }
            else if (!walls || !foliage || !trim || !floor || !triggers || !particles || !dynamic || !cameras)
            {
                if (GUILayout.Button(refreshHierarchyLabel, GUILayout.MaxWidth(380)))
                    FindHeirachy();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            #endregion

            if (!selectedNote)
            {
                if (!noHierarchyPresent)
                {
                    #region Quick Jump

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(blockButtonLabel,
                                         GUILayout.Width(60), GUILayout.Height(60)))
                        PingAssetInProjectWindow("Block_1x1x1", "Block_Slope_1x1x1");
                    if (GUILayout.Button(wallButtonLabel,
                                         GUILayout.Width(60), GUILayout.Height(60)))
                        PingAssetInProjectWindow("Wall_025x1x1", "Wall_025x3x1");

                    if (GUILayout.Button(floorButtonLabel,
                                         GUILayout.Width(60), GUILayout.Height(60)))
                        PingAssetInProjectWindow("Floor_1x-025x1", "Floor_Angle_3x - 025x3");
                    if (GUILayout.Button(dynamicButtonLabel,
                                         GUILayout.Width(60), GUILayout.Height(60)))
                        PingAssetInProjectWindow("Barrel", "Crate_1x1x1");
                    if (GUILayout.Button(foliageButtonLabel,
                                         GUILayout.Width(60), GUILayout.Height(60)))
                        PingAssetInProjectWindow("Bush_1", "Vines_Large");

                    if (GUILayout.Button(particlesButtonLabel,
                                         GUILayout.Width(60), GUILayout.Height(60)))
                        PingAssetInProjectWindow("Fire_Ground_1x1", "Water_Drip_1");


                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    #endregion

                    #region Style

                    GUILayout.BeginHorizontal();

                    GUILayout.FlexibleSpace();
                    // Loop through all the loaded grid textures and display them in a button.
                    // If its selected then apply that texture to every gameobject in the scene
                    for (var i = 0; i < gridTextures.Count; ++i)
                    {
                        if (i >= gridIcons.Count)
                            continue;
                        if (GUILayout.Button(gridIconLabels[i],
                                             GUILayout.MaxHeight(60), GUILayout.MinHeight(60), GUILayout.MaxWidth(60),
                                             GUILayout.MinWidth(60)))
                        {
                            currentGirdTexture = i;

                            ApplyTextureIncChildren(gridTextures[currentGirdTexture]);
                        }
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();


                    GUILayout.BeginHorizontal();


                    GUILayout.FlexibleSpace();
                    // Loop through all the loaded themes and display its index in a button.
                    // If its selected then apply that theme to every gameobject in the blockout hierarchy
                    for (var i = 0; i < themes.Count; ++i)
                        if (GUILayout.Button(themeLabels[i],
                                             GUILayout.MaxWidth(60), GUILayout.MaxHeight(30), GUILayout.MinWidth(60),
                                             GUILayout.MinHeight(30)))
                        {
                            if (!root)
                                if (!FindHeirachy())
                                {
                                    Debug.LogError("BLOCKOUT :: Unabe to apply theme, there is no blockout hierarchy!");
                                    break;
                                }
                            currentMaterialTheme = i;
                            currentPallet = PalletType.Preset;
                            ApplyTheme(themes[currentMaterialTheme]);
                        }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(randomisePalletLabel,
                                         GUILayout.Width(124)))
                    {
                        GenerateRandomTheme();
                        currentMaterialTheme = themes.Count + 1;
                        ApplyTheme(userTheme);
                        currentPallet = PalletType.User;
                    }
                    if (GUILayout.Button(applyPalletLabel,
                                         GUILayout.Width(124)))
                    {
                        currentMaterialTheme = themes.Count + 1;
                        ApplyTheme(userTheme);
                        currentPallet = PalletType.User;
                    }
                    if (GUILayout.Button(editPalletLabel, GUILayout.Width(124)))
                        userPalletFoldout = !userPalletFoldout;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();


                    if (userPalletFoldout)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginVertical(GUILayout.MaxWidth(375));

                        if (!Application.isPlaying && Application.isEditor)
                            for (var i = 0; i < colorStrings.Length; ++i)
                            {
                                GUILayout.BeginHorizontal();
                                userThemeColors[i] = EditorGUILayout.ColorField(colorStrings[i], userThemeColors[i]);

                                lockColors[i] = GUILayout.Toggle(lockColors[i],
                                                                 lockColorLabel,
                                                                 logoSkin.FindStyle("padlock"), GUILayout.Height(15),
                                                                 GUILayout.Width(15));
                                GUILayout.EndHorizontal();
                            }
                        else
                            GUILayout.Label("Feature not availble during play mode.");

                        userTheme.FloorMaterial.SetColor("_Color", userThemeColors[0]);
                        userTheme.TriFloor.SetColor("_Color", userThemeColors[0]);
                        userTheme.WallMaterial.SetColor("_Color", userThemeColors[1]);
                        userTheme.TriWalls.SetColor("_Color", userThemeColors[1]);
                        userTheme.DynamicMaterial.SetColor("_Color", userThemeColors[2]);
                        userTheme.TrimMaterial.SetColor("_Color", userThemeColors[3]);
                        userTheme.TriTrim.SetColor("_Color", userThemeColors[3]);
                        userTheme.FoliageMaterial.SetColor("_Color", userThemeColors[4]);
                        userTheme.LeavesMaterial.SetColor("_Color_1", userThemeColors[5]);
                        userTheme.WaterMateral.SetColor("_Color_1", userThemeColors[6]);
                        userTheme.TriggerMaterial.SetColor("_Color_1", userThemeColors[7]);

                        GUILayout.EndVertical();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }

                    #region Replace Asset

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(selectAssetLabel, GUILayout.Width(124)))
                        SelectAsset();

                    if (!Application.isPlaying && Application.isEditor)
                    {
                        targetReplacementAsset =
                            EditorGUILayout.ObjectField(new GUIContent(""), targetReplacementAsset,
                                                        typeof(GameObject), false, GUILayout.Width(124)) as GameObject;

                        if (GUILayout.Button(replaceAssetLabel, GUILayout.Width(124)))
                        {
                            Undo.IncrementCurrentGroup();
                            Undo.SetCurrentGroupName("Replace Assets");
                            var targetIdx = Undo.GetCurrentGroup();

                            var sel = Selection.gameObjects.ToList();
                            foreach (var s in sel)
                            {
                                var pos = s.transform.position;
                                var rot = s.transform.rotation;
                                var newAsset = Instantiate(targetReplacementAsset, pos, rot);
                                Undo.RegisterCreatedObjectUndo(newAsset, "");
                            }
                            sel.ForEach(x => Undo.DestroyObjectImmediate(x));

                            Undo.CollapseUndoOperations(targetIdx);
                        }
                    }
                    else
                    {
                        GUILayout.Label("Feature not availble during play mode.");
                    }


                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    #endregion

                    #endregion

                    GUILayout.Space(5);

                    #region Scalable Assets

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal(GUILayout.Width(390));
                    GUILayout.Box(triHeader, logoSkin.GetStyle("Texture"), GUILayout.Width(256), GUILayout.Height(30));
                    GUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();


                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(scalableWallLabel,
                                         GUILayout.Height(EditorGUIUtility.singleLineHeight * 3),
                                         GUILayout.Width(124)))
                        CreateTriPlanerAsset(AssetDatabase.FindAssets("Tri_Wall t:prefab")[0]);
                    if (GUILayout.Button(scalableFloorLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight * 3),
                                         GUILayout.Width(124)))
                        CreateTriPlanerAsset(AssetDatabase.FindAssets("Tri_Floor t:prefab")[0]);
                    if (GUILayout.Button(scalableTrimLabel,
                                         GUILayout.Height(EditorGUIUtility.singleLineHeight * 3),
                                         GUILayout.Width(124)))
                        CreateTriPlanerAsset(AssetDatabase.FindAssets("Tri_Trim t:prefab")[0]);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    #endregion

                    #region Transform Controls

                    // Helpful controls to zero out parts of a transform 

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal(GUILayout.Width(390));
                    GUILayout.Box(transformHeader, logoSkin.GetStyle("Texture"), GUILayout.MaxWidth(256),
                                  GUILayout.MaxHeight(30));
                    GUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();


                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(centerGlobalPositionLabel,
                                         GUILayout.Width(190)))
                    {
                        var selection = Selection.gameObjects;
                        Undo.RecordObjects(selection.ToList().Select(x => x.transform).ToArray(),
                                           "Reset To Global Position");
                        foreach (var sel in selection)
                            sel.transform.position = Vector3.zero;
                    }

                    if (GUILayout.Button(
                                         centerLocalPositionLabel, GUILayout.Width(190)))
                    {
                        var sel = Selection.gameObjects;
                        Undo.RecordObjects(sel.Select(x => x.transform).ToArray(), "Reset To Parent Position");
                        foreach (var s in sel)
                            s.transform.localPosition = Vector3.zero;
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(centerGlobalRotationLabel,
                                         GUILayout.Width(190)))
                    {
                        var sel = Selection.gameObjects;
                        Undo.RecordObjects(sel.Select(x => x.transform).ToArray(), "Reset Global Rotation");
                        for (var i = 0; i < sel.Length; ++i)
                            sel[i].transform.rotation = Quaternion.identity;
                    }

                    if (GUILayout.Button(centerLocalRotationLabel,
                                         GUILayout.Width(190)))
                    {
                        var sel = Selection.gameObjects;
                        Undo.RecordObjects(sel.Select(x => x.transform).ToArray(), "Reset To Parent Rotation");
                        for (var i = 0; i < sel.Length; ++i)
                            sel[i].transform.localRotation = Quaternion.identity;
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    // rotate an object along the global axis
                    GUILayout.BeginHorizontal();
                    var originalColour = GUI.backgroundColor;
                    var originalContentColour = GUI.contentColor;
                    GUI.backgroundColor = new Color(1.0f, 0.467f, 0.465f);

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(rotateX90Label,
                                         GUILayout.Width(60), GUILayout.Height(60)))
                        Rotate(Vector3.right, -90);
                    if (GUILayout.Button(rotateX180Label,
                                         GUILayout.Width(60), GUILayout.Height(60)))
                        Rotate(Vector3.right, 180);
                    GUI.backgroundColor = new Color(0.467f, 1.0f, 0.514f);
                    if (GUILayout.Button(rotateY90Label,
                                         GUILayout.Width(60), GUILayout.Height(60)))
                        Rotate(Vector3.up, -90);
                    if (GUILayout.Button(rotateY180Label,
                                         GUILayout.Width(60), GUILayout.Height(60)))
                        Rotate(Vector3.up, 180);
                    GUI.backgroundColor = new Color(0.467f, 0.67f, 1.0f);
                    if (GUILayout.Button(rotateZ90Label,
                                         GUILayout.Width(60), GUILayout.Height(60)))
                        Rotate(Vector3.forward, 90);
                    if (GUILayout.Button(rotateZ180Label,
                                         GUILayout.Width(60), GUILayout.Height(60)))

                        Rotate(Vector3.forward, 180);
                    GUILayout.FlexibleSpace();
                    GUI.backgroundColor = originalColour;
                    GUI.contentColor = originalContentColour;
                    GUILayout.EndHorizontal();

                    // Snap an object along the global axis
                    GUILayout.BeginHorizontal();
                    originalColour = GUI.backgroundColor;

                    GUI.backgroundColor = backgroundRed;

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(mirrorXLabel, GUILayout.Width(124),
                                         GUILayout.Height(30)))
                        Mirror(-Vector3.right);
                    GUI.backgroundColor = backgroundGreen;
                    if (GUILayout.Button(mirrorYLabel, GUILayout.Width(124),
                                         GUILayout.Height(30)))
                        Mirror(-Vector3.up);
                    GUI.backgroundColor = backgroundBlue;
                    if (GUILayout.Button(mirrorZLabel, GUILayout.Width(124),
                                         GUILayout.Height(30)))
                        Mirror(-Vector3.forward);
                    GUILayout.FlexibleSpace();
                    GUI.backgroundColor = originalColour;
                    GUILayout.EndHorizontal();

                    #endregion

                    #region Snap To Grid

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal(GUILayout.Width(390));
                    GUILayout.Box(snapHeader, logoSkin.GetStyle("Texture"), GUILayout.MaxWidth(256),
                                  GUILayout.MaxHeight(30));
                    GUILayout.FlexibleSpace();
                    GUILayout.Box(autoText, logoSkin.GetStyle("Texture"), GUILayout.Width(60), GUILayout.Height(30));
                    doSnapPosition = GUILayout.Toggle(doSnapPosition, "",
                                                      logoSkin.button, GUILayout.Height(30),
                                                      GUILayout.Width(30));
                    GUILayout.Space(5);
                    GUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(snapDistanceLabel);
                    selected = GUILayout.SelectionGrid(selected, Options, Options.Length, EditorStyles.miniButton,
                                                       GUILayout.Width(297));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    if (!Application.isPlaying && Application.isEditor)
                        if (selected < OptionValues.Length && selected > 0)
                        {
                            snapValue = OptionValues[selected];
                        }
                        else
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();

                            snapValue = EditorGUILayout.FloatField("Custom Snap", snapValue, GUILayout.Width(375));
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }
                    else
                        GUILayout.Label("Feature not availble during play mode.");

                    // Snap an object along the global axis
                    GUILayout.BeginHorizontal();
                    originalColour = GUI.backgroundColor;
                    GUI.backgroundColor = backgroundRed;
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(snapPXLabel,
                                         GUILayout.Width(60)))
                        Snap(Vector3.right, BlockoutAxis.X);
                    if (GUILayout.Button(snapNXLabel,
                                         GUILayout.Width(60)))
                        Snap(-Vector3.right, BlockoutAxis.X);
                    GUI.backgroundColor = backgroundGreen;
                    if (GUILayout.Button(snapPYLabel,
                                         GUILayout.Width(60)))
                        Snap(Vector3.up, BlockoutAxis.Y);
                    if (GUILayout.Button(snapNYLabel,
                                         GUILayout.Width(60)))
                        Snap(-Vector3.up, BlockoutAxis.Y);
                    GUI.backgroundColor = backgroundBlue;
                    if (GUILayout.Button(snapPZLabel,
                                         GUILayout.Width(60)))
                        Snap(Vector3.forward, BlockoutAxis.Z);
                    if (GUILayout.Button(snapNZLabel,
                                         GUILayout.Width(60)))
                        Snap(-Vector3.forward, BlockoutAxis.Z);
                    GUILayout.FlexibleSpace();
                    GUI.backgroundColor = originalColour;
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    #endregion

                    #region Hierarchy Tools

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal(GUILayout.Width(375));
                    showHeirachyToolsSection = GUILayout.Toggle(showHeirachyToolsSection, "",
                                                                logoSkin.button, GUILayout.Height(30),
                                                                GUILayout.Width(30));
                    GUILayout.Box(heiracyHeader, logoSkin.GetStyle("Texture"), GUILayout.MaxWidth(256),
                                  GUILayout.MaxHeight(30));
                    GUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    if (showHeirachyToolsSection)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginHorizontal(GUILayout.MaxWidth(365));
                        GUILayout.Label("Name:");
                        parentName = GUILayout.TextField(parentName);

                        // Change the material of the selected gameobejcts to the material specified in the material field
                        if (GUILayout.Button(createParentLabel))
                            CreateParentAndForceChild();
                        GUILayout.EndHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginHorizontal(GUILayout.Width(365));
                        GUILayout.Label("Target: ");
                        if (!Application.isPlaying && Application.isEditor)
                        {
                            targetHeirachyBaseObject =
                                EditorGUILayout.ObjectField(targetHeirachyBaseObject, typeof(GameObject),
                                                            true) as GameObject;

                            if (GUILayout.Button(hierarchySelectedLabel))
                                if (!targetHeirachyBaseObject)
                                {
                                    Debug.LogError("BLOCKOUT :: Assign a target object to hierarchy to!");
                                }
                                else
                                {
                                    Selection.gameObjects.ToList().Select(x => x.transform).ToList()
                                             .ForEach(x => x.parent = targetHeirachyBaseObject.transform);
                                    EditorGUIUtility.PingObject(targetHeirachyBaseObject);
                                    Selection.activeGameObject = targetHeirachyBaseObject;
                                }
                        }
                        else
                        {
                            GUILayout.Label("Feature Not available during play mode");
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();


                        // The buttons below reparent the selected gameobejects to pre-established roots in the blockout hierarchy. This only works if the blockout hierarchy was created using the
                        // Auto Create Scene Hierarchy button above 

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginHorizontal(GUILayout.Width(365));
                        GUILayout.Label("Reparent Selection");
                        GUILayout.EndHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();


                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginHorizontal(GUILayout.Width(365));
                        if (GUILayout.Button(reparentFloorLabel))
                            ApplyParentTransformFromSection(Selection.gameObjects, SectionID.Floors);
                        if (GUILayout.Button(reparentWallsLabel))
                            ApplyParentTransformFromSection(Selection.gameObjects, SectionID.Walls);
                        if (GUILayout.Button(reparentTrimLabel))
                            ApplyParentTransformFromSection(Selection.gameObjects, SectionID.Trim);
                        if (GUILayout.Button(reparentDynamicLabel))
                            ApplyParentTransformFromSection(Selection.gameObjects, SectionID.Dynamic);
                        if (GUILayout.Button(reparentFoliageLabel))
                            ApplyParentTransformFromSection(Selection.gameObjects, SectionID.Foliage);
                        if (GUILayout.Button(reparentParticleLabel))
                            ApplyParentTransformFromSection(Selection.gameObjects, SectionID.Particles);
                        GUILayout.EndHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginHorizontal(GUILayout.Width(365));
                        if (GUILayout.Button(createPrefabLabel))
                            AutoPrefabSelection.PrefabSelection();
                        GUILayout.EndHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.Space(5);
                    }

                    #endregion

                    #region Comment Tools

                    DrawCommentSection();

                    #endregion

                    #region Camera Anchors

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal(GUILayout.Width(375));
                    showCameraAnchors = GUILayout.Toggle(showCameraAnchors, "",
                                                         logoSkin.button, GUILayout.Height(30),
                                                         GUILayout.Width(30));
                    GUILayout.Box(cameraAnchor, logoSkin.GetStyle("Texture"), GUILayout.MaxWidth(256),
                                  GUILayout.MaxHeight(30));
                    GUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();


                    if (showCameraAnchors)
                    {
                        GUILayout.Space(5);
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginHorizontal(GUILayout.MaxWidth(365));
                        anchorName = GUILayout.TextField(anchorName);
                        if (GUILayout.Button(saveSceneCameraLabel, GUILayout.MaxWidth(182)))
                            if (root.GetComponentsInChildren(typeof(Transform), true).Where(x => x.name == anchorName)
                                    .ToList().Count > 0)
                            {
                                Debug.LogError(
                                               "BLOCKOUT :: Unable to create new camera anchor. Please unsure the name is unique");
                            }
                            else if (SceneView.lastActiveSceneView)
                            {
                                var newAnchor = new CameraAnchor
                                {
                                    name = anchorName,
                                    position = SceneView.lastActiveSceneView.pivot,
                                    rotation = SceneView.lastActiveSceneView.rotation,
                                    size = SceneView.lastActiveSceneView.size
                                };

                                Undo.IncrementCurrentGroup();
                                Undo.SetCurrentGroupName("Create Camera Anchor");
                                var undoGround = Undo.GetCurrentGroup();

                                var newCamera = new GameObject(newAnchor.name);
                                Undo.RegisterCreatedObjectUndo(newCamera, "");
                                var cam = newCamera.AddComponent<Camera>();
                                cam.depth = 100;

                                newCamera.transform.position =
                                    SceneView.lastActiveSceneView.camera
                                             .ScreenToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
                                newCamera.transform.rotation = newAnchor.rotation;
                                newCamera.transform.SetParent(cameras);
                                newCamera.SetActive(false);

                                Undo.RecordObject(CurrentSceneSetting, "Create Camera Anchor");
                                CurrentSceneSetting.cameraAnchor.Add(newAnchor);

                                EditorUtility.SetDirty(CurrentSceneSetting);
                                AssetDatabase.SaveAssets();

                                Undo.CollapseUndoOperations(undoGround);
                            }
                        GUILayout.EndHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.Space(5);

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginVertical(GUILayout.MaxWidth(365));
                        if (CurrentSceneSetting.cameraAnchor.Count > 0)
                            for (var i = 0; i < CurrentSceneSetting.cameraAnchor.Count; ++i)
                            {
                                GUILayout.BeginHorizontal(GUILayout.MaxWidth(365));

                                GUILayout.Space(10);

                                if (GUILayout.Button(i + ". " +
                                                     CurrentSceneSetting.cameraAnchor[i].name,
                                                     GUILayout.ExpandWidth(true),
                                                     GUILayout.Height(EditorGUIUtility.singleLineHeight + 5)))
                                    if (i >= cameras.childCount)
                                    {
                                        CurrentSceneSetting.cameraAnchor.RemoveAt(i);
                                        CurrentSceneSetting.cameraAnchor.TrimExcess();
                                        Debug.LogError("BLOCKOUT :: Camera Anchor In Scene Deleted...Removing Anchor");
                                    }
                                    else if (cameras.GetChild(i).gameObject.name !=
                                             CurrentSceneSetting.cameraAnchor[i].name)
                                    {
                                        CurrentSceneSetting.cameraAnchor.RemoveAt(i);
                                        CurrentSceneSetting.cameraAnchor.TrimExcess();
                                        Debug.LogError("BLOCKOUT :: Camera Anchor In Scene Deleted...Removing Anchor");
                                    }
                                    else
                                    {
                                        if (SceneView.lastActiveSceneView)
                                        {
                                            var currentAnchor = CurrentSceneSetting.cameraAnchor[i];
                                            SceneView.lastActiveSceneView.size = currentAnchor.size;
                                            SceneView.lastActiveSceneView.pivot = currentAnchor.position;
                                            SceneView.lastActiveSceneView.rotation = currentAnchor.rotation;
                                        }
                                    }

                                if (i >= cameras.childCount)
                                {
                                    CurrentSceneSetting.cameraAnchor.RemoveAt(i);
                                    CurrentSceneSetting.cameraAnchor.TrimExcess();
                                    GUILayout.Space(10);
                                    GUILayout.EndHorizontal();
                                    continue;
                                }


                                #region Screenshot

                                // SCREENSHOT
                                if (GUILayout.Button(screenshotWhiteLabel, GUILayout.Width(25),
                                                     GUILayout.Height(EditorGUIUtility.singleLineHeight + 5)))
                                {
                                    CommentBoxSceneGUI.Disable();
                                    ApplyNewMaterialSchemeWithoutUndo(screenshotWhiteMaterial, root.gameObject);

                                    var resWidthN = screenshotWidth * supersizeResolution;
                                    var resHeightN = screenshotHeight * supersizeResolution;

                                    var rt = new RenderTexture(resWidthN, resHeightN, 24);

                                    var myCamera = cameras.GetChild(i).GetComponent<Camera>();
                                    var currentRT = myCamera.targetTexture;

                                    myCamera.targetTexture = rt;

                                    TextureFormat tFormat;
                                    tFormat = TextureFormat.RGB24;


                                    var screenShot = new Texture2D(resWidthN, resHeightN, tFormat, false);
                                    myCamera.Render();
                                    RenderTexture.active = rt;
                                    screenShot.ReadPixels(new Rect(0, 0, resWidthN, resHeightN), 0, 0);
                                    myCamera.targetTexture = null;
                                    RenderTexture.active = null;
                                    var bytes = screenShot.EncodeToPNG();
                                    var filename = "BLOCKOUT SCREENSHOT " + DateTime.Now.ToString("yyyyMMddHHmmss") +
                                                   "_" + resWidthN + "x" + resHeightN + ".png";

                                    File.WriteAllBytes(filename, bytes);
                                    Debug.Log(string.Format("Took screenshot to: {0}", filename));
                                    Application.OpenURL(filename);

                                    myCamera.targetTexture = currentRT;

                                    Debug.Log("SAVED SCREENSHOT: " + Application.dataPath + "/../" + filename);

                                    EditorUtility.RevealInFinder(Application.dataPath + "/../" + filename);

                                    ApplyCurrentTheme();
                                }

                                if (GUILayout.Button(screenshotColorLabel, GUILayout.Width(25),
                                                     GUILayout.Height(EditorGUIUtility.singleLineHeight + 5)))
                                {
                                    CommentBoxSceneGUI.Disable();
                                    var resWidthN = screenshotWidth * supersizeResolution;
                                    var resHeightN = screenshotHeight * supersizeResolution;

                                    var rt = new RenderTexture(resWidthN, resHeightN, 24);

                                    var myCamera = cameras.GetChild(i).GetComponent<Camera>();
                                    var currentRT = myCamera.targetTexture;

                                    myCamera.targetTexture = rt;

                                    TextureFormat tFormat;
                                    tFormat = TextureFormat.RGB24;


                                    var screenShot = new Texture2D(resWidthN, resHeightN, tFormat, false);
                                    myCamera.Render();
                                    RenderTexture.active = rt;
                                    screenShot.ReadPixels(new Rect(0, 0, resWidthN, resHeightN), 0, 0);
                                    myCamera.targetTexture = null;
                                    RenderTexture.active = null;
                                    var bytes = screenShot.EncodeToPNG();
                                    var filename = "BLOCKOUT SCREENSHOT " + DateTime.Now.ToString("yyyyMMddHHmmss") +
                                                   "_" + resWidthN + "x" + resHeightN + ".png";

                                    File.WriteAllBytes(filename, bytes);
                                    Application.OpenURL(filename);

                                    myCamera.targetTexture = currentRT;

                                    Debug.Log("SAVED SCREENSHOT: " + Application.dataPath + "/../" + filename);

                                    EditorUtility.RevealInFinder(Application.dataPath + "/../" + filename);
                                }

                                #endregion


                                if (GUILayout.Button(
                                                     cameras.GetChild(i).gameObject.activeInHierarchy
                                                         ? gameVisOnLabel
                                                         : gameVisOffLabel,
                                                     GUILayout.MaxWidth(25),
                                                     GUILayout.Height(EditorGUIUtility.singleLineHeight + 5)))
                                    if (i >= cameras.childCount)
                                    {
                                        CurrentSceneSetting.cameraAnchor.RemoveAt(i);
                                        CurrentSceneSetting.cameraAnchor.TrimExcess();
                                        Debug.LogError("BLOCKOUT :: Camera Anchor In Scene Deleted...Removing Anchor");
                                    }
                                    else if (cameras.GetChild(i).gameObject.name !=
                                             CurrentSceneSetting.cameraAnchor[i].name)
                                    {
                                        CurrentSceneSetting.cameraAnchor.RemoveAt(i);
                                        CurrentSceneSetting.cameraAnchor.TrimExcess();
                                        Debug.LogError("BLOCKOUT :: Camera Anchor In Scene Deleted...Removing Anchor");
                                    }
                                    else
                                    {
                                        if (!cameras)
                                            if (!FindHeirachy())
                                                FindHeirachy();
                                        for (var c = 0; c < cameras.childCount; ++c)
                                        {
                                            if (c == i)
                                                continue;
                                            cameras.GetChild(c).gameObject.SetActive(false);
                                        }
                                        cameras.GetChild(i).gameObject
                                               .SetActive(!cameras.GetChild(i).gameObject.activeSelf);
                                    }


                                if (GUILayout.Button(trashCanLabel,
                                                     GUILayout.Width(EditorGUIUtility.singleLineHeight + 5),
                                                     GUILayout.Height(EditorGUIUtility.singleLineHeight + 5)))
                                {
                                    Undo.IncrementCurrentGroup();
                                    Undo.SetCurrentGroupName("Delete Camera Anchor");
                                    var undoGround = Undo.GetCurrentGroup();

                                    Undo.RecordObject(CurrentSceneSetting, "Destroy Camera Anchor");
                                    CurrentSceneSetting.cameraAnchor.RemoveAt(i);
                                    CurrentSceneSetting.cameraAnchor.TrimExcess();

                                    EditorUtility.SetDirty(CurrentSceneSetting);
                                    AssetDatabase.SaveAssets();

                                    Undo.RecordObject(cameras.GetChild(i).gameObject, "");
                                    DestroyImmediate(cameras.GetChild(i).gameObject);

                                    Undo.CollapseUndoOperations(undoGround);
                                }


                                GUILayout.Space(10);
                                GUILayout.EndHorizontal();
                            }
                        else
                            GUILayout.Label("There are no camera anchors defined");
                        GUILayout.EndVertical();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.Space(5);

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginVertical(GUILayout.MaxWidth(365));

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(clearAllLabel, GUILayout.MaxWidth(375)))
                        {
                            Undo.IncrementCurrentGroup();
                            Undo.SetCurrentGroupName("Clear All Camera Anchors");
                            var undoGround = Undo.GetCurrentGroup();

                            var toDelete = new List<GameObject>();

                            for (var i = cameras.childCount - 1; i >= 0; --i)
                                toDelete.Add(cameras.GetChild(i).gameObject);
                            Undo.RecordObjects(toDelete.ToArray(), "");
                            for (var i = toDelete.Count - 1; i >= 0; --i)
                                DestroyImmediate(toDelete[i]);
                            Undo.RecordObject(CurrentSceneSetting, "");
                            CurrentSceneSetting.cameraAnchor.Clear();

                            EditorUtility.SetDirty(CurrentSceneSetting);
                            AssetDatabase.SaveAssets();

                            Undo.CollapseUndoOperations(undoGround);
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginHorizontal(GUILayout.Width(390));
                        GUILayout.Space(4);
                        GUILayout.Box(screenshotSubHeader, logoSkin.GetStyle("Texture"), GUILayout.Width(256),
                                      GUILayout.Height(24));
                        GUILayout.EndHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginHorizontal(GUILayout.Width(390));
                        GUILayout.Space(8);
                        GUILayout.BeginVertical();
                        if (!Application.isPlaying && Application.isEditor)
                            supersizeResolution =
                                EditorGUILayout.IntSlider(resolutionScaleLabel, supersizeResolution, 1, 10);
                        else
                            GUILayout.Label("Feature Not Available during play mode");
                        screenshotWidth = EditorGUILayout.IntField(widthScaleLabel, screenshotWidth);
                        screenshotHeight = EditorGUILayout.IntField(heightScaleLabel, screenshotHeight);
                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();


                        GUILayout.EndVertical();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.Space(5);
                    }

                    #endregion

                    #region Other Tools

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal(GUILayout.Width(375));
                    showOtherSection = GUILayout.Toggle(showOtherSection, "",
                                                        logoSkin.button, GUILayout.Height(30),
                                                        GUILayout.Width(30));
                    GUILayout.Box(otherHeader, logoSkin.GetStyle("Texture"), GUILayout.MaxWidth(256),
                                  GUILayout.MaxHeight(30));
                    GUILayout.EndHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    if (showOtherSection)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(createTriggerLabel,
                                             GUILayout.Width(57), GUILayout.Height(57)))
                        {
                            var triggerBox =
                                (GameObject) AssetDatabase.LoadAssetAtPath(
                                                                           AssetDatabase.GUIDToAssetPath(AssetDatabase
                                                                                                             .FindAssets("Trigger t:prefab")
                                                                                                             [0]),
                                                                           typeof(GameObject));
                            var target = Instantiate(triggerBox);
                            Undo.RegisterCreatedObjectUndo(target, "Created Blockout Trigger");
                            Selection.activeObject = target;

                            var spawnPos =
                                SceneView.lastActiveSceneView.camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

                            target.transform.position = spawnPos;
                            target.transform.SetParent(triggers);
                            target.name = triggerBox.name;
                            Selection.activeGameObject = target;
                            SnapUpdate();
                            SceneView.lastActiveSceneView.FrameSelected();
                        }
                        if (GUILayout.Button(
                                             globalTriggerVisibilityAll
                                                 ? triggerVisabilityOnLabel
                                                 : triggerVisabilityOffLabel,
                                             GUILayout.Width(57), GUILayout.Height(57)))
                            ToggleTriggerVisibility(true);

                        var bts = Selection.gameObjects.ToList().Where(x => x.GetComponent<BlockoutTrigger>())
                                           .Select(s => s.GetComponent<BlockoutTrigger>()).ToList();

                        if (bts.Count == 0)
                        {
                            if (GUILayout.Button(
                                                 new GUIContent(toggleSelectedTriggersOff,
                                                                "Toggles the visibility of selected blockout triggers in game. (Still visible when the game is not running)"),
                                                 GUILayout.Width(57), GUILayout.Height(57)))
                                Debug.LogWarning("BLOCKOUT :: Please select some objects with a blockout trigger!");
                        }
                        else
                        {
                            if (GUILayout.Button(
                                                 bts[0].visibleInGame
                                                     ? selectedTriggerVisabilityOnLabel
                                                     : selectedTriggerVisabilityOffLabel,
                                                 GUILayout.Width(57), GUILayout.Height(57)))
                                ToggleTriggerVisibility(false, !bts[0].visibleInGame);
                        }

                        if (GUILayout.Button(globalParticlePlayAll ? particlePlayOnLabel : particlePlayOffLabel,
                                             GUILayout.Width(57), GUILayout.Height(57)))
                        {
                            var ps = FindObjectsOfType<BlockoutParticleHelper>();
                            if (ps.Length > 0)
                                Selection.objects = ps.Select(x => x.gameObject).ToArray();
                            ToggleParticleSystems(true);
                        }
                        if (GUILayout.Button(exportAllLabel,
                                             GUILayout.Width(57), GUILayout.Height(57)))
                        {
                            Selection.activeGameObject = root.gameObject;
                            EditorObjExporter.ExportWholeSelectionToSingle();
                        }
                        if (GUILayout.Button(exportSelectedLabel,
                                             GUILayout.Width(57), GUILayout.Height(57)))
                            EditorObjExporter.ExportWholeSelectionToSingle();
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        // Output the amount of gameobjects selected to the console
                        objectCountLabel.text = "Count Objects (" +
                                                (Selection.gameObjects.Length != 0 ? "Selected: " : "Global: ") +
                                                selectedObjectCount + ")";

                        if (GUILayout.Button(objectCountLabel, GUILayout.Width(365)))
                        {
                            selectedObjectCount = Selection.gameObjects.Length != 0
                                                      ? Selection.gameObjects.Length
                                                      : FindObjectsOfType<GameObject>().Length;
                            repaint = true;
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }

                    #endregion
                }
            }
            else
            {
                if (!noHierarchyPresent)
                    DrawCommentSection(true);
            }

            GUILayout.Space(15);
            GUILayout.EndScrollView();
        }

        /// <summary>
        /// Using the current selected asset, find it's prefab in thew project window
        /// </summary>
        public static void SelectAsset()
        {
            var go = Selection.activeGameObject;
            var regex = "(\\[.*\\])|(\".*\")|('.*')|(\\(.*\\))";
            var output = Regex.Replace(go.name, regex, "");
            var res = AssetDatabase.FindAssets(output + " t:prefab");
            if (res.Length > 0)
            {
                var asset = (GameObject) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(res[0]),
                                                                       typeof(GameObject));
                if (PrefabUtility.GetPrefabParent(asset) == null && PrefabUtility.GetPrefabObject(asset) != null)
                    EditorGUIUtility.PingObject(asset);
            }
        }

        #region Variables

        public static bool isVisible = false;
        private List<BlockoutSceneSettings> sceneSettings = new List<BlockoutSceneSettings>();
        private int currentSetting = -1;

        public BlockoutSceneSettings CurrentSceneSetting
        {
            get
            {
                if (currentSetting < 0)
                    TryLoadSceneDefinitions();
                return currentSetting >= 0 ? sceneSettings[currentSetting] : null;
            }
        }

        private Scene currentActiveScene, previousActiveScene;

        private Texture2D logo, icon;

        private Texture2D triHeader,
                          transformHeader,
                          snapHeader,
                          otherHeader,
                          heiracyHeader,
                          buttonBlocks,
                          buttonBlocksDynamic,
                          buttonFloors,
                          buttonFoliage,
                          buttonMirrorX,
                          buttonMirrorY,
                          buttonMirrorZ,
                          buttonParticles,
                          buttonWalls,
                          buttonRotationX90,
                          buttonRotationY90,
                          buttonRotationZ90,
                          buttonRotationX180,
                          buttonRotationY180,
                          buttonRotationZ180,
                          autoText,
                          commentsText,
                          areaCommentText,
                          pinCommentsText,
                          createTrigger,
                          cameraAnchor,
                          exportAll,
                          exportSelected,
                          toggleAllParticlesOff,
                          toggleAllParticlesOn,
                          toggleAllTriggersOff,
                          toggleAllTriggersOn,
                          toggleSelectedTriggersOff,
                          toggleSelectedTriggersOn,
                          trashcan,
                          gameVisOn,
                          gameVisOff,
                          createHierarchy,
                          screenshotWhite,
                          screenshotColor,
                          screenshotSubHeader,
                          blockoutAssetSubheader,
                          commentsHeader,
                          backIcon,
                          commentInfoHeader,
                          clickInScene,
                          radforgeLogo,
                          openDocumentation,
                          openTutorials,
                          allAssets,
                          //platformerExample,
                          fpsExample,
                          rollerballExample,
                          exampleScenes,
                          feedback;

        private List<Texture2D> gridTextures = new List<Texture2D>();
        private List<Texture2D> gridIcons = new List<Texture2D>();
        private List<Texture2D> themeIcons = new List<Texture2D>();
        private List<Texture2D> areaCommentTextures = new List<Texture2D>();
        private List<Texture2D> areaCommentTextureIcons = new List<Texture2D>();
        private List<Texture2D> pinIcons = new List<Texture2D>();
        public List<GameObject> pinObjects = new List<GameObject>();
        private List<ThemeDefinition> themes = new List<ThemeDefinition>();
        private ThemeDefinition userTheme;
        private Color[] userThemeColors = new Color[8];

        private readonly string[] colorStrings =
            {"Floor", "Wall", "Dynamic", "Trim", "Foliage", "Leaves", "Water", "Trigger"};

        private readonly bool[] lockColors = new bool[8];
        private int currentGirdTexture, currentMaterialTheme, currentCommentTexture;

        private Vector2 globalScrollPosition = Vector2.zero;

        private Transform root, floor, walls, trim, dynamic, foliage, triggers, particles, cameras, comments;
        private GUISkin logoSkin;

        private Vector3 prevPosition, prevScale;
        private Transform previousTransform;
        public bool doSnapPosition = true;
        private float snapValue = 1;

        private int selected = 1;
        private readonly string[] Options = {"Custom", "0.25", "0.5", "1.0"};
        private readonly float[] OptionValues = {0.0f, 0.25f, 0.5f, 1.0f};

        private string parentName = "";
        private string commentBoxName = "";
        private Camera sceneCamera;
        private GameObject currentObjectMain, previousObjectMain;

        private bool globalTriggerVisibilityAll, globalParticlePlayAll;

        private int selectedObjectCount, globalObejctCount;

        private bool userPalletFoldout;

        public bool showCommentsBox = false;

        private bool previousShowCommentsBox = true;
        private BlockoutCommentInGameGUI commentInGame;
        private bool showOtherSection = false;
        private bool showHeirachyToolsSection = false;
        private bool showCommentsSection = false;

        private bool showCameraAnchors = false;
        private bool previousShowCommentsSection = true;
        public Color initialCommentColor = new Color(1, 0, 0, 0.5f);
        public Color commentColor = new Color(1, 0, 0, 0.5f);

        private Color sceneCommentTextColor = Color.black;
        private Color previousSceneCommentTextColor = Color.black;

        private int currentTriggerSelectionCount, previousTriggerSelectionCount;
        private BlockoutTrigger targetInitTrigger, previousTargetInitTrigger;

        private GameObject targetHeirachyBaseObject;

        public int commentPinToPlace = -1;

        private static BlockoutBlockHelper assetHelper;
        private string anchorName = "";
        private bool noHierarchyPresent;

        private bool repaint;

        public enum PalletType
        {
            Preset,
            User
        }

        public PalletType currentPallet = PalletType.Preset;

        public Notepad selectedNote;

        private static bool DisplaySuggestedAssets;

        private int supersizeResolution = 1;
        private int screenshotWidth = 1920, screenshotHeight = 1080;
        private Material screenshotWhiteMaterial;

        private GUIContent introLabel;
        private GUIContent openDocumentationLabel;
        private GUIContent openTutorialsLabel;
        private GUIContent allAssetsButton;
        private GUIContent fpsExampleLabel;
        private GUIContent rollerballExampleLabel;
        //private GUIContent patformerLabel;
        private GUIContent createHierarchyLabel;
        private GUIContent selectAssetLabel;
        private GUIContent replaceAssetLabel;
        private GUIContent refreshHierarchyLabel;
        private GUIContent blockButtonLabel;
        private GUIContent wallButtonLabel;
        private GUIContent floorButtonLabel;
        private GUIContent foliageButtonLabel;
        private GUIContent dynamicButtonLabel;
        private GUIContent particlesButtonLabel;
        private GUIContent randomisePalletLabel;
        private GUIContent applyPalletLabel;
        private GUIContent editPalletLabel;
        private GUIContent lockColorLabel;
        private GUIContent scalableWallLabel;
        private GUIContent scalableFloorLabel;
        private GUIContent scalableTrimLabel;
        private GUIContent centerGlobalPositionLabel;
        private GUIContent centerGlobalRotationLabel;
        private GUIContent centerLocalPositionLabel;
        private GUIContent centerLocalRotationLabel;
        private GUIContent rotateX90Label;
        private GUIContent rotateX180Label;
        private GUIContent rotateY90Label;
        private GUIContent rotateY180Label;
        private GUIContent rotateZ90Label;
        private GUIContent rotateZ180Label;
        private GUIContent mirrorXLabel;
        private GUIContent mirrorYLabel;
        private GUIContent mirrorZLabel;
        private GUIContent snapPXLabel;
        private GUIContent snapNXLabel;
        private GUIContent snapPYLabel;
        private GUIContent snapNYLabel;
        private GUIContent snapPZLabel;
        private GUIContent snapNZLabel;
        private GUIContent createParentLabel;
        private GUIContent hierarchySelectedLabel;
        private GUIContent reparentFloorLabel;
        private GUIContent reparentWallsLabel;
        private GUIContent reparentTrimLabel;
        private GUIContent reparentDynamicLabel;
        private GUIContent reparentFoliageLabel;
        private GUIContent reparentParticleLabel;
        private GUIContent createPrefabLabel;
        private GUIContent saveSceneCameraLabel;
        private GUIContent screenshotWhiteLabel;
        private GUIContent screenshotColorLabel;
        private GUIContent gameVisOnLabel;
        private GUIContent gameVisOffLabel;
        private GUIContent trashCanLabel;
        private GUIContent clearAllLabel;
        private GUIContent resolutionScaleLabel;
        private GUIContent widthScaleLabel;
        private GUIContent heightScaleLabel;
        private GUIContent createTriggerLabel; 
        private GUIContent triggerVisabilityOnLabel;
        private GUIContent triggerVisabilityOffLabel;
        private GUIContent selectedTriggerVisabilityOnLabel;
        private GUIContent selectedTriggerVisabilityOffLabel;
        private GUIContent particlePlayOnLabel;
        private GUIContent particlePlayOffLabel;
        private GUIContent exportAllLabel;
        private GUIContent exportSelectedLabel;
        private GUIContent objectCountLabel;
        private GUIContent repartToCommentAreaLabel;
        private GUIContent backButtonLabel;
        private GUIContent snapDistanceLabel;
        private GUIContent feedbackLabel;
        private GUIContent editGlobalNotesLabel;
        private GUIContent[] pinIconLabels;
        private GUIContent[] areaTextureLabels;
        private GUIContent[] gridIconLabels;
        private GUIContent[] themeLabels;

        private Color backgroundRed, backgroundGreen, backgroundBlue;

        private GameObject targetReplacementAsset;

        public Notepad GlobalNotes;

        #endregion


        #region Function Definitions

        /// <summary>
        ///     Applies the material theme color pallet and texture to all blockout objects.
        /// </summary>
        /// <param name="theme">Target Theme.</param>
        private void ApplyTheme(ThemeDefinition theme)
        {
            Undo.IncrementCurrentGroup();
            var GroupId = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Change Material Theame");

            Undo.RecordObject(CurrentSceneSetting, "");
            CurrentSceneSetting.currentTheme = currentPallet == PalletType.Preset ? currentMaterialTheme : -1;

            EditorUtility.SetDirty(CurrentSceneSetting);
            AssetDatabase.SaveAssets();

            var allSections = FindObjectsOfTypeInActiveScene<BlockoutSection>();

            var floorObjs = allSections.Where(x => x.Section == SectionID.Floors).Select(x => x.gameObject);
            foreach (var floor_ in floorObjs)
                ApplyNewMaterialScheme(theme.FloorMaterial, floor_);

            var wallObjs = allSections.Where(x => x.Section == SectionID.Walls).Select(x => x.gameObject);
            foreach (var wall_ in wallObjs)
                ApplyNewMaterialScheme(theme.WallMaterial, wall_);

            var dynObjs = allSections.Where(x => x.Section == SectionID.Dynamic).Select(x => x.gameObject);
            foreach (var dyn_ in dynObjs)
                ApplyNewMaterialScheme(theme.DynamicMaterial, dyn_);

            var foliageObjs = allSections.Where(x => x.Section == SectionID.Foliage).Select(x => x.gameObject);
            foreach (var foliage_ in foliageObjs)
                ApplyNewMaterialScheme(theme.FoliageMaterial, foliage_);

            var trimObjs = allSections.Where(x => x.Section == SectionID.Trim).Select(x => x.gameObject);
            foreach (var trim_ in trimObjs)
                ApplyNewMaterialScheme(theme.TrimMaterial, trim_);

            ApplyMaterialToGroup(theme.WaterMateral,
                                 FindObjectsOfTypeInActiveScene<Transform>()
                                     .Where(x => x.name.Contains("Water") && x.gameObject.activeInHierarchy)
                                     .Select(x => x.gameObject)
                                     .ToArray());
            ApplyMaterialToGroup(theme.LeavesMaterial,
                                 FindObjectsOfTypeInActiveScene<Transform>()
                                     .Where(x => x.name.Contains("Leaves") && x.gameObject.activeInHierarchy)
                                     .Select(x => x.gameObject)
                                     .ToArray());
            ApplyMaterialToGroup(theme.TriggerMaterial,
                                 FindObjectsOfTypeInActiveScene<BlockoutTrigger>().Select(x => x.gameObject).ToArray());

            var wallTargets = new List<GameObject>();
            var floorTargets = new List<GameObject>();
            var trimTargets = new List<GameObject>();

            var gameObjectsWithCorrentBlockoutSection = FindObjectsOfTypeInActiveScene<Transform>().ToList().Where(x =>
            {
                if (x.parent == null) return false;
                var bs = x.GetComponentInParent<BlockoutSection>();
                if (bs)
                    if (bs.Section == SectionID.Walls || bs.Section == SectionID.Floors || bs.Section == SectionID.Trim)
                        return true;
                return false;
            }).Select(x => x.gameObject).ToList();

            foreach (var go in gameObjectsWithCorrentBlockoutSection)
            {
                var targetSection = go.transform.GetComponentInParent<BlockoutSection>();
                if (targetSection)
                    if (go.name.Contains("(Tri-Planar)"))
                        if (targetSection.Section == SectionID.Walls)
                            wallTargets.Add(go);
                        else if (targetSection.Section == SectionID.Floors)
                            floorTargets.Add(go);
                        else if (targetSection.Section == SectionID.Trim)
                            trimTargets.Add(go);
            }

            ApplyMaterialToGroup(theme.TriWalls, wallTargets.ToArray());
            ApplyMaterialToGroup(theme.TriFloor, floorTargets.ToArray());
            ApplyMaterialToGroup(theme.TriTrim, trimTargets.ToArray());

            ApplyTextureIncChildren(gridTextures[currentGirdTexture]);

            Undo.CollapseUndoOperations(GroupId);
        }

        /// Snap an object to another object in a global direction
        /// <param name="direction"> The global vcttor to snap along</param>
        /// <param name="axis"></param>
        public void Snap(Vector3 direction, BlockoutAxis axis, bool forceGlobal = false)
        {
            // Get the selected gameobjects and raycast all of them in the snap direction
            // If it hits someting then set its new position to be the snap point offset
            // by how large the obects bounds is
            var sel = Selection.gameObjects.Select(x => x.transform).ToArray();
            Undo.RecordObjects(sel, "Snap Objects");
            for (var i = 0; i < sel.Length; ++i)
            {
                var childColliders = sel[i].GetComponentsInChildren<Collider>().ToList();
                var rend = sel[i].GetComponent<Renderer>();
                if (rend)
                {
                    var ray = new Ray(rend.bounds.center, direction);

                    var pivotOffset = sel[i].transform.position - rend.bounds.center;

                    var allHits = Physics.RaycastAll(ray, 500.0f, Physics.DefaultRaycastLayers);
                    if (allHits.Length > 0)
                        for (var h = 0; h < allHits.Length; ++h)
                        {
                            if (childColliders.Contains(allHits[h].collider))
                                continue;

                            var targetPoint = allHits[i].point;

                            sel[i].position = targetPoint + pivotOffset;
                            sel[i].position += -direction.normalized *
                                               (rend.bounds.size[(int) axis] / 2);
                            break;
                        }
                }
            }
            SceneView.lastActiveSceneView.FrameSelected();
        }

        /// <summary>
        ///     Rotates selected objects around the specified axis and by amount degrees.
        /// </summary>
        /// <param name="axis">Axis.</param>
        /// <param name="amount">Amount in degrees.</param>
        private void Rotate(Vector3 axis, float amount)
        {
            var sel = Selection.gameObjects;
            Undo.RecordObjects(sel.ToList().Select(x => x.transform).ToArray(), "Rotate objects");

            for (var i = 0; i < sel.Length; ++i)
            {
                var spaceMode = Tools.pivotRotation;

                // Unity is pivot rotation inverted?
                if (spaceMode == PivotRotation.Global)
                {
                    if (axis == Vector3.forward)
                        sel[i].transform.Rotate(sel[i].transform.forward * amount);
                    else if (axis == Vector3.right)
                        sel[i].transform.Rotate(sel[i].transform.right * amount);
                    else if (axis == Vector3.up)
                        sel[i].transform.Rotate(sel[i].transform.up * amount);
                }
                else
                {
                    sel[i].transform.Rotate(axis * amount);
                }
            }
        }

        /// <summary>
        ///     Mirror selected objects along the specified axis.
        /// </summary>
        /// <param name="axis">Axis to mirror on</param>
        private void Mirror(Vector3 axis)
        {
            var selelected = Selection.gameObjects;
            Undo.RecordObjects(selelected.ToList().Select(x => x.transform).ToArray(), "Mirror objects");
            foreach (var s in selelected)
                s.transform.localScale = new Vector3(s.transform.localScale.x * (Math.Abs(axis.x) < 0.01f ? 1 : axis.x),
                                                     s.transform.localScale.y * (Math.Abs(axis.y) < 0.01f ? 1 : axis.y),
                                                     s.transform.localScale.z *
                                                     (Math.Abs(axis.z) < 0.01f ? 1 : axis.z));
        }

        /// Load all the texture resources in the BlockoutTextures folder
        /// <param name="target">The target blockout window</param>
        private static void LoadTextureResources(BlockoutEditorWindow target)
        {
            target.gridTextures = new List<Texture2D>();
            target.gridIcons = new List<Texture2D>();
            target.areaCommentTextures = new List<Texture2D>();
            target.areaCommentTextureIcons = new List<Texture2D>();
            target.gridTextures = Resources.LoadAll("Blockout/BlockoutTextures", typeof(Texture2D)).Cast<Texture2D>()
                                           .ToArray()
                                           .ToList();
            target.areaCommentTextures = Resources.LoadAll("Blockout/Area Comment Textures", typeof(Texture2D))
                                                  .Cast<Texture2D>()
                                                  .ToArray().ToList();
            target.pinIcons = Resources.LoadAll("Blockout/Pins", typeof(Texture2D))
                                       .Where(x => EditorGUIUtility.isProSkin
                                                       ? x.name.Contains("Light")
                                                       : !x.name.Contains("Light"))
                                       .Cast<Texture2D>().ToArray().ToList().OrderBy(x => x.name).ToList();
            target.pinObjects = Resources.LoadAll("Blockout/Pins", typeof(GameObject)).Cast<GameObject>().ToArray()
                                         .ToList()
                                         .OrderBy(x => x.name).ToList();
            target.gridIcons = Resources.LoadAll("Blockout/UI_Icons", typeof(Texture2D)).Cast<Texture2D>().ToArray()
                                        .ToList()
                                        .Where(x => x.name.Contains("Icon_Blockout")).ToList();
            target.areaCommentTextureIcons = Resources.LoadAll("Blockout/UI_Icons", typeof(Texture2D)).Cast<Texture2D>()
                                                      .ToArray().ToList()
                                                      .Where(x => x.name.Contains("Blockout_Comment_Texture_Icon"))
                                                      .ToList();
            target.screenshotWhiteMaterial = Resources.Load<Material>("Blockout/Materials/ScreenshotWhite");

            target.logo =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Logo_Light"
                                   : "Blockout/UI_Icons/Blockout_Logo_Dark",
                               typeof(Texture2D)) as Texture2D;
            target.triHeader =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Scalable_Object_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Scalable_Object", typeof(Texture2D)) as Texture2D;
            target.transformHeader =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Transform_Controls_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Transform_Controls",
                               typeof(Texture2D)) as Texture2D;
            target.snapHeader =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Grid_Snapping_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Grid_Snapping", typeof(Texture2D)) as Texture2D;
            target.otherHeader =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Others_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Others", typeof(Texture2D)) as Texture2D;
            target.heiracyHeader =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Heirachy_Tools_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Heirachy_Tools", typeof(Texture2D)) as Texture2D;
            target.buttonBlocks =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Blocks_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Blocks", typeof(Texture2D)) as Texture2D;
            target.buttonBlocksDynamic =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Dynamic_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Dynamic", typeof(Texture2D)) as Texture2D;
            target.buttonFloors =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Floors_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Floors", typeof(Texture2D)) as Texture2D;
            target.buttonFoliage =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Foliage_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Foliage", typeof(Texture2D)) as Texture2D;
            target.buttonMirrorX =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Mirror_x_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Mirror_x", typeof(Texture2D)) as Texture2D;
            target.buttonMirrorY =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Mirror_y_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Mirror_y", typeof(Texture2D)) as Texture2D;
            target.buttonMirrorZ =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Mirror_z_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Mirror_z", typeof(Texture2D)) as Texture2D;
            target.buttonParticles =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Particles_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Particles",
                               typeof(Texture2D)) as Texture2D;
            target.buttonWalls =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Walls_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Walls", typeof(Texture2D)) as Texture2D;
            target.buttonRotationX90 =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_x90_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_x90", typeof(Texture2D)) as Texture2D;
            target.buttonRotationY90 =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_y90_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_y90", typeof(Texture2D)) as Texture2D;
            target.buttonRotationZ90 =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_z90_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_z90", typeof(Texture2D)) as Texture2D;
            target.buttonRotationX180 =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_x180_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_x180", typeof(Texture2D)) as Texture2D;
            target.buttonRotationY180 =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_y180_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_y180", typeof(Texture2D)) as Texture2D;
            target.buttonRotationZ180 =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_z180_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_z180", typeof(Texture2D)) as Texture2D;
            target.autoText =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Auto_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Auto", typeof(Texture2D)) as Texture2D;
            target.commentsText =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Comments_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Comments", typeof(Texture2D)) as Texture2D;
            target.areaCommentText =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Area_Comments_Light"
                                   : "Blockout/UI_Icons/Blockout_Area_Comments", typeof(Texture2D)) as Texture2D;
            target.pinCommentsText =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Pin_Comments_Light"
                                   : "Blockout/UI_Icons/Blockout_Pin_Comments", typeof(Texture2D)) as Texture2D;
            target.createTrigger =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Create_Trigger_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Create_Trigger",
                               typeof(Texture2D)) as Texture2D;
            target.exportAll =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Export_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Export", typeof(Texture2D)) as Texture2D;
            target.exportSelected =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Export_Selected_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Export_Selected",
                               typeof(Texture2D)) as Texture2D;
            target.toggleAllParticlesOff =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Toggle_All_Particles_Off_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Toggle_All_Particles_Off",
                               typeof(Texture2D)) as Texture2D;
            target.toggleAllParticlesOn =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Toggle_All_Particles_On_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Toggle_All_Particles_On",
                               typeof(Texture2D)) as Texture2D;
            target.toggleAllTriggersOff =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Toggle_All_Triggers_Off_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Toggle_All_Triggers_Off",
                               typeof(Texture2D)) as Texture2D;
            target.toggleAllTriggersOn =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Toggle_All_Triggers_On_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Toggle_All_Triggers_On",
                               typeof(Texture2D)) as Texture2D;
            target.toggleSelectedTriggersOff =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Toggle_Selected_Triggers_Off_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Toggle_Selected_Triggers_Off",
                               typeof(Texture2D)) as Texture2D;
            target.toggleSelectedTriggersOn =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Toggle_Selected_Triggers_On_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Toggle_Selected_Triggers_On",
                               typeof(Texture2D)) as Texture2D;
            target.cameraAnchor =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Camera_Anchors_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Camera_Anchors", typeof(Texture2D)) as Texture2D;
            target.trashcan =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Trash_Can_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Trash_Can", typeof(Texture2D)) as Texture2D;
            target.gameVisOff =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Game_Visibility_Light"
                                   : "Blockout/UI_Icons/Blockout_Game_Visibility", typeof(Texture2D)) as Texture2D;
            target.gameVisOn =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Game_Visibility_On_Light"
                                   : "Blockout/UI_Icons/Blockout_Game_Visibility_On", typeof(Texture2D)) as Texture2D;

            target.createHierarchy =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_AUTO_GENERATE_SCENE_HIERARCHY_Light"
                                   : "Blockout/UI_Icons/Blockout_AUTO_GENERATE_SCENE_HIERARCHY",
                               typeof(Texture2D)) as Texture2D;
            target.radforgeLogo =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/RadicalForgeLogoLongLight"
                                   : "Blockout/UI_Icons/RadicalForgeLogoLong", typeof(Texture2D)) as Texture2D;
            target.screenshotSubHeader =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Screenshot_Settings_Light"
                                   : "Blockout/UI_Icons/Screenshot_Settings", typeof(Texture2D)) as Texture2D;
            target.screenshotColor =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Screenshot_Coloured_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Screenshot_Coloured",
                               typeof(Texture2D)) as Texture2D;
            target.screenshotWhite =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Screenshot_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Screenshot",
                               typeof(Texture2D)) as Texture2D;
            target.blockoutAssetSubheader =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Assets_Light"
                                   : "Blockout/UI_Icons/Blockout_Assets", typeof(Texture2D)) as Texture2D;

            target.commentsHeader =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Comments_Light"
                                   : "Blockout/UI_Icons/Blockout_Comments", typeof(Texture2D)) as Texture2D;
            target.backIcon =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Icon_Button_Back_Light"
                                   : "Blockout/UI_Icons/Blockout_Icon_Button_Back", typeof(Texture2D)) as Texture2D;
            target.commentInfoHeader =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Comment_Info_Light"
                                   : "Blockout/UI_Icons/Comment_Info", typeof(Texture2D)) as Texture2D;
            target.clickInScene =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Click_In_The_Scene_Light"
                                   : "Blockout/UI_Icons/Click_In_The_Scene", typeof(Texture2D)) as Texture2D;
            target.allAssets =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/ALL_ASSETS_Light"
                                   : "Blockout/UI_Icons/ALL_ASSETS", typeof(Texture2D)) as Texture2D;

            target.openDocumentation =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Button_Documentation_Light"
                                   : "Blockout/UI_Icons/Blockout_Button_Documentation_Dark", typeof(Texture2D)) as Texture2D;

            target.openTutorials =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Button_Tutorials_Light"
                                   : "Blockout/UI_Icons/Blockout_Button_Tutorials_Dark", typeof(Texture2D)) as Texture2D;

            target.fpsExample =
                Resources.Load("Blockout/UI_Icons/FPS_Button_Long", typeof(Texture2D)) as Texture2D;

            target.rollerballExample =
                Resources.Load("Blockout/UI_Icons/Rollerball_Button_Long", typeof(Texture2D)) as Texture2D;

           // target.platformerExample =
           //     Resources.Load("Blockout/UI_Icons/25dplatformer", typeof(Texture2D)) as Texture2D;

            target.exampleScenes =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Blockout_Example Scenes_Light"
                                   : "Blockout/UI_Icons/Blockout_Example Scenes", typeof(Texture2D)) as Texture2D;

            target.feedback =
                Resources.Load(
                               EditorGUIUtility.isProSkin
                                   ? "Blockout/UI_Icons/Submit_Feedback_Bug_Light"
                                   : "Blockout/UI_Icons/Submit_Feedback_Bug", typeof(Texture2D)) as Texture2D;

            }

        /// <summary>
        ///     Loads GUI content resources.
        /// </summary>
        private void LoadGUIContentResources()
        {
            selectAssetLabel = new GUIContent("Select Asset",
                                              "Selects the object in the project window if it is a prefab");

            replaceAssetLabel = new GUIContent("Replace Asset",
                                               "Replaces all selected assets with the requested asset to the left mating the original transforms of the selection");

            openDocumentationLabel = new GUIContent(openDocumentation,
                                                    "Opens the online documentation in the default web browser");

            openTutorialsLabel = new GUIContent(openTutorials,
                                                    "Opens the online tutorials (YouTube playlist) in the default web browser");

            allAssetsButton = new GUIContent(allAssets, "Opens the all assets scene");

            fpsExampleLabel = new GUIContent(fpsExample, "Opens the FPS example scene");

            rollerballExampleLabel = new GUIContent(rollerballExample, "Opens the rollerball example scene");

            //patformerLabel = new GUIContent(platformerExample, "Opens the 2.5D Platformer example");

            introLabel = new GUIContent("First off, thank you for purchasing Blockout! While you probably" +
                                        Environment.NewLine +
                                        "should read at least the Quick Start section in the documentation, " +
                                        Environment.NewLine +
                                        "you probably want to dive right in and that’s fine (that’s exactly" +
                                        Environment.NewLine +
                                        "what we would do). There should be tooltips around each section " +
                                        Environment.NewLine +
                                        "of the Blockout window to help clarify its function." + Environment.NewLine +
                                        Environment.NewLine +
                                        "Note: this tool supports multi-scene editing. ");

            createHierarchyLabel = new GUIContent(createHierarchy,
                                                  "Creates a scene hierarchy for the blockout editor to use. ");

            refreshHierarchyLabel = new GUIContent("Refresh Hierarchy To Fix Missing Links");

            blockButtonLabel = new GUIContent(buttonBlocks,
                                              "Jumps to the blocks folder in the blockout project folder");

            wallButtonLabel = new GUIContent(buttonWalls,
                                             "Jumps to the walls folder in the blockout project folder");

            floorButtonLabel = new GUIContent(buttonFloors,
                                              "Jumps to the floors folder in the blockout project folder");

            dynamicButtonLabel = new GUIContent(buttonBlocksDynamic,
                                                "Jumps to the dynamic folder in the blockout project folder");

            foliageButtonLabel = new GUIContent(buttonFoliage,
                                                "Jumps to the foliage folder in the blockout project folder");

            particlesButtonLabel = new GUIContent(buttonParticles,
                                                  "Jumps to the particles folder in the blockout project folder");

            randomisePalletLabel = new GUIContent("Randomize Pallet",
                                                  "Generates and applies a random color pallet");

            applyPalletLabel = new GUIContent("Apply User Pallet",
                                              "Applies the last generated random color pallet as the theme");

            editPalletLabel = new GUIContent("Edit User Pallet",
                                             "Shows the individual colors for the user pallet");

            lockColorLabel = new GUIContent("",
                                            "Lock this color choice from random generation?");

            scalableWallLabel = new GUIContent("Create Scalable" + Environment.NewLine + "Wall Block",
                                               "Creats a scalable wall base");

            scalableFloorLabel = new GUIContent("Create Scalable" + Environment.NewLine + "Floor Block",
                                                "Creates a scaeable floor base");

            scalableTrimLabel = new GUIContent("Create Scalable" + Environment.NewLine + "Trim Block",
                                               "Creates a scalable trim base");

            centerGlobalPositionLabel = new GUIContent("Center Global Position",
                                                       "Center an object to (0,0,0) in WORLD space");

            centerLocalPositionLabel = new GUIContent("Center To Parent Position",
                                                      "Center an object to (0,0,0) in LOCAL space (parents pivot)");

            centerGlobalRotationLabel = new GUIContent("Reset Global Rotation",
                                                       "Reset the objects rotation to a quaternion identity (0,0,0,1) in WORLD SPACE");

            centerLocalRotationLabel = new GUIContent("Reset To Parent Rotation",
                                                      "Reset the objects rotation to a quaternion identity (0,0,0,1) in LOCAL SPACE (relative to parent)");

            rotateX90Label = new GUIContent(buttonRotationX90,
                                            "Rotate a object clockwise 90 degrees along the X asis");

            rotateX180Label = new GUIContent(buttonRotationX180,
                                             "Rotate a object clockwise 180 degrees along the X axis");

            rotateY90Label = new GUIContent(buttonRotationY90,
                                            "Rotate a object clockwise 90 degrees along the Y axis");

            rotateY180Label = new GUIContent(buttonRotationY180,
                                             "Rotate a object clockwise 180 degrees along the Y axis");

            rotateZ90Label = new GUIContent(buttonRotationZ90,
                                            "Rotate a object clockwise 90 degrees along the Z axis");

            rotateZ180Label = new GUIContent(buttonRotationZ180,
                                             "Rotate a object clockwise 180 degrees along the Z axis");

            mirrorXLabel = new GUIContent(buttonMirrorX,
                                          "Mirror in the X Axis");

            mirrorYLabel = new GUIContent(buttonMirrorY,
                                          "Mirror in the Y Axis");

            mirrorZLabel = new GUIContent(buttonMirrorZ,
                                          "Mirror in the Z Axis");

            snapPXLabel = new GUIContent("+ X",
                                         "Snap Object To First Collider Positive X (RED) Axis in WORLD space");

            snapNXLabel = new GUIContent("- X",
                                         "Snap Object To First Collider Negative X (RED) Axis in WORLD space");

            snapPYLabel = new GUIContent("+ Y",
                                         "Snap Object To First Collider Positive Y (GREEN) Axis in WORLD space");

            snapNYLabel = new GUIContent("- Y",
                                         "Snap Object To First Collider NEGATIVE Y (GREEN) Axis in WORLD space (Shortcut: End Key)");

            snapPZLabel = new GUIContent("+ Z",
                                         "Snap Object To First Collider Positive Z (BLUE) Axis in WORLD space");

            snapNZLabel = new GUIContent("- Z",
                                         "Snap Object To First Collider NEGATIVE Z (BLUE) Axis in WORLD space");

            createParentLabel = new GUIContent("Create Parent",
                                               "Creats a parent object with the selected children named the custom name specified to the left");

            hierarchySelectedLabel = new GUIContent("Hierarchy Selected",
                                                    "Makes the parent of selcted objects the target object to the left");

            reparentFloorLabel = new GUIContent("Floors",
                                                "Reparent selected gameobjects to the 'Floor' obejct in the blockout hierarchy");

            reparentWallsLabel = new GUIContent("Walls",
                                                "Reparent selected gameobjects to the 'Wall' obejct in the blockout hierarchy");

            reparentTrimLabel = new GUIContent("Trim",
                                               "Reparent selected gameobjects to the 'Trim' obejct in the blockout hierarchy");

            reparentDynamicLabel = new GUIContent("Dynamic",
                                                  "Reparent selected gameobjects to the 'Dynamic' obejct in the blockout hierarchy");

            reparentFoliageLabel = new GUIContent("Foliage",
                                                  "Reparent selected gameobjects to the 'Foliage' obejct in the blockout hierarchy");

            reparentParticleLabel = new GUIContent("Particle",
                                                   "Reparent selected gameobjects to the 'Particle' obejct in the blockout hierarchy");

            createPrefabLabel = new GUIContent("Create Prefab",
                                               "Creates a prefab for EACH SELECTED gameobject.");

            saveSceneCameraLabel = new GUIContent("Save Scene Camera Anchor",
                                                  "Save current scene view to a camera anchor");

            screenshotWhiteLabel = new GUIContent(screenshotWhite,
                                                  "Captures a screenshot with a white material setup using the current game anchor camera");

            screenshotColorLabel = new GUIContent(screenshotColor,
                                                  "Captures a screenshot of the current selected game anchor camera using the current theme");

            gameVisOnLabel = new GUIContent(gameVisOn,
                                            "Show camera in game view");

            gameVisOffLabel = new GUIContent(gameVisOff,
                                             "Show camera in game view");

            trashCanLabel = new GUIContent(trashcan,
                                           "Delete the current camera anchor");

            clearAllLabel = new GUIContent("Clear All",
                                           "Deletes all the camera canchors in a scene");

            resolutionScaleLabel = new GUIContent("Resolution Scale",
                                                  "Scales the internal resolution of the screenshot");

            widthScaleLabel = new GUIContent("Width", "The width of the screenshot");

            heightScaleLabel = new GUIContent("Height", "The height of the screenshot");

            createTriggerLabel = new GUIContent(createTrigger,
                                                "Creats a trigger volume with a blockout trigger script");

            triggerVisabilityOnLabel = new GUIContent(toggleAllTriggersOn,
                                                      "Toggles the visibility of all blockout triggers in game. (Still visible when the game is not running)");

            triggerVisabilityOffLabel = new GUIContent(toggleAllTriggersOff,
                                                       "Toggles the visibility of all blockout triggers in game. (Still visible when the game is not running)");

            selectedTriggerVisabilityOnLabel = new GUIContent(toggleSelectedTriggersOn,
                                                              "Toggles the visibility of selected blockout triggers in game. (Still visible when the game is not running)");

            selectedTriggerVisabilityOffLabel = new GUIContent(toggleSelectedTriggersOff,
                                                               "Toggles the visibility of selected blockout triggers in game. (Still visible when the game is not running)");

            particlePlayOnLabel = new GUIContent(toggleAllParticlesOn,
                                                 "Toggles the play state of all particle systems in the editor if a particle system in selected");

            particlePlayOffLabel = new GUIContent(toggleAllParticlesOff,
                                                  "Toggles the play state of all particle systems in the editor if a particle system in selected");

            exportAllLabel = new GUIContent(exportAll,
                                            "Exports the entire hierarchy as a single Wavefront Obj file");

            exportSelectedLabel = new GUIContent(exportSelected,
                                                 "Exports the selected items as a single Wavefront Obj file");

            objectCountLabel = new GUIContent("",
                                              "If there are objects selected thoes will be counted, otherwise the amount of gameobjects in the scene will be counted.");

            repartToCommentAreaLabel = new GUIContent("Reparent objects in comment area",
                                                      "Reparents any gameobject within the area of the comment");

            backButtonLabel = new GUIContent(backIcon,
                                             "Cancels placing a pin comment");

            snapDistanceLabel = new GUIContent("Snap Distance",
                                               "Ctrl + L - Toggles snapping (Both Position and Scale handled in 1) || Ctrl + , -Decreases snapping distance || Ctrl + . - Increases snapping distance");

            feedbackLabel = new GUIContent(feedback,
                                           "Opens your default email client to send us an email");

            editGlobalNotesLabel = new GUIContent("Edit Global Notes",
                                                  "Brings up the inspector to edit the scenes global notes file");

            pinIconLabels = new[]
            {
                new GUIContent(pinIcons[0], "Select pin to place"),
                new GUIContent(pinIcons[1], "Select pin to place"),
                new GUIContent(pinIcons[2], "Select pin to place"),
                new GUIContent(pinIcons[3], "Select pin to place"),
                new GUIContent(pinIcons[4], "Select pin to place"),
                new GUIContent(pinIcons[5], "Select pin to place")
            };

            areaTextureLabels = new[]
            {
                new GUIContent(areaCommentTextureIcons[0], string.Format("Select texture {0:D} to be applied", 1)),
                new GUIContent(areaCommentTextureIcons[1], string.Format("Select texture {0:D} to be applied", 2)),
                new GUIContent(areaCommentTextureIcons[2], string.Format("Select texture {0:D} to be applied", 3)),
                new GUIContent(areaCommentTextureIcons[3], string.Format("Select texture {0:D} to be applied", 4)),
                new GUIContent(areaCommentTextureIcons[4], string.Format("Select texture {0:D} to be applied", 5)),
                new GUIContent(areaCommentTextureIcons[5], string.Format("Select texture {0:D} to be applied", 6))
            };

            gridIconLabels = new[]
            {
                new GUIContent(gridIcons[0], string.Format("Select texture {0:D} to be applied", 1)),
                new GUIContent(gridIcons[1], string.Format("Select texture {0:D} to be applied", 2)),
                new GUIContent(gridIcons[2], string.Format("Select texture {0:D} to be applied", 3)),
                new GUIContent(gridIcons[3], string.Format("Select texture {0:D} to be applied", 4)),
                new GUIContent(gridIcons[4], string.Format("Select texture {0:D} to be applied", 5)),
                new GUIContent(gridIcons[5], string.Format("Select texture {0:D} to be applied", 6))
            };

            themeLabels = new[]
            {
                new GUIContent(themeIcons[0],
                               string.Format("Select theme {0:D} to be applied to the blockout hierarchy", 1)),
                new GUIContent(themeIcons[1],
                               string.Format("Select theme {0:D} to be applied to the blockout hierarchy", 2)),
                new GUIContent(themeIcons[2],
                               string.Format("Select theme {0:D} to be applied to the blockout hierarchy", 3)),
                new GUIContent(themeIcons[3],
                               string.Format("Select theme {0:D} to be applied to the blockout hierarchy", 4)),
                new GUIContent(themeIcons[4],
                               string.Format("Select theme {0:D} to be applied to the blockout hierarchy", 5)),
                new GUIContent(themeIcons[5],
                               string.Format("Select theme {0:D} to be applied to the blockout hierarchy", 6))
            };

            backgroundRed = new Color(1.0f, 0.467f, 0.465f);
            backgroundGreen = new Color(0.467f, 1.0f, 0.514f);
            backgroundBlue = new Color(0.467f, 0.67f, 1.0f);
        }

        /// Load all the theme resources in any folder belonging to a resource folder
        /// <param name="target">The target blockout window</param>
        private static void LoadThemes(BlockoutEditorWindow target)
        {
            if (target.themes == null)
                target.themes = new List<ThemeDefinition>();

            if (target.themes.Count == 0)
                target.themes = Resources.LoadAll("Blockout/BlockoutMaterials", typeof(ThemeDefinition))
                                         .Cast<ThemeDefinition>()
                                         .ToArray().ToList();

            target.userTheme = Resources.Load("Blockout/BlockoutMaterials/Blockout_Theme_User") as ThemeDefinition;
            target.themes.Remove(target.userTheme);
            target.themes.TrimExcess();


            target.themeIcons = new List<Texture2D>();
            target.themeIcons = Resources.LoadAll("Blockout/UI_Icons", typeof(Texture2D)).Cast<Texture2D>().ToArray()
                                         .ToList()
                                         .Where(x => x.name.Contains("Icon_Theme")).ToList();

            if (target.userThemeColors == null)
                target.userThemeColors = new Color[8];

            if (target.userThemeColors.Length < 8)
                target.userThemeColors = new Color[8];

            target.userThemeColors[0] = target.userTheme.FloorMaterial.GetColor("_Color");
            target.userThemeColors[1] = target.userTheme.WallMaterial.GetColor("_Color");
            target.userThemeColors[2] = target.userTheme.DynamicMaterial.GetColor("_Color");
            target.userThemeColors[3] = target.userTheme.TrimMaterial.GetColor("_Color");
            target.userThemeColors[4] = target.userTheme.FoliageMaterial.GetColor("_Color");
            target.userThemeColors[5] = target.userTheme.LeavesMaterial.GetColor("_Color_1");
            target.userThemeColors[6] = target.userTheme.WaterMateral.GetColor("_Color_1");
            target.userThemeColors[7] = target.userTheme.TriggerMaterial.GetColor("_Color_1");
        }

        /// Recursively apply a texture to the current gameobject and its children
        /// <param name="current"> The current gameobject to change</param>
        /// <param name="texture"> The texture to apply to the renderer</param>
        private void ApplyTextureIncChildren(Texture texture)
        {
            Undo.IncrementCurrentGroup();
            var GroupId = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Assigning Textures");

            Undo.RecordObject(CurrentSceneSetting, "");
            CurrentSceneSetting.currentTexture = currentGirdTexture;

            EditorUtility.SetDirty(CurrentSceneSetting);
            AssetDatabase.SaveAssets();

            if (!root)
                FindHeirachy();

            var targetRenderers = root.GetComponentsInChildren<Renderer>();
            if (targetRenderers.Length > 0)
            {
                var materialsToEdit = new List<Material>();
                foreach (var r in targetRenderers)
                    foreach (var m in r.sharedMaterials)
                    {
                        if (!m) continue;
                        if (m.HasProperty("_Texture"))
                            materialsToEdit.Add(m);
                    }

                Undo.RecordObjects(materialsToEdit.ToArray(), "Assigning Textures");

                for (var i = 0; i < materialsToEdit.Count; ++i)
                    if (materialsToEdit[i])
                        materialsToEdit[i].SetTexture("_Texture", texture);
            }

            Undo.CollapseUndoOperations(GroupId);
        }

        /// Recursively apply a material to the current gameobject and its children
        /// <param name="current"> The current gameobject to change</param>
        /// <param name="targetMaterial"> The material to apply to the renderer</param>
        private void ApplyNewMaterialScheme(Material targetMaterial, GameObject current)
        {
            var renderers = current.GetComponentsInChildren<Renderer>();


            if (renderers != null)
            {
                Undo.IncrementCurrentGroup();
                Undo.RegisterCompleteObjectUndo(renderers, "Assigning Material Scheme");

                foreach (var r in renderers)
                    r.sharedMaterial = targetMaterial;
            }
        }

        /// <summary>
        ///     Applies the new material scheme without undo.
        /// </summary>
        /// <param name="targetMaterial">The target material.</param>
        /// <param name="current">The current gameobject.</param>
        private void ApplyNewMaterialSchemeWithoutUndo(Material targetMaterial, GameObject current)
        {
            var renderers = current.GetComponentsInChildren<Renderer>();


            if (renderers != null)
                foreach (var r in renderers)
                {
                    if (r.gameObject.name.Contains("Comment") || r.sharedMaterial.name.Contains("Water") ||
                        r.gameObject.name.Contains("Water"))
                        continue;
                    if (r.GetComponent<Notepad>())
                        continue;
                    r.sharedMaterial = targetMaterial;
                }
        }

        /// Apply a material to the given group of gameobjects
        /// <param name="targetMaterial"> The target material</param>
        /// <param name="group"> The group of objects to chnage the material of</param>
        private void ApplyMaterialToGroup(Material targetMaterial, GameObject[] group)
        {
            var targetRenderers = new List<Renderer>();
            group.ToList().ForEach(x => { targetRenderers.AddRange(x.GetComponentsInChildren<Renderer>()); });

            if (targetRenderers.Count > 0)
            {
                Undo.IncrementCurrentGroup();
                Undo.RegisterCompleteObjectUndo(targetRenderers.ToArray(), "Assigning Material Scheme");

                foreach (var r in targetRenderers)
                    r.sharedMaterial = targetMaterial;
            }
        }

        /// Apply a new parent transform to the target gameobjects
        /// <param name="targets"> The gameobjects to reparent</param>
        /// <param name="newParent"> The new transform that will become their parent</param>
        private void ApplyParentTransfrom(GameObject[] targets, Transform newParent)
        {
            for (var i = 0; i < targets.Length; ++i)
                targets[i].transform.SetParent(newParent);
            ApplyCurrentTheme();
        }

        /// Apply a new parent transform to the target gameobjects
        /// <param name="targets"> The gameobjects to reparent</param>
        /// <param name="blockoutSection"> The new section that it needs to find in the level above to parent to</param>
        private void ApplyParentTransformFromSection(GameObject[] targets, SectionID blockoutSection)
        {
            for (var i = 0; i < targets.Length; ++i)
            {
                var vl = targets[i].GetComponentsInParent<BlockoutSection>().Where(x => x.Section == SectionID.Root)
                                   .ToList();

                var localRoot = vl.Count > 0 ? vl[0].gameObject : root.transform.gameObject;
                CreateBlockoutSubHeirachyWithRoot(localRoot.transform, localRoot.name + "_");

                var targetTransform = localRoot.GetComponentsInChildren<BlockoutSection>()
                                               .Where(x => x.Section == blockoutSection)
                                               .ToList()[0].transform;

                targets[i].transform.SetParent(targetTransform);

                TrimTargetBlockoutHierarchy(localRoot);
            }
            ApplyCurrentTheme();
        }

        /// <summary>
        ///     Toggles the trigger visibility for both the selected assets and globaly.
        /// </summary>
        /// <param name="global">if set to <c>true</c> [global].</param>
        private void ToggleTriggerVisibility(bool global = false, bool value = false)
        {
            var targets = Selection.gameObjects.Where(x => x.GetComponent<BlockoutTrigger>() != null)
                                   .Select(x => x.GetComponent<BlockoutTrigger>()).ToArray();

            if (global)
            {
                targets = FindObjectsOfType<BlockoutTrigger>().ToArray();
                globalTriggerVisibilityAll = !globalTriggerVisibilityAll;
            }

            if (targets.Length > 0)
            {
                Undo.RecordObjects(targets, "Toggle Trigger Visibility");

                foreach (var target in targets)
                {
                    var trigger = target.GetComponent<BlockoutTrigger>();
                    trigger.visibleInGame = global ? globalTriggerVisibilityAll : value;
                }
            }
        }

        /// <summary>
        ///     Toggles the particle systems.
        /// </summary>
        /// <param name="global">if set to <c>true</c> [global].</param>
        private void ToggleParticleSystems(bool global = false)
        {
            var targets = Selection.gameObjects;

            if (global)
            {
                targets = FindObjectsOfType<BlockoutParticleHelper>().Select(x => x.gameObject).ToArray();
                globalParticlePlayAll = !globalParticlePlayAll;
                Selection.objects = targets;
            }

            foreach (var target in targets)
            {
                var helper = target.GetComponent<BlockoutParticleHelper>();
                if (helper)
                    helper.ShouldPlay = global ? globalParticlePlayAll : !helper.ShouldPlay;
            }
        }

        /// <summary>
        ///     Generates a random theme.
        /// </summary>
        private void GenerateRandomTheme()
        {
            var baseTheme = UnityEngine.Random.Range(0, themes.Count);
            var mats = themes[baseTheme].GetSortedUniqueMaterials;
            var randomMat = UnityEngine.Random.Range(0, mats.Length);
            var baseThemeColor = mats[randomMat].GetColor("_Color");
            if (!lockColors[2])
                userTheme.DynamicMaterial.SetColor("_Color", baseThemeColor);

            float h, s, v;
            Color.RGBToHSV(baseThemeColor, out h, out s, out v);
            var pallet = GenerateColors_SaturationLuminance(mats.Length, h);
            var sortedPallet = pallet.OrderBy(x => Luminance(x)).ToList();

            for (var i = 0; i < sortedPallet.Count; ++i)
            {
                Color.RGBToHSV(sortedPallet[i], out h, out s, out v);
                s *= 0.5f;
                sortedPallet[i] = Color.HSVToRGB(h, s, v);
            }

            if (!lockColors[0])
            {
                userTheme.FloorMaterial.SetColor("_Color", sortedPallet[0]);
                userTheme.TriFloor.SetColor("_Color", sortedPallet[0]);
            }
            if (!lockColors[1])
            {
                userTheme.WallMaterial.SetColor("_Color", sortedPallet[1]);
                userTheme.TriWalls.SetColor("_Color", sortedPallet[1]);
            }
            if (!lockColors[4])
            {
                userTheme.TrimMaterial.SetColor("_Color", sortedPallet[2]);
                userTheme.TriTrim.SetColor("_Color", sortedPallet[2]);
            }
            if (!lockColors[5])
                userTheme.FoliageMaterial.SetColor("_Color", sortedPallet[3]);

            if (!lockColors[7])
            {
                userTheme.TriggerMaterial.SetColor("_Color", sortedPallet[0]);
                userTheme.TriggerMaterial.SetColor("_Color_1", sortedPallet[0]);
            }

            if (!lockColors[5])
            {
                Color.RGBToHSV(sortedPallet[1], out h, out s, out v);
                var leafMatOptions = GenerateColors_Saturation(1, h, v);
                userTheme.LeavesMaterial.SetColor("_Color", leafMatOptions[0] * 0.5f);
                userTheme.LeavesMaterial.SetColor("_Color_1", leafMatOptions[0] * 0.5f);
            }


            userThemeColors[0] = userTheme.FloorMaterial.GetColor("_Color");
            userThemeColors[1] = userTheme.WallMaterial.GetColor("_Color");
            userThemeColors[2] = userTheme.DynamicMaterial.GetColor("_Color");
            userThemeColors[3] = userTheme.TrimMaterial.GetColor("_Color");
            userThemeColors[4] = userTheme.FoliageMaterial.GetColor("_Color");
            userThemeColors[5] = userTheme.LeavesMaterial.GetColor("_Color_1");
            userThemeColors[6] = userTheme.WaterMateral.GetColor("_Color_1");
            userThemeColors[7] = userTheme.TriggerMaterial.GetColor("_Color_1");
        }

        /// <summary>
        ///     Luminance the specified color.
        /// </summary>
        /// <param name="col">Color.</param>
        private float Luminance(Color col)
        {
            float h, s, l;
            Color.RGBToHSV(col, out h, out s, out l);
            return l;
        }

        private static readonly Random random = new Random((int) DateTime.Now.Ticks);

        /// <summary>
        ///     Generates random color saturations and luminances based on hue.
        /// </summary>
        /// <returns>List of colors with varying saturations and luminances.</returns>
        /// <param name="colorCount">Color count.</param>
        /// <param name="hue">Base Hue.</param>
        private static List<Color> GenerateColors_SaturationLuminance(int colorCount, float hue)
        {
            var colors = new List<Color>();

            for (var i = 0; i < colorCount; i++)
            {
                var hslColor = new Color(hue, (float) random.NextDouble(), (float) random.NextDouble());

                colors.Add(hslColor);
            }

            return colors;
        }

        /// <summary>
        ///     Generates random color saturations based on hue and luminance.
        /// </summary>
        /// <returns>List of colors with varying saturations.</returns>
        /// <param name="colorCount">Color count.</param>
        /// <param name="hue">Hue.</param>
        /// <param name="luminance">Luminance.</param>
        private static List<Color> GenerateColors_Saturation(int colorCount, float hue, float luminance)
        {
            var colors = new List<Color>();

            for (var i = 0; i < colorCount; i++)
            {
                var hslColor = new Color(hue, (float) random.NextDouble(), luminance);

                colors.Add(hslColor);
            }

            return colors;
        }

        /// <summary>
        ///     Creates the parent and force child.
        /// </summary>
        private void CreateParentAndForceChild()
        {
            if (Selection.gameObjects.Length <= 0)
            {
                EditorUtility.DisplayDialog("Blockout Error",
                                            "Unable to create parent because no gameobjects selected", "Ok");
                return;
            }
            if (parentName == "")
            {
                EditorUtility.DisplayDialog("Blockout Error",
                                            "Unable to create parent as object requires a name. Please provide a name!",
                                            "Ok");
                return;
            }
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Create Parent And Force Children");
            var groupIndex = Undo.GetCurrentGroup();

            var newParent = new GameObject(parentName);
            Undo.RegisterCreatedObjectUndo(newParent, "Created Parent");

            CreateBlockoutSubHeirachyWithRoot(newParent.transform, parentName + "_");

            var parentBounds = new Bounds(Selection.gameObjects[0].transform.position, Vector3.one);
            var renderers = new List<Renderer>();
            Selection.gameObjects.ToList().ForEach(x => renderers.AddRange(x.GetComponentsInChildren<Renderer>()));
            renderers.ForEach(x => parentBounds.Encapsulate(x.bounds));

            newParent.transform.SetParent(root);
            newParent.transform.position = parentBounds.center;
            Selection.gameObjects.ToList()
                     .ForEach(x => ReparentObjectToTargetRoot(x.transform, newParent.transform));

            TrimTargetBlockoutHierarchy(newParent);

            Selection.activeGameObject = newParent;
            EditorGUIUtility.PingObject(newParent);

            Undo.CollapseUndoOperations(groupIndex);
        }

        /// <summary>
        ///     Creates a tri planer asset in front of the camera.
        /// </summary>
        /// <param name="targetAssetGuid">The target asset unique identifier.</param>
        private void CreateTriPlanerAsset(string targetAssetGuid)
        {
            var asset = (GameObject) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(targetAssetGuid),
                                                                   typeof(GameObject));

            var spawnPos = SceneView.lastActiveSceneView.camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 5f));
            var target = Instantiate(asset);
            Undo.RegisterCreatedObjectUndo(target, "Created Tri-Planer Asset");
            target.transform.position = spawnPos;
            target.name = asset.name;
            target.name += " (Tri-Planar)";
            Selection.activeGameObject = target;
            SnapUpdate();
            SceneView.lastActiveSceneView.FrameSelected();

            Tools.current = Tool.Scale;
            ApplyCurrentTheme();
        }

        /// <summary>
        ///     Applies the current material theme to the world
        /// </summary>
        public void ApplyCurrentTheme()
        {
            if (currentPallet == PalletType.Preset)
            {
                if (currentMaterialTheme < 0 || currentMaterialTheme >= themes.Count)
                    currentMaterialTheme = 0;
                ApplyTheme(themes[currentMaterialTheme]);
            }
            else
            {
                ApplyTheme(userTheme);
            }
        }

        /// <summary>
        ///     Increases the snap value counter
        /// </summary>
        public void IncreaseSnapValue()
        {
            if (selected < OptionValues.Length - 1)
                selected++;
            snapValue = OptionValues[selected];
        }

        /// <summary>
        ///     Decreases the snap value counter
        /// </summary>
        public void DecreaseSnapValue()
        {
            if (selected > 0)
                selected--;
            snapValue = OptionValues[selected];
        }

        /// <summary>
        ///     Creates the blockout sub hierarchy with root.
        /// </summary>
        /// <param name="targetRoot">The target root object of the sub hierarchy.</param>
        private void CreateBlockoutSubHeirachyWithRoot(Transform targetRoot, string namePrefix = "")
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Create Blockout SubHeirachy");
            var groupIndex = Undo.GetCurrentGroup();

            GameObject internalGO;
            BlockoutSection section;
            if (!targetRoot.GetComponent<BlockoutSection>())
                targetRoot.gameObject.AddComponent<BlockoutSection>().Section = SectionID.Root;
            if (!targetRoot.Find(namePrefix + "Floors"))
            {
                internalGO = new GameObject(namePrefix + "Floors");
                Undo.RegisterCreatedObjectUndo(internalGO.gameObject, "Created go");
                internalGO.GetComponent<Transform>().SetParent(targetRoot);
                section = internalGO.gameObject.AddComponent<BlockoutSection>();
                section.Section = SectionID.Floors;
            }
            if (!targetRoot.Find(namePrefix + "Walls"))
            {
                internalGO = new GameObject(namePrefix + "Walls");
                Undo.RegisterCreatedObjectUndo(internalGO.gameObject, "Created go");
                internalGO.GetComponent<Transform>().SetParent(targetRoot);
                section = internalGO.gameObject.AddComponent<BlockoutSection>();
                section.Section = SectionID.Walls;
            }
            if (!targetRoot.Find(namePrefix + "Trim"))
            {
                internalGO = new GameObject(namePrefix + "Trim");
                Undo.RegisterCreatedObjectUndo(internalGO.gameObject, "Created go");
                internalGO.GetComponent<Transform>().SetParent(targetRoot);
                section = internalGO.gameObject.AddComponent<BlockoutSection>();
                section.Section = SectionID.Trim;
            }
            if (!targetRoot.Find(namePrefix + "Dynamic"))
            {
                internalGO = new GameObject(namePrefix + "Dynamic");
                Undo.RegisterCreatedObjectUndo(internalGO.gameObject, "Created go");
                internalGO.GetComponent<Transform>().SetParent(targetRoot);
                section = internalGO.gameObject.AddComponent<BlockoutSection>();
                section.Section = SectionID.Dynamic;
            }
            if (!targetRoot.Find(namePrefix + "Foliage"))
            {
                internalGO = new GameObject(namePrefix + "Foliage");
                Undo.RegisterCreatedObjectUndo(internalGO.gameObject, "Created go");
                internalGO.GetComponent<Transform>().SetParent(targetRoot);
                section = internalGO.gameObject.AddComponent<BlockoutSection>();
                section.Section = SectionID.Foliage;
            }
            if (!targetRoot.Find(namePrefix + "Triggers"))
            {
                internalGO = new GameObject(namePrefix + "Triggers");
                Undo.RegisterCreatedObjectUndo(internalGO.gameObject, "Created go");
                internalGO.GetComponent<Transform>().SetParent(targetRoot);
                section = internalGO.gameObject.AddComponent<BlockoutSection>();
                section.Section = SectionID.Triggers;
            }
            if (!targetRoot.Find(namePrefix + "Particles"))
            {
                internalGO = new GameObject(namePrefix + "Particles");
                Undo.RegisterCreatedObjectUndo(internalGO.gameObject, "Created go");
                internalGO.GetComponent<Transform>().SetParent(targetRoot);
                section = internalGO.gameObject.AddComponent<BlockoutSection>();
                section.Section = SectionID.Particles;
            }
            if (!targetRoot.Find(namePrefix + "Cameras"))
            {
                internalGO = new GameObject(namePrefix + "Cameras");
                Undo.RegisterCreatedObjectUndo(internalGO.gameObject, "Created go");
                internalGO.GetComponent<Transform>().SetParent(targetRoot);
                section = internalGO.gameObject.AddComponent<BlockoutSection>();
                section.Section = SectionID.Cameras;
            }
            Undo.CollapseUndoOperations(groupIndex);
        }

        /// <summary>
        ///     Reparents the object to target root in blockout helper.
        /// </summary>
        /// <param name="target">The target objecty.</param>
        /// <param name="root">The root object.</param>
        private void ReparentObjectToTargetRoot(Transform target, Transform root)
        {
			Transform[] parents;
			if(target.GetComponent<BlockoutHelper>())
			{
            	parents = root.GetComponentsInChildren<BlockoutSection>()
                              .Where(x => x.transform.parent == root)
							  .Where(x => x.Section == target.GetComponentInParent<BlockoutHelper>().initialBlockoutSection)
                              .Select(x => x.transform).ToArray();
			}
			else
			{
				
				parents = root.GetComponentsInChildren<BlockoutSection>()
					.Where(x => x.Section == target.GetComponentInParent<BlockoutSection>().Section)
					.Select(x => x.transform).ToArray();
			}
            if (parents.Length > 0)
            {
                var targetParent = parents.First();
                if (targetParent)
                    Undo.SetTransformParent(target, targetParent, "");
            }
        }

        /// <summary>
        ///     Reparents and gameobject within the bounds of target object.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="target">The target.</param>
        private void ReparentToBoundsContent(GameObject targetObject, Bounds target)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Reparent Objects");
            var groupID = Undo.GetCurrentGroup();

            var allObjects = FindObjectsOfType<GameObject>().Select(x => x.transform).Where(
                                                                                            x =>
                                                                                            {
                                                                                                if (x.GetComponent<
                                                                                                    BlockoutSection>())
                                                                                                    return false;
                                                                                                if (x.parent == null)
                                                                                                    return false;
                                                                                                if (x.parent ==
                                                                                                    targetObject)
                                                                                                    return false;
                                                                                                if (x
                                                                                                    .parent
                                                                                                    .GetComponent<
                                                                                                        BlockoutSection
                                                                                                    >()) return true;
                                                                                                return false;
                                                                                            }
                                                                                           ).ToArray();
            if (!targetObject.GetComponent<BlockoutSection>())
                targetObject.AddComponent<BlockoutSection>().Section = SectionID.Root;
            CreateBlockoutSubHeirachyWithRoot(targetObject.transform, targetObject.name + "_");

            Undo.RecordObjects(allObjects, "Reparent Objects");

            for (var i = 0; i < allObjects.Length; ++i)
            {
                Bounds colliderBounds;
                if (allObjects[i].GetComponent<Collider>())
                    colliderBounds = allObjects[i].GetComponent<Collider>().bounds;
                else if (allObjects[i].GetComponent<Renderer>())
                    colliderBounds = allObjects[i].GetComponent<Renderer>().bounds;
                else
                    continue;

                if (target.Contains(colliderBounds.max) && target.Contains(colliderBounds.min))
                {
                    var section = allObjects[i].transform.parent.GetComponent<BlockoutSection>();
                    if (section)
                        ReparentObjectToTargetRoot(allObjects[i].transform, targetObject.transform);
                }
            }

            TrimTargetBlockoutHierarchy(targetObject);

            Undo.CollapseUndoOperations(groupID);
        }

        /// <summary>
        /// Removes excess children of a Blockout hierarchy that are not used
        /// </summary>
        /// <param name="targetObject">The target Blockout hierarchy root</param>
        private void TrimTargetBlockoutHierarchy(GameObject targetObject)
        {
            if (targetObject.GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Floors)
                            .ToArray()[0].transform.childCount == 0)
                DestroyImmediate(targetObject.GetComponentsInChildren<BlockoutSection>()
                                             .Where(x => x.Section == SectionID.Floors).ToArray()[0].gameObject);
            if (targetObject.GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Walls)
                            .ToArray()[0].transform.childCount == 0)
                DestroyImmediate(targetObject.GetComponentsInChildren<BlockoutSection>()
                                             .Where(x => x.Section == SectionID.Walls).ToArray()[0].gameObject);
            if (targetObject.GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Trim)
                            .ToArray()[0].transform.childCount == 0)
                DestroyImmediate(targetObject.GetComponentsInChildren<BlockoutSection>()
                                             .Where(x => x.Section == SectionID.Trim).ToArray()[0].gameObject);
            if (targetObject.GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Dynamic)
                            .ToArray()[0].transform.childCount == 0)
                DestroyImmediate(targetObject.GetComponentsInChildren<BlockoutSection>()
                                             .Where(x => x.Section == SectionID.Dynamic).ToArray()[0].gameObject);
            if (targetObject.GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Foliage)
                            .ToArray()[0].transform.childCount == 0)
                DestroyImmediate(targetObject.GetComponentsInChildren<BlockoutSection>()
                                             .Where(x => x.Section == SectionID.Foliage).ToArray()[0].gameObject);
            if (targetObject.GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Particles)
                            .ToArray()[0].transform.childCount == 0)
                DestroyImmediate(targetObject.GetComponentsInChildren<BlockoutSection>()
                                             .Where(x => x.Section == SectionID.Particles).ToArray()[0].gameObject);
            if (targetObject.GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Triggers)
                            .ToArray()[0].transform.childCount == 0)
                DestroyImmediate(targetObject.GetComponentsInChildren<BlockoutSection>()
                                             .Where(x => x.Section == SectionID.Triggers).ToArray()[0].gameObject);
            if (targetObject.GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Cameras)
                            .ToArray()[0].transform.childCount == 0)
                DestroyImmediate(targetObject.GetComponentsInChildren<BlockoutSection>()
                                             .Where(x => x.Section == SectionID.Cameras).ToArray()[0].gameObject);
        }

        /// <summary>
        ///     Creates an area comment.
        /// </summary>
        private void CreateAreaComment(string commentTargetName = "")
        {
            if (Selection.gameObjects.Length <= 1)
            {
                EditorUtility.DisplayDialog("Blockout Error",
                                            "There should be at least 2 (2+) objects selected! The area comment will encompass these objects",
                                            "Ok");
            }
            else
            {
                var encapsulatingBounds = new Bounds(Selection.gameObjects[0].transform.position, Vector3.one);
                for (var i = 0; i < Selection.gameObjects.Length; ++i)
                {
                    var rend = Selection.gameObjects[i].GetComponent<Renderer>();
                    if (rend)
                        encapsulatingBounds.Encapsulate(rend.bounds);
                }

                var anchorCenter = encapsulatingBounds.center;
                var ext = new Vector3(-encapsulatingBounds.extents.x, encapsulatingBounds.extents.y,
                                      -encapsulatingBounds.extents.z);
                anchorCenter += ext - new Vector3(0.5f, -0.5f, 0.5f);
                var asset = (GameObject) AssetDatabase.LoadAssetAtPath(
                                                                       AssetDatabase.GUIDToAssetPath(AssetDatabase
                                                                                                         .FindAssets("Area_Comment t:prefab")
                                                                                                         [0]),
                                                                       typeof(GameObject));
                var newComment = Instantiate(asset, anchorCenter, Quaternion.identity);
                Undo.RegisterCreatedObjectUndo(newComment, "Create Area Comment");
                showCommentsBox = true;
                newComment.name = commentTargetName == "" ? "Area Comment" : commentTargetName;
                newComment.transform.localScale = encapsulatingBounds.size + Vector3.one;
                newComment.transform.SetParent(comments);
                newComment.GetComponent<BlockoutSceneViewCubeGizmo>().volumeColor = initialCommentColor;
                Selection.activeGameObject = newComment;
                SceneView.lastActiveSceneView.FrameSelected();
            }
        }

        /// <summary>
        ///     Sets the area comment texture.
        /// </summary>
        private void SetAreaCommentTexture()
        {
            var targetRenderers = Selection.gameObjects.Where(x => x.GetComponentsInChildren<Renderer>().Length > 0)
                                           .SelectMany(x => x.GetComponentsInChildren<Renderer>()).ToArray();

            if (targetRenderers.Length > 0)
            {
                var materialsToEdit = new List<Material>();
                foreach (var r in targetRenderers)
                    foreach (var m in r.sharedMaterials)
                    {
                        if (!m) continue;
                        if (m.HasProperty("_Tex"))
                            materialsToEdit.Add(m);
                    }

                Undo.RecordObjects(materialsToEdit.ToArray(), "Assign Area Comment Textures");

                for (var i = 0; i < materialsToEdit.Count; ++i)
                    if (materialsToEdit[i])
                        materialsToEdit[i].SetTexture("_Tex", areaCommentTextures[currentCommentTexture]);
            }
        }

        /// <summary>
        /// Draws the 2 versions of the comment window. With and without the ability
        /// to edit comment contents depending if a comment is selected
        /// </summary>
        /// <param name="alwaysOn">Force show comments section</param>
        private void DrawCommentSection(bool alwaysOn = false)
        {
            #region Comment Tools

            if (alwaysOn)
                showCommentsSection = true;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal(GUILayout.Width(375));
            if (!alwaysOn)
            {
                showCommentsSection = GUILayout.Toggle(showCommentsSection, "",
                                                       logoSkin.button, GUILayout.Height(30),
                                                       GUILayout.Width(30));

                GUILayout.Box(commentsText, logoSkin.GetStyle("Texture"), GUILayout.MaxWidth(256),
                              GUILayout.MaxHeight(30));
            }
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (showCommentsSection)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(360));
                CommentBoxSceneGUI.ShowCommentInfo = GUILayout.Toggle(CommentBoxSceneGUI.ShowCommentInfo, "",
                                                                      logoSkin.button,
                                                                      GUILayout.Height(EditorGUIUtility
                                                                                           .singleLineHeight),
                                                                      GUILayout.Width(EditorGUIUtility
                                                                                          .singleLineHeight));
                GUILayout.Label("Show Scene Information");
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(360));
                CommentBoxSceneGUI.showAreaComments = GUILayout.Toggle(CommentBoxSceneGUI.showAreaComments, "",
                                                                       logoSkin.button,
                                                                       GUILayout.Height(EditorGUIUtility
                                                                                            .singleLineHeight),
                                                                       GUILayout.Width(EditorGUIUtility
                                                                                           .singleLineHeight));
                GUILayout.Label("Show Area Comments");
                CommentBoxSceneGUI.showPinComments = GUILayout.Toggle(CommentBoxSceneGUI.showPinComments, "",
                                                                      logoSkin.button,
                                                                      GUILayout.Height(EditorGUIUtility
                                                                                           .singleLineHeight),
                                                                      GUILayout.Width(EditorGUIUtility
                                                                                          .singleLineHeight));
                GUILayout.Label("Show Pin Comments");
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (CommentBoxSceneGUI.showAreaComments != CommentBoxSceneGUI.showAreaCommentsInternal)
                    CommentBoxSceneGUI.Update();

                if (CommentBoxSceneGUI.showPinComments != CommentBoxSceneGUI.showPinCommentsInternal)
                    CommentBoxSceneGUI.Update();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(365));
                GUILayout.Label("Scene View Text Color:");
                if (!Application.isPlaying && Application.isEditor)
                    sceneCommentTextColor = EditorGUILayout.ColorField(sceneCommentTextColor, GUILayout.MaxWidth(175));
                else
                    GUILayout.Label("Feature Not Available during play mode");
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(365));
                if (GUILayout.Button(editGlobalNotesLabel))
                {
                    Selection.activeGameObject = root.gameObject;
                    var pt = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.InspectorWindow");
                    GetWindow(pt).Show();
                }
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            if (alwaysOn)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical(GUILayout.Width(365));

                GUILayout.Space(5);

                GUILayout.Box(commentInfoHeader, logoSkin.GetStyle("Texture"), GUILayout.MaxWidth(256),
                              GUILayout.MaxHeight(30));

                GUILayout.BeginHorizontal();
                GUILayout.Label("Note name:");
                selectedNote.gameObject.name = GUILayout.TextField(selectedNote.gameObject.name);
                GUILayout.EndHorizontal();

                // -------------------------------------------------------------

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(365));
                GUILayout.Label("Comment Color:");


                if (Selection.activeGameObject)
                {
                    if (Selection.activeGameObject.GetComponent<BlockoutPinGizmo>())
                        commentColor = Selection.activeGameObject.GetComponent<BlockoutPinGizmo>()
                                                .volumeColor;
                    else if (Selection.activeGameObject.GetComponent<BlockoutSceneViewCubeGizmo>())
                        commentColor = Selection.activeGameObject.GetComponent<BlockoutSceneViewCubeGizmo>()
                                                .volumeColor;

                    if (!Application.isPlaying && Application.isEditor)
                        commentColor = EditorGUILayout.ColorField(commentColor, GUILayout.MaxWidth(175));
                    else
                        GUILayout.Label("Feature Not Available during play mode");

                    if (Selection.activeGameObject.GetComponent<BlockoutPinGizmo>())
                    {
                        Selection.activeGameObject.GetComponent<BlockoutPinGizmo>().volumeColor = commentColor;
                        if (SceneView.lastActiveSceneView)
                            SceneView.lastActiveSceneView.Repaint();
                        repaint = true;
                    }
                    else if (Selection.activeGameObject.GetComponent<BlockoutSceneViewCubeGizmo>())
                    {
                        Selection.gameObjects.Where(x => x.GetComponent<BlockoutSceneViewCubeGizmo>() != null)
                                 .Select(x => x.GetComponent<BlockoutSceneViewCubeGizmo>()).ToList()
                                 .ForEach(x => x.volumeColor = commentColor);
                        if (SceneView.lastActiveSceneView)
                            SceneView.lastActiveSceneView.Repaint();

                        repaint = true;
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                // -------------------------------------------------------------
                if (!Application.isPlaying && Application.isEditor)
                {
                    GUILayout.Label("General Notes");
                    selectedNote.generalNotes = EditorGUILayout.TextArea(selectedNote.generalNotes,
                                                                         GUILayout.MinHeight(EditorGUIUtility
                                                                                                 .singleLineHeight *
                                                                                             3));
                    GUILayout.Label("TODO Notes");
                    selectedNote.toDoNotes = EditorGUILayout.TextArea(selectedNote.toDoNotes,
                                                                      GUILayout.MinHeight(EditorGUIUtility
                                                                                              .singleLineHeight * 3));
                    GUILayout.Label("Other Notes");
                    selectedNote.otherNotes = EditorGUILayout.TextArea(selectedNote.otherNotes,
                                                                       GUILayout.MinHeight(EditorGUIUtility
                                                                                               .singleLineHeight * 3));
                }
                else
                {
                    GUILayout.Label("Feature Not Available during play mode");
                }
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            if (showCommentsSection != previousShowCommentsSection)
                previousShowCommentsSection = showCommentsSection;

            showCommentsBox = showCommentsSection;

            if (showCommentsSection)
            {
                #region Area Comments

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal(GUILayout.Width(390));
                GUILayout.Space(8);
                GUILayout.Box(areaCommentText, logoSkin.GetStyle("Texture"), GUILayout.Width(256),
                              GUILayout.Height(24));
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(360));
                if (commentInGame)
                    commentInGame.intialVisabilityStateArea = GUILayout.Toggle(commentInGame.intialVisabilityStateArea,
                                                                               "",
                                                                               logoSkin.button,
                                                                               GUILayout.Height(EditorGUIUtility
                                                                                                    .singleLineHeight),
                                                                               GUILayout.Width(EditorGUIUtility
                                                                                                   .singleLineHeight));
                GUILayout.Label("Initial Visibility State In Game");
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(365));
                GUILayout.Label("Name:");
                if (Selection.activeGameObject)
                    if (Selection.activeGameObject.GetComponent<BlockoutSceneViewCubeGizmo>())
                    {
                        commentBoxName = Selection.activeGameObject.name;
                        repaint = true;
                    }
                commentBoxName = GUILayout.TextField(commentBoxName);
                if (Selection.activeGameObject)
                    if (Selection.activeGameObject.GetComponent<BlockoutSceneViewCubeGizmo>())
                    {
                        Selection.activeGameObject.name = commentBoxName;
                        repaint = true;
                    }
                if (GUILayout.Button("Create Area Comment", GUILayout.Width(175)))
                    CreateAreaComment(commentBoxName);
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();
                // Loop through all the loaded grid textures and display them in a button.
                // If its selected then apply that texture to every gameobject in the scene
                for (var i = 0; i < areaCommentTextureIcons.Count; ++i)
                {
                    if (i >= areaCommentTextureIcons.Count)
                        continue;
                    if (GUILayout.Button(areaTextureLabels[i], GUILayout.Height(57),
                                         GUILayout.Width(57)))
                    {
                        currentCommentTexture = i;
                        SetAreaCommentTexture();
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(repartToCommentAreaLabel, GUILayout.Width(360)))
                    if (Selection.activeGameObject)
                        if (Selection.activeGameObject.GetComponent<BlockoutSceneViewCubeGizmo>())
                            ReparentToBoundsContent(Selection.activeGameObject,
                                                    Selection.activeGameObject.GetComponent<Collider>().bounds);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                #endregion

                #region Comment Pins

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal(GUILayout.Width(390));
                GUILayout.Space(8);
                GUILayout.Box(pinCommentsText, logoSkin.GetStyle("Texture"), GUILayout.Width(256),
                              GUILayout.Height(24));
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(360));
                if (commentInGame)
                    commentInGame.intialVisabilityStatePin = GUILayout.Toggle(commentInGame.intialVisabilityStatePin,
                                                                              "",
                                                                              logoSkin.button,
                                                                              GUILayout.Height(EditorGUIUtility
                                                                                                   .singleLineHeight),
                                                                              GUILayout.Width(EditorGUIUtility
                                                                                                  .singleLineHeight));
                GUILayout.Label("Initial Visibility State In Game");
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal(GUILayout.MaxHeight(60), GUILayout.MinHeight(60));
                GUILayout.FlexibleSpace();
                if (commentPinToPlace >= 0)
                {
                    if (GUILayout.Button(backButtonLabel,
                                         GUILayout.Height(57), GUILayout.Width(57)))
                        commentPinToPlace = -1;
                    GUILayout.Label(clickInScene, GUILayout.Height(57), GUILayout.Width(299));
                }
                else
                {
                    for (var i = 0; i < 6; ++i)
                    {
                        if (i >= pinIcons.Count)
                            continue;
                        if (GUILayout.Button(pinIconLabels[i],
                                             GUILayout.Height(57), GUILayout.Width(57)))
                            commentPinToPlace = i;
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                #endregion

                GUILayout.Space(5);
            }

            #endregion
        }

        #endregion
    }
}