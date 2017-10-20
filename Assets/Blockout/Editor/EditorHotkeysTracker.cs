/* Radical Forge Copyright (c) 2017 All Rights Reserved
   </copyright>
   <author>Frederic Babord</author>
   <date>15th June 2017</date>
   <summary>Hotkey support bindings for the Blockout Window</summary>*/

using UnityEditor;
using UnityEngine;

namespace RadicalForge.Blockout
{
    [InitializeOnLoad]
    public static class EditorHotkeysTracker
    {
        static EditorHotkeysTracker()
        {
            SceneView.onSceneGUIDelegate += view =>
            {
                var e = Event.current;
                if (e != null && e.keyCode != KeyCode.None)
                {
                    if (e.control && e.alt && e.keyCode == KeyCode.B && e.type == EventType.KeyDown)
                        if (!BlockoutEditorWindow.isVisible)
                        {
                            BlockoutEditorWindow.Init();
                        }
                        else
                        {
                            EditorWindow.GetWindow<BlockoutEditorWindow>().Close();
                            SceneView.currentDrawingSceneView.Focus();
                        }

                    if (e.alt && e.keyCode == KeyCode.S && e.type == EventType.KeyDown)
                        if (BlockoutEditorWindow.isVisible)
                        {
                            var window = BlockoutEditorWindow.Instance;
                            window.Focus();
                            window.doSnapPosition = !window.doSnapPosition;
                            SceneView.currentDrawingSceneView.Focus();
                        }

                    if (e.alt && e.control && e.keyCode == KeyCode.Z && e.type == EventType.KeyDown)
                        if (BlockoutEditorWindow.isVisible)
                        {
                            var window = BlockoutEditorWindow.Instance;
                            window.Focus();
                            window.DecreaseSnapValue();
                            SceneView.currentDrawingSceneView.Focus();
                        }
                    if (e.alt && e.control && e.keyCode == KeyCode.X && e.type == EventType.KeyDown)
                        if (BlockoutEditorWindow.isVisible)
                        {
                            var window = BlockoutEditorWindow.Instance;
                            window.Focus();
                            window.IncreaseSnapValue();
                            SceneView.currentDrawingSceneView.Focus();
                        }

                    if (e.keyCode == KeyCode.End && e.type == EventType.KeyDown)
                    {
                        var window = BlockoutEditorWindow.Instance;
                        var tempDoSnap = window.doSnapPosition;
                        if (tempDoSnap)
                            window.doSnapPosition = !window.doSnapPosition;
                        window.Snap(Vector3.down, BlockoutAxis.Y, true);
                        if (tempDoSnap)
                            window.doSnapPosition = !window.doSnapPosition;
                    }

                    if (e.alt && e.keyCode == KeyCode.C && e.type == EventType.KeyDown)
                    {
                        var window = BlockoutEditorWindow.Instance;
                        window.showCommentsBox = !window.showCommentsBox;
                        SceneView.currentDrawingSceneView.Focus();
                    }

                    if (e.keyCode == KeyCode.G && e.type == EventType.KeyDown)
                    {
                        BlockoutEditorWindow.SelectAsset();
                    }
                }
            };
        }
    }
}