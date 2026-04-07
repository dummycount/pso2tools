namespace Pso2Tools;

public class CmxNames
{
	public string? Jp { get; set; }
	public string? En { get; set; }

	public string Value => En ?? Jp ?? "";

	public static implicit operator bool(CmxNames x) =>
		!string.IsNullOrEmpty(x.Jp) || !string.IsNullOrEmpty(x.En);

	public void SetByLanguage(int language, string value)
	{
		switch (language)
		{
			case 0:
				Jp = value;
				break;

			case 1:
				En = value;
				break;

			default:
				throw new ArgumentException($"Invalid language {language}");
		}
	}
}
