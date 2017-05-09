using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(ParticleSystem))]

public class AirSimulation : MonoBehaviour {

    public ComputeShader airShader;
    public Material visualMaterial;
    [Tooltip("number of basic cubes to be built in the compute shader, each basic cube is 10x10x10 nodes")]
    public int cubeSize = 10;
    private int basicCubeSize = 1000;
    private int numNodes;
    private ComputeBuffer airBuffer;
    private ComputeBuffer visualBuffer;


    private float[] inputData;
    private float[] outputData;

    private int kernalOne = 0;

    private int updateCounter = 0;
    [Tooltip("How many frames to wait between GPU data transfers")]
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

        updateCounter++;
        if(updateCounter > getDataInterval)
        {
            //move RunAirSim() to outside of  updateCounter if statement to run every frame, but only update graphics every n frames
            RunAirSim();
            airBuffer.GetData(outputData);
            //int randomIndex = (int)Random.Range(0f, (float)outputData.Length + 1f);
            //outputData[randomIndex] = 200f;
            visualBuffer.SetData(outputData);
            //airBuffer.SetData(outputData);
            updateCounter = 0;
        }
    }

    void SetupAirSim()
    {
        //make input array
        inputData = new float[numNodes];

        ///////////////////
        //**FOR TESTING**//
        ///////////////////
        //set an individual node to a certain amount
        int _x = 0;
        int _y = 0;
        int _z = 0;
        int desiredIndex = _x + (_z * 100) + (_y * 100 * 100);
        inputData[desiredIndex] = 50000000f;

        //make output array
        outputData = new float[numNodes];

        //find the appropriate kernal
        kernalOne = airShader.FindKernel("AirConstituentBalance");

        //make a buffer and set the input
        airBuffer = new ComputeBuffer(numNodes, sizeof(float));
        airBuffer.SetData(inputData);
        
        //tell the compute shader important info
        airShader.SetInt("numNodes", numNodes);
        airShader.SetInt("width", 10 * cubeSize);
        airShader.SetInt("height", 10 * cubeSize);
        airShader.SetInt("depth", 10 * cubeSize);

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

    void OnRenderObject()
    {
        visualMaterial.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points, numNodes, 1);
    }
}
