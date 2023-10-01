using UnityEngine;

namespace CodeBlaze.Vloxy.Demo.Utils {
    
    public class GameLogger {

        private static string _LogTag = $"<color=#{ColorUtility.ToHtmlStringRGB(GetColor("GAME"))}>[GAME]</color> ";
        
        /// <summary>
        /// Creates a tag with unique color for given type
        /// </summary>
        /// <typeparam name="T">type for which tag is to be created</typeparam>
        /// <returns>tag color formatted string</returns>
        public static string GetTag<T>() => GetTag(typeof(T).Name.Split('`')[0]);
        
        /// <summary>
        /// Creates a tag with unique color for given name
        /// </summary>
        /// <param name="name">tag value</param>
        /// <returns>tag color formatted string</returns>
        public static string GetTag(string name) {
            return $"{_LogTag}<color=#{ColorUtility.ToHtmlStringRGB(GetColor(name))}>{name}</color>";
        }
        
        /// <summary>
        /// Creates a tag with custom color for given type
        /// </summary>
        /// <param name="color">Hex code or name of the color (names found in Color class only)</param>
        /// <typeparam name="T">type for which tag is to be created</typeparam>
        /// <returns>tag color formatted string</returns>
        public static string GetTag<T>(string color) => GetTag(typeof(T).Name.Split('`')[0], color);
        
        /// <summary>
        /// Creates a tag with custom color for given name
        /// </summary>
        /// <param name="name">tag value</param>
        /// <param name="color">Hex code or name of the color (names found in Color class only)</param>
        /// <returns>tag color formatted string</returns>
        public static string GetTag(string name, string color) => $"<color={color}>{name}</color>";
        
        public static void SetLogLevel(LogType level) => Debug.unityLogger.filterLogType = level;

        public static void EnableLogging(bool enable) => Debug.unityLogger.logEnabled = enable;

        public static void SetLogHandler(ILogHandler handler) => Debug.unityLogger.logHandler = handler;

        public static void Info<T>(string message) => Debug.unityLogger.Log(LogType.Log, GetTag<T>(), message);
        public static void Warn<T>(string message) => Debug.unityLogger.Log(LogType.Warning, GetTag<T>(), message);
        public static void Error<T>(string message) => Debug.unityLogger.Log(LogType.Error, GetTag<T>(), message);
        
        public static void Info(string tag, string message) => Debug.unityLogger.Log(LogType.Log, GetTag(tag), message);
        public static void Warn(string tag, string message) => Debug.unityLogger.Log(LogType.Warning, GetTag(tag), message);
        public static void Error(string tag, string message) => Debug.unityLogger.Log(LogType.Error, GetTag(tag), message);

        /// <summary>
        /// Removes hue values between 0.6 to 0.7 as they are unredable
        /// </summary>
        /// <param name="hue">Original hue value</param>
        /// <returns>Normalized hue value</returns>
        private static float NormalizeHue(float hue) => Mathf.Lerp(0.7f, 1.6f, hue) % 1;

        private static Color GetColor(string name) {
            var hue = ((float) name.GetHashCode() % 10000 / 10000 + 1) / 2;
            return Color.HSVToRGB(NormalizeHue(hue), 1f, 1f);
        }
        
    }
    
}