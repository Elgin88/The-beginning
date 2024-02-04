#if UNITY_EDITOR
#if TERRAWORLD_PRO
//#if TW_TEMPLATES
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TerraUnity.UI;
using TerraUnity.Runtime;


namespace TerraUnity.Edittime.UI
{
    public struct Template
    {
        public string FolderPath;
        public string IconPath;
        public string DescriptionPath;
        public string Description;
        public Texture2D Icon;

        public Template(string folderpath)
        {
            FolderPath = folderpath;

            
            string[] templateIconsPaths = Directory.GetFiles(FolderPath, "*.png", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < templateIconsPaths.Length; i++)
            {
                string graphicPath = TAddresses.GetProjectPath(templateIconsPaths[i]);
                TextureImporter textureImporter = AssetImporter.GetAtPath(graphicPath) as TextureImporter;

                if (textureImporter.textureType != TextureImporterType.Sprite)
                {
                    textureImporter.textureType = TextureImporterType.Sprite;
                    AssetDatabase.ImportAsset(graphicPath, ImportAssetOptions.ForceUpdate);
                }
            }

            if (templateIconsPaths.Length > 0)
            {
                IconPath = templateIconsPaths[templateIconsPaths.Length - 1];
                Icon = AssetDatabase.LoadAssetAtPath(TAddresses.GetProjectPath(IconPath), typeof(Texture2D)) as Texture2D;
            }
            else
            {
                IconPath = "Assets/TerraWorld/Core/Templates/Editor/Contents/NoThumbnail.png";
                Icon = AssetDatabase.LoadAssetAtPath(TAddresses.GetProjectPath(IconPath), typeof(Texture2D)) as Texture2D;
            }

            string[] templateDescriptionsPaths = Directory.GetFiles(FolderPath, "*.txt", SearchOption.TopDirectoryOnly);

            if (templateDescriptionsPaths.Length > 0)
            {
                DescriptionPath = templateDescriptionsPaths[templateDescriptionsPaths.Length - 1];
                Description = File.ReadAllText(DescriptionPath);
            }
            else
            {
                DescriptionPath = "Assets/TerraWorld/Core/Templates/Editor/Contents/NoDescription.txt";
                Description = Path.GetFileName(FolderPath);

            }
        }

    }
    public struct TemplateCategory
    {
        public string Name;
        public string FolderPath;
        public List<Template> Templates;

        public TemplateCategory(string folderpath)
        {
            Name = Path.GetFileName(folderpath);

            string tempnum = Name.Substring(0, 2);
            int temoInt = -1;

            if (Int32.TryParse(tempnum, out temoInt))
                Name = Name.Substring(2, Name.Length - 2);

            FolderPath = Path.GetFullPath(folderpath);
            Templates = new List<Template>();

            try
            {
                string templateFullPath = Path.GetFullPath(FolderPath);
                string[] xxx = Directory.GetDirectories(templateFullPath);
                List<string> _ValidtemplatePath = new List<string>();
                for (int j = 0; j < xxx.Length; j++)
                {
                    string _templateFullPath = Path.GetFullPath(xxx[j]);
                    //string[] _templateIconNames = Directory.GetFiles(_templateFullPath, "*.png", SearchOption.TopDirectoryOnly);
                    //string[] _templateTxt = Directory.GetFiles(_templateFullPath, "*.txt", SearchOption.TopDirectoryOnly);
                    string[] _templateGraphs = Directory.GetFiles(_templateFullPath, "*.twg", SearchOption.TopDirectoryOnly);
                    string[] _templateLinks = Directory.GetFiles(_templateFullPath, "*.link", SearchOption.TopDirectoryOnly);

                    if (_templateGraphs.Length > 0 || _templateLinks.Length > 0)
                    {
                        _ValidtemplatePath.Add(_templateFullPath);
                    }
                }
                for (int i = 0; i < _ValidtemplatePath.Count; i++)
                {
                    Template template = new Template(_ValidtemplatePath[i]);
                    Templates.Add(template);
                }
            }
#if TERRAWORLD_DEBUG
            catch (Exception e)
            {
                throw e;
            }
#else
            catch { }
#endif

        }

    }

    public class TemplatesUI : EditorWindow
    {
        // Generic Params
        private static TerraWorld TerraWorldUI;
        private static GUIStyle style;
        private static Rect lastRect;
        private static int buttonWidth = 102; // 90
        private static int buttonHeight = 134; // 118
        //private static int offsetButtonWidth = 35;
        //private static int offsetButtonHeight = 134;
        //private static int totalItems = 4;
        //private static int offset = 0;
        private static Vector2 scrollPosition = Vector2.zero;
        private static int heightExpand = 21;

