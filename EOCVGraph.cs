using System;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using UnityEngine;

namespace EnhancedOutsideConnectionsView
{
	/// <summary>
	/// this is a copy of UIGraph with many improvements
	/// </summary>
	public class EOCVGraph : UISprite
	{
		/// <summary>
		/// settings for a curve on the graph
		/// </summary>
		private class CurveSettings
		{
			// the settings
			private string _name;
			private string _tooltip;
			private float[] _data;
			private float _width;
			private Color32 _color;

			// values computed from the data
			private float _minValue;
			private float _maxValue;

			private CurveSettings() {}

			public CurveSettings(string name, string tooltip, float[] data, float width, Color32 color)
			{
				// check parameters
				if (string.IsNullOrEmpty(name))    { throw new ArgumentNullException("name");        }
				if (string.IsNullOrEmpty(tooltip)) { throw new ArgumentNullException("tooltip");     }
				if (data == null)                  { throw new ArgumentNullException("data");        }
				if (width <= float.Epsilon)        { throw new ArgumentOutOfRangeException("width"); }

				// save parameters
				_name = name;
				_tooltip = tooltip;
				_data = data;
				_width = width;
				_color = color;

				// compute min and max of valid values
				_minValue = 0f;
				_maxValue = 0f;
				for (int i = 0; i < _data.Length; i++)
				{
					if (_data[i] >= 0f)
                    {
						_minValue = Mathf.Min(_minValue, _data[i]);
						_maxValue = Mathf.Max(_maxValue, _data[i]);
					}
				}
			}

			// readonly accessors for the settings
			public string Name    { get { return _name;    } }
			public string Tooltip { get { return _tooltip; } }
			public float[] Data   { get { return _data;    } }
			public float Width    {  get { return _width;  } }
			public Color32 Color  {  get { return _color;  } }

			// readonly accessors for the computed values
			public float MinValue { get { return _minValue; } }
			public float MaxValue { get { return _maxValue; } }
		}

		// constant for converting between ticks and days
		// the horizontal axis of the graph needs to accurate only to the day
		// 10000000 ticks per second times 24*60*60 seconds per day
		private const long TicksPerDay = 10000000L * 24L * 60L * 60L;

		// values that control the look of the graph
		private Color32 _textColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		private Color32 _axesColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		private Color32 _helpAxesColor = new Color32(128, 128, 128, byte.MaxValue);
		private float _axesWidth = 1f;
		private float _helpAxesWidth = 0.5f;
		private Rect _graphRect = new Rect(0.1f, 0.1f, 0.8f, 0.8f);
		private UIFont _font;

		// the dates for the horizontal axis
		private DateTime[] _dates;
		private DateTime _startDate;
		private DateTime _endDate;

		// the curves to be shown on the graph
		private List<CurveSettings> _curves = new List<CurveSettings>();
		private float _minCurveValue;
		private float _maxCurveValue;

		// accessors that invalidate the graph whenever changed
		public Color32 TextColor     { get { return _textColor;     } set { _textColor     = value; Invalidate(); } }
		public Color32 AxesColor     { get { return _axesColor;     } set { _axesColor     = value; Invalidate(); } }
		public Color32 HelpAxesColor { get { return _helpAxesColor; } set { _helpAxesColor = value; Invalidate(); } }
		public float AxesWidth       { get { return _axesWidth;     } set { _axesWidth     = value; Invalidate(); } }
		public float HelpAxesWidth   { get { return _helpAxesWidth; } set { _helpAxesWidth = value; Invalidate(); } }
		public Rect GraphRect        { get { return _graphRect;     } set { _graphRect     = value; Invalidate(); } }
		public UIFont Font           { get { return _font;          } set { _font          = value; Invalidate(); } }

		/// <summary>
		/// make sure font is initialized
		/// </summary>
		public override void Start()
		{
			// do base processing
			base.Start();

			// initialize font
			bool flag = _font != null && _font.isValid;
			if (Application.isPlaying && !flag)
			{
				_font = GetUIView().defaultFont;
			}
		}

