﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Grisaia.Asmodean;
using Grisaia.Categories;
using Grisaia.Categories.Sprites;
using Grisaia.SpriteViewer.Utils;
using Grisaia.SpriteViewer.ViewModel;
using Microsoft.Win32;

namespace Grisaia.SpriteViewer {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class SpriteSelectionWindow : Window {

		/*internal GameDatabase gameDb;
		internal CharacterDatabase charDb;
		internal SpriteDatabase spriteDb;

		private ISpriteCategory game;
		private ISpriteCategory character;
		private ISpriteCategory lighting;
		private ISpriteCategory distance;
		//private SpriteCharacterSize size;
		private ISpriteCategory pose;
		private ISpriteCategory blush;
		//private Dictionary<int, SpritePart> parts = new Dictionary<int, SpritePart>();
		private ComboBox[] comboPart;
		private TextBlock[] labelPart;
		private StackPanel[] panelPart;
		private Image[] imagePart;
		private ISpritePart[] parts;

		private GameInfo currentGame;
		private CharacterInfo currentChar;
		private CharacterSpritePartGroupInfo[] currentGroups;
		private ISpritePart[] currentParts;*/
		private double scale = 1.0;
		private bool centered = true;
		private int currentWidth = 0;
		private int currentHeight = 0;
		private Thickness expandShrink;

		private double savedScale = 1.0;
		private Vector savedNormalizedScroll = new Vector(-1, -1);

		private bool supressEvents = false;

		public SpriteSelectionViewModel ViewModel => (SpriteSelectionViewModel) DataContext;

		public SpriteSelectionWindow() {
			InitializeComponent();
		}

		/*public void Initialize(LoadGameCallback callback) {
			callback("Loading known games and characters...", null, 0);
			string gameDbFile = Path.Combine(AppContext.BaseDirectory, "Games.json");
			string charDbFile = Path.Combine(AppContext.BaseDirectory, "Characters.json");
			gameDb = GameDatabase.FromJsonFile(gameDbFile);
			charDb = CharacterDatabase.FromJsonFile(charDbFile);
			callback("Locating known games", null, 0);
			gameDb.LocateGames();
			gameDb.LoadCache();
			spriteDb = new SpriteDatabase(gameDb, charDb,
				new SpriteCategoryInfo[] {

				},
				new SpriteCategoryInfo[] {

				});
			callback("Categorization Complete!", null, 1);
			Thread.Sleep(200);
			Dispatcher.Invoke(() => {
				comboGame.ItemsSource = spriteDb.SortedGames.Select(g => g.JapaneseName).ToArray();
				game = spriteDb.SortedGames.First();
				character = game.SortedCharacters.First();
				lighting = character.SortedLightings.First();
				distance = lighting.SortedDistances.First();
				pose = distance.SortedPoses.First();
				//size = distance.SortedSizes.First();
				//pose = size.SortedPoses.First();
				blush = pose.SortedBlushes.First();
				comboPart = new ComboBox[8];
				labelPart = new TextBlock[comboPart.Length];
				panelPart = new StackPanel[comboPart.Length];
				imagePart = new Image[12];
				parts = new ISpritePart[imagePart.Length];
				currentParts = new ISpritePart[imagePart.Length];
				for (int i = 0; i < comboPart.Length; i++) {
					comboPart[i] = (ComboBox) FindName("comboPart" + i);
					labelPart[i] = (TextBlock) FindName("labelPart" + i);
					panelPart[i] = (StackPanel) FindName("panelPart" + i);
				}
				for (int i = 0; i < imagePart.Length; i++) {
					imagePart[i] = (Image) FindName("imagePart" + i);
					imagePart[i].Source = null;
				}
			});
#if DEBUG
			spriteDb.TraceUncategorizedSprites();
#endif
		}*/

		private void OnLoaded(object sender, RoutedEventArgs e) {
			//comboGame.SelectedIndex = 0;
			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
			OnViewModelPropertyChanged(null, new PropertyChangedEventArgs(nameof(SpriteSelectionViewModel.CurrentParts)));
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName == nameof(SpriteSelectionViewModel.CurrentParts)) {
				UpdatePartChanges();
			}
		}

