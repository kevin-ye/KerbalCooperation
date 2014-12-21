using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using ReeperCommon;


namespace ReeperCommon
{
    public static class GuiUtil
    {
        private const int EZGUI_LAYER = 25;
        private const float EZGUI_DEPTH = 90f;

        //public static Texture2D CreateAtlas(int width, int height, out ConfigNode node, List<Texture2D> textures, int padding = 0)
        //{
        //    node = new ConfigNode("ATLAS_UV_DATA");

        //    if (textures.Count == 0)
        //    {
        //        Log.Error("GuiUtil.CreateAtlas: no textures provided");
        //        return null;
        //    }

        //    var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

        //    var rects = texture.PackTextures(textures.ToArray(), padding);

        //    if (rects == null || rects.Length == 0)
        //    {
        //        Log.Error("GuiUtil: CreateAtlas failed to generate any textures");
        //        return null;
        //    }

        //    for (int i = 0; i < rects.Length; ++i)
        //        node.AddValue(i.ToString(), rects[i].AsVector().ToString());

        //    return texture;
        //}



        /// <summary>
        /// Retrieve the first camera that renders EzGui GUI elements
        /// </summary>
        /// <returns></returns>
        public static Camera GetCamera(string name)
        {
            foreach (var c in Camera.allCameras)
                if (c.name == name)
                    return c;

            return null;
        }

        public static Camera GetGuiCamera()
        {
            return GetCamera("EZGUI Cam") ?? GetCamera("UI camera"); ;
        }


        //public static Vector2 PixelCoordToUVCoord(Texture texture, Vector2 xy)
        //{
        //    return new Vector2(xy.x / ((float)texture.width - 1), 1.0f - (xy.y / ((float)texture.height - 1)));
        //}

        //public static Vector2 PixelSpaceToUVSpace(Texture texture, Vector2 xy)
        //{
        //    return new Vector2(xy.x / ((float)texture.width), xy.y / ((float)texture.height));
        //}

        public static Vector2 PixelCoordToUVCoord(Texture texture, Vector2 xy)
        {
            return new Vector2(xy.x / ((float)texture.width), 1.0f - (xy.y / ((float)texture.height)));
        }

        public static Vector2 PixelSpaceToUVSpace(Texture texture, Vector2 xy)
        {
            return new Vector2(xy.x / ((float)texture.width), xy.y / ((float)texture.height));
        }

        public static Vector2 PixelCoordToUVCoord(this Vector2 xy, Texture texture)
        {
            return PixelCoordToUVCoord(texture, xy);
        }

        public static Vector2 PixelSpaceToUVSpace(this Vector2 xy, Texture texture)
        {
            return PixelSpaceToUVSpace(texture, xy);
        }

        /// <summary>
        /// Basic version; still needs a material and UV data
        /// </summary>
        /// <param name="name"></param>
        /// <param name="screenPos"></param>
        /// <returns></returns>
        //public static UIButton CreateButton(string name)
        //{

        //    UIButton button = UIButton.Create(name, Vector3.zero);
        //    button.gameObject.layer = LayerMask.NameToLayer("EzGUI_UI");
        //    button.plane = SpriteRoot.SPRITE_PLANE.XY;
        //    button.SetAnchor(SpriteRoot.ANCHOR_METHOD.MIDDLE_CENTER);
        //    button.renderCamera = GetEzGuiCamera();

        //    return button;
        //}



        //public static UIButton LoadButton(string 



        public static UIButton CreateBlocker(Rect screenRect, float zPos = float.NaN, string name = "GuiUtil.Blocker")
        {

            var blocker = UIButton.Create("ButtonTest.Blocker", Vector3.zero);
            //var uiCamera = Camera.allCameras.ToList().Find(c => c.name == "EZGUI Cam");
            var uiCamera = GetGuiCamera();

            if (uiCamera == null)
            {
                Log.Error("GuiUtil.CreateBlocker: failed to find GUI camera!");
                Log.Write("... begin list");
                Camera.allCameras.ToList().ForEach(c => Log.Write("Camera: {0}", c.name));
                Log.Write("... end list");

                return null;
            }

            blocker.SetAnchor(SpriteRoot.ANCHOR_METHOD.UPPER_LEFT);
            blocker.renderCamera = uiCamera;
            blocker.gameObject.layer = LayerMask.NameToLayer("EzGUI_UI");

            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
#if DEBUG && DEBUG_UIBUTTON
            texture.SetPixels(new Color[] { new Color(1f, 0f, 1f, .5f) });
#else
            texture.SetPixels(new Color[] { Color.clear });
#endif
            texture.Apply();

            if (float.IsNaN(zPos)) zPos = UIManager.instance.transform.position.z;

            blocker.transform.position = new Vector3(screenRect.x, screenRect.y, zPos);
            blocker.Setup(screenRect.width, screenRect.height, new Material(Shader.Find("Sprite/Vertex Colored")) { mainTexture = texture });


            RepositionButton(blocker, screenRect);

            return blocker;
        }



