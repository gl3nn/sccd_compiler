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

        public SGUICanvas canvas    { get; private set; }
        
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

        /*private static void Initialize()
        {
            if (lineTex == null)
            {
                lineTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                lineTex.SetPixel(0, 1, Color.white);
                lineTex.Apply();
            }
            if (aaLineTex == null)
            {
                // TODO: better anti-aliasing of wide lines with a larger texture? or use Graphics.DrawTexture with border settings
                aaLineTex = new Texture2D(1, 3, TextureFormat.ARGB32, false);
                aaLineTex.SetPixel(0, 0, new Color(1, 1, 1, 0));
                aaLineTex.SetPixel(0, 1, Color.white);
                aaLineTex.SetPixel(0, 2, new Color(1, 1, 1, 0));
                aaLineTex.Apply();
            }
            
            // GUI.blitMaterial and GUI.blendMaterial are used internally by GUI.DrawTexture,
            // depending on the alphaBlend parameter. Use reflection to "borrow" these references.
            blitMaterial = (Material)typeof(GUI).GetMethod("get_blitMaterial", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            blendMaterial = (Material)typeof(GUI).GetMethod("get_blendMaterial", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
        }*/
        
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

            Handles.DrawBezier( 
               new Vector3(start_pos.x, start_pos.y),
               new Vector3(end_pos.x, end_pos.y),
               new Vector3(start_pos.x, start_pos.y),
               new Vector3(end_pos.x, end_pos.y),
               Color.black,
               null,
               3.25f
            );

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
                SGUICanvasEdge.drawLine(begin_point, this.end.getPosition());
        }
    }
}
