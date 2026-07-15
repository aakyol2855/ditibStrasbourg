using System;
using System.IO;
using PdfSharp.Fonts;

namespace DitibStasbourg.Services.Implementations
{
    public class LinuxFontResolver : IFontResolver
    {
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Map Arial and other fonts to LiberationSans
            string suffix = "Regular";
            if (isBold && isItalic) suffix = "BoldItalic";
            else if (isBold) suffix = "Bold";
            else if (isItalic) suffix = "Italic";

            return new FontResolverInfo($"LiberationSans#{suffix}");
        }

        public byte[]? GetFont(string faceName)
        {
            string path = faceName switch
            {
                "LiberationSans#Regular" => "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf",
                "LiberationSans#Bold" => "/usr/share/fonts/truetype/liberation/LiberationSans-Bold.ttf",
                "LiberationSans#Italic" => "/usr/share/fonts/truetype/liberation/LiberationSans-Italic.ttf",
                "LiberationSans#BoldItalic" => "/usr/share/fonts/truetype/liberation/LiberationSans-BoldItalic.ttf",
                _ => null
            };

            if (path != null && File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }

            // Fallback to regular
            var defaultPath = "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf";
            if (File.Exists(defaultPath))
            {
                return File.ReadAllBytes(defaultPath);
            }

            return null;
        }
    }
}
