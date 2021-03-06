﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Grisaia.Categories.Sprites {
	/// <summary>
	///  The generic implementation for all sprite elements contained by <see cref="ISpriteCategory"/>'s.
	/// </summary>
	internal abstract class SpriteElement<TKey, TValue>
		: IComparable<TValue>, IComparable/*, IEquatable<TValue>*/, ISpriteElement
		where TValue : SpriteElement<TKey, TValue>
	{
		#region Properties

		/// <summary>
		///  Gets the Id of the sprite element.
		/// </summary>
		public TKey Id { get; set; }
		object ISpriteElement.Id => Id;

		#endregion

		#region IEquatable/IComparable Implementation

		/*/// <summary>
		///  Gets the hash code for the sprite element.
		/// </summary>
		/// <returns>The sprite element's hash code.</returns>
		public override int GetHashCode() => Id.GetHashCode();
		/// <summary>
		///  Checks for equality between this sprite element and another.
		/// </summary>
		/// <param name="obj">The sprite element to compare.</param>
		/// <returns>True if <paramref name="obj"/> is the same type and of the same Id.</returns>
		public override bool Equals(object obj) {
			if (obj is TValue objSpr) return Equals(objSpr);
			return false;
		}*/
		/// <summary>
		///  Compares the order between this sprite element and another.
		/// </summary>
		/// <param name="obj">The sprite element to compare.</param>
		/// <returns>The comparison between this sprite element and <paramref name="obj"/>.</returns>
		public int CompareTo(object obj) {
			if (obj is TValue objSpr) return CompareTo(objSpr);
			throw new ArgumentException($"{nameof(obj)} is not a {GetType().Name}!");
		}
		/*/// <summary>
		///  Checks for equality between this sprite element and another.
		/// </summary>
		/// <param name="other">The sprite element to compare.</param>
		/// <returns>True if <paramref name="other"/> is of the same Id.</returns>
		public bool Equals(TValue other) => Id.Equals(other.Id);*/
		/// <summary>
		///  Compares the order between this sprite element and another.
		/// </summary>
		/// <param name="other">The sprite element to compare.</param>
		/// <returns>The comparison between this sprite element and <paramref name="other"/>.</returns>
		public virtual int CompareTo(TValue other) => Comparer<TKey>.Default.Compare(Id, other.Id);

		#endregion

		#region ToString Override

		/// <summary>
		///  Gets the string representation of the sprite element.
		/// </summary>
		/// <returns>The sprite element's string representation.</returns>
		public override string ToString() => Id.ToString();

		#endregion
	}
	/// <summary>
	///  The generic implementation for all <see cref="ISpriteElement"/> containers.
	/// </summary>
	internal abstract class SpriteCategory<TKey, TValue>
		: SpriteElement<TKey, TValue>, ISpriteCategoryBuilder
		where TValue : SpriteElement<TKey, TValue>
	{
		#region Fields

		/// <summary>
		///  Gets the sprite category entry for this category.
		/// </summary>
		public abstract SpriteCategoryInfo Category { get; }
		/// <summary>
		///  Gets the sorted list of elements in the category.
		/// </summary>
		public List<ISpriteElement> List { get; } = new List<ISpriteElement>();
		IReadOnlyList<ISpriteElement> ISpriteCategory.List => List;

		#endregion

		#region Properties
		
		/// <summary>
		///  Gets the number of elements in the category.
		/// </summary>
		public int Count => List.Count;
		/// <summary>
		///  Gets if this category is the last category and contains sprite part lists.
		/// </summary>
		public bool IsLastCategory => List[0] is ISpritePartList;
		/// <summary>
		///  Gets the display name of the category.
		/// </summary>
		public string DisplayName => ToString();

		#endregion

		#region Element Accessors

		/*/// <summary>
		///  Gets the element at the specified index in the category.
		/// </summary>
		/// <param name="index">The index of the element to get.</param>
		/// <returns>The element at the specified index.</returns>
		public ISpriteElement this[int index] => List[index];*/
		/// <summary>
		///  Gets the element with the specified Id in the category.
		/// </summary>
		/// <param name="id">The Id of the element to get.</param>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="id"/> is null.
		/// </exception>
		/// <exception cref="KeyNotFoundException">
		///  The element with the <paramref name="id"/> was not found.
		/// </exception>
		public ISpriteElement this[object id] {
			get {
				if (id == null)
					throw new ArgumentNullException(nameof(id));
				ISpriteElement element = List.Find(e => e.Id.Equals(id));
				return element ?? throw new KeyNotFoundException($"Could not find the key \"{id}\"!");
			}
		}

		/*/// <summary>
		///  Gets the element with the specified Id in the category.
		/// </summary>
		/// <param name="id">The Id of the element to get.</param>
		/// <returns>The element with the specified Id.</returns>
		public ISpriteElement Get(object id) {
			ISpriteElement element = List.Find(e => e.Id.Equals(id));
			return element ?? throw new KeyNotFoundException($"Could not find the key \"{id}\"!");
		}*/
		/// <summary>
		///  Tries to get the element with the specified Id in the category.
		/// </summary>
		/// <param name="id">The Id of the element to get.</param>
		/// <param name="value">The output element if one was found, otherwise null.</param>
		/// <returns>True if an element with the Id was found, otherwise null.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="id"/> is null.
		/// </exception>
		public bool TryGetValue(object id, out ISpriteElement value) {
			if (id == null)
				throw new ArgumentNullException(nameof(id));
			value = List.Find(e => e.Id.Equals(id));
			return value != null;
		}
		/// <summary>
		///  Tries to get the category with the specified Id in the category.
		/// </summary>
		/// <param name="id">The Id of the category to get.</param>
		/// <param name="value">The output category if one was found, otherwise null.</param>
		/// <returns>True if a category with the Id was found, otherwise null.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="id"/> is null.
		/// </exception>
		public bool TryGetValue(object id, out ISpriteCategory value) {
			if (id == null)
				throw new ArgumentNullException(nameof(id));
			value = (ISpriteCategory) List.Find(e => e.Id.Equals(id));
			return value != null;
		}
		/// <summary>
		///  Tries to get the part list with the specified Id in the category.
		/// </summary>
		/// <param name="id">The Id of the part list to get.</param>
		/// <param name="value">The output part list if one was found, otherwise null.</param>
		/// <returns>True if a part list with the Id was found, otherwise null.</returns>
		public bool TryGetValue(int id, out ISpritePartList value) {
			value = (ISpritePartList) List.Find(e => e.Id.Equals(id));
			return value != null;
		}
		/// <summary>
		///  Gets if the category contains an element with the specified Id.
		/// </summary>
		/// <param name="id">The Id to check for an element with.</param>
		/// <returns>True if an element exists with the specified Id, otherwise null.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="id"/> is null.
		/// </exception>
		public bool ContainsKey(object id) {
			if (id == null)
				throw new ArgumentNullException(nameof(id));
			return List.Find(e => e.Id.Equals(id)) != null;
		}

		#endregion

		#region Category Accessors

		/*/// <summary>
		///  Gets the category at the specified index in the category.
		/// </summary>
		/// <param name="index">The index of the category to get.</param>
		/// <returns>The category at the specified index.</returns>
		ISpriteCategory ISpriteCategoryContainer.this[int index] => (ISpriteCategory) this[index];

		/// <summary>
		///  Gets the category with the specified Id in the category.
		/// </summary>
		/// <param name="id">The Id of the category to get.</param>
		/// <returns>The category with the specified Id.</returns>
		/// 
		/// <exception cref="KeyNotFoundException">
		///  The element with the <paramref name="id"/> was not found.
		/// </exception>
		ISpriteCategory ISpriteCategoryContainer.Get(object id) => (ISpriteCategory) Get(id);
		/// <summary>
		///  Gets if the category contains a category with the specified Id.
		/// </summary>
		/// <param name="id">The Id to check for a category with.</param>
		/// <returns>True if a category exists with the specified Id, otherwise null.</returns>
		bool ISpriteCategoryContainer.ContainsKey(object id) => ContainsKey(id);*/

		#endregion

		#region Part List Accessors

		/*/// <summary>
		///  Gets the part list at the specified index in the category.
		/// </summary>
		/// <param name="index">The index of the part list to get.</param>
		/// <returns>The part list at the specified index.</returns>
		ISpritePartList ISpritePartListContainer.this[int index] => (ISpritePartList) this[index];

		/// <summary>
		///  Gets the part list with the specified Id in the category.
		/// </summary>
		/// <param name="id">The Id of the part list to get.</param>
		/// <returns>The part list with the specified Id.</returns>
		/// 
		/// <exception cref="KeyNotFoundException">
		///  The element with the <paramref name="id"/> was not found.
		/// </exception>
		ISpritePartList ISpritePartListContainer.Get(int id) => (ISpritePartList) Get(id);
		/// <summary>
		///  Gets if the category contains a part list with the specified Id.
		/// </summary>
		/// <param name="id">The Id to check for an part list with.</param>
		/// <returns>True if a part list exists with the specified Id, otherwise null.</returns>
		bool ISpritePartListContainer.ContainsKey(int id) => ContainsKey(id);*/

		#endregion

		#region Part Accessors

		/*/// <summary>
		///  Tries to get the part types that exist for this sprite selection for the specified sprite part group.
		/// </summary>
		/// <param name="group">The sprite part group whose sprite parts are being acquired.</param>
		/// <param name="partId">The Id of the parts being used for the group.</param>
		/// <param name="parts">The output sprite parts. This value should be ignored when false is returned.</param>
		/// <returns>True if any parts were found for this group's part Id.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="group"/> is null.
		/// </exception>
		public bool TryGetPartTypes(CharacterSpritePartGroupInfo group, int partId, out ISpritePart[] parts) {
			if (group == null)
				throw new ArgumentNullException(nameof(group));
			IReadOnlyList<int> typeIds = group.TypeIds;
			parts = new ISpritePart[typeIds.Count];
			bool found = false;
			for (int groupIndex = 0; groupIndex < typeIds.Count; groupIndex++) {
				int typeId = typeIds[groupIndex];
				if (TryGetValue(typeId, out var partTypesElement)) {
					SpritePartList partTypes = (SpritePartList) partTypesElement;
					if (partTypes.TryGetValue(partId, out parts[groupIndex]))
						found = true; // Not a situation where we break after found, fill in the parts array.
				}
			}
			return found;
		}
		/// <summary>
		///  Tries to get the first part types that exist for this sprite selection for the specified part group.
		/// </summary>
		/// <param name="group">The sprite part group whose sprite parts are being acquired.</param>
		/// <param name="partId">The output Id for the parts to use for this group.</param>
		/// <param name="parts">The output sprite parts. This value should be ignored when false is returned.</param>
		/// <returns>True if this sprite selection has any parts for this sprite part group.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="group"/> is null.
		/// </exception>
		public bool TryGetFirstPartTypes(CharacterSpritePartGroupInfo group, out int partId, out ISpritePart[] parts) {
			if (group == null)
				throw new ArgumentNullException(nameof(group));
			IReadOnlyList<int> typeIds = group.TypeIds;
			parts = new ISpritePart[typeIds.Count];
			partId = int.MaxValue;
			bool found = false;
			for (int groupIndex = 0; groupIndex < typeIds.Count; groupIndex++) {
				int typeId = typeIds[groupIndex];
				if (TryGetValue(typeId, out var partTypesElement)) {
					SpritePartList partTypes = (SpritePartList) partTypesElement;
					if (partTypes.List.Count != 0) {
						parts[groupIndex] = partTypes.List.First();
						partId = Math.Min(parts[groupIndex].Id, partId);
						found = true;
					}
				}
			}
			if (found) {
				for (int groupIndex = 0; groupIndex < typeIds.Count; groupIndex++) {
					if (parts[groupIndex] != null && parts[groupIndex].Id != partId)
						parts[groupIndex] = null;
				}
			}
			else {
				partId = -1;
			}
			return found;
		}*/

		#endregion

		#region CreateGroups

		/// <summary>
		///  Creates sprite part groups used to categorize the sprite parts during selection.
		/// </summary>
		/// <param name="game">The game info associated with this sprite category.</param>
		/// <param name="character">The character info associated with this sprite category.</param>
		/// <returns>An array of sprite part groups for use in sprite part selection.</returns>
		/// 
		/// <exception cref="ArgumentNullException">
		///  <paramref name="game"/> or <paramref name="character"/> is null.
		/// </exception>
		public ISpritePartGroup[] CreateGroups(GameInfo game, CharacterInfo character) {
			CharacterSpritePartGroupInfo[] groupInfos = character.Database.GetPartGroups(game, character);
			ISpritePartGroup[] groups = new ISpritePartGroup[groupInfos.Length];
			for (int i = 0; i < groups.Length; i++) {
				CharacterSpritePartGroupInfo groupInfo = groupInfos[i];
				SpritePartGroup group = new SpritePartGroup {
					Info = groupInfo,
					Index = i,
					TypeIds = groupInfo.TypeIds.ToArray(),
					GroupParts = new List<SpritePartGroupPart> { SpritePartGroupPart.None },
				};
				// Add sprite part group parts that contain sprite parts for each Type Id.
				for (int typeIndex = 0; typeIndex < groupInfo.TypeIds.Count; typeIndex++) {
					int typeId = groupInfo.TypeIds[typeIndex];
					SpritePartList partList = (SpritePartList) List.Find(e => e.Id.Equals(typeId));
					if (partList != null) {
						foreach (ISpritePart part in partList.List) {
							SpritePartGroupPart groupPart = group.GroupParts.Find(gp => gp.Id == part.Id);
							if (groupPart == null) {
								groupPart = new SpritePartGroupPart {
									Id = part.Id,
									Parts = new SpritePart[groupInfo.TypeIds.Count],
								};
								group.GroupParts.Add(groupPart);
							}
							groupPart.Parts[typeIndex] = (SpritePart) part;
						}
					}
				}
				group.GroupParts.Sort();
				groups[i] = group;
			}
			return groups;
		}
		/*/// <summary>
		///  Creates a group part representing no selection.
		/// </summary>
		/// <returns>The created empty group part.</returns>
		public ISpritePartGroupPart CreateNoneGroupPart() => SpritePartGroupPart.None;*/

		#endregion

		#region ISpriteCategoryBuilder Implementation

		/// <summary>
		///  Adds the sprite element to the category.
		/// </summary>
		/// <param name="value">The sprite element to add with its Id.</param>
		public void Add(ISpriteElement value) => List.Add(value);
		/// <summary>
		///  Performs sorting of the list after it has finished being populated.
		/// </summary>
		public void Sort() => List.Sort();

		#endregion

		#region IEnumerable Implementation

		IEnumerator IEnumerable.GetEnumerator() => List.GetEnumerator();

		#endregion
	}
}