		/*public ISpritePart[] GetGroupParts(CharacterSpritePartGroup group) {
			ISpritePart[] groupParts = new ISpritePart[group.TypeIds.Length];
			for (int i = 0; i < groupParts.Length; i++) {
				int typeId = group.TypeIds[i];
				if (parts[typeId] != null)
					groupParts[i] = parts[typeId];
			}
			return groupParts;
		}
		public int[] GetGroupPartIds(CharacterSpritePartGroup group) {
			int[] groupPartIds = new int[group.TypeIds.Length];
			for (int i = 0; i < group.TypeIds.Length; i++) {
				int typeId = group.TypeIds[i];
				if (parts[typeId] != null)
					groupPartIds[i] = parts[typeId].Id;
				else
					groupPartIds[i] = -1;
			}
			return groupPartIds;
		}*/

		private void UpdatePartChanges() {
			Vector normalized = CalculateNormalizedScrollCenter();
			/*for (int typeId = 0; typeId < DataContext.Cur; typeId++) {
				var part = parts[typeId];
				var image = imagePart[typeId];
				if (currentParts[typeId] != part) {
					currentParts[typeId] = part;
					SetImage(image, part);
				}
			}*/
			//var usedParts = parts.Where(p => p != null);
			UpdateExpand();
			if (centered) {
				UpdateCentered();
			}
			else {
				Vector areaCenter = CalculateAreaCenter();
				Vector newScroll = new Vector(
					normalized.X * areaCenter.X,
					normalized.Y * areaCenter.Y);
				transform.ScaleX = scale;
				transform.ScaleY = scale;
				scrollSprite.ScrollToHorizontalOffset(newScroll.X - scrollSprite.ViewportWidth / 2);
				scrollSprite.ScrollToVerticalOffset(newScroll.Y - scrollSprite.ViewportHeight / 2);
			}
			NewLines();
			UpdatePartList();
			UpdateStatusBar();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}

		private void UpdateExpand() {
			var usedParts = ViewModel.CurrentParts.Where(p => p != null);
			if (usedParts.Any()) {
				Size totalSize = new Size(
					usedParts.Max(p => p.Hg3.Images[0].TotalWidth),
					usedParts.Max(p => p.Hg3.Images[0].TotalHeight));
				expandShrink = new Thickness();
				if (!menuItemExpand.IsChecked) {
					expandShrink = new Thickness(
						usedParts.Min(p => p.Hg3.Images[0].MarginLeft),
						usedParts.Min(p => p.Hg3.Images[0].MarginTop),
						usedParts.Min(p => totalSize.Width - (p.Hg3.Images[0].MarginLeft + p.Hg3.Images[0].Width)),
						usedParts.Min(p => totalSize.Height - (p.Hg3.Images[0].MarginTop + p.Hg3.Images[0].Height)));
				}
				/*for (int typeId = 0; typeId < parts.Length; typeId++) {
					var part = ViewModel.CurrentParts[typeId];
					var image = imagePart[typeId];
					if (part != null) {
						var c = part.Hg3.Images[0];
						image.Width = c.Width;
						image.Height = c.Height;
						image.Margin = new Thickness(
							c.MarginLeft - expandShrink.Left,
							c.MarginTop - expandShrink.Top,
							c.MarginRight - expandShrink.Right,
							c.MarginBottom - expandShrink.Bottom);
					}
				}*/
				currentWidth = (int) Math.Round(totalSize.Width - expandShrink.Left - expandShrink.Right);
				currentHeight = (int) Math.Round(totalSize.Height - expandShrink.Top - expandShrink.Bottom);
			}
			else {
				currentWidth = 1;
				currentHeight = 1;
			}
			//gridSprite.Width = currentWidth;
			//gridSprite.Height = currentHeight;
			gridSprite.Margin = CalculateAreaMargins();
			gridLines.Margin = gridSprite.Margin;
		}

