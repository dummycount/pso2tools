using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AquaModelLibrary.Core.General;
using AquaModelLibrary.Data.PSO2.Aqua;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HelixToolkit.Maths;
using HelixToolkit.SharpDX;
using HelixToolkit.SharpDX.Assimp;
using HelixToolkit.SharpDX.Model.Scene;
using HelixToolkit.WinUI.SharpDX;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pso2Tools.Defrost.ViewModels.Preview;

public partial class PreviewModelAqp(
	IceArchiveModel mainViewModel,
	ISettingsService settings,
	EffectsManagerService effectsManagerService,
	NotificationService notificationService
) : ObservableObject
{
	[ObservableProperty]
	public partial IEffectsManager? EffectsManager { get; private set; }

	public PerspectiveCamera Camera { get; } = new PerspectiveCamera();

	public SceneNodeGroupModel3D Root { get; } = new();

	[ObservableProperty]
	public partial Vector3 ModelCentroid { get; set; } = default;

	[ObservableProperty]
	public partial BoundingBox BoundingBox { get; set; } = default;

	[ObservableProperty]
	public partial Geometry3D? Axes { get; private set; }

	[ObservableProperty]
	public partial bool ShowAxes { get; set; } = settings.ModelPreviewShowAxes;

	[ObservableProperty]
	public partial bool ShowGrid { get; set; } = settings.ModelPreviewShowGrid;

	[ObservableProperty]
	public partial bool ShowWireframe { get; set; } = settings.ModelPreviewShowWireframe;

	[ObservableProperty]
	public partial Windows.UI.Color SkinColor { get; set; } = settings.SkinColor;

	[ObservableProperty]
	public partial Windows.UI.Color SubSkinColor { get; set; } = settings.SubSkinColor;

	[ObservableProperty]
	public partial Windows.UI.Color RedColor { get; set; } = settings.RedColor;

	[ObservableProperty]
	public partial Windows.UI.Color GreenColor { get; set; } = settings.GreenColor;

	[ObservableProperty]
	public partial Windows.UI.Color BlueColor { get; set; } = settings.BlueColor;

	[ObservableProperty]
	public partial Windows.UI.Color AlphaColor { get; set; } = settings.AlphaColor;

	[ObservableProperty]
	public partial MaskUsedChannels MainUsedChannels { get; set; } = default;

	[ObservableProperty]
	public partial MaskUsedChannels SkinUsedChannels { get; set; } = default;

	private IceFileModel? file = null;
	private readonly List<TextureFile> textureCache = [];

	private HelixToolkitScene? scene = null;

	[RelayCommand]
	public void ResetCamera()
	{
		var maxWidth = Math.Max(Math.Max(BoundingBox.Width, BoundingBox.Height), BoundingBox.Depth);

		var fieldOfViewRad = (Math.PI / 180) * Camera.FieldOfView;
		var distance = (maxWidth / 2) / Math.Tan(fieldOfViewRad / 2) * 1.25f;

		var pos = BoundingBox.Center + new Vector3(0, 0, (float)distance);

		Camera.Position = pos;
		Camera.LookDirection = BoundingBox.Center - pos;
		Camera.UpDirection = Vector3.UnitY;
	}

	[RelayCommand]
	private async Task LoadModel(IceFileModel file)
	{
		this.file = file;

		EffectsManager ??= await effectsManagerService.GetEffectsManagerAsync();

		await UpdateSceneAsync(CancellationToken.None);

		ResetCamera();
	}

	private CancellationTokenSource? reloadCts;

	[RelayCommand]
	private async Task ReloadModel(CancellationToken token)
	{
		// Debounce this to reduce lag when adjusting a color picker
		reloadCts?.Cancel();
		reloadCts = new();

		try
		{
			await Task.Delay(300, reloadCts.Token);
			await UpdateSceneAsync(CancellationToken.None);
		}
		catch (TaskCanceledException) { }
	}

	private async Task UpdateSceneAsync(CancellationToken token = default)
	{
		if (file is null)
		{
			return;
		}

		var importer = new ModelImporter
		{
			AqpFile = file.File,
			AqnFile = FindAqnFile(),
			DdsFiles = FindDdsFiles(),
			Options = new()
			{
				SkinTextureT1File = settings.GetSkinTextureT1Path(),
				SkinTextureT2File = settings.GetSkinTextureT2Path(),
				SkinColors = new() { R = SkinColor.ToRgba32(), G = SubSkinColor.ToRgba32() },
				MaskColors = new()
				{
					R = RedColor.ToRgba32(),
					G = GreenColor.ToRgba32(),
					B = BlueColor.ToRgba32(),
					A = AlphaColor.ToRgba32(),
				},
			},
		};

		try
		{
			var result = await importer.LoadSceneAsync(token);

			scene = result.Scene;
			Root.Clear();
			Root.AddNode(scene.Root);

			// TODO: attach a view model to nodes and allow selecting and
			// showing mesh names.

			BoundingBox = result.Metadata.BoundingBox ?? default;
			ModelCentroid = result.Metadata.ModelCentroid ?? default;
			MainUsedChannels = result.Metadata.MainChannels;
			SkinUsedChannels = result.Metadata.SkinChannels;

			Debug.WriteLine($"Loaded {result.Metadata.LoadedTextures.Count} new textures");
			textureCache.AddRange(result.Metadata.LoadedTextures);

			UpdateAxes();
			UpdateNodeProperties();
		}
		catch (Exception ex)
		{
			scene = null;
			Root.Clear();

			notificationService.ShowNotification(
				new()
				{
					Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error,
					Title = "Failed to load model",
					Message = ex.Message,
					Duration = TimeSpan.FromSeconds(10),
				}
			);
		}
	}

	private IceDataFile? FindAqnFile()
	{
		return mainViewModel.FilesTyped.FirstOrDefault(file => file.Name.EndsWith(".aqn"))?.File;
	}

	private IEnumerable<TextureFile> FindDdsFiles()
	{
		var fileTextures = mainViewModel
			.FilesTyped.Where(file => file.Name.EndsWith(".dds"))
			.Select(file => TextureFile.FromIceFile(file.File));

		return fileTextures.Concat(textureCache);
	}

	private void UpdateAxes()
	{
		var builder = new LineBuilder();

		var x1 = Math.Min(0, BoundingBox.Center.X - BoundingBox.Width / 2);
		var x2 = Math.Max(0, BoundingBox.Center.X + BoundingBox.Width / 2);

		builder.AddLine(new Vector3(x1, 0, 0), new Vector3(x2, 0, 0));

		var y1 = Math.Min(0, BoundingBox.Center.Y - BoundingBox.Height / 2);
		var y2 = Math.Max(0, BoundingBox.Center.Y + BoundingBox.Height / 2);

		builder.AddLine(new Vector3(0, y1, 0), new Vector3(0, y2, 0));

		var z1 = Math.Min(0, BoundingBox.Center.Z - BoundingBox.Depth / 2);
		var z2 = Math.Max(0, BoundingBox.Center.Z + BoundingBox.Depth / 2);

		builder.AddLine(new Vector3(0, 0, z1), new Vector3(0, 0, z2));

		var axis = builder.ToLineGeometry3D();
		axis.Colors = [];
		axis.Colors.Resize(axis.Positions?.Count ?? 0, true);
		axis.Colors[0] = axis.Colors[1] = HelixToolkit.Maths.Color.Red;
		axis.Colors[2] = axis.Colors[3] = HelixToolkit.Maths.Color.Green;
		axis.Colors[4] = axis.Colors[5] = HelixToolkit.Maths.Color.Blue;

		Axes = axis;
	}

	partial void OnShowAxesChanged(bool value)
	{
		settings.ModelPreviewShowAxes = value;
	}

	partial void OnShowGridChanged(bool value)
	{
		settings.ModelPreviewShowGrid = value;
	}

	partial void OnShowWireframeChanged(bool value)
	{
		settings.ModelPreviewShowWireframe = value;
		UpdateNodeProperties();
	}

	private void UpdateNodeProperties()
	{
		if (scene is null || scene.Root is null)
		{
			return;
		}

		foreach (var node in scene.Root.Traverse())
		{
			if (node is MeshNode meshNode)
			{
				meshNode.RenderWireframe = ShowWireframe;

				// TODO: this doesn't render correctly when there are two meshes in the same spot
				// with different facing and back face culling enabled.
				var data = new MaterialData(meshNode.Material?.Name);
				meshNode.CullMode = data.IsTwoSided
					? SharpDX.Direct3D11.CullMode.None
					: SharpDX.Direct3D11.CullMode.Back;
			}
		}
	}

	private void UpdateTextureColors()
	{
		ReloadModelCommand.Execute(null);
	}

	partial void OnSkinColorChanged(Windows.UI.Color value)
	{
		settings.SkinColor = value;
		UpdateTextureColors();
	}

	partial void OnSubSkinColorChanged(Windows.UI.Color value)
	{
		settings.SubSkinColor = value;
		UpdateTextureColors();
	}

	partial void OnRedColorChanged(Windows.UI.Color value)
	{
		settings.RedColor = value;
		UpdateTextureColors();
	}

	partial void OnGreenColorChanged(Windows.UI.Color value)
	{
		settings.GreenColor = value;
		UpdateTextureColors();
	}

	partial void OnBlueColorChanged(Windows.UI.Color value)
	{
		settings.BlueColor = value;
		UpdateTextureColors();
	}

	partial void OnAlphaColorChanged(Windows.UI.Color value)
	{
		settings.AlphaColor = value;
		UpdateTextureColors();
	}
}

