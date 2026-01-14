using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable enable
// ReSharper disable once CheckNamespace

namespace GameLovers.GameData
{
	/// <summary>
	/// Extension methods for Unity objects.
	/// </summary>
	public static class UnityObjectsExtensions
	{
		/// <summary>
		/// Get the corners of the <see cref="RectTransform"/> in the local space of its Transform.
		/// </summary>
		public static Vector3[] GetLocalCornersArray(this RectTransform transform)
		{
			var corners =  new Vector3[4];
			var rect = transform.rect;
			var x = rect.x;
			var y = rect.y;
			var xMax = rect.xMax;
			var yMax = rect.yMax;

			corners[0] = new Vector3(x, y, 0f);
			corners[1] = new Vector3(x, yMax, 0f);
			corners[2] = new Vector3(xMax, yMax, 0f);
			corners[3] = new Vector3(xMax, y, 0f);

			return corners;
		}

		/// <summary>
		/// Get the corners of the <see cref="RectTransform"/> in world space of its Transform.
		/// </summary>
		public static Vector3[] GetWorldCornersArray(this RectTransform transform)
		{
			var corners = transform.GetLocalCornersArray();
			var matrix4x = transform.localToWorldMatrix;
			for (int i = 0; i < 4; i++)
			{
				corners[i] = matrix4x.MultiplyPoint(corners[i]);
			}

			return corners;
		}

		/// <summary>
		/// Extension method for GraphicRaycaster that performs a raycast at the specified screen point.
		/// Returns true if any object was hit.
		/// </summary>
		public static bool RaycastPoint(this GraphicRaycaster raycaster, Vector2 screenPoint, out List<RaycastResult> results)
		{
			var eventData = new PointerEventData(EventSystem.current)
			{
				position = screenPoint,
				displayIndex = 0
			};

			results = new List<RaycastResult>();

			raycaster.Raycast(eventData, results);

			return results.Count > 0;
		}
	}
}
