using UnityEngine;
using System.Collections.Generic;

namespace SCCDEditor{
    public abstract class GUICanvasBase : GUIWidget
    {
        public    GUICanvas                 canvas      { get; protected set; }
        protected List<GUICanvasElement>    elements    { get; private set; }

        public GUICanvasBase()
        {
            this.elements = new List<GUICanvasElement>();
        }

        public void addElement(GUICanvasElement element)
        {
            if (!this.elements.Contains(element))
            {
                this.elements.Add(element);
                element.setParent(this);
            }
        }
        
        public void removeElement(GUICanvasElement element)
        {
            this.elements.Remove(element);
            element.setParent(null);
        }

        public void pushChildToFront(GUICanvasElement element) 
        {
            if (this.elements.Remove(element))
                this.elements.Add(element);
        }
 
        public virtual void move(Vector2 delta)
		{
			for (int i=0; i < this.elements.Count; i++)
            {
                this.elements[i].move(delta);
            }
		}

        protected override void OnGUI()
        {
            for (int i=0; i < this.elements.Count; i++)
            {
                this.elements[i].doOnGUI();
            }
        }
        
        public Rect calculateContainer(Rect minimum, float margin = 0.0f)
        {
            float x_min = minimum.xMin + margin;
            float y_min = minimum.yMin + margin;
            float x_max = minimum.xMax - margin;
            float y_max = minimum.yMax - margin;
            foreach(GUIWidget element in this.elements)
            {
                if (element.position.xMin < x_min) x_min = element.position.xMin;
                if (element.position.yMin < y_min) y_min = element.position.yMin;
                if (element.position.xMax > x_max) x_max = element.position.xMax;
                if (element.position.yMax > y_max) y_max = element.position.yMax;
            }
            return new Rect (x_min - margin, y_min - margin, x_max-x_min + 2 * margin, y_max-y_min + 2 * margin);
        }
    }
}
