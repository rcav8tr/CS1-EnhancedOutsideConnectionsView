using ColossalFramework.UI;
using System;
using UnityEngine;

namespace EnhancedOutsideConnectionsView
{
    public class EOCVHistoryPanel : UIPanel
    {
        // import and export text
        private string _textImport;
        private string _textExport;

        // checkboxes and labels for history options
        private UILabel _historyShow;
        private class HistoryOption
        {
            public UIPanel Panel;
            public UISprite Checkbox;
            public UILabel Label;
        }
        private HistoryOption _historyAll;
        private HistoryOption _historyLast10;
        private HistoryOption _historyLast25;
        private HistoryOption _historyLast50;

        // resource texts for tool tip
        private string _textGoods;
        private string _textForestry;
        private string _textFarming;
        private string _textOre;
        private string _textOil;
        private string _textMail;
        private string _textFish;

        // resource colors, oil color is very dark so make it a little lighter
        private readonly Color32 _colorGoods    = EOCVUserInterface.instance.GetResourceColor(EOCVUserInterface.ResourceType.Goods);
        private readonly Color32 _colorForestry = EOCVUserInterface.instance.GetResourceColor(EOCVUserInterface.ResourceType.Forestry);
        private readonly Color32 _colorFarming  = EOCVUserInterface.instance.GetResourceColor(EOCVUserInterface.ResourceType.Farming);
        private readonly Color32 _colorOre      = EOCVUserInterface.instance.GetResourceColor(EOCVUserInterface.ResourceType.Ore);
        private readonly Color32 _colorOil      = EOCVUserInterface.instance.GetResourceColor(EOCVUserInterface.ResourceType.Oil) * 1.5f;
        private readonly Color32 _colorMail     = EOCVUserInterface.instance.GetResourceColor(EOCVUserInterface.ResourceType.Mail);
        private readonly Color32 _colorFish     = EOCVUserInterface.instance.GetResourceColor(EOCVUserInterface.ResourceType.Fish);

        // miscellaneous
        private UIFont _defaultFont;
        private UILabel _heading;
        private EOCVGraph _graph;

