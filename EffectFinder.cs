using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HelpfullWards
{
	/// <summary>
	/// Lists ZNetScene prefabs whose name matches a keyword and writes a summary
	/// of their components and child hierarchy to a file. Useful for discovering
	/// reusable visual / sound effects bundled in Valheim.
	/// </summary>
	public static class EffectFinder
	{
		private const string DefaultOutputDir = "/home/michel/Documents/http/public/hierarchies";

		public static void Dump(string keyword, string fileName)
		{
			if (ZNetScene.instance == null)
			{
				Plugin.Logger.LogWarning("[EffectFinder] ZNetScene not ready, abort.");
				return;
			}

			string path = Path.IsPathRooted(fileName)
				? fileName
				: Path.Combine(DefaultOutputDir, fileName);

			var matches = new List<(GameObject prefab, bool networked)>();
			foreach (var p in ZNetScene.instance.m_prefabs)
				if (Matches(p, keyword)) matches.Add((p, true));
			foreach (var p in ZNetScene.instance.m_nonNetViewPrefabs)
				if (Matches(p, keyword)) matches.Add((p, false));

			matches = matches.OrderBy(m => m.prefab.name).ToList();

			var sb = new StringBuilder();
			sb.AppendLine($"# Prefabs matching \"{keyword}\" — {matches.Count} hit(s)");
			sb.AppendLine();

			foreach (var (prefab, networked) in matches)
			{
				string tag = networked ? "[Net]" : "[NonNet]";
				sb.Append(tag).Append(' ').Append(prefab.name).Append(' ');
				sb.AppendLine(SummarizeComponents(prefab));
				DumpChildren(sb, prefab.transform, depth: 1);
				sb.AppendLine();
			}

			Directory.CreateDirectory(Path.GetDirectoryName(path)!);
			File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
			Plugin.Logger.LogInfo($"[EffectFinder] Wrote {matches.Count} prefab(s) to: {path}");
		}

		private static bool Matches(GameObject? prefab, string keyword)
			=> prefab != null
			   && prefab.name.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0;

		private static void DumpChildren(StringBuilder sb, Transform parent, int depth)
		{
			for (int i = 0; i < parent.childCount; i++)
			{
				var child = parent.GetChild(i);
				sb.Append(new string(' ', depth * 2));
				sb.Append("└ ");
				sb.Append(child.name);
				sb.Append(' ');
				sb.AppendLine(SummarizeComponents(child.gameObject));
				DumpChildren(sb, child, depth + 1);
			}
		}

		private static string SummarizeComponents(GameObject go)
		{
			var parts = new List<string>();

			AppendCount<ParticleSystem>(go, "ParticleSystem", parts);
			AppendCount<Light>(go, "Light", parts);
			AppendCount<AudioSource>(go, "AudioSource", parts);
			AppendCount<ZSFX>(go, "ZSFX", parts);
			AppendCount<TrailRenderer>(go, "TrailRenderer", parts);
			AppendCount<LineRenderer>(go, "LineRenderer", parts);
			AppendCount<MeshRenderer>(go, "MeshRenderer", parts);
			AppendCount<SkinnedMeshRenderer>(go, "SkinnedMeshRenderer", parts);

			if (go.GetComponent<ZNetView>()         != null) parts.Add("ZNetView");
			if (go.GetComponent<Aoe>()              != null) parts.Add("Aoe");
			if (go.GetComponent<TimedDestruction>() != null) parts.Add("TimedDestruction");

			var se = go.GetComponent<StatusEffect>();
			if (se != null) parts.Add($"SE:{se.GetType().Name}");

			return parts.Count == 0 ? "" : "[" + string.Join(", ", parts) + "]";
		}

		private static void AppendCount<T>(GameObject go, string label, List<string> parts) where T : Component
		{
			int n = go.GetComponents<T>().Length;
			if (n == 0) return;
			parts.Add(n == 1 ? label : $"{label}×{n}");
		}
	}
}
