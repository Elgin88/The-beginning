#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Drawing;
using System.Numerics;
using System.Xml;
using TerraUnity.Utils;
using System.Threading.Tasks;
using UnityEditor;
using TerraUnity.Runtime;

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    //public enum RequestLandcoverDataMode
    //{
    //    None,
    //    Data,
    //    Image,
    //    DataAndImage
    //}

    public class TMap
    {
        public TArea _area, _mosaicAreaElevation, _mosaicAreaImagery, _mosaicAreaLandcover;
        public TMap _ref;
        public TTerrain _refTerrain;
        private Action<TMap> _lastActions;
        private bool _requestImageData = false;
        private bool _requestElevationData = false;
        private bool _requestLandcoverData = false;
        private bool _requestLandcoverImageryData = false;
        //private RequestLandcoverDataMode _requestLandcoverData = RequestLandcoverDataMode.None;
        public TMapManager.ElevationMapServer _mapElevationSource = TMapManager.ElevationMapServer.ESRI;
        public TMapManager.ImageryMapServer _mapImagerySource = TMapManager.ImageryMapServer.ESRI;
        public TMapManager.ImageryMapServer _mapLandCoverImagerySource = TMapManager.ImageryMapServer.StandardOSM;
        public TMapManager.ImageryMapServer _OSMImagerySource = TMapManager.ImageryMapServer.Standard;
        public TMapManager.LandcoverMapServer _mapLandcoverSource = TMapManager.LandcoverMapServer.OSM;

        public int _imageryZoomLevel;
        public int _landcoverZoomLevel;
        private int _elevationZoomLevel;
        private TCommunications Connection = null;

        private int rowLeftElevation, colTopElevation, rowRightElevation, colBottomElevation;
        public int rowLeftImagery, colTopImagery, rowRightImagery, colBottomImagery;
        private int rowLeftLandcover, colTopLandcover, rowRightLandcover, colBottomLandcover;

        private int _progress;
        private bool disposed = false;

        private int xTilesElevation, yTilesElevation;
        public int xTilesImagery, yTilesImagery;
        private int xTilesLandcover, yTilesLandcover;

        public THeightmap _heightmap;
        private TImage _image, _Landcoverimage;
        private XmlDocument _landcoverXML;

        public int _lastError = 0;
        private bool saveTilesImagery = true;
        private bool saveTilesElevation = true;
        private bool saveTilesLandcover = true;
        public int _ID = 0;
        public bool _heightMapAvailableInZoomLevel = false;
        private static int Counter;

        public TMap(Action<TMap> action , TArea area, int ImageryZoomLevel, int ElevationZoomLevel)
                : this(area.Top, area.Left, area.Bottom, area.Right, null, ImageryZoomLevel, ElevationZoomLevel,false, 0)
        {
            TDebug.TraceMessage();
            UpdateMap(action);
        }

        public TMap(double top, double left, double bottom, double right, TTerrain refTerrain , int ImageryZoomLevel, int ElevationZoomLevel,bool requestLandcoverData, int LandcoverZoomLevel)
        {
            TDebug.TraceMessage();
            Random r = new Random((int)DateTime.Now.Ticks);
            _ID = (r.Next() + Counter++);
            _area = new TArea(top, left, bottom, right);
            Initialize(refTerrain, ImageryZoomLevel, ElevationZoomLevel, requestLandcoverData, LandcoverZoomLevel);
        }

        public void Initialize(TTerrain refTerrain, int ImageryzoomLevel, int ElevationZoomLevel, bool requestLandcoverData, int LandcoverZoomLevel)
        {
            TDebug.TraceMessage();
            _elevationZoomLevel = ElevationZoomLevel;
            _imageryZoomLevel = ImageryzoomLevel;
            _landcoverZoomLevel = LandcoverZoomLevel;
            _requestLandcoverData = requestLandcoverData;

            if (_elevationZoomLevel < 5)
                RequestElevationData = false;
            else
            {
                RequestElevationData = true;
                GetBBoxMosaicElevation(_elevationZoomLevel);
            }

            if (_imageryZoomLevel < 5)
                RequestImageData = false;
            else
            {
                RequestImageData = true;
                GetBBoxMosaicImagery(_imageryZoomLevel);
            }

            if (LandcoverZoomLevel < 5)
                _requestLandcoverImageryData = false;
            else
            {
                _requestLandcoverImageryData = true;
                GetBBoxMosaicLandcover(_landcoverZoomLevel);
            }

            //if (LandcoverZoomLevel < 5)
            //{
            //    switch (RequestLandcoverData)
            //    {
            //        case RequestLandcoverDataMode.None:
            //            break;
            //        case RequestLandcoverDataMode.Data:
            //            break;
            //        case RequestLandcoverDataMode.Image:
            //            RequestLandcoverData = RequestLandcoverDataMode.None;
            //            break;
            //        case RequestLandcoverDataMode.DataAndImage:
            //            RequestLandcoverData = RequestLandcoverDataMode.Data;
            //            break;
            //        default:
            //            break;
            //    }
            //
            //} else
            //{
            //    switch (RequestLandcoverData)
            //    {
            //        case RequestLandcoverDataMode.None:
            //            RequestLandcoverData = RequestLandcoverDataMode.Image;
            //            break;
            //        case RequestLandcoverDataMode.Data:
            //            RequestLandcoverData = RequestLandcoverDataMode.DataAndImage;
            //            break;
            //        case RequestLandcoverDataMode.Image:
            //            break;
            //        case RequestLandcoverDataMode.DataAndImage:
            //            break;
            //        default:
            //            break;
            //    }
            //    GetBBoxMosaicLandcover(_landcoverZoomLevel);
            //}

            _ref = this;
            _refTerrain = refTerrain;
            _heightmap = new THeightmap();
            _image = new TImage();
            _Landcoverimage = new TImage();
        }

        public void SetHeightMap(float[,] HeightMapsrc)
        {
            _heightmap = new THeightmap(HeightMapsrc);
        }

        public THeightmap Heightmap
        {
            get
            {
                if (_lastError == 0)
                    return _ref._heightmap;
                else
                    return null;
            }
            set
            {
                _ref._heightmap = value;
            }
        }

        public TImage Image
        {
            get
            {
                if (_lastError == 0)
                    return _ref._image;
                else
                    return null;
            }
            set
            {
                _ref._image = value;
            }
        }

        public TImage LandCoverImage
        {
            get
            {
                if (_lastError == 0)
                    return _ref._Landcoverimage;
                else
                    return null;
            }
            set
            {
                _ref._Landcoverimage = value;
            }
        }

        public XmlDocument LandcoverXML
        {
            get
            {
                if (_lastError == 0)
                    return _ref._landcoverXML;
                else
                    return null;
            }
            set
            {
                _ref._landcoverXML = value;
            }
        }

        /*
        public int ProgressElevation
        {
            get { return _progressElevation; }
            set
            {
                _progressElevation = value;
                UpdateProgress();
            }
        }

        public int ProgressImagery
        {
            get { return _progressImagery; }
            set
            {
                _progressImagery = value;
                UpdateProgress();
            }
        }

        public int ProgressLandcover
        {
            get { return _progressLandcover; }
            set
            {
                _progressLandcover = value;
                UpdateProgress();
            }
        }



        public void UpdateProgress()
        {
            int requestCount = 0;
    
            if (RequestImageData)
                requestCount++;
    
            if (RequestElevationData)
                requestCount++;
    
            if (RequestLandcoverData)
                requestCount++;
    
            if (requestCount == 0) _progress = 100;
    
            _progress = (_progressLandcover + _progressElevation + _progressImagery) / requestCount;
            _progress = (int) TUtils.Clamp(0, 100, _progress);
    
            if (_refTerrain != null ) _refTerrain.Progress = 0;
        }
        */

        public int Progress
        {
            get
            {
                UpdateProgress2();
                return _progress;
            }
        }

        public void UpdateProgress2()
        {
            if (Connection != null)
                _progress = (int)(Connection.Progress * 100);
            else
                _progress = -1;

            _progress = (int)TUtils.Clamp(-1, 100, _progress);
        }

        public bool SaveTilesImagery { get => saveTilesImagery; set => saveTilesImagery = value; }
        public bool SaveTilesElevation { get => saveTilesElevation; set => saveTilesElevation = value; }
        public bool SaveTilesLandcover { get => saveTilesLandcover; set => saveTilesLandcover = value; }
        private bool RequestImageData { get => _requestImageData; set { _requestImageData = value;} }
        private bool RequestElevationData { get => _requestElevationData; set => _requestElevationData = value; }
        //private RequestLandcoverDataMode RequestLandcoverData { get => _requestLandcoverData; set => _requestLandcoverData = value; }



        public void UpdateMap(Action<TMap> act = null)
        {
            TDebug.TraceMessage();

            if (act != null) _lastActions = act;

            if (RequestElevationData)
            {
                int newElevationZoomLevel = AvailableHeighmapZoomLevel(_elevationZoomLevel);
                Initialize(_refTerrain, _imageryZoomLevel, newElevationZoomLevel, _requestLandcoverData, _landcoverZoomLevel);
            }


          //  if (Connection != null)
          //  {
          //      //Connection.CancelActiveConnection();
          //      Connection = null;
          //  }

#if (TERRAWORLD_PRO_Exp_FastConnection)
            TConnectionsManager.SetAsyncConnections();
#endif

            Connection = new TCommunications
            (
                AfterDataDownloaded,
                xTilesImagery, yTilesImagery, colTopImagery, rowLeftImagery, _imageryZoomLevel,
                xTilesLandcover, yTilesLandcover, colTopLandcover, rowLeftLandcover, _landcoverZoomLevel,
                xTilesElevation, yTilesElevation, colTopElevation, rowLeftElevation, _elevationZoomLevel,
                SaveTilesImagery, SaveTilesElevation, SaveTilesLandcover, _area.Top, _area.Left, _area.Bottom, _area.Right
            );

            Connection.MaxConnectionCount = TProjectSettings.MaxAsyncConnection;

           if (RequestElevationData)
               UpdateElevationData();
         
            if (RequestImageData)
                UpdateImageryData();
         
            if (_requestLandcoverData)
                UpdateLandCoverData();
          
            if (_requestLandcoverImageryData)
                UpdateLandcoverImageryData();
          
            _lastError = 0;

            Connection.DownloadData();
        }


        public bool IsHeightmapAvailable(TMapManager.ElevationMapServer _mapElevationSource, string cachePathElevation)
        {
            TDebug.TraceMessage();

            Connection = new TCommunications
            (
                AfterDataDownloaded,
                xTilesImagery, yTilesImagery, colTopImagery, rowLeftImagery, _imageryZoomLevel,
                xTilesLandcover, yTilesLandcover, colTopLandcover, rowLeftLandcover, _landcoverZoomLevel,
                xTilesElevation, yTilesElevation, colTopElevation, rowLeftElevation, _elevationZoomLevel,
                SaveTilesImagery, SaveTilesElevation, SaveTilesLandcover, _area.Top, _area.Left, _area.Bottom, _area.Right
            );

            bool? Result = Connection.CheckIsHeightmapDataAvailable(_mapElevationSource, cachePathElevation);
            if (Result.HasValue)
            {
                return Result.Value;
            }
            else
            {
                return false;
            }
        }

        public void UpdateElevationData()
        {
            TDebug.TraceMessage();

            if (Connection == null)
                throw new Exception("No Connection!");

            Connection.GetElevationData(_mapElevationSource, TAddresses.cachePathElevation);
            _lastError = 0;
        }

        public void UpdateImageryData()
        {
            TDebug.TraceMessage();

            if (Connection == null)
                throw new Exception("No Connection!");

            Connection.UpdateImageryDataAsync(TAddresses.cachePathImagery,_mapImagerySource);
            _lastError = 0;
        }

        public void UpdateLandcoverImageryData()
        {
            TDebug.TraceMessage();

            if (Connection == null)
                throw new Exception("No Connection!");

            Connection.UpdateLandCoverImageryDataAsync(TAddresses.cachePathLandcover, _mapLandCoverImagerySource);
            _lastError = 0;
        }

        public void UpdateLandCoverData()
        {
            TDebug.TraceMessage();

            if (Connection == null)
                throw new Exception("No Connection!");

            Connection.UpdateLandcoverData(TAddresses.cachePathLandcover,_mapLandcoverSource, _area);
            _lastError = 0;
        }

        public void Dispose()
        {
            disposed = true;
            Connection?.CancelActiveConnection();
            Connection = null;
        }

        public void AfterDataDownloaded(float[,] heightsData, Bitmap SatelliteImage, Bitmap LandCoverImage, XmlDocument landcoverData
            , int majorVersion, int minorVersion, int CounterNum, string Message, string webpage, Exception criticalException, Exception nonCriticalException)
        {
            
            TDebug.TraceMessage();
          
            if (criticalException != null )
            {
                criticalException.Data.Add("TW", "Downloading data error!");
                TDebug.LogErrorToUnityUI(criticalException);
            }
            else
            {
                if (nonCriticalException != null)
                    TDebug.LogWarningToUnityUI(nonCriticalException.Message);

                if (majorVersion > TVersionController.MajorVersion)
                    TTerraWorld.NewVersionWebAddress = webpage;
                else if ((minorVersion > TVersionController.MinorVersion) && (majorVersion == TVersionController.MajorVersion))
                    TTerraWorld.NewVersionWebAddress = webpage;
                else
                    TTerraWorld.NewVersionWebAddress = "";

                if (CounterNum > TProjectSettings.LastTeamMessageNum)
                {
                    TProjectSettings.LastTeamMessageNum = CounterNum;
                    EditorUtility.DisplayDialog("Terra World", Message, "OK");
                }

                if (disposed) return;

                if (RequestImageData)
                {
                    this.Image.Copy(SatelliteImage);
                    AfterUpdateCompletedImagery();
                }

                if (_requestLandcoverData)
                {
                    this.LandcoverXML = landcoverData.Clone() as XmlDocument;
                    AfterUpdateCompletedLandcover();
                }

                if (_requestLandcoverImageryData)
                {
                    this.LandCoverImage.Copy(LandCoverImage);
                    AfterUpdateCompletedLandCoverImagery();
                }


                if (RequestElevationData)
                {
                    this.SetHeightMap(heightsData);
                    AfterUpdateCompletedElevation();
                }

                //  this._heightMapAvailableInZoomLevel = _heightMapAvailableInZoomLevel;
                //
                //  if (_heightMapAvailableInZoomLevel)
                //      this.SetHeightMap(heightsData);
                //
                //  AnalyzeElevationData(this);

                WhenAllDone();

            }

        }

        private int AvailableHeighmapZoomLevel(int startZoomLevel)
        {
            int result = startZoomLevel;
            if (result > 20) result = 20;
            if (result < 5) result = 5;

            while (result > 4)
            {
                TMap tempMap = new TMap(_area.Top, _area.Left, _area.Bottom, _area.Right, null, _imageryZoomLevel, result, _requestLandcoverData, _landcoverZoomLevel);
                bool HeightmapAvailable = tempMap.IsHeightmapAvailable(_mapElevationSource, TAddresses.cachePathElevation);
                if (HeightmapAvailable) return result;
                TDebug.LogInfoToUnityUI("Heighmap data is not available for zoomlevel:" + result);
                result--;
            }

            result = 0;

            return result;
        }

      //  private void AnalyzeElevationData(TMap CurrentMap)
      //  {
      //      TDebug.TraceMessage();
      //
      //      if (CurrentMap.RequestElevationData && !CurrentMap._heightMapAvailableInZoomLevel)
      //      {
      //          TDebug.LogWarningToUnityUI("No Elevation Data On Level : " + CurrentMap._elevationZoomLevel + ", Trying next zoom level ... ");
      //          TMap tempMap = new TMap (CurrentMap._area.Top, CurrentMap._area.Left, CurrentMap._area.Bottom, CurrentMap._area.Right, _refTerrain, CurrentMap._imageryZoomLevel, CurrentMap._elevationZoomLevel - 1, _requestLandcoverData, CurrentMap._landcoverZoomLevel);
      //          tempMap._ref = CurrentMap._ref;
      //          tempMap.SaveTilesElevation = SaveTilesElevation;
      //          tempMap._mapElevationSource = _mapElevationSource;
      //          tempMap.RequestElevationData = true;
      //          tempMap.RequestImageData = false;
      //          //tempMap.RequestLandcoverData = RequestLandcoverDataMode.None;
      //          tempMap._Landcoverimage = _Landcoverimage;
      //          tempMap._image = _image;
      //          tempMap.UpdateMap(AnalyzeElevationData);
      //      }
      //      else
      //      {
      //          if (RequestElevationData) AfterUpdateCompletedElevation();
      //          WhenAllDone();
      //      }
      //  }

        private void AnalyzeImageryData(TMap CurrentMap)
        {
            TDebug.TraceMessage();

            if (CurrentMap.RequestImageData && !CurrentMap._heightMapAvailableInZoomLevel)
            {
                TDebug.LogWarningToUnityUI("No Imagery Data On Level : " + CurrentMap._imageryZoomLevel + ", Trying next zoom level ... ");
                TMap tempMap = new TMap(CurrentMap._area.Top, CurrentMap._area.Left, CurrentMap._area.Bottom, CurrentMap._area.Right, _refTerrain, CurrentMap._imageryZoomLevel - 1, CurrentMap._elevationZoomLevel, _requestLandcoverData, CurrentMap._landcoverZoomLevel);
                tempMap._ref = CurrentMap._ref;
                tempMap.SaveTilesImagery = SaveTilesImagery;
                tempMap._mapImagerySource = _mapImagerySource;
                tempMap.RequestElevationData = false;
                tempMap.RequestImageData = true;
                //tempMap.RequestLandcoverData = RequestLandcoverDataMode.None;
                tempMap._heightmap = _heightmap;
                tempMap.UpdateMap(AnalyzeImageryData);
            }
            else
            {
                if (RequestImageData) AfterUpdateCompletedImagery();
                WhenAllDone();
            }
        }

        public void AfterUpdateCompletedElevation()
        {
            TDebug.TraceMessage();

            if (_heightmap == null || _heightmap.heightsData == null) throw new Exception("Downloading elevation data error....!");

            _heightmap.heightsData = THeightmapProcessors.CropHeightmap(_mosaicAreaElevation, _area, _heightmap.heightsData);
            _heightmap.heightsData = THeightmapProcessors.ResampleHeightmap(_heightmap.heightsData, THeightmapProcessors.ResampleMode.DOWN);
            _heightmap.heightsData = THeightmapProcessors.RotateHeightmap(_heightmap.heightsData);

            if (_ref._elevationZoomLevel != this._elevationZoomLevel)
                _ref._heightmap.heightsData = _heightmap.heightsData.Clone() as float[,];
        }

        public void AfterUpdateCompletedImagery()
        {
            TDebug.TraceMessage();

            if (_image == null || _image.Image == null) throw new Exception("Downloading Imagery data error....!");
            _image.Image = TImageProcessors.CropImage(_mosaicAreaImagery, _area, _image.Image);
        }

        public void AfterUpdateCompletedLandCoverImagery()
        {
            TDebug.TraceMessage();

            if (_Landcoverimage == null || _Landcoverimage.Image == null) throw new Exception("Downloading Landcover data error....!");
            _Landcoverimage.Image = TImageProcessors.CropImage(_mosaicAreaLandcover, _area, _Landcoverimage.Image);
        }

        public void AfterUpdateCompletedLandcover()
        {
            TDebug.TraceMessage();

            if (LandcoverXML == null )
                throw new Exception("Downloading Landcover data error....!");
        }

        public void WhenAllDone()
        {
            TDebug.TraceMessage();

            if (_ref._lastActions != null && !disposed )
                _ref._lastActions.Invoke(this);
        }

        public void GetBBoxMosaicElevation(int zoomLevel)
        {
            // Set Row/Col for TopLeft
            int[] rowColTL = TConvertors.WorldToTilePos(_area.Left, _area.Top, zoomLevel);
            rowLeftElevation = rowColTL[0];
            colTopElevation = rowColTL[1];
            double[] tileLatLonTL = TConvertors.TileToWorldPos(rowLeftElevation, colTopElevation, zoomLevel);
            double left = tileLatLonTL[0];
            double top = tileLatLonTL[1];

            // Set Row/Col for BottomRight
            int[] rowColBR = TConvertors.WorldToTilePos(_area.Right, _area.Bottom, zoomLevel);
            rowRightElevation = rowColBR[0];
            colBottomElevation = rowColBR[1];
            double[] tileLatLonBR = TConvertors.TileToWorldPos(rowRightElevation + 1, colBottomElevation + 1, zoomLevel);
            double right = tileLatLonBR[0];
            double bottom = tileLatLonBR[1];

            if (_mosaicAreaElevation == null)
                _mosaicAreaElevation = new TArea(top, left, bottom, right);
            else
                _mosaicAreaElevation.UpdateTLBR(top, left, bottom, right);

            xTilesElevation = Math.Abs(rowRightElevation - rowLeftElevation) + 1;
            yTilesElevation = Math.Abs(colTopElevation - colBottomElevation) + 1;
        }

        public void GetBBoxMosaicImagery(int zoomLevel)
        {
            // Set Row/Col for TopLeft
            int[] rowColTL = TConvertors.WorldToTilePos(_area.Left, _area.Top, zoomLevel);
            rowLeftImagery = rowColTL[0];
            colTopImagery = rowColTL[1];
            double[] tileLatLonTL = TConvertors.TileToWorldPos(rowLeftImagery, colTopImagery, zoomLevel);
            double left = tileLatLonTL[0];
            double top = tileLatLonTL[1];

            // Set Row/Col for BottomRight
            int[] rowColBR = TConvertors.WorldToTilePos(_area.Right, _area.Bottom, zoomLevel);
            rowRightImagery = rowColBR[0];
            colBottomImagery = rowColBR[1];
            double[] tileLatLonBR = TConvertors.TileToWorldPos(rowRightImagery + 1, colBottomImagery + 1, zoomLevel);
            double right = tileLatLonBR[0];
            double bottom = tileLatLonBR[1];

            if (_mosaicAreaImagery == null)
                _mosaicAreaImagery = new TArea(top, left, bottom, right);
            else
                _mosaicAreaImagery.UpdateTLBR(top, left, bottom, right);

            xTilesImagery = Math.Abs(rowRightImagery - rowLeftImagery) + 1;
            yTilesImagery = Math.Abs(colTopImagery - colBottomImagery) + 1;
        }

        public void GetBBoxMosaicLandcover(int zoomLevel)
        {
            // Set Row/Col for TopLeft
            int[] rowColTL = TConvertors.WorldToTilePos(_area.Left, _area.Top, zoomLevel);
            rowLeftLandcover = rowColTL[0];
            colTopLandcover = rowColTL[1];
            double[] tileLatLonTL = TConvertors.TileToWorldPos(rowLeftLandcover, colTopLandcover, zoomLevel);
            double left = tileLatLonTL[0];
            double top = tileLatLonTL[1];

            // Set Row/Col for BottomRight
            int[] rowColBR = TConvertors.WorldToTilePos(_area.Right, _area.Bottom, zoomLevel);
            rowRightLandcover = rowColBR[0];
            colBottomLandcover = rowColBR[1];
            double[] tileLatLonBR = TConvertors.TileToWorldPos(rowRightLandcover + 1, colBottomLandcover + 1, zoomLevel);
            double right = tileLatLonBR[0];
            double bottom = tileLatLonBR[1];

            if (_mosaicAreaLandcover == null)
                _mosaicAreaLandcover = new TArea(top, left, bottom, right);
            else
                _mosaicAreaLandcover.UpdateTLBR(top, left, bottom, right);

            xTilesLandcover = Math.Abs(rowRightLandcover - rowLeftLandcover) + 1;
            yTilesLandcover = Math.Abs(colTopLandcover - colBottomLandcover) + 1;
        }

        public static int GetZoomLevel(int resolution , TArea area)
        {
            double temp = Math.Log(156543.03 * Math.Cos(area.Center().latitude * Math.PI / 180) * resolution / (area.AreaSizeLat * 1000));
            //double temp = Math.Log(156543.03 * Math.Cos(area._center.latitude * Math.PI / 180) * resolution / (area._areaSizeLat * 1000));
            //double mpp = (area._areaSizeLat * 1000) / resolution;
            //double zoomlevel2 = Math.Log(156543.03 *  mpp * Math.Cos(area._center.latitude * Math.PI / 180))/Math.Log(2) ; 

            double zoomlevel =temp / Math.Log(2);

            return (int)Math.Ceiling(zoomlevel);
        }
 
        public double GetHeight(TGlobalPoint node)
        {
            double normalizedX;
            double normalizedZ;
            GetNormalizedIndex(node, out normalizedX, out normalizedZ);
            if (normalizedX < 0 || normalizedX > 1) return 0;
            if (normalizedZ < 0 || normalizedZ > 1) return 0;
            double result = _heightmap.GetInterpolatedHeight(normalizedX, normalizedZ);
            return result;
        }

        public TGlobalPoint GetNearestPoint (TGlobalPoint point)
        {
            int pixelX, pixelY;
            pixelX = pixelY = 0;
            GetNearestIndex(point, out pixelX, out pixelY);
            double normalizedPixelX = (double)pixelX / Heightmap.heightsData.GetLength(0);
            double normalizedPixelY = (double)pixelY / Heightmap.heightsData.GetLength(1);
            return GetGeoPoint(normalizedPixelX, normalizedPixelY);
        }

        public TGlobalPoint GetGeoPoint(double IndexX, double IndexY)
        {
            TGlobalPoint result = new TGlobalPoint();
            double normalizedPixelX = (double)IndexX / Heightmap.heightsData.GetLength(0);
            double normalizedPixelY = (double)IndexY / Heightmap.heightsData.GetLength(1);
            result.longitude = (normalizedPixelX * (_area.Right - _area.Left)) + _area.Left;
            result.latitude = (normalizedPixelY * (_area.Top - _area.Bottom)) + _area.Bottom;
            return result;
        }

        public void GetNearestIndex (TGlobalPoint point , out int IndexX, out int IndexY)
        {
            Vector2 latlonDeltaNormalized = GetLatLongNormalizedPositionN(point);
            double normalizedX = latlonDeltaNormalized.Y;
            double normalizedZ = latlonDeltaNormalized.X;
            //GetNormalizedIndex(point, out normalizedX, out normalizedZ);
            IndexX = (int)(normalizedX * (Heightmap.heightsData.GetLength(0)-1));
            double tempNormalX1 = (IndexX * 1.0f/ (Heightmap.heightsData.GetLength(0) - 1));
            double tempNormalX2 = ((IndexX + 1) * 1.0f / (Heightmap.heightsData.GetLength(0) - 1));

            if ((normalizedX - tempNormalX1) > (tempNormalX2 - normalizedX))
                IndexX = IndexX + 1;

            IndexY = (int)(normalizedZ * (Heightmap.heightsData.GetLength(1) - 1));
            double tempNormalY1 = (IndexY * 1.0f/ (Heightmap.heightsData.GetLength(1) - 1));
            double tempNormalY2 = ((IndexY + 1) * 1.0f / (Heightmap.heightsData.GetLength(1) - 1));

            if ((normalizedZ - tempNormalY1) > (tempNormalY2 - normalizedZ))
               IndexY = IndexY + 1;
        }


        public T2DPoint GetPointNormalized(TGlobalPoint point)
        {
            Vector2 latlonDeltaNormalized = GetLatLongNormalizedPositionN(point);
            T2DPoint pointNormalized = new T2DPoint(latlonDeltaNormalized.Y, latlonDeltaNormalized.X);

            return pointNormalized;
        }

        public void GetNormalizedIndex(TGlobalPoint point, out double IndexX, out double IndexY)
        {
            IndexX = TUtils.InverseLerp(_area.Left, _area.Right, point.longitude);
            IndexY = TUtils.InverseLerp(_area.Bottom, _area.Top, point.latitude);
        }

        public void GetPixelBounds (TArea Src, out int minLonIndex, out int maxLonIndex, out int minLatIndex, out int maxLatIndex )
        {
            TGlobalPoint topLeft = new TGlobalPoint();
            topLeft.latitude = Src.Top;
            topLeft.longitude = Src.Left;
            TGlobalPoint bottomRight = new TGlobalPoint();
            bottomRight.latitude = Src.Bottom;
            bottomRight.longitude = Src.Right;
            GetNearestIndex(topLeft, out minLonIndex, out maxLatIndex);
            GetNearestIndex(bottomRight, out maxLonIndex, out minLatIndex);
        }

        public Vector2 GetWorldPosition(TGlobalPoint geoPoint)
        {
            double worldSizeX = _area.AreaSizeLon * 1000;
            double worldSizeY = _area.AreaSizeLat * 1000;
            Vector2 latlonDeltaNormalized = GetLatLongNormalizedPositionN( geoPoint);
            Vector2 worldPositionXZ = AreaBounds.GetWorldPositionFromTile(latlonDeltaNormalized.X, latlonDeltaNormalized.Y, worldSizeY, worldSizeX);
            //Vector3d worldPositionTemp = new Vector3d(worldPositionXZ.x + worldSizeY / 2, 0, worldPositionXZ.y - worldSizeX / 2);
            Vector3d worldPositionTemp = new Vector3d(worldPositionXZ.X , 0, worldPositionXZ.Y );

            return new Vector2((float)worldPositionTemp.x, (float)worldPositionTemp.z);
        }

        public Vector2 GetLatLongNormalizedPositionN(TGlobalPoint geoPoint)
        {
            double yMaxTop = AreaBounds.LatitudeToMercator(_area.Top);
            double xMinLeft = AreaBounds.LongitudeToMercator(_area.Left);
            double yMinBottom = AreaBounds.LatitudeToMercator(_area.Bottom);
            double xMaxRight = AreaBounds.LongitudeToMercator(_area.Right);
            double latSize = Math.Abs(yMaxTop - yMinBottom);
            double lonSize = Math.Abs(xMinLeft - xMaxRight);
            double LAT = AreaBounds.LatitudeToMercator(geoPoint.latitude);
            double LON = AreaBounds.LongitudeToMercator(geoPoint.longitude);
            double[] latlonDeltaNormalized = AreaBounds.GetNormalizedDeltaN(LAT, LON, yMaxTop, xMinLeft, latSize, lonSize);

            return new Vector2((float)latlonDeltaNormalized[0], (float)latlonDeltaNormalized[1]);
        }
    }
#endif
}
#endif
#endif

