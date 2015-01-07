using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KCoop
{
    public enum notifyInfoTypeString
    {
        None = 0,
        Initialize,
        SceneChange,
    }

    public enum notifyInfoString {
        None = 0,

        Instantly,
        EveryScene,
        MainMenu,
        Settings,
        Credits,
        SpaceCentre,
        Editor,
        Flight,
        TrackingStation,
        PSystemSpawn,
    }

    public class KCoopNotifyInfoString 
    {
        public notifyInfoTypeString InfoType { get; private set; }
        public notifyInfoString fromScene { get; set; }
        public notifyInfoString toScene
        {
            get;
            set
            {
                fromScene = toScene;
                toScene = value;
            }
        }

        public KCoopNotifyInfoString(notifyInfoTypeString type = notifyInfoTypeString.Initialize, notifyInfoString from = notifyInfoString.None, notifyInfoString to = notifyInfoString.None)
        {
            InfoType = type;
            fromScene = from;
            toScene = to;
        }

        public static notifyInfoString currentScene()
        {
            if (HighLogic.LoadedScene == GameScenes.CREDITS)
            {
                return notifyInfoString.Credits;
            }
            else if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                return notifyInfoString.Editor;
            }
            else if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                return notifyInfoString.Flight;
            }
            else if (HighLogic.LoadedScene == GameScenes.MAINMENU)
            {
                return notifyInfoString.MainMenu;
            }
            else if (HighLogic.LoadedScene == GameScenes.PSYSTEM)
            {
                return notifyInfoString.PSystemSpawn;
            }
            else if (HighLogic.LoadedScene == GameScenes.SETTINGS)
            {
                return notifyInfoString.Settings;
            }
            else if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                return notifyInfoString.SpaceCentre;
            }
            else if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                return notifyInfoString.TrackingStation;
            }
            else
            {
                return notifyInfoString.None;
            }
        }
    }
}
