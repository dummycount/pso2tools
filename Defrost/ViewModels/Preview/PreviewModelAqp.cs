using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AquaModelLibrary.Core.General;
using AquaModelLibrary.Data.AM2.BorderBreakPS4;
using AquaModelLibrary.Data.PSO2.Aqua;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HelixToolkit.Maths;
using HelixToolkit.SharpDX;
using HelixToolkit.SharpDX.Assimp;
using HelixToolkit.SharpDX.Model.Scene;
using HelixToolkit.WinUI.SharpDX;
using Windows.UI;

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

	public Camera Camera { get; } = new PerspectiveCamera();

	public SceneNodeGroupModel3D Root { get; } = new();

	[ObservableProperty]
	public partial Vector3 ModelCentroid { get; set; } = default;

	[ObservableProperty]
	public partial BoundingBox BoundingBox { get; set; } = default;

	[ObservableProperty]
	public partial Geometry3D? Axis { get; private set; }

	[ObservableProperty]
	public partial bool ShowAxis { get; set; } = true;

	[ObservableProperty]
	public partial bool ShowWireframe { get; set; } = false;

	[ObservableProperty]
	public partial bool ShowGrid { get; set; } = true;

	private HelixToolkitScene? scene = null;

	[RelayCommand]
	public void ResetCamera()
	{
		var maxWidth = Math.Max(Math.Max(BoundingBox.Width, BoundingBox.Height), BoundingBox.Depth);
		var pos = BoundingBox.Center + new Vector3(0, 0, maxWidth);

		Camera.Position = pos;
		Camera.LookDirection = BoundingBox.Center - pos;
		Camera.UpDirection = Vector3.UnitY;
	}

	[RelayCommand]
	private async Task LoadModel(IceFileModel file)
	{
		Root.Clear();
		scene = null;

		EffectsManager ??= await effectsManagerService.GetEffectsManagerAsync();

		var importer = new ModelImporter
		{
			AqpFile = file.File,
			AqnFile = FindAqnFile(),
			DdsFiles = FindDdsFiles(),
			SkinColor = settings.SkinColor,
			Configuration = new()
			{
				// TODO: set cull mode individual on meshes based on material
				CullMode = SharpDX.Direct3D11.CullMode.Back,
				ForceCullMode = true,
			},
		};

		try
		{
			var result = await importer.LoadSceneAsync();

			scene = result.Scene;
			Root.AddNode(scene.Root);

			BoundingBox = result.BoundingBox ?? default;
			ModelCentroid = result.ModelCentroid ?? default;

			UpdateAxis();
			UpdateWireframe();
			ResetCamera();
		}
		catch (ImportError ex)
		{
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

	private void UpdateAxis()
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

		Axis = axis;
	}

	partial void OnShowWireframeChanged(bool value)
	{
		UpdateWireframe();
	}

	private void UpdateWireframe()
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
			}
		}
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

	public Windows.UI.Color SkinColor { get; set; }

	// TODO: colorize diffuse textures using mask texture?

	public Task<Result> LoadSceneAsync()
	{
		var aqp = new AquaPackage(AqpFile.Data.ToArray());
		var aqn = AqnFile is null
			? AquaNode.GenerateBasicAQN()
			: new AquaNode(AqnFile.Data.ToArray());

		var ddsFiles = DdsFiles.ToArray();
		var skinColor = SkinColor;

		return Task.Run(async () => await LoadSceneInternal(aqp, aqn, ddsFiles, skinColor));
	}

	private static async Task<Result> LoadSceneInternal(
		AquaPackage aqp,
		AquaNode aqn,
		IEnumerable<IceDataFile> ddsFiles,
		Windows.UI.Color skinColor
	)
	{
		// TODO: need to handle multiple models?
		var obj = aqp.models.FirstOrDefault() ?? throw new ImportError(".aqp file has no models");

		var scene = AssimpModelExporter.AssimpExport("", obj, aqn);

		await AddTexturesAsync(scene, ddsFiles);

		var importer = new Pso2Importer() { SkinColor = skinColor };

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
		IEnumerable<IceDataFile> ddsFiles
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

		foreach (var name in names)
		{
			var tex = await FindTextureAsync(ddsFiles, name);
			if (tex is not null)
			{
				scene.Textures.Add(new SharpAssimp.EmbeddedTexture("DDS", tex, name));
			}
		}
	}

	private static async Task<byte[]?> FindTextureAsync(
		IEnumerable<IceDataFile> ddsFiles,
		string name
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

		var data = texture.Data.ToArray();

		// If this is a cast texture generate new 2:1 texture with the data shifted
		if (texture.Name.Contains("_rm_"))
		{
			return await ImageHelper.ShiftCastPartTextureAsync(data, CastTextureShift.Arms);
		}
		if (texture.Name.Contains("_bd_"))
		{
			return await ImageHelper.ShiftCastPartTextureAsync(data, CastTextureShift.Body);
		}
		if (texture.Name.Contains("_lg_"))
		{
			return await ImageHelper.ShiftCastPartTextureAsync(data, CastTextureShift.Legs);
		}

		return data;
	}
}

public partial class Pso2Importer : Importer
{
	public Windows.UI.Color SkinColor { get; set; } = Windows.UI.Color.FromArgb(255, 245, 196, 186);

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
				material.ColorDiffuse = new Vector4(
					(float)SkinColor.R / 255,
					(float)SkinColor.G / 255,
					(float)SkinColor.B / 255,
					(float)SkinColor.A / 255
				);
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
