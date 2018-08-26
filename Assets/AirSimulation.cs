using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[RequireComponent(typeof(ParticleSystem))]

public class AirSimulation : MonoBehaviour {

    public ComputeShader airShader;
    public Mesh visualMesh;
    public Bounds visualBounds;
    public Material visualMaterial;
    public Bounds[] rooms;
    [Tooltip("Number of basic cubes to be built along each axis in the compute shader, each basic cube is 3x3x3 nodes")]
    public int cubeSize = 32;
    //number of nodes in one basic cube (3x3x3)
    private int basicCubeSize = 27;
    private int numNodes;
    private int sideLength;
    private ComputeBuffer airBuffer;
    private ComputeBuffer visualBuffer;
    private ComputeBuffer transferBuffer;
    private ComputeBuffer bufferWithArgs;
    private CommandBuffer commandBuffer;

    private float[] inputData;
    private float[] outputData;
    private float[] outputDeltaData;
    private float[] transferability;
    private int[] neighborCounts;
   
    private int kernalBalance = 0;
    private int randomPass = 0;

    private float massCounter = 0;
    
    public bool runContinuously = false;
    public bool visualActive = false;
    
    // Use this for pre-initialization
    void OnEnable ()
    {
        numNodes = (int)Mathf.Pow((float)cubeSize, 3f);
        numNodes *= basicCubeSize;
        Debug.Log(numNodes + " nodes");
        sideLength = 3 * cubeSize;
        SetupAirSim();
    }

    private void Update()
    {
        RunAirSim();
    }

    // FixedUpdate is called once per physics frame
    void FixedUpdate ()
    {
        randomPass = RNG.Ints(0,9);
        airShader.SetInt("curDeltaChoice", randomPass);
    }

    void LateUpdate()
    {
        if (visualActive)
        {
            Graphics.DrawMeshInstancedIndirect(visualMesh, 0, visualMaterial, visualBounds, bufferWithArgs);
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

        //make output array
        outputData = new float[numNodes];

        kernalBalance = airShader.FindKernel("Balance");

        
        //make buffers and set inputs
        airBuffer = new ComputeBuffer(numNodes, sizeof(float));
        airBuffer.SetData(inputData);

        transferBuffer = new ComputeBuffer(numNodes, sizeof(float));
        transferBuffer.SetData(transferability);

        //tell the compute shader important info
        airShader.SetInt("numNodes", numNodes);
        airShader.SetInt("width", sideLength);
        airShader.SetInt("height", sideLength);
        airShader.SetInt("depth", sideLength);

        //tell visual shader important info
        visualMaterial.SetInt("width", sideLength);
        visualMaterial.SetInt("height", sideLength);
        visualMaterial.SetInt("depth", sideLength);
        Vector3 _extents = new Vector3(sideLength, sideLength, sideLength);
        Vector3 _center = _extents / 2;
        visualBounds.center = _center;
        visualBounds.extents = _extents;

        //set the RWStructuredBuffer in the compute shader to match up with our airBuffer here
        airShader.SetBuffer(kernalBalance, "airBuffer", airBuffer);
        airShader.SetBuffer(kernalBalance, "transferabilityBuffer", transferBuffer);

        //command buffer
        commandBuffer = new CommandBuffer();

        commandBuffer.BeginSample("Balance");
        commandBuffer.DispatchCompute(airShader, kernalBalance, cubeSize, cubeSize, cubeSize);
        commandBuffer.EndSample("Balance");

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
    
    void RunAirSim()
    {
        Graphics.ExecuteCommandBuffer(commandBuffer);
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
        //MakeRoom(39, 55, 39, 55, 39, 55);
        
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

    public void Unbuild()
    {
        ResetTransferability();
    }
    void ResetTransferability()
    {
        for(int i = 0; i < numNodes; i++)
        {
            transferability[i] = 1.000f;
        }
        transferBuffer.SetData(transferability);
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
        transferBuffer.Release();
        bufferWithArgs.Release();
    }
}
