using System;
using System.Linq.Expressions;
using Hover.Common.Items;
using Hover.Common.Items.Types;
using UnityEditor;
using UnityEngine;

namespace Hover.Common.Edit.Items {

	/*================================================================================================*/
	[CanEditMultipleObjects]
	[CustomEditor(typeof(HoverItemData))]
	public class HoverItemDataEditor : Editor {

		private string vOnSelectedEventName;
		private string vOnDeselectedEventName;
		private string vOnBoolValueChangedEventName;
		private string vOnFloatValueChangedEventName;
		private GUIStyle vVertStyle;

		private string vIsHiddenOpenKey;
		private string vIsEventOpenKey;
		
		private HoverItemData vTarget;
		
		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public void OnEnable() {
			CheckboxItem selBoolData = CreateInstance<CheckboxItem>();
			SliderItem selFloatData = CreateInstance<SliderItem>();

			vOnSelectedEventName = GetPropertyName(() => selBoolData.OnSelectedEvent);
			vOnDeselectedEventName = GetPropertyName(() => selBoolData.OnDeselectedEvent);
			vOnBoolValueChangedEventName = GetPropertyName(() => selBoolData.OnValueChangedEvent);
			vOnFloatValueChangedEventName = GetPropertyName(() => selFloatData.OnValueChangedEvent);

			vVertStyle = new GUIStyle();
			vVertStyle.padding = new RectOffset(16, 0, 0, 0);
			
			int targetId = target.GetInstanceID();

			vIsHiddenOpenKey = "IsHiddenOpen"+targetId;
			vIsEventOpenKey = "IsEventOpen"+targetId;
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public override void OnInspectorGUI() {
			vTarget = (HoverItemData)target;

			Undo.RecordObject(vTarget, vTarget.GetType().Name);
			DrawItems();
			
			if ( GUI.changed ) {
				EditorUtility.SetDirty(vTarget);
			}
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public override bool RequiresConstantRepaint() {
			return EditorPrefs.GetBool(vIsHiddenOpenKey);
		}
		
		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void DrawItems() {
			DrawMainItems();
			DrawEventItemGroup();
			DrawHiddenItemGroup();
		}
		
		/*--------------------------------------------------------------------------------------------*/
		private void DrawMainItems() {
			BaseItem baseData = vTarget.Data;
		
			vTarget.ItemType = (HoverItemData.HoverItemType)EditorGUILayout.EnumPopup(
				"Item Type", vTarget.ItemType);

			baseData.Id = EditorGUILayout.TextField("ID", baseData.Id);
			baseData.Label = EditorGUILayout.TextField("Label", baseData.Label);
			baseData.Width = EditorGUILayout.FloatField("Width", baseData.Width);
			baseData.Height = EditorGUILayout.FloatField("Height", baseData.Height);
			baseData.IsEnabled = EditorGUILayout.Toggle("Is Enabled", baseData.IsEnabled);
			baseData.IsVisible = EditorGUILayout.Toggle("Is Visible", baseData.IsVisible);
			
			////

			ISelectorItem selectorData = (vTarget.Data as ISelectorItem);
			ICheckboxItem checkboxData = (vTarget.Data as ICheckboxItem);
			IRadioItem radioData = (vTarget.Data as IRadioItem);
			ISliderItem sliderData = (vTarget.Data as ISliderItem);

			if ( selectorData != null ) {
				selectorData.NavigateBackUponSelect = EditorGUILayout.Toggle(
					"Navigate Back Upon Select", selectorData.NavigateBackUponSelect);
			}

			if ( checkboxData != null ) {
				checkboxData.Value = EditorGUILayout.Toggle("Checkbox Value", checkboxData.Value);
			}

			if ( radioData != null ) {
				radioData.GroupId = EditorGUILayout.TextField("Radio Group ID", radioData.GroupId);
				radioData.Value = EditorGUILayout.Toggle("Radio Value", radioData.Value);
			}

			if ( sliderData != null ) {
				float rangeValue = Mathf.Lerp(
					sliderData.RangeMin, sliderData.RangeMax, sliderData.Value);

				sliderData.RangeMin = EditorGUILayout.FloatField(
					"Slider Range Min", sliderData.RangeMin);
				sliderData.RangeMax = EditorGUILayout.FloatField(
					"Slider Range Max", sliderData.RangeMax);
				rangeValue = EditorGUILayout.Slider(
					"Slider Range Value", rangeValue, sliderData.RangeMin, sliderData.RangeMax);
				sliderData.Value = Mathf.InverseLerp(
					sliderData.RangeMin, sliderData.RangeMax, rangeValue);
				sliderData.Ticks = EditorGUILayout.IntField(
					"Slider Ticks", sliderData.Ticks);
				sliderData.Snaps = EditorGUILayout.IntField(
					"Slider Snaps", sliderData.Snaps);
				sliderData.AllowJump = EditorGUILayout.Toggle(
					"Slider Allow Jump", sliderData.AllowJump);
				sliderData.FillStartingPoint = (SliderItem.FillType)EditorGUILayout.EnumPopup(
					"Slider Fill Starting-Point", sliderData.FillStartingPoint);
			}
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void DrawEventItemGroup() {
			SelectableItem selectableData = (vTarget.Data as SelectableItem);

			if ( selectableData == null ) {
				return;
			}

			bool isEventOpen = EditorGUILayout.Foldout(EditorPrefs.GetBool(vIsEventOpenKey), "Events");
			EditorPrefs.SetBool(vIsEventOpenKey, isEventOpen);
			
			if ( isEventOpen ) {
				EditorGUILayout.BeginVertical(vVertStyle);
				DrawEventItems();
				EditorGUILayout.EndVertical();
			}
		}
		
		/*--------------------------------------------------------------------------------------------*/
		private void DrawEventItems() {
			var serializedData = new SerializedObject(vTarget.Data);
			var onSelectedProp = serializedData.FindProperty(vOnSelectedEventName);
			var onDeselectedProp = serializedData.FindProperty(vOnDeselectedEventName);

			EditorGUILayout.PropertyField(onSelectedProp);
			EditorGUILayout.PropertyField(onDeselectedProp);

			if ( vTarget.Data is ISelectableItem<bool> ) {
				var onBoolValProp = serializedData.FindProperty(vOnBoolValueChangedEventName);
				EditorGUILayout.PropertyField(onBoolValProp);
			}

			if ( vTarget.Data is ISelectableItem<float> ) {
				var onFloatValProp = serializedData.FindProperty(vOnFloatValueChangedEventName);
				EditorGUILayout.PropertyField(onFloatValProp);
			}
		}

			
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void DrawHiddenItemGroup() {
			bool isHiddenOpen = EditorGUILayout.Foldout(EditorPrefs.GetBool(vIsHiddenOpenKey), "Info");
			EditorPrefs.SetBool(vIsHiddenOpenKey, isHiddenOpen);
			
			if ( isHiddenOpen ) {
				EditorGUILayout.BeginVertical(vVertStyle);
				GUI.enabled = false;
				DrawHiddenItems();
				GUI.enabled = true;
				EditorGUILayout.EndVertical();
			}
		}
		
		/*--------------------------------------------------------------------------------------------*/
		private void DrawHiddenItems() {
			BaseItem baseData = vTarget.Data;
			SelectableItem selectableData = (baseData as SelectableItem);

			EditorGUILayout.IntField("Auto ID", baseData.AutoId);
			EditorGUILayout.Toggle("Is Ancestry Enabled", baseData.IsAncestryEnabled);
			EditorGUILayout.Toggle("Is Ancestry Visible", baseData.IsAncestryVisible);

			if ( selectableData == null ) {
				return;
			}

			EditorGUILayout.Toggle("Is Sticky-Selected", selectableData.IsStickySelected);
			EditorGUILayout.Toggle("Allow Selection", selectableData.AllowSelection);

			IRadioItem radioData = (vTarget.Data as IRadioItem);
			ISliderItem sliderData = (vTarget.Data as ISliderItem);

			if ( radioData != null ) {
				EditorGUILayout.TextField("Radio Default Group ID", radioData.DefaultGroupId);
			}

			if ( sliderData != null ) {
				EditorGUILayout.FloatField("Slider Value", sliderData.Value);
				EditorGUILayout.FloatField("Slider Snapped Value", sliderData.SnappedValue);
				EditorGUILayout.FloatField("Slider Snapped Range Value", sliderData.SnappedRangeValue);
				EditorGUILayout.TextField("Slider Hover Value", sliderData.HoverValue+"");
				EditorGUILayout.TextField("Slider Snapped Hover Value",sliderData.SnappedHoverValue+"");
			}
		}
		
		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private static string GetPropertyName<T>(Expression<Func<T>> pPropExpr) {
			return ((MemberExpression)pPropExpr.Body).Member.Name;
		}

	}

}
