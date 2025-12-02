using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using SharpCompress;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace ShinRyuModManager
{
    internal static class ArchiveUtils
    {
        public static bool ExtractArchive(string archivePath, string outputDirectory)
        {
            if (!File.Exists(archivePath))
                return false;
            try
            {
                using (Stream stream = File.OpenRead(archivePath))
                {
                    var is7Z = SevenZipArchive.IsSevenZipFile(stream);

                    stream.Seek(0, SeekOrigin.Begin);

                    if (is7Z)
                    {
                        return Extract7ZFile(stream, outputDirectory);
                    }
                    
                    using (var reader = ReaderFactory.Open(stream))
                    {

                        while (reader.MoveToNextEntry())
                        {
                            if (!reader.Entry.IsDirectory)
                            {
                                Console.WriteLine(reader.Entry.Key);
                                reader.WriteEntryToDirectory(outputDirectory, new ExtractionOptions()
                                {
                                    ExtractFullPath = true,
                                    Overwrite = true
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to install mod archive\n" + ex.ToString());
            }


            return true;
        }

        private static bool Extract7ZFile(Stream stream, string outputDirectory)
        {
            using (var archive = SevenZipArchive.Open(stream))
            using (var reader = archive.ExtractAllEntries())
            {
                reader.WriteAllToDirectory(outputDirectory, new ExtractionOptions()
                {
                    ExtractFullPath = true,
                    Overwrite = true
                });
            }

            return true;
        }
    }
}
