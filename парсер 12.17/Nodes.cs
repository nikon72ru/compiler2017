using System.Collections.Generic;

namespace Lab06_Interpreter {
	interface INode { }

	class ProgramNode : INode {
		public readonly List<IStatement> Statements;
		public ProgramNode(List<IStatement> stats) {
			Statements = stats;
		}
		public override string ToString() {
			return string.Join(" ", Statements);
		}
	}
}
