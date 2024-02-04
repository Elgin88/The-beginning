#if (TERRAWORLD_PRO || TERRAWORLD_LITE)
//using UnityEngine;
//using UnityEditor;
//using System;
//using System.Drawing;
//using System.IO;

//namespace TerraUnity.Edittime
//{
//    public class InteractiveMapGUI : EditorWindow
//    {
//        public static void Init()
//        {
//            InteractiveMapGUI window = (InteractiveMapGUI)GetWindow(typeof(InteractiveMapGUI));
//            window.position = new Rect(5, 135, 1600, 670);
//        }

//        public TMap InteractiveTMap = null;

//        TMap InteractiveBGTMap = null;
//        int progressPercentage = 0;
//        Vector2 mouse_move;
//        Event key;
//        Vector2 offset_map;
//        Vector2 offset_map3;
//        Vector2 offset_map4;

//        Vector2 offset;
//        UnityEngine.Color[] pixels;
//        Vector2 offset2;
//        float time1;

//        bool zooming = false;
//        double zoom;
//        double zoom1;
//        double zoom2;
//        double zoom3;
//        double zoom4;
//        double zoom_step;
//        double zoom1_step;
//        double zoom2_step;
//        double zoom3_step;
//        double zoom4_step;
//        double zoom_pos;
//        double zoom_pos1;
//        double zoom_pos2;
//        float zoom_pos3;
//        float zoom_pos4;

//        bool request3;
//        bool request4;

//        bool request_load3;
//        bool request_load4;

//        bool animate = false;
//        latlong_class latlong_animate;
//        latlong_class latlong_mouse;

//        bool map_scrolling = false;
//        float save_global_time;
//        public bool focus = false;
//        Vector2 scrollPos;
//        private float mouse_sensivity = 2f;

//        int image_width;
//        int image_height;

//        private float save_global_timer = 5f;

//        //bool request_load3;
//        //bool request_load4;
//        bool map_load3 = false;
//        bool map_load4 = false;
//        public static int map_zoom = 17;
//        int map_zoom_old;
//        Texture2D map3;
//        Texture2D map4;

//        latlong_class map_latlong = new latlong_class();
//        public static latlong_class map_latlong_center = new latlong_class();
//        latlong_class centerCoords = new latlong_class();
//        latlong_class centerCoordsMosaic = new latlong_class();

//        private int frameWidth = 1024; // 1600
//        private int frameHeight = 512; // 800
//        private int tileSize = 256; //400
//        int cropSize = 0; //32
//        string url;

//        Rect areaRect;
//        Vector2 topLeft;
//        Vector2 bottomRight;
//        Vector2 topLeftMosaic;
//        Vector2 bottomRightMosaic;
//        latlong_class coordsTopLft = new latlong_class();
//        latlong_class coordsBotRgt = new latlong_class();
//        latlong_class coordsTopLftPrevious = new latlong_class(90, -180);
//        latlong_class coordsBotRgtPrevious = new latlong_class(-90, 180);
//        latlong_class coordsTopLftMosaic = new latlong_class();
//        latlong_class coordsBotRgtMosaic = new latlong_class();
//        Texture2D centerCross;
//        Material mat;
//        string mouseLocation;
//        float loadedTiles = 0;
//        bool checkedTile1, checkedTile2, checkedTile3, checkedTile4, checkedTile5, checkedTile6, checkedTile7, checkedTile8, checkedTile9, checkedTile10 = false;

//        public static bool showHUD = true;
//        public static bool lockArea = false;
//        public static bool showBounds = true;
//        public static bool showCross = true;

//        //private static WorldArea _worldArea;

//        private int mapSourceIndex = 0;
//        private string[] selectionMode = new string[] { "CENTER", "BBOX" };
//        //private TTerraWorldGraph worldGraph;
//        private static TAreaGraph areaGraph;
//        private int areaSelectionMode = 0;

//        private static WorldArea _worldArea = null;

//        public WorldArea WorldArea { get => _worldArea; }

//        public static void SetWorldArea ( ref WorldArea WorldArea)
//        {
//            _worldArea = WorldArea;
//        }

//        void OnEnable()
//        {
//            SetCrossMarker();
//            //RequestMap();
//            InitParams();
//        }

//        void OnInspectorUpdate()
//        {
//            if (focus) Repaint();
//        }

//        void OnFocus()
//        {
//            focus = true;
//        }

//        void OnLostFocus()
//        {
//            focus = false;
//        }

//        void SetCrossMarker()
//        {
//            centerCross = Resources.Load("TerraUnity/Downloader/CenterCross") as Texture2D;
//            mat = (Material)Resources.Load("TerraUnity/Downloader/CrossMat");
//        }

//        private void InitParams ()
//        {
//            coordsTopLft = new latlong_class(double.Parse(WorldArea.Top), double.Parse(WorldArea.Left));
//            coordsBotRgt = new latlong_class(double.Parse(WorldArea.Bottom), double.Parse(WorldArea.Right));
//            centerCoords = new latlong_class(double.Parse(WorldArea.latitude), double.Parse(WorldArea.longitude));
//        }

//        void OnGUI()
//        {
//            if (map3 == null || map4 == null) return;

//            MapTexturesGUI();
//            GeneralSettingsGUI();
//            if (showHUD) WindowSettingsGUI();
//            StatsGUI();
//            ProgressGUI();
//            EventsGUI();
//        }

//        // Map Textures
//        //---------------------------------------------------------------------------------------------------------------------------------------------------

//        private void MapTexturesGUI ()
//        {
//            key = Event.current;

//            latlong_mouse = TConvertors.pixel_to_latlong(new Vector2(key.mousePosition.x - (position.width / 2) + offset_map.x, key.mousePosition.y - (position.height / 2) - offset_map.y), map_latlong_center, zoom);

//            EditorGUI.DrawPreviewTexture(new Rect(
//                                            (-1 * (zoom_pos4 + 1) * position.width / 2) - zoom_pos4 * position.width / 2 - offset_map4.x
//                                            , (-1 * ((zoom_pos4 + 1) * position.height / 2) - zoom_pos4 * position.height / 2 + offset_map4.y)
//                                            , (zoom_pos4 + 1) * 2 * position.width
//                                            , (zoom_pos4 + 1) * 2 * position.height)
//                                            , map4);

//            EditorGUI.DrawPreviewTexture(new Rect(
//                                            (-1 * (zoom_pos3) * position.width / 2) - offset_map3.x
//                                            , (-1 * (zoom_pos3) * position.height / 2) + offset_map3.y
//                                            , (zoom_pos3 + 1) * position.width
//                                            , (zoom_pos3 + 1) * position.height)
//                                            , map3);

//            if (showBounds)
//            {
//                if (!lockArea)
//                {
//                    coordsTopLft = new latlong_class(double.Parse(WorldArea.Top), double.Parse(WorldArea.Left));
//                    coordsBotRgt = new latlong_class(double.Parse(WorldArea.Bottom), double.Parse(WorldArea.Right));
//                }

//                if (!lockArea)
//                    centerCoords = new latlong_class(double.Parse(WorldArea.latitude), double.Parse(WorldArea.longitude));
//                else
//                    centerCoords = TConvertors.pixel_to_latlong(new Vector2(offset_map.x, -offset_map.y), map_latlong_center, zoom);

//                topLeft = TConvertors.latlong_to_pixel(coordsTopLft, centerCoords, zoom, new Vector2(position.width, position.height));
//                bottomRight = TConvertors.latlong_to_pixel(coordsBotRgt, centerCoords, zoom, new Vector2(position.width, position.height));

//                // Area bounds in mosaic based server
//                //topLeftMosaic = TConvertors.latlong_to_pixel(coordsTopLftMosaic, centerCoords, zoom, new Vector2(position.width, position.height));
//                //bottomRightMosaic = TConvertors.latlong_to_pixel(coordsBotRgtMosaic, centerCoords, zoom, new Vector2(position.width, position.height));

//                areaRect = Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
//                //areaRectMosaic = Rect.MinMaxRect(topLeftMosaic.x, topLeftMosaic.y, bottomRightMosaic.x, bottomRightMosaic.y);

//                EditorGUI.DrawRect(areaRect, new UnityEngine.Color(0f, 0.75f, 0f, 0.25f));

//                // Mosaic Area
//                //EditorGUI.DrawRect(areaRectMosaic, new UnityEngine.Color(1f, 1f, 0f, 0.25f));
//            }

//            if (showCross)
//                EditorGUI.DrawPreviewTexture(new Rect((position.width / 2) - 12, (position.height / 2) - 12, 24, 24), centerCross, mat);
//        }

//        // Window Settings
//        //---------------------------------------------------------------------------------------------------------------------------------------------------

//        private void GeneralSettingsGUI ()
//        {
//            GUILayout.Space(20);

//            GUIStyle style = new GUIStyle(EditorStyles.radioButton);
//            style.normal.textColor = new UnityEngine.Color(0.95f, 0.95f, 0.95f, 1);
//            style.onNormal.textColor = new UnityEngine.Color(0.95f, 0.95f, 0.95f, 1);
//            Rect lastRect = GUILayoutUtility.GetLastRect();
//            lastRect.x = 10;

//            lastRect.width = 120;
//            lastRect.height = 20;

//            EditorGUILayout.BeginHorizontal();
//            lastRect.y += 25;
//            showHUD = GUI.Toggle(lastRect, showHUD, "SHOW HUD", style);
//            lastRect.y += 25;
//            lockArea = GUI.Toggle(lastRect, lockArea, "LOCK AREA", style);
//            lastRect.y += 25;
//            showBounds = GUI.Toggle(lastRect, showBounds, "SHOW BOUNDS", style);
//            lastRect.y += 25;
//            showCross = GUI.Toggle(lastRect, showCross, "SHOW CROSS", style);
//            EditorGUILayout.EndHorizontal();
//        }

//        private void WindowSettingsGUI ()
//        {
//            EditorGUI.BeginChangeCheck();

//            EditorGUILayout.BeginHorizontal();
//            GUILayout.FlexibleSpace();
//            mapSourceIndex = GUILayout.SelectionGrid(mapSourceIndex, TMapManager.mapTypeMode, 2);
//            TTerraWorldGraph._interactiveMapImagerySource = (TMapManager.mapImagerySourceEnum)mapSourceIndex;
//            GUILayout.FlexibleSpace();
//            EditorGUILayout.EndHorizontal();

