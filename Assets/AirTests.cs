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
    }

    void AddAirToTestPoint()
    {
        airSim.AddAirAtPoint((int)testPoints[testIndex].x, (int)testPoints[testIndex].y, (int)testPoints[testIndex].z, testAmount);
    }

	// Update is called once per frame
	void Update () {
		
	}
}
