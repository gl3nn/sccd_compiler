using UnityEngine;
using System.Collections;

public class Bullet: MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate() {
        this.transform.Translate(Vector3.forward * 0.1f);
    }

    void OnCollisionEnter()
    {
        Destroy(this.gameObject);
    }
}
