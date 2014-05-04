using UnityEngine;
using UnityEditor;
using System;


namespace StateChartEditor{
    
    public static class MyTools{
        public static void drawLine(Vector2 startPos, Vector2 endPos){
            
            if ( startPos.x > endPos.x ){
                Vector2 temp = startPos;
                startPos = endPos;
                endPos = temp;
            }
            
            var distanceBetweenNodes = endPos.x - startPos.x;
                
            Handles.DrawBezier( 
                new Vector3(startPos.x, startPos.y),
                new Vector3(endPos.x, endPos.y),
                new Vector3(startPos.x + (distanceBetweenNodes / 2.0f) + 10.0f, startPos.y),
                new Vector3(endPos.x - (distanceBetweenNodes / 2.0f) + 10.0f, endPos.y),
                Color.black,
                null,
                3.25f);
        }
    }
    
    public static class RectExtensions
    {
        public static Vector2 TopLeft(this Rect rect)
        {
            return new Vector2(rect.xMin, rect.yMin);
        }
        public static Rect ScaleSizeBy(this Rect rect, float scale)
        {
            return rect.ScaleSizeBy(scale, rect.center);
        }
        public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
        {
            Rect result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale;
            result.xMax *= scale;
            result.yMin *= scale;
            result.yMax *= scale;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }
        public static Rect ScaleSizeBy(this Rect rect, Vector2 scale)
        {
            return rect.ScaleSizeBy(scale, rect.center);
        }
        public static Rect ScaleSizeBy(this Rect rect, Vector2 scale, Vector2 pivotPoint)
        {
            Rect result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale.x;
            result.xMax *= scale.x;
            result.yMin *= scale.y;
            result.yMax *= scale.y;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }
    }
}