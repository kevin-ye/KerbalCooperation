using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
using System.IO;
using UnityEngine;

namespace ReeperCommon
{
    internal class PlayableSound
    {
        public AudioClip clip;
        public string shortName = ""; // just filename, so we could play a sound as "ScienceAlert/sounds/noise" or just "noise"
        public float nextPlayableTime;

        internal PlayableSound(AudioClip aclip)
        {
            clip = aclip;
            nextPlayableTime = 0f;
            shortName = GetShortName(aclip.name);
        }


        public static string GetShortName(string name)
        {
            if (name.Contains("/"))
            {
                int idx = name.LastIndexOf('/');
                if (idx >= 0)
                    return name.Substring(idx + 1);;
            }

            return name;
        }
    }



    internal class AudioPlayer : MonoBehaviour
    {
        private static AudioPlayer _instance;

        Dictionary<string, PlayableSound> sounds = new Dictionary<string, PlayableSound>();
        AudioSource source;


        //-----------------------------------------------------------------------------
        // Begin implementation
        //-----------------------------------------------------------------------------

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }



        public void SetSource(GameObject src, bool b2d = true)
        {
            source = src.GetComponent<AudioSource>() ?? src.AddComponent<AudioSource>();

            if (b2d) source.panLevel = 0f; // 2d sounds
        }



        /// <summary>
        /// Loads sounds from the specified directory. Accepts absolute path, path relative to
        /// GameData, and then path relative to DLL in that order
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="b2D"></param>
        /// <returns></returns>
        public int LoadSoundsFrom(string dir, bool b2D = true)
        {
            int counter = 0;

            if (Path.IsPathRooted(dir) && Directory.Exists(dir))
            {
                Log.Verbose("AudioPlayer: '{0}' seems to be a full path", dir);

                dir = Path.GetFullPath(dir).Replace('\\', '/');

                Log.Debug("AudioPlayer.LoadSoundsFrom: Path after transformation and replace: {0}", dir);
                dir = ConfigUtil.GetRelativeToGameData(dir);
            }
            else
            {
                dir = dir.TrimStart('\\', '/');

                // not a whole dir, so it could be relative to GameData such as:
                // [ksproot]/GameData/      dir = ScienceAlert/sounds
                //
                // or it could be relative to the DLL, such as:
                // sounds

                if (!Directory.Exists(Path.Combine(Path.GetFullPath(KSPUtil.ApplicationRootPath + "GameData"), dir))) // not relative to GameData
                {
                    Log.Verbose("AudioPlayer: '{0}' seems to be a plugin-relative path", dir);

                    // try relative to dll dir
                    string relDll = Path.Combine(ConfigUtil.GetDllDirectoryPath(), dir);
                    Log.Debug("trying relative path: " + relDll);

                    if (Directory.Exists(relDll))
                    {
                        dir = ConfigUtil.GetRelativeToGameData(relDll).Replace('\\', '/');
                    }
                    else
                    {
                        Log.Error("AudioPlayer: Couldn't find '{0}'", dir);
                        return 0;
                    }
                }
                else
                {
                    dir = dir.Replace('\\', '/');
                    Log.Verbose("AudioPlayer: '{0}' seems to be a GameData-relative path", dir);
                    Log.Debug("checked: " + Path.Combine(Path.GetFullPath(KSPUtil.ApplicationRootPath), dir));
                }
            }

            /*
             * [LOG 17:56:12.954] ScienceAlert, name: ScienceAlert/sounds/bubbles
[LOG 17:56:12.955] ScienceAlert, name: ScienceAlert/sounds/click1
[LOG 17:56:12.956] ScienceAlert, name: ScienceAlert/sounds/click2
[LOG 17:56:12.956] ScienceAlert, name: ScienceAlert/sounds/error*/

            GameDatabase.Instance.databaseAudio.ForEach(ac =>
                {
                    // strip off the filename and just use the path (if any) of the clip's gamedata-relative url
                    string urlDir = ac.name;
                    int idx = urlDir.LastIndexOf('/');

                    if (idx >= 0) urlDir = urlDir.Substring(0, idx);

                    

                    //if (ac.name.Contains(dir))
                    if (string.Equals(urlDir, dir))
                    {
                        if (sounds.ContainsKey(ac.name))
                        {
                            Log.Warning("AudioPlayer: Already have key '{0}'", ac.name);
                            return;
                        }
                        else
                        {
                            sounds.Add(ac.name, new PlayableSound(ac));
                            Log.Normal("{0} ready", ac.name);
                            ++counter;
                        }
                    }

                });

            if (counter == 0)
                Log.Warning("AudioPlayer: Didn't load any sounds from directory '{0}'", dir);

            return counter;
        }



        /// <summary>
        /// Returns first AudioPlayer created, unless there isn't one in which a default
        /// version will be automatically created (destroyed on scene change).
        /// </summary>
        public static AudioPlayer Audio
        {
            get
            {
                if (_instance == null)
                {
                    GameObject ap = new GameObject("Reeper.AudioPlayer", typeof(AudioSource));
                    ap.AddComponent<AudioPlayer>().SetSource(ap);
                }

                return _instance;
            }
        }



