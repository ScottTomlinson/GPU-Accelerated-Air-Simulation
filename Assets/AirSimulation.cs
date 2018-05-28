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

    private int kernalOne = 0;
    private int kernalTwo = 0;
    private int kernalThree = 0;
    private int kernalFour = 0;
    private int kernalFive = 0;
    private int kernalSix = 0;
    private int kernalSeven = 0;
    private int kernalEight = 0;
    private int kernalNine = 0;

    [Tooltip("Number of frames to skip between dispatches")]
    public int getDataInterval = 10;
    private int updateCounter = 0;

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

        //command buffer
        commandBuffer = new CommandBuffer();

        commandBuffer.BeginSample("First Pass");
        commandBuffer.DispatchCompute(airShader, kernalOne, cubeSize, cubeSize, cubeSize);
        commandBuffer.EndSample("First Pass");

        commandBuffer.BeginSample("Second Pass");
        commandBuffer.DispatchCompute(airShader, kernalTwo, cubeSize, cubeSize, cubeSize);
        commandBuffer.EndSample("Second Pass");

        commandBuffer.BeginSample("Third Pass");
        commandBuffer.DispatchCompute(airShader, kernalThree, cubeSize, cubeSize, cubeSize);
        commandBuffer.EndSample("Third Pass");

        commandBuffer.BeginSample("Fourth Pass");
        commandBuffer.DispatchCompute(airShader, kernalFour, cubeSize, cubeSize, cubeSize);
        commandBuffer.EndSample("Fourth Pass");

        commandBuffer.BeginSample("Fifth Pass");
        commandBuffer.DispatchCompute(airShader, kernalFive, cubeSize, cubeSize, cubeSize);
        commandBuffer.EndSample("Fifth Pass");

        commandBuffer.BeginSample("Sixth Pass");
        commandBuffer.DispatchCompute(airShader, kernalSix, cubeSize, cubeSize, cubeSize);
        commandBuffer.EndSample("Sixth Pass");

        commandBuffer.BeginSample("Seventh Pass");
        commandBuffer.DispatchCompute(airShader, kernalSeven, cubeSize, cubeSize, cubeSize);
        commandBuffer.EndSample("Seventh Pass");

        commandBuffer.BeginSample("Eighth Pass");
        commandBuffer.DispatchCompute(airShader, kernalEight, cubeSize, cubeSize, cubeSize);
        commandBuffer.EndSample("Eighth Pass");

        commandBuffer.BeginSample("Ninth Pass");
        commandBuffer.DispatchCompute(airShader, kernalNine, cubeSize, cubeSize, cubeSize);
        commandBuffer.EndSample("Ninth Pass");

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
