#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using System.IO;
using System;
using UnityEditor;

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    public class TMaterial 
    {
        private int _ID;
        private string _objectPath;
        private Material Material;

        public TMaterial ()
        {
            System.Random rand = new System.Random((int)DateTime.Now.Ticks);
            _ID = rand.Next();
        }

        public void SetMaterial(Material material)
        {
            Material = material;
        }

        public string ObjectPath { get => _objectPath; set => _objectPath = value; }

        private string GenerateAssetPath()
        {
            return TTerraWorld.WorkDirectoryLocalPath + "Asset_" + _ID + ".mat";  
        }

        public Material GetMaterial()
        {
            if (Material == null)
                return AssetDatabase.LoadAssetAtPath(_objectPath, typeof(Material)) as Material;
            else
                return Material;
        }

    }
#endif
}
#endif
#endif

