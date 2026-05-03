using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HelixToolkit.Maths;
using HelixToolkit.SharpDX;
using HelixToolkit.SharpDX.Assimp;
using HelixToolkit.SharpDX.Model.Scene;
using HelixToolkit.WinUI.SharpDX;
using SixLabors.ImageSharp;

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
	public partial SceneNode? SceneRoot { get; set; }

	[ObservableProperty]
	public partial SceneNode? SelectedNode { get; set; }

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

	private IceDataFile? file = null;
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
	private async Task LoadModel(IceDataFile file)
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
			AqpFile = file,
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

			SceneRoot = scene.Root;
			Root.Clear();
			Root.AddNode(SceneRoot);

			AttachedNodeViewModel.AttachToTree(SceneRoot);

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
		return mainViewModel
			.FilesTyped.FirstOrDefault(file =>
				file.Name.EndsWith(".aqn") || file.Name.EndsWith(".trn")
			)
			?.File;
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

public partial class AttachedNodeViewModel(SceneNode node) : ObservableObject
{
	public static void AttachToTree(SceneNode node)
	{
		foreach (var child in node.Items.Traverse())
		{
			child.Tag = new AttachedNodeViewModel(child);
		}
	}

	public static void SetHighlighted(SceneNode node, bool value)
	{
		if (node.Tag is AttachedNodeViewModel vm)
		{
			vm.IsHighlighted = value;
		}
	}

	public static void ExpandNode(SceneNode node)
	{
		if (node.Tag is AttachedNodeViewModel vm)
		{
			vm.IsExpanded = true;

			if (node.Parent is not null)
			{
				ExpandNode(node.Parent);
			}
		}
	}

	[ObservableProperty]
	public partial bool IsHighlighted { get; set; } = false;

	[ObservableProperty]
	public partial bool IsExpanded { get; set; } = false;

	private readonly SceneNode node = node;

	partial void OnIsHighlightedChanged(bool value)
	{
		switch (node)
		{
			case GeometryNode geo:
				geo.PostEffects = value ? $"highlight[color:#FFFF00]" : "";
				break;

			case GroupNode group:
				foreach (var child in group.Items)
				{
					SetHighlighted(child, value);
				}
				break;
		}
	}
}