        /// <summary>
        /// Start is called after the panel is created
        /// set up the panel
        /// </summary>
        public override void Start()
        {
            // do base processing
            base.Start();

            try
            {
                // set basic properties
                name = "EOCVHistoryPanel";
                backgroundSprite = "MenuPanel2";
                opacity = 1f;
                isVisible = false;  // default to hidden
                canFocus = false;
                autoSize = false;
                size = new Vector2(800f, 800f);

                // get the outside connections info view panel (displayed when the user clicks on the Outside Connections info view button)
                OutsideConnectionsInfoViewPanel ocInfoViewPanel = UIView.library.Get<OutsideConnectionsInfoViewPanel>(typeof(OutsideConnectionsInfoViewPanel).Name);
                if (ocInfoViewPanel == null)
                {
                    LogUtil.LogError("Unable to find [OutsideConnectionsInfoViewPanel].");
                    return;
                }

                // move panel to the right of OutsideConnectionsInfoViewPanel
                relativePosition = new Vector3(ocInfoViewPanel.component.size.x - 1f, 0f);

                // get default font to use for the panel
                // the view default font is OpenSans-Semibold, but OpenSans-Regular is desired
                // so copy the font from a component with that font
                _defaultFont = GetUIView().defaultFont;
                UITextComponent[] textComponents = FindObjectsOfType<UITextComponent>();
                foreach (UITextComponent textComponent in textComponents)
                {
                    UIFont font = textComponent.font;
                    if (font != null && font.isValid && font.name == "OpenSans-Regular")
                    {
                        _defaultFont = font;
                        break;
                    }
                }

                // create icon in upper left
                UISprite panelIcon = AddUIComponent<UISprite>();
                if (panelIcon == null)
                {
                    LogUtil.LogError($"Unable to create icon on panel [{name}].");
                    return;
                }
                panelIcon.name = "PanelIcon";
                panelIcon.autoSize = false;
                panelIcon.size = new Vector2(109f / 3f, 75f / 3f);    // original size is 109x75 pixels
                panelIcon.relativePosition = new Vector3(10f, 7f);
                panelIcon.spriteName = "ThumbStatistics";
                panelIcon.isVisible = true;

                // create close button
                UIButton closeButton = AddUIComponent<UIButton>();
                if (closeButton == null)
                {
                    LogUtil.LogError($"Unable to create close button on panel [{name}].");
                    return;
                }
                closeButton.name = "Close";
                closeButton.autoSize = false;
                closeButton.size = new Vector2(32f, 32f);
                closeButton.relativePosition = new Vector3(width - 34f, 2f);
                closeButton.normalBgSprite = "buttonclose";
                closeButton.hoveredBgSprite = "buttonclosehover";
                closeButton.pressedBgSprite = "buttonclosepressed";
                closeButton.eventClicked += CloseButton_eventClicked;

                // create heading label
                _heading = AddUIComponent<UILabel>();
                if (_heading == null)
                {
                    Debug.LogError($"Unable to create title label on panel [{name}].");
                    return;
                }
                _heading.name = "Title";
                _heading.font = _defaultFont;
                _heading.text = "Heading";  // gets set later
                _heading.textAlignment = UIHorizontalAlignment.Center;
                _heading.textScale = 1f;
                _heading.textColor = new Color32(254, 254, 254, 255);
                _heading.autoSize = false;
                _heading.size = new Vector2(width, 18f);
                _heading.relativePosition = new Vector3(0f, 11f);
                _heading.isVisible = true;

                // make sure icon and close button are in front of heading
                panelIcon.BringToFront();
                closeButton.BringToFront();

                // create graph
                _graph = AddUIComponent<EOCVGraph>();
                if (_graph == null)
                {
                    LogUtil.LogError($"Unable to create graph on panel [{name}].");
                    return;
                }
                _graph.name = "HistoryGraph";
                const float HeadingAreaHeight = 41f;
                const float Padding = 5f;
                _graph.autoSize = false;
                _graph.size = new Vector2(size.x - 2f * Padding - 2f * 5f, size.y - HeadingAreaHeight - 2f * Padding);
                _graph.relativePosition = new Vector3(Padding + 5f, HeadingAreaHeight + Padding);
                _graph.spriteName = "PieChartWhiteBg";
                _graph.AxesColor = new Color32(116, 149, 165, 255);
                _graph.HelpAxesColor = new Color32(52, 62, 71, 255);
                _graph.GraphRect = new Rect(0.1f, 0.05f, 0.88f, 0.89f);
                _graph.Font = _defaultFont;
                _graph.isVisible = true;

                // create Show label
                _historyShow = AddUIComponent<UILabel>();
                if (_historyShow == null)
                {
                    LogUtil.LogError($"Unable to create Show label on panel [{name}].");
                    return;
                }
                _historyShow.name = "Show";
                _historyShow.font = _defaultFont;
                _historyShow.text = "Show:";
                _historyShow.textAlignment = UIHorizontalAlignment.Left;
                _historyShow.textScale = 0.875f;
                _historyShow.textColor = new Color32(254, 254, 254, 255);
                _historyShow.autoSize = false;
                _historyShow.size = new Vector2(50f, 16f);
                _historyShow.relativePosition = new Vector3(100f, 60f);
                _historyShow.isVisible = false;     // starts hidden

                // create 4 check boxes for All, last 10 years, last 25, last 50 years
                float left = _historyShow.relativePosition.x + _historyShow.size.x + 10f;
                if (!CreateHistoryOption("AllHistory",  "All History",   left, out _historyAll   )) return; left += 150f;
                if (!CreateHistoryOption("Last10Years", "Last 10 Years", left, out _historyLast10)) return; left += 150f;
                if (!CreateHistoryOption("Last25Years", "Last 25 Years", left, out _historyLast25)) return; left += 150f;
                if (!CreateHistoryOption("Last50Years", "Last 50 Years", left, out _historyLast50)) return;

                // initialize check boxes so All is set by default
                SetCheckBox(_historyAll.Checkbox, true);
            }
            catch (Exception ex)
            {
                LogUtil.LogException(ex);
            }
        }