        // Section Params
        private static int loadedTypeIndex;
        private static int loadedTemplateIndex;
        private static int templateTypeIndex = 0;
        private static List<TemplateCategory> TemplateCategories = new List<TemplateCategory>();
        private static string[] templateTypesList;



        public static bool SectionUI(TerraWorld TWUI)
        {

            TerraWorldUI = TWUI;
            THelpersUI.GUI_HelpBoxTitleLeft(new GUIContent("TEMPLATES", "Select between available biome types based on selected area"), -10, TWUI.enabledColor, THelpersUI.UIColor);

            style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 42; style.fixedHeight = 42;

            if (TTemplatesManager.Status == TemplateManagerStatus.FetchingFolders)
                ShowSyncMessage(TWUI.windowWidth);
            else
            {
                if (TTemplatesManager.NeedsReRendering)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(TTemplatesManager.notificationMessage)) DisplayNotification(TTemplatesManager.notificationMessage);
                        InitTemplatesUI();
                        TTemplatesManager.NeedsReRendering = false;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
                else
                {
                    ShowSyncButton();

                    if (TemplateCategories == null || TemplateCategories.Count == 0)
                    {
                        THelpersUI.GUI_HelpBox("\nPRESS THE SYNC BUTTON AT ABOVE TO UPDATE TEMPLATES LIST!\n", MessageType.Info);
                    }
                    else
                    {
                        ShowTypesGUI();

                        if (templateTypeIndex > 0)
                        {
                            ShowTepmplatesGUI(TWUI);
                        }

                        if (TTemplatesManager.PackageDownloadProgress != 0 && TTemplatesManager.PackageDownloadProgress != 1)
                        {
                            ShowDownloadingGUI(TWUI.windowWidth);
                        }
                        else
                        {
                            ShowForumGUI();
                        }

                    }

                }
            }

            THelpersUI.DrawUILine(10);
            if (TTemplatesManager.Status == TemplateManagerStatus.Idle  || TTemplatesManager.Status == TemplateManagerStatus.Error)
                return true;
            else
                return false;
        }

        private static void OpenHelloWorldPage()
        {
            Help.BrowseURL("https://terraunity.com/community/forum/templates/");
        }

        private static void ShowSyncMessage(float windowWidth)
        {
            THelpersUI.GUI_HelpBox("\nSYNCING TEMPLATES FROM TERRA SERVERS, PLEASE WAIT...\n", MessageType.Warning);

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (TTemplatesManager.TemplatesSyncProgress != 0 && TTemplatesManager.TemplatesSyncProgress != 1)
            {
                float padding = 20f;
                lastRect = GUILayoutUtility.GetLastRect();
                lastRect.x = padding / 2f;
                lastRect.width = windowWidth - padding - 44;
                lastRect.height = 18;
                EditorGUI.ProgressBar(lastRect, TTemplatesManager.TemplatesSyncProgress, "SYNC IN PROGRESS...");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private static void ShowDownloadingGUI(float windowWidth)
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            float padding = 20f;
            lastRect = GUILayoutUtility.GetLastRect();
            lastRect.x = padding / 2f;
            lastRect.width = windowWidth - padding - 24 - 65;
            lastRect.height = 18;
            EditorGUI.ProgressBar(lastRect, TTemplatesManager.PackageDownloadProgress, "TEMPLATE DOWNLOAD IN PROGRESS...");
            GUILayout.FlexibleSpace();
            style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 60; style.fixedHeight = 18;
            THelpersUI.GUI_Button(new GUIContent("CANCEL", "Cancel current download?"), style, CancelDownload, (int)lastRect.width - 5);
            EditorGUILayout.EndHorizontal();
        }

        private static void ShowForumGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 200; style.fixedHeight = 28;
            THelpersUI.GUI_Button(new GUIContent("VISIT TEMPLATES FORUM", "Visit \"Templates Forum\" to rate, leave comments or download template packages for offline use"), style, OpenHelloWorldPage);
            EditorGUILayout.EndHorizontal();
        }

        private static void ShowSyncButton()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = new Color(1, 1, 1, 0.1f);

