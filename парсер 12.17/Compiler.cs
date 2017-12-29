using Lab06_Interpreter;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace парсер_12._17
{
    class Compiler
    {
        readonly ModuleDefinition module;
        readonly MethodDefinition method;
        readonly ILProcessor asm;
        readonly TypeReference Int;
        readonly TypeReference Uint;
        readonly TypeReference Void;
        readonly TypeReference Bool;
        public Compiler(MethodDefinition method)
        {
            module = method.Module;
            this.method = method;
            asm = method.Body.GetILProcessor();
            Int = module.TypeSystem.Int32;
            Void = module.TypeSystem.Void;
            Bool = module.TypeSystem.Boolean;
            Uint = module.TypeSystem.UInt32;
        }
        public void Compile(ProgramNode programNode)
        {
            foreach (var st in programNode.Statements)
            {
                CompileStatement(st);
            }
        }

        private void CompileStatement(IStatement st)
        {
            if (st is ExpressionStatement)
            {
                CompileEs((ExpressionStatement)st);
            }
            else if (st is Assignment)
            {
                CompileAss((Assignment)st);
            }
            else if (st is If)
            {
                CompileIf((If)st);
            }
            else if (st is While)
            {
                CompileWhile((While)st);
            }
            else
            {
                throw new Exception("wrong statement");
            }
        }

        private void CompileWhile(While st)
        {
            var @iter = Instruction.Create(OpCodes.Nop);
            var afterIf = Instruction.Create(OpCodes.Nop);
            var @else = Instruction.Create(OpCodes.Nop);
            asm.Append(@iter);
            var condition = CompileExpr(st.Condition);
            asm.Emit(OpCodes.Ldc_I4, 1);
            asm.Emit(OpCodes.Ceq);
            asm.Emit(OpCodes.Brfalse, @else);
            {
                foreach (var stats in st.Statements)
                {
                    CompileStatement(stats);
                }
                asm.Emit(OpCodes.Br, @iter);
            }
            asm.Append(@else);
            asm.Append(afterIf);

        }

        private void CompileIf(If st)
        {
            var condition = CompileExpr(st.Condition);
            asm.Emit(OpCodes.Ldc_I4, 1);
            asm.Emit(OpCodes.Ceq);
            //   var afterIf = Instruction.Create(OpCodes.Nop);
            var @else = Instruction.Create(OpCodes.Nop);
            asm.Emit(OpCodes.Brfalse, @else);
            {
                foreach (var stats in st.Statements)
                {
                    CompileStatement(stats);
                }
                //    asm.Emit(OpCodes.Br, afterIf);
            }
            asm.Append(@else);
            // asm.Append(afterIf);

        }

        private void CompileAss(Assignment st)
        {
            var rightType = CompileExpr(st.Expr);
            var var = method.Body.Variables.SingleOrDefault(v => v.Name == st.VarName);
            if (var == null)
            {
                //throw new Exception("Надо добавить переменную, проверить что rightTy[e != Void");
                if (rightType == Void)
                {
                    throw new Exception("НЕльзя создать переенную типа Void");
                }
                var = new VariableDefinition(st.VarName, rightType);
                method.Body.Variables.Add(var);
            }
            else if (var.VariableType != rightType)
            {
                throw new Exception($"Нельзя присвоить {rightType} в {var.Name}  типа {var.VariableType}");
            }
            asm.Emit(OpCodes.Stloc, var);
        }

        void CompileEs(ExpressionStatement st)
        {
            var type = CompileExpr(st.Expr);
            if (type != Void)
            {
                asm.Emit(OpCodes.Pop);
            }
            //   throw new Exception();
        }

        TypeReference CompileExpr(IExpression expr)
        {
            if (expr is FunctionCall)
            {
                return CompileFc((FunctionCall)expr);
            }
            else if (expr is Number)
            {
                return CompileNumb((Number)expr);
            }
            else if (expr is BinaryOp)
            {
                return CompileBinaryOp((BinaryOp)expr);
            }
            else if (expr is Identifier)
            {
                return CompileIdent((Identifier)expr);
            }
            else if (expr is Parenthesis)
            {
                return CompileExpr(((Parenthesis)expr).Arg);
            }
            else
            {
                throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

        private TypeReference CompileIdent(Identifier expr)
        {
            //добавить инструкцию на булеаны
            var var = method.Body.Variables.SingleOrDefault(v => v.Name == expr.Name);
            if (var == null)
            {
                if (expr.Name == "true")
                {
                    asm.Emit(OpCodes.Ldc_I4, 1);
                    return Bool;
                }
                if (expr.Name == "false")
                {
                    asm.Emit(OpCodes.Ldc_I4, 0);
                    return Bool;
                }
                throw new Exception($"Переменная {expr.Name} не найдена!");
            }
            asm.Emit(OpCodes.Ldloc, var);
            return var.VariableType;
        }

        private TypeReference CompileBinaryOp(BinaryOp expr)
        {
            var leftType = CompileExpr(expr.Left);
            if (expr.Op == BinaryOpType.Less && leftType == Bool)
            {
                asm.Emit(OpCodes.Not);
            }
            var rightType = CompileExpr(expr.Right);
            if (leftType != rightType)
            {
                throw new Exception("Разные типы переменных");
            }
            if (leftType == Void)
            {
                throw new Exception("Нельзя Void");
            }
            //HEAD
            switch (expr.Op)
            {
                case BinaryOpType.Plus:
                    {
                        if (leftType == Int)
                        {
                            asm.Emit(OpCodes.Add_Ovf);
                            return Int;
                        }
                        if (leftType == Uint)
                        {
                            asm.Emit(OpCodes.Add_Ovf_Un);
                            return Uint;
                        }
                        break;
                    }
                case BinaryOpType.Minus:
                    {
                        if (leftType == Int)
                        {
                            asm.Emit(OpCodes.Sub_Ovf);
                            return Int;
                        }
                        if (leftType == Uint)
                        {
                            asm.Emit(OpCodes.Sub_Ovf_Un);
                            return Uint;
                        }
                        break;
                    }
                case BinaryOpType.Mult:
                    {
                        if (leftType == Int)
                        {
                            asm.Emit(OpCodes.Mul_Ovf);
                            return Int;
                        }
                        if (leftType == Uint)
                        {
                            asm.Emit(OpCodes.Mul_Ovf_Un);
                            return Uint;
                        }
                        break;
                    }
                case BinaryOpType.Div:
                    {
                        if (leftType == Int)
                        {
                            asm.Emit(OpCodes.Div);
                            return Int;
                        }
                        if (leftType == Uint)
                        {
                            asm.Emit(OpCodes.Div_Un);
                            return Uint;
                        }
                        break;
                    }
                case BinaryOpType.Rem:
                    {
                        if (leftType == Int)
                        {
                            asm.Emit(OpCodes.Rem);
                            return Int;
                        }
                        if (leftType == Uint)
                        {
                            asm.Emit(OpCodes.Rem_Un);
                            return Uint;
                        }
                        break;
                    }
                case BinaryOpType.Equal:
                    {
                        asm.Emit(OpCodes.Ceq);
                        return Bool;
                    }
                case BinaryOpType.Less:
                    {
                        if (leftType == Int)
                        {
                            asm.Emit(OpCodes.Clt);
                            return Bool;
                        }
                        if (leftType == Uint)
                        {
                            asm.Emit(OpCodes.Clt_Un);
                            return Bool;
                        }
                        if (leftType == Bool)
                        {
                            asm.Emit(OpCodes.And);

                            return Bool;
                        }
                        break;
                    }
            }
            throw new NotImplementedException();

        }

        private TypeReference CompileNumb(Number expr)
        {
            asm.Emit(OpCodes.Ldc_I4, expr.Value);
            return Int;
        }

        TypeReference CompileFc(FunctionCall expr)
        {
            var id = expr.Callable as Identifier;
            if (id == null)
            {
                throw new Exception("Вызывать можно только int, uint, print или bool");
            }
            if (expr.Args.Count != 1)
            {
                throw new Exception("Мoжно только 1 аргумент");
            }
            var arg = expr.Args[0];
            var argType = CompileExpr(arg);
            switch (id.Name)
            {
                case "print":
                    {
                        if (argType == Int)
                        {
                            asm.Emit(OpCodes.Call, module.Import(typeof(Console).GetMethod("WriteLine", new[] { typeof(int) })));
                            return Void;
                        }
                        else if (argType == Bool)
                        {
                            asm.Emit(OpCodes.Call, module.Import(typeof(Console).GetMethod("WriteLine", new[] { typeof(bool) })));
                            return Void;
                        }
                        else if (argType == Uint)
                        {
                            asm.Emit(OpCodes.Call, module.Import(typeof(Console).GetMethod("WriteLine", new[] { typeof(uint) })));
                            return Void;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                case "int":
                    {
                        if (argType == Int)
                        {
                            return Int;
                        }
                        else
                        if (argType == Uint)
                        {
                            asm.Emit(OpCodes.Conv_Ovf_I4_Un);
                        }
                        else if (argType == Bool)
                        {
                            return Int;
                        }
                        else
                        {
                            throw new Exception();
                        }
                        return Int;
                    }
                case "bool":
                    {
                        if (argType == Bool)
                        {
                            return Bool;
                        }
                        if (argType == Int)
                        {
                            return Bool;
                        }
                        if (argType == Uint)
                        {
                            return Bool;
                        }
                    }
                    break;
                case "uint":
                    {
                        if (argType == Int)
                        {
                            asm.Emit(OpCodes.Conv_Ovf_U4);
                            return Uint;
                        }
                        if (argType == Bool)
                        {
                            return Uint;
                        }
                    }
                    break;
            }
            throw new NotImplementedException();
        }
    }
}
