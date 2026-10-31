using System.IO;
namespace Veilbreak.Gui.Tests {
/// <summary>Files inside this package wherever it is installed (embedded, local, Git or registry).</summary>
static class PackageFiles {
 public static string Path(string relative)=>System.IO.Path.Combine(UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(VbIcon).Assembly).resolvedPath,relative);
 public static string Read(string relative)=>File.ReadAllText(Path(relative));
}
}
