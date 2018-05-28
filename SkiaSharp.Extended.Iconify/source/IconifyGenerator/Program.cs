using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ExCSS;
using Mono.Options;

namespace IconifyGenerator
{
	class Program
	{
		private const string Template =
@"using System.Collections.Generic;

namespace {{inject-namespace}}
{
	public static partial class {{inject-type}}
	{
		public static readonly IReadOnlyDictionary<string, string> Characters;

		static {{inject-type}}()
		{
			Characters = new Dictionary<string, string>
			{
{{inject-characters}}
			};
		}
	}
}
";

		private const string CharacterTemplate =
@"				{ ""{{inject-selector}}"", ""{{inject-value}}"" },";

		private enum CodepointType
		{
			css,
			codepoints
		}

		public static int Main(string[] args)
		{
			// defaults
			string output = null;
			var codepoint = CodepointType.css;
			string injectNamespace = "MyNamespace";
			string injectType = "MyType";
			var showHelp = false;

			// the options
			var p = new OptionSet {
				{ "o|output:", "the output file name.", (string v) => output = v },
				{ "n|namespace=", "the namespace to use.", (string v) => injectNamespace = v },
				{ "t|type=", "the type name to use.", (string v) => injectType = v },
				{ "c|codepoints:", "the type of codepoint file.", (CodepointType v) => codepoint = v },
				{ "h|help",  "show this message and exit", v => showHelp = v != null },
			};

			// read the options
			string stylesheet;
			try
			{
				var extras = p.Parse(args);
				stylesheet = extras.FirstOrDefault();
			}
			catch (Exception ex)
			{
				Console.Write("iconify: ");
				Console.WriteLine(ex.Message);
				Console.WriteLine("Try `iconify --help' for more information.");
				return 1;
			}

			// do help
			if (showHelp)
			{
				ShowHelp(p);
				return 0;
			}

			// make sure the input file is valid
			stylesheet = MakeAbsolute(stylesheet);
			if (!File.Exists(stylesheet))
			{
				Console.WriteLine("The specified codepoint file does not exist: " + stylesheet);
				return 1;
			}

			// get the output file name
			if (string.IsNullOrWhiteSpace(output))
			{
				output = Path.Combine(Path.GetDirectoryName(stylesheet), Path.GetFileNameWithoutExtension(stylesheet) + ".cs");
			}

			// process the template
			var source = ProcessStyleSheet(stylesheet, injectNamespace, injectType, codepoint);

			// write the source to disk
			File.WriteAllText(output, source);

			return 0;
		}

		private static string MakeAbsolute(string path)
		{
			if (Path.DirectorySeparatorChar != '/')
				path = path.Replace('/', Path.DirectorySeparatorChar);
			if (Path.DirectorySeparatorChar != '\\')
				path = path.Replace('\\', Path.DirectorySeparatorChar);
			return Path.GetFullPath(path);
		}

		private static string ProcessStyleSheet(string codepointFile, string injectNamespace, string injectType, CodepointType codepoint)
		{
			var characters = new StringBuilder();

			switch (codepoint)
			{
				case CodepointType.codepoints:
					ParseCodepoints(codepointFile, characters);
					break;
				case CodepointType.css:
				default:
					ParseCssCodepoints(codepointFile, characters);
					break;
			}

			// create the source file
			return Template
				.Replace("{{inject-namespace}}", injectNamespace)
				.Replace("{{inject-type}}", injectType)
				.Replace("{{inject-characters}}", characters.ToString());
		}

		private static void ParseCodepoints(string codepointsPath, StringBuilder characters)
		{
			var lines = File.ReadAllLines(codepointsPath);

			foreach (var line in lines)
			{
				var pair = line.Split(' ');
				if (pair.Length != 2)
				{
					continue;
				}

				var chars = CharacterTemplate
					.Replace("{{inject-selector}}", pair[0])
					.Replace("{{inject-value}}", "\\u" + pair[1]);
				characters.AppendLine(chars);
			}
		}

		private static void ParseCssCodepoints(string cssPath, StringBuilder characters)
		{
			var keys = new Dictionary<string, string>();

			var parser = new Parser();
			var stylesheet = parser.Parse(File.ReadAllText(cssPath));
			var rules = stylesheet.StyleRules;

			foreach (var rule in rules)
			{
				// make sure this one has a "content" property
				var property = rule.Declarations.FirstOrDefault(d => d.Name.Equals("content", StringComparison.OrdinalIgnoreCase));
				if (property == null)
				{
					continue;
				}

				// get the value for this rule
				string content = null;
				var primTerm = property.Term as PrimitiveTerm;
				if (primTerm != null)
				{
					content = primTerm.Value?.ToString();
					content = "\\u" + Char.ConvertToUtf32(content, 0).ToString("x");
				}
				if (string.IsNullOrEmpty(content))
				{
					continue;
				}

				// get all the selectors (keys)
				var selectors = rule.Selector.ToString().Split(',');
				if (selectors == null || selectors.Length == 0)
				{
					continue;
				}

				// create the dictionary
				foreach (var sel in selectors)
				{
					var key = sel.Substring(1, sel.Length - 7 - 1);
					if (string.IsNullOrEmpty(key))
					{
						continue;
					}
					if (keys.ContainsKey(key))
					{
						if (!keys[key].Equals(content, StringComparison.OrdinalIgnoreCase))
						{
							Console.WriteLine($"Duplicate key found: '{key}' with value: '{content}'");
						}
						continue;
					}
					keys.Add(key, content);

					var chars = CharacterTemplate
						.Replace("{{inject-selector}}", key)
						.Replace("{{inject-value}}", content);
					characters.AppendLine(chars);
				}
			}
		}

		private static void ShowHelp(OptionSet p)
		{
			Console.WriteLine("Usage: iconify [OPTIONS]+ stylesheet.css");
			Console.WriteLine("Create a Iconify source file from a stylesheet (CSS) file.");
			Console.WriteLine();
			Console.WriteLine("Options:");
			p.WriteOptionDescriptions(Console.Out);
		}
	}
}
