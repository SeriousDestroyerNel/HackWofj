using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace HackWofj
{
    public partial class HackWofj : Form
    {
        private FileBinaryReader _fileReader;
        private bool _isUpdatingGrid = false; // 防止在更新表格时触发事件
        private const long START_OFFSET = 0x6DB44;
        private const long END_OFFSET = 0x6E754;
        private const int BYTES_PER_ROW = 16;
        private bool isUpdating = false; // 防止递归更新
        private string[] _predefinedNames = new string[]
    {
        "关羽a1", "关羽a2", "关羽a3", "关羽原地跳a", "关羽前跳a",
        "关羽下b1，下b2", "关羽上下a1", "关羽上下a2", "关羽上下a3",
        "全角色ab，马ab", "关羽马a1", "关羽马a2", "关羽马a3",
        "关羽马后a", "关羽马b", "关羽马后b", "关羽马前b", "关羽马前ba",
        "关羽马bb1", "关羽马aaa", "关羽马波", "关羽马bb2", "关羽未知",
        "关羽未知", "关羽未知", "关羽未知", "关羽未知", "关羽未知",
        "关羽未知", "关羽未知",

        "张飞a1", "张飞a2", "张飞a3", "张飞原地跳a", "张飞前跳a",
        "张飞下b1，下b2", "张飞未知", "张飞上下a1", "张飞上下a2",
        "张飞上下a3", "张飞马a1", "张飞马a2", "张飞马a3",
        "张飞马后a", "张飞马b", "张飞马后b", "张飞马前b", "张飞马前ba",
        "张飞马bb1", "张飞马aaa", "张飞马波", "张飞未知", "张飞马bb2",
        "张飞未知", "张飞未知", "张飞未知", "张飞未知", "张飞未知",
        "张飞未知", "张飞未知",

        "赵云a1", "赵云a2", "赵云a3", "赵云原地跳a", "赵云前跳a",
        "赵云下b1，下b2", "赵云上下a1", "赵云上下a2", "赵云上下a3",
        "赵云抓人跳a", "赵云马a1", "赵云马a2", "赵云马a3",
        "赵云马后a", "赵云马b", "赵云马后b", "赵云马前b", "赵云马前ba",
        "赵云马前bb1", "赵云马aaa", "赵云马波", "赵云未知", "赵云马前bb2",
        "赵云未知",

        "黄忠马a1，马后b1", "黄忠马a2，马后b2", "黄忠马a3，马后b3",
        "黄忠马后a1", "黄忠马后a2", "黄忠马后a3", "黄忠未知", "黄忠未知",
        "黄忠原地跳a", "黄忠马bb2", "黄忠前跳a", "黄忠下b1，下b2",
        "黄忠上下a1", "黄忠上下a2", "黄忠上下a3", "黄忠未知", "黄忠未知",
        "黄忠未知", "黄忠未知", "黄忠马b", "黄忠未知", "黄忠未知",
        "黄忠马前b", "黄忠马前ba", "黄忠马bb1", "黄忠aaa，马aaa",
        "黄忠马波", "黄忠a1", "黄忠a2", "黄忠a3", "黄忠未知",
        "黄忠未知", "黄忠未知", "黄忠未知", "黄忠未知", "黄忠未知",

        "魏延a1", "魏延a2", "魏延a3", "魏延原地跳a", "魏延前跳a",
        "魏延下b1 ，下b2", "魏延上下a1（远脚刀）", "魏延上下a2（远脚刀）",
        "魏延上下a3（远脚刀）", "魏延未知", "魏延马a1", "魏延马a3",
        "魏延马波", "魏延马后a", "魏延马b", "魏延马后b", "魏延马前b",
        "魏延马前ba", "魏延马前bb1", "魏延aaa", "魏延上下a1（近脚刀）",
        "魏延上下a2（近脚刀）", "魏延未知", "魏延未知", "魏延未知",
        "魏延未知", "魏延未知", "魏延未知", "魏延未知", "魏延未知",

        "剑", "草薙剑", "怒龙", "青釭剑", "倚天剑",
        "未知", "七星剑", "圣剑", "村正1段", "村正2段",
        "村正3段", "正宗1段", "正宗2段", "正宗3段", "村雨1段",
        "村雨2段", "村雨3段", "虎撤1段", "虎撤2段", "菊一文字1段",
        "菊一文字2段", "飞龙1段", "飞龙2段", "正国1段", "正国2段",
        "青龙刀", "偃月刀", "战斧", "斧", "晕锤",
        "金锤", "狼牙棒", "狼牙棒B", "小槌", "叉",

        "未知", "未知", "未知", "未知", "未知", "未知", "未知", "魏延马a2"


    };

        public HackWofj()
        {
            InitializeComponent();
            _fileReader = new FileBinaryReader();
            InitializeDataGridView();
            InitializeConversionTextBoxes();
        }

        /// <summary>
        /// 初始化换算文本框
        /// </summary>
        private void InitializeConversionTextBoxes()
        {
            // 绑定TextChanged事件
            textBox2.TextChanged += textBox2_TextChanged;
            textBox3.TextChanged += textBox3_TextChanged;
        }   



        private void InitializeDataGridView()
        {
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                // 禁止用户调整列宽
                column.Resizable = DataGridViewTriState.False;
                // 禁止自动调整列宽
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                // 确保列宽保持不变（如果你在设计器已经设置了列宽，这里可以省略）
                column.Width = 60;
            }
            dataGridView1.AllowUserToAddRows = false;
            // 注册事件
            dataGridView1.CellEndEdit += DataGridView1_CellEndEdit;
            dataGridView1.EditingControlShowing += DataGridView1_EditingControlShowing;
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
                        //lblFileInfo.Text = $"文件已打开，大小: {_fileReader.FileSize} 字节";
                        LoadDataToGrid();
                    }
                }
            }
        }

        /// <summary>
        /// 将数据加载到表格
        /// </summary>
        /// <summary>
        /// 将数据加载到表格
        /// </summary>
        private void LoadDataToGrid()
        {
            try
            {
                _isUpdatingGrid = true;
                dataGridView1.Rows.Clear();

                long totalBytes = END_OFFSET - START_OFFSET + 1;
                int totalRows = (int)(totalBytes / BYTES_PER_ROW);

                for (int row = 0; row < totalRows; row++)
                {
                    long rowOffset = START_OFFSET + (row * BYTES_PER_ROW);

                    // 添加新行
                    DataGridViewRow dataGridViewRow = new DataGridViewRow();
                    dataGridViewRow.CreateCells(dataGridView1);

                    // 设置偏移量（第1列）
                    dataGridViewRow.Cells[0].Value = $"0x{rowOffset:X}";

                    // 第2列：名称（从预定义数组中获取，如果数组不够长则使用默认名称）
                    string name = row < _predefinedNames.Length ? _predefinedNames[row] : $"数据区域 {row + 1}";
                    dataGridViewRow.Cells[1].Value = name;

                    // 读取16字节数据到第3-18列
                    for (int col = 0; col < BYTES_PER_ROW; col++)
                    {
                        long byteOffset = rowOffset + col;
                        if (byteOffset <= END_OFFSET)
                        {
                            string hexData = _fileReader.GetDataAtOffset(byteOffset, 1);
                            dataGridViewRow.Cells[col + 2].Value = hexData; // +2 因为前2列是偏移量和名称
                        }
                    }

                    dataGridView1.Rows.Add(dataGridViewRow);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载数据到表格时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isUpdatingGrid = false;
            }
        }


        private void btnReadData_Click(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// 单元格结束编辑事件 - 写入数据到文件
        /// </summary>
        /// <summary>
        /// 单元格结束编辑事件 - 写入数据到文件
        /// </summary>
        private void DataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (_isUpdatingGrid || e.RowIndex < 0 || e.ColumnIndex < 2) // 从第3列开始才是数据列
                return;

            try
            {
                var cell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                string newValue = cell.Value?.ToString() ?? "";

                // 只处理数据列（第3-18列）
                if (e.ColumnIndex >= 2 && e.ColumnIndex <= 17)
                {
                    // 验证输入格式
                    if (!IsValidHexByte(newValue))
                    {
                        MessageBox.Show("请输入有效的十六进制字节（00-FF）", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        LoadDataToGrid(); // 重新加载数据恢复原值
                        return;
                    }

                    // 计算实际文件偏移量
                    long rowOffset = GetRowOffset(e.RowIndex);
                    long byteOffset = rowOffset + (e.ColumnIndex - 2); // -2 因为前2列是偏移量和名称

                    // 写入数据到文件
                    if (_fileReader.WriteHexStringAtOffset(byteOffset, newValue))
                    {
                        // 写入成功，更新单元格显示（确保大写）
                        cell.Value = newValue.ToUpper();
                    }
                    else
                    {
                        // 写入失败，恢复原值
                        LoadDataToGrid();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"写入数据时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadDataToGrid(); // 出错时重新加载数据
            }
        }

        /// <summary>
        /// 编辑控件显示事件 - 限制输入（仅对数据列）
        /// </summary>
        private void DataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is System.Windows.Forms.TextBox textBox)
            {
                // 获取当前编辑的列索引
                int columnIndex = dataGridView1.CurrentCell.ColumnIndex;

                // 只对数据列（第3-18列）进行输入限制
                if (columnIndex >= 2 && columnIndex <= 17)
                {
                    textBox.KeyPress -= TextBox_KeyPress;
                    textBox.KeyPress += TextBox_KeyPress;
                    textBox.TextChanged -= TextBox_TextChanged;
                    textBox.TextChanged += TextBox_TextChanged;
                }
                else
                {
                    // 对于偏移量和名称列，移除限制（虽然它们都是只读的）
                    textBox.KeyPress -= TextBox_KeyPress;
                    textBox.TextChanged -= TextBox_TextChanged;
                }
            }
        }

        /// <summary>
        /// 键盘输入限制
        /// </summary>
        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许输入 0-9, A-F, a-f, 退格键
            if (!char.IsControl(e.KeyChar) && !IsHexCharacter(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 文本变化事件 - 限制长度和自动大写
        /// </summary>
        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.TextBox textBox)
            {
                // 转换为大写
                textBox.Text = textBox.Text.ToUpper();
                textBox.SelectionStart = textBox.Text.Length;

                // 限制长度为2
                if (textBox.Text.Length > 2)
                {
                    textBox.Text = textBox.Text.Substring(0, 2);
                    textBox.SelectionStart = 2;
                }
            }
        }

        /// <summary>
        /// 获取指定行的起始偏移量
        /// </summary>
        private long GetRowOffset(int rowIndex)
        {
            return START_OFFSET + (rowIndex * BYTES_PER_ROW);
        }

        /// <summary>
        /// 检查是否为有效的十六进制字节
        /// </summary>
        private bool IsValidHexByte(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length != 2)
                return false;

            return Regex.IsMatch(input, "^[0-9A-Fa-f]{2}$");
        }

        /// <summary>
        /// 检查字符是否为十六进制字符
        /// </summary>
        private bool IsHexCharacter(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (_fileReader.IsFileOpen)
            {
                LoadDataToGrid();
            }
            else
            {
                MessageBox.Show("请先打开文件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (isUpdating) return;

            isUpdating = true;

            try
            {
                string hexText = textBox2.Text.Trim();

                if (string.IsNullOrEmpty(hexText))
                {
                    textBox3.Text = "";
                    return;
                }

                // 确保是有效的16进制数
                if (IsValidHex(hexText))
                {
                    // 转换为10进制
                    int decimalValue = Convert.ToInt32(hexText, 16);

                    // 检查是否超过255
                    if (decimalValue <= 255)
                    {
                        textBox3.Text = decimalValue.ToString();
                    }
                    else
                    {
                        // 如果超过255，截断为FF
                        textBox2.Text = "FF";
                        textBox3.Text = "255";
                    }
                }
                else
                {
                    // 如果不是有效的16进制，清空另一个文本框
                    textBox3.Text = "";
                }
            }
            catch (Exception ex)
            {
                // 转换失败时清空另一个文本框
                textBox3.Text = "";
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许输入16进制字符（0-9, A-F, a-f）和退格键
            if (!char.IsControl(e.KeyChar) &&
                !((e.KeyChar >= '0' && e.KeyChar <= '9') ||
                  (e.KeyChar >= 'A' && e.KeyChar <= 'F') ||
                  (e.KeyChar >= 'a' && e.KeyChar <= 'f')))
            {
                e.Handled = true;
            }
        }

        

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (isUpdating) return;

            isUpdating = true;

            try
            {
                string decText = textBox3.Text.Trim();

                if (string.IsNullOrEmpty(decText))
                {
                    textBox2.Text = "";
                    return;
                }

                // 确保是有效的数字
                if (int.TryParse(decText, out int decimalValue))
                {
                    // 检查是否在0-255范围内
                    if (decimalValue >= 0 && decimalValue <= 255)
                    {
                        // 转换为16进制，并格式化为2位大写
                        textBox2.Text = decimalValue.ToString("X2");
                    }
                    else if (decimalValue > 255)
                    {
                        // 如果超过255，设置为最大值
                        textBox3.Text = "255";
                        textBox2.Text = "FF";
                    }
                    else if (decimalValue < 0)
                    {
                        // 如果小于0，设置为最小值
                        textBox3.Text = "0";
                        textBox2.Text = "00";
                    }
                }
                else
                {
                    // 如果不是有效的数字，清空另一个文本框
                    textBox2.Text = "";
                }
            }
            catch (Exception ex)
            {
                // 转换失败时清空另一个文本框
                textBox2.Text = "";
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 只允许输入数字和退格键
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private bool IsValidHex(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            foreach (char c in text)
            {
                if (!((c >= '0' && c <= '9') ||
                      (c >= 'A' && c <= 'F') ||
                      (c >= 'a' && c <= 'f')))
                {
                    return false;
                }
            }
            return true;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