//            //GUILayout.Space(10);
//            //
//            //EditorGUILayout.BeginHorizontal();
//            //GUILayout.FlexibleSpace();
//            //
//            //switch (TMapManager.MapTypeIndex)
//            //{
//            //    case 0: TMapManager.mapTypeOSM = (TMapManager.mapTypeOSMEnum)EditorGUILayout.EnumPopup(TMapManager.mapTypeOSM); break;
//            //
//            //    //case 0: TMapManager.mapTypeGoogle = (TMapManager.mapTypeGoogleEnum)EditorGUILayout.EnumPopup(TMapManager.mapTypeGoogle); break;
//            //    //case 1: TMapManager.mapTypeBing = (TMapManager.mapTypeBingEnum)EditorGUILayout.EnumPopup(TMapManager.mapTypeBing); break;
//            //    //case 2: TMapManager.mapTypeOSM = (TMapManager.mapTypeOSMEnum)EditorGUILayout.EnumPopup(TMapManager.mapTypeOSM); break;
//            //    //case 3: TMapManager.mapTypeMapQuest = (TMapManager.mapTypeMapQuestEnum)EditorGUILayout.EnumPopup(TMapManager.mapTypeMapQuest); break;
//            //    //case 4: TMapManager.mapTypeMapBox = (TMapManager.mapTypeMapBoxEnum)EditorGUILayout.EnumPopup(TMapManager.mapTypeMapBox); break;
//            //    //case 5: TMapManager.mapTypeYandex = (TMapManager.mapTypeYandexEnum)EditorGUILayout.EnumPopup(TMapManager.mapTypeYandex); break;
//            //}
//            //
//            ////if (TMapManager.MapTypeIndex == 0)
//            ////{
//            ////    //TMapManager.mapSource = TMapManager.mapSourceEnum.google;
//            ////    TMapManager.mapTypeGoogle = (TMapManager.mapTypeGoogleEnum)EditorGUILayout.EnumPopup(TMapManager.mapTypeGoogle);
//            ////    //TMapManager.mapTypeGoogle = (TMapManager.mapTypeGoogleEnum)TMapManager.mapTypeGoogle;
//            ////}
//            ////else if (mapTypeIndex == 1)
//            ////{
//            ////    TMapManager.mapSource = TMapManager.mapSourceEnum.bing;
//            ////    mapTypeBing = (mapTypeBingEnum)EditorGUILayout.EnumPopup(mapTypeBing);
//            ////    TMapManager.mapTypeBing = (TMapManager.mapTypeBingEnum)mapTypeBing;
//            ////}
//            ////else if (mapTypeIndex == 2)
//            ////{
//            ////    TMapManager.mapSource = TMapManager.mapSourceEnum.openstreetmap;
//            ////}
//            ////else if (mapTypeIndex == 3)
//            ////{
//            ////    TMapManager.mapSource = TMapManager.mapSourceEnum.mapquest;
//            ////    mapTypeMapQuest = (mapTypeMapQuestEnum)EditorGUILayout.EnumPopup(mapTypeMapQuest);
//            ////    TMapManager.mapTypeMapQuest = (TMapManager.mapTypeMapQuestEnum)mapTypeMapQuest;
//            ////}
//            ////else if (mapTypeIndex == 4)
//            ////{
//            ////    TMapManager.mapSource = TMapManager.mapSourceEnum.mapbox;
//            ////    mapTypeMapBox = (mapTypeMapBoxEnum)EditorGUILayout.EnumPopup(mapTypeMapBox);
//            ////    TMapManager.mapTypeMapBox = (TMapManager.mapTypeMapBoxEnum)mapTypeMapBox;
//            ////}
//            ////else if (mapTypeIndex == 5)
//            ////{
//            ////    TMapManager.mapSource = TMapManager.mapSourceEnum.yandex;
//            ////    mapTypeYandex = (mapTypeYandexEnum)EditorGUILayout.EnumPopup(mapTypeYandex);
//            ////    TMapManager.mapTypeYandex = (TMapManager.mapTypeYandexEnum)mapTypeYandex;
//            ////}
//            //
//            //GUILayout.FlexibleSpace();
//            //EditorGUILayout.EndHorizontal();

//            if (EditorGUI.EndChangeCheck())
//                RefreshMap();

//            //GUILayout.Space(40);
//            GUILayout.Space(Screen.height - 200);

//            EditorGUILayout.BeginHorizontal();
//            GUILayout.FlexibleSpace();
//            TAreaGraph.areaSelectionMode = GUILayout.SelectionGrid(TAreaGraph.areaSelectionMode, selectionMode, 2);
//            GUILayout.FlexibleSpace();
//            EditorGUILayout.EndHorizontal();

//            if (TAreaGraph.areaSelectionMode == 0)
//                CenterGUI();
//            else if (TAreaGraph.areaSelectionMode == 1)
//                BBoxGUI();
//        }

//        private void CenterGUI()
//        {
//            EditorGUILayout.BeginHorizontal();
//            GUILayout.FlexibleSpace();
//            GUI.backgroundColor = UnityEngine.Color.gray;
//            EditorGUILayout.HelpBox("LAT", MessageType.None);
//            GUI.backgroundColor = UnityEngine.Color.white;

//            EditorGUI.BeginChangeCheck();

//            WorldArea.latitude = EditorGUILayout.DelayedTextField(WorldArea.latitude);

//            GUILayout.Space(5);
//            GUI.backgroundColor = UnityEngine.Color.gray;
//            EditorGUILayout.HelpBox("LON", MessageType.None);
//            GUI.backgroundColor = UnityEngine.Color.white;

//            WorldArea.longitude = EditorGUILayout.DelayedTextField(WorldArea.longitude);

//            if (EditorGUI.EndChangeCheck())
//                RefreshMap();

//            GUILayout.FlexibleSpace();
//            EditorGUILayout.EndHorizontal();

//            GUILayout.Space(20);

//            EditorGUILayout.BeginHorizontal();
//            GUILayout.FlexibleSpace();
//            EditorGUILayout.HelpBox(" WIDTH ", MessageType.None, true);
//            WorldArea.WorldSizeKMLon = EditorGUILayout.Slider(WorldArea.WorldSizeKMLon, 0.01f, 1000.0f);
//            EditorGUILayout.HelpBox("KM", MessageType.None, true);
//            GUILayout.FlexibleSpace();
//            EditorGUILayout.EndHorizontal();

//            EditorGUILayout.BeginHorizontal();
//            GUILayout.FlexibleSpace();
//            EditorGUILayout.HelpBox("LENGTH", MessageType.None, true);
//            WorldArea.WorldSizeKMLat = EditorGUILayout.Slider(WorldArea.WorldSizeKMLat, 0.01f, 1000.0f);
//            EditorGUILayout.HelpBox("KM", MessageType.None, true);
//            GUILayout.FlexibleSpace();
//            EditorGUILayout.EndHorizontal();

//            //GUILayout.Space(15);
//            //
//            //GUI.backgroundColor = UnityEngine.Color.clear;
//            //EditorGUILayout.BeginHorizontal();
//            //GUILayout.FlexibleSpace();
//            //EditorGUILayout.HelpBox("SQUARE AREA", MessageType.None);
//            //GUILayout.FlexibleSpace();
//            //EditorGUILayout.EndHorizontal();
//            //GUI.backgroundColor = UnityEngine.Color.white;
//            //
//            //Rect rectToggle = GUILayoutUtility.GetLastRect();
//            //rectToggle.x = (rectToggle.width / 2f) + 65f;
//            //
//            //TAreaGraph.squareArea = EditorGUI.Toggle(rectToggle, TAreaGraph.squareArea);
//            //
//            //if (TAreaGraph.squareArea)
//            //    worldArea.WorldSizeKMLon = worldArea.WorldSizeKMLat;
//        }

//        private void BBoxGUI()
//        {
//            EditorGUI.BeginChangeCheck();

//            GUI.backgroundColor = UnityEngine.Color.gray;
//            EditorGUILayout.BeginHorizontal();
//            GUILayout.FlexibleSpace();
//            EditorGUILayout.HelpBox("TOP", MessageType.None, true);
//            GUI.backgroundColor = UnityEngine.Color.white;
//            WorldArea.Top = EditorGUILayout.DelayedTextField(WorldArea.Top);

//            if (double.Parse(WorldArea.Top) < double.Parse(WorldArea.Bottom))
//                WorldArea.Top = (double.Parse(WorldArea.Bottom) + 0.1d).ToString();

//            GUILayout.FlexibleSpace();
//            EditorGUILayout.EndHorizontal();

//            GUILayout.Space(10);

//            GUI.backgroundColor = UnityEngine.Color.gray;
//            EditorGUILayout.BeginHorizontal();
//            GUILayout.FlexibleSpace();
//            EditorGUILayout.HelpBox("LFT", MessageType.None, true);
//            GUI.backgroundColor = UnityEngine.Color.white;
//            WorldArea.Left = EditorGUILayout.DelayedTextField(WorldArea.Left);

//            if (double.Parse(WorldArea.Left) > double.Parse(WorldArea.Right))
//                WorldArea.Left = (double.Parse(WorldArea.Right) - 0.1d).ToString();

//            GUILayout.Space(10);

//            GUI.backgroundColor = UnityEngine.Color.gray;
//            EditorGUILayout.HelpBox("RGT", MessageType.None, true);
//            GUI.backgroundColor = UnityEngine.Color.white;
//            WorldArea.Right = EditorGUILayout.DelayedTextField(WorldArea.Right);
//            GUILayout.FlexibleSpace();
//            EditorGUILayout.EndHorizontal();

//            if (double.Parse(WorldArea.Right) < double.Parse(WorldArea.Left))
//                WorldArea.Right = (double.Parse(WorldArea.Left) + 0.1d).ToString();

//            GUILayout.Space(10);

//            GUI.backgroundColor = UnityEngine.Color.gray;
//            EditorGUILayout.BeginHorizontal();
//            GUILayout.FlexibleSpace();
//            EditorGUILayout.HelpBox("BTM", MessageType.None, true);
//            GUI.backgroundColor = UnityEngine.Color.white;
//            WorldArea.Bottom = EditorGUILayout.DelayedTextField(WorldArea.Bottom);
//            GUILayout.FlexibleSpace();
//            EditorGUILayout.EndHorizontal();

