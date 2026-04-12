using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pso2Tools.CmxViewer.ViewModels;

public interface ICmxMember
{
	public string Name { get; }
	public string? Text { get; }
	public string? TextHex { get; }
	public string? Bytes { get; }
}

public class IceFileModel(IceFileInfo file, string displayPath, string fullPath)
{
	public IceFileInfo File { get; } = file;
	public string DisplayPath { get; } = displayPath;
	public string FullPath { get; } = fullPath;

	public string? Description => File.Description;
	public string Name => File.Name;
}

public partial class CmxEntryModel : ObservableObject
{
	private readonly string pso2BinPath;

	[ObservableProperty]
	public partial CmxObjectType ObjectType { get; set; }

	[ObservableProperty]
	public partial ICmxEntry? Object { get; set; }

	[ObservableProperty]
	public partial IEnumerable<ICmxMember> Members { get; private set; } = [];

	[ObservableProperty]
	public partial IEnumerable<IceFileModel> Files { get; private set; } = [];

	public CmxEntryModel(ISettingsService settings)
	{
		pso2BinPath = settings.Pso2BinPath ?? "";
	}

	partial void OnObjectChanged(ICmxEntry? value)
	{
		if (value is null)
		{
			Members = [];
			Files = [];
		}
		else
		{
			Members = MemberHelper.GetMembers(value.Data);
			Files = value.IceFiles.SelectMany(file =>
			{
				if (file.FindFile(pso2BinPath) is string fullPath)
				{
					var relativePath = Path.GetRelativePath(pso2BinPath, fullPath);

					return new IceFileModel[] { new IceFileModel(file, relativePath, fullPath) };
				}
				return [];
			});
		}
	}
}

public static class MemberHelper
{
	public static string GetBytes(byte value) => $"{value:X2}";

	public static string GetBytes(short value) => GetBytes(BitConverter.GetBytes(value));

	public static string GetBytes(int value) => GetBytes(BitConverter.GetBytes(value));

	public static string GetBytes(long value) => GetBytes(BitConverter.GetBytes(value));

	public static string GetBytes(float value) => GetBytes(BitConverter.GetBytes(value));

	public static string GetBytes(double value) => GetBytes(BitConverter.GetBytes(value));

	public static string GetBytes(Enum value) => GetBytes(Convert.ToInt32(value));

	public static string GetBytes(byte[] value)
	{
		return string.Join(' ', value.Select(b => $"{b:X2}"));
	}

	public static IEnumerable<ICmxMember> GetMembers(object? obj, string parentName = "")
	{
		if (obj is null)
		{
			return [];
		}

		var type = obj.GetType();
		var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);

		return fields.SelectMany(field => GetMembers(obj, field, type, parentName));
	}

	private static IEnumerable<ICmxMember> GetMembers(
		object obj,
		FieldInfo field,
		Type parentType,
		string parentName
	)
	{
		var name = parentName + field.Name;

		return field.FieldType switch
		{
			Type t when t == typeof(byte) =>
			[
				new CmxByteMember(
					name,
					GetValue<byte>(obj, field),
					GetPackedEnumType(parentType, field)
				),
			],
			Type t when t == typeof(short) =>
			[
				new CmxShortMember(
					name,
					GetValue<short>(obj, field),
					GetPackedEnumType(parentType, field)
				),
			],
			Type t when t == typeof(int) => [new CmxIntMember(name, GetValue<int>(obj, field))],
			Type t when t == typeof(long) => [new CmxLongMember(name, GetValue<long>(obj, field))],
			Type t when t == typeof(float) =>
			[
				new CmxFloatMember(name, GetValue<float>(obj, field)),
			],
			Type t when t == typeof(double) =>
			[
				new CmxDoubleMember(name, GetValue<double>(obj, field)),
			],
			Type t when t == typeof(string) =>
			[
				new CmxStringMember(name, GetValue<string?>(obj, field)),
			],
			// All enum types
			Type t when t.IsEnum => [new CmxEnumMember(name, GetValue<Enum>(obj, field))],
			// All class/struct types
			Type t when t.IsClass || IsStruct(t) => GetMembers(
				GetValue<object?>(obj, field),
				name + "."
			),
			_ => [new CmxUnknownMember(name, GetValue<object>(obj, field))],
		};
	}

	private static T GetValue<T>(object obj, FieldInfo field)
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
		return (T)field.GetValue(obj);
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
	}

	private static bool IsStruct(Type type) =>
		type.IsValueType && !type.IsPrimitive && !type.IsEnum;

	// Some enums are packed into smaller types. Add overrides for specific fields.
	private static Type? GetPackedEnumType(Type parentType, FieldInfo field)
	{
		if (parentType == typeof(HAIRMaskColorMapping))
		{
			return field.Name switch
			{
				nameof(HAIRMaskColorMapping.redIndex)
				or nameof(HAIRMaskColorMapping.greenIndex)
				or nameof(HAIRMaskColorMapping.blueIndex)
				or nameof(HAIRMaskColorMapping.alphaIndex) => typeof(CharColorMapping),
				_ => null,
			};
		}

		return null;
	}

	public static string IntToText<T>(T value, Type? asEnumType = null)
		where T : struct
	{
		if (asEnumType is not null)
		{
			var enumValue = Enum.ToObject(asEnumType, value);
			if (Enum.IsDefined(asEnumType, enumValue))
			{
				return $"{value} ({enumValue})";
			}
		}

		return value.ToString() ?? "";
	}
}

