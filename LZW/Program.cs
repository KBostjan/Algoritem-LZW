using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LZW
{
    class Program
    {
        const int _maxZnakov = 256;
        static private byte[] kodiranoByte;

        static private Dictionary<int, string> Compressor(string text, ref List<int> indices)
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>();

            for (int i = 0; i < 256; i++)
                dictionary.Add(i, new string((char)i, 1));

            char c = '\0';
            int index = 1, n = text.Length, nextKey = 256;
            string s = new string(text[0], 1), sc = string.Empty;

            while (index < n)
            {
                c = text[index++];
                sc = s + c;

                if (dictionary.ContainsValue(sc))
                    s = sc;

                else
                {
                    foreach (KeyValuePair<int, string> kvp in dictionary)
                    {
                        if (kvp.Value == s)
                        {
                            indices.Add(kvp.Key);
                            break;
                        }
                    }

                    dictionary.Add(nextKey++, sc);
                    s = new string(c, 1);
                }
            }

            foreach (KeyValuePair<int, string> kvp in dictionary)
            {
                if (kvp.Value == s)
                {
                    indices.Add(kvp.Key);
                    break;
                }
            }

            return dictionary;
        }


        static private Dictionary<int, string> LWZCompression(string besedilo, ref List<int> T)
        {
            Dictionary<int, string> slovar = new Dictionary<int, string>();

            for(int i = 0; i < _maxZnakov; i++)
            {
                slovar.Add(i, new string((char)i, i));
            }

            char tmp = '\0';
            int j = 1;
            int naslednjiKljuc = 256;
            string str = new string(besedilo[0], 1);
            string strC = null;

            for(; j < besedilo.Length;)
            {
                tmp = besedilo[j++];
                strC = str + tmp;
                if (slovar.ContainsValue(strC))
                {
                    str = strC;
                }
                else
                {
                    foreach (KeyValuePair<int, string> k in slovar)
                    {
                        if (k.Value == str)
                        {
                            T.Add(k.Key);
                            break;
                        }
                    }
                    slovar.Add(naslednjiKljuc++, strC);
                    str = new string(tmp, 1);
                }
               
            }
            foreach (KeyValuePair<int, string> k in slovar)
            {
                if (k.Value == str)
                {
                    T.Add(k.Key);
                    break;
                }
            }

            return slovar;
        }
        static private List<string> Odpri_Kodirano(string imeDatoteke)
        {
            kodiranoByte = File.ReadAllBytes(imeDatoteke);

            BitArray btt = new BitArray(kodiranoByte);
            string t = BitArrayToStr(btt);

            string[] subStrings = t.Split(',');

            List<string> list = new List<string>();
            foreach (string str in subStrings)
            {
                //Console.WriteLine(str);
                list.Add(str);
            }
            list.RemoveAt(list.Count - 1);
            return list;
        }
        public static byte[] createByte(String s)
        {
            ASCIIEncoding e = new ASCIIEncoding();
            return e.GetBytes(s.ToString());
        }
        static private void Shrani_Kodirano(string imeDatoteke, List<int> T , Dictionary<int, string> slovar)
        {
            string str = "";
            foreach (int index in T)
                str += index.ToString() + ',';
            byte[] stt = createByte(str);
            File.WriteAllBytes(imeDatoteke, stt);
        }
        static private string LWZDecompresion(List<string> list)
        {
            List<string> slovar = new List<string>();
            string tmpP; 
            string tmpT; 
            StringBuilder str = new StringBuilder();
            List<string> beseda = new List<string>();

            for(int i = 0; i < _maxZnakov; i++)
            {
                char t = (char)i;
                slovar.Add(t.ToString());
            }

            int tmp = Convert.ToInt32(list[0]);
            char tt = (char)tmp;
            tmpT = tt.ToString();
            //Console.WriteLine("T: " + tmpT);
            beseda.Add(tmpT);
            tmpP = tmpT;

            for (int i = 1; i < list.Count; i++)
            {
                tmp = Convert.ToInt32(list[i]);
                tt = (char)tmp;
                tmpT = slovar[tmp];
                //Console.WriteLine("T: " + tmpT);
                beseda.Add(tmpT);
                char c = tmpT[0];

                string pplusc = tmpP + c;
                slovar.Add(pplusc);
                //Console.WriteLine("P+C: " + pplusc);
                tmpP = tmpT;
            }
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < beseda.Count; i++)
            {
                //Console.Write(beseda[i]);
                s.Append(beseda[i]);
            }
            return s.ToString();
        }
        static string BitArrayToStr(BitArray ba)
        {
            byte[] strArr = new byte[ba.Length / 8];
            ASCIIEncoding encoding = new ASCIIEncoding();

            for (int i = 0; i < ba.Length / 8; i++)
            {
                for (int index = i * 8, m = 1; index < i * 8 + 8; index++, m *= 2)
                {
                    strArr[i] += ba.Get(index) ? (byte)m : (byte)0;
                }
            }
            return encoding.GetString(strArr);
        }
        static void Main(string[] args)
        {
            string potDatoteka = args[1];

            if(args[0] == "c")
            {
                //Compression
                string text = File.ReadAllText(potDatoteka);
               // string test = "Danes je lep soncen dan za programiranje.";
                List<int> T = new List<int>();
                Dictionary<int, string> slovar;
                //slovar = LWZCompression(test, ref T);
                slovar = Compressor(text, ref T);

                //foreach (KeyValuePair<int, string> kvp in slovar)
                //{
                //    if (kvp.Key >= 256)
                //    {
                //        Console.WriteLine( kvp.Key.ToString("D3") + "\t");
                //        Console.WriteLine(kvp.Value + "\r");
                //    }
                //}
                //Console.WriteLine("T: ");
                //Console.WriteLine();
                //foreach (int index in T)
                //    Console.WriteLine( index.ToString() + "\r");

                Shrani_Kodirano("out.bin", T, slovar);

            }
            if(args[0] == "d")
            {
                //Decompression
                List<string> decomp = Odpri_Kodirano(potDatoteka);

                string dtext = LWZDecompresion(decomp);

                File.WriteAllText("out.bin", dtext);

            }


        }
    }
}
