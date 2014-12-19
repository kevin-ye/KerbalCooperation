using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KCoop
{
    class SpaceCenterManager: MonoBehaviour
    {
        private ApplicationLauncherButton button;

        public SpaceCenterManager()
        {
            Logger.log("SpaceCenterManager constructor.");
            GameEvents.onGUIApplicationLauncherReady.Add(this.onGUIApplicationLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(this.onGUIApplicationLauncherDestroyed);
        }

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
                        null,
                        null,
                        null, null, null, null,
                        ApplicationLauncher.AppScenes.ALWAYS,
                        GameDatabase.Instance.GetTexture("KSP_Timer/Resource/toolbarButton", false)
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
    }
}