public class CmxByteMember(string name, byte value, Type? asEnumType = null) : ICmxMember
{
	public string Name { get; set; } = name;
	public byte Value { get; set; } = value;

	public string Text => MemberHelper.IntToText(Value, asEnumType);
	public string TextHex => $"0x{Value:X2}";
	public string Bytes => MemberHelper.GetBytes(Value);
}

public class CmxShortMember(string name, short value, Type? asEnumType = null) : ICmxMember
{
	public string Name { get; set; } = name;
	public short Value { get; set; } = value;

	public string Text => MemberHelper.IntToText(Value, asEnumType);
	public string TextHex => $"0x{Value:X4}";
	public string Bytes => MemberHelper.GetBytes(Value);
}

public class CmxIntMember(string name, int value) : ICmxMember
{
	public string Name { get; set; } = name;
	public int Value { get; set; } = value;

	public string Text => Value.ToString();
	public string TextHex => $"0x{Value:X8}";
	public string Bytes => MemberHelper.GetBytes(Value);
}

public class CmxLongMember(string name, long value) : ICmxMember
{
	public string Name { get; set; } = name;
	public long Value { get; set; } = value;

	public string Text => Value.ToString();

	// The only long field currently in CMX data is "originalOffset", which is just
	// the location of the data in the file. Don't need to show that in hexadecimal
	// and make that column wider just for the one field.
	public string? TextHex => null;
	public string Bytes => MemberHelper.GetBytes(Value);
}

public class CmxFloatMember(string name, float value) : ICmxMember
{
	public string Name { get; set; } = name;
	public float Value { get; set; } = value;

	public string Text => Value.ToString();
	public string? TextHex => null;
	public string Bytes => MemberHelper.GetBytes(Value);
}

public class CmxDoubleMember(string name, double value) : ICmxMember
{
	public string Name { get; set; } = name;
	public double Value { get; set; } = value;

	public string Text => Value.ToString();
	public string? TextHex => null;
	public string Bytes => MemberHelper.GetBytes(Value);
}

public class CmxStringMember(string name, string? value) : ICmxMember
{
	public string Name { get; set; } = name;
	public string? Value { get; set; } = value;

	public string Text => Value?.ToString() ?? "";
	public string? TextHex => null;
	public string? Bytes => null;
}

public class CmxEnumMember(string name, Enum value) : ICmxMember
{
	public string Name { get; set; } = name;
	public Enum Value { get; set; } = value;

	public string Text =>
		Enum.IsDefined(Value.GetType(), Value)
			? $"{Convert.ToInt32(Value)} ({Value})"
			: Value.ToString();

	public string? TextHex => $"0x{Convert.ToInt32(Value):X}";
	public string? Bytes => MemberHelper.GetBytes(Value);
}

public class CmxUnknownMember(string name, object value) : ICmxMember
{
	public string Name { get; set; } = name;
	public object Value { get; set; } = value;

	public string? Text => Value.ToString();
	public string? TextHex => null;
	public string? Bytes => null;
}