//            if (double.Parse(WorldArea.Bottom) > double.Parse(WorldArea.Top))
//                WorldArea.Bottom = (double.Parse(WorldArea.Top) - 0.1d).ToString();

//            if (EditorGUI.EndChangeCheck())
//                RefreshMap();
//        }

//        // Stats
//        //---------------------------------------------------------------------------------------------------------------------------------------------------

//        private void StatsGUI()
//        {
//            GUI.color = UnityEngine.Color.gray;

//            mouseLocation = Math.Round(latlong_mouse.latitude, 9, MidpointRounding.AwayFromZero) +
//            "   " +
//            Math.Round(latlong_mouse.longitude, 9, MidpointRounding.AwayFromZero);

//            //string mapProvider = InteractiveTMap._mapImagerySource.ToString().ToUpper();
//            EditorGUI.HelpBox(new Rect(5, Screen.height - 40, 260, 16), "Zoom: " + map_zoom + "    " + mouseLocation, MessageType.None);

//            EditorGUI.HelpBox
//            (
//                new Rect(Screen.width - 205, Screen.height - 40, 200, 16),
//                "AREA SIZE: " + WorldArea.WorldSizeKMLon.ToString("0.000") + " x " + WorldArea.WorldSizeKMLat.ToString("0.000") + " KM",
//                MessageType.None
//            );
//        }

//        // Loading Progress
//        //---------------------------------------------------------------------------------------------------------------------------------------------------

//        private void ProgressGUI ()
//        {
//            GUI.color = new UnityEngine.Color(0f, 0.75f, 0f, 1);

//            if (InteractiveTMap != null)
//            {
//                //progressPercentage = (int)((InteractiveTMap.Progress + InteractiveBGTMap.Progress) / 2); //(int) Mathf.InverseLerp(0f, 100f, InteractiveTMap.Progress*100);
//                progressPercentage = InteractiveTMap.Progress;
//            }

//            float progressTiles = Mathf.InverseLerp(0f, 105f, progressPercentage) * (position.width - 10);

//            if (progressPercentage < 100)
//                EditorGUI.HelpBox(new Rect(5, 5, progressTiles, 9), "", MessageType.None);

//            GUI.color = UnityEngine.Color.white;
//        }

//        // Events
//        //---------------------------------------------------------------------------------------------------------------------------------------------------

//        private void EventsGUI()
//        {
//            zoom = Math.Log((zoom_pos1 + 1), 2.0) + (double)map_zoom_old;
//            mouse_move = key.delta;

//            if (key.button == 0)
//            {
//                if (key.type == EventType.MouseDown)
//                {
//                    map_scrolling = true;
//                }

//                else if (key.type == EventType.MouseUp)
//                {
//                    map_scrolling = false;
//                }

//                if (key.type == EventType.MouseDrag)
//                {
//                    if (map_scrolling && key.mousePosition.y > 0)
//                    {
//                        animate = false;
//                        move_center(new Vector2(-mouse_move.x / mouse_sensivity, mouse_move.y / mouse_sensivity), true);
//                    }
//                }
//            }

//            if (key.type == EventType.ScrollWheel)
//            {
//                bool zoom_change = false;

//                if (key.delta.y > 0)
//                {
//                    if (map_zoom > 1)
//                    {
//                        if (zoom1 > 0)
//                        {
//                            zoom1 = (zoom1 - 1) / 2;

//                            if (zoom1 < 1)
//                                zoom1 = 0;
//                        }
//                        else if (zoom1 < 0)
//                        {
//                            zoom1_step /= 2;
//                            zoom1 += zoom1_step;
//                        }
//                        else
//                        {
//                            zoom1 = -0.5f;
//                            zoom1_step = -0.5f;
//                        }

//                        if (zoom2 > 0)
//                        {
//                            zoom2 = (zoom2 - 1) / 2;

//                            if (zoom2 < 1)
//                                zoom2 = 0;
//                        }
//                        else if (zoom2 < 0)
//                        {
//                            zoom2_step /= 2;
//                            zoom2 += zoom2_step;
//                        }
//                        else
//                        {
//                            zoom2 = -0.5f;
//                            zoom2_step = -0.5f;
//                        }

//                        if (zoom3 > 0) { zoom3 = (zoom3 - 1) / 2; if (zoom3 < 1) { zoom3 = 0; } }
//                        else if (zoom3 < 0) { zoom3_step /= 2; zoom3 += zoom3_step; } else { zoom3 = -0.5; zoom3_step = -0.5; }

//                        if (zoom4 > 0) { zoom4 = (zoom4 - 1) / 2; if (zoom4 < 1) { zoom4 = 0; } }
//                        else if (zoom4 < 0) { zoom4_step /= 2; zoom4 += zoom4_step; } else { zoom4 = -0.5; zoom4_step = -0.5; }

//                        convert_center();
//                        --map_zoom;
//                        zoom_change = true;
//                        RequestMap_timer();
//                    }
//                }
//                else
//                {
//                    if (map_zoom < 19)
//                    {
//                        if (zoom1 < 0) { zoom1 -= zoom1_step; zoom1_step *= 2; if (zoom1 > -0.5) { zoom1 = 0; } }
//                        else if (zoom1 > 0)
//                        {
//                            zoom1 = (zoom1 * 2) + 1;
//                        }
//                        else { zoom1 = 1; }

//                        if (zoom2 < 0) { zoom2 -= zoom2_step; zoom2_step *= 2; if (zoom2 > -0.5) { zoom2 = 0; } }
//                        else if (zoom2 > 0)
//                        {
//                            zoom2 = (zoom2 * 2) + 1;
//                        }
//                        else { zoom2 = 1; }

//                        if (zoom3 < 0) { zoom3 -= zoom3_step; zoom3_step *= 2; if (zoom3 > -0.5) { zoom3 = 0; } }
//                        else if (zoom3 > 0)
//                        {
//                            zoom3 = (zoom3 * 2) + 1;
//                        }
//                        else { zoom3 = 1; }

//                        if (zoom4 < 0) { zoom4 -= zoom4_step; zoom4_step *= 2; if (zoom4 > -0.5) { zoom4 = 0; } }
//                        else if (zoom4 > 0)
//                        {
//                            zoom4 = (zoom4 * 2) + 1;
//                        }
//                        else { zoom4 = 1; }

//                        convert_center();
//                        ++map_zoom;

//                        zoom_change = true;
//                        RequestMap_timer();
//                    }
//                }

//                if (zoom_change)
//                {
//                    stop_download();
//                    time1 = Time.realtimeSinceStartup;
//                    zooming = true;
//                }
//            }
//        }

//        private void RefreshMap ()
//        {
//            map_latlong_center = new latlong_class(double.Parse(WorldArea.latitude), double.Parse(WorldArea.longitude));
//            map_zoom = WorldArea.zoomLevel;
//            convert_center();
//            RequestMap_timer();
//        }

//        bool move_to_latlong(latlong_class latlong, float speed)
//        {
//            latlong_class latlong_center = TConvertors.pixel_to_latlong(Vector2.zero, map_latlong_center, zoom);

//            Vector2 pixel = TConvertors.latlong_to_pixel(latlong, latlong_center, zoom, new Vector2(position.width, position.height));

//            float delta_x = (pixel.x - (position.width / 2)) - offset_map.x;
//            float delta_y = -((pixel.y - (position.height / 2)) + offset_map.y);

//            if (Mathf.Abs(delta_x) < 0.01f && Mathf.Abs(delta_y) < 0.01f)
//            {
//                map_latlong_center = latlong;
//                offset_map = Vector2.zero;

//                RequestMap();
//                this.Repaint();
//                return true;
//            }

//            delta_x /= (250 / speed);
//            delta_y /= (250 / speed);

//            move_center(new Vector2(delta_x, delta_y), false);

//            return false;
//        }

//        void move_center(Vector2 offset2, bool map)
//        {
//            offset = offset2;
//            offset_map += offset;

//            //if (zoom_pos1 != 0)
//            //    offset_map1 += offset / (float)(zoom_pos1 + 1);
//            //else
//            //    offset_map1 += offset;
//            //
//            //if (zoom_pos2 != 0)
//            //    offset_map2 += offset / (float)(zoom_pos2 + 1);
//            //else
//            //    offset_map2 += offset;

//            if (zoom_pos3 != 0)
//                offset_map3 += offset / (float)(zoom_pos3 + 1);
//            else
//                offset_map3 += offset;

//            if (zoom_pos4 != 0)
//                offset_map4 += offset / (float)(zoom_pos4 + 1);
//            else
//                offset_map4 += offset;

//            if (map) RequestMap_timer();

//            Repaint();
//        }

//        void convert_center()
//        {
//            map_latlong_center = TConvertors.pixel_to_latlong(new Vector2(offset_map.x, -offset_map.y), map_latlong_center, zoom);

//            if(!lockArea)
//            {
//                WorldArea.latitude = map_latlong_center.latitude.ToString();
//                WorldArea.longitude = map_latlong_center.longitude.ToString();
//            }

//            offset_map = Vector2.zero;
//        }

//        void RequestMap_timer()
//        {
//            time1 = Time.realtimeSinceStartup;
//            request3 = true;
//            request4 = true;
//            Repaint();
//        }

//        public void RequestMap()
//        {
//            RequestMap3();
//            RequestMap4();

//            // Preview area in the scene
//            RequestAreaPreview();

//            Repaint();
//        }

//        private void GetMainImage(TMap CurrentMap)
//        {
//            Bitmap tileImage = null;
//            if (CurrentMap.Image != null) tileImage = CurrentMap.Image.Image;

//            if (!map3)
//            {
//                map3 = new Texture2D((int)position.width, (int)position.height);
//                map3.wrapMode = TextureWrapMode.Clamp;
//            }

//            //myExt3.LoadImageIntoTexture(map3);
//            map3.LoadImage(TImageProcessors.ImageToByte(tileImage));

//            map_load3 = true;

//            if (map3.width == tileSize && map3.height == tileSize)
//            {
//                pixels = map3.GetPixels(0, cropSize, tileSize, (tileSize - cropSize));

