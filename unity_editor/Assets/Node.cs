using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

namespace StateChartEditor
{
    
    public class Node
    {       
        String name;
        List<Edge> out_edges = new List<Edge>();
        List<Edge> in_edges = new List<Edge>();

        public Rect rect { get; private set; }
        public bool selected { get; private set; } 
        
        public Node(Vector2 position, String name)
        {
            this.selected = false;
            var rect = new Rect(0,0,100,100);
            rect.center = position;
            this.rect = rect;
            this.name = name;
        }
        
        public Vector2 getPos()
        {
            return this.rect.center;    
        }
        
        public void draw()
        {
            if(this.selected)
            {
                var old_color = GUI.backgroundColor;
                GUI.backgroundColor = Color.Lerp(GUI.backgroundColor,Color.green,0.5f);
                GUI.Box(this.rect, name);    
                GUI.backgroundColor = old_color;
            }
            else
            {
                GUI.Box(this.rect, name);    
            }
            /*
            IOPositions = new Vector2[] {    new Vector2(rect.center.x        ,rect.y                    ),
                                            new Vector2(rect.x + rect.width    ,rect.center.y            ),
                                            new Vector2(rect.center.x        ,rect.y + rect.height    ),
                                            new Vector2(rect.x                ,rect.center.y            )};
            */
        }
        
        public void setSelected()
        {
            this.selected = true;
        }
        
        public bool checkMouseOver(Vector2 mousePosition)
        {
            if(rect.Contains(mousePosition)){
                return true;
            }
            return false;
        }
        
        public void move(Vector2 offset)
        {
            this.rect = new Rect(this.rect.x + offset [0], this.rect.y + offset [1], this.rect.width, this.rect.height);
        }
        
        public void unSelect()
        {
            this.selected = false;    
        }
        
        public void addInput(Edge edge)
        {
            in_edges.Add (edge);
        }
        
        public void addOutput(Edge edge)
        {
            out_edges.Add (edge);
        }
    }
}