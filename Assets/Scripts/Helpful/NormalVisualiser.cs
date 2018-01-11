using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NormalVisualiser : MonoBehaviour {

    public float normalHight = .5f;
    public Color normalColor = Color.green;

    private Mesh mesh;
    public Mesh Mesh
    {
        get
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf)
            {
                mesh = mf.sharedMesh;
            }

            return mesh;
        }
    }

    private void Start()
    {
        Destroy(this);
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < Mesh.vertices.Length; i++)
        {
            Vector3 pos = transform.TransformPoint(Mesh.vertices[i]);
            Vector3 end = pos + (transform.TransformVector(mesh.normals[i]) * normalHight);

            Color oldColor = Gizmos.color;
            Gizmos.color = normalColor;
            Gizmos.DrawLine(pos, end);
            Gizmos.color = oldColor;
        }
    }
}