		/// <summary>
		/// get the text render data
		/// </summary>
		private UIRenderData textRenderData
		{
			get
			{
				// if there are not 2, then add one
				while (m_RenderData.Count <= 1)
				{
					UIRenderData item = UIRenderData.Obtain();
					m_RenderData.Add(item);
                }

				// return the one that was added previously
				return m_RenderData[1];
			}
		}

		/// <summary>
		/// set the dates for the horizontal axis
		/// </summary>
		public void SetDates(DateTime[] dates)
		{
			// save the dates
			_dates = dates;

			// compute start and end dates
			if (_dates == null || _dates.Length == 0)
			{
				// use current game date
				_startDate = _endDate = SimulationManager.instance.m_currentGameTime.Date;
			}
			else
            {
				// verify dates are in ascending order with no duplicates
				DateTime date = _dates[0];
				for (int i = 1; i < _dates.Length; i++)
                {
					if (_dates[i] <= date)
                    {
						throw new InvalidOperationException("Dates must be in ascending order with no duplicates.");
                    }
					date = _dates[i];
                }

				// compute start and end dates
				_startDate = _dates[0];
				_endDate = _dates[_dates.Length - 1];
			}

			// reset curve min and max values
			_minCurveValue = 0f;
			_maxCurveValue = 0f;
		}

		/// <summary>
		/// add a curve to the graph
		/// </summary>
		/// <param name="name">curve name</param>
		/// <param name="tooltip">prefix to use for tooltip text</param>
		/// <param name="data">curve data</param>
		/// <param name="width">curve width</param>
		/// <param name="color">curve color</param>
		public void AddCurve(string name, string tooltip, float[] data, float width, Color32 color)
		{
			// date must be set first
			if (_dates == null)
			{
				throw new InvalidOperationException("Dates must be set before adding a curve.");
			}
			if (data.Length != _dates.Length)
			{
				throw new InvalidOperationException("Curve data must have the same number of entries as the dates.");
			}

			// create a new curve
			CurveSettings curveSettings = new CurveSettings(name, tooltip, data, width, color);
			_curves.Add(curveSettings);

			// compute new min and max values
			_minCurveValue = Mathf.Min(_minCurveValue, curveSettings.MinValue);
			_maxCurveValue = Mathf.Max(_maxCurveValue, curveSettings.MaxValue);
		}

		/// <summary>
		/// clear the graph
		/// </summary>
		public void Clear()
		{
			_dates = null;
			_curves.Clear();
			Invalidate();
		}

