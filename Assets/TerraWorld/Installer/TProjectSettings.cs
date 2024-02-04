#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.Rendering;
using System.IO;

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public class TProjectSettings 
    {
        static TProjectSettings()
        {
            BuildTarget bt = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup btg = BuildPipeline.GetBuildTargetGroup(bt);
            string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
            PipelineType pipelineType = DetectPipeline();

            switch (pipelineType)
            {
                case PipelineType.Unsupported:
                        if (defineSymbols.Contains("TW_HDRP")) defineSymbols = defineSymbols.Replace("TW_HDRP", "");
                        if (defineSymbols.Contains("TW_URP")) defineSymbols = defineSymbols.Replace("TW_URP", "");
                    break;
                case PipelineType.BuiltInPipeline:
                        if (defineSymbols.Contains("TW_HDRP")) defineSymbols = defineSymbols.Replace("TW_HDRP", "");
                        if (defineSymbols.Contains("TW_URP")) defineSymbols = defineSymbols.Replace("TW_URP", "");
                    break;
                case PipelineType.UniversalPipeline:
                        if (!defineSymbols.Contains("TW_URP")) defineSymbols = defineSymbols + ";TW_URP";
                        if (defineSymbols.Contains("TW_HDRP")) defineSymbols = defineSymbols.Replace("TW_HDRP", "");
                    break;
                case PipelineType.HDPipeline:
                        if (!defineSymbols.Contains("TW_HDRP")) defineSymbols = defineSymbols + ";TW_HDRP";
                        if (defineSymbols.Contains("TW_URP")) defineSymbols = defineSymbols.Replace("TW_URP", "");
                    break;
                default:
                        if (defineSymbols.Contains("TW_HDRP")) defineSymbols = defineSymbols.Replace("TW_HDRP", "");
                        if (defineSymbols.Contains("TW_URP")) defineSymbols = defineSymbols.Replace("TW_URP", "");
                    break;
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, defineSymbols);

            string cscFile = Application.dataPath + Path.DirectorySeparatorChar + "csc.rsp";
            string mscFile = Application.dataPath + Path.DirectorySeparatorChar + "mcs.rsp";

#if !TERRAWORLD_PRO_Exp_FastConnection
            RemoveStringFromFile(cscFile, "-r:System.Configuration.dll");
            RemoveStringFromFile(mscFile, "-r:System.Configuration.dll");
#else
            AddStringToFile(cscFile, "-r:System.Configuration.dll");
            AddStringToFile(mscFile, "-r:System.Configuration.dll");
#endif
        }

        public static bool FeedbackSystem
        {
            set
            {
                if (value)
                    SetRegKeyInt("FeedbackSystem", 1);
                else
                    SetRegKeyInt("FeedbackSystem", 0);
            }
            get
            {
                if (PlayerPrefs.HasKey("FeedbackSystem"))
                {
                    if (GetRegKeyInt("FeedbackSystem",1) == 0) return false; else return true;
                }
                else
                {
                    SetRegKeyInt("FeedbackSystem", 1);
                    return true;
                }
            }
        }

        public static bool CreateNewWorkDirectory
        {
            set
            {
                if (value)
                    SetRegKeyInt("CreateNewWorkDirectory", 1);
                else
                    SetRegKeyInt("CreateNewWorkDirectory", 0);
            }
            get
            {
                if (PlayerPrefs.HasKey("CreateNewWorkDirectory"))
                {
                    if (GetRegKeyInt("CreateNewWorkDirectory", 1) == 0) return false; else return true;
                }
                else
                {
                    SetRegKeyInt("CreateNewWorkDirectory", 1);
                    return true;
                }
            }
        }

        public static bool CleanUpUnderTerrainObjects
        {
            set
            {
                if (value)
                    SetRegKeyInt("CleanUpUnderTerrainObjects", 1);
                else
                    SetRegKeyInt("CleanUpUnderTerrainObjects", 0);
            }
            get
            {
                if (PlayerPrefs.HasKey("CleanUpUnderTerrainObjects"))
                {
                    if (GetRegKeyInt("CleanUpUnderTerrainObjects", 1) == 0)
                        return false;
                    else
                        return true;
                }  
                else
                {
                    SetRegKeyInt("CleanUpUnderTerrainObjects", 1);
                    return true;
                }
            }
        }

        public struct BBoxPoints
        {
           public double Top, Left, Bottom, Right;
        }

        public static BBoxPoints ProjectArea
        {
            set
            {
                SetRegKeyDouble("TWProjectArea_top", value.Top);
                SetRegKeyDouble("TWProjectArea_left", value.Left);
                SetRegKeyDouble("TWProjectArea_bottom", value.Bottom);
                SetRegKeyDouble("TWProjectArea_right", value.Right);
            }
            get
            {
                BBoxPoints bBoxPoints;
                bBoxPoints.Top = GetRegKeyDouble("TWProjectArea_top", 56.682549f);
                bBoxPoints.Left = GetRegKeyDouble("TWProjectArea_left", -5.1186105f);
                bBoxPoints.Bottom = GetRegKeyDouble("TWProjectArea_bottom", 56.66458f);
                bBoxPoints.Right = GetRegKeyDouble("TWProjectArea_right", -5.08590f);
                return bBoxPoints;
            }
        }

        public static bool NewGraphSystem
        {
            set
            {
#if TERRAWORLD_XPRO
                if (!value) SetDefineDirectives(false, "TERRAWORLD_XPRO");
#else
                if (value) SetDefineDirectives(true, "TERRAWORLD_XPRO");
#endif
            }
            get
            {
#if TERRAWORLD_XPRO
                return true;
#else
                return false;
#endif
            }
        }

        public static bool FastConnection
        {
            set
            {
#if TERRAWORLD_PRO_Exp_FastConnection
                if (!value)
                {
                    SetDefineDirectives(false, "TERRAWORLD_PRO_Exp_FastConnection");
                }
#else
                if (value)
                {
                    string cscFile = Application.dataPath + Path.DirectorySeparatorChar + "csc.rsp";
                    string mscFile = Application.dataPath + Path.DirectorySeparatorChar + "mcs.rsp";
                    AddStringToFile(cscFile, "-r:System.Configuration.dll");
                    AddStringToFile(mscFile, "-r:System.Configuration.dll");
                    SetDefineDirectives(true, "TERRAWORLD_PRO_Exp_FastConnection");
                }
#endif
            }
            get
            {
#if TERRAWORLD_PRO_Exp_FastConnection
                return true;
#else
                return false;
#endif
            }
        }

        public static bool DebugMode
        {
            set
            {
#if TERRAWORLD_DEBUG
                if (!value) SetDefineDirectives(false, "TERRAWORLD_DEBUG");
#else
                if (value) SetDefineDirectives(true, "TERRAWORLD_DEBUG");
#endif
            }
            get
            {
#if TERRAWORLD_DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public static bool CacheData
        {
            set
            {
                SetRegKeyBool("CacheData",value);
            }
            get
            {
                return GetRegKeyBool("CacheData", true);
            }
        }

        public static int PreviewResolution
        {
            set
            {
                if (value > 0)
                    SetRegKeyInt("PreviewResolution", value);
            }
            get
            {
                return GetRegKeyInt("PreviewResolution", 128);
            }
        }

        public static int SceneViewGUI
        {
            get
            {
               return GetRegKeyInt("SceneViewGUI",0);
            }
            set
            {
                SetRegKeyInt("SceneViewGUI", value);
            }
        }

        public static string ActiveTemplatePath
        {
            get
            {
                if (PlayerPrefs.HasKey("ActiveTemplatePath"))
                    return PlayerPrefs.GetString("ActiveTemplatePath");
                else
                {
                    return null;
                }
            }
            set
            {
                PlayerPrefs.SetString("ActiveTemplatePath",value);
            }
        }

        public static string LastProjectDirPath
        {
            get
            {
                if (PlayerPrefs.HasKey("LastProjectPath"))
                    return PlayerPrefs.GetString("LastProjectPath");
                else
                {
                    return null;
                }
            }
            set
            {
                PlayerPrefs.SetString("LastProjectPath", value);
            }
        }

        public static string LastSearchedPlace
        {
            get
            {
                if (PlayerPrefs.HasKey("LastSearchedPlace"))
                    return PlayerPrefs.GetString("LastSearchedPlace");
                else
                {
                    return "";
                }
            }
            set
            {
                PlayerPrefs.SetString("LastSearchedPlace", value);
            }
        }

        public static bool SetRegKeyInt(string key, int value)
        {
            try
            {
                PlayerPrefs.SetInt(key, value);
                PlayerPrefs.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SetRegKeyStr(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }

        private static string GetRegKeyStr(string key, string defaultValue)
        {
            if (PlayerPrefs.HasKey(key))
                return PlayerPrefs.GetString(key);
            else
            {
                SetRegKeyStr(key, defaultValue);
                return defaultValue;
            }
        }

        private static void SetRegKeyDouble(string key, double value)
        {
            string valueStr = value.ToString();
            PlayerPrefs.SetString(key, valueStr);
            PlayerPrefs.Save();
        }

        private static double GetRegKeyDouble(string key, double defaultValue)
        {
            try
            {
                if (PlayerPrefs.HasKey(key)) return double.Parse(PlayerPrefs.GetString(key));
                else return defaultValue;
            }
            catch
            {
                return 0;
            }
        }

        public static DateTime LastTimeTemplateSyncCalled
        {
            get
            {
                DateTime result = DateTime.MinValue;

                try
                {
                    string lastTerrWorldRunTime = PlayerPrefs.GetString("LastTimeTemplateSyncCalled");
                    PlayerPrefs.SetString("LastTimeTemplateSyncCalled", DateTime.Now.ToString());
                    DateTime.TryParse(lastTerrWorldRunTime, out result);
                    return result;
                }
                catch
                {
                    return result;
                }
            }
        }

        public static DateTime LastTimeUpdateWindowShowed
        {
            get
            {
                DateTime result = DateTime.MinValue;
                try
                {
                    string lastTerrWorldRunTime = PlayerPrefs.GetString("LastTimeUpdateWindowShowed");
                    PlayerPrefs.SetString("LastTimeUpdateWindowShowed", DateTime.Now.ToString());
                    DateTime.TryParse(lastTerrWorldRunTime, out result);
                    return result;
                }
                catch
                {
                    return result;
                }
            }
        }

        public static int GetRegKeyInt(string key , int defaultValue)
        {
            if (PlayerPrefs.HasKey(key)) return PlayerPrefs.GetInt(key);
            else
            {
                SetRegKeyInt(key, defaultValue);
                return defaultValue;
            }
        }

        public static void SetRegKeyBool(string key, bool value)
        {
            if (value)
                PlayerPrefs.SetInt(key, 1);
            else
                PlayerPrefs.SetInt(key, 0);

            PlayerPrefs.Save();
        }

        private static bool GetRegKeyBool(string key, bool defaultValue)
        {
            if (PlayerPrefs.HasKey(key))
            {
                if (PlayerPrefs.GetInt(key) == 0) return false;
                else return true;
            }
            else
            {
                SetRegKeyBool(key, defaultValue);
                return defaultValue;
            }
        }

        public static bool IsInstalled
        {
            get
            {
                if (GetRegKeyInt("TWInstalled",0) == 1) return true;
                else return false;
            }
        }

        public static int LastTeamMessageNum
        {
            set
            {
                SetRegKeyInt("LastTeamMessageNum", value);
            }
            get
            {
              return GetRegKeyInt("LastTeamMessageNum",0);
            }
        }

        public static int MaxAsyncConnection
        {
            set
            {
                SetRegKeyInt("MaxAsyncConnection", value);
            }
            get
            {
                return GetRegKeyInt("MaxAsyncConnection", 5);
            }
        }

        public static void InstallationCompleted()
        {
            SetRegKeyInt("TWInstalled", 1);
            SetRegKeyInt("TWVERSION", TVersionController.Version);
        }

        public static bool IsInstallationCompleted()
        {
            if (GetInstalledVersion() == TVersionController.Version)
                return true;
            else
                return false;
        }

        public static int GetInstalledVersion()
        {
            return GetRegKeyInt("TWVERSION",0); ;
        }

        public static bool IsIntroWindowShowed()
        {
            if (TVersionController.Version == GetRegKeyInt("IntroWindowShowed",0))
                return true;
            else
                return false;
        }

        public static void SetIntroWindowShowed()
        {
            SetRegKeyInt("IntroWindowShowed", TVersionController.Version);
        }

        public enum PipelineType
        {
            Unsupported,
            BuiltInPipeline,
            UniversalPipeline,
            HDPipeline
        }

        /// <summary>
        /// Returns the type of renderpipeline that is currently running
        /// </summary>
        /// <returns></returns>
        public static PipelineType DetectPipeline()
        {
#if UNITY_2019_1_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                // SRP
                var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();

                if (srpType.Contains("HDRenderPipelineAsset"))
                    return PipelineType.HDPipeline;
                else if (srpType.Contains("UniversalRenderPipelineAsset") || srpType.Contains("LightweightRenderPipelineAsset"))
                    return PipelineType.UniversalPipeline;
                else
                    return PipelineType.Unsupported;
            }

#elif UNITY_2017_1_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                // SRP not supported before 2019
                return PipelineType.Unsupported;
            }
#endif
            // no SRP
            return PipelineType.BuiltInPipeline;
        }

        static void SetDefineDirectives(bool Enable, string define)
        {
            BuildTarget bt = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup btg = BuildPipeline.GetBuildTargetGroup(bt);
            string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
            if (defineSymbols.Contains(define) && !Enable) defineSymbols = defineSymbols.Replace(define, "");
            if (!defineSymbols.Contains(define) && Enable) defineSymbols = defineSymbols + ";" + define + ";";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, defineSymbols);
        }

        public static int InteractiveMapZoomLevel
        {
            set
            {
                if (value > 0 && value < 24)
                    SetRegKeyInt("InteractiveMapZoomLevel", value);
            }
            get
            {
                return GetRegKeyInt("InteractiveMapZoomLevel", 13);
            }
        }

        public static int ActiveGraphIndex
        {
            set
            {
                SetRegKeyInt("ActiveGraphIndex", value);
            }
            get
            {
                return GetRegKeyInt("ActiveGraphIndex", 0);
            }
        }

        public static int ActiveNodeIndex
        {
            set
            {
                SetRegKeyInt("ActiveNodeIndex", value);
            }
            get
            {
                return GetRegKeyInt("ActiveNodeIndex", 0);
            }
        }

        public static void AddStringToFile(string filename, string text)
        {
            // Check if file already exists.     
            if (!File.Exists(filename))
            {
                // Create a new file     
                using (FileStream fs = File.Create(filename))
                {
                }
            }

            if (!File.ReadAllText(filename).Contains(text))
            {
                using (StreamWriter sw = File.AppendText(filename))
                {
                    sw.Write(" " + text);
                }
            }
        }

        public static void RemoveStringFromFile(string filename, string text)
        {
            if (File.Exists(filename))
            {
                string AllText = File.ReadAllText(filename);

                if (AllText.Contains(text))
                {
                    AllText = AllText.Replace(text, "");
                    File.WriteAllText(filename, AllText);
                }
            }
        }
    }
#endif
}
#endif