        /// <summary>
        /// create history option
        /// </summary>
        /// <returns></returns>
        private bool CreateHistoryOption(string namePrefix, string labelText, float left, out HistoryOption historyOption)
        {
            // create history option
            historyOption = new HistoryOption();

            // create panel
            historyOption.Panel = AddUIComponent<UIPanel>();
            if (historyOption.Panel == null)
            {
                LogUtil.LogError($"Unable to create {namePrefix} history panel on panel [{name}].");
                return false;
            }
            historyOption.Panel.name = namePrefix + "Panel";
            historyOption.Panel.autoSize = false;
            historyOption.Panel.size = new Vector2(140f, 20f);
            historyOption.Panel.relativePosition = new Vector3(left, _historyShow.relativePosition.y - 2f, 0f);
            historyOption.Panel.isVisible = false;   // starts hidden
            historyOption.Panel.eventClicked += HistoryOption_eventClicked;

            // create check box
            historyOption.Checkbox = historyOption.Panel.AddUIComponent<UISprite>();
            if (historyOption.Checkbox == null)
            {
                LogUtil.LogError($"Unable to create {namePrefix} checkbox on panel [{name}].");
                return false;
            }
            historyOption.Checkbox.name = namePrefix + "Checkbox";
            historyOption.Checkbox.autoSize = false;
            historyOption.Checkbox.size = new Vector2(15f, 15f);
            historyOption.Checkbox.relativePosition = new Vector3(5f, 0f, 0f);
            SetCheckBox(historyOption.Checkbox, false);

            // create label
            historyOption.Label = historyOption.Panel.AddUIComponent<UILabel>();
            if (historyOption.Label == null)
            {
                LogUtil.LogError($"Unable to create {namePrefix} label on panel [{name}].");
                return false;
            }
            historyOption.Label.name = namePrefix + "Label";
            historyOption.Label.text = labelText;
            historyOption.Label.textColor = new Color32(254, 254, 254, 255);
            historyOption.Label.font = _defaultFont;
            historyOption.Label.textScale = 0.875f;
            historyOption.Label.textAlignment = UIHorizontalAlignment.Left;
            historyOption.Label.autoSize = false;
            historyOption.Label.size = new Vector2(110f, 16f);
            historyOption.Label.relativePosition = new Vector3(25f, 2f, 0f);

            // success
            return true;
        }

        /// <summary>
        /// handle click on history option
        /// </summary>
        private void HistoryOption_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            // determine which history option was clicked
            HistoryOption historyOption = null;
            if (component == _historyAll   .Panel) historyOption = _historyAll;
            if (component == _historyLast10.Panel) historyOption = _historyLast10;
            if (component == _historyLast25.Panel) historyOption = _historyLast25;
            if (component == _historyLast50.Panel) historyOption = _historyLast50;

            // if clicked history option is alredy checked, then ignore click event
            if (IsCheckBoxChecked(historyOption.Checkbox))
            {
                return;
            }

            // uncheck all the history options
            SetCheckBox(_historyAll.Checkbox,    false);
            SetCheckBox(_historyLast10.Checkbox, false);
            SetCheckBox(_historyLast25.Checkbox, false);
            SetCheckBox(_historyLast50.Checkbox, false);

            // check the history option that was clicked
            SetCheckBox(historyOption.Checkbox, true);

            // update history
            EOCVUserInterface.instance.UpdateHistoryPanel();
        }

        /// <summary>
        /// return whether or not the check box is checked
        /// </summary>
        private bool IsCheckBoxChecked(UISprite checkbox)
        {
            return checkbox.spriteName == "check-checked";
        }

        /// <summary>
        /// set the check box status
        /// </summary>
        private void SetCheckBox(UISprite checkbox, bool value)
        {
            // update checkbox based on value
            if (value)
            {
                checkbox.spriteName = "check-checked";
            }
            else
            {
                checkbox.spriteName = "check-unchecked";
            }
        }