        public static void RepositionButton(UIButton button, Vector2 screenPos)
        {
            button.transform.position = new Vector3(
                UIManager.instance.transform.position.x + screenPos.x - Screen.width * 0.5f,
                UIManager.instance.transform.position.y - screenPos.y + Screen.height * 0.5f,
                button.transform.position.z);
        }

        public static void RepositionButton(UIButton button, Rect screenRect)
        {
            button.Setup(screenRect.width, screenRect.height);
            RepositionButton(button, new Vector2(screenRect.x, screenRect.y));
        }

        public static void Reposition(this UIButton button, Rect screenRect)
        {
            RepositionButton(button, screenRect);
        }

        public static void Reposition(this UIButton button, Vector2 screenPos)
        {
            RepositionButton(button, screenPos);
        }

        public static void Move(this UIButton button, Rect r)
        {
            RepositionButton(button, r);
        }

        public static void Move(this UIButton button, Vector2 pos)
        {
            RepositionButton(button, pos);
        }
    }

    public static class UIButtonExtensions
    {
        public static void SetZ(this UIButton btn, float z)
        {
            btn.transform.position = new Vector3(btn.transform.position.x, btn.transform.position.y, z);
        }
    }

    namespace Window
    {
        public delegate void WindowDelegate(bool tf);
        public delegate void WindowClosedDelegate();

        /// <summary>
        /// A draggable window with title and optional close/lock buttons
        /// </summary>
        abstract class DraggableWindow : MonoBehaviour
        {
            // constants
            private const int WindowButtonSize = 16;


            // protected members, made accessible to implementors so they can be
            // customized if necessary
            protected UIButton backstop;                        // prevent players from accidentally clicking things in the background
                                                                // this is a bit more reliable than InputLockManager, especially since

            protected Rect windowRect = new Rect();             // current window rect. If ShrinkToFitHeight is true, will constantly have a minimal height
            protected Rect lastRect = new Rect();               // Size of the window last frame
            private GUISkin skin;

            private int winId = UnityEngine.Random.Range(2444, int.MaxValue);

            // window buttons
            private static Vector2 offset = new Vector2(4f, 4f);
            private static GUIStyle buttonStyle;


            // events
            public event WindowDelegate OnVisibilityChange = delegate { };
                // note: includes visibility changes from hiding UI

            public event WindowDelegate OnDraggabilityChange = delegate { };
            public event WindowClosedDelegate OnClosed = delegate { };

/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/


            /// <summary>
            /// Perform any needful setup here, mainly locating textures if necessary and creating
            /// the UIButton to backstop any clicks
            /// </summary>
            protected void Awake()
            {
                #region static style

                if (buttonStyle == null)
                {
                    buttonStyle = new GUIStyle(GUIStyle.none);

                    if (hoverBackground != null)
                        buttonStyle.hover.background = hoverBackground;
                }

                #endregion

                Log.Debug("DraggableWindow.Awake");
                backstop = GuiUtil.CreateBlocker(windowRect, float.NaN /*GuiUtil.GetGuiCamera().nearClipPlane + 1f*/, "DraggableWindow.Backstop");

                Draggable = true;
                Visible = true;
                ClampToScreen = true;
                Title = "Draggable Window";


                windowRect = Setup();
                lastRect = new Rect(windowRect);


                backstop.Move(windowRect);
                backstop.transform.parent = transform; // links blocker visibility with window visibility

                // check for programmer error
                if (windowRect.width < 1f || windowRect.height < 1f)
                    Log.Warning("DraggableWindow.Base: Derived class did not set up initial window Rect");

                GameEvents.onHideUI.Add(OnHideUI);
                GameEvents.onShowUI.Add(OnShowUI);

                Log.Debug("DraggableWindow {0} Awake", Title);
#if DEBUG
                // if debugging, register for our own events so we can see when they're being fired
                //OnVisibilityChange += OnVisibilityChangedEvent;
                //OnClosed += OnCloseEvent;
                //OnDraggabilityChange += OnDraggabilityChangedEvent;
#endif
            }


            private void Start()
            {
                Log.Debug("DraggableWindow {0} Start", Title);
            }


            /// <summary>
            /// Called when this Component is destroyed
            /// </summary>
            protected virtual void OnDestroy()
            {
                Log.Debug("DraggableWindow.OnDestroy");

                GameEvents.onHideUI.Remove(OnHideUI);
                GameEvents.onShowUI.Remove(OnShowUI);
            }



