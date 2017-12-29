using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lab06_Interpreter {
	/*
			   *Program:   Statements
			   Statements: (Statements Statement)?
			   Statement:  ExpressionStatement: Expression ';'
						   Assignment: Identifier '=' Expression ';'
						   If: 'if' '(' Expression ')' '{' Statements '}'
						   While: 'while' '(' Expression ')' '{' Statements '}'
						   Break: 'break' ';'
			   Expression: Equality
			   Equality:   Equality '==' Compare
						   Compare
			   Compare:    Compare ('<'|'>') Sum
						   Sum
			   Sum:        Sum ('+'|'-') Product
						   Product
			   Product:    Product ('*'|'/') Primary
						   Primary
			   Primary:    PrimaryPrefix ( '(' Params? ')' )*
		 PrimaryPrefix:    '(' Expression ')'
						   number
						   identifier                            
			   Params:     Params ',' Expression | Expression   

		*/
	class Parser {
		int p;
		List<Token> tokens;
		Token token;
		public Parser(IEnumerable<Token> tokens) {
			var tt = tokens.ToList();
			Debug.Assert(tt.Count(t => t.Type == TokenType.EOF) == 0);
			tt.Add(new Token(TokenType.EOF, ""));
			this.tokens = tt;
		}
		public ProgramNode Parse() {
			p = -1;
			Skip();
			var res = ParseProgram();
			if (token.Type != TokenType.EOF)
				throw new Exception("Не допарсили до конца");
			return res;
		}
		void Skip() {
			token = tokens[++p];
		}
		bool SkipIf(string s) {
			if (token.Lexeme == s) {
				Skip();
				return true;
			}
			return false;
		}
		void Expect(string s) {
			if (!SkipIf(s)) {
				throw new Exception($"Ожидали {s}");
			}
		}
		ProgramNode ParseProgram() {
			var statements = new List<IStatement>();
			while (token.Type != TokenType.EOF) {
				statements.Add(ParseStatement());
			}
			return new ProgramNode(statements);
		}
		List<IStatement> ParseStatementsBlock() {
			var statements = new List<IStatement>();
			Expect("{");
			while (!SkipIf("}")) {
				statements.Add(ParseStatement());
			}
			return statements;
		}

		IStatement ParseStatement() {
			if (token.Type == TokenType.Identifier) {
				switch (token.Lexeme) {
					case "if":
						return ParseIf();
					case "while":
						return ParseWhile();
					case "break":
						return ParseBreak();
				}
			}
			var expr = ParseExpression();
			if (SkipIf("=")) {
				var var = expr as Identifier;
				if (var == null) {
					throw new Exception("Ожидали присваивание в идентификатор");
				}
				var value = ParseExpression();
				Expect(";");
				return new Assignment(var.Name, value);
			}
			Expect(";");
			return new ExpressionStatement(expr);
		}

		Break ParseBreak() {
			Expect("break");
			Expect(";");
			return new Break();
		}

		While ParseWhile() {
			Expect("while");
			Expect("(");
			var expr = ParseExpression();
			Expect(")");
			return new While(expr, ParseStatementsBlock());
		}

		If ParseIf() {
			Expect("if");
			Expect("(");
			var expr = ParseExpression();
			Expect(")");
			return new If(expr, ParseStatementsBlock());
		}

		IExpression ParseExpression() {
			return ParseEquality();
		}
		IExpression ParseEquality() {
			var left = ParseCompare();
			while (true) {
				if (SkipIf("==")) {
					left = new BinaryOp(left, ParseCompare(), BinaryOpType.Equal);
				}
				else
					return left;
			}
		}
		IExpression ParseCompare() {
			var left = ParseSum();
			while (true) {
				if (SkipIf(">")) {
					left = new BinaryOp(left, ParseSum(), BinaryOpType.Greater);
				}
				else if (SkipIf("<")) {
					left = new BinaryOp(left, ParseSum(), BinaryOpType.Less);
				}
				else
					return left;
			}
		}
		IExpression ParseSum() {
			var left = ParseProduct();
			while (true) {
				if (SkipIf("+")) {
					left = new BinaryOp(left, ParseProduct(), BinaryOpType.Plus);
				}
				else if (SkipIf("-")) {
					left = new BinaryOp(left, ParseProduct(), BinaryOpType.Minus);
				}
				else
					return left;
			}
		}
		IExpression ParseProduct() {
			var left = ParsePrimary();
			while (true) {
				if (SkipIf("*")) {
					left = new BinaryOp(left, ParsePrimary(), BinaryOpType.Mult);
				}
				else if (SkipIf("/")) {
					left = new BinaryOp(left, ParsePrimary(), BinaryOpType.Div);
				}
                else if (SkipIf("%"))
                {
                    left = new BinaryOp(left, ParsePrimary(), BinaryOpType.Rem);
                }
                else
					return left;
			}
		}

		IExpression ParsePrimary() {
			IExpression prefix = ParsePrimaryPrefix();
			while (true) {
				if (SkipIf("(")) {
					var args = new List<IExpression>();
					if (!SkipIf(")")) {
						args.Add(ParseExpression());
						while (SkipIf(",")) {
							args.Add(ParseExpression());
						}
						if (!SkipIf(")"))
							throw new Exception("Ожидали закрывающую скобку");
					}
					prefix = new FunctionCall(prefix, args.ToArray());
				}
				else {
					return prefix;
				}
			}
		}

		IExpression ParsePrimaryPrefix() {
			switch (token.Type) {
				case TokenType.Number:
					return ParseNumber();
				case TokenType.Identifier:
					return ParseIdentifier();
				default:
					if (!SkipIf("(")) {
						throw new Exception("Ожидали число, идентификатор или открывающую скобку");
					}
					var sum = ParseEquality();
					if (!SkipIf(")"))
						throw new Exception("Ожидали закрывающую скобку");
					return new Parenthesis(sum);
			}
		}
		Identifier ParseIdentifier() {
			if (token.Type != TokenType.Identifier)
				throw new Exception("Ожидали идентификатор");
			var var = new Identifier(token.Lexeme);
			Skip();
			return var;
		}
		Number ParseNumber() {
			if (token.Type != TokenType.Number)
				throw new Exception("Ожидали число");
			var number = new Number(int.Parse(token.Lexeme));
			Skip();
			return number;
		}
	}
}
