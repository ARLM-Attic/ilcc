﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ilcclib.Parser;
using ilcclib.Converter.CIL;
using ilcclib.Converter;
using ilcclib.Compiler;
using System.Reflection;
using System.Runtime.InteropServices;
using ilcclib.Preprocessor;
using System.IO;

namespace ilcclib.Tests.Converter.CIL
{
	[TestClass]
	unsafe public class CILConverterTest
	{
		static private Type CompileProgram(string CProgram)
		{
			return CCompiler.CompileProgram(CProgram);
		}

		static private string CaptureOutput(Action Action)
		{
			var OldOut = Console.Out;
			var StringWriter = new StringWriter();
			Console.SetOut(StringWriter);
			{
				Action();
			}
			Console.SetOut(OldOut);
			return StringWriter.ToString();
		}

		[TestMethod]
		public void TestSimpleMethod()
		{
			var TestMethod = CompileProgram(@"
				int test(int arg) {
					return arg;
				}
			").GetMethod("test");
			Assert.AreEqual(777, TestMethod.Invoke(null, new object[] { 777 }));
		}

		[TestMethod]
		public void TestPointerAsArraySet()
		{
			var TestMethod = CompileProgram(@"
				void test(int *ptr, int arg) {
					ptr[0] = arg;
				}
			").GetMethod("test");

			int Value = -7;

			TestMethod.Invoke(null, new object[] { new IntPtr(&Value), 777 });

			Assert.AreEqual(777, Value);
		}

		[TestMethod]
		public void TestSimpleFor()
		{
			var TestMethod = CompileProgram(@"
				int test() {
					int n, m = 0;
					for (n = 0; n < 10; n++) m = m + n;
					return m;
				}
			").GetMethod("test");

			Assert.AreEqual(45, TestMethod.Invoke(null, new object[] { }));
		}

		[TestMethod]
		public void TestPrintf()
		{
			var TestMethod = CompileProgram(@"
				void test() {
					printf(""Hello World %d!"", 7);
				}
			").GetMethod("test");

			var Output = CaptureOutput(() =>
			{
				TestMethod.Invoke(null, new object[] { });
			});

			Assert.AreEqual("Hello World 7!", Output);
		}

		[TestMethod]
		public void TestForBreak()
		{
			var TestMethod = CompileProgram(@"
				int test() {
					int n, m = 0;
					for (n = 0; n < 10; n++) {
						if (n == 5) break;
						m = m + n;
					}
					return m;
				}
			").GetMethod("test");

			Assert.AreEqual(10, TestMethod.Invoke(null, new object[] { }));
		}

		[TestMethod]
		public void TestForContinue()
		{
			var TestMethod = CompileProgram(@"
				int test() {
					int n, m = 0;
					for (n = 0; n < 10; n++) {
						if (n % 2) continue;
						m = m + n;
					}
					return m;
				}
			").GetMethod("test");

			Assert.AreEqual(20, TestMethod.Invoke(null, new object[] { }));
		}

		[TestMethod]
		public void TestForEver()
		{
			var TestMethod = CompileProgram(@"
				int test() {
					int n = 0;
					for (;;) { n = 7; break; }
					return n;
				}
			").GetMethod("test");

			Assert.AreEqual(7, TestMethod.Invoke(null, new object[] { }));
		}

		[TestMethod]
		public void TestForMultipleInitializers()
		{
			var TestMethod = CompileProgram(@"
				int test() {
					int a = -1, b = -1;
					for (a = 3, b = 4;;) break;
					return a + b;
				}
			").GetMethod("test");

			Assert.AreEqual(7, TestMethod.Invoke(null, new object[] { }));
		}

		[TestMethod]
		public void TestWhile()
		{
			var TestMethod = CompileProgram(@"
				int test() {
					int n = 0;
					int m = 0;
					while (n < 14) {
						n++;
						if (n % 2 == 0) continue;
						if (n == 10) break;
						m = m + n;
					}
					return m;
				}
			").GetMethod("test");

			Assert.AreEqual(49, TestMethod.Invoke(null, new object[] { }));
		}

		[TestMethod]
		public void TestDoWhile()
		{
			var TestMethod = CompileProgram(@"
				int test() {
					int n = 0;
					int m = 0;
					do {
						m = 7;
					} while (n != 0);
					return m;
				}
			").GetMethod("test");

			Assert.AreEqual(7, TestMethod.Invoke(null, new object[] { }));
		}

		[TestMethod]
		public void TestGotoUp()
		{
			var TestMethod = CompileProgram(@"
				int test() {
					int n = 0;
					int m = 0;
					loop:
					{
						n++;
						m = m + n;
					}
					if (n < 10) goto loop;
					return m;
				}
			").GetMethod("test");

			Assert.AreEqual(55, TestMethod.Invoke(null, new object[] { }));
		}

		[TestMethod]
		public void TestGotoDown()
		{
			var TestMethod = CompileProgram(@"
				int test(int n) {
					int m = -7;
					if (n == 1) goto Skip;
					m = 7;
					Skip:
					return m;
				}
			").GetMethod("test");

			Assert.AreEqual(7, TestMethod.Invoke(null, new object[] { 0 }));
			Assert.AreEqual(-7, TestMethod.Invoke(null, new object[] { 1 }));
		}

		[TestMethod]
		public void TestSimpleSwitch()
		{
			var TestMethod = CompileProgram(@"
				int test(int v) {
					int z;
					switch (v) {
						case 1: z = -1; break;
						case 2: z = -2; break;
						case 10: z = -10; // Notice that there is not a break, so 10 will yield -11.
						case 11: z = -11; break;
						default: z = -999; break;
					}
					return z;
				}
			").GetMethod("test");

			Assert.AreEqual(-999, TestMethod.Invoke(null, new object[] { -1 }));
			Assert.AreEqual(-999, TestMethod.Invoke(null, new object[] { 0 }));
			Assert.AreEqual(-1, TestMethod.Invoke(null, new object[] { 1 }));
			Assert.AreEqual(-2, TestMethod.Invoke(null, new object[] { 2 }));
			Assert.AreEqual(-999, TestMethod.Invoke(null, new object[] { 3 }));
			Assert.AreEqual(-11, TestMethod.Invoke(null, new object[] { 10 }));
			Assert.AreEqual(-11, TestMethod.Invoke(null, new object[] { 11 }));
			Assert.AreEqual(-999, TestMethod.Invoke(null, new object[] { 12 }));
		}

		[TestMethod]
		public void TestSimpleIf()
		{
			var TestMethod = CompileProgram(@"
				char *test(int a) {
					if (a > 5) return ""greater than 5"";
					return ""not greater than 5"";
				}
			").GetMethod("test");

			{
				var Result = (Pointer)TestMethod.Invoke(null, new object[] { 6 });
				var Pointer2 = new IntPtr(Pointer.Unbox(Result));
				//Console.WriteLine(Marshal.PtrToStringAnsi(Pointer2));
				Assert.AreEqual("greater than 5", Marshal.PtrToStringAnsi(Pointer2));
			}
			{
				var Result = (Pointer)TestMethod.Invoke(null, new object[] { 5 });
				var Pointer2 = new IntPtr(Pointer.Unbox(Result));
				//Console.WriteLine(Marshal.PtrToStringAnsi(Pointer2));
				Assert.AreEqual("not greater than 5", Marshal.PtrToStringAnsi(Pointer2));
			}
		}

		[TestMethod]
		public void TestSizeof()
		{
			var TestProgram = CompileProgram(@"
				typedef struct TestStruct { int a, b, c; } TestStruct;
				int sizeof_int32() { return sizeof(int); }
				int sizeof_int64() { return sizeof(long long int); }
				int sizeof_struct() { return sizeof(TestStruct); }
			");

			Assert.AreEqual(sizeof(int), TestProgram.GetMethod("sizeof_int32").Invoke(null, new object[] { }));
			Assert.AreEqual(sizeof(long), TestProgram.GetMethod("sizeof_int64").Invoke(null, new object[] { }));
			Assert.AreEqual(sizeof(int) * 3, TestProgram.GetMethod("sizeof_struct").Invoke(null, new object[] { }));
		}

		[TestMethod]
		public void TestFieldAccess()
		{
			var TestMethod = CompileProgram(@"
				typedef struct TestStruct { int x, y, z; } TestStruct;

				int test() {
					TestStruct v;
					v.z = 300;
					v.y = 20;
					v.x = 1;
					return v.x + v.y + v.z;
				}
			").GetMethod("test");

			Assert.AreEqual(321, TestMethod.Invoke(null, new object[] { }));
		}

		[TestMethod]
		public void TestIncrementAssign()
		{
			var TestMethod = CompileProgram(@"
				int test(int value) {
					int result = 7;
					result += value;
					return result;
				}
			").GetMethod("test");

			Assert.AreEqual(11, TestMethod.Invoke(null, new object[] { 4 }));
		}

		[TestMethod]
		public void TestAlloca()
		{
			var TestMethod = CompileProgram(@"
				int test() {
					int result = 0;
					int m = 10, n;
					int *data = (int *)alloca(sizeof(int) * m);
					for (n = 0; n < m; n++) data[n] = n;
					for (n = 0; n < m; n++) result += data[n];
					return result;
				}
			").GetMethod("test");

			Assert.AreEqual(45, TestMethod.Invoke(null, new object[] { }));
		}

		[TestMethod]
		public void TestReferencingAndDereferencing()
		{
			var TestMethod = CompileProgram(@"
				int test() {
					int z;
					int *ptr = &z;
					*ptr = 7;
					return z;
				}
			").GetMethod("test");

			Assert.AreEqual(7, TestMethod.Invoke(null, new object[] { }));
		}

		[TestMethod]
		public void TestReferencingAndDereferencingStructTypes()
		{
			var Program = CompileProgram(@"
				typedef struct { int x, y, z; } Point;

				int test1() {
					Point *point = malloc(sizeof(Point));
					point->x = 7;
					return point->x;
				}

				int test2() {
					Point *point = malloc(sizeof(Point) * 10);
					point[0].x = 7;
					return point[0].x;
				}
			");

			Assert.AreEqual(7, Program.GetMethod("test1").Invoke(null, new object[] { }));
			Assert.AreEqual(7, Program.GetMethod("test2").Invoke(null, new object[] { }));
		}
	}
}