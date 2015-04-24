using UnityEngine;
using System.Collections;
using System;

public class SCCDScript : MonoBehaviour {

    public string sccdcontroller_namespace;
    private sccdlib.GameLoopControllerBase controller;

    public string xml_file = "";

    void Awake () {
        this.controller = (sccdlib.GameLoopControllerBase) Activator.CreateInstance(Type.GetType(/*sccdcontroller_namespace +*/ "PlayerTank.Controller"), new object[]{this.gameObject});
    }

	void Start () {
        this.controller.start();
	}
	
	// Update is called once per frame
	void Update () {
        this.generateInputEvents();
        this.controller.update(Time.deltaTime);
	}

    void FixedUpdate() {
        this.controller.addInput(new sccdlib.Event("fixed-update", "engine", new object[] {
            Time.deltaTime
        }));
        this.controller.update(0);
    }

    void generateInputEventsForAxis(string axis, string negative_event_down, string positive_event_down, string negative_event_up, string positive_event_up)
    {
        if (Input.GetButtonDown(axis))
        {
            Debug.Log(Input.GetAxisRaw(axis));
            this.controller.addInput(new sccdlib.Event(Input.GetAxisRaw(axis) < 0 ? negative_event_down : positive_event_down, "input"));
        }
        if (Input.GetButtonUp(axis))
        {
            Debug.Log(Input.GetAxisRaw(axis));
            this.controller.addInput(new sccdlib.Event(Input.GetAxisRaw(axis) < 0 ? negative_event_up : positive_event_up, "input"));
        }
    }

    void generateInputEvents() {
        sccdlib.Event input_event;

        this.generateInputEventsForAxis("Horizontal", "left-pressed", "right-pressed", "left-released", "right-released");
        this.generateInputEventsForAxis("Vertical", "down-pressed", "up-pressed", "down-released", "up-released");
        this.generateInputEventsForAxis("ArrowHorizontal", "arrow-left-pressed", "arrow-right-pressed", "arrow-left-released", "arrow-right-released");
        this.generateInputEventsForAxis("ArrowVertical", "arrow-down-pressed", "arrow-up-pressed", "arrow-down-released", "arrow-up-released");

    }
}
