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
			// reg delegates
			KCoopNotifyInfoString sceneChangedtoKSC = new KCoopNotifyInfoString(
				notifyInfoTypeString.SceneChange, notifyInfoString.Any, notifyInfoString.SpaceCentre);
			KCoopNotifyInfoString sceneChangedoutKSC = new KCoopNotifyInfoString(
				notifyInfoTypeString.SceneChange, notifyInfoString.SpaceCentre, notifyInfoString.Any);
			kcoop_model.registerDelegate(sceneChangedtoKSC, sceneChangedin);
			kcoop_model.registerDelegate(sceneChangedoutKSC, sceneChangedout);

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

		public void Awake()
		{
			if (kcoop_model == null) 
			{
				kcoop_model = KerbalCooperation.Instance;
				return;
			}

			kcoop_model.refreshScene();
		}

		// scene changed into KSC
		public void sceneChangedin(KCoopNotifyInfoString info)
		{
			kcoop_model.KSCtimerStart();
		}

		// scene changed to Any from KSC
		public void sceneChangedout(KCoopNotifyInfoString info)
		{
			kcoop_model.KSCtimerpause();
		}
    }
}
