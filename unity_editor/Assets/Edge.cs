using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;


namespace StateChartEditor{
    
    public class Edge
    {
        public Node start { get; private set; }
        public Node end{ get; private set; }
    
        public Edge(Node start, Node end){
            this.start = start;
            this.end = end;
        }
        
        public void draw(){
            MyTools.drawLine(start.getPos(), end.getPos());
        }
    }
    
}