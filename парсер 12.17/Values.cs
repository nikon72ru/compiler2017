using System;
using System.Collections.Generic;
using System.Globalization;

namespace Lab06_Interpreter {
	static class ValueUtils {
		public static string ValueToString(object value) {
			if (value == null) {
				return "null";
			}
			if (value is int) {
				return ((int)value).ToString(CultureInfo.InvariantCulture);
			}
			if (value is bool) {
				return (bool)value ? "true" : "false";
			}
			if (value is ICallable) {
				return value.ToString();
			}
			throw new NotSupportedException($"неподдерживаемый тип значения {value.GetType()}");
		}
	}
	interface ICallable {
		object Call(IReadOnlyList<object> args);
	}
	class PrintFunction : ICallable {
		public object Call(IReadOnlyList<object> args) {
			var first = true;
			foreach (object obj in args) {
				if (!first) {
					Console.Write(" ");
				}
				first = false;
				Console.Write(ValueUtils.ValueToString(obj));
			}
			Console.WriteLine();
			return null;
		}
		public override string ToString() {
			return "print";
		}
	}
}
