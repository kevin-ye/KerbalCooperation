using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KCoop
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class KerbalCooperation: MonoBehaviour
    {
        private static GameObject theKerbalCooperation;
        public static KerbalCooperation Instance {get; private set;}

        private void Initialize()
        {
            Instance = this;
            if (GameObject.Find("KerbalCooperation") == null)
            {
                theKerbalCooperation = new GameObject("KerbalCooperation", new [] {typeof (KerbalCooperation)});
                UnityEngine.Object.DontDestroyOnLoad(theKerbalCooperation);
            }
        } 

        public void Start()
        {
            Logger.log("KerbalCooperation Initializing.");
            Initialize();
            Logger.log("KerbalCooperation Initialized.");
        }

        public void Awake()
        {
            Logger.log("KerbalCooperation Awake.");
        }
    }
}