//                map3.Resize(tileSize, (tileSize - cropSize));
//                map3.SetPixels(0, 0, tileSize, (tileSize - cropSize), pixels);
//                map3.Apply();
//            }

//            zoom3 = 0;
//            zoom_pos3 = 0;
//            offset_map3 = Vector2.zero;
//            Repaint();
//        }

//        private void GetBackgroundImage(TMap CurrentMap)
//        {
//            Bitmap tileImage = null;
//            if (CurrentMap.Image != null)
//              tileImage = CurrentMap.Image.Image;

//            if (!map4)
//            {
//                map4 = new Texture2D((int)position.width, (int)position.height);
//                map4.wrapMode = TextureWrapMode.Clamp;
//            }

//            //myExt3.LoadImageIntoTexture(map3);
//            map4.LoadImage(TImageProcessors.ImageToByte(tileImage));

//            map_load4 = true;

//            if (map4.width == tileSize && map4.height == tileSize)
//            {
//                pixels = map4.GetPixels(0, cropSize, tileSize, (tileSize - cropSize));

//                map4.Resize(tileSize, (tileSize - cropSize));
//                map4.SetPixels(0, 0, tileSize, (tileSize - cropSize), pixels);
//                map4.Apply();
//            }

//            zoom4 = 0;
//            zoom_pos4 = 0;
//            offset_map4 = Vector2.zero;
//            Repaint();
//        }

//        private bool AllTilesInCache()
//        {
//            bool allInCache = false;
//            string tilePath = TAddresses.tempImagePath;
//            latlong_class map_latlongTL = TConvertors.pixel_to_latlong(new Vector2(-(position.width / 2), -(position.height / 2)), map_latlong_center, map_zoom);
//            latlong_class map_latlongBR = TConvertors.pixel_to_latlong(new Vector2((position.width / 2), (position.height / 2)), map_latlong_center, map_zoom);
//            TMap referenceMap = new TMap(map_latlongTL.latitude, map_latlongTL.longitude, map_latlongBR.latitude, map_latlongBR.longitude, null, map_zoom);

//            for (int j = 0; j < referenceMap.yTiles; j++)
//            {
//                for (int i = 0; i < referenceMap.xTiles; i++)
//                {
//                    string rowCol = (referenceMap.colTop + j) + "_" + (referenceMap.rowLeft + i) + "_" + referenceMap._zoomLevel;
//                    string tileName = tilePath + rowCol + ".jpg";

//                    if (!File.Exists(tileName))
//                    {
//                        allInCache = false;
//                        break;
//                    }
//                    else
//                        allInCache = true;
//                }
//            }

//            return allInCache;
//        }

//        void RequestMap3()
//        {
//            latlong_class map_latlongTL = TConvertors.pixel_to_latlong(new Vector2(-(position.width / 2), -(position.height / 2)), map_latlong_center, map_zoom);
//            latlong_class map_latlongBR = TConvertors.pixel_to_latlong(new Vector2((position.width / 2), (position.height / 2)), map_latlong_center, map_zoom);

//            if (InteractiveTMap == null)
//                InteractiveTMap = new TMap(map_latlongTL.latitude, map_latlongTL.longitude, map_latlongBR.latitude, map_latlongBR.longitude, null, map_zoom);
//            else
//            {
//                InteractiveTMap._zoomLevel = map_zoom;
//                InteractiveTMap.SetArea(map_latlongTL.latitude, map_latlongTL.longitude, map_latlongBR.latitude, map_latlongBR.longitude);
//            }

//            // Bypass elevation tiles
//            InteractiveTMap.RequestElevationData = false;
//            // Bypass landcover tiles
//            InteractiveTMap.RequestLandcoverData = false;

//            // Download imagery tiles
//            InteractiveTMap.RequestImageData = true;
//            InteractiveTMap.SaveTilesImagery = true;
//            InteractiveTMap._mapImagerySource = TTerraWorldGraph._interactiveMapImagerySource;
//            InteractiveTMap._OSMImagerySource = TTerraWorldGraph._OSMImagerySource;

//            // Retreive Interactive Map image
//            InteractiveTMap.UpdateMap(GetMainImage);

//            if
//                (
//                coordsTopLft.latitude != coordsTopLftPrevious.latitude ||
//                coordsTopLft.longitude != coordsTopLftPrevious.longitude ||
//                coordsBotRgt.latitude != coordsBotRgtPrevious.latitude ||
//                coordsBotRgt.longitude != coordsBotRgtPrevious.longitude
//                )
//            {
//                TerraWorld.SetCacheStates(true);
//                coordsTopLftPrevious.latitude = coordsTopLft.latitude;
//                coordsTopLftPrevious.longitude = coordsTopLft.longitude;
//                coordsBotRgtPrevious.latitude = coordsBotRgt.latitude;
//                coordsBotRgtPrevious.longitude = coordsBotRgt.longitude;
//            }
//        }

//        public void RequestAreaPreview ()
//        {
//            //if (!TAreaPreview.Visible)
//             //   return;

//            //TArea area = new TArea(double.Parse(WorldArea.Top), double.Parse(WorldArea.Left), double.Parse(WorldArea.Bottom), double.Parse(WorldArea.Right));
//            //TAreaPreview.UpdatePreview3D();
//            //TAreaPreview.RequestArea(area, TAreaPreview.GeneratePreviewWorld, true, true, false, true);
//        }

//        void RequestMap4()
//        {
//            latlong_class map_latlongBGTL = TConvertors.pixel_to_latlong(new Vector2(-(position.width), -(position.height)), map_latlong_center, map_zoom);
//            latlong_class map_latlongBGBR = TConvertors.pixel_to_latlong(new Vector2((position.width), (position.height)), map_latlong_center, map_zoom);

//            if (InteractiveBGTMap == null)
//            {
//                InteractiveBGTMap = new TMap(map_latlongBGTL.latitude, map_latlongBGTL.longitude, map_latlongBGBR.latitude, map_latlongBGBR.longitude, null, map_zoom - 2);
//            }
//            else
//            {
//                InteractiveBGTMap._zoomLevel = map_zoom - 2;
//                InteractiveBGTMap.SetArea(map_latlongBGTL.latitude, map_latlongBGTL.longitude, map_latlongBGBR.latitude, map_latlongBGBR.longitude);
//                //InteractiveBGTMap._zoomLevel = map_zoom - 2;
//            }

//            InteractiveBGTMap.SaveTilesImagery = true;
//            InteractiveBGTMap._mapImagerySource = TMapManager.mapSourceImagery;
//            InteractiveBGTMap.RequestElevationData = false;
//            InteractiveBGTMap.RequestImageData = true;
//            InteractiveBGTMap.RequestLandcoverData = false;
//            InteractiveBGTMap.UpdateMap(GetBackgroundImage);
//        }

//        void stop_download()
//        {
//            stop_download_map3();
//            stop_download_map4();
//        }
//        void stop_download_map3()
//        {
//            if (request_load3)
//            {
//                map_load3 = false;
//            }
//            request_load3 = false;
//        }

//        void stop_download_map4()
//        {
//            if (request_load4)
//            {
//                map_load4 = false;
//            }
//            request_load4 = false;
//        }
//        void Update ()
//        {
//            if (Application.isPlaying)
//            {
//                Close();
//                return;
//            }

//            if (Time.realtimeSinceStartup > save_global_time + (save_global_timer * 60))
//                save_global_time = Time.realtimeSinceStartup;

//            if (zooming)
//            {
//                zoom_pos = Mathf.Lerp((float)zoom_pos, (float)zoom1, 0.1f);

//                zoom_pos1 = Mathf.Lerp((float)zoom_pos1, (float)zoom1, 0.1f);
//                zoom_pos2 = Mathf.Lerp((float)zoom_pos2, (float)zoom2, 0.1f);

//                zoom_pos3 = Mathf.Lerp((float)zoom_pos3, (float)zoom3, 0.1f);
//                zoom_pos4 = Mathf.Lerp((float)zoom_pos4, (float)zoom4, 0.1f);

//                if (Mathf.Abs((float)zoom_pos1 - (float)zoom1) < 0.001f)
//                {
//                    zoom_pos = zoom1;
//                    zoom_pos1 = zoom1;
//                    zoom_pos2 = zoom2;
//                    zoom_pos3 = (float)zoom3;
//                    zoom_pos4 = (float)zoom4;
//                    zooming = false;
//                }

//                Repaint();
//            }

//            if (animate)
//            {
//                if (move_to_latlong(latlong_animate, 45))
//                    animate = false;
//            }

//            if (request3)
//            {
//                float delay = 1.9f;
//                if (AllTilesInCache()) delay = 0.25f;

//                if (Time.realtimeSinceStartup - time1 > delay)
//                {
//                    request3 = false;
//                    convert_center();
//                    RequestMap3();

//                    // Preview area in the scene
//                    RequestAreaPreview();
//                }
//            }

//            if (request4 && Time.realtimeSinceStartup - time1 > 2.1f)
//            {
//                request4 = false;
//                convert_center();
//                RequestMap4();
//            }
//        }
//    }
//}

using UnityEngine;
using UnityEditor;
using System;
using System.Drawing;
using System.IO;
using TerraUnity.Edittime;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TerraUnity.Runtime;


namespace TerraUnity.Edittime.UI
{
    public class InteractiveMapGUI : EditorWindow
    {
        public static void Init()
        {
            Vector2 windowSize = new Vector2(700, 700);
            InteractiveMapGUI mapWindow = (InteractiveMapGUI)GetWindow(typeof(InteractiveMapGUI), false, "Interactive Map", true);
            mapWindow.position = new Rect
                (
                    (Screen.currentResolution.width / 2) - (windowSize.x / 2),
                    (Screen.currentResolution.height / 2) - (windowSize.y / 2),
                    windowSize.x,
                    windowSize.y
                );

            //mapWindow.minSize = new Vector2(windowSize.x, windowSize.y);
            //mapWindow.maxSize = new Vector2(windowSize.x, windowSize.y);
            mapWindow.RefreshMap();
        }

        public TMap InteractiveTMap = null;
        TMap InteractiveBGTMap = null;
        int progressPercentage = 0;
        Vector2 mouse_move;
        Event key;
        Vector2 offset_map;
        Vector2 offset_map3;
        Vector2 offset_map4;