        /// <summary>
        /// handle click on Close button
        /// </summary>
        private void CloseButton_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            isVisible = false;
        }

        /// <summary>
        /// set the text for each resource type
        /// </summary>
        public void SetTexts(
            string textImport,
            string textExport,
            string textGoods,
            string textForestry,
            string textFarming,
            string textOre,
            string textOil,
            string textMail,
            string textFish)
        {
            // save import/export texts
            _textImport = textImport;
            _textExport = textExport;

            // save resource texts
            _textGoods    = textGoods;
            _textForestry = textForestry;
            _textFarming  = textFarming;
            _textOre      = textOre;
            _textOil      = textOil;
            _textMail     = textMail;
            _textFish     = textFish;
        }

        /// <summary>
        /// update the panel
        /// </summary>
        public void UpdatePanel(
            EOCVUserInterface.ConnectionDirection direction,
            bool showGoods,
            bool showForestry,
            bool showFarming,
            bool showOre,
            bool showOil,
            bool showMail,
            bool showFish)
        {
            try
            {
                // lock thread while working with the snapshots
                // because this can be called from UI thread and simulation thread
                EOCVSnapshots.instance.LockThread();

                // set the heading
                _heading.text = (direction == EOCVUserInterface.ConnectionDirection.Import ? _textImport : _textExport);

                // clear the graph
                _graph.Clear();

                // continue only if there is data
                int snapshotsCount = EOCVSnapshots.instance.Count;
                if (snapshotsCount > 0)
                {
                    // determine which history options to show based on the number of snapshots
                    if (snapshotsCount >= 12 * 10) { _historyLast10.Panel.isVisible = true; _historyAll.Panel.isVisible = true; _historyShow.isVisible = true; }
                    if (snapshotsCount >= 12 * 25) { _historyLast25.Panel.isVisible = true; }
                    if (snapshotsCount >= 12 * 50) { _historyLast50.Panel.isVisible = true; }

                    // compute the first data index to include based on the selected history option
                    int firstIndexToInclude = 0;
                    if      (IsCheckBoxChecked(_historyLast10.Checkbox)) firstIndexToInclude = snapshotsCount - 12 * 10;
                    else if (IsCheckBoxChecked(_historyLast25.Checkbox)) firstIndexToInclude = snapshotsCount - 12 * 25;
                    else if (IsCheckBoxChecked(_historyLast50.Checkbox)) firstIndexToInclude = snapshotsCount - 12 * 50;

                    // the number of points that can be included on the graph is somehow limited by the graph calculations depending on the number of curves and the number of points per curve
                    // the highest number of curves is 7 for export with all DLC enabled
                    // with 7 curves displayed, the graph allows a maximum of about 1100 points per curve before the calculations break down
                    // but 1100 points on the graph results in the points being too close together
                    // so compute the number of points to combine so that there are never more than 300 points to graph
                    int snapshotsToInclude = snapshotsCount - firstIndexToInclude;
                    int pointsToCombine = Mathf.CeilToInt((float)snapshotsToInclude / 300);
                    int arraySize = Mathf.CeilToInt((float)snapshotsToInclude / pointsToCombine);

                    // process the snapshots into arrays needed by the graph
                    const long TicksPerSecond = 10000000;
                    DateTime[] dates = new DateTime[arraySize];
                    float[] goods    = new float[arraySize];
                    float[] forestry = new float[arraySize];
                    float[] farming  = new float[arraySize];
                    float[] ore      = new float[arraySize];
                    float[] oil      = new float[arraySize];
                    float[] mail     = new float[arraySize];
                    float[] fish     = new float[arraySize];
                    int arrayIndex = 0;
                    long totalSeconds = 0;
                    int totalGoods    = 0;
                    int totalForestry = 0;
                    int totalFarming  = 0;
                    int totalOre      = 0;
                    int totalOil      = 0;
                    int totalMail     = 0;
                    int totalFish     = 0;
                    int countDates    = 0;
                    int countGoods    = 0;
                    int countForestry = 0;
                    int countFarming  = 0;
                    int countOre      = 0;
                    int countOil      = 0;
                    int countMail     = 0;
                    int countFish     = 0;
                    for (int i = firstIndexToInclude; i < snapshotsCount; i++)
                    {
                        // get the snapshot
                        EOCVSnapshot snapshot = EOCVSnapshots.instance[i];

                        // always accumulate the date seconds
                        // because all the snapshot dates have no time, this should always divide evenly
                        totalSeconds += snapshot.SnapshotDate.Ticks / TicksPerSecond;
                        countDates++;

                        // accumulate resource totals excluding the data points that are invalid
                        if (direction == EOCVUserInterface.ConnectionDirection.Import)
                        {
                            if (snapshot.ImportGoods    >= 0f) { totalGoods    += snapshot.ImportGoods;    countGoods++;    }
                            if (snapshot.ImportForestry >= 0f) { totalForestry += snapshot.ImportForestry; countForestry++; }
                            if (snapshot.ImportFarming  >= 0f) { totalFarming  += snapshot.ImportFarming;  countFarming++;  }
                            if (snapshot.ImportOre      >= 0f) { totalOre      += snapshot.ImportOre;      countOre++;      }
                            if (snapshot.ImportOil      >= 0f) { totalOil      += snapshot.ImportOil;      countOil++;      }
                            if (snapshot.ImportMail     >= 0f) { totalMail     += snapshot.ImportMail;     countMail++;     }
                        }
                        else
                        {
                            if (snapshot.ExportGoods    >= 0f) { totalGoods    += snapshot.ExportGoods;    countGoods++;    }
                            if (snapshot.ExportForestry >= 0f) { totalForestry += snapshot.ExportForestry; countForestry++; }
                            if (snapshot.ExportFarming  >= 0f) { totalFarming  += snapshot.ExportFarming;  countFarming++;  }
                            if (snapshot.ExportOre      >= 0f) { totalOre      += snapshot.ExportOre;      countOre++;      }
                            if (snapshot.ExportOil      >= 0f) { totalOil      += snapshot.ExportOil;      countOil++;      }
                            if (snapshot.ExportMail     >= 0f) { totalMail     += snapshot.ExportMail;     countMail++;     }
                            if (snapshot.ExportFish     >= 0f) { totalFish     += snapshot.ExportFish;     countFish++;     }
                        }

                        // check if the number of points to combine have been accumulated
                        // also handle the last array entry, which may not count up to pointsToCombine
                        if (countDates == pointsToCombine || i == snapshotsCount - 1)
                        {
                            // compute average date
                            long averageSeconds = totalSeconds / countDates;
                            dates[arrayIndex] = new DateTime(averageSeconds * TicksPerSecond).Date;

                            // compute average of data points
                            goods   [arrayIndex] = (countGoods    == 0 ? -1f : (float)totalGoods    / countGoods   );
                            forestry[arrayIndex] = (countForestry == 0 ? -1f : (float)totalForestry / countForestry);
                            farming [arrayIndex] = (countFarming  == 0 ? -1f : (float)totalFarming  / countFarming );
                            ore     [arrayIndex] = (countOre      == 0 ? -1f : (float)totalOre      / countOre     );
                            oil     [arrayIndex] = (countOil      == 0 ? -1f : (float)totalOil      / countOil     );
                            mail    [arrayIndex] = (countMail     == 0 ? -1f : (float)totalMail     / countMail    );
                            fish    [arrayIndex] = (countFish     == 0 ? -1f : (float)totalFish     / countFish    );

                            // reset totals
                            totalSeconds  = 0;
                            totalGoods    = 0;
                            totalForestry = 0;
                            totalFarming  = 0;
                            totalOre      = 0;
                            totalOil      = 0;
                            totalMail     = 0;
                            totalFish     = 0;

                            // reset counts
                            countDates    = 0;
                            countGoods    = 0;
                            countForestry = 0;
                            countFarming  = 0;
                            countOre      = 0;
                            countOil      = 0;
                            countMail     = 0;
                            countFish     = 0;

                            // go to next array index
                            arrayIndex++;
                        }
                    }

                    // populate the graph
                    const float CurveWidth = 0.5f;
                    _graph.SetDates(dates);
                    if (showGoods   ) _graph.AddCurve("Goods",    _textGoods,    goods,    CurveWidth, _colorGoods   );
                    if (showForestry) _graph.AddCurve("Forestry", _textForestry, forestry, CurveWidth, _colorForestry);
                    if (showFarming ) _graph.AddCurve("Farming",  _textFarming,  farming,  CurveWidth, _colorFarming );
                    if (showOre     ) _graph.AddCurve("Ore",      _textOre,      ore,      CurveWidth, _colorOre     );
                    if (showOil     ) _graph.AddCurve("Oil",      _textOil,      oil,      CurveWidth, _colorOil     );
                    if (showMail    ) _graph.AddCurve("Mail",     _textMail,     mail,     CurveWidth, _colorMail    );
                    if (showFish    ) _graph.AddCurve("Fish",     _textFish,     fish,     CurveWidth, _colorFish    );

                    // refresh the graph
                    _graph.Invalidate();
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogException(ex);
            }
            finally
            {
                // make sure thread is unlocked
                EOCVSnapshots.instance.UnlockThread();
            }
        }

    }
}
