using Unity.Profiling;

namespace CodeBlaze.Vloxy.Profiling {

    public static class VloxyProfiler {

        public static ProfilerMarker MeshBuildJobMarker = new("MeshBuildJob");
        public static ProfilerMarker ViewRegionUpdateMarker = new("ViewRegionUpdate");

        public static ProfilerRecorder MeshBuildJobRecorder;
        
        public static void Initialize() {
            MeshBuildJobRecorder = ProfilerRecorder.StartNew(MeshBuildJobMarker);
        }

        public static void Dispose() {
            MeshBuildJobRecorder.Dispose();
        }

        public static string TimeMS(this ProfilerRecorder recorder) => $"{recorder.CurrentValueAsDouble * (1e-6f):F}ms";

    }

}