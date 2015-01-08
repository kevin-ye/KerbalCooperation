using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using window = ReeperCommon.Window;

namespace KCoop
{
    class KCoopSpaceCenterWindow: window.DraggableWindow
    {
        private SpaceCenterManager parent = null;
        protected override Rect Setup()
        {
            Logger.log("SpaceCenterWindow setup.");

            return new Rect(300, 300, 300, 300);
        }

        protected override void DrawUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("test SpaceCenterManager.DrawUI");
            GUILayout.EndVertical();
        }

        protected override void OnCloseClick()
        {
            Logger.log("SpaceCenterWindow OnCloseClick.");
        }

        public void setParent(SpaceCenterManager manager)
        {
            parent = manager;
        }

		public void notify(KCoopNotifyInfoString info)
        {

        }
    }
}
