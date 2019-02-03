﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using Grisaia.Asmodean;
using Grisaia.Categories;
using Grisaia.Categories.Sprites;
using Grisaia.Mvvm.Model;
using Grisaia.Utils;
using Newtonsoft.Json;

namespace Grisaia.Mvvm.ViewModel {
	public class SpriteImageViewModel : ObservableObject {
		#region Constants

		/// <summary>
		///  The total number of part group part Ids.
		/// </summary>
		private const int PartCount = Categories.Sprites.SpriteSelection.PartCount;

		private static readonly bool CacheHg3s = true;

		#endregion

		#region Fields

		/// <summary>
		///  The sprite selection that determines what parts to draw.
		/// </summary>
		private IReadOnlySpriteSelection selection = new ImmutableSpriteSelection();
		private bool expand = false;
		private bool useOrigin = false;
		private Thickness expandCenter;
		private Point spriteOrigin = new Point();
		private Size spriteSize = new Size();
		//private readonly ISpritePart[] parts = new ISpritePart[PartCount];
		private readonly Hg3[] hg3s = new Hg3[PartCount];
		private IReadOnlyList<ISpritePart> currentParts = Array.AsReadOnly(new ISpritePart[PartCount]);

		#endregion

		#region Properties

		/*/// <summary>
		///  Gets the database for all Grisaia databases.
		/// </summary>
		public GrisaiaModel GrisaiaDatabase { get; }
		/// <summary>
		///  Gets the database for all Grisaia games.
		/// </summary>
		public GameDatabase GameDatabase => GrisaiaDatabase.GameDatabase;
		/// <summary>
		///  Gets the database for all known Grisaia characters.
		/// </summary>
		public CharacterDatabase CharacterDatabase => GrisaiaDatabase.CharacterDatabase;*/
		/// <summary>
		///  Gets the database for all located character sprites.
		/// </summary>
		public SpriteDatabase SpriteDatabase { get; }
		/// <summary>
		///  Gets or sets the sprite selection that determines what parts to draw.
		/// </summary>
		public IReadOnlySpriteSelection SpriteSelection {
			get => selection;
			set {
				if (Set(ref selection, value))
					UpdateSelection();
			}
		}
		/// <summary>
		///  Gets or sets if the image should be expanded to the total size of all sprites combined.
		/// </summary>
		public bool Expand {
			get => expand;
			set {
				if (Set(ref expand, value))
					UpdateMargins(null, null);
			}
		}
		/// <summary>
		///  Gets or sets if the image is drawn at its origin instead of the top left.
		/// </summary>
		public bool UseOrigin {
			get => useOrigin;
			set => Set(ref useOrigin, value);
		}
		/// <summary>
		///  Gets or sets if the image is drawn at its origin instead of the top left.
		/// </summary>
		public Point SpriteOrigin {
			get => spriteOrigin;
			private set => Set(ref spriteOrigin, value);
		}
		/// <summary>
		///  Gets or sets the total size of the sprite image.
		/// </summary>
		public Size SpriteSize {
			get => spriteSize;
			private set => Set(ref spriteSize, value);
		}
		/// <summary>
		///  Gets or sets the sprite part items for each image.
		/// </summary>
		public ObservableArray<SpritePartViewModelItem> SpriteParts { get; }
			= new ObservableArray<SpritePartViewModelItem>(PartCount);

		/// <summary>
		///  Gets the currently selected sprite parts.
		/// </summary>
		public IReadOnlyList<ISpritePart> CurrentParts {
			get => currentParts;
			private set => Set(ref currentParts, value);
		}

		#endregion

		#region Constructors

		public SpriteImageViewModel(SpriteDatabase spriteDb) {
			SpriteDatabase = spriteDb;
			SpriteDatabase.BuildComplete += OnSpriteDatabaseBuildComplete;
			for (int typeId = 0; typeId < PartCount; typeId++)
				SpriteParts[typeId] = SpritePartViewModelItem.None;
		}

		#endregion

		#region Event Handlers

		private void OnSpriteDatabaseBuildComplete(object sender, EventArgs e) {
			UpdateSelection();
		}

		#endregion

		#region Private Methods