		/// <summary>
		/// when the user hovers the cursor near a data point, show data for the point
		/// </summary>
		protected override void OnTooltipHover(UIMouseEventParameter p)
		{
			// assume no data point found
			bool foundDataPoint = false;
			
			// there must be curves
			if (_curves.Count > 0)
            {
				PixelsToUnits();

				// compute the cursor position relative to the graph rect
				Vector2 hitPosition = GetHitPosition(p);
				hitPosition.x /= base.size.x;
				hitPosition.y /= base.size.y;
				hitPosition.x = (hitPosition.x - _graphRect.xMin) / _graphRect.width;
				hitPosition.y = 1f - hitPosition.y;

				// cursor must be in the graph rect
				if (hitPosition.x >= 0f && hitPosition.x <= 1f && hitPosition.y >= 0f && hitPosition.y <= 1f)
				{
					// compute date index according to x hit position
					int dateIndex = -1;
					const float MIN_TOOLTIP_DISTANCE = 0.02f;
					CalculateYearRange(_startDate, _endDate, out int _, out int _, out int _, out long startDays, out long endDays);
					float graphDays = endDays - startDays;
					float minDistanceX = MIN_TOOLTIP_DISTANCE;
					for (int i = 0; i < _dates.Length; i++)
					{
						float posX = (_dates[i].Ticks / TicksPerDay - startDays) / graphDays;
						float distanceX = Mathf.Abs(posX - hitPosition.x);
						if (distanceX < minDistanceX)
						{
							dateIndex = i;
							minDistanceX = distanceX;
						}
					}

					// date must be found
					if (dateIndex >= 0)
					{
						// compute curve index by finding the curve with the point closest to the y hit position
						int curveIndex = -1;
						CalculateValueRange(_minCurveValue, _maxCurveValue, out float minRange, out float maxRange, out float _);
						float minDistanceY = MIN_TOOLTIP_DISTANCE;
						for (int i = 0; i < _curves.Count; i++)
						{
							float posY = 0.5f + NormalizeY(_curves[i].Data[dateIndex], minRange, maxRange);
							float distanceY = Mathf.Abs(posY - hitPosition.y);
							if (distanceY < minDistanceY)
							{
								curveIndex = i;
								minDistanceY = distanceY;
							}
						}

						// curve must be found
						if (curveIndex >= 0)
						{
							// value must be valid
							float value = _curves[curveIndex].Data[dateIndex];
							if (value >= 0f)
							{
								// found a valid data point
								foundDataPoint = true;

								// compute the tool tip box position to follow the cursor
								UIView uIView = GetUIView();
								Vector2 screenResolution = GetUIView().GetScreenResolution();
								Vector2 cursorPositionOnScreen = uIView.ScreenPointToGUI(p.position / uIView.inputScale);
								Vector3 vector3 = base.tooltipBox.pivot.UpperLeftToTransform(base.tooltipBox.size, base.tooltipBox.arbitraryPivotOffset);
								Vector2 tooltipPosition = cursorPositionOnScreen + new Vector2(vector3.x, vector3.y);

								// make sure tooltip box is entirely on the screen
								if (tooltipPosition.x < 0f)
								{
									tooltipPosition.x = 0f;
								}
								if (tooltipPosition.y < 0f)
								{
									tooltipPosition.y = 0f;
								}
								if (tooltipPosition.x + base.tooltipBox.width > screenResolution.x)
								{
									tooltipPosition.x = screenResolution.x - base.tooltipBox.width;
								}
								if (tooltipPosition.y + base.tooltipBox.height > screenResolution.y)
								{
									tooltipPosition.y = screenResolution.y - base.tooltipBox.height;
								}
								base.tooltipBox.relativePosition = tooltipPosition;

								// set the tool tip text to the curve tooltip + date + value
								m_Tooltip = _curves[curveIndex].Tooltip + " (" + _dates[dateIndex].ToString("dd/MM/yyyy") + "  :  " + value.ToString("N0", LocaleManager.cultureInfo) + ")";
							}
						}
					}
				}
			}

			// check if data point was found
			if (foundDataPoint)
            {
				base.OnTooltipHover(p);
			}
			else
            {
				base.OnTooltipLeave(p);
			}
			RefreshTooltip();
		}

		/// <summary>
		/// called when graph needs to be rendered
		/// </summary>
		protected override void OnRebuildRenderData()
		{
			try
			{
				// make sure font is defined and valid
				if (!(_font != null) || !_font.isValid)
				{
					_font = GetUIView().defaultFont;
				}

				// proceed only if things needed to render are valid
				if (base.atlas != null && base.atlas.material != null && _font != null && _font.isValid && base.isVisible && base.spriteInfo != null)
				{
					// clear the text render
					textRenderData.Clear();

					// copy material from base atlas
					base.renderData.material = base.atlas.material;
					textRenderData.material = base.atlas.material;

					// get items from base render data
					PoolList<Vector3> vertices = base.renderData.vertices;
					PoolList<int> triangles = base.renderData.triangles;
					PoolList<Vector2> uvs = base.renderData.uvs;
					PoolList<Color32> colors = base.renderData.colors;

					// draw axes and labels
					DrawAxesAndLabels(vertices, triangles, uvs, colors);

					// draw each curve
					for (int i = 0; i < _curves.Count; i++)
					{
						DrawCurve(vertices, triangles, uvs, colors, _curves[i]);
					}
				}
			}
			catch (Exception ex)
			{
				LogUtil.LogException(ex);
			}
		}

		/// <summary>
		/// compute the position of the X value in the graph rect
		/// </summary>
		private float NormalizeX(float x, float min, float max)
        {
			return -0.5f + _graphRect.xMin + _graphRect.width * (x - min) / (max - min);
		}

		/// <summary>
		/// compute the position of the Y value in the graph rect
		/// </summary>
		private float NormalizeY(float y, float min, float max)
		{
			return -0.5f + _graphRect.yMin + _graphRect.height * (y - min) / (max - min);
		}

