using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class layoutTests : MonoBehaviour {

    public int cubeSize = 3;
    public int numCubesPerSide = 1;
    private int nodesPerSide;
    private int totalNodes;
    private int numCubes;
    private int nodesInCube;
    public GameObject nodeObject;
    public int nodePoolSize = 100;
    private List<GameObject> nodes;

    public Vector2[] nodeLocations;

	// Use this for initialization
	void Start () {
        numCubes = (int)Mathf.Pow((float)numCubesPerSide, 3.0f);
        nodesInCube = (int)Mathf.Pow((float)cubeSize, 3.0f);
        nodesPerSide = cubeSize * numCubesPerSide;
        totalNodes = nodesInCube * numCubes;
        nodes = new List<GameObject>();
        MakeObjectPool(nodes, nodeObject, nodePoolSize);
	}
    void MakeObjectPool(List<GameObject> pool, GameObject objToPool, int poolSize)
    {
        GameObject o = new GameObject("Nodes");
        GameObject home = o;
        for (int i = 0; i < poolSize; i++)
        {
            o = GameObject.Instantiate(nodeObject);
            nodes.Add(o);
            nodes[i].transform.parent = home.transform;
            nodes[i].SetActive(false);
        }
    }
    GameObject GetPooledObject()
    {
        int i = RNG.Ints(1, nodes.Count);
        int o = 0;
        while(o < 1)
        {
            if (nodes[i].activeInHierarchy)
            {
                i = RNG.Ints(0, nodes.Count);
            }
            else
            {
                o = 2;
            }
        }
        return nodes[i];
    }
    void ReturnAllToPool(List<GameObject> pool)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i].activeInHierarchy)
            {
                pool[i].SetActive(false);
            }
        }
    }

    int Flatten3DIndex(int x, int y, int z)
    {
        return x + (z * cubeSize) + (y * cubeSize * cubeSize);
    }
    Vector3 Expand1DIndex(int i)
    {
        int y = i / (cubeSize * cubeSize);
        int z = i / cubeSize % cubeSize;
        int x = i % cubeSize;
        Vector3 pos = new Vector3((float)x, (float)y, (float)z);
        return pos;
    }
    Vector3 ExpandOtherIndex(int i)
    {
        int y = i / (numCubesPerSide * numCubesPerSide);
        int z = i / numCubesPerSide % numCubesPerSide;
        int x = i % numCubesPerSide;
        Vector3 pos = new Vector3((float)x, (float)y, (float)z);
        return pos;
    }

    int curPass = 0;
    bool rotating = false;
    private void OnGUI()
    {
        if(GUI.Button(new Rect(10,50,100,30), "First Pass"))
        {
            ReturnAllToPool(nodes);
            MakePass(0);
        }
        if (GUI.Button(new Rect(10, 90, 100, 30), "Second Pass"))
        {
            ReturnAllToPool(nodes);
            MakePass(1);
        }
        if (GUI.Button(new Rect(10, 130, 100, 30), "Third Pass"))
        {
            ReturnAllToPool(nodes);
            MakePass(2);
        }
        if (GUI.Button(new Rect(10, 170, 100, 30), "Rotate Passes"))
        {
            ReturnAllToPool(nodes);
            MakePass(curPass);
            curPass++;
            if (curPass >= 9)
            {
                curPass = 0;
            }
        }
        if (GUI.Button(new Rect(10, 210, 100, 30), "Auto Rotate"))
        {
            rotating = !rotating;
        }
    }

    private void FixedUpdate()
    {
        if (rotating)
        {
            MakeRotation();
        }
    }

    void MakeRotation()
    {
        ReturnAllToPool(nodes);
        MakePass(curPass);
        curPass++;
        if (curPass >= 9)
        {
            curPass = 0;
        }
    }

    void MakePass(int passNum)
    {
        int activeCount = 0;
        int cubeStartIndex = 0;
        for (int i = 0; i < numCubes; i++)
        {

            Vector3 pos = Expand1DIndex(passNum);
            pos += (ExpandOtherIndex(i) * cubeSize);
            GameObject g = GetPooledObject();
            g.SetActive(true);
            g.transform.position = pos;
            activeCount++;

            g = GetPooledObject();
            g.SetActive(true);
            pos = Expand1DIndex((int)nodeLocations[passNum].x) + (ExpandOtherIndex(i) * cubeSize);
            g.transform.position = pos;
            activeCount++;

            g = GetPooledObject();
            g.SetActive(true);
            pos = Expand1DIndex((int)nodeLocations[passNum].y) + (ExpandOtherIndex(i) * cubeSize);
            g.transform.position = pos;
            activeCount++;


            /*
            for(int o = 0; o < 9; o++)
            {
                Vector3 pos = Expand1DIndex(o);
                pos += (ExpandOtherIndex(i) * cubeSize);
                GameObject g = GetPooledObject();
                g.SetActive(true);
                g.transform.position = pos;
                activeCount++;
                /*
                g = GetPooledObject();
                g.SetActive(true);
                pos = Expand1DIndex((int)nodeLocations[o].x) + (ExpandOtherIndex(i) * cubeSize);
                g.transform.position = pos;
                activeCount++;
                g = GetPooledObject();

                g.SetActive(true);
                pos = Expand1DIndex((int)nodeLocations[o].y) + (ExpandOtherIndex(i) * cubeSize);
                g.transform.position = pos;
                activeCount++;
                
              }
              */
        
            cubeStartIndex++;
        }

        //Debug.Log(activeCount + " nodes active");
    }
}
