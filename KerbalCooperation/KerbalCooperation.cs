using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KCoop
{
	public static class KCoopVersion
	{
		public static readonly string name = "KerbalCooperation";
		public static readonly string version = "0.1";
	}

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class KCoopScenarioLoader: MonoBehaviour
	{
		public static KerbalCooperation _instance = null;

		public KCoopScenarioLoader()
		{
			_instance = new KerbalCooperation();
		}

	}



	public class KerbalCooperation : ScenarioModule
    {
        private static bool flag_Initialized = false;
		private Dictionary<KCoopNotifyInfoString, KCoopNotifyDelegate> DelMappings = new Dictionary<KCoopNotifyInfoString, KCoopNotifyDelegate>();
        private KCoopNotifyInfoString notifyInfo = null;
		private readonly string saveNodeName = "KCoop";
		private bool newSave = true;
		private List<ISaveAble> SaveAbleList = new List<ISaveAble>();
		private KCoopTimer KSCTimer;

        public static KerbalCooperation Instance { get; private set; }

        private void Initialize()
        {
            Instance = this;
			SaveAbleList.Clear ();
			KSCTimer = new KCoopTimer("KSCtimer", false);
			SaveAbleList.Add(KSCTimer);

            UnityEngine.Object.DontDestroyOnLoad(this);
            if (notifyInfo == null)
            {
                notifyInfo = new KCoopNotifyInfoString(notifyInfoTypeString.SceneChange,
                    KCoopNotifyInfoString.currentScene(), notifyInfoString.None);
            }
			DelMappings.Clear();
			flag_Initialized = true;
        }

		public KerbalCooperation()
		{
			Logger.log("KerbalCooperation Initializing.");
			Initialize();
			Logger.log("KerbalCooperation Initialized.");
		}

		public override void OnLoad(ConfigNode gameNode)
		{
			base.OnLoad(gameNode);
			newSave = !(gameNode.HasNode (saveNodeName));
			if (!newSave) {
				// load save
				ConfigNode saveNode = gameNode.GetNode (saveNodeName);
				foreach (ISaveAble i in SaveAbleList) {
					i.Load (saveNode);
				}
			}
		}

		public override void OnSave(ConfigNode gameNode)
		{
			base.OnSave(gameNode);
			if (!newSave) {
				ConfigNode saveNode = gameNode.GetNode (saveNodeName);
				foreach (ISaveAble i in SaveAbleList) {
					i.Save (saveNode);
				}
			} else {
				ConfigNode saveNode = gameNode.AddNode (saveNodeName);
				saveNode.AddValue ("");
			}
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

		#region data controls
		/// <summary>
		/// data contol methods
		/// </summary>

		// timers
		public void KSCtimerStart()
		{
			// start KSC timer
			KSCTimer.Start();
		}

		public void KSCtimerpause()
		{
			// pause KSC timer
			KSCTimer.Pause();
		}

		#endregion
    }
}
