using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace SCCDEditor{

    public abstract class MyPopupWindowContent
    {
        public MyPopupWindow popup_window;
        public abstract void drawContent();
    }

    public class MyPopupWindow : EditorWindow
    {

        private MyPopupWindowContent window_content;

        public static void create(MyPopupWindowContent content, string title, float pos_x, float pos_y)
        {
            MyPopupWindow popup = MyPopupWindow.CreateInstance<MyPopupWindow>();
            popup.window_content = content;
            content.popup_window = popup;
            popup.title = "StateChart Editor";
            popup.ShowPopup();
            popup.position = new Rect(pos_x, pos_y, 0, 0);
            popup.minSize = new Vector2(20, 10);
            popup.Focus();
        }
        
        private void OnGUI()
        {
            //GUI.skin = (GUISkin) (Resources.LoadAssetAtPath("Assets/Editor/SCCDSkin.guiskin", typeof(GUISkin)));
            GUIStyle popup_style = GUI.skin.GetStyle("PopupBackground");
            GUILayout.BeginVertical("box");
            this.window_content.drawContent();
            GUILayout.EndVertical();
            if (Event.current.type == EventType.Repaint)
            {
                Rect last_rect = GUILayoutUtility.GetLastRect();
                Debug.Log(last_rect);
                last_rect.x = this.position.x;
                last_rect.y = this.position.y;
                last_rect.height += popup_style.border.vertical;
                last_rect.width += popup_style.border.horizontal;
                this.position = last_rect;
            }
        }

        public void OnLostFocus() {
            this.Focus();
        }
    }

}