public class TextureFile
{
	public required string FileName { get; set; }
	public required byte[] Data { get; set; }
	public string? ModelName { get; set; }

	public static TextureFile FromIceFile(IceDataFile file)
	{
		return new() { FileName = file.Name, Data = file.Data.ToArray() };
	}

	public static async Task<TextureFile> FromImageAsync(
		string fileName,
		Image<Rgba32> image,
		CancellationToken token = default
	)
	{
		return new()
		{
			FileName = fileName,
			Data = await ImageHelper.ImageToDdsBufferAsync(image, token),
		};
	}

	public SharpAssimp.EmbeddedTexture ToEmbeddedTexture()
	{
		return new SharpAssimp.EmbeddedTexture("DDS", Data, ModelName ?? FileName);
	}

	public async Task<Image<Rgba32>> ToImageAsync(CancellationToken token = default)
	{
		return await ImageHelper.DdsBufferToImageAsync(Data, token);
	}
}

public class ImportError(string? message = null, ErrorCode code = ErrorCode.None)
	: Exception(message)
{
	public ErrorCode Code { get; } = code;
}

public class ModelImporterOptions
{
	public MaskColors SkinColors { get; set; }
	public MaskColors MaskColors { get; set; }
	public string? SkinTextureT1File { get; set; }
	public string? SkinTextureT2File { get; set; }
};

