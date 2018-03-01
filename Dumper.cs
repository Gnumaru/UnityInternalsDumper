using UnityEditor;

public static class _MenuItems_ {
	/// <summary>
	/// Dumps info for all public methods of all public classes from UnityEngineInternal and UnityEditorInternal namespaces
	/// </summary>
	[MenuItem("ETC/dump UnityEngineInternal and UnityEditorInternal info at C:\\t\\<name>.txt")]
	public static void a() {
		var sb = new StringBuilder();
		var a = typeof(UnityEngineInternal.APIUpdaterRuntimeServices).Assembly;
		printTypes(a, "UnityEngineInternal");
		a = typeof(UnityEditorInternal.AssemblyDefinitionAsset).Assembly;
		printTypes(a, "UnityEditorInternal");
	}

	public static void printTypes(Assembly a, string nsPrefix = "") {
		var sb = new StringBuilder();
		sb.AppendLine("assembly name:");
		sb.AppendLine("    "+a.FullName);
		sb.AppendLine("assembly path:");
		sb.AppendLine("    " + Uri.UnescapeDataString(new UriBuilder(a.CodeBase).Path));
		sb.AppendLine();
		sb.AppendLine("assembly types:");
		sb.AppendLine();

		var types = a
			.ExportedTypes
			.Where(t => !string.IsNullOrWhiteSpace(t.Namespace)
					&& t.Namespace.StartsWith(nsPrefix))
			.OrderBy(t => t.FullName);

		foreach(var t in types) appendType(t, sb);

		var sbs = sb.ToString();
		if(!Directory.Exists("C:/t")) Directory.CreateDirectory("C:/t");
		if(string.IsNullOrWhiteSpace(nsPrefix)) nsPrefix = a.GetName().Name;
		File.WriteAllText($"C:/t/{nsPrefix}_types_{DateTime.UtcNow.Ticks}.txt", sbs);
		Debug.Log("truncated version:\n\n" + sbs);
		sb.Clear();
	}

	public static StringBuilder appendType(Type t, StringBuilder sb) {
		appendGenericTypeName(t, sb, true);
		sb.AppendLine();
		var methods = t
			.GetMethods()
			.Where(m =>
				m.Name != "GetType"
				&& m.Name != "GetHashCode"
				&& m.Name != "Equals"
				&& m.Name != "ToString")
			.OrderBy(m => m.Name);

		foreach(var m in methods) appendMethod(m, sb);

		return sb;
	}

	public static StringBuilder appendMethod(MethodInfo m, StringBuilder sb) {
		sb.Append("    ");
		if(m.ReturnType.IsByRef) sb.Append("ref ");
		appendGenericTypeName(m.ReturnType, sb);
		sb.Append(" " + m.Name);
		appendMethodGenericArguments(m, sb);
		sb.Append("(");
		appendMethodParameters(m, sb);
		sb.AppendLine(")");
		return sb;
	}

	public static StringBuilder appendMethodGenericArguments(MethodInfo m, StringBuilder sb) {
		var genArgs = m.GetGenericArguments();
		if(genArgs.FirstOrDefault() != null) {
			sb.Append("<");
			foreach(var ga in genArgs) {
				appendGenericTypeName(ga, sb);
				sb.Append(", ");
			}
			sb.Remove(sb.Length - 2, 2); // remove last comma and space
			sb.Append(">");
		}
		return sb;
	}

	public static StringBuilder appendMethodParameters(MethodInfo m, StringBuilder sb) {
		var parms = m.GetParameters();
		if(parms.FirstOrDefault() != null) {
			foreach(var p in parms) {
				var typename = p.ParameterType.Name;
				if(p.IsOut) sb.Append("out ");
				if(p.ParameterType.IsByRef && !p.IsOut) sb.Append("ref ");
				// there are some instances of "out ref" wich I couldnt find out what means. they are:
				// the second parameter below
				//UnityEditorInternal.VersionControl.AssetModificationHook.IsOpenForEdit("", ?
				// the fifth parameter below
				//UnityEditorInternal.ProfilerDriver.GetStatisticsValues(0, 0, 0, null, ?
				appendGenericTypeName(p.ParameterType, sb);
				sb.Append(" " + p.Name + ", ");
			}
			sb.Remove(sb.Length - 2, 2); // remove last comma and space
		}
		return sb;
	}

	public static StringBuilder appendGenericTypeName(Type t, StringBuilder sb, bool fullname = false) {
		if(sb == null) sb = new StringBuilder();
		if(!fullname) sb.Append(t.Name);
		else sb.Append(t.FullName);
		if(t.IsByRef) sb.Remove(sb.Length - 1, 1); // remove last &
		if(!t.IsGenericType) return sb;
		sb.Remove(sb.Length - 2, 2); // remove last '`X' that generic names contain
		sb.Append("<");
		foreach(var gp in t.GetGenericArguments()) {
			appendGenericTypeName(gp, sb);
			sb.Append(", ");
		}
		sb.Remove(sb.Length - 2, 2);
		sb.Append(">");
		return sb;
	}
}