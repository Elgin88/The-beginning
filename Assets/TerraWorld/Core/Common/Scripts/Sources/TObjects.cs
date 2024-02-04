#if UNITY_EDITOR
using UnityEditor;
using System;
using System.IO;

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    // public enum AssetTypes
    // {
    //     Material,
    //     Image
    // }

    //  public abstract class TObjects 
    //  {
    //      private static int Counter;
    //      private UnityEngine.Object _assetObject = null;
    //
    //      public UnityEngine.Object AssetObject
    //      {
    //          get { return OnGetAsset(); }
    //          set { OnSetAsset(value); }
    //      }
    //
    //      private string _ObjPath;
    //      public string ObjectPath
    //      {
    //          get { return OnGetAssetPath(); }
    //          set { OnSetAssetAtPath(value); }
    //      }
    //
    //      private int _ID;
    //      public int ID { get => _ID; }
    //
    //      public TObjects ()
    //      {
    //          GenerateID();
    //      }
    //
    //
    //      private void GenerateID()
    //      {
    //          Random rand = new Random((int)DateTime.Now.Ticks);
    //          _ID = (rand.Next() + Counter++);
    //      }
    //
    //      public abstract string GenerateAssetPath();
    //
    //      public abstract void SaveObject(string path);
    //
    //      private string OnGetAssetPath()
    //      {
    //          return _ObjPath;
    //      }
    //
    //
    //      private void OnUpdate()
    //      {
    //          if (string.IsNullOrEmpty(_ObjPath))
    //          {
    //              if (_assetObject == null)
    //                  _ObjPath = string.Empty;
    //              else
    //              {
    //                  string assetpath = AssetDatabase.GetAssetPath(_assetObject);
    //                  if (string.IsNullOrEmpty(assetpath))
    //                  {
    //                      _ObjPath = GenerateAssetPath();
    //                      SaveObject(_ObjPath);
    //                  }
    //                  else
    //                  {
    //                      _ObjPath = assetpath;
    //                  }
    //              }
    //          }
    //          else
    //          {
    //              if (!File.Exists(_ObjPath))
    //              {
    //                  if (_assetObject == null)
    //                      _ObjPath = string.Empty;
    //                  else
    //                  {
    //                      SaveObject(_ObjPath);
    //                  }
    //              }
    //              else
    //              {
    //                  if (_assetObject == null)
    //                  {
    //                      _assetObject = AssetDatabase.LoadMainAssetAtPath(_ObjPath);
    //                  }
    //                  else
    //                  {
    //                      string assetpath = AssetDatabase.GetAssetPath(_assetObject);
    //                      if (assetpath != _ObjPath)
    //                      {
    //                          SaveObject(_ObjPath);
    //                      }
    //                  }
    //              }
    //
    //          }
    //      }
    //      private void OnSetAssetAtPath(string path)
    //      {
    //          _ObjPath = path;
    //          OnUpdate();
    //      }
    //
    //
    //      private void OnSetAsset(UnityEngine.Object asset)
    //      {
    //          _assetObject = asset;
    //          OnUpdate();
    //      }
    //
    //      private UnityEngine.Object OnGetAsset()
    //      {
    //          if (_assetObject == null) throw new Exception("Internal 2876");
    //          return _assetObject;
    //      }
    //
    //
    //  }
#endif
}
#endif

