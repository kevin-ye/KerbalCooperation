/******************************************************************************
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
 * ***************************************************************************/
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

namespace ReeperCommon
{
    public static class ResourceUtil
    {
        /// <summary>
        /// Saves texture into plugin dir with supplied name.
        /// Precondition: texture must be readable
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="name"></param>
        public static bool SaveToDisk(this UnityEngine.Texture2D texture, string pathInGameData)
        {
            // texture format - needs to be ARGB32, RGBA32, RGB24 or Alpha8
            var validFormats = new List<TextureFormat>{ TextureFormat.Alpha8, 
                                                        TextureFormat.RGB24,
                                                        TextureFormat.RGBA32,
                                                        TextureFormat.ARGB32};

            if (!validFormats.Contains(texture.format))
            {
                Log.Write("Texture to be saved has invalid format. Converting to a valid format.");
                return CreateReadable(texture).SaveToDisk(pathInGameData);
            }

            if (pathInGameData.StartsWith("/"))
                pathInGameData = pathInGameData.Substring(1);

            pathInGameData = "/GameData/" + pathInGameData;

            if (!pathInGameData.EndsWith(".png"))
                pathInGameData += ".png";

            try
            {
                Log.Verbose("Saving a {0}x{1} texture as '{2}'", texture.width, texture.height, pathInGameData);

                System.IO.FileStream file = new System.IO.FileStream(KSPUtil.ApplicationRootPath + pathInGameData, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                System.IO.BinaryWriter writer = new System.IO.BinaryWriter(file);
                writer.Write(texture.EncodeToPNG());

                Log.Verbose("Texture saved as {0} successfully.", pathInGameData);
                return true;
            }
            catch (Exception e)
            {
                Log.Error("Failed to save texture '{0}' due to {1}", pathInGameData, e);
                return false;
            }
        }


        public static Texture2D as2D(this UnityEngine.Texture tex)
        {
            return tex as Texture2D;
        }

        public static Material GetEmbeddedMaterial(string resource)
        {
            string contents = string.Empty;

            if (!GetEmbeddedContents(resource, out contents))
                return null;

            return new Material(contents);
        }


        /// <summary>
        /// Retrieves a texture embedded in the DLL, given its resource url
        /// Returns null if failed
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public static Texture2D GetEmbeddedTexture(string resource, bool compress = false, bool mip = false)
        {
            var input = GetEmbeddedContentsStream(resource);

            if (input == null)
            {
                Log.Error("Failed to locate embedded texture '{0}'", resource);
                return null;
            }
            else
            {
                // success!  now to turn the string string into a usable
                // texture
                byte[] buffer = new byte[16 * 1024];
                int read = 0;
                var ms = new MemoryStream();

                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, read);

                Texture2D texture = new Texture2D(1, 1, compress ? TextureFormat.DXT5 : TextureFormat.ARGB32, mip);

                if (texture.LoadImage(ms.ToArray()))
                {
                    Log.Verbose("Loaded embedded texture '{0}', dimensions {1}x{2}", resource, texture.width, texture.height);
                    return texture;
                }
                else
                {
                    Log.Error("GetEmbeddedTexture: Failed to create Texture2D out of {0} bytes", ms.ToArray().Length);
                }
            }

            return null;
        }



        /// <summary>
        /// Get the contents of a resource embedded in the running DLL given its
        /// resource url
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static bool GetEmbeddedContents(string resource, System.Reflection.Assembly assembly, out string contents)
        {
            contents = string.Empty;

            try
            {
                var stream = GetEmbeddedContentsStream(resource, assembly);

                if (stream != null)
                {
                    var reader = new System.IO.StreamReader(stream);

                    if (reader != null)
                    {
                        contents = reader.ReadToEnd();

                        return contents.Length > 0;
                    } 
                }
            } catch (Exception e)
            {
                Log.Error("GetEmbeddedContents: {0}", e);
            }

            return false;
        }


        public static bool GetEmbeddedContents(string resource, out string contents)
        {
            return GetEmbeddedContents(resource, System.Reflection.Assembly.GetExecutingAssembly(), out contents);
        }


        public static byte[] GetEmbeddedContentsBytes(string resource, System.Reflection.Assembly assembly)
        {
            Stream contents = GetEmbeddedContentsStream(resource, assembly);
            if (contents != null && contents.Length > 0) 
            {
                byte[] data = new byte[contents.Length];

                int read = 0;
                var ms = new MemoryStream();

                while ((read = contents.Read(data, 0, data.Length)) > 0)
                    ms.Write(data, 0, read);

                return data;

            } else return null;
        }


