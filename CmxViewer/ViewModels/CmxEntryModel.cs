using System;
using System.Collections.Generic;
using System.Linq;
using AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pso2Tools.CmxViewer.ViewModels;

interface ICmxMember
{
	public string Name { get; }
	public object ValueObject { get; }
	public string? Bytes { get; }
}

public class CmxIntMember : ICmxMember
{
	public string Name { get; set; } = "";
	public int Value { get; set; }

	public object ValueObject => Value;
	public string Bytes => MemberHelper.GetBytes(Value);
}

public class CmxShortMember : ICmxMember
{
	public string Name { get; set; } = "";
	public short Value { get; set; }

	public object ValueObject => Value;
	public string Bytes => MemberHelper.GetBytes(Value);
}

public class CmxFloatMember : ICmxMember
{
	public string Name { get; set; } = "";
	public float Value { get; set; }

	public object ValueObject => Value;
	public string Bytes => MemberHelper.GetBytes(Value);
}

public partial class CmxEntryModel : ObservableObject
{
	[ObservableProperty]
	public partial CmxObjectType ObjectType { get; set; }

	[ObservableProperty]
	public partial ICmxEntry? Object { get; set; }

	[ObservableProperty]
	public partial IEnumerable<ICmxEntry> Members { get; private set; } = [];

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

	public static string GetBytes(int value) => GetBytes(BitConverter.GetBytes(value));

	public static string GetBytes(short value) => GetBytes(BitConverter.GetBytes(value));

	public static string GetBytes(float value) => GetBytes(BitConverter.GetBytes(value));

	public static string GetBytes(byte[] value)
	{
		return string.Join(' ', value.Select(b => $"{b:X2}"));
	}

	public static IEnumerable<ICmxEntry> GetMembers(BaseCMXObject obj)
	{
		var type = obj.GetType();
		var members = type.GetMembers();

		return [];
	}
}
