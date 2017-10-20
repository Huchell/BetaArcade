/* Radical Forge Copyright (c) 2017 All Rights Reserved
   </copyright>
   <author>Frederic Babord</author>
   <date>16th July 2017</date>
   <summary>Block helper provides a list of favourite and sugessted 
            assets that can be dragged into the scene</summary>*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace RadicalForge.Blockout
{

	public struct BlockoutItemPreview
	{
		public string name;
		public GameObject prefab;
		public Texture2D previewImage;
	}

	public class BlockoutBlockHelper : EditorWindow {

		private static Texture2D suggestedAsstesTex, sceneFavouritesTex;
		private static GUISkin logoSkin;
		private static bool overSceneView;
		private BlockoutEditorWindow parentWindow;
		private GameObject spwanedAsset;
		int amountOfItemsToShow = 6;
		float currentSize = 60;
		const float bottomBarHieght = 17.0f;
		Rect bottomBarRect;
		bool dragging = false;
		string curractAssetName = "";
		BlockoutItemPreview[] suggestedItems, favouriteItems;
		BlockoutHelper previousHelper;

		public static bool visible = false;
	    private bool repaint = false;
	    private static int windowCount = 0;

		public static BlockoutBlockHelper Init()
		{
			// Get existing open window or if none, make a new one:
			var window = (BlockoutBlockHelper) GetWindow(typeof(BlockoutBlockHelper));
			window.maxSize = new Vector2(4000, 500);
			window.minSize = new Vector2(405, 100);
			window.Show();
			visible = true;
			return window;
		}

		public static void Hide()
		{
			var window = (BlockoutBlockHelper) GetWindow(typeof(BlockoutBlockHelper));
			if(visible)
				window.Close ();
			visible = false;
		}

        // Refresh the asset database and load in the texture resources required for the editor.
        // Close the window however if the main Blockout window is not open
		void OnEnable()
		{
			AssetDatabase.Refresh ();

			var icon = Resources.Load(
				EditorGUIUtility.isProSkin ? "Blockout/UI_Icons/Blockout_Icon_Light" : "Blockout/UI_Icons/Blockout_Icon_Dark",
				typeof(Texture2D)) as Texture2D;
			titleContent = new GUIContent("Block Helper", icon);

			suggestedAsstesTex =
				Resources.Load(
					EditorGUIUtility.isProSkin ? "Blockout/UI_Icons/Blockout_Suggested_Assets_Light" : "Blockout/UI_Icons/Blockout_Suggested_Assets",
					typeof(Texture2D)) as Texture2D;

			sceneFavouritesTex =
				Resources.Load(
					EditorGUIUtility.isProSkin ? "Blockout/UI_Icons/Blockout_Scene_Favourites_Light" : "Blockout/UI_Icons/Blockout_Scene_Favourites",
					typeof(Texture2D)) as Texture2D;

			logoSkin = (GUISkin)Resources.Load(EditorGUIUtility.isProSkin ? "Blockout/UI_Icons/BlockoutEditorSkinLight" : "Blockout/UI_Icons/BlockoutEditorSkin", typeof(GUISkin));

			SceneView.onSceneGUIDelegate += OnScene;
			bottomBarRect = position;
			bottomBarRect.yMin = bottomBarRect.yMax - bottomBarHieght;

			parentWindow = BlockoutEditorWindow.Instance;
		    windowCount++;

            if (!parentWindow) {
				Debug.LogError ("Blockout Window Required To Be Active!");
				this.Close ();
			}

            if(windowCount > 1)
                Close();
		}

        // Removed Scene udpate delegates as the window is no longer visible
		void OnDisable()
		{
			SceneView.onSceneGUIDelegate -= OnScene;
		    windowCount--;
        }

		void OnGUI()
		{
            // Work out the amount of items that can be dispolayed in the current window state
            amountOfItemsToShow = Mathf.FloorToInt((position.width - 55) / currentSize);

            // Draw the suggested assets group of objects
			GUILayout.BeginVertical ();
			GUILayout.Box(new GUIContent(suggestedAsstesTex), logoSkin.GetStyle("Texture"), GUILayout.Height(35));
			DrawAssetList (suggestedItems);
			GUILayout.Space (5);

            // Draw the scene favourite group of objects
			GUILayout.Box(new GUIContent(sceneFavouritesTex), logoSkin.GetStyle("Texture"), GUILayout.Height(35));
			if (favouriteItems != null) {
				if(favouriteItems.Length > 0)
					DrawAssetList (favouriteItems);
			}
			else
				GUILayout.Label ("There are no favourite assets!");
			GUILayout.FlexibleSpace ();
			GUILayout.EndVertical ();

            // Draw the footer of the window containing the obejct name and scale slider
			bottomBarRect = new Rect (-5, position.height - bottomBarHieght - 5, position.width + 10, bottomBarHieght + 10);
			GUILayout.BeginArea (bottomBarRect, EditorStyles.helpBox );
			GUILayout.BeginVertical ();
			GUILayout.FlexibleSpace ();
			GUILayout.BeginHorizontal ();
			GUILayout.Space (5);
            if(curractAssetName != "")
			    GUILayout.Box( EditorGUIUtility.FindTexture("PrefabNormal Icon"), logoSkin.GetStyle("Texture"), GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(EditorGUIUtility.singleLineHeight));
			GUILayout.Label (curractAssetName);
			GUILayout.FlexibleSpace ();
			currentSize = GUILayout.HorizontalSlider(currentSize, 20, 55, GUILayout.Width(75));
			GUILayout.Space (10);
			GUILayout.EndHorizontal ();
			GUILayout.FlexibleSpace ();
			GUILayout.EndVertical ();
			GUILayout.EndArea ();
		}

		void Update()
		{
            // If there is an obejcts selected, then find assets with a similar name to it in the asset database. 
            // Only is its selected in the scene and not in the project window
			if (Selection.gameObjects.Length > 0) {
				var selected = Selection.gameObjects.ToList ();
				foreach (var x in selected) {
					if (!AssetDatabase.Contains (x.gameObject)) {
						var helper = x.GetComponent<BlockoutHelper> ();
						if (helper) {
							if (previousHelper != helper) {
								previousHelper = helper;
								if (parentWindow.CurrentSceneSetting.assetDictionary == null)
									parentWindow.CurrentSceneSetting.assetDictionary = new List<AssetDefinition> ();

								var names = helper.gameObject.name.Split ('_').ToList ();
								string targetName = names [0];
								int amount = ((names.Count - 1) > 2 ? 2 : names.Count - 1);
								for (int i = 1; i < amount; ++i) {
									targetName += "_" + names [i];
								}


								var foundAssets = AssetDatabase.FindAssets (targetName + " t:prefab");

								bool cap = (foundAssets.Length > amountOfItemsToShow);

								GameObject[] loadedSuggestions =
									new GameObject[cap ? amountOfItemsToShow : foundAssets.Length];
								suggestedItems = new BlockoutItemPreview[cap ? amountOfItemsToShow : foundAssets.Length];
								for (int i = 0; i < (cap ? amountOfItemsToShow : foundAssets.Length); ++i) {
									var path = AssetDatabase.GUIDToAssetPath (foundAssets [i]);
									suggestedItems[i].prefab =
										(GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
									suggestedItems [i].name = suggestedItems [i].prefab.name;
									suggestedItems[i].previewImage = AssetPreview.GetAssetPreview(loadedSuggestions[i]);
								}
								break;
							}
						    repaint = true;
                            break;
						}
					}
				}
			}
            // If no assets are selected then default the suggested assets to a selection of Block prefabs
            else {
				var foundAssets = AssetDatabase.FindAssets("Block t:prefab");

				bool cap = (foundAssets.Length > amountOfItemsToShow);

				GameObject[] loadedSuggestions =
					new GameObject[cap ? amountOfItemsToShow : foundAssets.Length];
				suggestedItems = new BlockoutItemPreview[cap ? amountOfItemsToShow : foundAssets.Length];
				for (int i = 0; i < (cap ? amountOfItemsToShow : foundAssets.Length); ++i)
				{
					var path = AssetDatabase.GUIDToAssetPath(foundAssets[i]);
					suggestedItems[i].prefab =
						(GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
					suggestedItems [i].name = suggestedItems [i].prefab.name;
					suggestedItems[i].previewImage = AssetPreview.GetAssetPreview(loadedSuggestions[i]);
				}
				previousHelper = null;
			    repaint = true;
            }

            // Do a redundancy check to see if the main editor window exists. If it does, then get the favourite assets list from the scene definition
			if (parentWindow)
			{
				var dict = parentWindow.CurrentSceneSetting.assetDictionary;

				#region Favourites

				if (dict != null)
				{
					var sortedFavourites = dict.ToList().OrderByDescending(x => x.assetQuantity).ToList();
					favouriteItems = new BlockoutItemPreview[(sortedFavourites.Count > amountOfItemsToShow
						? amountOfItemsToShow
						: sortedFavourites.Count)];

					for (var i = 0; i < sortedFavourites.Count; ++i)
					{
						if (sortedFavourites[i] == null || i >= amountOfItemsToShow)
						{
							break;
						}


						var fa = AssetDatabase.FindAssets(sortedFavourites[i].assetName + " t:prefab");
						if (fa.Length != 0)
						{
							var path = AssetDatabase.GUIDToAssetPath(fa[0]);
							favouriteItems[i].prefab =
								(GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
							favouriteItems [i].name = favouriteItems [i].prefab.name;
							favouriteItems[i].previewImage = AssetPreview.GetAssetPreview(favouriteItems[i].prefab);
						}
					}

				    favouriteItems = favouriteItems.ToList().Distinct().ToArray();
				    var toRemove = favouriteItems.ToList().Where(x => x.previewImage == null).ToList();
				    favouriteItems = favouriteItems.ToList().Except(toRemove).ToArray();

				}

				#endregion
			}

		    if (repaint)
		    {
                Repaint();
		        repaint = false;
            }
		}

        // Draw a collection of item previews in a similar style to the project window
		protected void DrawAssetList(BlockoutItemPreview[] collection, float space = 10)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Space (space);
			if (collection != null){
				for (int i = 0; i < (collection.Length < amountOfItemsToShow ? collection.Length : amountOfItemsToShow); ++i) {
				
					GUILayout.BeginVertical (GUILayout.MaxHeight (currentSize + EditorGUIUtility.singleLineHeight));

					if (collection [i].previewImage == null)
						collection [i].previewImage = AssetPreview.GetAssetPreview (collection [i].prefab);
					GUILayout.Box (new GUIContent (collection [i].previewImage), logoSkin.GetStyle ("Texture"), GUILayout.Width (currentSize), GUILayout.Height (currentSize));

					DragDropGUI (collection [i], GUILayoutUtility.GetLastRect ());

					GUILayout.Label (collection [i].name, GUILayout.Width (currentSize));

					GUILayout.EndVertical ();

				}
			}
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
		}

        // Hack Used to check if the mouse is over the scene view win=dow
		private void OnScene(SceneView sceneview)
		{
		    if (sceneview == null)
		        overSceneView = false;
            else
			    overSceneView = mouseOverWindow != null ? mouseOverWindow.ToString ().Contains ("SceneView") : false;
		}

        // Drag and drop logic
		protected void DragDropGUI(BlockoutItemPreview targetPreview, Rect previewArea)
		{
			// Chache event data
			Event currentEvent = Event.current;
			EventType currentEventType = currentEvent.type;

			// The DragExited event does not have the same mouse position data as the other events,
			// so it must be checked now:
			if ( currentEventType == EventType.DragExited ) DragAndDrop.PrepareStartDrag();// Clear generic data when user pressed escape. (Unfortunately, DragExited is also called when the mouse leaves the drag area)



			switch (currentEventType){
			case EventType.MouseDown:
				if (!previewArea.Contains(currentEvent.mousePosition)) return;
                // Mouse is within the preview area and has been clicked. Reset drag data
				DragAndDrop.PrepareStartDrag();// reset data
				dragging = false;
				currentEvent.Use();
				break;
			case EventType.MouseDrag:
				// If drag was started here:
				if (!previewArea.Contains (currentEvent.mousePosition))
					return;
			    // Start the drag event with drag references
                dragging = true;
				Object[] objectReferences = new Object[1]{ targetPreview.prefab };// Careful, null values cause exceptions in existing editor code.
				DragAndDrop.objectReferences = objectReferences;// Note: this object won't be 'get'-able until the next GUI event.
				curractAssetName = targetPreview.name;
				DragAndDrop.StartDrag (targetPreview.name);
				currentEvent.Use ();
				break;
			case EventType.DragUpdated:
                    // Drag positioning has been updated so check if its valid.
                if (IsDragTargetValid ()) {
                    
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    // Spawn the asset if it doesnt exist
					if (!spwanedAsset) {
						spwanedAsset = Instantiate (targetPreview.prefab);
					}

					PlaceDraggedAsset ();
				} else
					DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
				dragging = true;
				currentEvent.Use();
				break;     
			case EventType.DragPerform:
                // When the drag event has finished, place it and end the event if its valid. If it isn't valid,
                // destroy the object
				if (IsDragTargetValid ()) {
					DragAndDrop.AcceptDrag ();
					PlaceDraggedAsset ();
				} else if (spwanedAsset != null) {
					DestroyImmediate (spwanedAsset);
				}
				dragging = true;	
				currentEvent.Use();
				break;
			case EventType.DragExited:
                // If the drag event has ben canceled, destroy the spawned asset if its already spawned
				if (spwanedAsset != null) {
					DestroyImmediate (spwanedAsset);
				}
				dragging = false;
				break;
			case EventType.MouseUp:
				// Clean up, in case MouseDrag never occurred:
				DragAndDrop.PrepareStartDrag ();
				if (!dragging && previewArea.Contains (currentEvent.mousePosition)) {
                    // if the mouse is still within the preview area and no drag event has occured, then its only been clicked
                    // So selected it in the project window
					Selection.activeGameObject = targetPreview.prefab;
					EditorGUIUtility.PingObject (targetPreview.prefab);
					curractAssetName = targetPreview.name;
				    repaint = true;
				}
				break;
			}

		}

        // Only valid if the mouse is over the scene view
		private bool IsDragTargetValid()
		{
			return overSceneView;
		}

		private void PlaceDraggedAsset()
		{
			Ray mouseRay = SceneView.lastActiveSceneView.camera.ScreenPointToRay(new Vector3(Event.current.mousePosition.x,
				Screen.height - Event.current.mousePosition.y, 0.5f));
			RaycastHit[] hit;
			hit = (Physics.RaycastAll(mouseRay));
			bool placed = false;
			if (hit.Length > 0)
			{
				for (int i = 0; i < hit.Length; ++i)
				{
					if (!DragAndDrop.objectReferences.Contains(hit[i].collider.gameObject))
					{
						spwanedAsset.transform.position = hit[i].point;
						placed = true;
						break;
					}
				}
			}
			if (!placed && spwanedAsset)
			{
				spwanedAsset.transform.position =
					SceneView.lastActiveSceneView.camera.ScreenToWorldPoint(new Vector3(
						Event.current.mousePosition.x, Screen.height - Event.current.mousePosition.y, 50f));
			}

		    repaint = true;
		}

	}

}