        Vector2 offset;
        UnityEngine.Color[] pixels;
        Vector2 offset2;
        float time1;

        int minZoomLevel = 6;
        int maxZoomLevel = 20;
        bool zooming = false;
        double zoom;
        double zoom1;
     //   double zoom2;

        double zoom3;
        double zoom4;
        //double zoom_step;
        //double zoom1_step;
        //double zoom2_step;
        double zoom3_step;
        double zoom4_step;
        double zoom_pos;
        double zoom_pos1;
        double zoom_pos2;
        float zoom_pos3;
        float zoom_pos4;
        readonly float areaSizeMin = 0.1f;
        readonly float areaSizeMax = 32f;

        bool request3;
        bool request4;

        bool request_load3;
        bool request_load4;

       // bool animate = false;
        //latlong_class latlong_animate;
        latlong_class latlong_mouse;

        bool map_scrolling = false;
        float save_global_time;
        public bool focus = false;
        Vector2 scrollPos;
        private float mouse_sensivity = 2f;

        int image_width;
        int image_height;

        private float save_global_timer = 5f;

        bool map_load3 = false;
        bool map_load4 = false;

        TMapManager.ImageryMapServer _interactiveMapImagerySource = TMapManager.ImageryMapServer.ESRI;
        private int InteractiveMapZoomLevel { get => TProjectSettings.InteractiveMapZoomLevel; set => TProjectSettings.InteractiveMapZoomLevel = value; }

        static int areaSelectionMode;

        int map_zoom_old;

        Texture2D map3;
        Texture2D map4;

        public static latlong_class map_latlong_center = new latlong_class();
        latlong_class centerCoords = new latlong_class();
        //latlong_class centerCoordsMosaic = new latlong_class();

        //private int frameWidth = 1024; // 1600
        //private int frameHeight = 512; // 800
        private int tileSize = 256; //400
        int cropSize = 0; //32
        string url;

        Rect areaRect;
        Vector2 topLeft;
        Vector2 bottomRight;
        //Vector2 topLeftMosaic;
        //Vector2 bottomRightMosaic;
        latlong_class coordsTopLft = new latlong_class();
        latlong_class coordsBotRgt = new latlong_class();
        latlong_class coordsTopLftPrevious = new latlong_class(90, -180);
        latlong_class coordsBotRgtPrevious = new latlong_class(-90, 180);
        //latlong_class coordsTopLftMosaic = new latlong_class();
        //latlong_class coordsBotRgtMosaic = new latlong_class();
        string mouseLocation;
        //float loadedTiles = 0;
        //bool checkedTile1, checkedTile2, checkedTile3, checkedTile4, checkedTile5, checkedTile6, checkedTile7, checkedTile8, checkedTile9, checkedTile10 = false;

        public static bool showLegend = true;
        public static bool showHUD = true;
        public static bool lockArea = false;
        public static bool showBounds = true;
        public static bool showCross = true;
        private int mapSourceIndex = 0;
        private string[] selectionMode = new string[] { "CENTER", "BBOX" };
        private static TAreaGraph areaGraph;
        //private int areaSelectionMode = 0;

        //string _address = "";

       // public WorldArea WorldArea { get => TTerraWorld.WorldGraph.areaGraph.WorldArea; }

        void OnEnable ()
        {
            InitParams();
        }

        void OnInspectorUpdate ()
        {
            if (focus) this.Repaint();
        }

        void OnFocus()
        {
            focus = true;
        }

        void OnLostFocus()
        {
            focus = false;
        }

        void SetCrossMarker()
        {
            TResourcesManager.LoadInteractiveMapResources();
        }

        private void InitParams ()
        {
            SetCrossMarker();
            coordsTopLft = new latlong_class(TTerraWorld.WorldArea.Top, TTerraWorld.WorldArea.Left);
            coordsBotRgt = new latlong_class(TTerraWorld.WorldArea.Bottom, TTerraWorld.WorldArea.Right);
            centerCoords = new latlong_class(TTerraWorld.WorldArea.Latitude, TTerraWorld.WorldArea.Longitude);
            RefreshMap();
            //RequestMap();
        }

        void OnGUI()
        {
            if (map3 == null || map4 == null) return;

            MapTexturesGUI();
            GeneralSettingsGUI();
            if (showHUD) WindowSettingsGUI();
            if (mapSourceIndex == 1 ) OSMLegendTexturesGUI();
            StatsGUI();
            ProgressGUI();
            EventsGUI();
            if (mapSourceIndex == 1) OSMDetailsButtonGUI();
        }

        // Map Textures
        //---------------------------------------------------------------------------------------------------------------------------------------------------

        private void MapTexturesGUI()
        {
            key = Event.current;

            //if (key.type == EventType.KeyDown)
            //RequestMap();

            latlong_mouse = TConvertors.pixel_to_latlong(new Vector2(key.mousePosition.x - (position.width / 2) + offset_map.x, key.mousePosition.y - (position.height / 2) - offset_map.y), map_latlong_center, zoom);

            Rect Rect4 = new Rect(
                                            (-1 * (zoom_pos4 + 1) * position.width / 2) - zoom_pos4 * position.width / 2 - offset_map4.x
                                            , (-1 * ((zoom_pos4 + 1) * position.height / 2) - zoom_pos4 * position.height / 2 + offset_map4.y)
                                            , (zoom_pos4 + 1) * 2 * position.width
                                            , (zoom_pos4 + 1) * 2 * position.height);
           
            Rect Rect3 = new Rect((-1 * (zoom_pos3) * position.width / 2) - offset_map3.x
                                            , (-1 * (zoom_pos3) * position.height / 2) + offset_map3.y
                                            , (zoom_pos3 + 1) * position.width
                                            , (zoom_pos3 + 1) * position.height);



            EditorGUI.DrawPreviewTexture(Rect4, map4);

            EditorGUI.DrawPreviewTexture(Rect3, map3);

            if (showBounds)
            {
                if (!lockArea)
                {
                    coordsTopLft = new latlong_class(TTerraWorld.WorldArea.Top, TTerraWorld.WorldArea.Left);
                    coordsBotRgt = new latlong_class(TTerraWorld.WorldArea.Bottom, TTerraWorld.WorldArea.Right);
                }

                if (!lockArea)
                    centerCoords = new latlong_class(TTerraWorld.WorldArea.Latitude, TTerraWorld.WorldArea.Longitude);
                else
                    centerCoords = TConvertors.pixel_to_latlong(new Vector2(offset_map.x, -offset_map.y), map_latlong_center, zoom);

                topLeft = TConvertors.latlong_to_pixel(coordsTopLft, centerCoords, zoom, new Vector2(position.width, position.height));
                bottomRight = TConvertors.latlong_to_pixel(coordsBotRgt, centerCoords, zoom, new Vector2(position.width, position.height));
                areaRect = Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
                EditorGUI.DrawRect(areaRect, new UnityEngine.Color(0f, 0.75f, 0f, 0.25f));

                // Area bounds in mosaic based server
                //topLeftMosaic = TConvertors.latlong_to_pixel(coordsTopLftMosaic, centerCoords, zoom, new Vector2(position.width, position.height));
                //bottomRightMosaic = TConvertors.latlong_to_pixel(coordsBotRgtMosaic, centerCoords, zoom, new Vector2(position.width, position.height));
                //areaRectMosaic = Rect.MinMaxRect(topLeftMosaic.x, topLeftMosaic.y, bottomRightMosaic.x, bottomRightMosaic.y);
                //EditorGUI.DrawRect(areaRectMosaic, new UnityEngine.Color(1f, 1f, 0f, 0.25f));
            }

            if (showCross)
                EditorGUI.DrawPreviewTexture(new Rect((position.width / 2) - 12, (position.height / 2) - 12, 24, 24), TResourcesManager.centerCross, TResourcesManager.mat);
        }

        private void OSMLegendTexturesGUI()
        {
            if (showLegend)
                EditorGUI.DrawPreviewTexture(new Rect(((int)(position.width*0.8)) , 1, ((int)(position.width * 0.2)), position.height), TResourcesManager.OSMLegend);

       }

        private void OSMDetailsButtonGUI()
        {
            if (GUILayout.Button(new GUIContent("Show details about landcovers")))
            {
                string URL = "https://osmlanduse.org/#" + InteractiveMapZoomLevel + "/" + TTerraWorld.WorldArea.Longitude.ToString() + "/" + TTerraWorld.WorldArea.Latitude.ToString() + "/0/";
                Help.BrowseURL(URL);
            }
        }

        // Window Settings
        //---------------------------------------------------------------------------------------------------------------------------------------------------

