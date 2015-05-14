using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor{
    public class SGUICanvasConnectionPoint
    {
        public SGUICanvasElement   canvas_element { get; private set;}
        public int          point_id { get; set;}

        public SGUICanvasConnectionPoint(SGUICanvasElement canvas_element, int point_id)
        {
            this.canvas_element = canvas_element;
            this.point_id = point_id;
        }

        public SGUICanvasConnectionPoint(SGUICanvasElement canvas_element, Vector2 closest_to)
        {
            this.canvas_element = canvas_element;
            this.setClosest(closest_to);
        }

        public Vector2 getPosition()
        {
            return this.canvas_element.getConnectionPointPosition(this.point_id);
        }

        /*public Vector2 getClosest(Vector2 mouse)
        {
            return(mouse);
        }
        */

        public void setClosest(Vector2 closest_to)
        {
            this.point_id =  this.canvas_element.getClosestConnectionPoint(closest_to);
        }
    }

    public class SGUICanvasEdge : SGUIWidget
    {
        SGUICanvasConnectionPoint start = null;
        SGUICanvasConnectionPoint end = null;
        public List<Vector2> control_points = new List<Vector2>();

        public SGUICanvas canvas        { get; private set; }
        public string label             { get; private set; }
        private Vector2 label_offset    = new Vector2(0, 0);
        private GUIStyle label_style    = "title";
        
        public SGUICanvasEdge(SGUICanvasElement start_element, Vector2 closest_to)
        {
            this.start = new SGUICanvasConnectionPoint(start_element, closest_to);
            this.end = new SGUICanvasConnectionPoint(start_element, closest_to);
            this.canvas = start_element.canvas;
            this.canvas.addEdge(this);
        }

        public void adjustEndPoint(Vector2 closest_to)
        {
            this.end.setClosest(closest_to);
        }

        public void createEndPoint(SGUICanvasElement end_item, Vector2 closest_to)
        {
            this.end = new SGUICanvasConnectionPoint(end_item, closest_to);
        }

        public void removeEndPoint()
        {
            this.end = null;
        }

        public void delete()
        {
            this.canvas.removeEdge(this);
        }

        public void setLabel(string text)
        {
            this.label = text;
            Vector2 size = this.label_style.CalcSize(new GUIContent(this.label));
            this.position = new Rect(this.position.x, this.position.y, size.x, size.y);
        }

        public void resetLabelStyle()
        {
            this.label_style = "title";
        }
        
        public void setLabelStyle(GUIStyle label_style)
        {
            this.label_style = label_style;
        }

        public void moveLabel(Vector2 delta)
        {
            this.label_offset = new Vector2(this.label_offset.x + delta.x, this.label_offset.y + delta.y);
        }
        
        public static void drawLine(Vector2 start_pos, Vector2 end_pos)
        {       
            if ( start_pos.x > end_pos.x ){
                Vector2 temp = start_pos;
                start_pos = end_pos;
                end_pos = temp;
            }
            
            var distance_between_nodes = end_pos.x - start_pos.x;

            //float rico = (end_pos.y - start_pos.y) / (end_pos.x - start_pos.x);
            
            /*Handles.DrawBezier( 
               new Vector3(start_pos.x, start_pos.y),
               new Vector3(end_pos.x, end_pos.y),
               new Vector3(start_pos.x + (distance_between_nodes / 2.0f) + 10.0f, start_pos.y),
               new Vector3(end_pos.x - (distance_between_nodes / 2.0f) + 10.0f, end_pos.y),
               Color.black,
               null,
               3.25f
            );*/

            /*Handles.DrawAAPolyLine(
                new Vector3(start_pos.x, start_pos.y),
                new Vector3(end_pos.x, end_pos.y)
            );*/

            Handles.color = Color.black;

            Handles.DrawAAPolyLine(
                4.0f,
                new Vector3(start_pos.x, start_pos.y),
                new Vector3(end_pos.x, end_pos.y)
            );
            /*
            Handles.DrawBezier( 
               new Vector3(start_pos.x, start_pos.y),
               new Vector3(end_pos.x, end_pos.y),
               new Vector3(start_pos.x, start_pos.y),
               new Vector3(end_pos.x, end_pos.y),
               Color.black,
               null,
               3.25f
            );*/

            /*Handles.DrawLine(
                new Vector3(start_pos.x, start_pos.y),
                new Vector3(end_pos.x, end_pos.y)
            );*/

        }

        public void popControlPoint()
        {
            this.control_points.RemoveAt(this.control_points.Count - 1);
        }

        public void addControlPoint(Vector2 position)
        {
            this.control_points.Add(position);
        }

        public bool hasControlPoints()
        {
            return this.control_points.Count > 1;
        }

        private void calculatePosition()
        {
            float x = (this.start.canvas_element.position.center.x + this.end.canvas_element.position.center.x) / 2;
            float y = (this.start.canvas_element.position.center.y + this.end.canvas_element.position.center.y) / 2;
            this.setCenter(new Vector2(x + this.label_offset.x, y + this.label_offset.y));
        }

        protected override void OnGUI()
        {
			Vector2 begin_point = this.start.getPosition ();
            Vector2 next_point;
            for (int i = 0; i < this.control_points.Count; i++)
            {
                next_point = this.control_points[i];
                SGUICanvasEdge.drawLine(begin_point, next_point);
                begin_point = next_point;
            }
            if (this.end != null)
            {
                SGUICanvasEdge.drawLine(begin_point, this.end.getPosition());
                this.calculatePosition();
                this.catchMouseDefault();
                GUI.Label(this.position, this.label, this.label_style);

                Texture2D arrow_texture = GUI.skin.GetStyle("arrowhead").normal.background;
                Vector2 end_pos = this.end.getPosition();
                Matrix4x4 matrixBackup = GUI.matrix;
                float angle = Mathf.Atan2(end_pos.y-begin_point.y, end_pos.x-begin_point.x);
                //Vector2 angle_vector = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                //end_pos.x += angle_vector.x;
                //end_pos.y += angle_vector.y;
                GUIUtility.RotateAroundPivot((angle * 180 / Mathf.PI) + 90, end_pos);
                GUI.DrawTexture(new Rect(end_pos.x - 6.0f, end_pos.y, 12, 12), arrow_texture);
                GUI.matrix = matrixBackup;
            }

        }
    }
}
