using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace HackWofj
{
    public class FileBinaryReader : IDisposable
    {
        private FileStream _fileStream;
        private string _currentFilePath;
        private bool _isDisposed = false;
        private bool _isReadOnly = false;

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="readOnly">是否只读模式</param>
        /// <returns>是否成功打开</returns>
        public bool OpenFile(string filePath, bool readOnly = false)
        {
            try
            {
                // 如果已有文件流打开，先关闭
                CloseFile();

                if (!File.Exists(filePath))
                {
                    MessageBox.Show("文件不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                _isReadOnly = readOnly;
                FileAccess fileAccess = readOnly ? FileAccess.Read : FileAccess.ReadWrite;
                FileShare fileShare = readOnly ? FileShare.Read : FileShare.None;

                _fileStream = new FileStream(filePath, FileMode.Open, fileAccess, fileShare);
                _currentFilePath = filePath;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开文件时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 关闭文件
        /// </summary>
        public void CloseFile()
        {
            _fileStream?.Close();
            _fileStream?.Dispose();
            _fileStream = null;
            _currentFilePath = null;
        }

        /// <summary>
        /// 向指定偏移量写入一个字节并自动保存
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="data">要写入的字节数据（0-255）</param>
        /// <returns>是否写入成功</returns>
        public bool WriteByteAtOffset(long offset, byte data)
        {
            if (_fileStream == null)
            {
                MessageBox.Show("请先打开文件！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (_isReadOnly)
            {
                MessageBox.Show("文件以只读模式打开，无法写入！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                // 检查偏移量是否有效
                if (offset < 0 || offset > _fileStream.Length)
                {
                    MessageBox.Show($"偏移量 0x{offset:X} 超出文件范围！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // 保存当前位置
                long originalPosition = _fileStream.Position;

                // 移动到指定偏移量
                _fileStream.Seek(offset, SeekOrigin.Begin);

                // 写入字节
                _fileStream.WriteByte(data);

                // 强制刷新到磁盘
                _fileStream.Flush();

                // 恢复原始位置（如果原始位置在写入位置之后，需要调整）
                if (originalPosition > offset)
                {
                    originalPosition++; // 因为插入了一个字节，位置后移
                }
                _fileStream.Seek(originalPosition, SeekOrigin.Begin);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"写入数据时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 向指定偏移量写入字节数组并自动保存
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="data">要写入的字节数组</param>
        /// <returns>是否写入成功</returns>
        public bool WriteBytesAtOffset(long offset, byte[] data)
        {
            if (_fileStream == null)
            {
                MessageBox.Show("请先打开文件！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (_isReadOnly)
            {
                MessageBox.Show("文件以只读模式打开，无法写入！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (data == null || data.Length == 0)
            {
                MessageBox.Show("要写入的数据不能为空！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                // 检查偏移量是否有效
                if (offset < 0 || offset > _fileStream.Length)
                {
                    MessageBox.Show($"偏移量 0x{offset:X} 超出文件范围！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // 检查写入范围是否超出文件末尾
                if (offset + data.Length > _fileStream.Length)
                {
                    // 可以选择扩展文件大小或者截断数据
                    if (MessageBox.Show($"写入数据将超出文件末尾，是否扩展文件大小？", "确认",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _fileStream.SetLength(offset + data.Length);
                    }
                    else
                    {
                        // 截断数据以适应文件大小
                        int availableLength = (int)(_fileStream.Length - offset);
                        if (availableLength <= 0)
                        {
                            MessageBox.Show("没有可用空间写入数据！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                        Array.Resize(ref data, availableLength);
                    }
                }

                // 保存当前位置
                long originalPosition = _fileStream.Position;

                // 移动到指定偏移量
                _fileStream.Seek(offset, SeekOrigin.Begin);

                // 写入字节数组
                _fileStream.Write(data, 0, data.Length);

                // 强制刷新到磁盘
                _fileStream.Flush();

                // 恢复原始位置
                _fileStream.Seek(originalPosition, SeekOrigin.Begin);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"写入数据时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 向指定偏移量写入十六进制字符串并自动保存
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="hexString">十六进制字符串（如 "FF" 或 "A1B2C3"）</param>
        /// <returns>是否写入成功</returns>
        public bool WriteHexStringAtOffset(long offset, string hexString)
        {
            try
            {
                // 将十六进制字符串转换为字节数组
                byte[] data = HexStringToByteArray(hexString);
                if (data == null)
                {
                    MessageBox.Show("十六进制字符串格式错误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return WriteBytesAtOffset(offset, data);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"写入十六进制数据时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 将十六进制字符串转换为字节数组
        /// </summary>
        private byte[] HexStringToByteArray(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return new byte[0];

            // 移除可能的分隔符
            hex = hex.Replace(" ", "").Replace("-", "").Replace(":", "");

            // 检查长度是否为偶数
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("十六进制字符串长度必须为偶数");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                string byteValue = hex.Substring(i, 2);
                bytes[i / 2] = Convert.ToByte(byteValue, 16);
            }

            return bytes;
        }

        // 原有的读取方法保持不变
        public string ReadFullFileAsHex()
        {
            if (_fileStream == null)
            {
                MessageBox.Show("请先打开文件！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }

            try
            {
                // 保存当前位置
                long originalPosition = _fileStream.Position;

                // 移动到文件开头
                _fileStream.Seek(0, SeekOrigin.Begin);

                // 读取所有字节
                byte[] fileBytes = new byte[_fileStream.Length];
                int bytesRead = _fileStream.Read(fileBytes, 0, fileBytes.Length);

                // 恢复原始位置
                _fileStream.Seek(originalPosition, SeekOrigin.Begin);

                // 转换为十六进制字符串
                StringBuilder hexBuilder = new StringBuilder(bytesRead * 2);
                for (int i = 0; i < bytesRead; i++)
                {
                    hexBuilder.AppendFormat("{0:X2}", fileBytes[i]);
                }

                return hexBuilder.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取文件时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }

        public string GetDataAtOffset(long offset, int length = 1)
        {
            if (_fileStream == null)
            {
                MessageBox.Show("请先打开文件！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }

            try
            {
                // 检查偏移量是否有效
                if (offset < 0 || offset >= _fileStream.Length)
                {
                    MessageBox.Show($"偏移量 0x{offset:X} 超出文件范围！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return string.Empty;
                }

                // 检查读取长度是否有效
                if (offset + length > _fileStream.Length)
                {
                    length = (int)(_fileStream.Length - offset);
                    if (length == 0)
                    {
                        return string.Empty;
                    }
                }

                // 移动到指定偏移量
                _fileStream.Seek(offset, SeekOrigin.Begin);

                // 读取数据
                byte[] buffer = new byte[length];
                int bytesRead = _fileStream.Read(buffer, 0, length);

                if (bytesRead == 0)
                {
                    return string.Empty;
                }

                // 转换为十六进制字符串
                StringBuilder hexBuilder = new StringBuilder(bytesRead * 2);
                for (int i = 0; i < bytesRead; i++)
                {
                    hexBuilder.AppendFormat("{0:X2}", buffer[i]);
                }

                return hexBuilder.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取偏移量数据时出错：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        public long FileSize
        {
            get { return _fileStream?.Length ?? 0; }
        }

        /// <summary>
        /// 检查文件是否已打开
        /// </summary>
        public bool IsFileOpen
        {
            get { return _fileStream != null; }
        }

        /// <summary>
        /// 检查是否只读模式
        /// </summary>
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        /// <summary>
        /// 获取当前文件路径
        /// </summary>
        public string CurrentFilePath
        {
            get { return _currentFilePath; }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                CloseFile();
                _isDisposed = true;
            }
        }
    }
}