            /// <summary>
            /// Called whenever the GameObject this MonoBehaviour is attached to becomes active, including
            /// right before Start
            /// </summary>
            protected void OnEnable()
            {
                Log.Debug("DraggableWindow.OnEnable");
                OnVisibilityChange(true);
            }



            /// <summary>
            /// Called whenever the GameObject this MonoBehaviour is attached to is disabled
            /// </summary>
            protected void OnDisable()
            {
                Log.Debug("DraggableWindow.OnDisable");
                OnVisibilityChange(false);
            }



            /// <summary>
            /// Show or hide the window. Equivalent to using Visible property
            /// </summary>
            /// <param name="tf"></param>
            public void Show(bool tf)
            {
                Visible = tf;
            }



            protected void Update()
            {
                if (ShrinkHeightToFit)
                    windowRect.height = 1f;

            }



            /// <summary>
            /// Standard Unity method
            /// </summary>
            protected void OnGUI()
            {
                GUI.skin = Skin;

                
                windowRect = GUILayout.Window(winId, windowRect, _InternalDraw, Title);
                
                if (ClampToScreen)
                    windowRect = KSPUtil.ClampRectToScreen(windowRect);

                backstop.Move(windowRect);

                //lastRect = new Rect(windowRect);
            }



            /// <summary>
            /// Calls the implementor draw method and then handles the window buttons (close/lock) if
            /// applicable.
            /// </summary>
            /// <param name="winid"></param>
            private void _InternalDraw(int winid)
            {
                DrawUI();


                // now window buttons
                lastRect.x = windowRect.x;
                lastRect.y = windowRect.y;

                GUILayout.BeginArea(new Rect(0f, offset.y, lastRect.width, lastRect.height));
                lastRect = new Rect(windowRect);

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));

                // spacer to right-justify buttons
                GUILayout.FlexibleSpace();

                // lock button
                if (LockTexture != null && UnlockTexture != null)
                {
                    if (GUILayout.Button(Draggable ? UnlockTexture : LockTexture, buttonStyle))
                    {
                        Draggable = !Draggable;
                        if (!string.IsNullOrEmpty(ButtonSound))
                            AudioPlayer.Audio.PlayUI(ButtonSound);

                        Log.Debug("DraggableWindow {0}", Draggable ? "unlocked" : "locked");
                    }


                    // a little space to separate buttons
                    if (CloseTexture != null)
                        GUILayout.Space(offset.x * 0.5f);

                }

                // close button
                if (CloseTexture != null)
                    if (GUILayout.Button(CloseTexture, buttonStyle))
                    {
                        if (!string.IsNullOrEmpty(ButtonSound))
                            AudioPlayer.Audio.PlayUI(ButtonSound);

                        // note: we do not set window visibility ourselves here because it might
                        // restrict how the derived class can act; for instance, preventing the window
                        // from closing and instead opening a confirmation popup
                        OnCloseClick();
                    }


                GUILayout.Space(offset.x);
                GUILayout.EndHorizontal();
                GUILayout.EndArea();

                if (Draggable)
                    GUI.DragWindow();
            }



            /// <summary>
            /// Implementations of a draggable window should set up an initial Rect here and do
            /// any other needful setup
            /// </summary>
            /// <returns></returns>
            protected abstract Rect Setup();



            /// <summary>
            /// Implementations supply this method which will be called to handle UI
            /// </summary>
            protected abstract void DrawUI();



            /// <summary>
            /// Implemented by derived classes; called when the close button is clicked
            /// </summary>
            protected abstract void OnCloseClick();



            #region GameEvents

            private void OnHideUI()
            {
                gameObject.SetActive(false);
            }

            private void OnShowUI()
            {
                gameObject.SetActive(Visible);
            }

            #endregion

            #region properties

            


            /// <summary>
            /// If true, the player can click on any portion of the window and drag it around
            /// </summary>
            public bool Draggable
            {
                get
                {
                    return draggable;
                }

                protected set
                {
                    if (draggable != value)
                        OnDraggabilityChange(value);
                    draggable = value;
                }
            }
            private bool draggable = true;



            public bool ShrinkHeightToFit { get; set; }


            /// <summary>
            /// Shows or hides the window. Fires visibility event on change.
            /// </summary>
            public bool Visible
            {
                get
                {
                    return visible;
                }
                set
                {
                    if (value != visible)
                        OnVisibilityChange(value);

                    visible = value;

                    if (gameObject.activeInHierarchy != visible && visible == false)
                        OnClosed();

                    gameObject.SetActive(visible);
                }
            }
            private bool visible = true;