public partial class ModelImporter
{
	public class SceneMetadata
	{
		public BoundingBox? BoundingBox { get; set; }
		public Vector3? ModelCentroid { get; set; }
		public List<TextureFile> LoadedTextures { get; set; } = [];
		public MaskUsedChannels MainChannels { get; set; }
		public MaskUsedChannels SkinChannels { get; set; }
	};

	public class Result
	{
		public required HelixToolkitScene Scene { get; set; }
		public required SceneMetadata Metadata { get; set; }
	}

	public ImporterConfiguration Configuration { get; set; } = new();
	public required IceDataFile AqpFile { get; set; }
	public IceDataFile? AqnFile { get; set; }
	public IEnumerable<TextureFile> DdsFiles { get; set; } = [];

	public ModelImporterOptions Options { get; set; } = new();

	public Task<Result> LoadSceneAsync(CancellationToken token = default)
	{
		var aqpName = AqpFile.Name;
		var aqpData = AqpFile.Data.ToArray();
		var aqnData = AqnFile?.Data.ToArray();
		var ddsFiles = DdsFiles.ToArray();
		var options = Options;

		return Task.Run(
			async () =>
			{
				var sw = Stopwatch.StartNew();
				var aqp = new AquaPackage(aqpData);
				token.ThrowIfCancellationRequested();

				var aqn = aqnData is null ? AquaNode.GenerateBasicAQN() : new AquaNode(aqnData);
				token.ThrowIfCancellationRequested();

				Debug.WriteLine($"Created aqp/aqn in {sw.ElapsedMilliseconds} ms");

				return await LoadSceneInternalAsync(aqp, aqn, aqpName, ddsFiles, options, token);
			},
			token
		);
	}

