using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(ParticleSystem))]

public class AirSimulation : MonoBehaviour {

    public ComputeShader airShader;
    public Mesh visualMesh;
    public Bounds visualBounds;
    public Material visualMaterial;
    public Bounds[] rooms;
    [Tooltip("Number of basic cubes to be built along each axis in the compute shader, each basic cube is 8x8x8 nodes")]
    public int cubeSize = 12;
    //number of nodes in one basic cube (8x8x8)
    private int basicCubeSize = 512;
    private int numNodes;
    private ComputeBuffer airBuffer;
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
    private int kernalThree = 0;
    private int kernalFour = 0;
    private int kernalFive = 0;
    private int kernalSix = 0;
    private int kernalSeven = 0;
    private int kernalEight = 0;
    private int kernalNine = 0;

    [Tooltip("Number of frames to skip between GPU data transfers")]
    public int getDataInterval = 10;
    private int updateCounter = 0;

    private float massCounter = 0;
    
    public bool runContinuously = false;
    public bool visualActive = false;
    void OnAwake()
    {
        //Application.targetFrameRate = 240;
    }

    // Use this for pre-initialization
    void OnEnable ()
    {
        numNodes = (int)Mathf.Pow((float)cubeSize, 3f);
        numNodes *= basicCubeSize;
        Debug.Log(numNodes + " nodes");
        SetupAirSim();
    }
    
    void LateUpdate()
    {
        //Graphics.DrawProceduralIndirect(MeshTopology.Quads, bufferWithArgs);
        Graphics.DrawMeshInstancedIndirect(visualMesh, 0, visualMaterial, visualBounds, bufferWithArgs);
    }


	// FixedUpdate is called once per physics frame
	void FixedUpdate ()
    {
        if (runContinuously)
        {
            //RunAirSim();
        }
        updateCounter++;
        if(updateCounter > getDataInterval)
        {
            //move RunAirSim() to outside of  updateCounter if statement to run every frame, but only pull data every n frames
            RunAirSim();
            //airBuffer.GetData(outputData);
            //inputData = outputData;
            //visualBuffer.SetData(inputData);
            updateCounter = 0;
        }
    }

    void SetupAirSim()
    {
        //make input array
        inputData = new float[numNodes];

        //make transfer array
        transferability = new float[numNodes];
        for(int i = 0; i < transferability.Length; i++)
        {
            transferability[i] = 1.00f;
        }

        outputDeltaData = new float[numNodes];
        for (int i = 0; i < outputDeltaData.Length; i++)
        {
            outputDeltaData[i] = 0;
        }
        
        //neighbor checking
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


        //make output array
        outputData = new float[numNodes];

        //find the appropriate kernal
        kernalOne = airShader.FindKernel("FirstPass");
        kernalTwo = airShader.FindKernel("SecondPass");
        kernalThree = airShader.FindKernel("ThirdPass");
        kernalFour = airShader.FindKernel("FourthPass");
        kernalFive = airShader.FindKernel("FifthPass");
        kernalSix = airShader.FindKernel("SixthPass");
        kernalSeven = airShader.FindKernel("SeventhPass");
        kernalEight = airShader.FindKernel("EighthPass");
        kernalNine = airShader.FindKernel("NinthPass");

        //make buffers and set inputs
        airBuffer = new ComputeBuffer(numNodes, sizeof(float));
        airBuffer.SetData(inputData);

        transferBuffer = new ComputeBuffer(numNodes, sizeof(float));
        transferBuffer.SetData(transferability);

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
        airShader.SetBuffer(kernalThree, "airBuffer", airBuffer);
        airShader.SetBuffer(kernalFour, "airBuffer", airBuffer);
        airShader.SetBuffer(kernalFive, "airBuffer", airBuffer);
        airShader.SetBuffer(kernalSix, "airBuffer", airBuffer);
        airShader.SetBuffer(kernalSeven, "airBuffer", airBuffer);
        airShader.SetBuffer(kernalEight, "airBuffer", airBuffer);
        airShader.SetBuffer(kernalNine, "airBuffer", airBuffer);
        airShader.SetBuffer(kernalOne, "transferabilityBuffer", transferBuffer);
        airShader.SetBuffer(kernalTwo, "transferabilityBuffer", transferBuffer);
        airShader.SetBuffer(kernalThree, "transferabilityBuffer", transferBuffer);
        airShader.SetBuffer(kernalFour, "transferabilityBuffer", transferBuffer);
        airShader.SetBuffer(kernalFive, "transferabilityBuffer", transferBuffer);
        airShader.SetBuffer(kernalSix, "transferabilityBuffer", transferBuffer);
        airShader.SetBuffer(kernalSeven, "transferabilityBuffer", transferBuffer);
        airShader.SetBuffer(kernalEight, "transferabilityBuffer", transferBuffer);
        airShader.SetBuffer(kernalNine, "transferabilityBuffer", transferBuffer);
        airShader.SetBuffer(kernalOne, "neighborCount", neighborBuffer);

        //airShader.Dispatch(kernalTwo, 32, 32, 32);

        //visual stuff
        visualBuffer = new ComputeBuffer(numNodes, sizeof(float));
        visualBuffer.SetData(inputData);
        visualMaterial.SetBuffer("airVisBuffer", airBuffer);
        

        //indirect renderer from jknightdoeswork
        uint indexCountPerInstance = visualMesh.GetIndexCount(0);
        uint instanceCount = (uint)numNodes;
        uint startIndexLocation = 0;
        uint baseVertexLocation = 0;
        uint startInstanceLocation = 0;
        uint[] args = new uint[] { indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation };
        bufferWithArgs = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        bufferWithArgs.SetData(args);
    }

