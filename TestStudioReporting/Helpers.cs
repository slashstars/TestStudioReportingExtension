using ArtOfTest.WebAii.Core;
using ArtOfTest.WebAii.ObjectModel;
using System;
using System.Data.SqlClient;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace TestStudioReporting
{
    public static class Helpers
    {
        public static void AssignParameterValue(this SqlCommand cmd, string parameter, object value)
        {
            object obj = value ?? DBNull.Value;
            if (!parameter.StartsWith("@")) parameter = "@" + parameter;
            if (obj is string)
            {
                string str = (string)obj;
                if (string.IsNullOrEmpty(str))
                {
                    cmd.Parameters[parameter].Value = DBNull.Value;

                }
                else
                {
                    cmd.Parameters[parameter].Value = obj;
                }
                return;
            }
            else
            {
                cmd.Parameters[parameter].Value = obj;
            }
        }

        public static byte[] TakeScreenshot(this Manager manager)
        {
            if (manager != null)
            {
                var screenshot = manager.ActiveBrowser.Capture();
                using (MemoryStream stream = new MemoryStream())
                {
                    screenshot.Save(stream, ImageFormat.Jpeg);
                    return stream.ToArray();
                }
            }
            else
            {
                return new byte[] { };
            }

        }

        public static string CaptureDOM(this Manager manager)
        {
            StringBuilder builder = new StringBuilder(1024);
            BuildDOMRecursive(manager.ActiveBrowser.DomTree.Root, builder);
            return builder.ToString();
        }

        private static void BuildDOMRecursive(Element root, StringBuilder builder, int depth = 0)
        {
            builder.Append(new string('\t', depth)).Append(root.Content).Append('\n');
            foreach(var child in root.Children)
            {
                BuildDOMRecursive(child, builder, depth + 1);
            }
        }
    }
}
