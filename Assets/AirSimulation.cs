using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(ParticleSystem))]

public class AirSimulation : MonoBehaviour {

    public ComputeShader airShader;
    public Material visualMaterial;
    [Tooltip("Number of basic cubes to be built along each axis in the compute shader, each basic cube is 8x8x8 nodes")]
    public int cubeSize = 12;
    private int basicCubeSize = 512;
    private int numNodes;
    private ComputeBuffer airBuffer;
    private ComputeBuffer airDeltaBuffer;
    private ComputeBuffer neighborBuffer;
    private ComputeBuffer visualBuffer;
    private ComputeBuffer transferBuffer;
    private ComputeBuffer bufferWithArgs;

    private float[] inputData;
    private float[] outputData;
    private float[] outputDeltaData;
    private float[] transferability;
    private int[] neighborCounts;

    private int kernalOne = 0;
    private int kernalTwo = 0;
    [Tooltip("Number of frames to skip between GPU data transfers")]
    public int getDataInterval = 10;
    private int updateCounter = 0;

    void OnAwake()
    {
        Application.targetFrameRate = 120;
    }

    // Use this for initialization
    void Start ()
    {
        numNodes = (int)Mathf.Pow((float)cubeSize, 3f);
        numNodes *= basicCubeSize;
        Debug.Log(numNodes + " nodes");
        SetupAirSim();
    }
    
	// Update is called once per frame
	void FixedUpdate ()
    {

        RunAirSim();
        updateCounter++;
        if(updateCounter > getDataInterval)
        {
            
            //move RunAirSim() to outside of  updateCounter if statement to run every frame, but only update graphics every n frames
            //RunAirSim();
            airBuffer.GetData(outputData);
            inputData = outputData;
            visualBuffer.SetData(inputData);
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
        outputDeltaData = new float[numNodes];
        for (int i = 0; i < outputDeltaData.Length; i++)
        {
            outputDeltaData[i] = 0;
        }

        int _x = 0;
        int _y = 0;
        int _z = 0;
        int neighbsWithZero = 0;
        int nodesWithNeighbs = 0;
        neighborCounts = new int[numNodes];
        for(int i = 0; i < neighborCounts.Length; i++)
        {
            int count = 0;
            for(int x = -1; x <= 1; x++)
            {
                int checkX = x + _x;
                if (checkX >= 0 && checkX <= 96)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        int checkZ = z + _z;
                        if (checkZ >= 0 && checkZ <= 96)
                        {
                            for (int y = -1; y <= 1; y++)
                            {
                                int checkY = y + _y;
                                if (checkY >= 0 && checkY <= 96)
                                {
                                    count++;
                                }
                            }
                        }
                    }
                }
            }

            neighborCounts[i] = count;
            if(neighborCounts[i] <= 0)
            {
                neighborCounts[i] = 1;
                neighbsWithZero++;
            }
            else
            {
                nodesWithNeighbs++;
            }

            _x++;
            if(_x > 95)
            {
                _x = 0;
                _z++;
                if(_z > 95)
                {
                    _z = 0;
                    _y++;
                    if(_y > 95)
                    {
                        _y = 0;
                    }
                }
            }
        }
        //Debug.Log(neighbsWithZero + " " + nodesWithNeighbs);
        SetOutsideBorder();

        //make output array
        outputData = new float[numNodes];

        //find the appropriate kernal
        kernalOne = airShader.FindKernel("AirConstituentBalance");
        kernalTwo = airShader.FindKernel("AirDeltaSum");

        //make buffers and set inputs
        airBuffer = new ComputeBuffer(numNodes, sizeof(float));
        airBuffer.SetData(inputData);
        transferBuffer = new ComputeBuffer(numNodes, sizeof(float));
        transferBuffer.SetData(transferability);
        airDeltaBuffer = new ComputeBuffer(numNodes, sizeof(float));
        airDeltaBuffer.SetData(outputDeltaData);

        neighborBuffer = new ComputeBuffer(numNodes, sizeof(int));
        neighborBuffer.SetData(neighborCounts);

        //tell the compute shader important info
        airShader.SetInt("numNodes", numNodes);
        airShader.SetInt("width", 8 * cubeSize);
        airShader.SetInt("height", 8 * cubeSize);
        airShader.SetInt("depth", 8 * cubeSize);

        //set the RWStructuredBuffer in the compute shader to match up with our airBuffer here
        airShader.SetBuffer(kernalOne, "airBuffer", airBuffer);
        airShader.SetBuffer(kernalTwo, "airBuffer", airBuffer);
        airShader.SetBuffer(kernalOne, "transferabilityBuffer", transferBuffer);
        airShader.SetBuffer(kernalOne, "deltas", airDeltaBuffer);
        airShader.SetBuffer(kernalTwo, "deltas", airDeltaBuffer);
        airShader.SetBuffer(kernalOne, "neighborCount", neighborBuffer);
        airShader.Dispatch(kernalOne, cubeSize, cubeSize, cubeSize);

        //visual stuff
        visualBuffer = new ComputeBuffer(numNodes, sizeof(float));
        visualBuffer.SetData(inputData);
        visualMaterial.SetBuffer("airBuffer", visualBuffer);

        //idk other vis stuff
        uint indexCountPerInstance = (uint)4;
        uint instanceCount = (uint)numNodes;
        uint startIndexLocation = 0;
        uint baseVertexLocation = 0;
        uint startInstanceLocation = 0;
        uint[] args = new uint[] { indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation };
        bufferWithArgs = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        bufferWithArgs.SetData(args);
    }

    bool sum = false;
    void RunAirSim()
    {
        //airShader.Dispatch(kernalOne, cubeSize, cubeSize, cubeSize);
        
        if (sum)
        {
            airShader.Dispatch(kernalOne, cubeSize, cubeSize, cubeSize);
        }
        else
        {
            airShader.Dispatch(kernalTwo, cubeSize, cubeSize, cubeSize);
        }
        sum = !sum;
        
    }

    void OnRenderObject()
    {
        visualMaterial.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points, numNodes);
    }

    int Flatten3DIndex(int x, int y, int z)
    {
        return x + (z * 96) + (y * 96 * 96);
    }

    void SetOutsideBorder()
    {
        //such hack much smart
        //xy borders
        ChangeTransferabilityPlaneXY(0, 95, 0, 95, 30, 0.0f);
        ChangeTransferabilityPlaneXY(0, 95, 0, 95, 64, 0.0f);

        //xz borders
        ChangeTransferabilityPlaneXZ(0, 95, 0, 95, 30, 0.0f);
        ChangeTransferabilityPlaneXZ(0, 95, 0, 95, 64, 0.0f);

        //yz borders
        ChangeTransferabilityPlaneYZ(0, 95, 0, 95, 30, 0.0f);
        ChangeTransferabilityPlaneYZ(0, 95, 0, 95, 64, 0.0f);
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

    public void AddAirAtPoint(int x, int y, int z, float value)
    {
        inputData[Flatten3DIndex(x, y, z)] += value;
        airBuffer.SetData(inputData);
        Debug.Log("Air Inserted -> Amount Added: " + value + " Total Volume: " + GetTotalVolume());
    }

    public float GetTotalVolume()
    {
        float val = 0;
        for (int i = 0; i < numNodes; i++)
        {
            val += outputData[i];
        }
        return val;
    }

    public void DispatchSim()
    {
        RunAirSim();
    }

    void OnDestroy()
    {
        visualBuffer.Release();
        airBuffer.Release();
        airDeltaBuffer.Release();
        neighborBuffer.Release();
        transferBuffer.Release();
        bufferWithArgs.Release();
    }
}
