// Landscape Builder. Copyright (c) 2016-2022 SCSM Pty Ltd. All rights reserved.
using UnityEngine;
using System.Collections.Generic;

namespace LandscapeBuilder
{
    [System.Serializable]
    public class LBLandscapeExtItem
    {
        #region Public Variables
        public Texture2D texture;
        //public Texture2D normalMap;
        // Last known texture name. Used to help detect missing textures.
        public string textureName;
        //public string normalMapName;
        #endregion

        #region Constructors
        // Basic class constructor
        public LBLandscapeExtItem()
        {
            SetClassDefaults();
        }

        // Copy constructor
        public LBLandscapeExtItem (LBLandscapeExtItem lbLandscapeExtItem)
        {
            if (lbLandscapeExtItem == null) { SetClassDefaults(); }
            else
            {
                this.texture = lbLandscapeExtItem.texture;
                this.textureName = lbLandscapeExtItem.textureName;
            }
        }

        #endregion

        #region Non-Static Private Methods

        private void SetClassDefaults()
        {
            texture = null;
            textureName = string.Empty;
        }

        #endregion
    }
}
