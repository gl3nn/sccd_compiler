using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SCCDEditor{
    public class StateChartEditorWindow : SGUIEditorWindow
    {

        private static Dictionary<XElement, StateChartEditorWindow> windows = new Dictionary<XElement, StateChartEditorWindow>();
        public XElement statechart_xml {get; private set;}

        public static StateChartEditorWindow getWindow(XElement statechart_xml)
        {
            StateChartEditorWindow window;
            StateChartEditorWindow.windows.TryGetValue(statechart_xml, out window);
            return window;
        }

        public static StateChartEditorWindow createOrFocus(XElement statechart_xml)
        {
            StateChartEditorWindow window = StateChartEditorWindow.getWindow(statechart_xml);
            if (window == null)
            {
                window = CreateInstance<StateChartEditorWindow>();
                window.wantsMouseMove = true;
                //window.title = title;
                UnityEngine.Object.DontDestroyOnLoad(window);
                StateChartEditorWindow.windows[statechart_xml] = window;
                window.statechart_xml = statechart_xml;
                window.init();
                window.Show();
            } else
            {
                window.Focus();
            }
            //StateChartEditorWindow window = EditorWindow.GetWindow<StateChartEditorWindow>(new System.Type[]{typeof(ClassDiagramEditorWindow)});
            return window;
        }

        private void init()
        {
            this.top_level_widget = new SGUITopLevel(this);
            this.controller = new StateChartEditor.Controller(this.top_level_widget, this.statechart_xml);
            this.controller.start();
        }

        public void OnDestroy()
        {
            StateChartEditorWindow.windows.Remove(this.statechart_xml);
        }
    }
}