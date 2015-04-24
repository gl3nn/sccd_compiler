using UnityEngine;
using System.Collections;

public class PlayerTank : MonoBehaviour {
    Transform turret;
    Transform barrell;
    Transform bullet_spawn;
    float shoot_time = 0;
    public Transform bullet_prefab;

	// Use this for initialization
	void Start () {
        this.turret = this.transform.FindChild("turret");
        this.barrell = this.turret.FindChild("barrell");
        this.bullet_spawn = this.turret.FindChild("bullet_spawn");
	}

    void Update() {
        shoot_time -= Time.deltaTime;
        if (Input.GetAxis("ArrowVertical") == 1 && shoot_time <= 0)
        {
            shoot_time = 1;
            Transform bullet = GameObject.Instantiate(bullet_prefab, this.bullet_spawn.position, this.barrell.rotation * Quaternion.Euler(-90, 0, 0)) as Transform;
        }
    }
	
	void FixedUpdate () {
        this.transform.Translate(Vector3.forward * 0.05f * Input.GetAxis("Vertical"));
        this.transform.Rotate(0, Input.GetAxis("Horizontal") * 3, 0);
        this.turret.transform.RotateAround(this.transform.position, Vector3.up, Input.GetAxis("ArrowHorizontal") * 3);
	}
}
