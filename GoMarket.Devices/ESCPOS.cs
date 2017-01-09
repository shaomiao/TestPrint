/**
 * ESCPOS.cs
 *
 * @author Tony Fox
 * @version 15.12.3
 * @package GoMarket.Devices
 * @updates
 *  - Tony Fox : first version @ 2015/12/3 19:27
 */
 
#region Using directives
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

using GoMarket.LogSystem;

#endregion



namespace GoMarket.Devices {
	// FIXME 如果有非 Native 的接口，则替换此接口
	/// <summary>
	/// ESC-POS Native 接口
	/// </summary>
	public static class ESCPOS {
		public const string PORTNAME = "LPT1";
		#region Windows API
		[StructLayout(LayoutKind.Sequential)]
		struct OVERLAPPED {
			public UIntPtr Internal;
			public UIntPtr InternalHigh;
			public uint Offset;
			public uint OffsetHight;
			IntPtr Event;
		}
		
		[DllImport("kernel32.dll", EntryPoint="CreateFileW", CharSet=CharSet.Unicode)]
		static extern uint CreateFile(
			string lpFilename,
			uint dwDesiredAccess,
			uint dwShareMode,
			uint lpSecurityAttributes,
			uint dwCreationDisposition,
			uint dwFlagsAndAttributes,
			uint hTemplateFile
		);
		
		[DllImport("kernel32.dll", EntryPoint="WriteFile")]
		static extern bool WriteFile(
			uint hFile,
			byte[] lpBuffer,
			uint nNumberOfBytesToWrite,
			out uint lpNumberOfBytesWritten,
			ref OVERLAPPED lpOverlapped
		);
		
		[DllImport("kernel32.dll", EntryPoint="CloseHandle")]
		static extern bool CloseHandle(
			uint hObject
		);
		#endregion
		
		static OVERLAPPED ovl = new ESCPOS.OVERLAPPED();
		#region 成员
		static uint m_shFile = 0;
		#endregion
		/// <summary>
		/// 打开到 ESC-POS 热敏打印机的连接
		/// </summary>
		/// <param name="name">port name</param>
		public static void Open() {
			if (m_shFile != 0)
				return;
			m_shFile = CreateFile(PORTNAME, 0x40000000, 0, 0, 3, 0, 0);
			if (m_shFile == 0xffffffff) {
				m_shFile = 0;
				// NDI : Native Device Interface
				Logger.Error("NDI", "Printer not found @ {0} !", PORTNAME);
				throw new FileNotFoundException("Printer not found!", PORTNAME);
			}
			// 初始化打印机
			uint writted = 0;
			// ESC @
			WriteFile(m_shFile, new byte[] { 0x1b, 0x40 }, 2, out writted, ref ovl);
		}
		/// <summary>
		/// 写入字符串数据到打印机
		/// </summary>
		/// <param name="content">要写入的内容</param>
		public static void Write(string content) {
			byte[] bytes = Encoding.GetEncoding("GB2312").GetBytes(content);
			uint writted = 0;
			bool result = WriteFile(m_shFile, bytes, (uint)bytes.Length, out writted, ref ovl);
			Logger.Info("NDI", "Write : ", result ? "SUCCEEDED" : "FAILED");
		}
        public static void Write2(string content)
        {
            byte[] bytes = Encoding.GetEncoding("GBK").GetBytes(content);
            uint writted = 0;
            bool result = WriteFile(m_shFile, bytes, (uint)bytes.Length, out writted, ref ovl);
            Logger.Info("NDI", "Write : ", result ? "SUCCEEDED" : "FAILED");
        }
        public static void WriteUTF(string content)
        {
            byte[] bytes = Encoding.GetEncoding("UTF-8").GetBytes(content);
            uint writted = 0;
            bool result = WriteFile(m_shFile, bytes, (uint)bytes.Length, out writted, ref ovl);
            Logger.Info("NDI", "Write : ", result ? "SUCCEEDED" : "FAILED");
        }
		/// <summary>
		/// 写入字节数组
		/// </summary>
		/// <param name="bytes">要写入的字节数组</param>
		public static void WriteBytes(byte[] bytes) {
			uint writted = 0;
			bool result = WriteFile(m_shFile, bytes, (uint)bytes.Length, out writted, ref ovl);
			Logger.Info("NDI", "WriteBytes : ", result ? "SUCCEEDED" : "FAILED");
		}
		/// <summary>
		/// 改变文字模式
		/// </summary>
		/// <param name="changeChinese">是否修改中文文字的大小</param>
		/// <param name="doubleWidth">是否双倍宽度</param>
		/// <param name="doubleHeight">是否双倍高度</param>
		public static void SetTextMode(bool changeChinese, bool doubleWidth, bool doubleHeight, bool italic, bool underline) {
			byte n = 0;
			byte n2 = 0;
			if (doubleWidth) {
				n |= 0x10;
				n2 |= 0x04;
			}
			if (doubleHeight) {
				n |= 0x20;
				n2 |= 0x08;
			}
			if (italic)
				n |= 0x08;
			if (underline)
				n |= 0x80;
			uint writted = 0;
			// ESC !
			bool result = WriteFile(m_shFile, new byte[] { 0x1b, 0x21, n }, 3, out writted, ref ovl);
			Logger.Info("NDI", "SetTextMode : ", result ? "SUCCEEDED" : "FAILED");
			if (changeChinese) {
				// FS !
				result = WriteFile(m_shFile, new byte[] { 0x1c, 0x21, n2 }, 3, out writted, ref ovl);
				Logger.Info("NDI", "SetTextMode : ", result ? "SUCCEEDED" : "FAILED");
			}
		}
		public static void SendPulse(byte m, byte t1, byte t2) {
			uint writted = 0;
			// ESC p
			WriteFile(m_shFile, new byte[] { 0x1B, 0x70, m, t1, t2 }, 5, out writted, ref ovl);
		}
		
