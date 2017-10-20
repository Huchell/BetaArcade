/* Radical Forge Copyright (c) 2017 All Rights Reserved
   </copyright>
   <author>Frederic Babord</author>
   <date>15th June 2017</date>
   <summary>Editor Helper to place pin comments in the scene</summary>*/

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RadicalForge.Blockout
{
    [InitializeOnLoad]
    [ExecuteInEditMode]
    public class BlockoutEditorHelper : EditorWindow
    {
        private static BlockoutEditorWindow targetWindow;

        public static void Awake()
        {
            targetWindow = BlockoutEditorWindow.Instance;
            SceneView.onSceneGUIDelegate += OnScene;
        }

        public static void Destroy()
        {
            SceneView.onSceneGUIDelegate -= OnScene;
        }

        private static void OnScene(SceneView sceneview)
        {
            if (targetWindow)
                if (targetWindow.commentPinToPlace >= 0)
                {
                    var cur = Event.current;

                    if (cur.type == EventType.MouseDown && cur.button == 0)
                    {
                        var hits = Physics.SphereCastAll(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), 0.25f, 50);
                        hits.ToList().ForEach(x => Debug.Log(x.collider.gameObject));
                        if (hits.Length > 0)
                            for (var i = 0; i < hits.Length; i++)
                            {
                                var hit = hits[i];

                                if (hit.collider.GetComponent<Notepad>())
                                    continue;

                                var targetRotation =
                                    (Quaternion.FromToRotation(Vector3.up, hit.normal) *
                                     hit.collider.transform.rotation)
                                    .eulerAngles;

                                for (var j = 0; j < 3; ++j)
                                    if (targetRotation[j] > -45 && targetRotation[j] <= 45)
                                        targetRotation[j] = 0;
                                    else if (targetRotation[j] > 45 && targetRotation[j] <= 135)
                                        targetRotation[j] = 90;
                                    else if (targetRotation[j] > 135 && targetRotation[j] <= 225)
                                        targetRotation[j] = 180;
                                    else
                                        targetRotation[j] = 270;

                                var pin = Instantiate(targetWindow.pinObjects[targetWindow.commentPinToPlace],
                                                      hit.point,
                                                      Quaternion.Euler(targetRotation));
                                Undo.RegisterCreatedObjectUndo(pin, "Create Pin Comment");
                                pin.transform.SetParent(GameObject.Find("Blockout").GetComponentsInChildren<BlockoutSection>().Where(x => x.Section == SectionID.Comments).ToArray()[0].transform);
                                pin.AddComponent<Notepad>();
                                var bpg = pin.AddComponent<BlockoutPinGizmo>();
                                bpg.SelectAfterFrame = true;
                                targetWindow.commentPinToPlace = -1;
                                break;
                            }
                    }
                }
        }
    }
}