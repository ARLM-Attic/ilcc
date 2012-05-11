﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ilcc.Runtime
{
	unsafe public class CLib
	{
		static public void* malloc(int Count)
		{
			return Marshal.AllocHGlobal(Count).ToPointer();
		}

		static public void free(void* Pointer)
		{
			Marshal.FreeHGlobal(new IntPtr(Pointer));
		}

		static public sbyte* GetLiteralStringPointer(string Text)
		{
			var Bytes = Encoding.UTF8.GetBytes(Text + "\0");
			var Pointer = (sbyte*)Marshal.AllocHGlobal(Bytes.Length).ToPointer();
			Marshal.Copy(Bytes, 0, new IntPtr(Pointer), Bytes.Length);
			return Pointer;
		}

		static public void printf(sbyte* format, params object[] Params)
		{
			throw(new NotImplementedException());
		}
	}
}
