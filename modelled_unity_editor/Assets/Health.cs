using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {

    public int health = 5;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

    void OnCollisionEnter(Collision collision)
    {
        Bullet bullet = collision.gameObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            //hit by a bullet
            if (this.gameObject.tag != collision.gameObject.tag)
            {
                this.health -= 1;
                if (this.health <= 0)
                    Destroy(this.gameObject);
            }
        }

    }

    void OnGUI() {
        Vector3 screen_pos = Camera.main.WorldToScreenPoint(this.gameObject.transform.position);
        GUI.color = Color.magenta;
        GUI.Label(new Rect(screen_pos.x - 5, (Screen.height - screen_pos.y - 40), 20, 20), this.health.ToString());
    }
}