    int passCount = 0;
    void RunAirSim()
    {
        airShader.Dispatch(kernalTwo, 32, 32, 32);
        switch (passCount)
        {
            case 0:
                airShader.Dispatch(kernalOne, 32, 32, 32);
                break;
            case 1:
                airShader.Dispatch(kernalTwo, 32, 32, 32);
                break;
            case 2:
                airShader.Dispatch(kernalThree, 32, 32, 32);
                break;
            case 3:
                airShader.Dispatch(kernalFour, 32, 32, 32);
                break;
            case 4:
                airShader.Dispatch(kernalFive, 32, 32, 32);
                break;
            case 5:
                airShader.Dispatch(kernalSix, 32, 32, 32);
                break;
            case 6:
                airShader.Dispatch(kernalSeven, 32, 32, 32);
                break;
            case 7:
                airShader.Dispatch(kernalEight, 32, 32, 32);
                break;
            case 8:
                airShader.Dispatch(kernalNine, 32, 32, 32);
                break;
        }
        passCount++;
        if(passCount > 8)
        {
            passCount = 0;
        }
        //airShader.Dispatch(kernalOne, cubeSize, cubeSize, cubeSize);
    }
    
    int Flatten3DIndex(int x, int y, int z)
    {
        return x + (z * 96) + (y * 96 * 96);
    }

    public void Build()
    {
        MakeShip();
    }
    void MakeShip()
    {
        //1st room - works
        MakeRoom(40, 54, 40, 54, 40, 54);
        //2nd room hallway in -x direction from 1st room
        MakeRoom(20, 40, 43, 47, 43, 47);
        //open door in 1st room walls
        ChangeTransferabilityPlaneYZ(44, 46, 44, 46, 40, 1.0f);
        
        //3rd room hallway in +x direction
        MakeRoom(54, 74, 43, 47, 43, 47);
        //other door in 1st room
        ChangeTransferabilityPlaneYZ(44, 46, 44, 46, 54, 1.0f);
        //rooms
        MakeRoom(74, 84, 5, 80, 40, 50);
        ChangeTransferabilityPlaneYZ(44, 46, 44, 46, 74, 1.0f);
        MakeRoom(16, 20, 43, 47, 43, 63);
        ChangeTransferabilityPlaneYZ(44, 46, 44, 46, 20, 1.0f);
        MakeRoom(16, 60, 42, 48, 63, 73);
        ChangeTransferabilityPlaneXY(17, 19, 44, 46, 63, 1.0f);

        transferBuffer.SetData(transferability);
    }
    void MakeRoom(int _xStart, int _xEnd, int _yStart, int _yEnd, int _zStart, int _zEnd)
    {
        //xz borders - top and bottom
        ChangeTransferabilityPlaneXZ(_xStart, _xEnd, _zStart, _zEnd, _yStart, 0.000f);
        ChangeTransferabilityPlaneXZ(_xStart, _xEnd, _zStart, _zEnd, _yEnd, 0.000f);
        //xy border - sides
        ChangeTransferabilityPlaneXY(_xStart, _xEnd, _yStart, _yEnd, _zStart, 0.000f);
        ChangeTransferabilityPlaneXY(_xStart, _xEnd, _yStart, _yEnd, _zEnd, 0.000f);
        //yz borders-frant and back
        ChangeTransferabilityPlaneYZ(_yStart, _yEnd, _zStart, _zEnd, _xStart, 0.000f);
        ChangeTransferabilityPlaneYZ(_yStart, _yEnd, _zStart, _zEnd, _xEnd, 0.000f);
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
        airBuffer.GetData(outputData);
        inputData = outputData;
        inputData[Flatten3DIndex(x, y, z)] += value;
        massCounter += value;
        airBuffer.SetData(inputData);
        //Debug.Log("Air Inserted -> Amount Added: " + value + " Total Mass: " + GetTotalMass());
    }

    public float GetTotalMass()
    {
        airBuffer.GetData(outputData);
        float val = 0;
        for (int i = 0; i < numNodes; i++)
        {
            val += outputData[i];
        }
        return val;
    }
    public float GetAddedMass()
    {
        return massCounter;
    }
    public void DispatchSim()
    {
        RunAirSim();
    }


    void OnDestroy()
    {
        visualBuffer.Release();
        airBuffer.Release();
        neighborBuffer.Release();
        transferBuffer.Release();
        bufferWithArgs.Release();
    }
}
