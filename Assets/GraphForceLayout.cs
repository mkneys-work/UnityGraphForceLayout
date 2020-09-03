using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GraphForceLayout : MonoBehaviour
{
    public GameObject nodePrefab;
    public GameObject linkPrefab;

    private GameObject[] nodeObjects;
    private GameObject[] edgeObjects;
    private Vector3[] nodesForces;
    private int[,] edgeStructureContainer;
    private float stepCoef = 0.1f;
    private const float stepCoefdiffPerFrame = 0.0001f;
    
    //this constant defines how much will graph spread in scene (lesser value lesser spread), value 50 was experimentally found as good value for position and fov of camera setted in Unity.
    private const int graphSpreadValue = 50;
    
    private const int nodeCount = 8;
    private int edgeCount = 28;

    void Start()
    {
        int maximumPosibleEdgeCount = (nodeCount - 1) * (nodeCount / 2);
        edgeCount = System.Math.Min(edgeCount, maximumPosibleEdgeCount);

        nodeObjects = new GameObject[nodeCount];
        edgeObjects = new GameObject[edgeCount]; 
        nodesForces = new Vector3[nodeCount];
        edgeStructureContainer = new int[edgeCount, 2];
        
        CreateSceneObjects();
        GenerateSceneGraphStructure();
        RecalculateEdgesSpatialInfoBasedOnNodes();
    }

    void Update()
    {
        if(stepCoef - stepCoefdiffPerFrame > 0)
        {
            CalculateRepulsiveForcesForAllNodes();
            CalculateAttractiveForcesBasedOnEdges();
            ApplyForcesToNodes();
            RecalculateEdgesSpatialInfoBasedOnNodes();
            stepCoef -= stepCoefdiffPerFrame;
        } 
    }

    private void ApplyForcesToNodes()
    {
        for (int i = 0; i < nodeCount; i++)
        {
            Vector3 motionVector = (nodesForces[i] / nodesForces[i].magnitude) * System.Math.Min(stepCoef, nodesForces[i].magnitude);
            nodeObjects[i].transform.position = nodeObjects[i].transform.position + motionVector;
        }
    }

    private void CalculateAttractiveForcesBasedOnEdges()
    {
        for (int edge = 0; edge < edgeCount; edge++)
        {
            int firstNodeIndex = edgeStructureContainer[edge, 0];
            int secondNodeIndex = edgeStructureContainer[edge, 1];

            nodesForces[firstNodeIndex] -= GetAttractiveForceForNodes(firstNodeIndex, secondNodeIndex);
            nodesForces[secondNodeIndex] += GetAttractiveForceForNodes(firstNodeIndex, secondNodeIndex);
        }
    }

    private void CalculateRepulsiveForcesForAllNodes()
    {
        for (int i = 0; i < nodeCount; i++)
        {
            nodesForces[i] = new Vector3(0, 0, 0);

            for (int j = 0; j < nodeCount; j++)
            {
                if (j != i)
                {
                    nodesForces[i] += GetRepulsiveForceForNodes(i, j);
                }
            }
        }
    }

    private Vector3 GetAttractiveForceForNodes(int i, int j)
    {
        Vector3 firstNodePos = nodeObjects[i].transform.position;
        Vector3 secondNodePos = nodeObjects[j].transform.position;

        return (Vector3.Magnitude(firstNodePos - secondNodePos) / graphSpreadValue) * (firstNodePos - secondNodePos);
    }

    private Vector3 GetRepulsiveForceForNodes(int i, int j)
    {
        Vector3 firstNodePos = nodeObjects[i].transform.position;
        Vector3 secondNodePos = nodeObjects[j].transform.position;

        return -((graphSpreadValue * graphSpreadValue) / (Vector3.Magnitude(firstNodePos - secondNodePos) * Vector3.Magnitude(firstNodePos - secondNodePos))) * (secondNodePos - firstNodePos);
    }

    private bool IsNewNodePositionAlreadyInGraph(Vector3 center, int alreadyAddedNodesCount)
    {
        for(int i = 0; i < alreadyAddedNodesCount; i++)
        {
            if (nodeObjects[i].transform.position.Equals(center))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsEdgeAlreadyInGraph(int edgeIndex1, int edgeIndex2, int alreadyAddedEdgesCount)
    {
        for (int i = 0; i < alreadyAddedEdgesCount; i++)
        {

            bool isEdgeInEdgeSet = (edgeIndex1 == edgeStructureContainer[i, 0] && edgeIndex2 == edgeStructureContainer[i, 1]) || (edgeIndex2 == edgeStructureContainer[i, 0] && edgeIndex1 == edgeStructureContainer[i, 1]);
            if (isEdgeInEdgeSet)
            {
                return true;
            }
        }
        return false;
    }

    private void GenerateSceneGraphStructure()
    {
        GenerateRandomNodes();
        GenerateRandomEdges();
    }

    private void GenerateRandomEdges()
    {
        for (int i = 0; i < edgeCount; i++)
        {
            int randomNodeIndex1, randomNodeIndex2;
            do
            {
                randomNodeIndex1 = Random.Range(0, nodeCount);
                randomNodeIndex2 = Random.Range(0, nodeCount);
            } while (randomNodeIndex1 == randomNodeIndex2 || IsEdgeAlreadyInGraph(randomNodeIndex1, randomNodeIndex2, i));

            edgeStructureContainer[i, 0] = randomNodeIndex1;
            edgeStructureContainer[i, 1] = randomNodeIndex2;
        }
    }

    private void GenerateRandomNodes()
    {
        for (int i = 0; i < nodeCount; i++)
        {
            Vector3 randomPosition;
            do
            {
                randomPosition = new Vector3(Random.Range(-3.0f, 3.0f), Random.Range(-3.0f, 3.0f), Random.Range(-3.0f, 3.0f));
            } while (IsNewNodePositionAlreadyInGraph(randomPosition, i));
            nodeObjects[i].transform.position = randomPosition;
        }
    }

    private void CreateSceneObjects()
    {
        for (int i = 0; i < nodeCount; i++)
        {
            nodeObjects[i] = Instantiate(nodePrefab, transform, true) as GameObject;
        }

        for (int i = 0; i < edgeCount; i++)
        {
            edgeObjects[i] = Instantiate(linkPrefab, transform, true) as GameObject;
        }
    }

    private void RecalculateEdgesSpatialInfoBasedOnNodes()
    {
        for (int i = 0; i < edgeCount; i++)
        {
            Vector3 firstNode = nodeObjects[edgeStructureContainer[i, 0]].transform.position;
            Vector3 secondNode = nodeObjects[edgeStructureContainer[i, 1]].transform.position;

            edgeObjects[i].transform.position = (firstNode + secondNode) / 2;
            edgeObjects[i].transform.up = (secondNode - firstNode).normalized;
            edgeObjects[i].transform.localScale = new Vector3(edgeObjects[i].transform.localScale.x, Vector3.Distance(firstNode, secondNode) * 0.5f, edgeObjects[i].transform.localScale.z);
        }
    }
}
