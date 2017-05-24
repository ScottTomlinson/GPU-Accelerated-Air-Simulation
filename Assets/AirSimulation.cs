using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(ParticleSystem))]

public class AirSimulation : MonoBehaviour {

    public ComputeShader airShader;
    public Material visualMaterial;
    [Tooltip("Number of basic cubes to be built along each axis in the compute shader, each basic cube is 10x10x10 nodes")]
    public int cubeSize = 10;
    private int basicCubeSize = 1000;
    private int numNodes;
    private ComputeBuffer airBuffer;
    private ComputeBuffer visualBuffer;
    private ComputeBuffer transferBuffer;

    private float[] inputData;
    private float[] outputData;
    private float[] transferability;

    private int kernalOne = 0;

    [Tooltip("Number of frames to skip between GPU data transfers")]
    public int getDataInterval = 10;
    private int updateCounter = 0;

    // Use this for initialization
    void Start ()
    {
        numNodes = (int)Mathf.Pow((float)cubeSize, 3f);
        numNodes *= basicCubeSize;
        SetupAirSim();
    }
    
	// Update is called once per frame
	void FixedUpdate ()
    {

        //RunAirSim();
        updateCounter++;
        if(updateCounter > getDataInterval)
        {
            //move RunAirSim() to outside of  updateCounter if statement to run every frame, but only update graphics every n frames
            RunAirSim();
            airBuffer.GetData(outputData); 
            visualBuffer.SetData(outputData);
            updateCounter = 0;
        }
    }

    void SetupAirSim()
    {
        //make input array
        inputData = new float[numNodes];

        transferability = new float[numNodes];
        for(int i = 0; i < transferability.Length; i++)
        {
            transferability[i] = 1;
        }


        ///////////////////
        //**FOR TESTING**//
        ///////////////////

        //set an individual node to a certain amount
        int _x = 24;
        int _y = 24;
        int _z = 6;
        inputData[Flatten3DIndex(_x, _y, _z)] = 500000000f;

        //ChangeTransferabilityPlaneXY(0, 99, 0, 75, 10, 0.0f);
        //ChangeTransferabilityPlaneXZ(0, 99, 9, 50, 74, 0.0f);
        
        ///////////////////
        //**FOR TESTING**//
        ///////////////////

        //make output array
        outputData = new float[numNodes];

        //find the appropriate kernal
        kernalOne = airShader.FindKernel("AirConstituentBalance");
        

        //make buffers and set inputs
        airBuffer = new ComputeBuffer(numNodes, sizeof(float));
        airBuffer.SetData(inputData);
        transferBuffer = new ComputeBuffer(numNodes, sizeof(float));
        transferBuffer.SetData(transferability);

        //tell the compute shader important info
        airShader.SetInt("numNodes", numNodes);
        airShader.SetInt("width", 10 * cubeSize);
        airShader.SetInt("height", 10 * cubeSize);
        airShader.SetInt("depth", 10 * cubeSize);

        //set the RWStructuredBuffer in the compute shader to match up with our airBuffer here
        airShader.SetBuffer(kernalOne, "airBuffer", airBuffer);
        airShader.SetBuffer(kernalOne, "transferabilityBuffer", transferBuffer);
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
        Graphics.DrawProcedural(MeshTopology.Points, numNodes);
    }

    int Flatten3DIndex(int x, int y, int z)
    {
        return x + (z * 100) + (y * 100 * 100);
    }

    void ChangeTransferabilityPlaneXY(int _xStart, int _xEnd, int _yStart, int _yEnd, int _zPlane, float newValue)
    {
        for(int x = _xStart; x <= _xEnd; x++)
        {
            for(int y = _yStart; y <= _yEnd; y++)
            {
                int index = Flatten3DIndex(x, y, _zPlane);
                transferability[index] = newValue;
            }
        }
    }
    void ChangeTransferabilityPlaneXZ(int _xStart, int _xEnd, int _zStart, int _zEnd, int _yPlane, float newValue)
    {
        for (int x = _xStart; x <= _xEnd; x++)
        {
            for (int z = _zStart; z <= _zEnd; z++)
            {
                int index = Flatten3DIndex(x, _yPlane, z);
                transferability[index] = newValue;
            }
        }
    }
    void ChangeTransferabilityPlaneYZ(int _yStart, int _yEnd, int _zStart, int _zEnd, int _xPlane, float newValue)
    {
        for (int y = _yStart; y <= _yEnd; y++)
        {
            for (int z = _zStart; z <= _zEnd; z++)
            {
                int index = Flatten3DIndex(_xPlane, y, z);
                transferability[index] = newValue;
            }
        }
    }
}
