using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : MonoBehaviour {
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

    public float cell_size = 1.0f;

    public struct Cell {
        public int x;
        public int y;

        public Cell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object Obj)
        {
            Cell other = (Cell)Obj;
            return (this.x == other.x &&
                this.y == other.y);
        }

        public static bool operator ==(Cell cell1, Cell cell2)
        {
            return cell1.Equals(cell2);
        }

        public static bool operator !=(Cell cell1, Cell cell2)
        {
            return !(cell1 == cell2);
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + this.x.GetHashCode();
            hash = (hash * 7) + this.y.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return string.Format("Cell({0},{1})", this.x, this.y);
        }
    }

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

    public Cell calculateCell(Vector3 pos){
        return new Cell(Mathf.FloorToInt(pos.x/this.cell_size), Mathf.FloorToInt(pos.z/this.cell_size));
    }

    public Vector3 calculateCoords(Cell cell)
    {
        return new Vector3(this.cell_size * (cell.x + 0.5f), 0, this.cell_size * (cell.y+0.5f));
    }

    public Vector3 getNewExplore(Vector3 position, float tank_angle) {
        Cell cell = this.calculateCell(position);
        List<KeyValuePair<Cell, float>> successors = this.getSuccessors(cell);
        List<Cell> good_cells = new List<Cell>();
        float max_value = 0;
        foreach (KeyValuePair<Cell, float> pair in successors)
        {
            Cell successor = pair.Key;
            float diff_angle = Mathf.Abs(tank_angle - this.getAngleToDest(cell,successor));
            if (diff_angle > 180)
                diff_angle = 360-diff_angle;
            float value = (this.getSuccessors(successor)).Count;
            if (diff_angle <= 60)
                value += 7;
            else if(diff_angle <= 105)
                value += 4;
            if (value > max_value)
            {
                good_cells.Clear();
                good_cells.Add(successor);
                max_value = value;
            }
            else if (value == max_value)
            {
                good_cells.Add(successor);
            }
        }
        return this.calculateCoords(good_cells[Random.Range( 0, good_cells.Count)]);
    }

    public static float getAngleToDest(Cell cell_start, Cell cell_end)
    {
        float angle = Mathf.Atan2(cell_end.x-cell_start.x, cell_end.y-cell_start.y)*180 / Mathf.PI;
        if (angle < 0)
            angle += 360.0f;
        return angle;
    }

    public static float getAngleToDest(Vector3 start, Vector3 end)
    {
        float angle = Mathf.Atan2(end.x-start.x, end.z-start.z)*180 / Mathf.PI;
        if (angle < 0)
            angle += 360.0f;
        return angle;
    }

    List<KeyValuePair<Cell, float>> getSuccessors(Cell cell)
    {

        int i = 0;
        List<KeyValuePair<Cell, float>> successors = new List<KeyValuePair<Cell, float>>();
        for (int y = cell.y - 1; y <= cell.y+1; y++)
        {
            for (int x = cell.x - 1; x <= cell.x+1; x++)
            {
                i++;

                if (x < 0 || x >= this.map.GetLength(0) || y < 0 || y >= this.map.GetLength(1))
                    continue;
                if (i == 5)
                    continue;
                if (this.map[x,y] == 1)
                    continue;


                if(i == 1){
                    if (this.map[x+1,y] == 0 && this.map[x,y+1] == 0)
                        successors.Add(new KeyValuePair<Cell, float>(new Cell(x,y), 1.4f));
                }else if(i == 3){
                    if (this.map[x-1,y] == 0 && this.map[x,y+1] == 0)
                        successors.Add(new KeyValuePair<Cell, float>(new Cell(x,y), 1.4f));
                }else if(i == 7){
                    if (this.map[x+1,y] == 0 && this.map[x,y-1] == 0)
                        successors.Add(new KeyValuePair<Cell, float>(new Cell(x,y), 1.4f));
                }else if(i == 9){
                    if (this.map[x-1,y] == 0 && this.map[x,y-1] == 0)
                        successors.Add(new KeyValuePair<Cell, float>(new Cell(x,y), 1.4f));
                }
                else
                    successors.Add(new KeyValuePair<Cell, float>(new Cell(x,y), 1.0f));
            }
        }
        return successors;
    }


    private class PriorityQueue<Priority, Value>
    {
        private SortedDictionary<Priority, LinkedList<Value>> structure = new SortedDictionary<Priority, LinkedList<Value>>();
        
        public void push(Value value, Priority priority)
        {
            LinkedList<Value> value_list;
            if (!structure.TryGetValue(priority, out value_list))
            {
                value_list = new LinkedList<Value>();
                structure.Add(priority, value_list);
                //Debug.Log(string.Format("added key {0} with value {1}", priority, value));
            }
            value_list.AddLast(value);
        }
        
        public Value pop()
        {
            SortedDictionary<Priority, LinkedList<Value>>.KeyCollection.Enumerator iter = structure.Keys.GetEnumerator();
            iter.MoveNext();
            Priority key = iter.Current;
            LinkedList<Value> value_list = structure[key];
            Value result = value_list.First.Value;
            value_list.RemoveFirst();
            if (value_list.Count == 0){
                structure.Remove(key);
            }
            return result;
        }
        
        public bool IsEmpty
        {
            get { return structure.Count==0; }
        }
    }

    private class Node {
        public Cell cell;
        public Node parent;
        public float cost;

        public Node(Cell cell, Node parent , float cost)
        {
            this.cell = cell;
            this.parent = parent;
            this.cost = cost;
        }
    }

    public List<Vector3> calculatePath(Vector3 start, Vector3 destination)
    {
        Debug.Log(string.Format("calculating path from {0} ti {1}", start, destination));
        Cell start_cell = this.calculateCell(start);
        Cell destination_cell = this.calculateCell(destination);
        Debug.Log(string.Format("calculating path from {0} to {1}", start_cell, destination_cell));
        HashSet<Cell> explored = new HashSet<Cell>();
        PriorityQueue<float, Node> fringe = new PriorityQueue<float, Node>();
        Node start_node = new Node(start_cell, null, 0.0f);
        fringe.push(start_node, start_node.cost);
        Node current_node;
        while (true)
        {
            while (true)
            {
                current_node = fringe.pop();
                if (!explored.Contains(current_node.cell))
                    break;
            }
            explored.Add(current_node.cell);
            //Debug.Log(current_node.cell);
            if (current_node.cell == destination_cell)
                break;
            List<KeyValuePair<Cell, float>> successors = this.getSuccessors(current_node.cell);
            foreach (KeyValuePair<Cell, float> pair in successors)
            {
                Cell successor = pair.Key;
                float cost = pair.Value;
                float totalcost = current_node.cost + cost;
                if (!explored.Contains(successor))
                {
                    Node node = new Node(successor, current_node, totalcost);
                    float heuristic = ((successor.x - destination.x) * 2 + (successor.y - destination.y) * 2 ) * 0.5f;
                    fringe.push(node, totalcost+heuristic);
                }
            }
        }

        List<Vector3> new_points = new List<Vector3>();
        while ( current_node.cell != start_cell)
        {
            new_points.Insert(0, this.calculateCoords(current_node.cell));
            current_node = current_node.parent;
        }
        return new_points;
    }


	// Use this for initialization
	void Start () {
        /*Debug.Log(this.getAngleToDest(new Cell(1,1), new Cell(0,0)));
        Debug.Log(this.getAngleToDest(new Cell(0,0), new Cell(1,1)));
        Debug.Log(this.getAngleToDest(new Cell(0,0), new Cell(0,1)));
        Debug.Log(this.getAngleToDest(new Cell(0,1), new Cell(0,0)));
        Debug.Log(this.getAngleToDest(new Cell(1,0), new Cell(0,0)));*/
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
