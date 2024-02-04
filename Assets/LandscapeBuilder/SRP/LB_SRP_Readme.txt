Scriptable Render Pipeline assets in LB require Unity 2019.4.32f1 or newer.
IMPORTANT: Do not import these packages without URP or HDRP installed, else you will get errors. 

1. Import the entire asset (e.g. Landscape Builder) into a project setup for LRWP or HDRP.
2. If using HDRP, from package manager, import High Definition Render Pipeline 7.7.1 (U2019.4.32 LTS+), 10.3.2 (U2020.3 LTS+) or 12.1.4 (U2021.2.12+)
3. If using URP, from package manager, import Universal Render Pipeline 7.7.1 (U2019.4.32 LTS+)
4. From the Unity Editor double-click** on the LB_URP_[version] or LB_HDRP_[version] package within this folder

NOTES:

a) Currently the LBImageFX, LB Lighting, and weather features are not supported in HDRP or URP.
b) From the LB Editor, on the Landscape tab, change the Material Type in Terrain Settings to URP or HDRP and Apply Terrain Settings.
c) The demo scenes are designed for the built-in RP. Vegetation and water will not render or will look incorrect.

** If double-click does not work, from the Unity Editor menu, Assets/Import Package/Custom Package... navigate to the folder within your project where the package is located to import the package.