using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace batchTwat
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args[0] == "-e")
            {
                foreach(string file in Directory.EnumerateFiles(Directory.GetCurrentDirectory(),"*.twt", SearchOption.AllDirectories))
                {
                    Console.WriteLine("Untwattimg: " + file);
                    unTWaT(file);
                }

                Console.Read();
            }
            else
            {
                foreach (DirectoryInfo di in new DirectoryInfo(Directory.GetCurrentDirectory()).GetDirectories())
                {
                    foreach (DirectoryInfo pdi in di.GetDirectories("PIX*"))
                    {
                        rePIXIE(pdi);
                    }

                    reTWaT(di);
                }
            }
        }
        public static void unTWaT(string file)
        {
            using (BinaryReader br = new BinaryReader(new FileStream(file, FileMode.Open)))
            {
                br.ReadInt32();
                int numFiles = br.ReadInt32();
                List<string> files = new List<string>();
                List<int> filesizes = new List<int>();
                for(int i=0;i<numFiles;i++)
                {
                    int size = br.ReadInt32();
                    filesizes.Add(size);

                    StringBuilder filename = new StringBuilder();
                    while (true)
                    {
                        char letter = br.ReadChar();
                        if (letter == char.MinValue) break;
                        filename.Append(letter);
                    }
                    files.Add(filename.ToString());
                    Console.WriteLine(i.ToString() + ":\t" + filename + "\t" + size);
                    for (int c= filename.Length; c < 51; c++)
                    {
                        br.ReadByte();
                    }
                }
                string targetDirectory = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file));
                if (!Directory.Exists(targetDirectory))
                    Directory.CreateDirectory(targetDirectory);
                for(int i =0; i < numFiles;i++)
                {
                    string targetFilename = Path.Combine(targetDirectory, files[i]);
                    using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(targetFilename)))
                    {
                        bw.Write(br.ReadBytes(filesizes[i]));
                        if (filesizes[i] % 4 != 0)
                        {
                            for (int x = filesizes[i] % 4; x < 4; x++)
                            {
                                br.ReadByte();
                            }
                        }
                    }
                }
            }
        }
        public static void reTWaT(DirectoryInfo di)
        {
            string dest = di.Parent.FullName + "\\" + di.Name + ".twt";

            BinaryWriter bw = new BinaryWriter(new FileStream(dest, FileMode.Create));

            bw.Write(0);
            bw.Write(di.GetFiles().Length);

            foreach (FileInfo fi in di.GetFiles())
            {
                char[] name = (fi.Name + char.MinValue).ToCharArray();

                bw.Write((int)fi.Length);
                bw.Write(name);
                for (int i = name.Length; i < 52; i++)
                {
                    bw.Write((byte)205);
                }
            }

            foreach (FileInfo fi in di.GetFiles())
            {
                using (BinaryReader br = new BinaryReader(new FileStream(fi.FullName, FileMode.Open))) 
                {
                    int length = (int)br.BaseStream.Length;

                    bw.Write(br.ReadBytes(length));
                    if (length % 4 != 0)
                    {
                        for (int i = length % 4; i < 4; i++)
                        {
                            bw.Write((byte)205);
                        }
                    }
                }
            }

            bw.BaseStream.Seek(0, SeekOrigin.Begin);
            bw.Write((int)bw.BaseStream.Length);

            bw.Close();
        }

        public static void rePIXIE(DirectoryInfo di)
        {
            string dest = di.Parent.FullName + "\\" + (di.Name.ToLower() == "pix16" ? "PIXIES.P16" : "PIXIES.P08");

            BinaryWriter bw = new BinaryWriter(new FileStream(dest, FileMode.Create));
            bw.WriteInt32(18);
            bw.WriteInt32(8);
            bw.WriteInt32(2);
            bw.WriteInt32(2);

            foreach (FileInfo fi in di.GetFiles())
            {
                using (BinaryReader br = new BinaryReader(new FileStream(fi.FullName, FileMode.Open)))
                {
                    int length = (int)br.BaseStream.Length - 16;
                    br.BaseStream.Seek(16, SeekOrigin.Begin);
                    bw.Write(br.ReadBytes(21));
                    br.ReadByte();
                    bw.Write((fi.Name.Replace(fi.Extension, "") + char.MinValue).ToCharArray());
                    bw.Write(br.ReadBytes(length - 22));
                }
            }

            bw.Close();
        }
    }

    static class ExtenstionMethods
    {
        public static void WriteInt32(this BinaryWriter bw, int i)
        {
            byte[] b = BitConverter.GetBytes(i);
            Array.Reverse(b);
            bw.Write(BitConverter.ToInt32(b, 0));
        }
    }
}