        private void GeneralSettingsGUI()
        {
            GUILayout.Space(20);

            GUIStyle style = new GUIStyle(EditorStyles.radioButton);
            style.normal.textColor = new UnityEngine.Color(0.95f, 0.95f, 0.95f, 1);
            style.onNormal.textColor = new UnityEngine.Color(0.95f, 0.95f, 0.95f, 1);
            Rect lastRect = GUILayoutUtility.GetLastRect();
            lastRect.x = 10;

            lastRect.width = 120;
            lastRect.height = 20;

            EditorGUILayout.BeginHorizontal();
            lastRect.y += 25;
            showHUD = GUI.Toggle(lastRect, showHUD, "SHOW HUD", style);
            lastRect.y += 25;
            lockArea = GUI.Toggle(lastRect, lockArea, "LOCK AREA", style);
            lastRect.y += 25;
            showBounds = GUI.Toggle(lastRect, showBounds, "SHOW BOUNDS", style);
            lastRect.y += 25;
            showCross = GUI.Toggle(lastRect, showCross, "SHOW CROSS", style);

            if (mapSourceIndex == 1)
            {
                lastRect.y += 25;
                showLegend = GUI.Toggle(lastRect, showLegend, "SHOW LEGEND", style);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void WindowSettingsGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            mapSourceIndex = GUILayout.SelectionGrid(mapSourceIndex, TMapManager.mapTypeMode, 2);
            if (mapSourceIndex == 0)
                _interactiveMapImagerySource = TMapManager.ImageryMapServer.ESRI;
            else
                _interactiveMapImagerySource = TMapManager.ImageryMapServer.StandardOSM;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            //GUILayout.Space(10);
            //
            //EditorGUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            //
            //switch (TMapManager.MapTypeIndex)
            //{
            //    case 0: TMapManager.mapTypeOSM = (TMapManager.mapTypeOSMEnum)EditorGUILayout.EnumPopup(TMapManager.mapTypeOSM); break;
            //
            //    //case 0: TMapManager.mapTypeGoogle = (TMapManager.mapTypeGoogleEnum)EditorGUILayout.EnumPopup(TMapManager.mapTypeGoogle); break;
            //    //case 1: TMapManager.mapTypeBing = (TMapManager.mapTypeBingEnum)EditorGUILayout.EnumPopup(TMapManager.mapTypeBing); break;
            //    //case 2: TMapManager.mapTypeOSM = (TMapManager.mapTypeOSMEnum)EditorGUILayout.EnumPopup(TMapManager.mapTypeOSM); break;
            //    //case 3: TMapManager.mapTypeMapQuest = (TMapManager.mapTypeMapQuestEnum)EditorGUILayout.EnumPopup(TMapManager.mapTypeMapQuest); break;
            //    //case 4: TMapManager.mapTypeMapBox = (TMapManager.mapTypeMapBoxEnum)EditorGUILayout.EnumPopup(TMapManager.mapTypeMapBox); break;
            //    //case 5: TMapManager.mapTypeYandex = (TMapManager.mapTypeYandexEnum)EditorGUILayout.EnumPopup(TMapManager.mapTypeYandex); break;
            //}
            //
            ////if (TMapManager.MapTypeIndex == 0)
            ////{
            ////    //TMapManager.mapSource = TMapManager.mapSourceEnum.google;
            ////    TMapManager.mapTypeGoogle = (TMapManager.mapTypeGoogleEnum)EditorGUILayout.EnumPopup(TMapManager.mapTypeGoogle);
            ////    //TMapManager.mapTypeGoogle = (TMapManager.mapTypeGoogleEnum)TMapManager.mapTypeGoogle;
            ////}
            ////else if (mapTypeIndex == 1)
            ////{
            ////    TMapManager.mapSource = TMapManager.mapSourceEnum.bing;
            ////    mapTypeBing = (mapTypeBingEnum)EditorGUILayout.EnumPopup(mapTypeBing);
            ////    TMapManager.mapTypeBing = (TMapManager.mapTypeBingEnum)mapTypeBing;
            ////}
            ////else if (mapTypeIndex == 2)
            ////{
            ////    TMapManager.mapSource = TMapManager.mapSourceEnum.openstreetmap;
            ////}
            ////else if (mapTypeIndex == 3)
            ////{
            ////    TMapManager.mapSource = TMapManager.mapSourceEnum.mapquest;
            ////    mapTypeMapQuest = (mapTypeMapQuestEnum)EditorGUILayout.EnumPopup(mapTypeMapQuest);
            ////    TMapManager.mapTypeMapQuest = (TMapManager.mapTypeMapQuestEnum)mapTypeMapQuest;
            ////}
            ////else if (mapTypeIndex == 4)
            ////{
            ////    TMapManager.mapSource = TMapManager.mapSourceEnum.mapbox;
            ////    mapTypeMapBox = (mapTypeMapBoxEnum)EditorGUILayout.EnumPopup(mapTypeMapBox);
            ////    TMapManager.mapTypeMapBox = (TMapManager.mapTypeMapBoxEnum)mapTypeMapBox;
            ////}
            ////else if (mapTypeIndex == 5)
            ////{
            ////    TMapManager.mapSource = TMapManager.mapSourceEnum.yandex;
            ////    mapTypeYandex = (mapTypeYandexEnum)EditorGUILayout.EnumPopup(mapTypeYandex);
            ////    TMapManager.mapTypeYandex = (TMapManager.mapTypeYandexEnum)mapTypeYandex;
            ////}
            //
            //GUILayout.FlexibleSpace();
            //EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
                RefreshMap();



            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUIStyle style = new GUIStyle(EditorStyles.toolbarTextField);
            style.fixedHeight = 30;
            style.fontSize = 13;
            style.alignment = TextAnchor.MiddleCenter;

            GUI.SetNextControlName("SearchBox");
            TProjectSettings.LastSearchedPlace = EditorGUILayout.TextField(TProjectSettings.LastSearchedPlace, style);


            GUILayout.Space(5);

            float yOffset = GUILayoutUtility.GetLastRect().y + 43;
            GUILayout.BeginArea(new Rect((this.position.width / 2) + 88, yOffset, 35, 35));

            GUILayout.Space(65);
            GUI.backgroundColor = new UnityEngine.Color(1, 1, 1, 0.2f);

            if (GUI.GetNameOfFocusedControl() == "SearchBox")
                if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyUp)
                {
                    ShowSearchResultWindow(TProjectSettings.LastSearchedPlace);
                }

            GUILayout.EndArea();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();


            //GUILayout.Space(40);
            GUILayout.Space(this.position.height - 200);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            areaSelectionMode = GUILayout.SelectionGrid(areaSelectionMode, selectionMode, 2);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (areaSelectionMode == 0)
                CenterGUI();
            else 
                BBoxGUI();
        }

        private void CenterGUI()
        {

            double _latitude = TTerraWorld.WorldArea.Latitude;
            double _longitude = TTerraWorld.WorldArea.Longitude;
            float _AreaSizaLat = TTerraWorld.WorldArea.AreaSizeLat;
            float _AreaSizaLon = TTerraWorld.WorldArea.AreaSizeLon;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = UnityEngine.Color.black;
            EditorGUILayout.HelpBox("LAT", MessageType.None);
            GUI.backgroundColor = UnityEngine.Color.white;

            #region AreaChanged
            EditorGUI.BeginChangeCheck(); 
            _latitude = EditorGUILayout.DelayedDoubleField(new GUIContent("", "Latitude coordinate of the area in Decimal Degrees"), _latitude);

            GUILayout.Space(5);
            GUI.backgroundColor = UnityEngine.Color.gray;
            EditorGUILayout.HelpBox("LON", MessageType.None);
            GUI.backgroundColor = UnityEngine.Color.white;

            _longitude = EditorGUILayout.DelayedDoubleField(new GUIContent("", "Longitude coordinate of the area in Decimal Degrees"), _longitude);

            if (EditorGUI.EndChangeCheck())
            {
                TTerraWorld.SetWorldArea(_latitude, _longitude, _AreaSizaLat, _AreaSizaLon);
                RefreshMap();
            }
                
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox(" SIZE ", MessageType.None, true);
            _AreaSizaLat = EditorGUILayout.Slider(_AreaSizaLat, areaSizeMin, areaSizeMax);
            EditorGUILayout.HelpBox("KM", MessageType.None, true);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (_AreaSizaLat > 16f && _AreaSizaLat <= 32f)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.HelpBox("LARGE AREAS WITH BIOME ELEMENTS WON'T BE PRACTICAL!", MessageType.Warning, true);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            else if (_AreaSizaLon > 32f)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.HelpBox("LARGE AREAS WITH BIOME ELEMENTS WON'T BE PRACTICAL!", MessageType.Error, true);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            //EditorGUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            //EditorGUILayout.HelpBox("LENGTH", MessageType.None, true);
            //WorldArea.WorldSizeKMLat = EditorGUILayout.Slider(WorldArea.WorldSizeKMLat, areaSizeMin, areaSizeMax);
            //EditorGUILayout.HelpBox("KM", MessageType.None, true);
            //GUILayout.FlexibleSpace();
            //EditorGUILayout.EndHorizontal();

            //GUILayout.Space(15);
            //
            //GUI.backgroundColor = UnityEngine.Color.clear;
            //EditorGUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            //EditorGUILayout.HelpBox("SQUARE AREA", MessageType.None);
            //GUILayout.FlexibleSpace();
            //EditorGUILayout.EndHorizontal();
            //GUI.backgroundColor = UnityEngine.Color.white;
            //
            //Rect rectToggle = GUILayoutUtility.GetLastRect();
            //rectToggle.x = (rectToggle.width / 2f) + 65f;
            //
            //TAreaGraph.squareArea = EditorGUI.Toggle(rectToggle, TAreaGraph.squareArea);
            //
            //if (TAreaGraph.squareArea)
            //    worldArea.WorldSizeKMLon = worldArea.WorldSizeKMLat;

            if (EditorGUI.EndChangeCheck())
                TTerraWorld.SetWorldArea(_latitude, _longitude, _AreaSizaLat, _AreaSizaLon);
            #endregion AreaChanged
        }

        private void BBoxGUI()
        {

            double _top = TTerraWorld.WorldArea.Top;
            double _left = TTerraWorld.WorldArea.Left;
            double _bottom = TTerraWorld.WorldArea.Bottom;
            double _right = TTerraWorld.WorldArea.Right;

            EditorGUI.BeginChangeCheck();

            GUI.backgroundColor = UnityEngine.Color.gray;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("TOP", MessageType.None, true);
            GUI.backgroundColor = UnityEngine.Color.white;

            // TOP
            _top = EditorGUILayout.DelayedDoubleField(new GUIContent("", "Top Latitude coordinate of the area in Decimal Degrees"), _top);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUI.backgroundColor = UnityEngine.Color.gray;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("LFT", MessageType.None, true);
            GUI.backgroundColor = UnityEngine.Color.white;

            // LEFT
            _left = EditorGUILayout.DelayedDoubleField(new GUIContent("", "Left Longitude coordinate of the area in Decimal Degrees"), _left);

            GUILayout.Space(10);

            GUI.backgroundColor = UnityEngine.Color.gray;
            EditorGUILayout.HelpBox("RGT", MessageType.None, true);
            GUI.backgroundColor = UnityEngine.Color.white;

            // RIGHT
            _right = EditorGUILayout.DelayedDoubleField(new GUIContent("", "Right Longitude coordinate of the area in Decimal Degrees"), _right);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUI.backgroundColor = UnityEngine.Color.gray;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("BTM", MessageType.None, true);
            GUI.backgroundColor = UnityEngine.Color.white;

            // BOTTOM
            _bottom = EditorGUILayout.DelayedDoubleField(new GUIContent("", "Bottom Latitude coordinate of the area in Decimal Degrees"), _bottom);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                TTerraWorld.SetWorldArea(_top, _left, _bottom, _right);
                RefreshMap();
            }
        }

