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


            using (BufferedStream oldFile = new BufferedStream(File.OpenRead(inputCpk)))
            {
                string[] files = Directory.GetFiles(replaceDir, "*.");
                HashSet<string> filesNames = new HashSet<string>();

                foreach (string str in files.Select(x => Path.GetFileNameWithoutExtension(x)))
                    filesNames.Add(str);

                FileInfo fi = new FileInfo(inputCpk);

                Stopwatch time = new Stopwatch();
                time.Start();

                using (BinaryWriter newCPK = new BinaryWriter(new BufferedStream(File.OpenWrite(outputCpk))))
                {
                    List<FileEntry> entries = cpk.FileTable.OrderBy(x => x.FileOffset).ToList();

                    foreach (var entry in entries)
                    {
                        if (entry.FileType != "CONTENT")
                        {
                            if (entry.FileType == "FILE")
                            {
                                if ((ulong)newCPK.BaseStream.Position < cpk.ContentOffset)
                                {
                                    ulong padLength = cpk.ContentOffset - (ulong)newCPK.BaseStream.Position;
                                    newCPK.Write(new byte[padLength], 0, (int)padLength);
                                }
                            }

                            if (entry.FileSize == null || entry.FileOffset == null || entry.FileName == null)
                            {
                                throw new NullReferenceException("Critical properties of the file entry are not initialized.");
                            }

                            if (!filesNames.Contains(entry.FileName.ToString()))
                            {
                                oldFile.Seek((long)entry.FileOffset, SeekOrigin.Begin);
                                entry.FileOffset = (ulong)newCPK.BaseStream.Position;
                                cpk.UpdateFileEntry(entry);

                                byte[] chunk = ReadBytes(oldFile, int.Parse(entry.FileSize.ToString()));
                                newCPK.Write(chunk, 0, chunk.Length);
                            }
                            else
                            {
                                byte[] newbie = File.ReadAllBytes(Path.Combine(replaceDir, entry.FileName.ToString()));
                                entry.FileOffset = (ulong)newCPK.BaseStream.Position;
                                entry.FileSize = Convert.ChangeType(newbie.Length, entry.FileSizeType);
                                entry.ExtractSize = Convert.ChangeType(newbie.Length, entry.FileSizeType);
                                cpk.UpdateFileEntry(entry);
                                newCPK.Write(newbie, 0, newbie.Length);
                            }

                            if ((newCPK.BaseStream.Position % 0x800) > 0)
                            {
                                int padding = (int)(0x800 - (newCPK.BaseStream.Position % 0x800));
                                newCPK.Write(new byte[padding], 0, padding);
                            }
                        }
                        else
                        {
                            cpk.UpdateFileEntry(entry);
                        }
                    }


                    cpk.WriteCPK(newCPK);
                    cpk.WriteITOC(newCPK);
                    cpk.WriteTOC(newCPK);
                    cpk.WriteETOC(newCPK);
                    cpk.WriteGTOC(newCPK);

                    newCPK.Close();
                    oldFile.Close();
                }

                ShinRyuModManager.Program.Log("Writing " + new FileInfo(inputCpk).Name + " took " + time.Elapsed.TotalSeconds);
            }

            /*
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
            */

           // Console.WriteLine("Done in " + time.Elapsed.TotalSeconds);
        }

        static byte[] ReadBytes(BufferedStream stream, int count)
        {
            byte[] buffer = new byte[count];
            int bytesRead = stream.Read(buffer, 0, count);
            if (bytesRead != count)
            {
                throw new EndOfStreamException();
            }
            return buffer;
        }
    }
}