        public static Stream GetEmbeddedContentsStream(string resource, System.Reflection.Assembly assembly)
        {
            return assembly.GetManifestResourceStream(resource);
        }

        public static Stream GetEmbeddedContentsStream(string resource)
        {
            return GetEmbeddedContentsStream(resource, System.Reflection.Assembly.GetExecutingAssembly());
        }


        public static Texture2D LocateTexture(string textureName, bool relativeToGameData = false)
        {
            if (string.IsNullOrEmpty(textureName))
            {
                Log.Error("LocateTexture: invalid texture name");
                return null;
            }

            string data = string.Empty;
            Texture2D tex;

            // try as embedded resource first
            var bytes = GetEmbeddedContentsBytes(textureName, System.Reflection.Assembly.GetExecutingAssembly());

            if (bytes != null)
            {
                tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                if (tex.LoadImage(bytes))
                    return tex;
            }

            var file = Path.GetFileNameWithoutExtension(textureName);
            var dir = Path.GetDirectoryName(textureName);

            if (file.StartsWith("/") || file.StartsWith("\\"))
                file = file.Substring(1);

            if (dir.EndsWith("/") || dir.EndsWith("\\"))
                dir = dir.Substring(1);

            // wasn't embedded, look in GameDatabase instead
            if (textureName.StartsWith("/")) textureName = textureName.Substring(1);

            if (relativeToGameData)
            {
                textureName = dir + "/" + file;
            }
            else
            {
                textureName = ConfigUtil.GetRelativeToGameData(ConfigUtil.GetDllDirectoryPath()) + dir + "/" + file;
            }

            tex = GameDatabase.Instance.GetTexture(textureName, false);

            if (tex == null)
                Log.Error("Failed to find texture '{0}'", textureName);

            return tex;
        }



        /// <summary>
        /// Look for a material in this order:
        ///     1) Embedded shader
        ///     2) File relative to same dir as DLL
        ///     3) Shader lib
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="backups"></param>
        /// <returns></returns>
        public static Material LocateMaterial(string resourceName, params string[] backups)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                Log.Error("LocateMaterial: invalid resource name");
                return null;
            }

            var possibilities = new Dictionary<string /* resource name */, string /* what to display in log */>();
            possibilities.Add(resourceName, string.Format("LocateMaterial: Creating shader material '{0}'", resourceName));

            foreach (var backup in backups)
                possibilities.Add(backup, string.Format("LocateMaterial: Attempting backup shader '{0}'", backup));

            foreach (var resource in possibilities)
            {
                Log.Verbose(resource.Value);

                // check embedded resources
                string contents;

                if (GetEmbeddedContents(resource.Key, out contents))
                {
                    Log.Write("Found '{0}' as embedded resource stream.", resource.Key);
                    return new Material(contents);
                }
                else
                {
                    // not embedded. Try as a filename in our dir
                    string ourDir = ConfigUtil.GetDllDirectoryPath();
                    if (resource.Key[0] != '/')
                        ourDir += "/";

                    if (File.Exists(ourDir + resource.Key))
                    {
                        string filename = ourDir + resource.Key;

                        Log.Write("Possibly found '{0}' as file path {1}", resource.Key, filename);

                        try
                        {
                            contents = System.IO.File.ReadAllText(filename);
                            var mat = new Material(contents);

                            if (mat != null)
                            {
                                Log.Write("Created material from file {0}", resource.Key);
                                return mat;
                            }
                        } catch (Exception)
                        {
                        }

                        Log.Error("{0} looks like file path, but is not a valid material source. Continuing search", filename);
                    }


                    // not in our resources.  Check Shader lib
                    var shader = Shader.Find(resource.Key);

                    if (shader != null)
                    {
                        try
                        {
                            return new Material(shader);
                        }
                        catch (Exception e)
                        {
                            Log.Error("Failed to create material with non-embedded shader '{0}'!  Exception: {1}", resource.Key, e);
                        }
                    }
                    else
                    {
                        Log.Error("LocateMaterial: Failed to find '{0}'.  Proceeding to next fallback.", resource.Key);
                    }
                }
            }