		/*private void SetImage(Image image, ISpritePart part) {
			if (part == null) {
				image.Source = null;
				image.Visibility = Visibility.Collapsed;
				return;
			}
			string outDir = Path.Combine(AppContext.BaseDirectory, "cache", currentGame.Id);
			string pngFile = Path.Combine(outDir, part.PngFile);
			string jsonFile = Path.Combine(outDir, part.JsonFile);
			if (!File.Exists(pngFile) || !File.Exists(pngFile)) {
				if (!Directory.Exists(outDir))
					Directory.CreateDirectory(outDir);
				part.Hg3 = Kifint.ExtractHg3AndImages(currentGame.ImageLookup[part.FileName], outDir, false);
				part.Hg3.SaveJsonToDirectory(gameDb.CachePath);
				//part.Cached = Exkifint.ExtractHgx(currentGame.ImageLookup[part.FileName + ".hg3"], outDir);
				//File.WriteAllText(jsonFile, JsonConvert.SerializeObject(part.Cached, Formatting.Indented));
			}
			else if (part.Hg3 == null) {
				part.Hg3 = Hg3.FromJsonDirectory(gameDb.CachePath, part.FileName);
			}

			BitmapImage source = new BitmapImage();
			source.BeginInit();
			source.UriSource = new Uri(pngFile);
			source.EndInit();

			var c = part.Hg3.Images[0];
			image.Source = source;
			image.Width = c.Width;
			image.Height = c.Height;
			image.Margin = new Thickness(c.MarginLeft, c.MarginTop, c.MarginRight, c.MarginBottom);
			image.Visibility = Visibility.Visible;
		}*/

