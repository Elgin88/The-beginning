#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using TerraUnity.Runtime;

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    public enum ImageFormats
    {
        PNG,
        JPG
    }

    public class TImage 
    {
        public ImageFormats _imageType = ImageFormats.JPG;
        public TextureImporterFormat imageFormat = TextureImporterFormat.Automatic;
        public bool isReadable = false;
        private Bitmap _cachedImage;
        private int _ID;

        public TImage() : base()
        {
            System.Random rand = new System.Random((int)DateTime.Now.Ticks);
            _ID = rand.Next() ;
        }

        public TImage(Bitmap Image) : base()
        {
            if (Image != null)
                _cachedImage = Image;
            else
                _cachedImage = new Bitmap(32, 32);
        }

        public string GenerateAssetPath()
        {
             return TTerraWorld.WorkDirectoryLocalPath + "Asset_" + _ID + ".jpg";
        }

        public void Copy(Bitmap Image) 
        {
            _cachedImage = Image.Clone() as Bitmap;
        }

        public Bitmap Image
        {
            get { return _cachedImage; }
            set { _cachedImage = value; }
        }

        private Bitmap Texture2DToBitmap (Texture2D texture)
        {
            Bitmap result = new Bitmap(texture.width, texture.height);

            if (_imageType == ImageFormats.PNG)
            {
                byte[] bytes = texture.EncodeToPNG();

                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    return result = new Bitmap(ms);
                }
            }
            else if (_imageType == ImageFormats.JPG)
            {
                byte[] bytes = texture.EncodeToJPG();

                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    return result = new Bitmap(ms);
                }
            }

            return null;
        }

        private Texture2D BitmapToTexture2D (Bitmap bmp)
        {
            Texture2D result = new Texture2D(bmp.Width, bmp.Height);

            using (MemoryStream stream = new MemoryStream())
            {
                if(_imageType == ImageFormats.PNG)
                    bmp.Save(stream, ImageFormat.Png);
                else if (_imageType == ImageFormats.JPG)
                    bmp.Save(stream, ImageFormat.Jpeg);

                byte[] bytes = stream.ToArray();
                result.LoadImage(bytes);
            }

            return result;
        }
        public void FillImage(System.Drawing.Color color, TMask mask)
        {
            int Width = _cachedImage.Width;
            int Length = _cachedImage.Height;

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    double normilizX = (i * 1.0 / Width);
                    double normilizY = (j * 1.0 / Length);

                    if (mask.CheckNormal(normilizX, normilizY))
                        _cachedImage.SetPixel(i, Length - j - 1, color);
                }
            }
        }

        public void SaveObject(string path)
        {
            
            string extension = Path.GetExtension(path);

            if (extension == ".png")
                _imageType = ImageFormats.PNG;
            else if (extension == ".jpg")
                _imageType = ImageFormats.JPG;
            else
                throw new Exception("Unknown Format!");

            if (_cachedImage != null)
            {
                Texture2D _tempImage = BitmapToTexture2D(_cachedImage);
                
                if (extension == ".png")
                    File.WriteAllBytes(path, _tempImage.EncodeToPNG());
                else if (extension == ".jpg")
                    File.WriteAllBytes(path, _tempImage.EncodeToJPG());

                AssetDatabase.Refresh();
                TextureImporter imageImport = AssetImporter.GetAtPath(TAddresses.GetProjectPath(path)) as TextureImporter;

                TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
                platformSettings.format = imageFormat;
                imageImport.SetPlatformTextureSettings(platformSettings);

                // set texture importer parameters
                imageImport.mipmapEnabled = true;
                imageImport.wrapMode = TextureWrapMode.Clamp;

                imageImport.maxTextureSize = Mathf.ClosestPowerOfTwo(_cachedImage.Width);
                imageImport.isReadable = isReadable;

                if (imageFormat == TextureImporterFormat.Alpha8)
                {
                    imageImport.sRGBTexture = false;
                    imageImport.alphaSource = TextureImporterAlphaSource.FromGrayScale;
                }

                AssetDatabase.ImportAsset(TAddresses.GetProjectPath(path), ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();
            }
        }

        public Texture2D GetTextureAndSaveOnDisk(string assetname)
        {
            return GetAsTexture2D(TextureImporterFormat.Alpha8, assetname);
        }

        public Texture2D GetTextureRGBAAndSaveOnDisk(string assetname)
        {
            return GetAsTexture2D(TextureImporterFormat.Automatic, assetname);
        }

        public Texture2D GetAsTexture2D(TextureImporterFormat textureImporterFormat = TextureImporterFormat.Automatic, string assetname = null)
        {
            imageFormat = textureImporterFormat;
            isReadable = true;
            if (string.IsNullOrEmpty(assetname)) assetname = "Image_" + _ID;
            string path = TTerraWorld.WorkDirectoryLocalPath + assetname + ".jpg";
            SaveObject(path);
            return AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
        }

    }
#endif
}
#endif
#endif