	private static async Task<Result> LoadSceneInternalAsync(
		AquaPackage aqp,
		AquaNode aqn,
		string aqpName,
		IEnumerable<TextureFile> ddsFiles,
		ModelImporterOptions options,
		CancellationToken token = default
	)
	{
		var metadata = new SceneMetadata();
		var sw = Stopwatch.StartNew();

		// TODO: need to handle multiple models?
		var obj = aqp.models.FirstOrDefault() ?? throw new ImportError(".aqp file has no models");

		var scene = AssimpModelExporter.AssimpExport("", obj, aqn);
		token.ThrowIfCancellationRequested();

		Debug.WriteLine($"AssimpModelExporter.AssimpExport {sw.ElapsedMilliseconds} ms");

		await AddTexturesAsync(scene, metadata, aqpName, ddsFiles, options, token);
		ddsFiles = ddsFiles.Concat(metadata.LoadedTextures);

		var importer = new Pso2Importer()
		{
			HasSkinTexture = ddsFiles.Any(x => x.FileName.Contains("_sk_")),
			SkinColor = options.SkinColors.R.ToScaledVector4(),
		};

		var sw2 = Stopwatch.StartNew();

		var code = importer.ToHelixToolkitScene(scene, out var helixScene);
		if ((code & ErrorCode.Succeed) == 0)
		{
			throw new ImportError(code: code);
		}

		Debug.WriteLine($"importer.ToHelixToolkitScene {sw2.ElapsedMilliseconds} ms");

		if (helixScene is null)
		{
			throw new ImportError("Scene import failed.");
		}

		var result = new Result { Scene = helixScene, Metadata = metadata };

		if (helixScene.Root.TryGetBound(out var bound))
		{
			result.Metadata.BoundingBox = bound;
		}

		if (helixScene.Root.TryGetCentroid(out var centroid))
		{
			result.Metadata.ModelCentroid = centroid;
		}

		Debug.WriteLine($"LoadSceneInternalAsync {sw.ElapsedMilliseconds} ms");

		return result;
	}

	private static async Task AddTexturesAsync(
		SharpAssimp.Scene scene,
		SceneMetadata metadata,
		string aqpName,
		IEnumerable<TextureFile> ddsFiles,
		ModelImporterOptions options,
		CancellationToken token = default
	)
	{
		var sw = Stopwatch.StartNew();

		var names = new HashSet<string>();

		foreach (var mat in scene.Materials)
		{
			foreach (var tex in mat.GetAllMaterialTextures())
			{
				names.Add(tex.FilePath);
			}
		}

		token.ThrowIfCancellationRequested();

		foreach (var name in names)
		{
			var allTextures = ddsFiles.Concat(metadata.LoadedTextures);
			var tex = await GetTextureAsync(metadata, aqpName, allTextures, name, options, token);
			if (tex is not null)
			{
				scene.Textures.Add(tex.ToEmbeddedTexture());
			}
		}

		Debug.WriteLine($"AddTexturesAsync {sw.ElapsedMilliseconds} ms");
	}

