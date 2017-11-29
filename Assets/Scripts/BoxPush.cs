using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxPush : MonoBehaviour
{
    [ReadOnly]
    public List<Vector3> nodeList = new List<Vector3>();
    [ReadOnly]
    public List<GameObject> nodesInWorld = new List<GameObject>();
    public int nodeCount;
    public GameObject nodeMesh;
    public int atNode = 0;
    public float betweenNode = 0.0f;
    [HideInInspector]
    public bool pushingNorth = false, pushingEast = false, pushingSouth = false, pushingWest = false;

    /*
     *  when box walked into from a 'push direction', box is pushable.
     *  if box is 'roughly' at any 'node' all of that nodes 'push directions' are enabled, and pushing in the direction associated with a path will push it along that path.
     *  a box should always have two pushes - forward and back. when it is close enough to a node, it snaps to node position(+rotation) and this changes to 'previous-path and next-path'
     *  box is always on a 'path' between two 'nodes' which are declared in editor script
     *  if box hits one of a pair of 'auto nodes' the box will move along that path of it's own accord, from 'start' node to 'end' node.
     *  auto nodes are only used for one-directional movement, such as the box sliding or falling down.
     */

    void Start()
    {
        foreach (GameObject node in nodesInWorld)
        {
            node.GetComponent<MeshRenderer>().enabled = false;
        }

        transform.position = nodeList[atNode];
    }

    public void RefreshNodes()
    {
        foreach (GameObject node in nodesInWorld)
        {
            DestroyImmediate(node.gameObject);
        }

        nodesInWorld.Clear();

        if (nodeList.Count < 1)
        {
            nodeList.Add(transform.position);
        }

        while (nodeList.Count < nodeCount)
        {
            nodeList.Add(nodeList[nodeList.Count - 1] + new Vector3(0f, 2f, 0f));
        }

        for (int x = 0; x < nodeCount; x++)
        {
            GameObject newNode = Instantiate(nodeMesh);
            nodesInWorld.Add(newNode);
            newNode.name += "[" + x + "]";

            newNode.transform.SetParent(transform.parent, false);

            if (nodeList[x] != null)
            {
                Debug.Log("not null");
                newNode.transform.position = nodeList[x];
            }
            else if (x == 0)
            {
                nodeList[0] = transform.position;
                Debug.Log("using parent");
                newNode.transform.position = transform.position;
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
            for (int x = nodeList.Count - 1; x >= nodeCount; x--)
            {
                Debug.Log(x);
                if (nodeList[x] != null)
                {
                    nodeList.Remove(nodeList[x]);
                }
            }
        }

        SuggestPushAlignments();

        /*
        nodesInWorld[0].transform.position = transform.position;
        nodeList[0] = transform.position;
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
        nodeList[0] = transform.position;
        for (int node = 1; node < nodeList.Count; node++)
        {
            nodeList[node] = transform.position + new Vector3((node) * 2f, 0f, 0f);
        }
        RefreshNodes();
    }

    public void SuggestPushAlignments()
    {
        for (int x = 1; x < nodesInWorld.Count; x++)
        {
            nodesInWorld[x].gameObject.GetComponent<PushBoxNodeData>().SuggestPushes(nodesInWorld[(x-1)].gameObject.transform);
            Debug.Log("node" + x + " handled");
        }

        nodesInWorld[0].gameObject.GetComponent<PushBoxNodeData>().sideToPushAway = nodesInWorld[1].gameObject.GetComponent<PushBoxNodeData>().sideToPushAway;
        nodesInWorld[0].gameObject.GetComponent<PushBoxNodeData>().sideToPushTo = nodesInWorld[1].gameObject.GetComponent<PushBoxNodeData>().sideToPushTo;

    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "BoxNode")
        {
            transform.position = collision.gameObject.transform.position;

            for (int node = 0; node < nodesInWorld.Count; node++)
            {
                if (collision.gameObject == nodesInWorld[node])
                {
                    atNode = node;
                    betweenNode = 0.0f;
                    break;
                }
            }
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        if (collision.collider.GetComponent<CharacterController>() != null)
        {
            if (collision.gameObject.transform.position.x > transform.position.x)
            {
                pushingEast = false;
                pushingWest = true;
            }
            else
            {
                pushingEast = true;
                pushingWest = false;
            }

            if (collision.gameObject.transform.position.z > transform.position.z)
            {
                pushingNorth = false;
                pushingSouth = true;
            }
            else
            {
                pushingNorth = true;
                pushingSouth = false;
            }
            
            if ((nodesInWorld[atNode + 1].gameObject.GetComponent<PushBoxNodeData>().sideToPushTo == PushBoxNodeData.collisionSide.North && pushingNorth)||
                (nodesInWorld[atNode + 1].gameObject.GetComponent<PushBoxNodeData>().sideToPushTo == PushBoxNodeData.collisionSide.East && pushingEast)||
                (nodesInWorld[atNode + 1].gameObject.GetComponent<PushBoxNodeData>().sideToPushTo == PushBoxNodeData.collisionSide.South && pushingSouth)||
                (nodesInWorld[atNode + 1].gameObject.GetComponent<PushBoxNodeData>().sideToPushTo == PushBoxNodeData.collisionSide.West && pushingWest))
            {
                betweenNode += 0.05f;
            }
            else if ((nodesInWorld[atNode + 1].gameObject.GetComponent<PushBoxNodeData>().sideToPushAway == PushBoxNodeData.collisionSide.North && pushingNorth) ||
                    (nodesInWorld[atNode + 1].gameObject.GetComponent<PushBoxNodeData>().sideToPushAway == PushBoxNodeData.collisionSide.East && pushingEast) ||
                    (nodesInWorld[atNode + 1].gameObject.GetComponent<PushBoxNodeData>().sideToPushAway == PushBoxNodeData.collisionSide.South && pushingSouth) ||
                    (nodesInWorld[atNode + 1].gameObject.GetComponent<PushBoxNodeData>().sideToPushAway == PushBoxNodeData.collisionSide.West && pushingWest))
                    {
                        betweenNode -= 0.05f;
                    }

            transform.position = Vector3.Lerp(nodesInWorld[atNode - 1].gameObject.transform.position, nodesInWorld[atNode].gameObject.transform.position, betweenNode);

            if (betweenNode < 0)
            {
                if (atNode != 0)
                {
                    betweenNode = 1 + betweenNode;
                    atNode -= 1;
                }
            }
        }
    }

        public void OnCollisionExit(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            pushingNorth = pushingEast = pushingSouth = pushingWest = false;
        }
    }
}