        // Stats
        //---------------------------------------------------------------------------------------------------------------------------------------------------

        private void StatsGUI()
        {
            GUI.color = UnityEngine.Color.gray;

            mouseLocation = Math.Round(latlong_mouse.latitude, 9, MidpointRounding.AwayFromZero) +
            "   " +
            Math.Round(latlong_mouse.longitude, 9, MidpointRounding.AwayFromZero);

            //string mapProvider = InteractiveTMap._mapImagerySource.ToString().ToUpper();
            EditorGUI.HelpBox(new Rect(5, this.position.height - 40, 260, 16), "Zoom: " + InteractiveMapZoomLevel + "    " + mouseLocation, MessageType.None);

            EditorGUI.HelpBox
            (
                new Rect(position.width - 205, position.height - 40, 200, 16),
                "AREA SIZE: " + TTerraWorld.WorldArea.AreaSizeLon.ToString("0.000") + " x " + TTerraWorld.WorldArea.AreaSizeLat.ToString("0.000") + " KM",
                MessageType.None
            );
        }

        // Loading Progress
        //---------------------------------------------------------------------------------------------------------------------------------------------------

        private void ProgressGUI()
        {
            

            if (InteractiveTMap != null)
            {
                //progressPercentage = (int)((InteractiveTMap.Progress + InteractiveBGTMap.Progress) / 2); //(int) Mathf.InverseLerp(0f, 100f, InteractiveTMap.Progress*100);
                progressPercentage = InteractiveTMap.Progress;
            }

            float progressTiles = Mathf.InverseLerp(0f, 105f, progressPercentage) * (position.width - 10);
            progressTiles = Mathf.InverseLerp(0f, 100f, ((progressTiles - position.width*0.5f) * 2));

            if (progressPercentage > 0 && progressPercentage < 100)
            {
                GUIStyle style = new GUIStyle(EditorStyles.helpBox);
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = 18;
                //style.fontStyle = FontStyle.Bold;
                GUIContent gUIContent = new GUIContent("...  LOADING DATA .... PLEASE WAIT ....");
                style.fixedWidth = style.CalcSize(gUIContent).x + 10;
                GUI.color = UnityEngine.Color.white;
                //EditorGUI.HelpBox(new Rect(5, (Mathf.Round(position.height/2) - 30), (105f* (position.width - 10)), 60), " ", MessageType.None);
                GUI.color = new UnityEngine.Color(1f, 1f, 0f, 1);
                EditorGUI.HelpBox(new Rect(5, (Mathf.Round(position.height / 2) - 27), progressTiles, 58), "", MessageType.None);
                GUI.Label(new Rect((Mathf.Round(position.width / 2) - Mathf.Round(style.fixedWidth/2)), (Mathf.Round(position.height / 2) - 27), progressTiles, 58), gUIContent,style);


                //EditorGUILayout.LabelField( Label(gUIContent.text, style);

            }

            GUI.color = UnityEngine.Color.white;
        }

        // Events
        //---------------------------------------------------------------------------------------------------------------------------------------------------

        private void EventsGUI()
        {
            InteractiveMapZoomLevel = Mathf.Clamp(InteractiveMapZoomLevel, minZoomLevel, maxZoomLevel);
            zoom = Math.Log((zoom_pos1 + 1), 2.0) + (double)map_zoom_old;
            mouse_move = key.delta;

            if (key.button == 0)
            {
                if (key.type == EventType.MouseDown)
                {
                    map_scrolling = true;
                }

                else if (key.type == EventType.MouseUp)
                {
                    map_scrolling = false;
                }

                if (key.type == EventType.MouseDrag)
                {
                    if (map_scrolling && key.mousePosition.y > 0)
                    {
                      //  animate = false;
                        move_center(new Vector2(-mouse_move.x / mouse_sensivity, mouse_move.y / mouse_sensivity), true);
                    }
                }
            }

            if (key.type == EventType.ScrollWheel)
            {
                bool zoom_change = false;

                if (key.delta.y > 0)
                {
                    if (InteractiveMapZoomLevel > minZoomLevel)
                    {
                        //if (zoom1 > 0)
                        //{
                        //    zoom1 = (zoom1 - 1) / 2;
                        //
                        //    if (zoom1 < 1)
                        //        zoom1 = 0;
                        //}
                        //else if (zoom1 < 0)
                        //{
                        //    zoom1_step /= 2;
                        //    zoom1 += zoom1_step;
                        //}
                        //else
                        //{
                        //    zoom1 = -0.5f;
                        //    zoom1_step = -0.5f;
                        //}
                        //
                        //if (zoom2 > 0)
                        //{
                        //    zoom2 = (zoom2 - 1) / 2;
                        //
                        //    if (zoom2 < 1)
                        //        zoom2 = 0;
                        //}
                        //else if (zoom2 < 0)
                        //{
                        //    zoom2_step /= 2;
                        //    zoom2 += zoom2_step;
                        //}
                        //else
                        //{
                        //    zoom2 = -0.5f;
                        //    zoom2_step = -0.5f;
                        //}

                        if (zoom3 > 0) { zoom3 = (zoom3 - 1) / 2; if (zoom3 < 1) { zoom3 = 0; } }
                        else if (zoom3 < 0) { zoom3_step /= 2; zoom3 += zoom3_step; } else { zoom3 = -0.5; zoom3_step = -0.5; }

                        if (zoom4 > 0) { zoom4 = (zoom4 - 1) / 2; if (zoom4 < 1) { zoom4 = 0; } }
                        else if (zoom4 < 0) { zoom4_step /= 2; zoom4 += zoom4_step; } else { zoom4 = -0.5; zoom4_step = -0.5; }

                        convert_center();
                        InteractiveMapZoomLevel--;
                        zoom_change = true;
                        RequestMap_timer();
                    }
                }
                else
                {
                    if (InteractiveMapZoomLevel < maxZoomLevel)
                    {
                        //if (zoom1 < 0) { zoom1 -= zoom1_step; zoom1_step *= 2; if (zoom1 > -0.5) { zoom1 = 0; } }
                        //else if (zoom1 > 0)
                        //{
                        //    zoom1 = (zoom1 * 2) + 1;
                        //}
                        //else { zoom1 = 1; }
                        //
                        //if (zoom2 < 0) { zoom2 -= zoom2_step; zoom2_step *= 2; if (zoom2 > -0.5) { zoom2 = 0; } }
                        //else if (zoom2 > 0)
                        //{
                        //    zoom2 = (zoom2 * 2) + 1;
                        //}
                        //else { zoom2 = 1; }

                        if (zoom3 < 0) { zoom3 -= zoom3_step; zoom3_step *= 2; if (zoom3 > -0.5) { zoom3 = 0; } }
                        else if (zoom3 > 0)
                        {
                            zoom3 = (zoom3 * 2) + 1;
                        }
                        else { zoom3 = 1; }

                        if (zoom4 < 0) { zoom4 -= zoom4_step; zoom4_step *= 2; if (zoom4 > -0.5) { zoom4 = 0; } }
                        else if (zoom4 > 0)
                        {
                            zoom4 = (zoom4 * 2) + 1;
                        }
                        else { zoom4 = 1; }

                        convert_center();
                        InteractiveMapZoomLevel++;

                        zoom_change = true;
                        RequestMap_timer();
                    }
                }

                if (zoom_change)
                {
                    stop_download();
                    time1 = Time.realtimeSinceStartup;
                    zooming = true;
                }
            }
        }

        public void RefreshMap()
        {
            map_latlong_center = new latlong_class(TTerraWorld.WorldArea.Latitude, TTerraWorld.WorldArea.Longitude);
            //if (map_zoom < minZoomLevel) map_zoom = WorldArea.zoomLevel;
            convert_center();
            RequestMap_timer();
        }

        bool move_to_latlong(latlong_class latlong, float speed)
        {
            latlong_class latlong_center = TConvertors.pixel_to_latlong(Vector2.zero, map_latlong_center, zoom);

            Vector2 pixel = TConvertors.latlong_to_pixel(latlong, latlong_center, zoom, new Vector2(position.width, position.height));

            float delta_x = (pixel.x - (position.width / 2)) - offset_map.x;
            float delta_y = -((pixel.y - (position.height / 2)) + offset_map.y);

            if (Mathf.Abs(delta_x) < 0.01f && Mathf.Abs(delta_y) < 0.01f)
            {
                map_latlong_center = latlong;
                offset_map = Vector2.zero;

                RefreshMap();
                this.Repaint();
                return true;
            }

            delta_x /= (250 / speed);
            delta_y /= (250 / speed);

            //move_center(new Vector2(delta_x, delta_y), false);

            return false;
        }

        void move_center(Vector2 offset2, bool map)
        {
            offset = offset2;
            offset_map += offset;

            //if (zoom_pos1 != 0)
            //    offset_map1 += offset / (float)(zoom_pos1 + 1);
            //else
            //    offset_map1 += offset;
            //
            //if (zoom_pos2 != 0)
            //    offset_map2 += offset / (float)(zoom_pos2 + 1);
            //else
            //    offset_map2 += offset;

            if (zoom_pos3 != 0)
                offset_map3 += offset / (float)(zoom_pos3 + 1);
            else
                offset_map3 += offset;

            if (zoom_pos4 != 0)
                offset_map4 += offset / (float)(zoom_pos4 + 1);
            else
                offset_map4 += offset;

            if (map)
            {
                stop_download();
                RequestMap_timer();
            }

            this.Repaint();
        }

        void convert_center()
        {
            map_latlong_center = TConvertors.pixel_to_latlong(new Vector2(offset_map.x, -offset_map.y), map_latlong_center, zoom);

            if (!lockArea)
            {
                TTerraWorld.SetWorldArea(map_latlong_center.latitude, map_latlong_center.longitude, TTerraWorld.WorldArea.AreaSizeLat, TTerraWorld.WorldArea.AreaSizeLon);
            }

            offset_map = Vector2.zero;
            //animate = true;
        }

        void RequestMap_timer()
        {
            time1 = Time.realtimeSinceStartup;
            request3 = true;
            request4 = true;

            this.Repaint();
        }

