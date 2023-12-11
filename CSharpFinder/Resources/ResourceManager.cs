using System.Threading;

namespace CSharpFinder.Resources
{
    internal static class ResourceManager
    {
        internal static string GetString(string key)
        {
            try
            {
                string prefix;

                if (Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "de")
                    prefix = "DE_";
                else
                    prefix = "EN_";

                return Properties.Resources.ResourceManager.GetString(prefix + key);
            }
            catch
            {
                return "ResourcesError";
            }
        }
    }
}