		/// <summary>
		/// calculate the start year, end year, and increment year based on the min and max dates
		/// also calculate seconds for start year and end year
		/// </summary>
		private void CalculateYearRange(DateTime minDate, DateTime maxDate, out int startYear, out int endYear, out int incrementYear, out long startDays, out long endDays)
		{
			// get min year
			int minYear = minDate.Year;

			// get max year
			// for January 1, use the year
			// for other than January 1, use the next year
			int maxYear = maxDate.Year + (maxDate.Month == 1 && maxDate.Day == 1 ? 0 : 1);

			// max year must be at least min year + 1 (i.e. show a minimum of one year)
			if (maxYear <= minYear)
			{
				maxYear = minYear + 1;
			}

			// compute whole year increment
			incrementYear = Mathf.CeilToInt(Mathf.Pow(10f, Mathf.FloorToInt(Mathf.Log10(0.5f * (maxYear - minYear)))));
			if (incrementYear == 0)
			{
				incrementYear = 1;
			}

			// compute start and end years
			startYear = incrementYear * Mathf.FloorToInt((float)minYear / incrementYear);
			endYear = incrementYear * Mathf.CeilToInt((float)maxYear / incrementYear);

			// if more than 15, double increment and recompute
			if ((float)(endYear - startYear) / incrementYear > 15f)
			{
				incrementYear *= 2;
				startYear = incrementYear * Mathf.FloorToInt((float)minYear / incrementYear);
				endYear = incrementYear * Mathf.CeilToInt((float)maxYear / incrementYear);
			}

			// compute start and end days
			// allow division to truncate
			startDays = (startYear < 1 ? new DateTime(1, 1, 1).Ticks : new DateTime(startYear, 1, 1).Ticks) / TicksPerDay;
			endDays = (endYear > 9999 ? new DateTime(9999, 12, 31).Ticks : new DateTime(endYear, 1, 1).Ticks) / TicksPerDay;
		}

		/// <summary>
		/// calculate the min range, max range, and increment range based on the min and max values
		/// </summary>
		private void CalculateValueRange(float minValue, float maxValue, out float minRange, out float maxRange, out float incrementRange)
		{
			// if min value is less than 30% of max value, then use zero for min value
			if (minValue < 0.3f * maxValue)
			{
				minValue = 0f;
			}

			// if min and max values are very close to each other, set max value to min + 1
			if (maxValue - minValue < 1f)
			{
				maxValue = minValue + 1f;
			}

			// compute whole number increment, must be at least 1
			incrementRange = Mathf.CeilToInt(Mathf.Pow(10f, Mathf.FloorToInt(Mathf.Log10(0.5f * (maxValue - minValue)))));
			if (incrementRange == 0f)
			{
				incrementRange = 1f;
			}

			// compute min and max range values
			minRange = incrementRange * (float)Mathf.FloorToInt(minValue / incrementRange);
			maxRange = incrementRange * (float)Mathf.CeilToInt(maxValue / incrementRange);
		}

		/// <summary>
		/// draw a line
		/// </summary>
		private void AddSolidQuad(Vector2 corner1, Vector2 corner2, Color32 col, PoolList<Vector3> vertices, PoolList<int> indices, PoolList<Vector2> uvs, PoolList<Color32> colors)
		{
			using (PoolList<Vector2> poolList = PoolList<Vector2>.Obtain())
			{
				// get sprite info
				UITextureAtlas.SpriteInfo spriteInfo = base.spriteInfo;
				if (spriteInfo != null)
				{
					// this logic is unchanged from UIGraph.AddSolidQuad
					Rect region = spriteInfo.region;
					poolList.Add(new Vector2(0.75f * region.xMin + 0.25f * region.xMax, 0.75f * region.yMin + 0.25f * region.yMax));
					poolList.Add(new Vector2(0.25f * region.xMin + 0.75f * region.xMax, 0.75f * region.yMin + 0.25f * region.yMax));
					poolList.Add(new Vector2(0.25f * region.xMin + 0.75f * region.xMax, 0.25f * region.yMin + 0.75f * region.yMax));
					poolList.Add(new Vector2(0.75f * region.xMin + 0.25f * region.xMax, 0.25f * region.yMin + 0.75f * region.yMax));
					uvs.AddRange(poolList);
					vertices.Add(new Vector3(corner1.x, corner1.y));
					vertices.Add(new Vector3(corner2.x, corner1.y));
					vertices.Add(new Vector3(corner2.x, corner2.y));
					vertices.Add(new Vector3(corner1.x, corner2.y));
					indices.Add(vertices.Count - 4);
					indices.Add(vertices.Count - 3);
					indices.Add(vertices.Count - 2);
					indices.Add(vertices.Count - 4);
					indices.Add(vertices.Count - 2);
					indices.Add(vertices.Count - 1);
					_ = (Color32)((Color)col).linear;
					colors.Add(col);
					colors.Add(col);
					colors.Add(col);
					colors.Add(col);
				}
			}
		}

