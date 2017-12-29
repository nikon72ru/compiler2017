using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using парсер_12._17;

namespace Lab06_Interpreter {
	class Program {
		static ProgramNode Parse(string program) {
			var tokens = Token.Tokenize(program).Where(t =>!(
            t.Type == TokenType.WhiteSpace ||
            t.Type == TokenType.SinglelineComment ||
            t.Type == TokenType.MultilineComment)
            ).ToArray();
			var parser = new Parser(tokens);
			var programTree = parser.Parse();
			return programTree;
		}
		static void Run(string program, Dictionary<string, object> vars = null) {
			var programTree = Parse(program);
			var s1 = programTree.ToString();
			var t1 = Parse(s1);
			var s2 = t1.ToString();
			if (s1 != s2) {
				Console.WriteLine(s1);
				Console.WriteLine(s2);
				throw new Exception("ToString у узлов и/или парсер работают неправильно");
			}
			Console.WriteLine(programTree.ToString());
			Interpreter v = new Interpreter(vars);
			v.Run(programTree);
		}

		static void Main(string[] args) {
            var programNode = Parse(File.ReadAllText(@"\\psf\Home\Documents\Visual Studio 2015\Projects\ПОПК\парсер 12.17\парсер 12.17\TextFile1.txt"));
            var module = Mono.Cecil.ModuleDefinition.CreateModule("gg", Mono.Cecil.ModuleKind.Console);



            var program = new TypeDefinition(
              "", "Program",
              TypeAttributes.Public |
              TypeAttributes.Abstract |
              TypeAttributes.Sealed |
              TypeAttributes.BeforeFieldInit,
              module.TypeSystem.Object
          );
            module.Types.Add(program);

            var main = new MethodDefinition(
                "Main",
                MethodAttributes.Public |
                MethodAttributes.Static |
                MethodAttributes.HideBySig,
                module.TypeSystem.Void
            );
            program.Methods.Add(main);
            var c = new VariableDefinition(
                  "c", module.TypeSystem.Int32
                  );
            main.Body.Variables.Add(c);
            module.EntryPoint = main;
            new Compiler(main).Compile(programNode);
            main.Body.GetILProcessor().Emit(OpCodes.Ret);
            module.Write("../../out.exe");
            System.Reflection.Assembly.LoadFrom("../../out.exe").GetType("Program").GetMethod("Main").Invoke(null, new object[] { });



        }
    }
}
