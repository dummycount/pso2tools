using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.WinUI.Behaviors;
using CommunityToolkit.WinUI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Pso2Tools.Defrost.ViewModels;
using Pso2Tools.Defrost.ViewModels.Preview;
using Windows.Graphics.Imaging;
using Image = SixLabors.ImageSharp.Image;

namespace Pso2Tools.Defrost.Views.Preview;

public sealed partial class PreviewPageDds : Page
{
	private readonly NotificationService notificationService;
	private readonly ISettingsService settings;
	private readonly PreviewModelDds viewModel;

	private IceDataFile? file;
	private Image? baseImage;
	private readonly Dictionary<ImagePreviewMode, SoftwareBitmap> cachedBitmaps = [];

	public PreviewPageDds()
	{
		notificationService = App.Current.Services.GetRequiredService<NotificationService>();
		viewModel = App.Current.Services.GetRequiredService<PreviewModelDds>();
		settings = App.Current.Services.GetRequiredService<ISettingsService>();
		settings.PropertyChanged += Settings_PropertyChanged;

		InitializeComponent();

		UpdateVisualState();
	}

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		base.OnNavigatedTo(e);

		if (e.Parameter is IceDataFile file)
		{
			this.file = file;
			await UpdateImageSourceAsync();
		}
	}

	protected override void OnNavigatedFrom(NavigationEventArgs e)
	{
		base.OnNavigatedFrom(e);

		settings.PropertyChanged -= Settings_PropertyChanged;
		TextureImage.Source = null;

		baseImage?.Dispose();
		baseImage = null;

		foreach (var bitmap in cachedBitmaps.Values)
		{
			bitmap.Dispose();
		}

		cachedBitmaps.Clear();
	}

	private void UpdateVisualState()
	{
		VisualStateManager.GoToState(this, ActualWidth < 360 ? "Collapsed" : "Expanded", true);
	}

	private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		UpdateVisualState();
	}

	private async void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(settings.ImagePreviewMode):
				await UpdateImageSourceAsync();
				break;
		}
	}

	private async Task<Image?> LoadBaseImageAsync(byte[] data)
	{
		try
		{
			return await ImageHelper.DdsBufferToImageAsync(data);
		}
		catch (Exception ex)
		{
			notificationService.ShowNotification(
				new Notification
				{
					Title = "Failed to load image",
					Message = ex.Message,
					Duration = TimeSpan.FromSeconds(10),
				}
			);
			return null;
		}
	}

	private async Task<SoftwareBitmap> LoadPreviewImageAsync()
	{
		ArgumentNullException.ThrowIfNull(baseImage);

		var mode = settings.ImagePreviewMode;

		if (cachedBitmaps.TryGetValue(mode, out var bitmap))
		{
			return bitmap;
		}

		var image = baseImage;

		bitmap = await Task.Run(() =>
			ImageHelper.ImageToSoftwareBitmap(ImageHelper.ApplyImagePreviewMode(image, mode))
		);

		cachedBitmaps[mode] = bitmap;
		return bitmap;
	}

	private async Task UpdateImageSourceAsync()
	{
		if (file is null)
		{
			TextureImage.Source = null;
			return;
		}

		try
		{
			viewModel.IsLoading = true;

			baseImage ??= await LoadBaseImageAsync(file.Data.ToArray());
			var image = await LoadPreviewImageAsync();

			var source = new SoftwareBitmapSource();
			await source.SetBitmapAsync(image);

			TextureImage.Width = image.PixelWidth;
			TextureImage.Height = image.PixelHeight;
			TextureImage.Source = source;

			ZoomToFit(ScrollingAnimationMode.Disabled);
		}
		catch (Exception ex)
		{
			notificationService.ShowNotification(
				new Notification
				{
					Title = "Failed to load image",
					Message = ex.Message,
					Duration = TimeSpan.FromSeconds(10),
				}
			);
		}
		finally
		{
			viewModel.IsLoading = false;
		}
	}

	private void ZoomToFit(ScrollingAnimationMode animation = ScrollingAnimationMode.Auto)
	{
		// Add a small margin to avoid there being a small amount of scrolling.
		double margin = 2;

		var scale = (float)
			Math.Min(
				(ScrollView.ActualWidth - margin) / TextureImage.Width,
				(ScrollView.ActualHeight - margin) / TextureImage.Height
			);

		ScrollView.ZoomTo(scale, null, new ScrollingZoomOptions(animation));
	}

	Windows.UI.Color darkColor = Windows.UI.Color.FromArgb(255, 42, 42, 42);
	Windows.UI.Color lightColor = Windows.UI.Color.FromArgb(255, 54, 54, 54);

	private void CanvasControl_Draw(
		Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender,
		Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args
	)
	{
		using var list = new CanvasCommandList(sender);
		using var session = list.CreateDrawingSession();

		session.FillRectangle(0, 0, 8, 8, darkColor);
		session.FillRectangle(8, 0, 8, 8, lightColor);

		session.FillRectangle(0, 8, 8, 8, lightColor);
		session.FillRectangle(8, 8, 8, 8, darkColor);

		using var tile = new TileEffect();
		tile.Source = list;

		tile.SourceRectangle = new Windows.Foundation.Rect(0, 0, 16, 16);

		args.DrawingSession.DrawImage(tile);
	}

	private void ColorChannels_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is Segmented control && control.SelectedValue is ImagePreviewMode mode)
		{
			settings.ImagePreviewMode = mode;
		}
	}

	private void ZoomToFitButton_Click(object sender, RoutedEventArgs e)
	{
		ZoomToFit();
	}

	private void ZoomActualSize_Click(object sender, RoutedEventArgs e)
	{
		ScrollView.ZoomTo(1, null);
	}
}
