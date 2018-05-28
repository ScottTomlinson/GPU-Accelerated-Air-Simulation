using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AirTests : MonoBehaviour {

    private AirSimulation airSim;
    private Button startTestButton;
    public Texture buttonTexture;
    public float testAmount;
    public Vector3[] testPoints;
    private int testIndex = 0;
    private string userInput = "";
    private string testOutput = "Test Output Data Here";

    

	// Use this for initialization
	void Start () {
        airSim = this.gameObject.transform.GetComponent<AirSimulation>();
	}
	
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 50, 100, 30), "Insert Air"))
        {
            AddAirToTestPoint();
            //AddAirToRandomtestPoints();
        }
        if(GUI.Button(new Rect(10,90,100,30), "Check Levels"))
        {
            CheckAirLevels();
        }
        if (GUI.Button(new Rect(10, 170, 100, 30), "Dispatch"))
        {
            airSim.DispatchSim();
        }
        if (GUI.Button(new Rect(10, 210, 100, 30), "Build"))
        {
            airSim.Build();
        }
        if (GUI.Button(new Rect(10, 250, 100, 30), "Unbuild"))
        {
            airSim.Unbuild();
        }
        userInput = GUI.TextField(new Rect(10, 10, 400, 30), testOutput, 100);
    }

    void CheckAirLevels()
    {
        float tMass = airSim.GetTotalMass();
        float aMass = airSim.GetAddedMass();
        float e = (aMass - tMass) / aMass * -10000;
        e = Mathf.RoundToInt(e);
        e /= 100;
        testOutput = "Current Mass: " + tMass + "  |  Added Mass: " + aMass + "  |  Error: " + e + "%";
        Debug.Log(testOutput);
    }

    void AddAirToTestPoint()
    {
        airSim.AddAirAtPoint((int)testPoints[testIndex].x, (int)testPoints[testIndex].y, (int)testPoints[testIndex].z, testAmount);
        testIndex++;
        if(testIndex >= testPoints.Length)
        {
            testIndex = 0;
        }
    }

    void AddAirToRandomtestPoints()
    {
        for (int i = 0; i < 10; i++)
        {
            airSim.AddAirAtPoint(RNG.Ints(0, 96), RNG.Ints(0, 96), RNG.Ints(0, 96), RNG.Floats(0, 1) * testAmount);
        }
    }
}
