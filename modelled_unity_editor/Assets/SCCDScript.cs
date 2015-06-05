using UnityEngine;
using System.Collections;
using System;

public class SCCDScript : MonoBehaviour {

    public string sccd_namespace;
    private sccdlib.GameLoopControllerBase controller;

    public string xml_file = "";

    void Awake () {
        this.controller = (sccdlib.GameLoopControllerBase) Activator.CreateInstance(Type.GetType(string.Format("{0}.Controller", this.sccd_namespace)), new object[]{this.gameObject});
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

    void inputKey(KeyCode key, string pressed_event, string relesed_event)
    {
        if (Input.GetKeyDown(key))
            this.controller.addInput(new sccdlib.Event(pressed_event, "input"));
        if (Input.GetKeyUp(key))
            this.controller.addInput(new sccdlib.Event(relesed_event, "input"));
    }

    void generateInputEvents() {
        this.inputKey(KeyCode.W, "up-pressed", "up-released");
        this.inputKey(KeyCode.S, "down-pressed", "down-released");
        this.inputKey(KeyCode.A, "left-pressed", "left-released");
        this.inputKey(KeyCode.D, "right-pressed", "right-released");
        this.inputKey(KeyCode.LeftArrow, "arrow-left-pressed", "arrow-left-released");
        this.inputKey(KeyCode.RightArrow, "arrow-right-pressed", "arrow-right-released");
        this.inputKey(KeyCode.UpArrow, "arrow-up-pressed", "arrow-up-released");
    }
}
