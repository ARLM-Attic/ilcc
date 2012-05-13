﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ilcclib.Ast.Types;

namespace ilcclib.Ast.Expression
{
	public class CastExpressionAstNode : ExpressionAstNode
	{
		AstNode Type;
		ExpressionAstNode ExpressionAstNode;

		public CastExpressionAstNode(AstNode Type, ExpressionAstNode ExpressionAstNode)
		{
			this.Type = Type;
			this.ExpressionAstNode = ExpressionAstNode;
		}

		protected override AstType __GetAstTypeUncached(AstGenerateContext Context)
		{
			//return Type.GetAstType(Context);
			throw(new NotImplementedException());
		}

		public override void Analyze(AstGenerateContext Context)
		{
		}

		public override void GenerateCSharp(AstGenerateContext Context)
		{
			Context.Write("(");
			Context.Write(Type);
			Context.Write(")");
			Context.Write(ExpressionAstNode);
		}

		public override void GenerateIL(AstGenerateContext Context)
		{
			throw new NotImplementedException();
		}
	}
}
