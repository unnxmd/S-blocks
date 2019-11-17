using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace S_blocks //вариант 9 m=256, k=(21,57).
{
    public partial class FormMain : Form
    {
        private string[] filesEncrypt;
        private string[] filesDecrypt;

        static int[,] S = {{ 12, 1, 10, 15, 9, 2, 6, 8, 0, 13, 3, 4, 14, 7, 5, 11 },
                        { 10, 15, 4, 2, 7, 12, 9, 5, 6, 1, 13, 14, 0, 11, 3, 8 },
                        { 9, 14, 15, 5, 2, 8, 12, 3, 7, 0, 4, 10, 1, 13, 11, 6 },
                        { 4, 3, 2, 12, 9, 5, 15, 10, 11, 14, 1, 7, 6, 0, 8, 13 } };

        public FormMain()
        {
            InitializeComponent();
        }

        private void listBoxEncrypt_DragDrop(object sender, DragEventArgs e)
        {
            filesEncrypt = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (string file in filesEncrypt)
            {
                listBoxEncrypt.Items.Add(file);
            }
        }

        private void buttonEncrypt_Click(object sender, EventArgs e)
        {
            foreach (string file in filesEncrypt)
            {
                byte[] to_enc = System.IO.File.ReadAllBytes(file);
                bool[] to_enc_bool = ByteToBool(to_enc);
                bool[] enc_bool = Encrypt(to_enc_bool);
                byte[] enc = BitArrayToByteArray(enc_bool);
                System.IO.File.WriteAllBytes(file, enc);
                listBoxEncrypt.Items.Remove(file);
            }
            Array.Clear(filesEncrypt, 0, filesEncrypt.Length);
        }

        private void listBoxDecrypt_DragDrop(object sender, DragEventArgs e)
        {
            filesDecrypt = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (string file in filesDecrypt)
            {
                listBoxDecrypt.Items.Add(file);
            }
        }

        private void buttonDecrypt_Click(object sender, EventArgs e)
        {
            foreach (string file in filesDecrypt)
            {
                byte[] enc_byte = System.IO.File.ReadAllBytes(file);
                bool[] enc = ByteToBool(enc_byte);
                bool[] dec_bool = Decrypt(enc);
                byte[] dec = BitArrayToByteArray(dec_bool);
                System.IO.File.WriteAllBytes(file, dec);
                listBoxDecrypt.Items.Remove(file);
            }
            Array.Clear(filesDecrypt, 0, filesDecrypt.Length);
        }

        static int ConvertToInt(bool[] b)
        {
            b = b.Reverse().ToArray();
            int res = 0;
            for (int i = 0; i < b.Length; i++) if (b[i]) res += Convert.ToInt32(Math.Pow(2, i));
            return res;
        }

        static bool[] ConvertToBinary(int n)
        {
            List<bool> b = new List<bool>();
            while (n > 0)
            {
                if (n % 2 == 1) b.Add(true);
                else b.Add(false);
                n /= 2;
            }
            b.Reverse();
            return b.ToArray();
        }

        static bool[] Encrypt(bool[] to_enc)
        {
            int tail = to_enc.Length % 6;
            bool[] res = new bool[to_enc.Length];
            for (int i = 0; i < to_enc.Length - tail; i += 6)
            {
                int rs = S[ConvertToInt(new bool[] { to_enc[1 + i], to_enc[i] }), ConvertToInt(new bool[] { to_enc[i + 2], to_enc[i + 3], to_enc[i + 4], to_enc[i + 5] })];
                bool[] rs_bit = ConvertToBinary(rs);
                if (rs_bit.Length != 4)
                {
                    int l = rs_bit.Length - 1;
                    int c = 3;
                    bool[] tmp = { false, false, false, false };
                    while (l >= 0)
                    {
                        tmp[c] = rs_bit[l];
                        c--;
                        l--;
                    }
                    rs_bit = tmp;
                }
                bool[] b = new bool[] { to_enc[1 + i], to_enc[i] };
                for (int j = 0; j < 2; j++) res[j + i] = b[j];
                for (int j = 0; j < 4; j++) res[2 + j + i] = rs_bit[j];
            }
            if (tail != 0)
            {
                for (int i = res.Length - tail; i < res.Length; i++) res[i] = to_enc[i];
            }
            return res;
        }

        static bool[] Decrypt(bool[] to_dec)
        {
            int tail = to_dec.Length % 6;
            bool[] res = new bool[to_dec.Length];
            for (int i = 0; i < to_dec.Length - tail; i += 6)
            {
                int str_num = ConvertToInt(new bool[] { to_dec[i], to_dec[i + 1] });
                int num = Return_Position(str_num, (byte)ConvertToInt(new bool[] { to_dec[i + 2], to_dec[i + 3], to_dec[i + 4], to_dec[i + 5] }));

                bool[] num_bool = ConvertToBinary(num);

                if (num_bool.Length != 4)
                {
                    int l = num_bool.Length - 1;
                    int c = 3;
                    bool[] tmp = { false, false, false, false };
                    while (l >= 0)
                    {
                        tmp[c] = num_bool[l];
                        c--;
                        l--;
                    }
                    num_bool = tmp;
                }

                res[i] = to_dec[i + 1];
                res[i + 1] = to_dec[i];

                for (int j = 0; j < num_bool.Length; j++) res[2 + j + i] = num_bool[j];
            }

            if (tail != 0)
            {
                for (int i = res.Length - tail; i < res.Length; i++) res[i] = to_dec[i];
            }
            return res;
        }

        static int Return_Position(int str, byte b)
        {
            int j;
            for (j = 0; j < 16; j++)
            {
                if (S[str, j] == b) break;
            }
            return j;
        }

        public static byte[] BitArrayToByteArray(bool[] bits)
        {
            byte[] ret;
            int tail = bits.Length % 8;
            if (bits.Length % 8 == 0) ret = new byte[bits.Length / 8];
            else ret = new byte[bits.Length / 8 + 1];
            for (int i = 0; i < ret.Length; i++)
            {
                bool[] tmp = new bool[8];
                for (int j = 0; j < 8; j++) tmp[j] = bits[i * 8 + j];
                ret[i] = (byte)ConvertToInt(tmp);
            }
            if (tail != 0)
            {
                bool[] tmp = new bool[tail];
                for (int i = 0; i < tail; i++)
                {
                    tmp[i] = bits[bits.Length - 1 - i];
                }
                ret[ret.Length - 1] = (byte)ConvertToInt(tmp);
            }
            return ret;
        }

        public static bool[] ByteToBool(byte[] b)
        {
            bool[] b_bool = new bool[b.Length * 8];
            for (int i = 0; i < b.Length * 8; i += 8)
            {
                byte bt = b[i / 8];
                bool[] bt_bool = ConvertToBinary(bt);

                if (bt_bool.Length != 8)
                {
                    int l = bt_bool.Length - 1;
                    int c = 7;
                    bool[] tmp = { false, false, false, false, false, false, false, false };
                    while (l >= 0)
                    {
                        tmp[c] = bt_bool[l];
                        c--;
                        l--;
                    }
                    bt_bool = tmp;
                }
                for (int j = 0; j < 8; j++) b_bool[j + i] = bt_bool[j];
            }
            return b_bool;
        }

        private void listBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                e.Effect = DragDropEffects.All;
            }
        }
    }
}