        /// <summary>
        /// Plays the specified sound and prevents it from being played again for \a 
        /// delay seconds.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public bool PlayThenDelay(string name, float delay = 1f)
        {
            return Play(name, 1f, delay);
        }


        /// <summary>
        /// Plays the specified sound using UI volume in KSP
        /// </summary>
        /// <param name="name"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public bool PlayUI(string name, float delay = 0f)
        {
            return Play(name, GameSettings.UI_VOLUME, delay);
        }



        /// <summary>
        /// Plays the specified sound. Accepts names either relative to GameData or shorthand, e.g.
        /// "ScienceAlert/sounds/beep" or just "beep" assuming a sound with that name is loaded
        /// </summary>
        /// <param name="name"></param>
        /// <param name="volume"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public bool Play(string name, float volume = 1f, float delay = 0f)
        {
            PlayableSound sound = null;

            if (sounds.ContainsKey(name))
            {
                sound = sounds[name];
            }
            else
            {
                //string located = sounds.Keys.ToList().Find(k => string.Equals(PlayableSound.GetShortName(k), sounds[k].shortName));
                string located = sounds.Keys.ToList().SingleOrDefault(k => string.Equals(PlayableSound.GetShortName(k), name));

                if (!string.IsNullOrEmpty(located) && sounds.ContainsKey(located))
                    sound = sounds[located];
            }

            if (sound == null)
            {
                Log.Error("AudioPlayer: Cannot play '{0}'!", name);
                return false;
            }

            if (Time.realtimeSinceStartup - sound.nextPlayableTime > 0f)
            {
                if (source == null) SetSource(gameObject);

                try
                {
                    source.PlayOneShot(sound.clip, Mathf.Clamp(volume, 0f, 1f));
                    sound.nextPlayableTime = Time.realtimeSinceStartup + delay;
                    Log.Debug("Played {0}, next available time {1}", sound.clip.name, sound.nextPlayableTime);

                    return true;
                }
                catch (Exception e)
                {
                    Log.Error("AudioPlayer.Play exception while playing '{0}': {1}", name, e);
                    return false;
                }
            }
            else return false;
        }


        public int Count
        {
            get
            {
                return sounds.Count;
            }
        }
    }


//    /// <summary>
//    /// General purpose audio utilities
//    /// </summary>
//    internal class AudioUtil
//    {
//        private static AudioUtil _instance = null;
//        private Dictionary<string /* sound name */, PlayableSound> sounds = new Dictionary<string, PlayableSound>();
//        private const float LOADING_TIMEOUT = 5f;
//        private GameObject gameObject = new GameObject("ReeperCommon.AudioUtil", typeof(AudioSource));


//        internal class PlayableSound
//        {
//            private AudioClip clip;
//            private AudioSource source;

//            private float lastPlayedTime;
//            private float delay;

//            internal PlayableSound(AudioClip aclip, AudioSource player)
//            {
//                clip = aclip;
//                source = player;
//                lastPlayedTime = delay = 0f;
//            }


//            public bool Play(float volume = 1f, float delay = 0f)
//            {
//                if (clip != null && Time.realtimeSinceStartup - lastPlayedTime > delay)
//                {
//                    if (!source.gameObject.activeSelf)
//                        source.gameObject.SetActive(true);

//                    try
//                    {
//                        source.PlayOneShot(clip, Mathf.Clamp(volume, 0f, 1f));
//                        lastPlayedTime = Time.realtimeSinceStartup;
//                        this.delay = delay;
//                        return true;
//                    } catch (Exception e)
//                    {
//                        Log.Error("AudioUtil.Play exception while playing '{0}': {1}", clip.name, e);
//                        return false;
//                    }
//                }
//                else return false;
//            }
//        }

////-----------------------------------------------------------------------------
//// Begin implementation
////-----------------------------------------------------------------------------
//        internal AudioUtil()
//        {
//            gameObject.transform.parent = Camera.main.transform;
//        }

//        public static AudioUtil Instance
//        {
//            get
//            {
//                if (_instance == null)
//                    _instance = new AudioUtil();
                

//                return _instance;
//            }
//        }

//        public static void EnsureLoaded()
//        {
//            var audio = Instance;
//        }



//        public static void UnloadAll()
//        {
//            Instance._UnloadAll();
//        }



//        private void _UnloadAll()
//        {
//            Log.Normal("Unloading {0} sounds", sounds.Count);
//            sounds.Clear();
//        }



//        public PlayableSound this[string name]
//        {
//            get
//            {
//                if (sounds.ContainsKey(name))
//                {
//                    return sounds[name];
//                } else 
//                {
//                    Log.Error("AudioUtil: Did not find sound '{0}'", name);
//                    return null;
//                }
//            }
//        }



//        /// <summary>
//        /// Locates all files with the specified extension in the specified
//        /// directory relative to GameData (with empty string being inside
//        /// GameData itself) and attempts to load them.
//        /// <param name="relToGameData">Relative directory to load sounds from</param>
//        /// <param name="type">Type of sound to load</param>
//        /// </summary>
//        /// <returns>Number of unique sounds loaded</returns>
//        public static int LoadSoundsFrom(string relToGameData, AudioType type)
//        {
//            return Instance._LoadSoundsFrom(relToGameData, type);
//        }



