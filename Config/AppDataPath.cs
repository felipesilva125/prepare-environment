using System;
using System.IO;

namespace prepare_environment
{
    internal static class AppDataPath
    {        
        public static string GetRootPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "NCSystems", "Dcs");
    }
}
