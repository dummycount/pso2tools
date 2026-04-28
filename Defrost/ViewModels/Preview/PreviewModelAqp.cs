using System;
using System.Buffers;
using System.Collections.Generic;
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
	public partial Windows.UI.Color RedColor { get; set; } = settings.RedColor;

	[ObservableProperty]
	public partial Windows.UI.Color GreenColor { get; set; } = settings.GreenColor;

	[ObservableProperty]
	public partial Windows.UI.Color BlueColor { get; set; } = settings.BlueColor;

	[ObservableProperty]
	public partial Windows.UI.Color AlphaColor { get; set; } = settings.AlphaColor;

	private IceFileModel? file = null;

	private HelixToolkitScene? scene = null;

	[RelayCommand]
	public void ResetCamera()
	{
		// TODO: this doesn't fit models that are tall and thin fully in the camera.

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

	[RelayCommand]
	private async Task ReloadModel(CancellationToken token)
	{
		await UpdateSceneAsync(CancellationToken.None);
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
			SkinColor = SkinColor.ToRgba32(),
			MaskColors = new()
			{
				R = RedColor.ToRgba32(),
				G = GreenColor.ToRgba32(),
				B = BlueColor.ToRgba32(),
				A = AlphaColor.ToRgba32(),
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

			BoundingBox = result.BoundingBox ?? default;
			ModelCentroid = result.ModelCentroid ?? default;

			UpdateAxes();
			UpdateNodeProperties();
		}
		catch (ImportError ex)
		{
			scene = null;
			Root.Clear();

			notificationService.ShowNotification(
				new()
				{
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

	private IEnumerable<IceDataFile> FindDdsFiles()
	{
		return mainViewModel
			.FilesTyped.Where(file => file.Name.EndsWith(".dds"))
			.Select(file => file.File);
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
				meshNode.CullMode = SharpDX.Direct3D11.CullMode.Back;
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

public class ImportError(string? message = null, ErrorCode code = ErrorCode.None)
	: Exception(message)
{
	public ErrorCode Code { get; } = code;
}

public class ModelImporter
{
	public struct Result
	{
		public HelixToolkitScene Scene;
		public BoundingBox? BoundingBox;
		public Vector3? ModelCentroid;
	}

	public ImporterConfiguration Configuration { get; set; } = new();
	public required IceDataFile AqpFile { get; set; }
	public IceDataFile? AqnFile { get; set; }
	public IEnumerable<IceDataFile> DdsFiles { get; set; } = [];

	public Rgba32 SkinColor { get; set; }
	public MaskColors MaskColors { get; set; }

	public Task<Result> LoadSceneAsync(CancellationToken token = default)
	{
		var aqpData = AqpFile.Data.ToArray();
		var aqnData = AqnFile?.Data.ToArray();
		var ddsFiles = DdsFiles.ToArray();
		var skinColor = SkinColor;
		var maskColors = MaskColors;

		return Task.Run(
			async () =>
			{
				var aqp = new AquaPackage(aqpData);
				token.ThrowIfCancellationRequested();

				var aqn = aqnData is null ? AquaNode.GenerateBasicAQN() : new AquaNode(aqnData);
				token.ThrowIfCancellationRequested();

				return await LoadSceneInternalAsync(
					aqp,
					aqn,
					ddsFiles,
					skinColor,
					maskColors,
					token
				);
			},
			token
		);
	}

	private static async Task<Result> LoadSceneInternalAsync(
		AquaPackage aqp,
		AquaNode aqn,
		IEnumerable<IceDataFile> ddsFiles,
		Rgba32 skinColor,
		MaskColors maskColors,
		CancellationToken token = default
	)
	{
		// TODO: need to handle multiple models?
		var obj = aqp.models.FirstOrDefault() ?? throw new ImportError(".aqp file has no models");

		var scene = AssimpModelExporter.AssimpExport("", obj, aqn);
		token.ThrowIfCancellationRequested();

		await AddTexturesAsync(scene, ddsFiles, maskColors, token);

		var importer = new Pso2Importer() { SkinColor = skinColor.ToScaledVector4() };

		var code = importer.ToHelixToolkitScene(scene, out var helixScene);
		if ((code & ErrorCode.Succeed) == 0)
		{
			throw new ImportError(code: code);
		}

		if (helixScene is null)
		{
			throw new ImportError("Scene import failed.");
		}

		var result = new Result { Scene = helixScene };

		if (helixScene.Root.TryGetBound(out var bound))
		{
			result.BoundingBox = bound;
		}

		if (helixScene.Root.TryGetCentroid(out var centroid))
		{
			result.ModelCentroid = centroid;
		}

		return result;
	}

	private static async Task AddTexturesAsync(
		SharpAssimp.Scene scene,
		IEnumerable<IceDataFile> ddsFiles,
		MaskColors maskColors,
		CancellationToken token = default
	)
	{
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
			var tex = await GetTextureAsync(ddsFiles, name, maskColors, token);
			if (tex is not null)
			{
				scene.Textures.Add(new SharpAssimp.EmbeddedTexture("DDS", tex, name));
			}
		}
	}

	private static async Task<byte[]?> GetTextureAsync(
		IEnumerable<IceDataFile> ddsFiles,
		string name,
		MaskColors maskColors,
		CancellationToken token = default
	)
	{
		var pattern = name switch
		{
			// Classic
			"pl_body_diffuse.dds" => @"pl_bd_.+_d_.+_bw",
			"pl_body_multi.dds" => @"pl_bd_.+_m_.+_bw",
			"pl_body_normal.dds" => @"pl_bd_.+_n_.+_bw",

			// NGS
			"pl_body_base_diffuse.dds" => @"pl_rbd_.+_.+_d",
			"pl_body_base_multi.dds" => @"pl_rbd_.+_.+_m",
			"pl_body_base_normal.dds" => @"pl_rbd_.+_.+_n",

			_ => null,
		};

		// TODO: add a setting to pick which skin is loaded and pso2_data path, then load textures
		// if there is a skin material.

		IceDataFile? texture;

		if (pattern is null)
		{
			texture = ddsFiles.FirstOrDefault(text => text.Name == name);
		}
		else
		{
			var regex = new Regex(pattern + @"\.dds", RegexOptions.IgnoreCase);
			texture = ddsFiles.FirstOrDefault(tex => regex.IsMatch(tex.Name));
		}

		if (texture is null)
		{
			return null;
		}

		return await ApplyTextureTransforms(texture, ddsFiles, maskColors, token);
	}

	private static async Task<byte[]> ApplyTextureTransforms(
		IceDataFile texture,
		IEnumerable<IceDataFile> ddsFiles,
		MaskColors maskColors,
		CancellationToken token = default
	)
	{
		List<Func<Image<Rgba32>, Task<Image<Rgba32>>>> transforms = [];

		// Colorize diffuse textures using multi color mask
		// TODO: move this into a shader so colors can be changed on the fly
		if (texture.Name.EndsWith("_d.dds"))
		{
			var maskName = texture.Name.Replace("_d.dds", "_m.dds");
			var mask = ddsFiles.FirstOrDefault(x => x.Name == maskName);
			if (mask is not null)
			{
				transforms.Add(
					async (image) =>
					{
						var maskImage = await ImageHelper.DdsBufferToImageAsync(
							mask.Data.ToArray(),
							token
						);

						token.ThrowIfCancellationRequested();
						return ImageHelper.ColorizeDiffuseTexture(
							image,
							maskImage,
							maskColors,
							token
						);
					}
				);
			}
		}

		// Expand and shift cast part textures according so the UVs line up with the texture data
		if (texture.Name.Contains("_rm_"))
		{
			transforms.Add(
				async (image) => ImageHelper.ShiftCastPartTexture(image, CastTextureShift.Arms)
			);
		}
		if (texture.Name.Contains("_bd_"))
		{
			transforms.Add(
				async (image) => ImageHelper.ShiftCastPartTexture(image, CastTextureShift.Body)
			);
		}
		if (texture.Name.Contains("_lg_"))
		{
			transforms.Add(
				async (image) => ImageHelper.ShiftCastPartTexture(image, CastTextureShift.Legs)
			);
		}

		if (transforms.Count == 0)
		{
			return texture.Data.ToArray();
		}

		var image = await ImageHelper.DdsBufferToImageAsync(texture.Data.ToArray(), token);

		foreach (var transform in transforms)
		{
			token.ThrowIfCancellationRequested();
			image = await transform(image);
		}

		return await ImageHelper.ImageToDdsBufferAsync(image, token);
	}
}

public partial class Pso2Importer : Importer
{
	public Vector4 SkinColor { get; set; }

	protected override HelixToolkit.SharpDX.Model.PhongMaterialCore OnCreatePhongMaterial(
		SharpAssimp.Material material
	)
	{
		// Some materials have black for a diffuse color, which makes them solid black
		// in the preview. We aren't trying to accurately render things, so undo that.
		material.ColorDiffuse = new Vector4(1, 1, 1, 1);

		var match = MaterialNameRegex.Match(material.Name);
		if (match.Success)
		{
			var shader = match.Groups["shaders"].Value;

			if (shader == "1102p,1102" || shader == "1101p,1101")
			{
				// Skin shader. Just use a flat color instead of loading textures from a different file.
				material.ColorDiffuse = SkinColor;
			}

			material.IsTwoSided = match.Groups["two_sided"]?.Value == "1";

			// TODO: need to write a custom shader to support alpha threshold
		}

		material.ColorAmbient = material.ColorDiffuse;
		material.ColorTransparent = new Vector4(1, 1, 1, 0);

		material.TextureDisplacement = default;
		material.TextureEmissive = default;
		material.TextureHeight = default;
		material.TextureLightMap = default;
		material.TextureOpacity = default;
		material.TextureReflection = default;
		material.TextureSpecular = default;

		return base.OnCreatePhongMaterial(material);
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