            if (GUILayout.Button(new GUIContent(TResourcesManager.syncIcon, "Sync templates from server"), GUILayout.Width(40), GUILayout.Height(40)))
            {
                if (TTemplatesManager.Status != TemplateManagerStatus.FetchingFolders && TTemplatesManager.Status != TemplateManagerStatus.Downloading && TTemplatesManager.Status != TemplateManagerStatus.Importing)
                {
                    TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "SyncTemplates");
                    TTemplatesManager.SyncTemplatesFromServer();
                }
                else
                    EditorUtility.DisplayDialog("SECTION IS BUSY", "This section is currently busy!\n\nPlease wait until active operations are finished and try again!", "OK");
            }

            GUI.backgroundColor = Color.white;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        private static void ShowTypesGUI()
        {
            GUILayout.Space(10);
            EditorGUI.BeginChangeCheck();
            GetTemplateIdentifier();
            if (templateTypeIndex < 1 || (templateTypeIndex > TemplateCategories.Count)) SetTemplateIdentifier(0);
            templateTypeIndex = THelpersUI.GUI_PopupCentered("TYPES", templateTypeIndex, templateTypesList, -10);
            if (EditorGUI.EndChangeCheck())
            {
                if (templateTypeIndex == 0) //templateTypeIndex = 1;
                    GetTemplateIdentifier();
                else
                    SetTemplateIdentifier(templateTypeIndex);
            }
        }

        private static void ShowTepmplatesGUI(TerraWorld TWUI)
        {
            GUILayout.Space(20);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.Height(buttonHeight + heightExpand));

            GUI.backgroundColor = new Color(1, 1, 1, 0.5f);

            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GetLoadedTemplateIdentifiers();
                for (int i = 0; i < TemplateCategories[templateTypeIndex - 1].Templates.Count; i++)
                {
                    if (templateTypeIndex == loadedTypeIndex && i == loadedTemplateIndex)
                        GUI.color = Color.white;
                    else
                        GUI.color = new Color(1, 1, 1, 0.4f);

                    if (TemplateCategories[templateTypeIndex - 1].Templates[i].Icon != null)
                    {
                        if (GUILayout.Button(new GUIContent(TemplateCategories[templateTypeIndex - 1].Templates[i].Icon, TemplateCategories[templateTypeIndex - 1].Templates[i].Description), GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
                        {
                            //TODO: Check if world is in progress too and avoid continue if world generation is active

                            if (TTemplatesManager.Status != TemplateManagerStatus.Downloading)
                            {
                                bool offlineMode = Event.current.control;
                                if (EditorUtility.DisplayDialog("LOAD TEMPLATE", "Are you sure you want to reset graph and load new template?", "Yes", "No"))
                                {
                                    TDebug.Initialize();
                                    //string templatePathAbsolute = Path.GetDirectoryName(TemplateCategories[templateTypeIndex - 1].Templates[i].IconPath);
                                    string templatePathAbsolute = TemplateCategories[templateTypeIndex - 1].Templates[i].FolderPath;
                                    SetLoadedTemplateIdentifiers(templateTypeIndex, i);

                                    if (!offlineMode)
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                        TTemplatesManager.TryDownloadTemplatePackage(templatePathAbsolute);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                    else
                                        TTemplatesManager.RunPackageInOfflineMode(templatePathAbsolute);
                                }
                            }
                            else
                                EditorUtility.DisplayDialog("DOWNLOAD IN PROGRESS", "Selected template content is currently downloading!\n\nPlease wait until download is finished and try again!", "OK");
                        }
                    }
                    else
                        TTemplatesManager.NeedsReRendering = true;

                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            GUI.color = TWUI.enabledColor;
            GUI.backgroundColor = TWUI.enabledColor;
            GUI.contentColor = TWUI.enabledColor;

            EditorGUILayout.EndScrollView();


        }

        private static void CancelDownload()
        {
            if (EditorUtility.DisplayDialog("CANCEL DOWNLOAD", "Are you sure you want to cancel package download?\n\nTerraWorld needs to download selected template's package from TERRA servers to generate world in your scene!", "Yes", "No"))
                TTemplatesManager.CancelDownload2();
        }

        private static void DisplayNotification(string s)
        {
            if (!string.IsNullOrEmpty(s)) EditorUtility.DisplayDialog("TERRAWORLD SERVER SYNC", s, "OK");
        }

        private static void ScanGraphics()
        {
            AssetDatabase.Refresh();
            string[] graphics = Directory.GetFiles(TAddresses.templatesPath_Pro, "*.png", SearchOption.AllDirectories);

            for (int i = 0; i < graphics.Length; i++)
            {
                string graphicPath = TAddresses.GetProjectPath(graphics[i]);
                TextureImporter textureImporter = AssetImporter.GetAtPath(graphicPath) as TextureImporter;

                if (textureImporter.textureType != TextureImporterType.Sprite)
                {
                    textureImporter.textureType = TextureImporterType.Sprite;
                    AssetDatabase.ImportAsset(graphicPath, ImportAssetOptions.ForceUpdate);
                }
            }

            AssetDatabase.Refresh();
        }

        public static void InitTemplatesUI()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            UpdateTemplateCategories();
            GetLoadedTemplateIdentifiers();
            GetTemplateIdentifier();
        }

        private static void GetLoadedTemplateIdentifiers()
        {
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString("loadedtemplateIdentifiers")))
            {
                loadedTypeIndex = int.Parse(PlayerPrefs.GetString("loadedtemplateIdentifiers").Split('_')[0]);
                loadedTemplateIndex = int.Parse(PlayerPrefs.GetString("loadedtemplateIdentifiers").Split('_')[1]);

                //   if (loadedTypeIndex == -1)
                //       templateTypeIndex = 0;
                //   else
                //       templateTypeIndex = loadedTypeIndex;
            }
            else
            {

                loadedTypeIndex = -1;
                loadedTemplateIndex = -1;
                SetLoadedTemplateIdentifiers(loadedTypeIndex, loadedTemplateIndex);
                //templateTypeIndex = 0;
            }
        }

        private static void GetTemplateIdentifier()
        {
            try
            {
                templateTypeIndex = PlayerPrefs.GetInt("newtemplateIdentifier");
            }
            catch (Exception)
            {
                templateTypeIndex = 0;
            }

        }

        private static void SetLoadedTemplateIdentifiers(int type, int index)
        {
            string templateIdentifier = type + "_" + index;
            PlayerPrefs.SetString("loadedtemplateIdentifiers", templateIdentifier);
            PlayerPrefs.Save();
            GetLoadedTemplateIdentifiers();
        }

        private static void SetTemplateIdentifier(int typeIndex)
        {
            PlayerPrefs.SetInt("newtemplateIdentifier", typeIndex);
            PlayerPrefs.Save();
            GetTemplateIdentifier();
        }

        public static void CustomGraphLoaded()
        {
            loadedTypeIndex = -1;
            loadedTemplateIndex = -1;
            templateTypeIndex = 0;
            SetLoadedTemplateIdentifiers(loadedTypeIndex, loadedTemplateIndex);
            SetTemplateIdentifier(templateTypeIndex);
        }

        private static void UpdateTemplateCategories()
        {

            AssetDatabase.Refresh();
            string[] FullPaths = Directory.GetDirectories(TAddresses.templatesPath_Pro); ;

            if (TTemplatesManager.NeedsReRendering)
            {
                if (TemplateCategories == null)
                    TemplateCategories = new List<TemplateCategory>();
                else
                    TemplateCategories.Clear();

                for (int i = 0; i < FullPaths.Length; i++)
                {
                    string[] xxx = Directory.GetDirectories(FullPaths[i]);
                    bool TemplateCategoryHasAValidFolder = false;

                    for (int j = 0; j < xxx.Length; j++)
                    {
                        string templateFullPath = Path.GetFullPath(xxx[j]);
                        //string[] templateIconNames = Directory.GetFiles(templateFullPath, "*.png", SearchOption.TopDirectoryOnly);
                        //string[] templateTxt = Directory.GetFiles(templateFullPath, "*.txt", SearchOption.TopDirectoryOnly);
                        string[] templateGraphs = Directory.GetFiles(templateFullPath, "*.twg", SearchOption.TopDirectoryOnly);
                        string[] templateLinks = Directory.GetFiles(templateFullPath, "*.link", SearchOption.TopDirectoryOnly);

                        if (templateGraphs.Length > 0 || templateLinks.Length > 0)
                            TemplateCategoryHasAValidFolder = true;
                        if (TemplateCategoryHasAValidFolder) break;
                    }

                    if (TemplateCategoryHasAValidFolder)
                    {
                        TemplateCategory templateCategory = new TemplateCategory(FullPaths[i]);
                        TemplateCategories.Add(templateCategory);
                    }
                }

                templateTypesList = new string[TemplateCategories.Count + 1];
                templateTypesList[0] = "NO TEMPLATES LOADED";


                for (int i = 0; i < TemplateCategories.Count; i++)
                {
                    string templateCategoryName = TemplateCategories[i].Name.Replace("New", "");
                    templateTypesList[i + 1] = templateCategoryName;
                }

            }
            AssetDatabase.Refresh();
        }

    }
}
//#endif
#endif
#endif

