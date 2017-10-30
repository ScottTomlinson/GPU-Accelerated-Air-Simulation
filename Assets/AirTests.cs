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

	// Use this for initialization
	void Start () {
        airSim = this.gameObject.transform.GetComponent<AirSimulation>();
	}
	
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Insert Air"))
        {
            AddAirToTestPoint();
        }
        if(GUI.Button(new Rect(10,50,100,30), "Check Levels"))
        {
            CheckAirLevels();
        }
        if (GUI.Button(new Rect(10, 90, 100, 30), "Open Hole"))
        {
            airSim.OpenHole();
        }
    }

    void CheckAirLevels()
    {
        Debug.Log(airSim.GetTotalVolume());
    }

    void AddAirToTestPoint()
    {
        airSim.AddAirAtPoint((int)testPoints[testIndex].x, (int)testPoints[testIndex].y, (int)testPoints[testIndex].z, testAmount);
    }
}
