using System;
using System.Collections.Generic;
using System.Linq;
using KSP;
using UnityEngine;

namespace KCoop
{
	public class KCoopSpaceCenterManager: MonoBehaviour
    {
        private ApplicationLauncherButton button = null;
        private KCoopSpaceCenterWindow gui = null;
		private KerbalCooperation kcoop_model = null;

        private void onGUIApplicationLauncherDestroyed()
        {
            if (button != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(this.button);
                button = null;
            }
        }

        private void onGUIApplicationLauncherReady()
        {
            try
            {
                if (ApplicationLauncher.Ready)
                {
                    button = ApplicationLauncher.Instance.AddModApplication(
                        buttonClick,
                        buttonClick,
                        null, null, null, null,
                        ApplicationLauncher.AppScenes.ALWAYS,
                        GameDatabase.Instance.GetTexture("KerbalCooperation/Resource/ToolbarIcon", false)
                        );
                    Logger.log("Button created");
                }
            }
            catch
            {
                Logger.exception("Failed to create button");
            }
        }

        private void OnDestroy()
        {
			Logger.log("SpaceCenterManager OnDestroy");
            GameEvents.onGUIApplicationLauncherReady.Remove(onGUIApplicationLauncherReady);
            if (button != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(button);
            }
        }

        public void Start()
        {
            Logger.log("SpaceCenterManager started.");
            kcoop_model = KerbalCooperation.Instance;
			if (kcoop_model == null) 
			{
				Logger.error ("Cannot find KerbalCooperation instance!");
				return;
			}

            GameEvents.onGUIApplicationLauncherReady.Add(this.onGUIApplicationLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(this.onGUIApplicationLauncherDestroyed);

        }

		public void Awake()
		{
			if (kcoop_model == null) 
			{
				kcoop_model = KerbalCooperation.Instance;
				Logger.error ("Cannot find KerbalCooperation instance!");
				return;
			}
		}

        public void buttonClick()
        {
			Logger.log("KCoopSpaceCenterManager Button clicked.");
            if (gui == null)
            {
				gui = gameObject.AddComponent<KCoopSpaceCenterWindow> () as KCoopSpaceCenterWindow;
				gui.setParent(this);
            }
            else
            {
                gui.Visible = !gui.Visible;
            }
        }
    }
}
