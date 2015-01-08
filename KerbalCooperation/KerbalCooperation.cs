using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KCoop
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class KerbalCooperation : MonoBehaviour
    {
        private static bool flag_Initialized = false;
		private Dictionary<KCoopNotifyInfoString, KCoopNotifyDelegate> DelMappings = new Dictionary<KCoopNotifyInfoString, KCoopNotifyDelegate>();
        private KCoopNotifyInfoString notifyInfo = null;

        public static KerbalCooperation Instance { get; private set; }

        private void Initialize()
        {
            Instance = this;
            UnityEngine.Object.DontDestroyOnLoad(this);
            if (notifyInfo == null)
            {
                notifyInfo = new KCoopNotifyInfoString(notifyInfoTypeString.SceneChange,
                    KCoopNotifyInfoString.currentScene(), notifyInfoString.None);
            }
			DelMappings.Clear();
			flag_Initialized = true;
        }

        public void Start()
        {
            Logger.log("KerbalCooperation Initializing.");
            Initialize();
            Logger.log("KerbalCooperation Initialized.");
        }

		/*
        public void Awake()
        {
            if (!flag_Initialized)
            {
				// avoid first time Awake for Start
                return;
            }
            Logger.log("KerbalCooperation Awake.");

        }
        */

		#region MVC pattern
		/// <summary>
		/// MVC pattern with methods
		/// </summary>
		public void refreshScene()
		{
			notifyInfo.InfoType = notifyInfoTypeString.SceneChange;
			notifyInfo.toScene = KCoopNotifyInfoString.currentScene();
		} 

		public void registerDelegate(KCoopNotifyInfoString info, KCoopNotifyDelegate del)
		{
			DelMappings.Add(info, del);
		}

		public void notifyAll()
		{
			foreach (var mapping in DelMappings) 
			{
				if (mapping.Key.match(notifyInfo))
				{
					KCoopNotifyDelegate del = mapping.Value;
					del(notifyInfo);
				}
			}
		}

		#endregion
    }
}