		/// <summary>
		/// draw the axes and labels on the graph
		/// much of this logic is copied from UIGraph.BuildLabels
		/// </summary>
		private void DrawAxesAndLabels(PoolList<Vector3> vertices, PoolList<int> indices, PoolList<Vector2> uvs, PoolList<Color32> colors)
		{
			// ignore if no curves
			if (_curves.Count == 0)
			{
				return;
			}

			// compute some stuff
			float pixelRatio = PixelsToUnits();
			float ratioXY = base.size.x / base.size.y;
			Vector3 baseSize = pixelRatio * base.size;
			Vector2 maxTextSize = new Vector2(base.size.x, base.size.y);
			Vector3 center = base.pivot.TransformToCenter(base.size, base.arbitraryPivotOffset) * pixelRatio;

			// draw each horizontal helper line and the labels to the left
			CalculateValueRange(_minCurveValue, _maxCurveValue, out float minRange, out float maxRange, out float incrementRange);
			for (float value = minRange; value <= maxRange; value += incrementRange)
			{
				// for unknown reasons, must obtain the renderer again for each label to ensure a value with a space separator (e.g. French "1 100") is rendered correctly
				using (UIFontRenderer uIFontRenderer = _font.ObtainRenderer())
				{
					// draw the label
					string text = value.ToString("N0", LocaleManager.cultureInfo);
					uIFontRenderer.textScale = 1f;
					uIFontRenderer.vectorOffset = new Vector3(0f, (0f - base.height) * pixelRatio * (0.5f - NormalizeY(value, minRange, maxRange)) + pixelRatio * 8f, 0f);
					uIFontRenderer.pixelRatio = pixelRatio;
					uIFontRenderer.maxSize = maxTextSize;
					uIFontRenderer.defaultColor = TextColor;
					uIFontRenderer.Render(text, textRenderData);
				}

				// draw a helper axis line, except for the first line which is the main axis (drawn below)
				if (value != minRange)
                {
					Vector2 corner1 = new Vector2(-0.5f + _graphRect.xMin, NormalizeY(value, minRange, maxRange) - pixelRatio * HelpAxesWidth);
					Vector2 corner2 = new Vector2(corner1.x + _graphRect.width, NormalizeY(value, minRange, maxRange) + pixelRatio * HelpAxesWidth);
					AddSolidQuad(Vector3.Scale(corner1, baseSize) + center, Vector3.Scale(corner2, baseSize) + center, HelpAxesColor, vertices, indices, uvs, colors);
				}
			}

			// draw each vertical helper line and the year labels below
			float minXPos = pixelRatio * base.width * (-0.5f + _graphRect.xMin);
			float maxXPos = pixelRatio * base.width * (-0.5f + _graphRect.xMax);
			CalculateYearRange(_startDate, _endDate, out int startYear, out int endYear, out int incrementYear, out long startDays, out long endDays);
			float graphDays = endDays - startDays;
			for (int year = startYear; year <= endYear; year += incrementYear)
			{
				// compute X position of line and label
				float yearDays;
				if (year < 1)
				{
					yearDays = new DateTime(1, 1, 1).Ticks / TicksPerDay;
				}
				else if (year > 9999)
				{
					yearDays = new DateTime(9999, 12, 31).Ticks / TicksPerDay;
				}
				else
				{
					yearDays = new DateTime(year, 1, 1).Ticks / TicksPerDay;
				}
				float x = Mathf.Lerp(minXPos, maxXPos, (yearDays - startDays) / graphDays);

				// obtain the renderer again for each label just in case it is needed like above
				using (UIFontRenderer uIFontRenderer = _font.ObtainRenderer())
				{
					// draw the label
					string text = year.ToString();
					uIFontRenderer.textScale = 1f;
					uIFontRenderer.vectorOffset = new Vector3(x, base.height * pixelRatio * -0.97f, 0f);
					uIFontRenderer.pixelRatio = pixelRatio;
					uIFontRenderer.maxSize = maxTextSize;
					uIFontRenderer.textAlign = UIHorizontalAlignment.Center;
					uIFontRenderer.defaultColor = TextColor;
					uIFontRenderer.Render(text, textRenderData);
				}

				// draw a helper axis line, except for the first line which is the main axis (drawn below)
				if (year != startYear)
                {
					float num14 = Mathf.Lerp(-0.5f + _graphRect.xMin, -0.5f + _graphRect.xMax, (yearDays - startDays) / graphDays);
					Vector2 corner1 = new Vector2(num14 - pixelRatio * HelpAxesWidth * ratioXY, -0.5f + _graphRect.yMin);
					Vector2 corner2 = new Vector2(num14 + pixelRatio * HelpAxesWidth * ratioXY, corner1.y + _graphRect.height);
					AddSolidQuad(Vector3.Scale(corner1, baseSize) + center, Vector3.Scale(corner2, baseSize) + center, HelpAxesColor, vertices, indices, uvs, colors);
				}
			}

			// draw horizontal main axis line
			Vector2 cornerA = new Vector2(-0.5f + _graphRect.xMin - ratioXY * pixelRatio * AxesWidth, -0.5f + _graphRect.yMin);
			Vector2 cornerB = new Vector2(-0.5f + _graphRect.xMin + ratioXY * pixelRatio * AxesWidth, cornerA.y + _graphRect.height);
			AddSolidQuad(Vector3.Scale(cornerA, baseSize) + center, Vector3.Scale(cornerB, baseSize) + center, AxesColor, vertices, indices, uvs, colors);

			// draw vertical main axis line
			cornerA = new Vector2(-0.5f + _graphRect.xMin,      -0.5f + _graphRect.yMin - pixelRatio * AxesWidth);
			cornerB = new Vector2(cornerA.x + _graphRect.width, -0.5f + _graphRect.yMin + pixelRatio * AxesWidth);
			AddSolidQuad(Vector3.Scale(cornerA, baseSize) + center, Vector3.Scale(cornerB, baseSize) + center, AxesColor, vertices, indices, uvs, colors);
		}

