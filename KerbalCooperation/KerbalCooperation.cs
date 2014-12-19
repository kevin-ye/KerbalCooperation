using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KCoop
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre,false)]
    public class AddScenarioModules : MonoBehaviour
    {
        void Start()
        {
            var game = HighLogic.CurrentGame;

            ProtoScenarioModule psm = game.scenarios.Find(s => s.moduleName == typeof(KerbalCooperation).Name);
            if (psm == null)
            {
                Logger.log("Adding the scenario module.");
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
    }

    public class KerbalCooperation : ScenarioModule
    {
        private static KerbalCooperation Instance { get; private set;}
        public KerbalCooperation()
        {
            Logger.log("Scenario constructor.");
            Instance = this;
        }

        public override void OnAwake()
        {
            base.OnAwake();
        }
    }
}
