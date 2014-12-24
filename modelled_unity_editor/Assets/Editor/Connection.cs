using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor{
    public class ConnectionPoint
    {
        public CanvasItem   canvas_item { get; private set;}
        public int          point_id { get; set;}

        public ConnectionPoint(CanvasItem canvas_item, int point_id)
        {
            this.canvas_item = canvas_item;
            this.point_id = point_id;
        }

        public Vector2 getPoint()
        {
            return this.canvas_item.getPoint(this.point_id);
        }
    }

    public class Connection
    {

        ConnectionPoint begin = null;
        ConnectionPoint end = null;
        List<Vector2> mid_points = new List<Vector2>();

        public Connection(CanvasItem begin_item)
        {
            this.begin = new ConnectionPoint(begin_item, 0);
        }

        public void setEndPoint(CanvasItem end_item)
        {
            this.end = new ConnectionPoint(end_item, 0);
        }
        /*
        private static void Initialize()
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
            
            Handles.DrawBezier( 
               new Vector3(start_pos.x, start_pos.y),
               new Vector3(end_pos.x, end_pos.y),
               new Vector3(start_pos.x + (distance_between_nodes / 2.0f) + 10.0f, start_pos.y),
               new Vector3(end_pos.x - (distance_between_nodes / 2.0f) + 10.0f, end_pos.y),
               Color.black,
               null,
               3.25f
            );
        }
        
        public void draw () 
        {
            Vector2 begin_point = this.begin.getPoint();
            Vector2 next_point;
            for (int i = 0; i < this.mid_points.Count; i++)
            {
                next_point = this.mid_points[i];
                Connection.drawLine(begin_point, next_point);
                begin_point = next_point;
            }
            if (this.end != null)
                Connection.drawLine(begin_point, this.end.getPoint());
        }
    }
}