		/// <summary>
		/// 设置对齐方式
		/// </summary>
		/// <param name="method">0：左对齐 1：居中 2：右对齐</param>
		public static void SetAlignment(byte method) {
			if (method > 2)
				throw new ArgumentOutOfRangeException("method");
			uint writted = 0;
			WriteFile(m_shFile, new byte[] { 0x1b, 0x61, method}, 3, out writted, ref ovl);
		}
		
		/// <summary>
		/// 打印条码，格式为 CODE39
		/// </summary>
		/// <param name="barCode">条码的内容，支持数字、大写字母</param>
		public static void PrintBarcode(string barCode, byte height) {
			barCode = barCode.ToUpper();
			if (!Regex.IsMatch(barCode, @"^[0-9A-Z]+$"))
				throw new FormatException("条码内容必须由半角数字和英文大写字母组成！");
			uint writted = 0;
			bool result = WriteFile(m_shFile, new byte[] { 0x1d, 0x48, 0, 0x1d, 0x68, height, 0x1d, 0x77, 2, 0x1d, 0x66, 0, 0x1d, 0x6b, 4 }, 15, out writted, ref ovl);
			Logger.Info("NDI", "PrintBarcode : ", result ? "SUCCEEDED" : "FAILED");
			Write(barCode);
			result = WriteFile(m_shFile, new byte[] { 0x00 }, 1, out writted, ref ovl);
			Logger.Info("NDI", "PrintBarcode : ", result ? "SUCCEEDED" : "FAILED");
		}
		
		/// <summary>
		/// 打印一个图像
		/// </summary>
		/// <param name="image">图像内容，若像素的 R 不是 0 则打印。</param>
		public static void PrintFixedImage(Bitmap image) {
			if (image == null) {
				throw new ArgumentException();
			}
			
			uint writted = 0;
			bool result = WriteFile(m_shFile, new byte[] { 0x1b, 0x33, 24 }, 3, out writted, ref ovl);
			Logger.Info("NDI", "PrintFixedImage : ", result ? "SUCCEEDED" : "FAILED");
			
			int y = 0, x = 0;
			//Color c = Color.Black;
			byte b = 0;
			
			int rows = (image.Height % 24 == 0) ? (image.Height / 24) : (image.Height / 24 + 1);
			
			byte l = (byte)(image.Width % 256), h = (byte)(image.Width / 256);
			
			for (int i = 0; i < rows * 24 * image.Width; i++) {
				if (i % 8 == 0 && i != 0) {
					WriteFile(m_shFile, new byte[] { b }, 1, out writted, ref ovl);
					b = 0;
				}
				x = (i / 24) % image.Width;
				if (x == 0 && (i % 24) == 0) {
					if( i != 0 )
						WriteFile(m_shFile, new byte[] { 0x0a }, 1, out writted, ref ovl);
					WriteFile(m_shFile, new byte[] { 0x1b, 0x2a, 0x21, l, h }, 5, out writted, ref ovl);
				}
				y = (i % 24) + (i / 24 / image.Width) * 24;
				b <<= 1;
				if (y < image.Height) {
					if (image.GetPixel(x, y).R != 0) {
						b |= 1;
					}
				}
			}
			result = WriteFile(m_shFile, new byte[] { b }, 1, out writted, ref ovl);
			Logger.Info("NDI", "PrintFixedImage : ", result ? "SUCCEEDED" : "FAILED");
			result = WriteFile(m_shFile, new byte[] { 0x0a }, 1, out writted, ref ovl);
			Logger.Info("NDI", "PrintFixedImage : ", result ? "SUCCEEDED" : "FAILED");
		}
		/// <summary>
		/// 打印机进纸并开始工作
		/// </summary>
		/// <param name="units">打印机进纸单位长度（每单位 8 点）</param>
		public static void Feed(int units) {
			int l = units / 256;
			uint writted = 0;
			bool result;
			for (int i = 0; i < l; i++) {
				result = WriteFile(m_shFile, new byte[] { 0x1b, 0x64, 0xff}, 3, out writted, ref ovl);
				Logger.Info("NDI", "Feed : ", result ? "SUCCEEDED" : "FAILED");
			}
			result = WriteFile(m_shFile, new byte[] { 0x1b, 0x64, (byte)(units % 256)}, 3, out writted, ref ovl);
			Logger.Info("NDI", "Feed : ", result ? "SUCCEEDED" : "FAILED");
		}
		/// <summary>
		/// 切纸
		/// </summary>
		public static void CutPapper() {
			uint writted = 0;
			bool result = WriteFile(m_shFile, new byte[] { 0x1b, 0x69, 0x00 }, 3, out writted, ref ovl);
			Logger.Info("NDI", "CutPapper : ", result ? "SUCCEEDED" : "FAILED");
		}
		/// <summary>
		/// 关闭打印机连接
		/// </summary>
		public static void Close() {
			if (m_shFile == 0)
				return;
			CloseHandle(m_shFile);
			m_shFile = 0;
		}
	}
}
