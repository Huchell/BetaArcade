using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxPush : MonoBehaviour
{
    public List<Vector3> nodeList = new List<Vector3>();
    public List<GameObject> nodesInWorld = new List<GameObject>();
    public int nodeCount;
    public GameObject nodeMesh;

    /*
     *  when box walked into from a 'push direction', box is pushable.
     *  if box is 'roughly' at any 'node' all of that nodes 'push directions' are enabled, and pushing in the direction associated with a path will push it along that path.
     *  a box should always have two pushes - forward and back. when it is close enough to a node, it snaps to node position(+rotation) and this changes to 'previous-path and next-path'
     *  box is always on a 'path' between two 'nodes' which are declared in editor script
     *  if box hits one of a pair of 'auto nodes' the box will move along that path of it's own accord, from 'start' node to 'end' node.
     *  auto nodes are only used for one-directional movement, such as the box sliding or falling down.
     *  
     */

    void Start()
    {
        
	}

    public void RefreshNodes()
    {
        foreach (GameObject node in nodesInWorld)
        {
            DestroyImmediate(node);
        }

        foreach (Transform child in transform)
        {
            GameObject.DestroyImmediate(child.gameObject);
        }
                
        nodesInWorld.Clear();

        if (nodeList.Count < 1)
        {
            nodeList.Add(transform.position);
        }

        while (nodeList.Count < nodeCount)
        {
            nodeList.Add(nodeList[nodeList.Count-1] + new Vector3(0f, 2f, 0f));
        }

        for (int x = 0; x < nodeCount; x++)
        {
            GameObject newNode = Instantiate(nodeMesh);
            nodesInWorld.Add(newNode);
            newNode.name += "[" + x + "]";

            newNode.transform.SetParent(transform, false);

            if (nodeList[x] != null || nodeList[x] == Vector3.zero)
            {
                Debug.Log("not null");
                newNode.transform.position = nodeList[x];
            }
            else if (x == 0)
            {
                nodeList[x] = transform.position;
                Debug.Log("using parent");
                newNode.transform.position = nodeList[x];
            }
            else
            {
                nodeList[x] = (nodeList[x - 1] + new Vector3(0f, 2f, 0f));
                Debug.Log("offset by 2");
                newNode.transform.position = nodeList[x];
            }
        }
              
        if (nodeList.Count > nodeCount)
        {
            for (int x = nodeList.Count-1; x >= nodeCount; x--)
            {
                Debug.Log(x);
                if (nodeList[x] != null)
                {
                    nodeList.Remove(nodeList[x]);
                }
            }
        }

        /*

        for (int x = nodeList.Count; x >= nodeCount; x--)
        {
            nodeList.RemoveAt(x);
        }
        
         * 
         * 
         * 
         * 
         * 
         *  I want to be able to say: here's a list of world positions.
         *  For the world positions from 0 to the nodeCount, put a node object there
         *  For any world positions higher than nodeCount, remove.
         *  If a world position for a node index less than nodeCount is not defined, define it as the world position of the previous node, plus 10f in y.
         *  
         *  On refresh, remove and replace all nodes.
         *  If a node is moved, update it's world position in the list.
         *  If a node is deleted, reduce the index of all nodes further in the list by 1.
         *
         */


        /*
        int keepCount = nodeList.Count;
        if (keepCount > nodeCount)
        {
            for (int x = nodeCount; x < keepCount; x++)
            {
                //DestroyImmediate(nodeList[x]);
            }
        }

        //nodeList.Clear(); do later
        for (int x = 0; x < nodeCount; x++)
        {
            //nodeList[x].gameObject.transform.position = new Vector3(20f * x, 0f, 0f);
        }
        */
    }

    public void RefreshNodeLocations()
    {
        nodeList[0] = transform.position + new Vector3(2f, 0f, 0f);
        for (int node = 1; node < nodeList.Count; node++)
        {
            nodeList[node] = transform.position + new Vector3((node+1) * 2f, 0f, 0f);
        }
        RefreshNodes();
    }
    

}
