using System.Reflection;

namespace MyChatApp
{
    public static class VersionInfo
    {
        // Version Constants - Update these manually
        public const int MajorVersion = 1;
        public const int MinorVersion = 0;
        public const int PatchVersion = 1;
        public const int BuildNumber = 0;
        
        // String constants for assembly attributes (compile-time constants)
        public const string AssemblyVersionString = "1.0.1.0";
        public const string FileVersionString = "1.0.1.0";
        public const string InformationalVersionString = "1.0.1";
        
        // Computed Version Strings (runtime)
        public static string Version => $"{MajorVersion}.{MinorVersion}.{PatchVersion}";
        public static string AssemblyVersion => $"{MajorVersion}.{MinorVersion}.{PatchVersion}.{BuildNumber}";
        public static string FileVersion => AssemblyVersion;
        public static string ProductVersion => Version;
        public static string InformationalVersion => GetInformationalVersion();
        
        // Application Metadata
        public const string ProductName = "MyChatApp";
        public const string Company = "Your Company Name";
        public const string Copyright = "Copyright © 2025";
        public const string Description = "A powerful Windows Forms application for multi-provider LLM chat with MCP server integration";
        
        private static string GetInformationalVersion()
        {
            var version = Version;
            var buildDate = GetBuildDate();
            return $"{version} (Built: {buildDate:yyyy-MM-dd HH:mm})";
        }
        
        private static DateTime GetBuildDate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var location = assembly.Location;
            if (File.Exists(location))
            {
                return File.GetLastWriteTime(location);
            }
            return DateTime.Now;
        }
        
        public static string GetFullVersionInfo()
        {
            return $"{ProductName} v{Version} (Build: {FileVersion})";
        }
        
        public static string GetDetailedVersionInfo()
        {
            return $"{ProductName} v{Version}\n" +
                   $"Build: {FileVersion}\n" +
                   $"Built: {GetBuildDate():yyyy-MM-dd HH:mm}\n" +
                   $"{Copyright}";
        }
    }
}

