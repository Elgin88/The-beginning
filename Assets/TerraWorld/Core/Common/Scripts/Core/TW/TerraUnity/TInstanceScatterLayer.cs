#if TERRAWORLD_PRO
#if UNITY_EDITOR

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
    public enum GridSize
    {
        _1 = 1,
        _2x2 = 2,
        _3x3 = 3,
        _4x4 = 4,
        _5x5 = 5,
        _6x6 = 6,
        _7x7 = 7,
        _8x8 = 8,
        _9x9 = 9,
        _10x10 = 10,
        _11x11 = 11,
        _12x12 = 12,
        _13x13 = 13,
        _14x14 = 14,
        _15x15 = 15,
        _16x16 = 16,
        _32x32 = 32,
        _64x64 = 64,
        _100x100 = 100,
        _128x128 = 128,
        _200x200 = 200,
        _256x256 = 256
    }

    public class TInstanceScatterLayer : TPointLayer
    {
        //TODO: Do we need to add [NonSerialized] attribute to the following fields in this class?

        // patchScale resolution supports
        //16:   supports 0.75 meters models distance
        //32:   supports 1    meters models distance
        //64:   supports 2    meters models distance
        //128:  supports 3    meters models distance
        //256:  supports 4    meters models distance
        //512:  supports 6    meters models distance
        //1024: supports 7    meters models distance
        //2048: supports 10   meters models distance
        //4096: supports 18   meters models distance
        // Default is 256
        public float averageDistance = 10;

        //public int gridResolution = 100;
        public float maxDistance = 1000;
        public float LODMultiplier = 1;
        public TPatch[,] patches;
        public string prefabName;
        //public List<string> LODNames;
        //public List<float> LODDistances;
        public TShadowCastingMode shadowCastingMode;
        public bool receiveShadows;
        public bool bypassLake;
        public bool underLake;
        public bool underLakeMask;
        public bool onLake;

        public float[,] maskData;
        //public TMask mask;

        public float frustumMultiplier = 1.1f;
        public bool checkBoundingBox = false;
        public bool occlusionCulling = false;
    }
#endif
}
#endif
#endif

