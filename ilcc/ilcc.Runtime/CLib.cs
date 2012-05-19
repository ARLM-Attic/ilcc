﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace ilcc.Runtime
{
	[CModule]
	unsafe public partial class CLib
	{
		/// <summary>
		/// Global with the arguments pointer.
		/// </summary>
		[CExport]
		public static sbyte** _argv;

		/// <summary>
		/// Global with the argument count.
		/// </summary>
		[CExport]
		public static int _argc;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Size"></param>
		/// <returns></returns>
		[CExport]
		static public void* malloc(int Size)
		{
			return Marshal.AllocHGlobal(Size).ToPointer();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Size"></param>
		/// <returns></returns>
		[CExport]
		static public void* memcpy(sbyte* dest, sbyte* source, int count)
		{
			// TODO: Improve speed copying words
			while (count >= 8)
			{
				*(ulong *)dest = *(ulong *)source;
				count -= 8;
				dest += 8;
				source += 8;
			}

			while (count >= 1)
			{
				*(sbyte*)dest = *(sbyte*)source;
				count -= 1;
				dest += 1;
				source += 1;
			}

			return dest;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Size"></param>
		/// <returns></returns>
		[CExport]
		static public void memset(sbyte* dest, sbyte Value1, int count)
		{
			// TODO: Improve speed copying words
			//for (int n = 0; n < count; n++) ((sbyte*)dest)[n] = value;
			sbyte* dest_end = dest + count;
			ushort Value2 = (ushort)((Value1 << 0) | (Value1 << 8));
			uint Value4 = (uint)((Value2 << 0) | (Value2 << 16));
			ulong Value8 = (uint)((Value4 << 0) | (Value4 << 32));

			while (count >= 8)
			{
				*(ulong*)dest = Value8;
				count -= 8;
				dest += 8;
			}

			while (count >= 4)
			{
				*(uint*)dest = Value4;
				count -= 4;
				dest += 4;
			}

			while (count >= 1)
			{
				*(sbyte*)dest = Value1;
				count -= 1;
				dest += 1;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Pointer"></param>
		[CExport]
		static public void free(void* Pointer)
		{
			Marshal.FreeHGlobal(new IntPtr(Pointer));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Pointer"></param>
		/// <param name="Size"></param>
		/// <returns></returns>
		[CExport]
		static public void* realloc(void* Pointer, int Size)
		{
			return Marshal.ReAllocHGlobal(new IntPtr(Pointer), new IntPtr(Size)).ToPointer();
		}

		/// <summary>
		/// Allocate space for array in memory
		/// Allocates a block of memory for an array of num elements, each of them size bytes long, and initializes all its bits to zero.
		/// The effective result is the allocation of an zero-initialized memory block of (num * size) bytes.
		/// </summary>
		/// <param name="Pointer">Number of elements to be allocated.</param>
		/// <param name="Size">Size of elements.</param>
		/// <returns>
		/// A pointer to the memory block allocated by the function.
		/// The type of this pointer is always void*, which can be cast to the desired type of data pointer in order to be dereferenceable.
		/// If the function failed to allocate the requested block of memory, a NULL pointer is returned.
		/// </returns>
		[CExport]
		static public void* calloc(int Num, int Size)
		{
			return malloc(Num * Size);
		}

#if false
		static private class Internal
		{
			[DllImport("msvcrt.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
			public static extern int printf(__arglist); 
		}
#endif

		[CExport]
		static public int sprintf(__arglist)
		{
			var Arguments = CLibUtils.GetObjectsFromArgsIterator(new ArgIterator(__arglist));
			var Buffer = (sbyte *)((UIntPtr)Arguments[0]).ToPointer();
			var Format = CLibUtils.GetStringFromPointer((UIntPtr)Arguments[1]);
			var Str = CLibUtils.sprintf_hl(Format, Arguments.Skip(2).ToArray());
			var Bytes = CLibUtils.DefaultEncoding.GetBytes(Str);
			Marshal.Copy(Bytes, 0, new IntPtr(Buffer), Bytes.Length);
			Buffer[Bytes.Length] = 0;
			return Bytes.Length;
		}


		[CExport]
		static public int memcmp(__arglist)
		{
			throw (new NotImplementedException());
		}

		[CExport]
		static public int getc(__arglist)
		{
			throw (new NotImplementedException());
		}

		[CExport]
		static public int putc(__arglist)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[CExport]
		static public int puts(sbyte* str)
		{
			if (str != null)
			{
				var Str = CLibUtils.GetStringFromPointer(str);
				Console.WriteLine(Str);
				Console.Out.Flush();
				return Str.Length;
			}
			else
			{
				return 0;
			}
		}

		// FAKE!
		[CExport]
		static public int puti(int value)
		{
			Console.Write(value);
			Console.Out.Flush();
			return 0;
		}

		[CExport]
		static public int clock()
		{
			return (int)(DateTime.Now - Process.GetCurrentProcess().StartTime).TotalMilliseconds;
		}

		[CExport]
		static public int _vsnwprintf(short* s, int n, short* format, sbyte* arg)
		{
			throw(new NotImplementedException());
		}

		[CExport]
		static public int _vsnprintf(sbyte* s, int n, sbyte* format, sbyte* arg)
		{
			throw(new NotImplementedException());
		}

		[CExport]
		static public double strtod(sbyte* str, sbyte** endptr)
		{
			throw (new NotImplementedException());
		}

		[CExport]
		static public double wcstod(short* str, short** endptr)
		{
			throw (new NotImplementedException());
		}

		[CExport]
		static public void _exit(int Result)
		{
			throw (new NotImplementedException());
		}

		[CExport]
		static public int atoi(string str)
		{
			return int.Parse(str);
		}

		[CExport]
		static public long _atoi64(string str)
		{
			return long.Parse(str);
		}

		[CExport]
		static public long _wtoi64(short* str)
		{
			throw (new NotImplementedException());
		}

		[CExport]
		static public sbyte* _i64toa(long value, sbyte *str, int unk)
		{
			throw (new NotImplementedException());
		}

		[CExport]
		static public sbyte* _ui64toa(ulong value, sbyte* str, int unk)
		{
			throw (new NotImplementedException());
		}

		[CExport]
		static public short* _i64tow(long value, short* str, int unk)
		{
			throw (new NotImplementedException());
		}

		[CExport]
		static public short* _ui64tow(ulong value, short* str, int unk)
		{
			throw (new NotImplementedException());
		}

		[CExport] static public int exit(__arglist) { throw (new NotImplementedException()); }
		[CExport] static public int strcmp(__arglist) { throw (new NotImplementedException()); }

		[CExport]
		static public int strlen(string text) { return text.Length; }
	}
}
