using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pso2Tools.CmxViewer.ViewModels;

public interface ICmxMember
{
	public string Name { get; }
	public string? Text { get; }
	public string? TextHex { get; }
	public string? Bytes { get; }
}

public partial class CmxEntryModel : ObservableObject
{
	[ObservableProperty]
	public partial CmxObjectType ObjectType { get; set; }

	[ObservableProperty]
	public partial ICmxEntry? Object { get; set; }

	[ObservableProperty]
	public partial IEnumerable<ICmxMember> Members { get; private set; } = [];

	partial void OnObjectChanged(ICmxEntry? value)
	{
		if (value is null)
		{
			Members = [];
		}
		else
		{
			Members = MemberHelper.GetMembers(value.Data);
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

	public static IEnumerable<ICmxMember> GetMembers(object obj, string parentName = "")
	{
		var type = obj.GetType();
		var fields = type.GetFields();

		return [.. fields.SelectMany(field => GetMembers(obj, field, parentName))];
	}

	private static IEnumerable<ICmxMember> GetMembers(
		object obj,
		FieldInfo field,
		string parentName
	)
	{
		var name = parentName + field.Name;

		return field.FieldType switch
		{
			Type t when t == typeof(byte) => [new CmxByteMember(name, GetValue<byte>(obj, field))],
			Type t when t == typeof(short) =>
			[
				new CmxShortMember(name, GetValue<short>(obj, field)),
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
				new CmxStringMember(name, GetValue<string>(obj, field)),
			],
			Type t when t.IsEnum => [new CmxEnumMember(name, GetValue<Enum>(obj, field))],
			Type t when t.IsClass || IsStruct(t) => GetMembers(
				GetValue<object>(obj, field),
				name + "."
			),
			_ => [new CmxUnknownMember(name, GetValue<object>(obj, field))],
		};
	}

	private static T GetValue<T>(object obj, FieldInfo field)
		where T : notnull
	{
		if (field.GetValue(obj) is T value)
		{
			return value;
		}

		throw new KeyNotFoundException($"Object '{obj}' is missing field '{field.Name}'");
	}

	private static bool IsStruct(Type type) =>
		type.IsValueType && !type.IsPrimitive && !type.IsEnum;
}

public class CmxByteMember(string name, byte value) : ICmxMember
{
	public string Name { get; set; } = name;
	public byte Value { get; set; } = value;

	public string Text => Value.ToString();
	public string TextHex => $"0x{Value:X2}";
	public string Bytes => MemberHelper.GetBytes(Value);
}

public class CmxShortMember(string name, short value) : ICmxMember
{
	public string Name { get; set; } = name;
	public short Value { get; set; } = value;

	public string Text => Value.ToString();
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

public class CmxStringMember(string name, string value) : ICmxMember
{
	public string Name { get; set; } = name;
	public string Value { get; set; } = value;

	public string Text => Value.ToString();
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