		/// <summary>
		/// draw the curve on the graph
		/// this is mostly a copy of UIGraph.BuildMeshData, but notable changes include:
		///		use date to determine X position of a data point
		///		do not draw a line to or from data points less than zero
		/// </summary>
		private void DrawCurve(PoolList<Vector3> vertices, PoolList<int> indices, PoolList<Vector2> uvs, PoolList<Color32> colors, CurveSettings curve)
		{
			// ignore if 0 or 1 data points
			if (curve.Data.Length <= 1)
			{
				return;
			}

			// ignore if no sprite info
			UITextureAtlas.SpriteInfo spriteInfo = base.spriteInfo;
			if (spriteInfo == null)
            {
				return;
            }

			using (PoolList<Vector2> poolList = PoolList<Vector2>.Obtain())
			{
				// compute region info
				Rect region = spriteInfo.region;
				poolList.Add(new Vector2(0.75f * region.xMin + 0.25f * region.xMax, 0.75f * region.yMin + 0.25f * region.yMax));
				poolList.Add(new Vector2(0.25f * region.xMin + 0.75f * region.xMax, 0.75f * region.yMin + 0.25f * region.yMax));
				poolList.Add(new Vector2(0.25f * region.xMin + 0.75f * region.xMax, 0.25f * region.yMin + 0.75f * region.yMax));
				poolList.Add(new Vector2(0.75f * region.xMin + 0.25f * region.xMax, 0.25f * region.yMin + 0.75f * region.yMax));
				poolList.Add(new Vector2(region.xMin, region.yMin));
				poolList.Add(new Vector2(region.xMax, region.yMin));
				poolList.Add(new Vector2(region.xMax, region.yMax));
				poolList.Add(new Vector2(region.xMin, region.yMax));

				// compute some stuff
				float pixelRatio = PixelsToUnits();
				float ratioXY = base.size.x / base.size.y;
				Vector3 baseSize = pixelRatio * base.size;
				Vector3 center = base.pivot.TransformToCenter(base.size, base.arbitraryPivotOffset) * pixelRatio;

				// get ranges
				CalculateYearRange(_startDate, _endDate, out int _, out int _, out int _, out long startDays, out long endDays);
				CalculateValueRange(_minCurveValue, _maxCurveValue, out float minRange, out float maxRange, out float _);

				// compute the X and Y locations of the first data point
				float previousData = curve.Data[0];
				Vector3 previousPoint = default(Vector3);
				previousPoint.x = NormalizeX(_dates[0].Ticks / TicksPerDay, startDays, endDays);
				previousPoint.y = NormalizeY(curve.Data[0], minRange, maxRange);

				// do each data point starting with 1
				Color32 item = ((Color)curve.Color).linear;
				for (int i = 1; i < curve.Data.Length; i++)
				{
					// compute the X and Y locations of the current point
					Vector3 currentPoint = default(Vector3);
					currentPoint.x = NormalizeX(_dates[i].Ticks / TicksPerDay, startDays, endDays);
					currentPoint.y = NormalizeY(curve.Data[i], minRange, maxRange);
					
					// draw a line only if previous and current data points are valid (i.e. at least zero)
					if (previousData >= 0f && curve.Data[i] >= 0f)
					{
						// add uvs
						uvs.AddRange(poolList);

						// compute distances between current and previous points
						float distanceX = currentPoint.x - previousPoint.x;
						float distanceY = currentPoint.y - previousPoint.y;
						float distanceXY = Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);

						// add some stuff I don't understand
						Vector3 vector5 = default(Vector3);
						vector5.x = pixelRatio * curve.Width * distanceY / (distanceXY * ratioXY);
						vector5.y = (0f - pixelRatio) * curve.Width * distanceX / distanceXY;
						vertices.Add(Vector3.Scale(previousPoint + vector5, baseSize) + center);
						vertices.Add(Vector3.Scale(currentPoint  + vector5, baseSize) + center);
						vertices.Add(Vector3.Scale(currentPoint  - vector5, baseSize) + center);
						vertices.Add(Vector3.Scale(previousPoint - vector5, baseSize) + center);
						indices.Add(vertices.Count - 4);
						indices.Add(vertices.Count - 3);
						indices.Add(vertices.Count - 2);
						indices.Add(vertices.Count - 4);
						indices.Add(vertices.Count - 2);
						indices.Add(vertices.Count - 1);
						colors.Add(item);
						colors.Add(item);
						colors.Add(item);
						colors.Add(item);

						// add more stuff I don't understand
						Vector3 vector6 = new Vector3(pixelRatio * curve.Width / ratioXY, 0f, 0f);
						Vector3 vector7 = new Vector3(0f, pixelRatio * curve.Width, 0f);
						vertices.Add(Vector3.Scale(previousPoint - vector6 - vector7, baseSize) + center);
						vertices.Add(Vector3.Scale(previousPoint + vector6 - vector7, baseSize) + center);
						vertices.Add(Vector3.Scale(previousPoint + vector6 + vector7, baseSize) + center);
						vertices.Add(Vector3.Scale(previousPoint - vector6 + vector7, baseSize) + center);
						indices.Add(vertices.Count - 4);
						indices.Add(vertices.Count - 3);
						indices.Add(vertices.Count - 2);
						indices.Add(vertices.Count - 4);
						indices.Add(vertices.Count - 2);
						indices.Add(vertices.Count - 1);
						colors.Add(curve.Color);
						colors.Add(curve.Color);
						colors.Add(curve.Color);
						colors.Add(curve.Color);
					}

					// copy current to previous
					previousPoint = currentPoint;
					previousData = curve.Data[i];
				}
			}
		}
	}
}