		/*private void UpdateGameChanges() {
			UpdateSelection(comboCharacter, game.Characters, game.SortedCharacters, ref character);
			currentGame = gameDb.Get((string) game.Id);
			UpdateCharacterChanges();
		}
		private void UpdateCharacterChanges() {
			UpdateSelection(comboLighting, character.Lightings, character.SortedLightings, ref lighting);
			currentChar = charDb.Get((string) character.Id);
			currentGroups = charDb.GetPartGroup(currentGame, currentChar);

			// Empty parts
			for (int i = 0; i < parts.Length; i++)
				parts[i] = null;

			for (int i = 0; i < comboPart.Length; i++) {
				if (i < currentGroups.Length) {
					labelPart[i].Text = currentGroups[i].Name;
					//comboPart[0].ItemsSource = new string[] { "(none)" };
					panelPart[i].Visibility = Visibility.Visible;
				}
				else {
					panelPart[i].Visibility = Visibility.Collapsed;
				}
			}

			UpdateLightingChanges();
		}
		private void UpdateLightingChanges() {
			UpdateSelection(comboDistance, lighting.Distances, lighting.SortedDistances, ref distance);
			UpdateDistanceChanges();
		}
		private void UpdateDistanceChanges() {
			UpdateSelection(comboPose, distance.Poses, distance.SortedPoses, ref pose);
			UpdatePoseChanges();
		}*/
		/*private void UpdateSizeChanges() {
			UpdateSelection(comboPose, size.Poses, size.SortedPoses, ref pose);
			UpdatePoseChanges();
		}*/
		/*private void UpdatePoseChanges() {
			UpdateSelection(comboBlush, pose.Blushes, pose.SortedBlushes, ref blush);
			UpdateBlushChanges();
		}
		private void UpdateBlushChanges() {
			//HashSet<int> removedParts = new HashSet<int>();
			//for (int typeId = 0; typeId < parts.Length; typeId++)
			//	removedParts.Add(typeId);
			for (int i = 0; i < currentGroups.Length; i++) {
				UpdateSelectionPartGroup(comboPart[i], currentGroups[i]);
				//for (int j = 0; j < currentGroups[i].TypeIds.Length; j++)
				//	removedParts.Remove(currentGroups[i].TypeIds[j]);
			}
			//foreach (int typeId in removedParts)
			//	parts[typeId] = null;
			UpdatePartChanges();
		}

		private void UpdateSelectionPartGroup(ComboBox combo, CharacterSpritePartGroup group) {
			HashSet<int> combinedAvailableIds = new HashSet<int>();
			foreach (int id in group.TypeIds) {
				if (((ISpritePartListContainer) blush).TryGetValue(id, out var partList)) {
					for (int i = 0; i < blush.Count; i++) {
						ISpritePartList partList = ((ISpritePartListContainer) blush)[i];
					}
					foreach (int partIndex in ptypes.SortedParts.Select(p => p.Id))
						combinedAvailableIds.Add(partIndex);
				}
			}
			List<int> list = new List<int>(combinedAvailableIds);
			list.Sort();
			combo.ItemsSource = new string[] { "(none)" }.Concat(list.Select(i => i.ToString())).ToArray();
			//int foundId = -1;
			HashSet<int> leftoverIds = new HashSet<int>(group.TypeIds);
			//foreach (int typeId in group.TypeIds) {
			//	if (parts[typeId] != null && blush.PartTypes.TryGetValue(typeId, out var ptypes)) {
			//		if (ptypes.Parts.TryGetValue(parts[typeId].Id, out var newPart)) {
			//			foundId = parts[typeId].Id;
			//			parts[typeId] = newPart;
			//		}
			//	}
			//}
			int partId = parts
				.Where((p, i) => group.TypeIds.Any(typeId => typeId == i))
				.FirstOrDefault()?.Id ?? -1;
			if (blush.TryGetPartTypes(group, partId, out var newParts)) {
				for (int groupIndex = 0; groupIndex < group.TypeIds.Length; groupIndex++) {
					int typeId = group.TypeIds[groupIndex];
					parts[typeId] = newParts[groupIndex];
				}
			}
			else {
			//if (foundId == -1) {
				//var relatedParts = blush.SortedPartTypes.Where(ptypes => group.Ids.Any(id => id == ptypes.Id));
				if (blush.TryGetFirstPartTypes(group, out partId, out newParts)) {
					combo.IsEnabled = true;
					if (!group.Enabled) {
						combo.SelectedIndex = 0; // (none)
						foreach (int typeId in group.TypeIds) {
							parts[typeId] = null;
						}
					}
					else {
						combo.SelectedIndex = list.IndexOf(partId) + 1; // Skip (none)
						for (int groupIndex = 0; groupIndex < newParts.Length; groupIndex++) {
							int typeId = group.TypeIds[groupIndex];
							if (newParts[groupIndex] != null) {
								parts[typeId] = newParts[groupIndex];
							}
							else {
								parts[typeId] = null;
							}
						}
					}
				}
				else {
					combo.SelectedIndex = 0;
					combo.IsEnabled = false;
					foreach (int typeId in group.TypeIds)
						parts[typeId] = null;
				}
				//if (!relatedParts.Any()) {
				//	combo.SelectedIndex = 0;
				//	combo.IsEnabled = false;
				//}
				//else {
				//	combo.IsEnabled = true;
				//	if (!group.Enabled) {
				//		combo.SelectedIndex = 0;
				//		foreach (int id in group.Ids) {
				//			if (parts.ContainsKey(id))
				//				parts.Remove(id);
				//		}
				//	}
				//	else {
				//		int firstId = relatedParts.Min(ptypes => ptypes.SortedParts.FirstOrDefault()?.);
				//		combo.SelectedIndex = list.IndexOf(firstId);
				//		foreach (int id in group.Ids) {
				//			if (blush.PartTypes.TryGetValue(id, out var ptypes) && ptypes.Parts.TryGetValue(firstId, out var newPart)) {
				//				parts[id] = newPart;
				//			}
				//			else if (parts.ContainsKey(id)) {
				//				parts.Remove(id);
				//			}
				//		}
				//	}
				//}
			}
		}
		private void UpdateSelection<TKey, TValue>(ComboBox combo, Dictionary<TKey, TValue> dic, List<TValue> list, ref TValue value)
			where TValue : class, IKey<TKey>
		{
			TKey key = default(TKey);
			if (value != null)
				key = value.Id;
			combo.ItemsSource = list.Select(v => v.Id).ToArray();
			if (!dic.TryGetValue(key, out value))
				value = list.FirstOrDefault();
			combo.SelectedIndex = value?.Index ?? -1;
		}

		private void OnGameChanged(object sender, SelectionChangedEventArgs e) {
			if (supressEvents) return;
			if (comboGame.SelectedIndex == -1) return;
			game = spriteDb.SortedGames[comboGame.SelectedIndex];
			supressEvents = true;
			UpdateGameChanges();
			supressEvents = false;
		}

		private void OnCharacterChanged(object sender, SelectionChangedEventArgs e) {
			if (supressEvents) return;
			if (comboCharacter.SelectedIndex == -1) return;
			character = game.SortedCharacters[comboCharacter.SelectedIndex];
			supressEvents = true;
			UpdateCharacterChanges();
			supressEvents = false;
		}

		private void OnLightingChanged(object sender, SelectionChangedEventArgs e) {
			if (supressEvents) return;
			if (comboLighting.SelectedIndex == -1) return;
			lighting = character.SortedLightings[comboLighting.SelectedIndex];
			supressEvents = true;
			UpdateLightingChanges();
			supressEvents = false;
		}

		private void OnDistanceChanged(object sender, SelectionChangedEventArgs e) {
			if (supressEvents) return;
			if (comboDistance.SelectedIndex == -1) return;
			distance = lighting.SortedDistances[comboDistance.SelectedIndex];
			supressEvents = true;
			UpdateDistanceChanges();
			supressEvents = false;
		}

		//private void OnSizeChanged(object sender, SelectionChangedEventArgs e) {
		//	if (supressEvents) return;
		//	if (comboSize.SelectedIndex == -1) return;
		//	size = distance.SortedSizes[comboSize.SelectedIndex];
		//	supressEvents = true;
		//	UpdateSizeChanges();
		//	supressEvents = false;
		//}

		private void OnPoseChanged(object sender, SelectionChangedEventArgs e) {
			if (supressEvents) return;
			if (comboPose.SelectedIndex == -1) return;
			pose = distance.SortedPoses[comboPose.SelectedIndex];
			//pose = size.SortedPoses[comboPose.SelectedIndex];
			supressEvents = true;
			UpdatePoseChanges();
			supressEvents = false;
		}

		private void OnBlushChanged(object sender, SelectionChangedEventArgs e) {
			if (supressEvents) return;
			if (comboBlush.SelectedIndex == -1) return;
			blush = pose.SortedBlushes[comboBlush.SelectedIndex];
			supressEvents = true;
			UpdateBlushChanges();
			supressEvents = false;
		}*/
		/*private void OnPartChanged(object sender, SelectionChangedEventArgs e) {
			/*ComboBox box = (ComboBox) sender;
			if (supressEvents) return;
			string partStr = box.Name.Substring("comboPart".Length);
			int groupId = int.Parse(partStr);
			if (box.SelectedIndex <= 0) {
				foreach (int typeId in currentGroups[groupId].TypeIds) {
					parts[typeId] = null;
					//SetImage(imagePart[typeId], null);
				}
				//UpdateStatusBar();
				//UpdatePartList();
				//return;
			}
			else {
				foreach (int typeId in currentGroups[groupId].TypeIds) {
					int partId = int.Parse((string) box.SelectedItem);
					if (blush.PartTypes.TryGetValue(typeId, out var ptypes) && ptypes.Parts.TryGetValue(partId, out var newPart)) {
						parts[typeId] = newPart;
						//SetImage(imagePart[typeId], newPart);
					}
					else {
						parts[typeId] = null;
						//SetImage(imagePart[typeId], null);
					}
				}
			}
			UpdatePartChanges();
			UpdateStatusBar();
			UpdatePartList();
		}*/

