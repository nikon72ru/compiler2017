using System.Collections.Generic;
using System.Linq;

namespace Lab06_Interpreter {
	interface IStatement : INode {
		void RunWith(IStatementVisitor v);
	}
	interface IStatementVisitor {
		void RunWhile(While @while);
		void RunIf(If @if);
		void RunAssignment(Assignment assignment);
		void RunBreak(Break @break);
		void RunExpressionStatement(ExpressionStatement exprStat);
	}
	class Assignment : IStatement {
		public readonly string VarName;
		public readonly IExpression Expr;

        public Assignment(string var, IExpression expr) {
			VarName = var;
			Expr = expr;
		}

        public void RunWith(IStatementVisitor v) {
			v.RunAssignment(this);
		}
		public override string ToString() {
			return $"{VarName} = {Expr.ToString()};";
		}
	}
	class If : IStatement {
		public readonly IExpression Condition;
		public readonly IReadOnlyList<IStatement> Statements;
		public If(IExpression cond, IReadOnlyList<IStatement> stats) {
			Condition = cond;
			Statements = stats;

		}

		public void RunWith(IStatementVisitor v) {
			v.RunIf(this);
		}
		public override string ToString() {
			return $"if ({Condition.ToString()}) {{ {string.Join(" ", Statements.Select(st => st.ToString()))} }}";
		}
	}
	class While : IStatement {
		public readonly IExpression Condition;
		public readonly IReadOnlyList<IStatement> Statements;
		public While(IExpression cond, IReadOnlyList<IStatement> stats) {
			Condition = cond;
			Statements = stats;
		}

		public void RunWith(IStatementVisitor v) {
			v.RunWhile(this);
		}
		public override string ToString() {
			return $"while ({Condition}) {{ {string.Join(" ", Statements)} }}";
		}
	}
	class Break : IStatement {
		public void RunWith(IStatementVisitor v) {
			v.RunBreak(this);
		}
		public override string ToString() {
			return "break;";
		}
	}
	class ExpressionStatement : IStatement {
		public readonly IExpression Expr;
		public ExpressionStatement(IExpression expr) {
			Expr = expr;
		}

		public void RunWith(IStatementVisitor v) {
			v.RunExpressionStatement(this);
		}
		public override string ToString() {
			return $"{Expr};";
		}
	}
}
