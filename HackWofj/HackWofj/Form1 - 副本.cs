using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace HackWofj
{
    public partial class Form1 : Form
    {
        private FileBinaryReader _fileReader;

        public Form1()
        {
            InitializeComponent();
            _fileReader = new FileBinaryReader();
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 以读写模式打开文件
                    if (_fileReader.OpenFile(openFileDialog.FileName, false))
                    {
                        //lblFileInfo.Text = $"文件已打开，大小: {_fileReader.FileSize} 字节 (读写模式)";
                        btnWriteData.Enabled = true;
                        btnReadData.Enabled = true;
                    }
                }
            }
            if (!_fileReader.IsFileOpen)
            {
                MessageBox.Show("请先打开文件！");
                return;
            }

            // 读取不同偏移量的数据
            string result1 = _fileReader.GetDataAtOffset(0x6E4A4, 16);

            textBox1.Text = $"0x100: {result1}";



        }

        private void btnReadData_Click(object sender, EventArgs e)
        {
            if (!_fileReader.IsFileOpen)
            {
                MessageBox.Show("请先打开文件！");
                return;
            }

            // 读取不同偏移量的数据
            string result1 = _fileReader.GetDataAtOffset(0x6E4A4, 16);

            textBox1.Text = $"0x100: {result1}";
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _fileReader?.Dispose();
        }

        private void btnWriteData_Click(object sender, EventArgs e)
        {
            if (!_fileReader.IsFileOpen)
            {
                MessageBox.Show("请先打开文件！");
                return;
            }

            try
            {
                // 示例1：写入单个字节
                long offset = 0x6E4A4;
                byte data = 0xff; // 十进制：171
                if (_fileReader.WriteByteAtOffset(offset, data))
                {
                    MessageBox.Show($"成功在偏移量 0x{offset:X} 写入字节 0x{data:X2}");
                }

                //// 示例2：写入十六进制字符串
                //long offset2 = 0x200;
                //string hexData = "DEADBEEF";
                //if (_fileReader.WriteHexStringAtOffset(offset2, hexData))
                //{
                //    MessageBox.Show($"成功在偏移量 0x{offset2:X} 写入数据: {hexData}");
                //}

                //// 验证写入的数据
                //string readBack = _fileReader.GetDataAtOffset(offset, 1);
                //string readBack2 = _fileReader.GetDataAtOffset(offset2, 4);
                //textBox1.Text = $"验证读取:\r\n0x{offset:X}: {readBack}\r\n0x{offset2:X}: {readBack2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"写入数据时出错: {ex.Message}");
            }
        }
    }
}
