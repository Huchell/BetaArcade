using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CollisionMaskWindow : EditorWindow {

    private struct ColliderInfo<T> where T : Collider
    {
        public T Colliders;
        public bool show;
    }
    [MenuItem("Tools/Collision Mask")]
    private static void Create()
    {
        CollisionMaskWindow cmw = GetWindow<CollisionMaskWindow>();

        cmw.Show();
    }

    private const string CollisionParentName = "Collision Mask";

    private GameObject selectedObject;
    private Transform collisionParent;
    private List<Collider> selectedColliders;

    private List<BoxCollider> BoxColliders;
    private List<SphereCollider> SphereColliders;
    private List<CapsuleCollider> CapsuleColliders;
    private List<MeshCollider> MeshColliders;

    private Vector2 colliderAreasScrollPosition;
    private Vector2 clickDownPosition;

    private bool showBoxColliders, showSphereColliders, showCapsuleColliders, showMeshColliders;
    private Vector2 boxCollidersScrollPosition, sphereColliderScrollPosition, capsuleColliderScrollPosition, meshColliderScrollPosition;
    private float boxColliderHeight, sphereColliderHeight, capsuleColliderHeight, meshColliderHeight; 

    private void Awake()
    {
        //autoRepaintOnSceneChange = true;

        selectedColliders = new List<Collider>();
        RefreshSelection();  
    }

    private void OnGUI()
    {
        HeaderGUI();

        if (collisionParent)
        {
            CollisionGUI();
        }
        else
        {
            InitializeCollisionMaskGUI();
        }
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnSelectionChange()
    {
        RefreshSelection();
    }

    private void OnFocus()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;
    }
    private void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        for (int i = 0; i < selectedColliders.Count; i++)
        {
            Collider c = selectedColliders[i];

            if (c)
            {
                switch (Tools.current)
                {
                    case Tool.Move:
                        EditorGUI.BeginChangeCheck();

                        Vector3 pos = Handles.DoPositionHandle(c.transform.position, c.transform.rotation);

                        if (EditorGUI.EndChangeCheck())
                        {
                            c.transform.position = pos;
                            selectedColliders[i] = c;
                        }
                        break;
                    case Tool.View: break;
                    case Tool.Rotate:
                        EditorGUI.BeginChangeCheck();

                        Quaternion rot = Handles.DoRotationHandle(c.transform.rotation, c.transform.position);

                        if (EditorGUI.EndChangeCheck())
                        {
                            c.transform.rotation = rot;
                            selectedColliders[i] = c;
                        }
                        break;
                    case Tool.Scale:
                        EditorGUI.BeginChangeCheck();

                        Vector3 scl = Handles.DoScaleHandle(c.transform.localScale, c.transform.position, c.transform.rotation, HandleUtility.GetHandleSize(c.transform.position));

                        if (EditorGUI.EndChangeCheck())
                        {
                            c.transform.localScale = scl;
                            selectedColliders[i] = c;
                        }
                        break;
                    case Tool.Rect: break;
                    case Tool.None: break;
                }
            }
            else
            {
                selectedColliders.Remove(c);
            }
        }
    }

    private void HeaderGUI()
    {
        EditorGUILayout.LabelField("Collision Mask Helper", EditorStyles.largeLabel);
        EditorGUILayout.LabelField("This window will help you build collision masks for your objects", EditorStyles.label);

        EditorGUILayout.Space();
    }
    private void CollisionGUI()
    {
        colliderAreasScrollPosition = EditorGUILayout.BeginScrollView(colliderAreasScrollPosition);

        showBoxColliders = DrawCollidersInfo(ref BoxColliders, "Box Colliders", showBoxColliders, ref boxCollidersScrollPosition);
        EditorGUILayout.Space();

        showSphereColliders = DrawCollidersInfo(ref SphereColliders, "Sphere Colliders", showSphereColliders, ref sphereColliderScrollPosition);
        EditorGUILayout.Space();

        showCapsuleColliders = DrawCollidersInfo(ref CapsuleColliders, "Capsule Colliders", showCapsuleColliders, ref capsuleColliderScrollPosition);
        EditorGUILayout.Space();

        showMeshColliders = DrawCollidersInfo(ref MeshColliders, "Mesh Colliders", showMeshColliders, ref meshColliderScrollPosition);

        EditorGUILayout.EndScrollView();
    }
    private bool DrawCollidersInfo<T>(ref List<T> colliders, string name, bool show, ref Vector2 scrollPos) where T : Collider
    {
        EditorGUILayout.BeginHorizontal();

        show = EditorGUILayout.Foldout(show, name, true);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Add"))
        {
            colliders.Add(AddCollider<T>());
            show = true;
        }

        EditorGUILayout.EndHorizontal();

        if (show)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, "box", GUILayout.MaxHeight(150));

            if (colliders != null)
            {
                for (int i = 0; i < colliders.Count; i++)
                {
                    bool destroy = false;
                    T bc = colliders[i];

                    if (bc)
                    {
                        GUI.color = selectedColliders.Contains(bc) ? Color.blue : Color.white * 0f;

                        Rect r = EditorGUILayout.BeginHorizontal("box");

                        GUI.color = Color.white;

                        EditorGUILayout.BeginVertical();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(bc.name);

                        GUILayout.FlexibleSpace();

                        destroy = GUILayout.Button("Delete");

                        EditorGUILayout.EndHorizontal();

                        if (selectedColliders.Contains(bc))
                        {
                            EditorGUI.indentLevel++;

                            EditorGUI.BeginDisabledGroup(true);

                            EditorGUILayout.Vector3Field("Position", bc.transform.position);
                            EditorGUILayout.Vector3Field("Rotation", bc.transform.eulerAngles);
                            EditorGUILayout.Vector3Field("Scale", bc.transform.localScale);

                            EditorGUI.EndDisabledGroup();
                            EditorGUI.indentLevel--;
                        }

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();

                        if (r.Contains(Event.current.mousePosition))
                        {
                            if (Event.current.type == EventType.MouseDown)
                            {
                                if (Event.current.control)
                                {
                                    if (selectedColliders.Contains(bc))
                                        selectedColliders.Remove(bc);
                                    else
                                        selectedColliders.Add(bc);
                                }
                                else
                                {
                                    if (selectedColliders.Contains(bc))
                                    {
                                        selectedColliders.Remove(bc);
                                    }
                                    else
                                    {
                                        selectedColliders.Clear();
                                        selectedColliders.Add(bc);
                                    }
                                }

                                Event.current.Use();
                            }
                        }
                    }
                    else
                    {
                        colliders.Remove(bc);
                    }

                    if (destroy)
                    {
                        DestroyImmediate(bc);
                        Repaint();
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        return show;
    }
    private void InitializeCollisionMaskGUI()
    {
        if (selectedObject)
        {
            EditorGUILayout.LabelField("Click the button to generate the collision mask");
            if (GUILayout.Button("Generate Collision Mask"))
            {
                collisionParent = new GameObject(CollisionParentName).transform;
                collisionParent.SetParent(selectedObject.transform, false);
            }
        }
        else
        {
            EditorGUILayout.LabelField("Select a object to start!");
        }
    }

    private void RefreshSelection()
    {
        selectedObject = null;
        collisionParent = null;

        if (Selection.activeGameObject)
        {
            selectedObject = Selection.activeGameObject;

            collisionParent = selectedObject.transform.Find(CollisionParentName);

            if (collisionParent)
            {
                RefreshColliderReference();
            }
        }
    }
    private void RefreshColliderReference()
    {
        SetColliderList(ref BoxColliders);
        SetColliderList(ref SphereColliders);
        SetColliderList(ref CapsuleColliders);
        SetColliderList(ref MeshColliders);
    }

    private void SetColliderList<T>(ref List<T> list) where T : Collider
    {
        list = new List<T>(collisionParent.GetComponentsInChildren<T>());
    }
    private void ClearColliderList<T>(ref List<T> list)
    {
        if (list == null)
            list = new List<T>();
        else
            list.Clear();

    }
    private T AddCollider<T>() where T : Collider
    {
        if (collisionParent)
        {
            // Name format = Collision_{TypeName}_{IndexOfCollider}
            GameObject newCollider = new GameObject(string.Format("Collision_{0}_{1}", typeof(T).Name, collisionParent.GetComponentsInChildren<T>().Length));
            newCollider.transform.SetParent(collisionParent.transform, false);
            return newCollider.AddComponent<T>();
        }

        return null;
    }
}