        private void GetMainImage(TMap CurrentMap)
        {
            Bitmap tileImage = null;
            if (CurrentMap.Image != null) tileImage = CurrentMap.Image.Image;
            CurrentMap.Dispose();

            if (!map3)
            {
                map3 = new Texture2D((int)position.width, (int)position.height);
                map3.wrapMode = TextureWrapMode.Clamp;
            }

            //myExt3.LoadImageIntoTexture(map3);
            map3.LoadImage(TImageProcessors.ImageToByte(tileImage));

            map_load3 = true;

            if (map3.width == tileSize && map3.height == tileSize)
            {
                pixels = map3.GetPixels(0, cropSize, tileSize, tileSize - cropSize);

#if UNITY_2021_3_OR_NEWER
                map3.Reinitialize(tileSize, tileSize - cropSize);
#else
                map3.Resize(tileSize, tileSize - cropSize);
#endif

                map3.SetPixels(0, 0, tileSize, tileSize - cropSize, pixels);
                map3.Apply();
            }

            zoom3 = 0;
            zoom_pos3 = 0;
            offset_map3 = Vector2.zero;
            this.Repaint();
        }

        private void GetBackgroundImage(TMap CurrentMap)
        {
            Bitmap tileImage = null;
            if (CurrentMap.Image != null)
                tileImage = CurrentMap.Image.Image;
            CurrentMap.Dispose();

            if (!map4)
            {
                map4 = new Texture2D((int)position.width, (int)position.height);
                map4.wrapMode = TextureWrapMode.Clamp;
            }

            //myExt3.LoadImageIntoTexture(map3);
            map4.LoadImage(TImageProcessors.ImageToByte(tileImage));

            map_load4 = true;

            if (map4.width == tileSize && map4.height == tileSize)
            {
                pixels = map4.GetPixels(0, cropSize, tileSize, tileSize - cropSize);

#if UNITY_2021_3_OR_NEWER
                map4.Reinitialize(tileSize, tileSize - cropSize);
#else
                map4.Resize(tileSize, tileSize - cropSize);
#endif

                map4.SetPixels(0, 0, tileSize, tileSize - cropSize, pixels);
                map4.Apply();
            }

            zoom4 = 0;
            zoom_pos4 = 0;
            offset_map4 = Vector2.zero;
            this.Repaint();
        }

        private bool AllTilesInCache ()
        {
            bool allInCache = false;
            string tilePath = TAddresses.cachePathImagery;
            latlong_class map_latlongTL = TConvertors.pixel_to_latlong(new Vector2(-(position.width / 2), -(position.height / 2)), map_latlong_center, InteractiveMapZoomLevel);
            latlong_class map_latlongBR = TConvertors.pixel_to_latlong(new Vector2((position.width / 2), (position.height / 2)), map_latlong_center, InteractiveMapZoomLevel);
            TMap referenceMap = new TMap(map_latlongTL.latitude, map_latlongTL.longitude, map_latlongBR.latitude, map_latlongBR.longitude, null, InteractiveMapZoomLevel, 0,false ,0);

            for (int j = 0; j < referenceMap.yTilesImagery; j++)
            {
                for (int i = 0; i < referenceMap.xTilesImagery; i++)
                {
                    string rowCol = (referenceMap.colTopImagery + j) + "_" + (referenceMap.rowLeftImagery + i) + "_" + referenceMap._imageryZoomLevel;
                    string tileName = tilePath + rowCol + ".jpg";

                    if (!File.Exists(tileName))
                    {
                        allInCache = false;
                        break;
                    }
                    else
                        allInCache = true;
                }
            }

            return allInCache;
        }

        void RequestMap3()
        {
            latlong_class map_latlongTL = TConvertors.pixel_to_latlong(new Vector2(-(position.width / 2), -(position.height / 2)), map_latlong_center, InteractiveMapZoomLevel);
            latlong_class map_latlongBR = TConvertors.pixel_to_latlong(new Vector2((position.width / 2), (position.height / 2)), map_latlong_center, InteractiveMapZoomLevel);

            //if (InteractiveTMap == null)
            InteractiveTMap?.Dispose();
            InteractiveTMap = new TMap(map_latlongTL.latitude, map_latlongTL.longitude, map_latlongBR.latitude, map_latlongBR.longitude, null, InteractiveMapZoomLevel, 0,false , 0);
            //else
            //{
            //    InteractiveTMap._zoomLevel = map_zoom;
            //    InteractiveTMap.SetArea(map_latlongTL.latitude, map_latlongTL.longitude, map_latlongBR.latitude, map_latlongBR.longitude);
            //}

            // Bypass elevation tiles
            //InteractiveTMap.RequestElevationData = false;
            // Bypass landcover tiles
            //InteractiveTMap.RequestLandcoverData = false;

            // Download imagery tiles
           // InteractiveTMap.RequestImageData = true;
            InteractiveTMap.SaveTilesImagery = true;
            InteractiveTMap._mapImagerySource = _interactiveMapImagerySource;
            //InteractiveTMap._OSMImagerySource = TTerraWorldGraph._OSMImagerySource;

            // Retreive Interactive Map image
            InteractiveTMap.UpdateMap(GetMainImage);

            if
                (
                coordsTopLft.latitude != coordsTopLftPrevious.latitude ||
                coordsTopLft.longitude != coordsTopLftPrevious.longitude ||
                coordsBotRgt.latitude != coordsBotRgtPrevious.latitude ||
                coordsBotRgt.longitude != coordsBotRgtPrevious.longitude
                )
            {
                TerraWorld.SetCacheStates(true);
                coordsTopLftPrevious.latitude = coordsTopLft.latitude;
                coordsTopLftPrevious.longitude = coordsTopLft.longitude;
                coordsBotRgtPrevious.latitude = coordsBotRgt.latitude;
                coordsBotRgtPrevious.longitude = coordsBotRgt.longitude;
            }
        }

        void RequestMap4()
        {
            latlong_class map_latlongBGTL = TConvertors.pixel_to_latlong(new Vector2(-(position.width), -(position.height)), map_latlong_center, InteractiveMapZoomLevel);
            latlong_class map_latlongBGBR = TConvertors.pixel_to_latlong(new Vector2((position.width), (position.height)), map_latlong_center, InteractiveMapZoomLevel);

            // if (InteractiveBGTMap == null)
            InteractiveBGTMap?.Dispose();
                 InteractiveBGTMap = new TMap(map_latlongBGTL.latitude, map_latlongBGTL.longitude, map_latlongBGBR.latitude, map_latlongBGBR.longitude, null, InteractiveMapZoomLevel - 2, 0,false ,0);
           // else
           // {
           //     InteractiveBGTMap._zoomLevel = map_zoom - 2;
           //     InteractiveBGTMap.SetArea(map_latlongBGTL.latitude, map_latlongBGTL.longitude, map_latlongBGBR.latitude, map_latlongBGBR.longitude);
           // }

            InteractiveBGTMap.SaveTilesImagery = true;
//            InteractiveBGTMap._mapImagerySource = TMapManager.mapSourceImagery;
            //InteractiveBGTMap._mapImagerySource = TTerraWorldGraph._interactiveMapImagerySource;
            //InteractiveBGTMap._OSMImagerySource = TTerraWorldGraph._OSMImagerySource;
           // InteractiveBGTMap.RequestElevationData = false;
           // InteractiveBGTMap.RequestImageData = true;
            //InteractiveBGTMap.RequestLandcoverData = false;
            InteractiveBGTMap.UpdateMap(GetBackgroundImage);
        }

        void stop_download()
        {
            stop_download_map3();
            stop_download_map4();
        }

        void stop_download_map3()
        {
            if (request_load3) map_load3 = false;
            request_load3 = false;
        }

        void stop_download_map4()
        {
            if (request_load4) map_load4 = false;
            request_load4 = false;
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                this.Close();
                return;
            }

            if (Time.realtimeSinceStartup > save_global_time + (save_global_timer * 60))
                save_global_time = Time.realtimeSinceStartup;

            if (zooming)
            {
                zoom_pos = Mathf.Lerp((float)zoom_pos, (float)zoom1, 0.1f);

                zoom_pos1 = Mathf.Lerp((float)zoom_pos1, (float)zoom1, 0.1f);
              //  zoom_pos2 = Mathf.Lerp((float)zoom_pos2, (float)zoom2, 0.1f);

                zoom_pos3 = Mathf.Lerp((float)zoom_pos3, (float)zoom3, 0.1f);
                zoom_pos4 = Mathf.Lerp((float)zoom_pos4, (float)zoom4, 0.1f);

                if (Mathf.Abs((float)zoom_pos1 - (float)zoom1) < 0.001f)
                {
                    zoom_pos = zoom1;
                    zoom_pos1 = zoom1;
                 //   zoom_pos2 = zoom2;
                    zoom_pos3 = (float)zoom3;
                    zoom_pos4 = (float)zoom4;
                    zooming = false;
                }

                this.Repaint();
            }

        //   if (animate)
        //   {
        //       if (move_to_latlong(centerCoords, 45));
        //       //if (move_to_latlong(latlong_animate, 45))
        //           animate = false;
        //   }

            if (request3)
            {
                float delay = 1.5f;

                if (AllTilesInCache())
                    delay = 0.0f;

                if (Time.realtimeSinceStartup - time1 > delay)
                {
                    request3 = false;
                    convert_center();
                    RequestMap3();
                }
            }

            if (request4 && Time.realtimeSinceStartup - time1 > 0.5f)
            {
                request4 = false;
                convert_center();
                RequestMap4();
            }

            if (map_load3 && map_load4)
            {
                map_zoom_old = InteractiveMapZoomLevel;
                zoom1 = 0;
                zoom_pos1 = 0;
                this.Repaint();
            }
        }

        private static void ShowSearchResultWindow( string address)
        {
            List<double[]> _coords;
            List<string> _locations;
            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "Search");
            _coords = GeoCoder.AddressToLatLong(Regex.Replace(address, @"\s+", string.Empty));
            _locations = GeoCoder.foundLocations;

            if (_coords != null && _locations != null && GeoCoder.recognized)
            {
                SearchResultsDisplay searchResultsDisplay = (SearchResultsDisplay)GetWindow(typeof(SearchResultsDisplay), false, "Search Results Display", true);
                searchResultsDisplay.position = new Rect(5, 135, 500, 384);
                searchResultsDisplay.coordinates = _coords;
                searchResultsDisplay.locations = _locations;
            }
        }
    }
}

#endif