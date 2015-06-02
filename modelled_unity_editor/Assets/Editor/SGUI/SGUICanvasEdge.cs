using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor{
    public class SGUICanvasConnectionPoint
    {
        public SGUICanvasElement    canvas_element  { get; private set;}
        public int                  point_id        { get; private set;}

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

        public int setClosest(Vector2 closest_to)
        {
            this.point_id = this.canvas_element.getClosestConnectionPoint(closest_to);
            return this.point_id;
        }
    }

    public class SGUICanvasEdge : SGUIWidget
    {
        public SGUICanvasConnectionPoint start      { get; private set;}
        public SGUICanvasConnectionPoint end        { get; private set;}
        public List<Vector2> control_points         { get; private set;}

        public SGUICanvas canvas                    { get; private set; }
        public string label                         { get; private set; }
        public Vector2 label_offset                 { get; private set; }
        private GUIStyle label_style                = "title";
        private static Texture2D arrow_texture      = GUI.skin.GetStyle("arrowhead").normal.background;
        private static Texture2D empty_texture      = GUI.skin.GetStyle("button").onNormal.background;
        
        public SGUICanvasEdge(SGUICanvasElement start_element, Vector2 closest_to)
        {
            this.start = new SGUICanvasConnectionPoint(start_element, closest_to);
            this.end = new SGUICanvasConnectionPoint(start_element, closest_to);
            this.canvas = start_element.canvas;
            this.init();
        }

        public SGUICanvasEdge(SGUICanvasElement start_element, int start_id, SGUICanvasElement end_element, int end_id)
        {
            this.start = new SGUICanvasConnectionPoint(start_element, start_id);
            this.end = new SGUICanvasConnectionPoint(end_element, end_id);
            this.canvas = start_element.canvas;
            this.init();
        }

        private void init()
        {
            this.canvas.addEdge(this);
            this.setLabel("");
            this.label_offset = new Vector2(0, 0);
            this.control_points = new List<Vector2>();
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
            if (text.Trim() == "")
                this.position = new Rect(this.position.x, this.position.y, 5, 5);
            else
            {
                Vector2 size = this.label_style.CalcSize(new GUIContent(this.label));
                this.position = new Rect(this.position.x, this.position.y, size.x, size.y);
            }
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
        
        public static void drawLine(List<Vector2> points_list)
        {
            for (int i = 1; i < points_list.Count; i++)
            {
                Handles.DrawAAPolyLine(
                    4.0f,
                    new Vector3(points_list[i-1].x , points_list[i-1].y),
                    new Vector3(points_list[i].x , points_list[i].y)
                );
            }
        }

        public static void drawArrow(Vector2 start_pos, Vector2 end_pos)
        {
            Matrix4x4 matrixBackup = GUI.matrix;
            float angle = Mathf.Atan2(end_pos.y - start_pos.y, end_pos.x - start_pos.x);
            GUIUtility.RotateAroundPivot((angle * 180 / Mathf.PI) + 90, end_pos);
            GUI.DrawTexture(new Rect(end_pos.x - 6.0f, end_pos.y, 12, 12), SGUICanvasEdge.arrow_texture);
            GUI.matrix = matrixBackup;
        }

        public void popControlPoint()
        {
            this.control_points.RemoveAt(this.control_points.Count - 1);
        }

        public void addControlPoint(Vector2 position)
        {
            this.control_points.Add(position);
        }

        public int getPointCount()
        {
            return this.control_points.Count;
        }

        private void calculatePosition()
        {
            Vector2 start = this.start.getPosition();
            Vector2 end = this.end.getPosition();
            this.setCenter(
                new Vector2(
                    ((start.x + end.x) / 2) + this.label_offset.x,
                    ((start.y + end.y) / 2) + this.label_offset.y
                )
            );
        }

        protected override void OnGUI()
        {
            List<Vector2> point_list = new List<Vector2>(){this.start.getPosition()};
            for (int i = 0; i < this.control_points.Count; i++)
                point_list.Add(this.control_points[i]);

            if (this.end != null)
            {
                point_list.Add(this.end.getPosition());

                this.calculatePosition();
                this.catchMouseDefault();

                if (this.label.Trim() == "")
                    GUI.DrawTexture(this.position, SGUICanvasEdge.empty_texture, ScaleMode.StretchToFill);
                else
                    GUI.Label(this.position, this.label, this.label_style);
            }

            if (point_list.Count >= 2)
            {
                Handles.color = Color.black;
                SGUICanvasEdge.drawLine(point_list);
                SGUICanvasEdge.drawArrow(point_list[point_list.Count-2], point_list[point_list.Count -1]);
            }

        }
    }
}