            // if we manage to get to this point, no appropriate material was found at all!
            Log.Error("LocateMaterial: Failed to find any appropriate shader!");
            return null;
        }

        public static void FlipTexture(Texture2D tex, bool horizontal, bool vertical)
        {
            var originalPixels = tex.GetPixels32();
            Color32[] newPixels = new Color32[originalPixels.Length];

            Log.Debug("FlipTexture: target pixel count {0}", originalPixels.Length);
            Log.Debug("Flip settings: horizontal {0}, vertical {1}", horizontal, vertical);

            for (int y = 0; y < tex.height; ++y)
                for (int x = 0; x < tex.width; ++x)
                {
                    int index = (vertical ? tex.height - y - 1: y) * tex.width + (horizontal ? tex.width - x - 1: x);
                    newPixels[y * tex.width + x] = originalPixels[index];
                }

            tex.SetPixels32(newPixels);
            tex.Apply();
        }

        //public static IsAssemblyLoaded(string FullName)
        //{

        //}

        //public static Type GetAssembly(string FullName)
        //{
        //    //Type assembly = AssemblyLoader.loadedAssemblies.Select(a => a.assembly.GetExportedTypes())
        //    //    .SelectMany(t => t)
        //    //    .FirstOrDefault(t => t.FullName == FullName);
        //    AssemblyLoader.loadedAssemblies
        //}

        /// <summary>
        /// Make a readable copy of the target texture.  If a texture isn't
        /// readable, we can't use functions like EncodeToPNG on it which is
        /// naturally going to be a problem when we're trying to save a copy
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static Texture2D CreateReadable(this UnityEngine.Texture2D original)
        {
            if (original.width == 0 || original.height == 0)
                throw new Exception("CreateReadable: Original has zero width or height or both");

            Texture2D finalTexture = new Texture2D(original.width, original.height);

            // nbTexture isn't read or writeable ... we'll have to get tricksy
            var rt = RenderTexture.GetTemporary(original.width, original.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB, 1);
            Graphics.Blit(original, rt);

            RenderTexture.active = rt;

            finalTexture.ReadPixels(new Rect(0, 0, finalTexture.width, finalTexture.height), 0, 0);

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            return finalTexture;
        }


        public static Texture2D Cutout(this UnityEngine.Texture2D source, Rect src, bool rectIsInUV = false)
        {
            Rect corrected = new Rect(src);

            if (rectIsInUV)
            {
                corrected.x *= source.width;
                corrected.width *= source.width;
                corrected.y *= source.height;
                corrected.height *= source.height;
            }

            return Cutout_Internal(source, corrected);
        }


        //public static Texture2D Cutout(this UnityEngine.Renderer renderer, Mesh mesh)
        //{
        //    var mat = renderer.sharedMaterial;
        //    return ((Texture2D)mat.mainTexture).Cutout(new Rect(mat.mainTextureOffset.x, mat.mainTextureOffset.y, mat.mainTextureScale.x, mat.mainTextureScale.y), true);
        //}
        public static Texture2D Cutout(this UnityEngine.Renderer renderer, Rect uv)
        {
            return ((Texture2D)renderer.sharedMaterial.mainTexture).Cutout(uv, true);
        }

        
        // src expected in pixel space
        private static Texture2D Cutout_Internal(Texture2D source, Rect src, bool secondAttempt = false)
        {
            Texture2D result = new Texture2D(Mathf.FloorToInt(src.width), Mathf.FloorToInt(src.height), TextureFormat.ARGB32, false);

            // source might not be readable
            try
            {
                var pixels = source.GetPixels(Mathf.FloorToInt(src.x), Mathf.FloorToInt(src.y), Mathf.FloorToInt(src.width), Mathf.FloorToInt(src.height));
                result.SetPixels(pixels);
                result.Apply();

                return result;
            }
            catch (Exception e)
            {
                if (secondAttempt)
                {
                    Log.Error("Texture2D.Cutout failed: {0}", e);
                    return null;
                }
                else return Cutout_Internal(source.CreateReadable(), src, true);
            }
        }

        public static void GenerateRandom(this UnityEngine.Texture2D tex)
        {
            var pixels = tex.GetPixels32();

            for (int y = 0; y < tex.height; ++y)
                for (int x = 0; x < tex.width; ++x)
                    pixels[y * tex.width + x] = new Color(UnityEngine.Random.Range(0f, 1f),
                                                            UnityEngine.Random.Range(0f, 1f),
                                                            UnityEngine.Random.Range(0f, 1f),
                                                            UnityEngine.Random.Range(0f, 1f));

            tex.SetPixels32(pixels);
            tex.Apply();
        }

        public static Texture2D GenerateRandom(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
            tex.GenerateRandom();
            return tex;
        }
    }
}
