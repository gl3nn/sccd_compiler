using UnityEngine;
using System.Collections;

public class LevelScript : MonoBehaviour {
    public Transform wall_prefab;

    private int[,] map = new int[12, 20]{
        {0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1},
        {0,0,0,0,1,0,0,0,0,0,0,1,1,1,1,1,1,1,0,1},
        {0,0,0,1,1,1,0,0,0,0,0,0,1,1,1,1,1,0,0,0},
        {0,0,1,1,1,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0},
        {0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
        {0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0},
        {0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,1,1,0,0},
        {0,0,0,0,0,0,0,1,1,1,1,0,0,0,0,1,1,1,0,0},
        {0,0,0,0,0,0,0,1,1,1,1,1,0,0,0,1,0,0,0,0},
        {0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0}
    };

    void Awake () {
        for(int row = 0; row < map.GetLength(0); row++)
        {
            for(int column = 0; column < map.GetLength(1); column++)
            {
                if (map[row,column] == 1)
                {
                    Transform wall = GameObject.Instantiate(wall_prefab, new Vector3(0.5f+row, 0.5f, 0.5f+column), Quaternion.identity) as Transform;
                    wall.transform.parent = this.transform;
                }
            }
        }
    }

	// Use this for initialization
	void Start () {


	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
