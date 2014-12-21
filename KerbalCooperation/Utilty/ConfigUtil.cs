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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using System.Linq;

namespace ReeperCommon
{
    public static class ConfigUtil
    {


        public static T ParseEnum<T>(this ConfigNode node, string valueName, T defaultValue)
        {
            try
            {
                var value = node.GetValue(valueName);

                if (string.IsNullOrEmpty(value))
                {
                    Log.Error("Settings: Value enum '{0}' does not exist in given ConfigNode", valueName);
                    return defaultValue;
                }

                var values = Enum.GetValues(typeof(T));

                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch (Exception e)
            {
                Log.Error("Settings: Failed to parse value '{0}' from ConfigNode, resulted in an exception {1}", valueName, e);
            }

            return defaultValue;
        }



        public static string Parse(this ConfigNode node, string valueName, string defaultValue = "")
        {
            try
            {
                if (!node.HasValue(valueName))
                    return defaultValue;

                return node.GetValue(valueName);
            } catch (Exception e)
            {
                Log.Error("Settings: Failed to parse string value '{0}' from ConfigNode, resulted in an exception {1}", valueName, e);
            }

            return defaultValue;
        }



        public static T Parse<T>(string value)
        {
            return Parse(value, default(T));
        }



        public static T Parse<T>(string value, T defaultValue)
        {
            try
            {
                var method = typeof(T).GetMethod("TryParse", new[] {
                    typeof (string),
                    typeof(T).MakeByRefType()
                });

                if (method == null)
                {
                    Log.Error("Failed to locate TryParse in {0}", typeof(T).FullName);
                }
                else
                {
                    object[] args = new object[] { value, default(T) };

                    if ((bool)method.Invoke(null, args))
                    {
                        return (T)args[1];
                    }
                    else
                    {
                        Log.Error("ConfigUtil.Parse<{0}>: TryParse failed with value '{1}'", typeof(T).FullName, value);
                        return defaultValue;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("ConfigUtil.Parse<{0}>: Failed to parse from value '{1}': {2}", typeof(T).FullName, value, e);
                return defaultValue;
            }

            return defaultValue;
        }


        public static T ParseThrowable<T>(string value)
        {
            try
            {
                var method = typeof(T).GetMethod("TryParse", new[] {
                    typeof (string),
                    typeof(T).MakeByRefType()
                });

                if (method == null)
                {
                    Log.Error("Failed to locate TryParse in {0}", typeof(T).FullName);
                    throw new Exception("TryParse method not found");
                }
                else
                {
                    object[] args = new object[] { value, default(T) };

                    if ((bool)method.Invoke(null, args))
                    {
                        return (T)args[1];
                    }
                    else
                    {
                        Log.Error("ConfigUtil.Parse<{0}>: TryParse failed with value '{1}'", typeof(T).FullName, value);
                        throw new Exception("TryParse invoke reports failure");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("ConfigUtil.Parse<{0}>: Failed to parse from value '{1}': {2}", typeof(T).FullName, value, e);
                throw;
            }
        }

        public static T Parse<T>(this ConfigNode node, string valueName, T defaultValue)
        {
            try
            {
                if (!node.HasValue(valueName))
                {
                    Log.Error("ConfigUtil.Parse<{0}>: Node does not have a value named {1}", typeof(T).FullName, valueName);
                    Log.Debug("ConfigNode = {0}", node.ToString());

                    return defaultValue;
                }
                else
                {
                    string value = node.GetValue(valueName);
                    return Parse(value, defaultValue);
                }
            }
            catch (Exception e)
            {
                Log.Error("ConfigUtil.Parse<{0}>: Exception while parsing a value named {1}: {2}", typeof(T).FullName, valueName, e);
            }

            return defaultValue;
        }


        //public static T Parse<T>(this ConfigNode node, string valueName, T defaultValue)
        //{
        //    try
        //    {
        //        if (!node.HasValue(valueName))
        //        {
        //            Log.Error("Settings: Value '{0}' does not exist in given ConfigNode", valueName);
        //            return defaultValue;
        //        }

        //        var value = node.GetValue(valueName);

        //        var method = typeof(T).GetMethod("TryParse", new[] {
        //            typeof (string),
        //            typeof(T).MakeByRefType()
        //        });

        //        if (method == null)
        //        {
        //            Log.Error("Failed to locate TryParse in {0}", typeof(T).FullName);
        //        }
        //        else
        //        {
        //            object[] args = new object[] { value, default(T) };

        //            if ((bool)method.Invoke(null, args))
        //            {
        //                //Log.Debug("Examined {0}, parse returned {1}", value, args[1]);
        //                return (T)args[1];
        //            }
        //            else
        //            {
        //                Log.Error("Settings: TryParse failed with node name '{0}' (returned value '{1}'", valueName, string.IsNullOrEmpty(valueName) ? "[null]" : value);
        //                return defaultValue;
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Error("Settings: Failed to parse value '{0}' from ConfigNode, resulted in an exception {1}", valueName, e);
        //    }

        //    return defaultValue;
        //}

        public static string ReadString(this ConfigNode node, string valueName, string defaultValue = "")
        {
            if (node == null || !node.HasValue(valueName)) return defaultValue;

            return node.GetValue(valueName);
        }


        public static void Set(this ConfigNode node, string valueName, string value)
        {
            // something seems to be broken with ConfigNode.SetValue
            //if (!node.SetValue(valueName, value)) node.SetValue(valueName, value);

            if (node.HasValue(valueName))
                node.SetValue(valueName, value);
            else node.AddValue(valueName, value);
        }

        public static void Set<T>(this ConfigNode node, string valueName, T value)
        {
            node.Set(valueName, value.ToString());
        }

        //public static bool ParseRect(string strRect, out Rect rect)
        //{
        //    rect = new Rect();

        //    // format: (x:0.00, y:0.00, width:0.25, height:0.25)
        //    if (!strRect.StartsWith("Rect("))
        //    {
        //        Log.Error("ParseRect: '{0}' does not appear to be a rect string", strRect);
        //    }

        //    return true;
        //}

        /// <summary>
        /// Returns an absolute path to the directory this DLL resides in
        /// ex: C:/program files (x86)/steam/steamapps/common/kerbal space program/GameData/NavBallTextureExport
        /// </summary>
        /// <returns></returns>
        public static string GetDllDirectoryPath()
        {
            //string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            //UriBuilder uri = new UriBuilder(codeBase);
            //string path = Uri.UnescapeDataString(uri.Path);
            //return Path.GetDirectoryName(path);
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }



        /// <summary>
        /// This function, given a directory path, will attempt to find a Uri relative
        /// to GameData that will be appropriate to use for the GetTexture function.
        /// 
        /// ex: given C:/program files (x86)/steam/steamapps/common/kerbal space program/GameData/NavBallTextureExport
        /// result would be "NavBallTextureExport" as its parent is GameData
        /// 
        /// if instead it were given "GameData/foo/bar", it would return "foo/bar"
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetRelativeToGameData(string path)
        {
            // just in case the path we're given doesn't actually
            // contain a GameData folder
            if (!path.Contains("GameData"))
            {
                Debug.LogError(string.Format("GetRelativeToGameData: Given path '{0}' does not reside in GameData.  The plugin does not appear to be installed correctly.", path));
                throw new FormatException(string.Format("GetRelativeToGameData: path '{0}' does not contain 'GameData'", path));
            }

            int index = path.IndexOf("GameData");

            string relative = "";

            if (path.Length > index + "GameData".Length + 1)
                relative = path.Substring(index + "GameData".Length + 1);

            Log.Debug(string.Format("Relative path: {0}", relative));

            return relative;
        }


        public static Rect ReadRect(this ConfigNode node, string name, Rect defaultValue = new Rect())
        {
            if (!node.HasValue(name))
            {
                Log.Error("ConfigUtil.ReadRect: value '{0}' does not exist", name);
            }
            else
            {
                try
                {
                    var parsed = KSPUtil.ParseVector4(node.GetValue(name));
                    return new Rect(parsed.x, parsed.y, parsed.z, parsed.w);
                } catch (Exception e)
                {
                    Log.Error("ConfigUtil.ReadRect: exception while reading value '{0}': {1}", name, e);
                }
            }

            return defaultValue;
        }


        public static Vector4 AsVector(this UnityEngine.Rect rect)
        {
            return new Vector4(rect.x, rect.y, rect.width, rect.height);
        }

        // Works but turned out to be unnecessary
        //public static void LoadFieldsFromConfig(this BaseFieldList fields, ConfigNode node)
        //{
        //    for (int i = 0; i < fields.Count; ++i)
        //    {
        //        BaseField field = fields[i];

        //        if (node.HasValue(field.name))
        //        {
        //            Log.Write("type of {0} = {1}", field.name, field.FieldInfo.FieldType.Name);
        //            var ft = field.FieldInfo.FieldType;

        //            try {
        //                Log.Debug("Creating params");
        //                object[] methodParams = new object[] { node, field.name, Activator.CreateInstance(ft) };

        //                Log.Debug("Invoking method");

        //                var parseMethod = typeof(ConfigUtil).GetMethod("Parse");
        //                if (parseMethod == null) Log.Error("Failed to find parse method!");

        //                var parseT = parseMethod.MakeGenericMethod(ft);
        //                if (parseT == null) Log.Error("Failed to create generic method!");

        //                var result = typeof(ConfigUtil).GetMethod("Parse").MakeGenericMethod(ft).Invoke(null, methodParams);

        //                Log.Debug("Setting value");
        //                field.SetValue(result, field.host);
        //                field.SetOriginalValue();

        //                Log.Write("Set '{0}' to value '{1}'", field.name, result.ToString());
        //            } 
        //            catch (Exception e)
        //            {
        //                Log.Error("Exception occurred in LoadFieldsFromConfig: {0}", e);
        //            }
        //        }
        //        else Log.Debug("LoadFieldsFromConfig: Node does not have a value for '{0}'", field.name);
        //    }
        //}
    }


    namespace ConfigNodeSerialization
    {
        /// <summary>
        /// Fields or properties tagged with this attribute won't be serialized by CreateConfigFromObjectEx
        /// or deserialized by CreateObjectFromConfigEx
        /// </summary>
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Property)]
        internal class DoNotSerialize : Attribute
        {
        }



        /// <summary>
        /// This attribute will help document members of a class inside the generated ConfigNode
        /// </summary>
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
        internal class HelpDoc : Attribute
        {
            string doc = "No documentation set";

            public HelpDoc(string doc)
            {
                this.doc = doc;
            }

            public string Documentation { get { return doc; } }
        }



        /// <summary>
        /// This attribute will cause the values for the field or attribute it's been applied to
        /// to be put inside a subsection of the ConfigNode being generated for an object. This is mainly
        /// for user readability and efficient organization
        /// </summary>
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        internal class Subsection : Attribute
        {
            string sectionName = "Subsection";

            public Subsection(string name)
            {
                sectionName = name;
                if (string.IsNullOrEmpty(name)) sectionName = "Subsection";
            }

            public string Section { get { return sectionName; } }
        }


        /// <summary>
        /// Objects which implement this interface will have these functions called whenver
        /// they're used in CreateConfigFromObject/reverse
        /// </summary>
        internal interface IReeperSerializable
        {
            void OnSerialize(ConfigNode node);
            void OnDeserialize(ConfigNode node);
        }


        /// <summary>
        /// This interface is implemented by formatters, special objects which help serialize non-trivial
        /// Types such as Vector2, Quaternion, etc. 
        /// </summary>
        internal interface IConfigNodeTypeFormatter
        {
            string Serialize(object obj);
            object Deserialize(object obj, string value);
        }


        /// <summary>
        /// This object contains some default formatter objects plus any custom formatters
        /// </summary>
        internal class ConfigNodeTypeHandler
        {
            Dictionary<Type, IConfigNodeTypeFormatter> handlers = new Dictionary<Type, IConfigNodeTypeFormatter>();


            internal class Vector2Formatter : IConfigNodeTypeFormatter
            {
                public string Serialize(object obj)
                {
                    Log.Debug("Vector2Formatter.Serialize: got " + obj.ToString());
                    return KSPUtil.WriteVector((Vector2)obj);
                }

                public object Deserialize(object obj, string value)
                {
                    Log.Debug("Vector2Formatter.Deserialize: got value " + value);
                    var v = (Vector2)obj;
                    v = KSPUtil.ParseVector2(value);

                   // v.x = parsed.x;
                    //v.y = parsed.y;

                    Log.Debug("Vector2Formatter.Deserialize: Vector is now " + v.ToString());
                    return v;
                }
            }


            internal ConfigNodeTypeHandler()
            {
                AddFormatter(typeof(Vector2), typeof(Vector2Formatter));
            }

            internal void AddFormatter(Type targetType, IConfigNodeTypeFormatter impl)
            {
                if (handlers.ContainsKey(targetType))
                {
                    Log.Verbose("ConfigNodeTypeHandler: Already have handler for type '{0}', it will be overwritten.", targetType.FullName);
                    handlers[targetType] = impl;
                }
                else
                {
                    handlers.Add(targetType, impl);
                }
            }



            internal void AddFormatter(Type targetType, Type formatter)
            {
                try
                {
                    if (typeof(IConfigNodeTypeFormatter).IsAssignableFrom(formatter))
                    {
                        // create an instance that we'll keep ourselves
                        var impl = (IConfigNodeTypeFormatter)Activator.CreateInstance(formatter);

                        if (handlers.ContainsKey(targetType))
                        {
                            Log.Verbose("ConfigNodeTypeHandler: Already have handler for type '{0}', it will be overwritten.", targetType.FullName);
                            handlers[targetType] = impl;
                        }
                        else handlers.Add(targetType, impl);
                    }
                    else Log.Error("ConfigNodeTypeHandler.AddFormatter: '{0}' is not a valid formatter implementation!", formatter.FullName);
                }
                catch (Exception e)
                {
                    Log.Error("ConfigNodeTypeHandler.AddFormatter: Exception while attempting to add handler for type '{0}' (of type {1}): {2}", targetType.FullName, formatter.FullName, e);
                }
            }


            /// <summary>
            /// Serialize obj to a string format we can store in a ConfigNode
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="obj"></param>
            /// <returns></returns>
            internal string Serialize<T>(ref T obj)
            {
                var fieldType = typeof(T);
                Log.Verbose("Serialize.fieldType = " + fieldType.Name);

                // always prefer type handlers if one is available
                if (handlers.ContainsKey(fieldType))
                {
                    Log.Verbose("ConfigNodeTypeHandler: Found formatter for field type {0}", fieldType.Name);

                    var formatter = handlers[fieldType];

                    return formatter.Serialize(obj);
                }
                else
                {
                    if (fieldType.IsEnum)
                    {
                        return obj.ToString(); // we can easily convert this anyway
                    }
                    else
                    {
                        Log.Verbose("ConfigNodeTypeHandler: No custom formatter found for type {0}, using ToString", typeof(T).FullName);
                        return obj.ToString();
                    }
                }
            }



            /// <summary>
            /// Attempt to deserialize value into obj
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="obj"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            internal bool Deserialize<T>(ref T obj, string value)
            {
                if (handlers.ContainsKey(typeof(T)))
                {
                    Log.Verbose("ConfigNodeTypeHandler: Found formatter for field type {0}, with value '{1}'", typeof(T).Name, value);

                    Log.Debug("Obj before formatter: " + obj.ToString());
                    object tmp = handlers[typeof(T)].Deserialize(obj, value);

                    if (tmp != null)
                    {
                        Log.Debug("Obj after formatter: " + tmp.ToString());
                        obj = (T)tmp; return true;
                    }
                    else return false;
                }
                else
                {
                    if (typeof(T).IsEnum)
                    {
                        try
                        {
                            obj = (T)Enum.Parse(typeof(T), value, true);
                            return true;
                        }
                        catch (Exception) {
                            Log.Error("Failed to parse Enum {0} from value '{1}'", typeof(T).Name, value);
                            return false; 
                        }
                    }
                    else
                    {
                        // try one of our parse methods
                        try
                        {
                            Log.Debug("obj before parsing {0}: {1}", value, obj.ToString());
                            obj = ConfigUtil.ParseThrowable<T>(value);
                            Log.Debug("obj after parsing: {0}", obj.ToString());

                            return true;
                        }
                        catch (Exception) {
                            Log.Error("ConfigNodeTypeHandler: No specified handler for {0}; could not parse {1}", typeof(T).Name, value);
                            return false; 
                        }
                    }
                }
            }
        }



        public static class ReeperConfigNodeExtensions
        {
            /// <summary>
            /// Creates a ConfigNode out of the specified object, capturing non-static fields and properties that
            /// have both a setter and getter
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="typeFormatter"></param>
            /// <returns></returns>
            internal static ConfigNode CreateConfigFromObjectEx(this object obj, ConfigNodeTypeHandler typeFormatter = null)
            {
                try
                {
                    ConfigNode n = new ConfigNode(obj.GetType().Name);
                    typeFormatter = typeFormatter ?? new ConfigNodeTypeHandler();

                    
                    // check if typeformatter has a serialize method for this type


                    #region serialize fields

                    var fields = GetObjectFields(obj);

                    foreach (var field in fields)
                    {
                        System.Object[] attributes = field.GetCustomAttributes(false);

                        var result = field.GetValue(obj);

                        if (result != null)
                        {
                            // check for ConfigNodes; they're a special case
                            if (typeof(ConfigNode).IsAssignableFrom(field.FieldType))
                            {
                                Log.Debug("{0} is a ConfigNode", field.Name);

                                // store ConfigNode in a subnode (so ConfigNode "test" would be stored in
                                // a subnode called "test" which then contains a copy of the ConfigNode). Do
                                // it this way so that any possible name the ConfigNode has doesn't collide
                                //
                                // why not rename it to field.name? we're not sure whether the node name matters
                                // to whichever objects are storing data there and we don't want to impose any 
                                // arbitrary restrictions if we can avoid doing so
                                var subnode = new ConfigNode(field.Name);


                                //if (ss != null)
                                //    subnode = subnode.AddNode(ss.Section);

                                var copy = ((ConfigNode)Convert.ChangeType(result, typeof(ConfigNode))).CreateCopy();
                                if (string.IsNullOrEmpty(copy.name)) copy.name = "ConfigNode";

                                Log.Debug("ConfigNode copy = {0}", copy.ToString());
                                Log.Debug("Original = {0}", ((ConfigNode)Convert.ChangeType(result, typeof(ConfigNode))).ToString());

                                subnode.ClearData();

                                // handle subsection attribute on ConfigNode fields
                                Subsection ss = attributes.SingleOrDefault(attr => attr is Subsection) as Subsection;

                                if (ss == null)
                                {
                                    subnode.AddNode(copy);
                                }
                                else
                                {
                                    subnode.AddNode(ss.Section).AddNode(copy);

                                    Log.Debug("Subnode with subsection = {0}", subnode.ToString());
                                }
                                
                                n.AddNode(subnode);
                            }
                            else
                            {
                                MethodInfo mi = typeFormatter.GetType().GetMethod("Serialize", BindingFlags.Instance | BindingFlags.NonPublic);
                                if (mi == null) Log.Error("CreateConfigFromObjectEx: Serialize method not found");

                                MethodInfo serialize = mi.MakeGenericMethod(field.FieldType);
                                if (serialize == null) Log.Error("CreateConfigFromObjectEx: Failed to create generic method for {0}", field.FieldType.Name);

                                string serialized = serialize.Invoke(typeFormatter, new object[] { result }) as string;

                                if (string.IsNullOrEmpty(serialized))
                                    Log.Warning("ConfigUtil.CreateConfigFromObjectEx: null or empty return value for serialized type {0}", field.FieldType.Name);


                                WriteValue(n, field.Name, serialized, attributes);
                            }
                        }
                        else Log.Warning("Could not get value for " + field.Name);
                    }

                    #endregion

                    #region serialize get/settable properties

                    var properties = GetObjectProperties(obj);


                    foreach (PropertyInfo property in properties)
                    {
                        Log.Verbose("Serializing property {0}, type {1}", property.Name, property.PropertyType.Name);
                        var propertyValue = property.GetGetMethod(true).Invoke(obj, null);
 
                        System.Object[] attributes = property.GetCustomAttributes(true);

                        MethodInfo mi = typeFormatter.GetType().GetMethod("Serialize", BindingFlags.Instance | BindingFlags.NonPublic);
                        if (mi == null)
                        {
                            Log.Error("CreateConfigFromObjectEx: Serialize method not found");
                            continue;
                        }

                        MethodInfo serialize = mi.MakeGenericMethod(property.PropertyType);
                        if (serialize == null) Log.Error("CreateConfigFromObjectEx: Failed to create generic method for {0}", property.PropertyType.Name);

                        string serialized = serialize.Invoke(typeFormatter, new object[] { propertyValue }) as string;

                        if (string.IsNullOrEmpty(serialized))
                            Log.Warning("ConfigUtil.CreateConfigFromObjectEx: null or empty return value for serialized type {0}", property.PropertyType.Name);


                        WriteValue(n, property.Name, serialized, attributes); 
                    }

                    #endregion

                    if (obj is IReeperSerializable)
                        ((IReeperSerializable)obj).OnSerialize(n);

                    return n;
                }
                catch (Exception e)
                {
                    Log.Error("ConfigUtil.CreateConfigFromObjectEx: Exception {0}", e);
                    return null;
                }
            }



            /// <summary>
            /// Deserializes the specified object from a ConfigNode, restoring non-static fields and properties with 
            /// getters and setters
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="node"></param>
            /// <param name="typeFormatter"></param>
            /// <returns></returns>
            internal static bool CreateObjectFromConfigEx(this ConfigNode node, object obj, ConfigNodeTypeHandler typeFormatter = null)
            {
                bool flag = true;
                typeFormatter = typeFormatter ?? new ConfigNodeTypeHandler();


                var fields = GetObjectFields(obj);
                var properties = GetObjectProperties(obj);

                Log.Debug("CreateObjectFromConfig: Found {0} fields and {1} properties", fields.Length, properties.Length);

                #region fields

                foreach (var field in fields)
                {
                    try
                    {
                        System.Object[] attributes = field.GetCustomAttributes(true);

                        if (typeof(ConfigNode).IsAssignableFrom(field.FieldType))
                        {
                            Log.Debug("Field {0} is a ConfigNode", field.Name);

                            if (node.HasNode(field.Name))
                            {
                                ConfigNode target = Convert.ChangeType(field.GetValue(obj) ?? new ConfigNode(), typeof(ConfigNode)) as ConfigNode;
                                ConfigNode subNode = node.GetNode(field.Name);

                                // handle subsection
                                Subsection ss = attributes.SingleOrDefault(attr => attr is Subsection) as Subsection;

                                if (ss != null)
                                {
                                    Log.Debug("ConfigNode field {0} under subsection {1}", field.Name, ss.Section);

                                    if (subNode.HasNode(ss.Section))
                                        subNode = subNode.GetNode(ss.Section);
                                    else Log.Warning("Field {0} tagged with Subsection {1} but no such section exists!", field.Name, ss.Section);
                                }
                                else Log.Debug("ConfigNode field {0} not under any subsection", field.Name);

                                // remember that subnode is the container for the node we wanted
                                if (subNode.CountNodes == 1)
                                {
                                    ConfigNode data = subNode.nodes[0]; // here's what we're interested in

                                    field.SetValue(obj, data);
                                    Log.Verbose("Successfully deserialized ConfigNode {0}", field.Name);
                                } else Log.Warning("Storage for ConfigNode {0} looks like it's missing or wrong", field.Name);
                            }
                            else
                            {
                                Log.Warning("No serialized ConfigNode found for ConfigNode {0}", field.Name);
                            }
                        }
                        else
                        {

                            string strValue = ReadValue(node, field.Name, field.GetCustomAttributes(true));

                            if (!string.IsNullOrEmpty(strValue))
                            {
                                Log.Verbose("Parsing field {0}.{1} from {2}", obj.GetType().Name, field.Name, strValue ?? "");

                                MethodInfo mi = typeFormatter.GetType().GetMethod("Deserialize", BindingFlags.Instance | BindingFlags.NonPublic);
                                if (mi == null) Log.Error("CreateObjectFromConfigEx: Failed to locate Deserialize method");

                                MethodInfo deserialize = mi.MakeGenericMethod(field.FieldType);
                                if (deserialize == null) Log.Error("CreateObjectFromConfigEx: Failed to create generic method for field {0}", field.FieldType.Name);

                                if (!(bool)deserialize.Invoke(typeFormatter, new object[] { field.GetValue(obj), strValue }))
                                {
                                    flag = false;
                                    Log.Warning("Failed to deserialize field {0}.{1} from {2}", obj.GetType().Name, field.FieldType.Name, strValue ?? "");
                                }
                                else
                                {
                                    Log.Verbose("Deserialized: {0}", field.GetValue(obj));
                                }
                            }
                            else Log.Warning("CreateObjectFromConfigEx: Empty/null string found for field named {0}, type {1}", field.Name, field.FieldType.Name);
                        }

                    }
                    catch (Exception e)
                    {
                        Log.Error("Exception while deserializing field '{0}': {1}", field.Name, e);
                        flag = false;
                    }
                }

                #endregion

                #region properties

                foreach (var property in properties)
                {
                    try
                    {

                        string strValue = ReadValue(node, property.Name, property.GetCustomAttributes(true));

                        if (!string.IsNullOrEmpty(strValue))
                        {
                            Log.Verbose("Parsing property {0}.{1} from {2}", obj.GetType().Name, property.Name, strValue ?? "");

                            MethodInfo mi = typeFormatter.GetType().GetMethod("Deserialize", BindingFlags.Instance | BindingFlags.NonPublic);
                            if (mi == null) Log.Error("CreateObjectFromConfigEx: Failed to locate Deserialize method");

                            MethodInfo deserialize = mi.MakeGenericMethod(property.PropertyType);
                            if (deserialize == null) Log.Error("CreateObjectFromConfigEx: Failed to create generic method for property {0}", property.PropertyType.Name);

                            // get existing value
                            var existing = Convert.ChangeType(property.GetGetMethod(true).Invoke(obj, null), property.PropertyType);

                            //object[] parameters = new object[] { Activator.CreateInstance(property.PropertyType), strValue };
                            object[] parameters = new object[] { existing, strValue };

                            if (!(bool)deserialize.Invoke(typeFormatter, parameters))
                            {
                                flag = false;
                                Log.Warning("Failed to deserialize property {0}.{1} from {2}", obj.GetType().Name, property.PropertyType.Name, strValue ?? "");
                            }
                            else
                            {
                                // use the actual setter 
                                property.SetValue(obj, parameters[0], BindingFlags.SetProperty | BindingFlags.Instance, null, null, null);
                            }
                        }
                        else
                        {
                            Log.Warning("CreateObjectFromConfigEx: Empty/null string found for property named {0}, type {1}", property.Name, property.PropertyType.Name);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("Exception while deserializing property '{0}': {1}", property.Name, e);
                        flag = false;
                    }
                }

                #endregion

                if (obj is IReeperSerializable)
                    ((IReeperSerializable)obj).OnDeserialize(node);

                return flag && fields.Count() > 0 || (obj is IReeperSerializable);
            }
            


            /// <summary>
            /// Basically does what it says, although it ignores fields flagged with DoNotSerialize and compiler-generated
            /// fields for properties
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            private static FieldInfo[] GetObjectFields(object obj)
            {
                return obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Where(fi =>
                {
                    // ignore compiled generated attributes (like backing fields for properties)
                    // and specially flagged items
                    return !fi.GetCustomAttributes(false).Any(attr =>
                        {
                            return attr is System.Runtime.CompilerServices.CompilerGeneratedAttribute ||
                                   attr is NonSerializedAttribute ||
                                   attr is DoNotSerialize;
                        });
                }).ToArray();
            }



            /// <summary>
            /// Returns properties that have both getters and setters defined
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            private static PropertyInfo[] GetObjectProperties(object obj)
            {
                return obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Where(pi =>
                    {
                        // ignore properties that don't have both a setter and getter
                        // or are flagged not to be serializable
                        return pi.GetGetMethod(true) != null && pi.GetSetMethod(true) != null &&
                            !pi.GetCustomAttributes(true).Any(attr => attr is DoNotSerialize || attr is NonSerializedAttribute);
                    }).ToArray();
            }



            /// <summary>
            /// Writes the specified value string into a ConfigNode as a value named valueName. If any of
            /// the attributes are of Subsection, that value will be written into a subsection of the
            /// ConfigNode with name provided (will be created if necessary)
            /// </summary>
            /// <param name="node"></param>
            /// <param name="valueName"></param>
            /// <param name="value"></param>
            /// <param name="attrs"></param>
            private static void WriteValue(ConfigNode node, string valueName, string value, System.Object[] attrs)
            {
                if (attrs == null) attrs = new System.Object[] { };

                Subsection subsection = attrs.SingleOrDefault(attr => attr is Subsection) as Subsection;

                if (subsection != null)
                {
                    //Log.Debug("valueName {0} with value '{1}' should be in a subsection called '{2}'", valueName, value, subsection.Section);

                    if (node.HasNode(subsection.Section))
                    {
                        //Log.Debug("Already has a section of that name");
                        node = node.GetNode(subsection.Section);
                    }
                    else
                    {
                        Log.Debug("Created a new section for " + subsection.Section);
                        node = node.AddNode(subsection.Section);
                    }
                }

                attrs.ToList().ForEach(attr =>
                    {
                        if (attr is HelpDoc)
                            node.AddValue(string.Format("// {0}", valueName), ((HelpDoc)attr).Documentation);
                    });
                node.AddValue(valueName, value);
            }



            /// <summary>
            /// Reads a given value from the ConfigNode. If a Subsection attribute is provided, this
            /// method will search in a subsection of the ConfigNode for the value
            /// </summary>
            /// <param name="node"></param>
            /// <param name="valueName"></param>
            /// <param name="attrs"></param>
            /// <returns></returns>
            private static string ReadValue(ConfigNode node, string valueName, System.Object[] attrs)
            {
                if (attrs == null) attrs = new System.Object[] { };

                Subsection ss = attrs.SingleOrDefault(attr => attr is Subsection) as Subsection;

                if (ss != null)
                {
                    if (node.HasNode(ss.Section))
                    {
                        node = node.GetNode(ss.Section);
                    }
                    else // uh oh, we expected a subsection ..
                    {
                        Log.Warning("ConfigUtil.ConfigNodeSerialization: Did not find a subsection called '{0}' when looking up value name {1}", ss.Section, valueName);
                    }
                }

                return node.ReadString(valueName);
            }
        }
    }
}
