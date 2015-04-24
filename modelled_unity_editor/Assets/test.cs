using UnityEngine;
using System.Collections;
using System;

public class test : MonoBehaviour {

    public string sccdcontroller_namespace;
    private sccdlib.GameLoopControllerBase controller;

    void Awake () {
        this.controller = (sccdlib.GameLoopControllerBase) Activator.CreateInstance(Type.GetType(sccdcontroller_namespace + ".Controller"), this.gameObject);
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
        this.controller.update();
    }

    void generateInputEvents() {

    }
}
