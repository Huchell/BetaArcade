using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatePath : MonoBehaviour {

	public Vector3[] localPathPoints;

	void Start() {
		if (localPathPoints.Length == 0) {
			Debug.LogError ("The Path has no points!");
			enabled = false;
		}
	}

	void OnDrawGizmos() {
		for (int i = 0; i < localPathPoints.Length - 1; i++) {
			Gizmos.DrawLine (
				transform.position + localPathPoints[i], 
				transform.position + localPathPoints[i + 1]);
		}
	}
}
