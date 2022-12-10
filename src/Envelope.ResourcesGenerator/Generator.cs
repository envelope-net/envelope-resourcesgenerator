using Envelope.Generator;
using Envelope.Localization;
using System.Globalization;
using System.Reflection;

namespace Envelope.ResourcesGenerator;

public class Generator
{
	public static void UpdateResource(
		IEnumerable<string> keys,
		IEnumerable<string> keyFormatters,
		string resxFilePath)
	{
		var keysList = keys?.ToList() ?? throw new ArgumentNullException(nameof(keys));
		var keyFormattersList = keyFormatters?.ToList() ?? throw new ArgumentNullException(nameof(keyFormatters));

		for (int i = 0; i < keysList.Count - 1; i++)
		{
			for (int j = i + 1; j < keysList.Count; j++)
			{
				if (string.IsNullOrWhiteSpace(keysList[i]))
					throw new InvalidOperationException($"Empty key at index = {i}. {keysList[i]}");
				if (string.IsNullOrWhiteSpace(keysList[j]))
					throw new InvalidOperationException($"Empty key at index = {j}. {keysList[j]}");

				if (keysList[i].Equals(keysList[j], StringComparison.OrdinalIgnoreCase))
					throw new InvalidOperationException($"Not UNIQUE key {keysList[i]} on {i} and {j}");
			}
		}

		var resxBuilder = new ResxBuilder(resxFilePath);
		var dataList = new List<ResxData>();

		foreach (var key in keysList)
		{
			if (keyFormattersList == null || keyFormattersList.Count == 0)
			{
				var existing = resxBuilder.Data.FirstOrDefault(data => data.Name == key);
				if (existing == null)
					dataList.Add(new ResxData(key, key));
				else
					dataList.Add(existing);
			}
			else
			{
				foreach (var keyFormatter in keyFormattersList)
				{
					string resxKey = string.Format(keyFormatter, key);
					var existing = resxBuilder.Data.FirstOrDefault(data => data.Name == resxKey);
					if (existing == null)
						dataList.Add(new ResxData(resxKey, key));
					else
						dataList.Add(existing);
				}
			}
		}

		resxBuilder.Clear();
		foreach (var data in dataList)
			resxBuilder.Add(data);

		resxBuilder.Serialize();
	}

	public static void GenerateResourceData(
		string targetProject,
		string rootNamespace,
		Assembly assembly,
		CultureInfo defaultCulture,
		string resourcesClassName,
		List<CultureInfo>? checkForCultures = null,
		bool compareResourceKyes = false)
	{
		if (string.IsNullOrWhiteSpace(targetProject))
			throw new ArgumentNullException(nameof(targetProject));

		if (!targetProject.EndsWith("\\"))
		{
			targetProject += "\\";
		}

		var resFiles = ResourceLoader.LoadResources(targetProject, assembly, defaultCulture, ResourceLoadOptions.LoadResxAllResources, SearchOption.AllDirectories, checkForCultures, compareResourceKyes);

		var resourcesGenerator = new ResourceDataGenerator
		{
			WriteMode = GeneratorBase.WriteModes.Overwritte
		};
		resourcesGenerator.SetParam("TargetProject", targetProject);
		resourcesGenerator.SetParam("RootNamespace", rootNamespace);
		resourcesGenerator.SetParam("ResFiles", resFiles);
		resourcesGenerator.SetParam("ResourcesClassName", resourcesClassName);
		resourcesGenerator.TransformText();
		var errors = resourcesGenerator.ErrorString();
		if (!string.IsNullOrWhiteSpace(errors))
			throw new Exception(errors);
	}

	public static void GenerateResources(
		string targetProject,
		string rootNamespace,
		bool generateOnlyKeys,
		Assembly assembly,
		CultureInfo? defaultCulture,
		List<CultureInfo>? checkForCultures = null,
		bool compareResourceKyes = false)
	{
		if (string.IsNullOrWhiteSpace(targetProject))
			throw new ArgumentNullException(nameof(targetProject));

		if (!targetProject.EndsWith("\\"))
		{
			targetProject += "\\";
		}

		var resFiles = ResourceLoader.LoadResources(targetProject, assembly, defaultCulture, ResourceLoadOptions.LoadResxAllResources, SearchOption.AllDirectories, checkForCultures, compareResourceKyes);

		var resourcesGenerator = new ResourcesGenerator
		{
			WriteMode = GeneratorBase.WriteModes.Overwritte
		};
		resourcesGenerator.SetParam("TargetProject", targetProject);
		resourcesGenerator.SetParam("RootNamespace", rootNamespace);
		resourcesGenerator.SetParam("ResFiles", resFiles);
		resourcesGenerator.SetParam("OnlyKeys", generateOnlyKeys);
		resourcesGenerator.TransformText();
		var errors = resourcesGenerator.ErrorString();
		if (!string.IsNullOrWhiteSpace(errors))
			throw new Exception(errors);
	}
}
