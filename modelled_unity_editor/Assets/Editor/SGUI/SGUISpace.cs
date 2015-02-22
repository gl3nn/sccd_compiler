using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SCCDEditor
{
    public class SGUISpace: SGUIWidget
    {
        public SGUISpace()
        {
        }
        
        protected override void OnGUI()
        {
            GUILayout.FlexibleSpace();
        }

    }
}