            /// <summary>
            /// Unity window identifier
            /// </summary>
            public int WindowID
            {
                get
                {
                    return winId;
                }

                private set
                {
                    winId = value;
                }
            }



            /// <summary>
            /// Sets window title
            /// </summary>
            public string Title { get; set; }



            /// <summary>
            /// Skin to be used for this particular window instance
            /// </summary>
            public GUISkin Skin
            {
                get
                {
                    return skin ?? DefaultSkin;
                }
                set
                {
                    skin = value ?? DefaultSkin;
                }
            }


            public UIButton Backstop { get { return backstop; } }

            public Rect WindowRect { get { return lastRect; } } // last because windowRect.h will be reset to 1 every frame if shrink to fit


            /// <summary>
            /// Prevents any portion of the window from extending outside of the screen. True by default.
            /// </summary>
            public bool ClampToScreen { get; set; }



            /// <summary>
            /// Texture to use for the "lock" button. If null, the lock/unlock window button won't
            /// be available.
            /// </summary>
            public static Texture2D LockTexture { get; set; }



            /// <summary>
            /// See LockTexture
            /// </summary>
            public static Texture2D UnlockTexture { get; set; }



            /// <summary>
            /// Texture to use for the window close button. If null, the window won't have one
            /// </summary>
            public static Texture2D CloseTexture { get; set; }



            /// <summary>
            /// Texture placed behind the Lock/Close button when moused over
            /// </summary>
            public static Texture2D ButtonHoverBackground
            {
                get
                {
                    return hoverBackground ?? ResourceUtil.GenerateRandom(WindowButtonSize, WindowButtonSize);
                }
                set
                {
                    hoverBackground = value;
                    if (buttonStyle != null)
                        buttonStyle.hover.background = value;
                }
            }
            private static Texture2D hoverBackground = null;



            /// <summary>
            /// Sound to play when the lock or close button is clicked. Null or empty to play nothing.
            /// Uses default AudioPlayer instance (usually the first one created in a scene)
            /// </summary>
            public static string ButtonSound { get; set; }

            

            /// <summary>
            /// Default skin to use for DraggableWindows (default is HighLogic.Skin if nothing set)
            /// </summary>
            public static GUISkin DefaultSkin
            {
                get
                {
                    return defaultSkin ?? HighLogic.Skin;
                }
                set
                {
                    defaultSkin = value;
                }
            }
            private static GUISkin defaultSkin;


            #endregion

            #region debug

#if DEBUG
            private void OnCloseEvent()
            {
                Log.Debug("DraggableWindow.OnCloseEvent");
            }

            private void OnVisibilityChangedEvent(bool tf)
            {
                Log.Debug("DraggableWindow.VisibilityChangedEvent - " + tf);
            }

            private void OnDraggabilityChangedEvent(bool tf)
            {
                Log.Debug("DraggableWindow.DraggabilityChangedEvent - " + tf);
            }
#endif

            #endregion

#region save/load


            /// <summary>
            /// Save window position into specified ConfigNode. Why not use our ConfigNode serialization for
            /// this? Because there are a lot of parameters that shouldn't be saved and it's just messier
            /// </summary>
            /// <param name="node"></param>
            public void SaveInto(ConfigNode node)
            {
                if (node != null)
                {
                    node.Set("WindowX", windowRect.x);
                    node.Set("WindowY", windowRect.y);
                    node.Set("Draggable", Draggable);
                    node.Set("Visible", Visible);

                    Log.Debug("DraggableWindow.SaveInto: Saved window {0} as ConfigNode {1}", Title, node.ToString());
                }
                else Log.Warning("GuiUtil.DraggableWindow: Can't save into null ConfigNode");
            }

           

            /// <summary>
            /// Load window position from specified ConfigNode
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            public bool LoadFrom(ConfigNode node)
            {
                if (node != null)
                {
                    windowRect.x = node.Parse<float>("WindowX", Screen.width * 0.5f - windowRect.width * 0.5f);
                    windowRect.y = node.Parse<float>("WindowY", Screen.height * 0.5f - windowRect.height * 0.5f);
                    Draggable = node.Parse<bool>("Draggable", true);
                    Visible = node.Parse<bool>("Visible", false);

#if DEBUG
                    Log.Debug("DraggableWindow: Window {0} loaded position {1}", Title, new Vector2(windowRect.x, windowRect.y).ToString());
#endif
                    return node.HasValue("WindowX") && node.HasValue("WindowY");
                }
                else
                {
                    Log.Warning("GuiUtil.DraggableWindow: Can't load from null ConfigNode");
                    return false;
                }
            }
#endregion
        }
    }
}
