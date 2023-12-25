using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace CriPakTools
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Yakuza CPK Repack - Based on CriPakTools\n");
            Modify(args[0], args[1], args[2]);
        }

        public static byte[] FileToByteArray(string fileName)
        {
            byte[] fileData = null;

            using (FileStream fs = File.OpenRead(fileName))
            {
                using (BinaryReader binaryReader = new BinaryReader(fs))
                {
                    fileData = binaryReader.ReadBytes((int)fs.Length);
                }
            }
            return fileData;
        }


        public static void Modify(string inputCpk, string replaceDir, string outputCpk)
        {
            GC.Collect();
            Console.WriteLine("Yakuza CPK Repack - Based on CriPakTools\n");

            CPK cpk = new CPK(new Tools());
            cpk.ReadCPK(inputCpk);

            GC.Collect();

            BinaryReader oldFile = new BinaryReader(File.OpenRead(inputCpk));

            string[] files = Directory.GetFiles(replaceDir, "*.");
            HashSet<string> filesNames = new HashSet<string>();

            foreach (string str in files.Select(x => Path.GetFileNameWithoutExtension(x)))
                filesNames.Add(str);

            FileInfo fi = new FileInfo(inputCpk);

            Stopwatch time = new Stopwatch();
            time.Start();


            MemoryStream ms = new MemoryStream();
            BinaryWriter newCPK = new BinaryWriter(ms);

            List<FileEntry> entries = cpk.FileTable.OrderBy(x => x.FileOffset).ToList();

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].FileType != "CONTENT")
                {

                    if (entries[i].FileType == "FILE")
                    {
                        // I'm too lazy to figure out how to update the ContextOffset position so this works :)
                        if ((ulong)newCPK.BaseStream.Position < cpk.ContentOffset)
                        {
                            ulong padLength = cpk.ContentOffset - (ulong)newCPK.BaseStream.Position;
                            for (ulong z = 0; z < padLength; z++)
                            {
                                newCPK.Write((byte)0);
                            }
                        }
                    }


                    if (!filesNames.Contains(entries[i].FileName.ToString()))
                    {
                        oldFile.BaseStream.Seek((long)entries[i].FileOffset, SeekOrigin.Begin);

                        entries[i].FileOffset = (ulong)newCPK.BaseStream.Position;
                        cpk.UpdateFileEntry(entries[i]);

                        byte[] chunk = oldFile.ReadBytes(Int32.Parse(entries[i].FileSize.ToString()));
                        newCPK.Write(chunk);

                        GC.Collect();
                    }
                    else
                    {
                        byte[] newbie = FileToByteArray((Path.Combine(replaceDir, entries[i].FileName.ToString())));  // File.ReadAllBytes(Path.Combine(replaceDir, entries[i].FileName.ToString()));
                        entries[i].FileOffset = (ulong)newCPK.BaseStream.Position;
                        entries[i].FileSize = Convert.ChangeType(newbie.Length, entries[i].FileSizeType);
                        entries[i].ExtractSize = Convert.ChangeType(newbie.Length, entries[i].FileSizeType);
                        cpk.UpdateFileEntry(entries[i]);
                        newCPK.Write(newbie);

                        GC.Collect();
                    }

                    if ((newCPK.BaseStream.Position % 0x800) > 0)
                    {
                        long cur_pos = newCPK.BaseStream.Position;
                        for (int j = 0; j < (0x800 - (cur_pos % 0x800)); j++)
                        {
                            newCPK.Write((byte)0);
                        }
                    }
                }
                else
                {
                    // Content is special.... just update the position
                    cpk.UpdateFileEntry(entries[i]);
                }
            }

            cpk.WriteCPK(newCPK);
            cpk.WriteITOC(newCPK);
            cpk.WriteTOC(newCPK);
            cpk.WriteETOC(newCPK);
            cpk.WriteGTOC(newCPK);

            newCPK.Close();
            Console.WriteLine("Writing to " + outputCpk);
            File.WriteAllBytes(outputCpk, ms.ToArray());
            oldFile.Close();

            Console.WriteLine("Done in " + time.Elapsed.TotalSeconds);
        }
    }
}

