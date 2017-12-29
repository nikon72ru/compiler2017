using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab06_Interpreter {
	class Interpreter : IExpressionVisitor, IStatementVisitor {
		Dictionary<string, object> vars;
		int countWhileStarted = 0;
		bool breakFlag = false;
		public Interpreter(Dictionary<string, object> v = null) {
			if (v == null) {
				vars = new Dictionary<string, object>();
			}
			else {
				vars = new Dictionary<string, object>(v);
			}
			Action<string, Func<object>> add = (name, getValue) => {
				if (!vars.ContainsKey(name)) {
					vars.Add(name, getValue());
				}
			};
			add("true", () => true);
			add("false", () => false);
			add("null", () => null);
			add("print", () => new PrintFunction());
		}
		public void SetVariable(string name, object val) {
			vars[name] = val;
		}
		public object GetVariable(string name) {
			object var;
			if (!vars.TryGetValue(name, out var))
				throw new Exception($"Переменная {name} не объявлена");
			return var;
		}
		public void Run(ProgramNode program) {
			foreach (var stat in program.Statements)
				stat.RunWith(this);
		}
		bool CalcBool(IExpression condition) {
			var cond = condition.CalcWith(this);
			if (cond is bool)
				return (bool)cond;
			else
				throw new Exception("Недопустимое значение для условия");
		}
		void RunStatements(IReadOnlyList<IStatement> statements) {
			foreach (var stat in statements) {
				stat.RunWith(this);
				if (breakFlag)
					break;
			}
		}
		#region StatementVisits
		void IStatementVisitor.RunAssignment(Assignment assignment) {
			SetVariable(assignment.VarName, assignment.Expr.CalcWith(this));
		}
		void IStatementVisitor.RunBreak(Break @break) {
			if (countWhileStarted <= 0)
				throw new Exception("break можно делать только в теле цикла");
			breakFlag = true;
		}

		void IStatementVisitor.RunExpressionStatement(ExpressionStatement exprStat) {
			exprStat.Expr.CalcWith(this);
		}

		void IStatementVisitor.RunIf(If @if) {
			if (CalcBool(@if.Condition)) {
				RunStatements(@if.Statements);
			}
		}

		void IStatementVisitor.RunWhile(While @while) {
			countWhileStarted += 1;
			while (CalcBool(@while.Condition)) {
				RunStatements(@while.Statements);
				if (breakFlag)
					break;
			}
			breakFlag = false;
			countWhileStarted -= 1;
		}
		#endregion
		#region ExprVisits
		object IExpressionVisitor.CalcBinary(BinaryOp binary) {
			var lobj = binary.Left.CalcWith(this);
			var robj = binary.Right.CalcWith(this);
			if (lobj is int && robj is int) {
				var a = (int)lobj;
				var b = (int)robj;
				switch (binary.Op) {
					case BinaryOpType.Plus:
						return a + b;
					case BinaryOpType.Minus:
						return a - b;
					case BinaryOpType.Mult:
						return a * b;
					case BinaryOpType.Div:
						return a / b;
					case BinaryOpType.Greater:
						return a > b;
					case BinaryOpType.Less:
						return a < b;
					case BinaryOpType.Equal:
						return a == b;
					default:
						throw new Exception("Неизвестный оператор для int");
				}
			}
			else if (lobj is bool && robj is bool) {
				var a = (bool)lobj;
				var b = (bool)robj;
				switch (binary.Op) {
					case BinaryOpType.Greater:
						return a && !b;
					case BinaryOpType.Less:
						return !a && b;
					case BinaryOpType.Equal:
						return a == b;
					default:
						throw new Exception("Неизвестный оператор для bool");
				}
			}
			else {
				switch (binary.Op) {
					case BinaryOpType.Equal:
						return false;
					default:
						throw new Exception("Некорректные значения");
				}
			}
		}

		object IExpressionVisitor.CalcNumber(Number number) {
			return number.Value;
		}

		object IExpressionVisitor.CalcParenthesis(Parenthesis parenthesis) {
			return parenthesis.Arg.CalcWith(this);
		}

		object IExpressionVisitor.CalcIdentifier(Identifier variable) {
			return GetVariable(variable.Name);
		}

		object IExpressionVisitor.CalcFunctionCall(FunctionCall func) {
			var v = func.Callable.CalcWith(this);
			var callable = v as ICallable;
			if (callable == null) {
				throw new Exception($"нельзя вызвать {ValueUtils.ValueToString(v)} как функцию");
			}
			var args = func.Args.Select(a => a.CalcWith(this)).ToArray();
			return callable.Call(args);
		}
		#endregion
	}
}