		private void UpdatePartList() {
			labelPartList.Text =
				string.Join("\n", ViewModel.CurrentParts
				.Where(p => p != null)
				.Select(p => Path.GetFileNameWithoutExtension(p.FileName)));
		}

		private void OnSaveSprite(object sender, RoutedEventArgs e) {
			var usedParts = ViewModel.CurrentParts.Where(p => p != null);
			if (usedParts.Any()) {
				SaveFileDialog dialog = new SaveFileDialog {
					FileName = GetSpriteUniqueFileName(),
					Filter = "PNG Images|*.png",
					OverwritePrompt = true,
					AddExtension = true,
					InitialDirectory = Path.Combine(AppContext.BaseDirectory, "saved"),
				};
				if (!Directory.Exists(dialog.InitialDirectory))
					Directory.CreateDirectory(dialog.InitialDirectory);
				bool result = dialog.ShowDialog() ?? false;
				if (!result)
					return;
				int width = usedParts.Max(p => p.Hg3.Images[0].TotalWidth);
				int height = usedParts.Max(p => p.Hg3.Images[0].TotalHeight);
				using (var bitmap = BuildImage())
					bitmap.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
			}
		}

		private void OnCopySprite(object sender, RoutedEventArgs e) {
			var usedParts = ViewModel.CurrentParts.Where(p => p != null);
			if (usedParts.Any()) {
				using (var bitmap = BuildImage())
				using (var bitmapNoTr = RemoveTransparency(bitmap)) {
					string tempPath = Path.Combine(AppContext.BaseDirectory, "cache", "clipboard.png");
					bitmap.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
					ImageClipboard.SetClipboardImage(bitmap, bitmapNoTr, tempPath);
				}
			}
		}

