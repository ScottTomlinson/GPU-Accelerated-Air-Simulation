using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(ParticleSystem))]

public class AirSimulation : MonoBehaviour {

    public ComputeShader airShader;
    public Material visualMaterial;
    //basic cube size in compute shader is 10 nodes
    [Tooltip("number of basic cubes to be build in the compute shader, each basic cube is 10x10x10 nodes")]
    public int cubeSize = 10;
    private int basicCubeSize = 1000;
    private int numNodes;
    private ComputeBuffer airBuffer;
    private ComputeBuffer textBuffer;
    private ComputeBuffer visualBuffer;


    private float[] inputData;
    private float[] outputData;

    private int kernalOne = 0;

    private int updateCounter = 0;
    public int getDataInterval = 10;

	// Use this for initialization
	void Start ()
    {
        Application.targetFrameRate = 240;
        numNodes = (int)Mathf.Pow((float)cubeSize, 3f);
        numNodes *= basicCubeSize;
        Debug.Log("Number of nodes on CPU: " + numNodes);
        SetupAirSim();
    }
    
	// Update is called once per frame
	void FixedUpdate ()
    {
        RunAirSim();

        updateCounter++;
        if(updateCounter > getDataInterval)
        {
            airBuffer.GetData(outputData);
            int randomIndex = (int)Random.Range(0f, (float)outputData.Length + 1f);
            //outputData[randomIndex] = 20000000f;
            visualBuffer.SetData(outputData);
            //airBuffer.SetData(outputData);
            updateCounter = 0;
        }
    }

    void SetupAirSim()
    {

        //tell the compute shader how many thread groups we're gonna dispatch
        int[] nodeCount = new int[1];
        nodeCount[0] = numNodes;
        int kNum = airShader.FindKernel("SetAirNodeQuantity");
        ComputeBuffer nodeCountBuffer = new ComputeBuffer(1, sizeof(int));
        nodeCountBuffer.SetData(nodeCount);
        airShader.SetBuffer(kNum, "nodeCountBuffer", nodeCountBuffer);
        airShader.Dispatch(kNum, 1, 1, 1);

        int[] dispatchSize = new int[3];
        for(int i = 0; i < 3; i++)
        {
            dispatchSize[i] = cubeSize;
        }
        kNum = airShader.FindKernel("SetDispatchSize");
        ComputeBuffer dispatchSizeBuffer = new ComputeBuffer(3, sizeof(int));
        dispatchSizeBuffer.SetData(dispatchSize);
        airShader.SetBuffer(kNum, "dispatchSizeBuffer", dispatchSizeBuffer);
        airShader.Dispatch(kNum, 1, 1, 1);


        //airShader.SetInt("numNodes", numNodes);
        //airShader.SetInts("dispatchSize", dispatchSize);

        airShader.SetInt("width", 10 * cubeSize);
        airShader.SetInt("height", 10 * cubeSize);
        airShader.SetInt("depth", 10 * cubeSize);

        //make sure that shit worked
        CheckAirSimData(nodeCountBuffer, dispatchSizeBuffer);

        //make input array
        inputData = new float[numNodes];
        for(int i = 0; i < numNodes; i++)
        {//randomize input data
            //inputData[i] = Random.Range(0f, 50f);
            inputData[i] = 0f;
        }
        int randomIndex = (int)Random.Range(0f, (float)inputData.Length - 1f);
        int centerIndex = 0 + (50 * 100) + (50 * 100 * 100);
        inputData[randomIndex] = 500000000f;
        //make output array
        outputData = new float[numNodes];

        //find the kernal
        kernalOne = airShader.FindKernel("AirConstituentBalance");

        //make a buffer and set the input
        airBuffer = new ComputeBuffer(numNodes, sizeof(float));
        airBuffer.SetData(inputData);


        //set the RWStructuredBuffer in the compute shader to match up with our airBuffer here
        airShader.SetBuffer(kernalOne, "airBuffer", airBuffer);
        airShader.Dispatch(kernalOne, cubeSize, cubeSize, cubeSize);

        //visual stuff
        visualBuffer = new ComputeBuffer(numNodes, sizeof(float));
        visualBuffer.SetData(inputData);
        visualMaterial.SetBuffer("airBuffer", visualBuffer);

    }

    void RunAirSim()
    {
        airShader.Dispatch(kernalOne, cubeSize, cubeSize, cubeSize);
    }

    void CheckAirSimData(ComputeBuffer nBuff, ComputeBuffer dBuff)
    {
        int[] _nodeCount = new int[1];
        int[] _dispatchSize = new int[3];
        nBuff.GetData(_nodeCount);
        dBuff.GetData(_dispatchSize);
        Debug.Log("Node count from GPU: " + _nodeCount[0]);
        Debug.Log("Dispatch size from GPU: x:" + _dispatchSize[0] + " y:" + _dispatchSize[1] + " z:" + _dispatchSize[2]);
    }

    void OnRenderObject()
    {
        visualMaterial.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points, numNodes, 1);
    }
}

struct VecMatPair
{
    public Vector3 point;
    public Matrix4x4 matrix;
}
