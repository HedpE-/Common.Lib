/*
 * Created by SharpDevelop.
 * User: GONCARJ3
 * Date: 06/01/2017
 * Time: 20:43
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.IO;

namespace Common.Lib.Extensions
{
    public static class DirectoryInfoExtensions
    {
        public static void CopyTo(this DirectoryInfo source, string targetString)
        {
            DirectoryInfo target = new DirectoryInfo(targetString);
            if (!target.Exists)
                target.Create();

            foreach (var file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);

            foreach (var subdir in source.GetDirectories())
                subdir.CopyTo(target.CreateSubdirectory(subdir.Name).FullName);
        }
    }
}