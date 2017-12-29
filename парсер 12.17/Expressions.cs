using System;
using System.Collections.Generic;

namespace Lab06_Interpreter {
	interface IExpression : INode {
		object CalcWith(IExpressionVisitor v);
	}
	interface IExpressionVisitor {
		object CalcParenthesis(Parenthesis parenthesis);
		object CalcNumber(Number number);
		object CalcIdentifier(Identifier variable);
		object CalcBinary(BinaryOp binary);
		object CalcFunctionCall(FunctionCall func);
	}
	class Parenthesis : IExpression {
		public readonly IExpression Arg;
		public Parenthesis(IExpression arg) {
			Arg = arg;
		}

		public object CalcWith(IExpressionVisitor v) {
			return v.CalcParenthesis(this);
		}
		public override string ToString() {
			return $"({Arg})";
		}
	}
	class Number : IExpression {
		public readonly int Value;
		public Number(int v) {
			Value = v;
		}

		public object CalcWith(IExpressionVisitor v) {
			return v.CalcNumber(this);
		}
		public override string ToString() {
			return Value.ToString();
		}
	}
	class Identifier : IExpression {
		public readonly string Name;
		public Identifier(string name) {
			Name = name;
		}

		public object CalcWith(IExpressionVisitor v) {
			return v.CalcIdentifier(this);
		}
		public override string ToString() {
			return Name;
		}
	}
	enum BinaryOpType {
		Plus,
		Minus,
		Mult,
		Div,
        Rem,
		Greater,
		Less,
		Equal
	}
	class BinaryOp : IExpression {
		public readonly IExpression Left, Right;
		public readonly BinaryOpType Op;

		public BinaryOp(IExpression left, IExpression right, BinaryOpType op) {
			Left = left;
			Right = right;
			Op = op;
		}

		public object CalcWith(IExpressionVisitor v) {
			return v.CalcBinary(this);
		}

		static string OpToString(BinaryOpType op) {
			switch (op) {
				case BinaryOpType.Plus: return "+";
				case BinaryOpType.Minus: return "-";
				case BinaryOpType.Mult: return "*";
				case BinaryOpType.Div: return "/";
				case BinaryOpType.Greater: return ">";
				case BinaryOpType.Less: return "<";
				case BinaryOpType.Equal: return "==";
				default:
					throw new NotImplementedException();
			}
		}
		public override string ToString() {
			return $"{Left} {OpToString(Op)} {Right}";
		}
	}
	class FunctionCall : IExpression {
		public readonly IExpression Callable;
		public readonly IReadOnlyList<IExpression> Args;
		public FunctionCall(IExpression callable, IReadOnlyList<IExpression> args) {
			Callable = callable;
			Args = args;
		}
		public object CalcWith(IExpressionVisitor v) {
			return v.CalcFunctionCall(this);
		}
		public override string ToString() {
			return $"{Callable}({string.Join(", ", Args)})";
		}
	}
}
