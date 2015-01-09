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
		private readonly string saveNodeName = "KCoop";
		private bool newSave = true;
		private List<ISaveAble> SaveAbleList = new List<ISaveAble>();
		private KCoopTimer KSCTimer;

        public static KerbalCooperation Instance { get; private set; }
		public KCoopSpaceCenterManager SpaceCenterManager { get; private set;}

        private void Initialize()
        {
            Instance = this;
			SaveAbleList.Clear ();
			KSCTimer = new KCoopTimer("KSCtimer", false);
			SaveAbleList.Add(KSCTimer);

            //UnityEngine.Object.DontDestroyOnLoad(this);
			flag_Initialized = true;

			// add scenario module
			var game = HighLogic.CurrentGame;

			ProtoScenarioModule psm = game.scenarios.Find(s => s.moduleName == typeof(KerbalCooperation).Name);
			if (psm == null)
			{
				psm = game.AddProtoScenarioModule(typeof(KerbalCooperation), GameScenes.SPACECENTER,
					GameScenes.FLIGHT, GameScenes.EDITOR);
			}
			else
			{
				if (!psm.targetScenes.Any(s => s == GameScenes.SPACECENTER))
				{
					psm.targetScenes.Add(GameScenes.SPACECENTER);
				}
				if (!psm.targetScenes.Any(s => s == GameScenes.FLIGHT))
				{
					psm.targetScenes.Add(GameScenes.FLIGHT);
				}
				if (!psm.targetScenes.Any(s => s == GameScenes.EDITOR))
				{
					psm.targetScenes.Add(GameScenes.EDITOR);
				}
			}
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

		public override void OnAwake()
        {
			base.OnAwake ();
            if (!flag_Initialized)
            {
				// avoid first time Awake for Start
                return;
            }

			if (HighLogic.LoadedScene == GameScenes.SPACECENTER) {
				Logger.log ("Adding SpaceCenterManager");
				SpaceCenterManager = gameObject.AddComponent<KCoopSpaceCenterManager>() as KCoopSpaceCenterManager;
			} else if (HighLogic.LoadedScene == GameScenes.FLIGHT) {
			} else if (HighLogic.LoadedScene == GameScenes.EDITOR) {
			}

		}

		private void OnDestroy()
		{
			Destroy(SpaceCenterManager);
		}

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