	private static async Task<TextureFile?> GetTextureAsync(
		SceneMetadata metadata,
		string aqpName,
		IEnumerable<TextureFile> ddsFiles,
		string textureName,
		ModelImporterOptions options,
		CancellationToken token = default
	)
	{
		var pattern = textureName switch
		{
			// Classic
			"pl_body_diffuse.dds" => @"pl_(rm|tr|lg|bd)_.+_d_.+_(bw|xx)",
			"pl_body_normal.dds" => @"pl_(rm|tr|lg|bd)_.+_n_.+_(bw|xx)",
			"pl_body_multi.dds" => @"pl_(rm|tr|lg|bd)_.+_s_.+_(bw|xx)",

			// NGS
			"pl_body_base_diffuse.dds" => @"pl_rbd_.+_(rm|lg|bd|bw|ow)_d",
			"pl_body_base_normal.dds" => @"pl_rbd_.+_(rm|lg|bd|bw|ow)_n",
			"pl_body_base_multi.dds" => @"pl_rbd_.+_(rm|lg|bd|bw|ow)_s",

			"pl_body_skin_diffuse.dds" => "pl_rbd_.+_sk_d",
			"pl_body_skin_normal.dds" => "pl_rbd_.+_sk_n",
			"pl_body_skin_multi.dds" => "pl_rbd_.+_sk_n",

			"pl_face_diffuse.dds" => "pl_rhd_.+_d",
			"pl_face_normal.dds" => "pl_rhd_.+_n",
			"pl_face_multi.dds" => "pl_rhd_.+_s",

			"pl_hair_diffuse.dds" => "pl_rhr_.+_d",
			"pl_hair_normal.dds" => "pl_rhr_.+_n",
			"pl_hair_multi.dds" => "pl_rhr_.+_s",
			"pl_hair_alpha.dds" => "pl_rhr_.+_a",

			_ => null,
		};

		TextureFile? texture;

		if (pattern is null)
		{
			texture = ddsFiles.FirstOrDefault(text => text.FileName == textureName);
		}
		else
		{
			var regex = new Regex(pattern + @"\.dds", RegexOptions.IgnoreCase);
			texture = ddsFiles.FirstOrDefault(tex => regex.IsMatch(tex.FileName));

			if (texture is null && textureName.Contains("_skin_"))
			{
				var loaded = await LoadSkinTexturesAsync(aqpName, options);

				texture = loaded.FirstOrDefault(tex => regex.IsMatch(tex.FileName));
				ddsFiles = ddsFiles.Concat(loaded);

				metadata.LoadedTextures.AddRange(loaded);
			}
		}

		if (texture is null)
		{
			return null;
		}

		var data = await ApplyTextureTransforms(
			metadata,
			aqpName,
			texture,
			ddsFiles,
			options,
			token
		);
		data.ModelName = textureName;
		return data;
	}

	private static async Task<IEnumerable<TextureFile>> LoadSkinTexturesAsync(
		string aqpName,
		ModelImporterOptions options
	)
	{
		var filePath = IsAqpT2(aqpName) ? options.SkinTextureT2File : options.SkinTextureT1File;
		if (filePath is null)
		{
			return [];
		}

		try
		{
			var sw = Stopwatch.StartNew();
			var ice = await IceWrapper.LoadAsync(filePath);
			Debug.WriteLine($"Load skin textures {sw.ElapsedMilliseconds} ms");

			return ice.Files.Where(f => f.Name.EndsWith(".dds")).Select(TextureFile.FromIceFile);
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Failed to load skin textures. {ex.Message}");
			return [];
		}
	}

	[GeneratedRegex(@"\d+")]
	private static partial Regex NumberRegex { get; }

	private static int GetAqpObjectId(string aqpName)
	{
		var match = NumberRegex.Match(aqpName);
		if (match.Success && int.TryParse(match.Value, out int objectId))
		{
			return objectId;
		}

		return 0;
	}

	private static bool IsAqpNgs(string aqpName) => CmxObjectIds.IsNgs(GetAqpObjectId(aqpName));

	private static bool IsAqpT2(string aqpName) => CmxObjectIds.IsT2(GetAqpObjectId(aqpName));