		/// <summary>
		///  Updates the currently drawn sprite parts and the margins for the images.
		/// </summary>
		private void UpdateSelection() {
			GameInfo game = null;
			ISpritePart[] newParts;
			ImageSource[] newImageSources = SpriteParts.Select(p => p.ImageSource).ToArray();
			if (SpriteDatabase == null || SpriteSelection == null)
				newParts = new ISpritePart[PartCount];
			else
				newParts = SpriteDatabase.GetSpriteParts(SpriteSelection, out game, out _);

			for (int i = 0; i < PartCount; i++) {
				if (CurrentParts[i] != newParts[i]) {
					//parts[i] = newParts[i];
					UpdateImage(newParts[i], i, game, newImageSources);
				}
			}

			// SpriteParts observable array is updated here
			UpdateMargins(newParts, newImageSources);
			CurrentParts = Array.AsReadOnly(newParts);
		}
		/// <summary>
		///  Updates the margin boundaries of all the sprite images.
		/// </summary>
		private void UpdateMargins(IReadOnlyList<ISpritePart> parts, IReadOnlyList<ImageSource> imageSources) {
			var usedHg3s = hg3s.Where(h => h != null).Select(h => h.Images[0]);

			Size totalSize = new Size();
			//Thickness expandShrink = new Thickness();
			expandCenter = new Thickness();

			if (usedHg3s.Any()) {
				if (Expand) {
					expandCenter = new Thickness(
						usedHg3s.Max(h => h.CenterLeft),
						usedHg3s.Max(h => h.BaselineTop),
						usedHg3s.Max(h => h.CenterRight),
						usedHg3s.Max(h => h.BaselineBottom));
				}
				else {
					expandCenter = new Thickness(
						usedHg3s.Max(h => h.CenterLeft - h.MarginLeft),
						usedHg3s.Max(h => h.BaselineTop - h.MarginTop),
						usedHg3s.Max(h => h.CenterRight - h.MarginRight),
						usedHg3s.Max(h => h.BaselineBottom - h.MarginBottom));
				}
				totalSize = new Size(expandCenter.Left + expandCenter.Right,
									 expandCenter.Top + expandCenter.Bottom);
				/*if (Expand) {
					expandShrink = new 
				}
				if (!Expand) {
					new Thickness(
						usedHg3s.Max(h => h.CenterLeft + h.MarginLeft),
						usedHg3s.Max(h => h.BaselineTop + h.MarginTop),
						usedHg3s.Max(h => h.CenterRight + h.MarginRight),
						usedHg3s.Max(h => h.BaselineBottom + h.MarginBottom));
				}
				totalSize = new Size(
					usedHg3s.Max(h => h.Images[0].TotalWidth),
					usedHg3s.Max(h => h.Images[0].TotalHeight));
				if (!Expand) {
					expandShrink = new Thickness(
						usedHg3s.Min(h => h.Images[0].MarginLeft),
						usedHg3s.Min(h => h.Images[0].MarginTop),
						usedHg3s.Min(h => totalSize.Width - (h.Images[0].MarginLeft + h.Images[0].Width)),
						usedHg3s.Min(h => totalSize.Height - (h.Images[0].MarginTop + h.Images[0].Height)));
				}*/
			}
			//SpriteSize = new Size(
			//	Math.Round(totalSize.Width - expandShrink.Left - expandShrink.Right),
			//	Math.Round(totalSize.Height - expandShrink.Top - expandShrink.Bottom));
			SpriteSize = totalSize;
			SpriteOrigin = new Point(expandCenter.Left, expandCenter.Top);
			for (int typeId = 0; typeId < PartCount; typeId++) {
				ISpritePart part = (parts != null ? parts[typeId] : CurrentParts[typeId]);
				Hg3Image h = part?.Hg3.Images[0];
				if (part != null) {
					ImageSource source = (imageSources != null ? imageSources[typeId] : SpriteParts[typeId].ImageSource);
					Thickness margin = new Thickness(
						expandCenter.Left   - h.CenterLeft     + h.MarginLeft,
						expandCenter.Top    - h.BaselineTop    + h.MarginTop,
						expandCenter.Right  - h.CenterRight    + h.MarginRight,
						expandCenter.Bottom - h.BaselineBottom + h.MarginBottom);
					/*margin = new Thickness(
						h.MarginLeft - expandShrink.Left,
						h.MarginTop - expandShrink.Top,
						h.MarginRight - expandShrink.Right,
						h.MarginBottom - expandShrink.Bottom);*/
					Size size = new Size(h.Width, h.Height);
					SpriteParts[typeId] = new SpritePartViewModelItem(
						source,
						margin,
						size);
				}
				else {
					SpriteParts[typeId] = SpritePartViewModelItem.None;
				}
			}
		}
		/// <summary>
		///  Updates the image sprite part at the specified index.
		/// </summary>
		/// <param name="typeId">The index of image sprite part to update.</param>
		/// <param name="game">The current game, used to get the cache directory location.</param>
		private void UpdateImage(ISpritePart part, int typeId, GameInfo game, ImageSource[] newImageSources) {
			//ISpritePart part = parts[typeId];
			Hg3 hg3 = part?.Hg3;
			if (part == null) {
				hg3s[typeId] = null;
				newImageSources[typeId] = null;
				return;
			}


			if (part.Hg3 == null || !CacheHg3s) {
				if (ViewModelBase.IsInDesignModeStatic) {
					string json = Embedded.ReadAllText(Embedded.Combine("GrisaiaCategorization.data.dummy", Hg3.GetJsonFileName(part.FileName)));
					hg3 = JsonConvert.DeserializeObject<Hg3>(json);
				}
				else {
					if (!Directory.Exists(game.CachePath))
						Directory.CreateDirectory(game.CachePath);
					// Extract and save the HG-3 if it's not physically cached
					if (!File.Exists(Hg3.GetJsonFilePath(game.CachePath, part.FileName))) {
						var kifintEntry = game.Lookups.Image[part.FileName];
						hg3 = kifintEntry.ExtractHg3AndImages(game.CachePath, false);
						hg3.SaveJsonToDirectory(game.CachePath);
					}
					else {
						hg3 = Hg3.FromJsonDirectory(game.CachePath, part.FileName);
					}
				}
				if (CacheHg3s)
					part.Hg3 = hg3;
			}
			hg3s[typeId] = hg3;

			string cachePath = game.CachePath;

			BitmapImage source = new BitmapImage();
			source.BeginInit();
			if (ViewModelBase.IsInDesignModeStatic)
				source.StreamSource = Embedded.Open(typeof(GrisaiaDatabase).Assembly, Embedded.Combine("GrisaiaCategorization.data.dummy", hg3.GetFrameFileName(0, 0)));
			else
				source.UriSource = new Uri(hg3.GetFrameFilePath(cachePath, 0, 0));
			source.EndInit();

			newImageSources[typeId] = source;
		}

		#endregion
	}
}
