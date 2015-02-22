using UnityEngine;
using System;
using System.Collections;

public class SCCDScript : MonoBehaviour
{
    public string xml_file = "";
    public string namespace_name = "";
    private sccdlib.GameLoopControllerBase controller = null;

    public SCCDScript () 
    {
    }

	// Use this for initialization
	void Start () {
        Type controller_type = Type.GetType(namespace_name + ".Controller");
        if (controller_type != null)
        {
            sccdlib.GameLoopControllerBase controller = (sccdlib.GameLoopControllerBase)Activator.CreateInstance(controller_type);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