	private static async Task<TextureFile> ApplyTextureTransforms(
		SceneMetadata metadata,
		string aqpName,
		TextureFile texture,
		IEnumerable<TextureFile> ddsFiles,
		ModelImporterOptions options,
		CancellationToken token = default
	)
	{
		List<Func<Image<Rgba32>, Task<Image<Rgba32>>>> transforms = [];

		// Colorize diffuse textures using multi color mask
		// TODO: move this into a shader so colors can be changed on the fly
		if (TextureNames.IsDiffuse(texture.FileName))
		{
			TextureFile? mask = null;

			foreach (var maskName in TextureNames.GetMaskTextures(texture.FileName))
			{
				mask = ddsFiles.FirstOrDefault(x => x.FileName == maskName);
				if (mask is not null)
				{
					break;
				}
			}

			if (mask is not null)
			{
				transforms.Add(
					async (image) =>
					{
						var maskImage = await mask.ToImageAsync(token);

						token.ThrowIfCancellationRequested();

						var isSkin = TextureNames.IsSkin(texture.FileName);

						var maskColors = isSkin ? options.SkinColors : options.MaskColors;
						var blendMode = isSkin
							? PixelColorBlendingMode.Multiply
							: PixelColorBlendingMode.Normal;

						var colorized = ImageHelper.ColorizeDiffuseTexture(
							image,
							maskImage,
							maskColors,
							blendMode,
							out var usedChannels,
							token
						);

						if (isSkin)
						{
							metadata.SkinChannels = usedChannels;
						}
						else
						{
							metadata.MainChannels = usedChannels;
						}

						return colorized;
					}
				);
			}
		}

		// Expand and shift cast part textures according so the UVs line up with the texture data
		if (TextureNames.IsCastArm(texture.FileName))
		{
			transforms.Add(
				async (image) =>
					ImageHelper.ShiftCastPartTexture(
						image,
						IsAqpNgs(aqpName) ? CastTextureShift.NgsArms : CastTextureShift.ClassicArms
					)
			);
		}
		if (TextureNames.IsCastBody(texture.FileName))
		{
			transforms.Add(
				async (image) =>
					ImageHelper.ShiftCastPartTexture(
						image,
						IsAqpNgs(aqpName) ? CastTextureShift.NgsBody : CastTextureShift.ClassicBody
					)
			);
		}
		if (TextureNames.IsCastLeg(texture.FileName))
		{
			transforms.Add(
				async (image) =>
					ImageHelper.ShiftCastPartTexture(
						image,
						IsAqpNgs(aqpName) ? CastTextureShift.NgsLegs : CastTextureShift.ClassicLegs
					)
			);
		}

		if (transforms.Count == 0)
		{
			return texture;
		}

		// TODO: speed this up or make an option to skip it
		var sw = Stopwatch.StartNew();

		var image = await texture.ToImageAsync(token);

		try
		{
			Debug.WriteLine($"DdsBufferToImageAsync {sw.ElapsedMilliseconds} ms");

			sw = Stopwatch.StartNew();

			foreach (var transform in transforms)
			{
				token.ThrowIfCancellationRequested();
				var newImage = await transform(image);

				if (!ReferenceEquals(newImage, image))
				{
					image.Dispose();
					image = newImage;
				}
			}

			Debug.WriteLine($"Apply transforms {sw.ElapsedMilliseconds} ms");

			return await TextureFile.FromImageAsync(texture.FileName, image, token);
		}
		finally
		{
			image.Dispose();
		}
	}
}

public partial class Pso2Importer : Importer
{
	public bool HasSkinTexture { get; set; }
	public Vector4 SkinColor { get; set; }

