using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Lab06_Interpreter {
	public enum TokenType {
		WhiteSpace,
		Identifier,
		Number,
		Punctuation,
		MultilineComment,
		SinglelineComment,
		Error_IncorrectInputString,
		Error_Skip,
		Error_IncorrectProccesingRegEx,
		EOF
	};
	public class Token {
		public TokenType Type;
		public string Lexeme;
		public Token(TokenType type, string lexeme) {
			Type = type;
			Lexeme = lexeme;
		}
		public override string ToString() {
			return string.Format("{0} \"{1}\"", Enum.GetName(Type.GetType(), Type), Lexeme);
		}
		public static IEnumerable<Token> Tokenize(string s) {
			var whitespaces = @"(?<ws>[\s\r]+)";
			var idents = @"(?<id>[A-Za-z_][A-Za-z_0-9]*)";
			var numbers = @"(?<num>[0-9]+)";
			var mlcomment = @"(?<mlcom>/\*[\s\S\r]*?\*/)";
			var slcomment = @"(?<slcom>//[^\n\r]*)";
			var puncs = @"(?<punc>==|\+\+|[\.,;\(\)\{\}\<\>\[\]=\+\-!\?%\/\*&\|])";
			var lexeme = string.Join("|", new string[] {
				whitespaces, idents, numbers, mlcomment, slcomment, puncs
			});
			var rx = new Regex(lexeme, RegexOptions.Compiled | RegexOptions.ExplicitCapture);
			var tokens = new List<Token>();
			int pos = 0;
			while (pos < s.Length) {
				var m = rx.Match(s, pos);
				if (!m.Success) {
					yield return (new Token(TokenType.Error_IncorrectInputString, s.Substring(pos)));
					++pos;
					continue;
				}
				if (m.Index > pos) {
					yield return (new Token(TokenType.Error_Skip, s.Substring(pos, m.Index - pos)));
				}
				pos = m.Index + m.Length;
				var g = m.Groups;
				if (g["ws"].Success) {
					yield return (new Token(TokenType.WhiteSpace, m.Value));
				}
				else if (g["id"].Success) {
					yield return (new Token(TokenType.Identifier, m.Value));
				}
				else if (g["num"].Success) {
					yield return (new Token(TokenType.Number, m.Value));
				}
				else if (g["punc"].Success) {
					yield return (new Token(TokenType.Punctuation, m.Value));
				}
				else if (g["mlcom"].Success) {
					yield return (new Token(TokenType.MultilineComment, m.Value));
				}
				else if (g["slcom"].Success) {
					yield return (new Token(TokenType.SinglelineComment, m.Value));
				}
				else {
					yield return (new Token(TokenType.Error_IncorrectProccesingRegEx, m.Value));
				}
			}
		}
	}
}
