using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KCoop
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class KerbalCooperation: MonoBehaviour
    {
        private static bool flag_Initialized = false;
        public static KerbalCooperation Instance {get; private set;}

        private void Initialize()
        {
            Instance = this;
            UnityEngine.Object.DontDestroyOnLoad(this);
            flag_Initialized = true;
        } 

        public void Start()
        {
            Logger.log("KerbalCooperation Initializing.");
            Initialize();
            Logger.log("KerbalCooperation Initialized.");
        }

        public void Awake()
        {
            if (!flag_Initialized)
            {
                return;
            }
            Logger.log("KerbalCooperation Awake.");
        }
    }
}
