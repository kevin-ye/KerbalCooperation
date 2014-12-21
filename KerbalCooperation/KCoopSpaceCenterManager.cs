using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KCoop
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    class SpaceCenterManager: MonoBehaviour
    {
        private ApplicationLauncherButton button = null;
        private KCoopSpaceCenterWindow gui = null;
        public KerbalCooperation kcoop_model = null;

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
            GameEvents.onGUIApplicationLauncherReady.Remove(onGUIApplicationLauncherReady);
            if (button != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(button);
            }
            Logger.log("SpaceCenterManager OnDestroy");
        }

        public void Start()
        {
            Logger.log("SpaceCenterManager started.");
            kcoop_model = KerbalCooperation.Instance;
            GameEvents.onGUIApplicationLauncherReady.Add(this.onGUIApplicationLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(this.onGUIApplicationLauncherDestroyed);
        }

        public void buttonClick()
        {
            Logger.log("SpaceCenterManager Button clicked.");
            if (gui == null)
            {
                gui = new GameObject("SpaceCenterManager.gui").AddComponent<KCoopSpaceCenterWindow>();
                gui.setParent(this);
            }
            else
            {
                gui.Visible = !gui.Visible;
            }
        }
    }
}
