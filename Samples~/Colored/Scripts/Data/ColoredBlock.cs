using UnityEngine;

using Random = UnityEngine.Random;

namespace CodeBlaze.Vloxy.Samples.Colored.Data {

    public static class ColoredBlocks {

        public static int FromColor32(Color32 color) {
            int x = 0;
            
            x += color.r;
            x <<= 8;
            x += color.g;
            x <<= 8;
            x += color.b;
            x <<= 8;
            x += color.a;

            return x;
        }
        
        public static int RandomColor() {
            int x = 0;
            
            x += Random.Range(0, 256);
            x <<= 8;
            x += Random.Range(0, 256);
            x <<= 8;
            x += Random.Range(0, 256);
            x <<= 8;
            x += 255;

            return x;
        }
        
        public static int Air() => 0;

    }

}