	protected override HelixToolkit.SharpDX.Model.PhongMaterialCore OnCreatePhongMaterial(
		SharpAssimp.Material material
	)
	{
		// TODO: need to write a custom shader to support alpha threshold

		// Some materials have black for a diffuse color, which makes them solid black
		// in the preview. We aren't trying to accurately render things, so undo that.
		material.ColorDiffuse = new Vector4(1, 1, 1, 1);

		var data = new MaterialData(material.Name);

		if (data.IsSkinShader && !HasSkinTexture)
		{
			// If we couldn't find a skin texture, just make it skin colored
			material.ColorDiffuse = SkinColor;
		}

		if (data.IsHairShader)
		{
			var textures = material.GetAllMaterialTextures().ToArray();
			var alpha = textures.FirstOrDefault(x => x.FilePath == "pl_hair_alpha.dds");
			var diffuse = textures.FirstOrDefault(x => x.FilePath == "pl_hair_diffuse.dds");
			var normal = textures.FirstOrDefault(x => x.FilePath == "pl_hair_normal.dds");
			// TODO: multi map

			// Alpha uses UV 0
			// TODO: helix-toolkit's built-in shaders don't seem to support an alpha texture
			material.TextureOpacity = ModifyTexture(
				alpha,
				textureType: SharpAssimp.TextureType.Opacity
			);

			// Diffuse, normal, etc. uses UV 1
			// TODO: helix-toolkit's built-in shaders don't support a second set of UVs
			material.TextureDiffuse = ModifyTexture(diffuse, uvIndex: 1);
			material.TextureNormal = ModifyTexture(normal, uvIndex: 1);
		}
		else
		{
			material.RemoveMaterialTexture(material.TextureOpacity);
		}

		material.ColorAmbient = material.ColorDiffuse;
		material.ColorTransparent = new Vector4(0, 0, 0, 0);

		material.RemoveMaterialTexture(material.TextureDisplacement);
		material.RemoveMaterialTexture(material.TextureEmissive);
		material.RemoveMaterialTexture(material.TextureHeight);
		material.RemoveMaterialTexture(material.TextureLightMap);
		material.RemoveMaterialTexture(material.TextureReflection);
		material.RemoveMaterialTexture(material.TextureSpecular); // TODO: multi map

		return base.OnCreatePhongMaterial(material);
	}

	public static SharpAssimp.TextureSlot ModifyTexture(
		SharpAssimp.TextureSlot slot,
		SharpAssimp.TextureType? textureType = null,
		int? uvIndex = null
	)
	{
		return new SharpAssimp.TextureSlot(
			slot.FilePath,
			textureType ?? slot.TextureType,
			0,
			SharpAssimp.TextureMapping.FromUV,
			uvIndex ?? slot.UVIndex,
			slot.BlendFactor,
			slot.Operation,
			slot.WrapModeV,
			slot.WrapModeV,
			slot.Flags
		);
	}
}

public partial class MaterialData
{
	public string Shaders { get; set; } = "";
	public bool IsTwoSided { get; set; } = false;

	public bool IsSkinShader => Shaders == "1102p,1102" || Shaders == "1101p,1101";
	public bool IsHairShader => Shaders == "1103p,1103";

	public MaterialData(string? name)
	{
		if (name is null)
		{
			return;
		}

		var match = MaterialNameRegex.Match(name);
		if (match.Success)
		{
			Shaders = match.Groups["shaders"].Value;
			IsTwoSided = match.Groups["two_sided"]?.Value == "1";
		}
	}

	[GeneratedRegex(
		@"^
		\((?<shaders>[\w,]+)\)
		\{(?<blend_type>\w+)\}
		(?:\[(?<special_type>\w+)\])?
		(?<name>[^@]*)
		(?:@(?<two_sided>\d+))?
		(?:@(?<alpha_cutoff>\d+))?
		$",
		RegexOptions.IgnorePatternWhitespace
	)]
	private static partial Regex MaterialNameRegex { get; }
}

public static class TextureNames
{
	public static string[] Split(string name)
	{
		return name.RemoveSuffix(".dds").Split("_");
	}

	public static bool ContainsPart(string name, params string[] search)
	{
		var parts = Split(name);
		return search.Any(x => parts.Contains(x));
	}

	public static string[] GetMaskTextures(string diffuseName)
	{
		var parts = Split(diffuseName);

		var textureM = new string[parts.Length];
		var textureL = new string[parts.Length];

		parts.Replace(textureM, "d", "m");
		parts.Replace(textureL, "d", "l");

		return [string.Join('_', textureM) + ".dds", string.Join('_', textureL) + ".dds"];
	}

	public static bool IsDiffuse(string name) => ContainsPart(name, "d");

	public static bool IsSkin(string name) => ContainsPart(name, "sk", "rhd");

	public static bool IsCastArm(string name) => ContainsPart(name, "rm");

	public static bool IsCastBody(string name) => ContainsPart(name, "bd", "tr");

	public static bool IsCastLeg(string name) => ContainsPart(name, "lg");
}
