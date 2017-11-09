using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxPush : MonoBehaviour {

    /*
     *  when box walked into from a 'push direction', box is pushable.
     *  if box is 'roughly' at any 'node' all of that nodes 'push directions' are enabled, and pushing in the direction associated with a path will push it along that path.
     *  a box should always have two pushes - forward and back. when it is close enough to a node, it snaps to node position(+rotation) and this changes to 'previous-path and next-path'
     *  box is always on a 'path' between two 'nodes' which are declared in editor script
     *  if box hits one of a pair of 'auto nodes' the box will move along that path of it's own accord, from 'start' node to 'end' node.
     *  auto nodes are only used for one-directional movement, such as the box sliding or falling down.
     */





    /*

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    */
}