//        private int _LoadSoundsFrom(string relToGameData, AudioType type)
//        {
//            if (relToGameData.StartsWith("/")) relToGameData = relToGameData.Substring(1);
//            string soundDir = KSPUtil.ApplicationRootPath + "GameData/" + relToGameData;

//            Log.Debug("Locating audio {0} files in '{1}'", type.ToString(), soundDir);
//            if (!Directory.Exists(soundDir))
//            {
//                Log.Error("Directory '{0}' does not exist!", soundDir);
//                return 0;
//            }

//            if (type == AudioType.UNKNOWN)
//            {
//                Log.Error("AudioUtil: \"Unknown\" is not a valid AudioType");
//                return 0;
//            }

//            string extension = string.Format(".{0}", type.ToString()).ToLower();


//            string[] files = Directory.GetFiles(soundDir, "*" + extension);
//            int loadCounter = 0;

//            Log.Debug("Found {0} sound ({2}) files in directory {1}", files.Length, soundDir, extension);
//            foreach (var f in files)
//                Log.Debug("File: {0}", f);

//            foreach (var file in files)
//            {
//                string name = Path.GetFileNameWithoutExtension(file);

//                if (name.EndsWith(extension))
//                    name = name.Substring(0, name.Length - extension.Length);

//                if (sounds.ContainsKey(name)) continue; // already loaded this sound

//                AudioClip newClip = LoadSound(file, type, false);

//                if (newClip != null)
//                {
//                    Log.Debug("Loaded sound; adding as {0}", name);

//                    try
//                    {
//                        sounds.Add(name, new PlayableSound(newClip, gameObject.audio));
//                        ++loadCounter;
//                    }
//                    catch (Exception e)
//                    {
//                        Log.Error("AudioController: cannot add {0} due to {1}", name, e);
//                    }
//                }
//                else
//                {
//                    Log.Error("AudioUtil: Failed to load '{0}'", file);
//                }
//            }
            
//            return loadCounter;
//        }


//        /// <summary>
//        /// Play a sound with given key, volume and delay
//        /// </summary>
//        /// <param name="name"></param>
//        /// <param name="volume"></param>
//        /// <returns></returns>
//        public static bool PlayVolume(string name, float volume = 1f, float delay = 0f)
//        {
//            var sound = Instance[name];

//            if (sound == null) return false;

//            return sound.Play(volume, delay);
//        }



//        /// <summary>
//        /// Play a sound with given key and delay. Volume is determined
//        /// by game's ui setting.
//        /// </summary>
//        /// <param name="name"></param>
//        /// <param name="delay"></param>
//        /// <returns></returns>
//        public static bool Play(string name, float delay = 0f)
//        {
//            return PlayVolume(name, GameSettings.UI_VOLUME, delay);
//        }



//        /// <summary>
//        /// The GameDatabase audio clips all seem to be intended for use with
//        /// 3D. It causes a problem with our UI sounds because the player's
//        /// viewpoint is moving. Even if we attach an audio source to the
//        /// player camera, strange effects due to that movement (like much
//        /// louder in one ear in certain orientations) seem to occur.
//        /// 
//        /// This allows us to load the sounds ourselves with the parameters
//        /// we want.
//        /// </summary>
//        /// <param name="path"></param>
//        /// <returns></returns>
//        private AudioClip LoadSound(string path, AudioType type = AudioType.WAV, bool relativeToGameData = true)
//        {
//            if (relativeToGameData)
//            {
//                if (path.StartsWith("/"))
//                    path = path.Substring(1);

//                path = KSPUtil.ApplicationRootPath + "GameData/" + path;
//            }
//            Log.Verbose("Loading sound {0}", path);

//            // windows requires three slashes.  see:
//            // http://docs.unity3d.com/Documentation/ScriptReference/WWW.html
//            if (Application.platform == RuntimePlatform.WindowsPlayer)
//            {
//                if (!path.StartsWith("file:///"))
//                    path = "file:///" + path;
//            }
//            else if (!path.StartsWith("file://")) path = "file://" + path;

//            Log.Debug("sound path: {0}, escaped {1}", path, System.Uri.EscapeUriString(path));

//            // WWW.EscapeURL doesn't seem to work all that great.  I couldn't get
//            // AudioClips to come out of it correctly.  Non-escaped local urls
//            // worked just fine but the docs say they should be escaped and this
//            // works so I think it's the best solution currently
//            //WWW clipData = new WWW(WWW.EscapeURL(path));
//            WWW clipData = new WWW(System.Uri.EscapeUriString(path));

//            float start = Time.realtimeSinceStartup;

//            while (!clipData.isDone && Time.realtimeSinceStartup - start < LOADING_TIMEOUT)
//            {
//            }

//            if (!clipData.isDone)
//                Log.Error("Audio.LoadSounds() - timed out in {0} seconds", Time.realtimeSinceStartup - start);

//            return clipData.GetAudioClip(false, false, type);
//        }
//    }
}