		private System.Drawing.Bitmap RemoveTransparency(System.Drawing.Bitmap bitmap) {
			var newBitmap = new System.Drawing.Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			try {
				using (var g = System.Drawing.Graphics.FromImage(newBitmap)) {
					g.Clear(System.Drawing.Color.White);
					g.DrawImageUnscaled(bitmap, 0, 0);
				}
				return newBitmap;
			}
			catch {
				newBitmap.Dispose();
				throw;
			}
		}

		private System.Drawing.Bitmap BuildImage() {
			var usedParts = ViewModel.CurrentParts.Where(p => p != null);
			string dir = Path.Combine(AppContext.BaseDirectory, "cache", ViewModel.CurrentGame.Id);
			var bitmap = new System.Drawing.Bitmap(currentWidth, currentHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			try {
				using (var g = System.Drawing.Graphics.FromImage(bitmap)) {
					g.Clear(System.Drawing.Color.Transparent);
					foreach (var part in usedParts) {
						string partFile = Path.Combine(dir, part.Hg3.GetFrameFileName(0, 0));
						using (var partBitmap = System.Drawing.Image.FromFile(partFile))
							g.DrawImageUnscaled(partBitmap,
								part.Hg3.Images[0].MarginLeft - (int) Math.Round(expandShrink.Left),
								part.Hg3.Images[0].MarginTop - (int) Math.Round(expandShrink.Top));
					}
				}
				return bitmap;
			}
			catch {
				// Only dispose on exception
				bitmap.Dispose();
				throw;
			}
		}

		private string GetSpriteUniqueId() {
			StringBuilder str = new StringBuilder();
			var selection = ViewModel.SpriteSelection;
			str.Append(selection.GameId);
			str.Append("-");
			str.Append(selection.CharacterId);
			str.Append("__L");
			str.Append((int) selection.Lighting);
			str.Append("-D");
			str.Append((int) selection.Distance);
			str.Append("-P");
			str.Append((int) selection.Pose);
			str.Append("-B");
			str.Append((int) selection.Blush);

			str.Append("__");
			str.Append(string.Join("-", ViewModel.CurrentParts.Select((p, i) => (p != null ? $"{i}P{p.Id:D2}" : null)).Where(p => p != null)));
			return str.ToString();
		}
		private string GetSpriteUniqueFileName() {
			return GetSpriteUniqueId() + ".png";
		}

		private void UpdateStatusBar() {
			statusItemDimensions.Content = $"{currentWidth}x{currentHeight}";
			statusItemScale.Content = $"{scale:P0}";
			statusItemUniqueId.Content = GetSpriteUniqueId();
		}

		private void OnZoomImage(object sender, System.Windows.Input.MouseWheelEventArgs e) {
			if (Keyboard.Modifiers!= ModifierKeys.Control)
				return;
			if (centered) {
				centered = false;
				scrollSprite.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
				scrollSprite.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
			}

			Point gridPoint = e.GetPosition(gridSprite);
			Point scrollPoint = e.GetPosition(scrollSprite);

			//Vector beforeOffset = CalculateAreaOffset();
			Vector beforeCenter = new Vector(
				scrollPoint.X - scrollSprite.ViewportWidth  / 2,
				scrollPoint.Y - scrollSprite.ViewportHeight / 2);
			//Vector beforeArea = new Vector(gridPoint.X, gridPoint.Y) + beforeOffset;

			double scaleChange = e.Delta > 0 ? 1.125 : 1 / 1.125;
			// 1% scale is the minimum
			if (scale == 0.01 && scaleChange < 1.0) {
				e.Handled = true;
				return;
			}
			double oldScale = scale;
			scale *= scaleChange;
			if (scale < 0.01) {
				scale = 0.01;
				scaleChange = scale / oldScale;
			}
			else if ((oldScale < 1 && scale > 1) || (oldScale > 1 && scale < 1)) {
				scale = 1;
				scaleChange = scale / oldScale;
			}

			Vector afterOffset = CalculateAreaOffset();
			//Vector afterCenter = beforeCenter * scaleChange;
			Vector afterArea = new Vector(gridPoint.X, gridPoint.Y) * scale + afterOffset;

			transform.ScaleX = scale;
			transform.ScaleY = scale;

			gridSprite.Margin = CalculateAreaMargins();
			gridLines.Margin = gridSprite.Margin;

			//Vector centerChange = afterCenter - beforeCenter;
			Vector newScroll = afterArea - beforeCenter;// - afterCenter + centerChange;
			scrollSprite.ScrollToHorizontalOffset(newScroll.X - scrollSprite.ViewportWidth / 2);
			scrollSprite.ScrollToVerticalOffset(newScroll.Y - scrollSprite.ViewportHeight / 2);

			UpdateStatusBar();
			e.Handled = true;
		}

		private void OnPreviewKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.B && Keyboard.Modifiers == ModifierKeys.Control) {
				Vector normalizedScroll = CalculateNormalizedScrollCenter();
				centered = !centered;
				Vector newScroll = CalculateAreaCenter();
				if (centered) {
					savedScale = scale;
					savedNormalizedScroll = normalizedScroll;
					scrollSprite.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
					scrollSprite.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
					UpdateCentered();
				}
				else {
					scale = savedScale;
					scrollSprite.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
					scrollSprite.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
					transform.ScaleX = scale;
					transform.ScaleY = scale;
					gridSprite.Margin = CalculateAreaMargins();
					gridLines.Margin = gridSprite.Margin;
					if (savedNormalizedScroll.X >= 0 && savedNormalizedScroll.Y >= 0) {
						Vector areaCenter = CalculateAreaCenter();
						newScroll.X = savedNormalizedScroll.X * areaCenter.X;
						newScroll.Y = savedNormalizedScroll.Y * areaCenter.Y;
					}
					scrollSprite.ScrollToHorizontalOffset(newScroll.X - scrollSprite.ViewportWidth / 2);
					scrollSprite.ScrollToVerticalOffset(newScroll.Y - scrollSprite.ViewportHeight / 2);
					UpdateStatusBar();
				}
			}
		}

		private Vector CalculateAreaCenter() {
			return new Vector(currentWidth, currentHeight) * scale / 2 + CalculateAreaOffset();
		}
		private Vector CalculateScrollCenter() {
			return new Vector(
				scrollSprite.HorizontalOffset + scrollSprite.ViewportWidth / 2,
				scrollSprite.VerticalOffset + scrollSprite.ViewportHeight / 2);
		}
		private Vector CalculateNormalizedScrollCenter() {
			Vector areaCenter = CalculateAreaCenter();
			Vector scrollCenter = CalculateScrollCenter();
			return new Vector(
				scrollCenter.X / areaCenter.X,
				scrollCenter.Y / areaCenter.Y);
		}


		private void UpdateCentered() {
			if (currentWidth == 0 || currentHeight == 0) {
				scale = 1;
				transform.ScaleX = scale;
				transform.ScaleY = scale;
			}
			else {
				Vector areaOffset = CalculateAreaOffset();
				Vector area = new Vector(
					scrollSprite.ViewportWidth,
					scrollSprite.ViewportHeight) - areaOffset * 2;
				//Thickness margin = 
				double areaRatio = area.X / area.Y;
				double spriteRatio = (double) currentWidth / currentHeight;
				if (areaRatio > spriteRatio) {
					scale = Math.Min(1, area.Y / currentHeight);
				}
				else {
					scale = Math.Min(1, area.X / currentWidth);
				}
				areaOffset.X = (scrollSprite.ViewportWidth - currentWidth * scale) / 2;
				areaOffset.Y = (scrollSprite.ViewportHeight - currentHeight * scale) / 2;
				gridSprite.Margin = new Thickness(areaOffset.X, areaOffset.Y, areaOffset.X, areaOffset.Y);
				gridLines.Margin = gridSprite.Margin;
				transform.ScaleX = scale;
				transform.ScaleY = scale;
			}
			UpdateStatusBar();
		}

		private void OnScrollSizeChanged(object sender, SizeChangedEventArgs e) {
		}

		private void OnScrollChanged(object sender, ScrollChangedEventArgs e) {
			Thickness margin = new Thickness();
			if (scrollSprite.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
				margin.Right = 1;
			if (scrollSprite.ComputedVerticalScrollBarVisibility == Visibility.Visible)
				margin.Bottom = 1;
			scrollSprite.Padding = margin;
			scrollSprite.Margin = margin;
			gridSprite.Margin = CalculateAreaMargins();
			gridLines.Margin = gridSprite.Margin;
			if (centered)
				UpdateCentered();
			UpdateLines();
		}
		
		private Vector CalculateAreaOffset() {
			if (centered)
				return new Vector(15, 15);
			double viewWidth  = scrollSprite.ViewportWidth;
			double viewHeight = scrollSprite.ViewportHeight;
			double scaledWidth  = currentWidth  * scale;
			double scaledHeight = currentHeight * scale;
			double x, y;
			if (scaledWidth > viewWidth)
				x = viewWidth / 2;
			else
				x = viewWidth - scaledWidth / 2;
			if (scaledHeight > viewHeight)
				y = viewHeight / 2;
			else
				y = viewHeight - scaledHeight / 2;
			//horizontal /= scale;
			//vertical /= scale;
			return new Vector(x, y);
		}
		private Thickness CalculateAreaMargins() {
			var offset = CalculateAreaOffset();
			return new Thickness(offset.X, offset.Y, offset.X, offset.Y);
		}

		private void OnExpandChanged(object sender, RoutedEventArgs e) {
			UpdatePartChanges();
		}

		private void OnShowLinesChanged(object sender, RoutedEventArgs e) {
			NewLines();
		}

		private void NewLines() {
			gridLines.Children.Clear();
			if (menuItemShowGuidelines.IsChecked) {
				HashSet<int> centers = new HashSet<int>();
				HashSet<int> baselines = new HashSet<int>();
				foreach (var part in ViewModel.CurrentParts.Where(p => p != null)) {
					var c = part.Hg3.Images[0];
					if (centers.Add(c.Center)) {
						System.Windows.Shapes.Rectangle r = new System.Windows.Shapes.Rectangle {
							Fill = Brushes.Red,
							HorizontalAlignment = HorizontalAlignment.Left,
							Width = 1,
						};
						gridLines.Children.Add(r);
					}
					if (baselines.Add(c.Baseline)) {
						System.Windows.Shapes.Rectangle r = new System.Windows.Shapes.Rectangle {
							Fill = Brushes.Blue,
							VerticalAlignment = VerticalAlignment.Top,
							Height = 1,
						};
						gridLines.Children.Add(r);
					}
				}
			}
			UpdateLines();
		}

		private void UpdateLines() {
			if (menuItemShowGuidelines.IsChecked) {
				HashSet<int> centers = new HashSet<int>();
				HashSet<int> baselines = new HashSet<int>();
				int index = 0;
				foreach (var part in ViewModel.CurrentParts.Where(p => p != null)) {
					var c = part.Hg3.Images[0];
					if (centers.Add(c.Center)) {
						System.Windows.Shapes.Rectangle r = (System.Windows.Shapes.Rectangle) gridLines.Children[index++];
						r.Margin = new Thickness((c.Center - expandShrink.Left) * scale, 0, 0, 0);
					}
					if (baselines.Add(c.Baseline)) {
						System.Windows.Shapes.Rectangle r = (System.Windows.Shapes.Rectangle) gridLines.Children[index++];
						r.Margin = new Thickness(0, (c.Baseline - expandShrink.Top) * scale, 0, 0);
					}
				}
			}
		}

		private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e) {
			Console.WriteLine("-------------------------------------");
		}
	}
}