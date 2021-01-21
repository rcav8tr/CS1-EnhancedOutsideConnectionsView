using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using System;
using ColossalFramework.Math;
using System.Collections.Generic;

namespace EnhancedOutsideConnectionsView
{
    /// <summary>
    /// handle all user interface functions
    /// </summary>
    public static class EOCVUserInterface
    {
        // import vs export
        private enum ConnectionDirection
        {
            Import,
            Export
        }

        // resources
        private enum ResourceType
        {
            None,
            Goods,
            Forestry,
            Farming,
            Ore,
            Oil,
            Mail,
            Fish
        }

        // UI information for a resource
        private class UIResource
        {
            public ConnectionDirection Direction;
            public ResourceType Type;
            public UIPanel OriginalPanel;
            public UIPanel Panel;
            public UISprite CheckBox;
            public UISprite ColorSprite;
            public UILabel Description;
            public UILabel Count;
            public Color ResourceColor;
        }

        // the individual resources
        private static UIResource _importGoods;
        private static UIResource _importForestry;
        private static UIResource _importFarming;
        private static UIResource _importOre;
        private static UIResource _importOil;
        private static UIResource _importMail;

        private static UIResource _exportGoods;
        private static UIResource _exportForestry;
        private static UIResource _exportFarming;
        private static UIResource _exportOre;
        private static UIResource _exportOil;
        private static UIResource _exportMail;
        private static UIResource _exportFish;

        // a list of the resources
        private static List<UIResource> _resources;

        // UI information for a total
        private class UITotal
        {
            public ConnectionDirection Direction;
            public UIRadialChart Chart;
            public UISprite Line;
            public UILabel Text;
            public UILabel Total;
        }

        // the two totals
        private static UITotal _importTotal;
        private static UITotal _exportTotal;

        // number format
        private static string _numberFormat;

        // text colors
        private static Color32 _textColorNormal;
        private static Color32 _textColorDisabled;

        // initialization for UpdatePanel
        private static bool _updatePanelInitialized = false;

        /// <summary>
        /// initialize the user interface
        /// </summary>
        /// <returns>success status</returns>
        public static bool Initialize()
        {
            try
            {
                // find Ingame atlas
                UITextureAtlas ingameAtlas = null;
                UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
                for (int i = 0; i < atlases.Length; i++)
                {
                    if (atlases[i] != null)
                    {
                        if (atlases[i].name == "Ingame")
                        {
                            ingameAtlas = atlases[i];
                            break;
                        }
                    }
                }
                if (ingameAtlas == null)
                {
                    Debug.LogError("Unable to find atlas [Ingame].");
                    return false;
                }

                // get the outside connections info view panel (displayed when the user clicks on the Outside Connections info view button)
                OutsideConnectionsInfoViewPanel ocInfoViewPanel = UIView.library.Get<OutsideConnectionsInfoViewPanel>(typeof(OutsideConnectionsInfoViewPanel).Name);
                if (ocInfoViewPanel == null)
                {
                    Debug.LogError("Unable to find [OutsideConnectionsInfoViewPanel].");
                    return false;
                }

                // increase height of OC view panel to make room to vertically stack the resources in the legend
                // this also increases the height of the import/export legend panels because they are anchored to the bottom
                ocInfoViewPanel.component.size = new Vector2(ocInfoViewPanel.component.size.x, ocInfoViewPanel.component.size.y + 30f);

                // find the import/export panels on the OC view panel
                // these are the main import/export panels that the base processing shows/hides when the Import/Export tabs are clicked
                if (!FindImportExportPanel(ocInfoViewPanel, "Import", out UIPanel importPanel)) return false;
                if (!FindImportExportPanel(ocInfoViewPanel, "Export", out UIPanel exportPanel)) return false;

                // find the import/export legend panels
                // this is the panel filling the bottom portion of the import/export panel that has the text "Legend" at the top of the panel
                if (!Find(importPanel, "ImportLegend", out UIPanel importLegendPanel)) return false;
                if (!Find(exportPanel, "ExportLegend", out UIPanel exportLegendPanel)) return false;

                // move legend panels up into the space that will be vacated when the total labels are hidden
                const float MoveLegend = 45f;
                importLegendPanel.relativePosition = new Vector3(importLegendPanel.relativePosition.x, importLegendPanel.relativePosition.y - MoveLegend, importLegendPanel.relativePosition.z);
                exportLegendPanel.relativePosition = new Vector3(exportLegendPanel.relativePosition.x, exportLegendPanel.relativePosition.y - MoveLegend, exportLegendPanel.relativePosition.z);

                // moving the legend panels up does NOT change the height, so increase legend panel height by the amount moved up
                importLegendPanel.size = new Vector2(importLegendPanel.size.x, importLegendPanel.size.y + MoveLegend);
                exportLegendPanel.size = new Vector2(exportLegendPanel.size.x, exportLegendPanel.size.y + MoveLegend);

                // create the resources on each legend panel
                _resources = new List<UIResource>();
                const float ResourcePanelTopStart = 20f;
                const float ResourcePanelSpacing = 25f;
                float resourcePanelTop = ResourcePanelTopStart;
                if (!CreateResource(importLegendPanel, ConnectionDirection.Import, ResourceType.Goods,    resourcePanelTop, ingameAtlas, out _importGoods   )) { return false; } resourcePanelTop += ResourcePanelSpacing;
                if (!CreateResource(importLegendPanel, ConnectionDirection.Import, ResourceType.Forestry, resourcePanelTop, ingameAtlas, out _importForestry)) { return false; } resourcePanelTop += ResourcePanelSpacing;
                if (!CreateResource(importLegendPanel, ConnectionDirection.Import, ResourceType.Farming,  resourcePanelTop, ingameAtlas, out _importFarming )) { return false; } resourcePanelTop += ResourcePanelSpacing;
                if (!CreateResource(importLegendPanel, ConnectionDirection.Import, ResourceType.Ore,      resourcePanelTop, ingameAtlas, out _importOre     )) { return false; } resourcePanelTop += ResourcePanelSpacing;
                if (!CreateResource(importLegendPanel, ConnectionDirection.Import, ResourceType.Oil,      resourcePanelTop, ingameAtlas, out _importOil     )) { return false; } resourcePanelTop += ResourcePanelSpacing;
                if (!CreateResource(importLegendPanel, ConnectionDirection.Import, ResourceType.Mail,     resourcePanelTop, ingameAtlas, out _importMail    )) { return false; } resourcePanelTop += ResourcePanelSpacing;

                resourcePanelTop = ResourcePanelTopStart;
                if (!CreateResource(exportLegendPanel, ConnectionDirection.Export, ResourceType.Goods,    resourcePanelTop, ingameAtlas, out _exportGoods   )) { return false; } resourcePanelTop += ResourcePanelSpacing;
                if (!CreateResource(exportLegendPanel, ConnectionDirection.Export, ResourceType.Forestry, resourcePanelTop, ingameAtlas, out _exportForestry)) { return false; } resourcePanelTop += ResourcePanelSpacing;
                if (!CreateResource(exportLegendPanel, ConnectionDirection.Export, ResourceType.Farming,  resourcePanelTop, ingameAtlas, out _exportFarming )) { return false; } resourcePanelTop += ResourcePanelSpacing;
                if (!CreateResource(exportLegendPanel, ConnectionDirection.Export, ResourceType.Ore,      resourcePanelTop, ingameAtlas, out _exportOre     )) { return false; } resourcePanelTop += ResourcePanelSpacing;
                if (!CreateResource(exportLegendPanel, ConnectionDirection.Export, ResourceType.Oil,      resourcePanelTop, ingameAtlas, out _exportOil     )) { return false; } resourcePanelTop += ResourcePanelSpacing;
                if (!CreateResource(exportLegendPanel, ConnectionDirection.Export, ResourceType.Mail,     resourcePanelTop, ingameAtlas, out _exportMail    )) { return false; } resourcePanelTop += ResourcePanelSpacing;
                if (!CreateResource(exportLegendPanel, ConnectionDirection.Export, ResourceType.Fish,     resourcePanelTop, ingameAtlas, out _exportFish    )) { return false; } resourcePanelTop += ResourcePanelSpacing;

                // save the text colors
                _textColorNormal = _importGoods.Description.textColor;
                const float DisabledTextMultiplier = 0.7f;
                _textColorDisabled = new Color32((byte)(_textColorNormal.r * DisabledTextMultiplier), (byte)(_textColorNormal.g * DisabledTextMultiplier), (byte)(_textColorNormal.b * DisabledTextMultiplier), 255);

                // hide resources unique to unowned DLC
                if (!SteamHelper.IsDLCOwned(SteamHelper.DLC.IndustryDLC))
                {
                    _importMail.Panel.isVisible = false;
                    _exportMail.Panel.isVisible = false;
                }
                if (!SteamHelper.IsDLCOwned(SteamHelper.DLC.UrbanDLC))      // i.e. Sunset Harbor
                {
                    _exportFish.Panel.isVisible = false;
                }

                // determine last resource based on owned DLC
                UIResource lastImportResource = _importOil;
                UIResource lastExportResource = _exportOil;
                if (SteamHelper.IsDLCOwned(SteamHelper.DLC.IndustryDLC))
                {
                    lastImportResource = _importMail;
                    lastExportResource = _exportMail;
                    if (SteamHelper.IsDLCOwned(SteamHelper.DLC.UrbanDLC))  // i.e. Sunset Harbor
                    {
                        lastExportResource = _exportFish;
                    }
                }
                else
                {
                    if (SteamHelper.IsDLCOwned(SteamHelper.DLC.UrbanDLC))  // i.e. Sunset Harbor
                    {
                        lastExportResource = _exportFish;

                        // Industries: no, Sunset Harbor: yes
                        // move fish resource panel up to where mail is
                        _exportFish.Panel.relativePosition = _exportMail.Panel.relativePosition;
                    }
                }

                // create totals beneath the last resource
                if (!CreateTotal(importPanel, importLegendPanel, lastImportResource, ConnectionDirection.Import, ingameAtlas, out _importTotal)) return false;
                if (!CreateTotal(exportPanel, exportLegendPanel, lastExportResource, ConnectionDirection.Export, ingameAtlas, out _exportTotal)) return false;

                // load config file, if any
                EOCVConfiguration config = Configuration<EOCVConfiguration>.Load();

                // set check boxes according to config values
                SetCheckBox(_importGoods,    config.ImportGoods);
                SetCheckBox(_importForestry, config.ImportForestry);
                SetCheckBox(_importFarming,  config.ImportFarming);
                SetCheckBox(_importOre,      config.ImportOre);
                SetCheckBox(_importOil,      config.ImportOil);
                SetCheckBox(_importMail,     config.ImportMail);

                SetCheckBox(_exportGoods,    config.ExportGoods);
                SetCheckBox(_exportForestry, config.ExportForestry);
                SetCheckBox(_exportFarming,  config.ExportFarming);
                SetCheckBox(_exportOre,      config.ExportOre);
                SetCheckBox(_exportOil,      config.ExportOil);
                SetCheckBox(_exportMail,     config.ExportMail);
                SetCheckBox(_exportFish,     config.ExportFish);

                // create patches for all building AIs where the GetColor routine has logic for Outside Connections
                BuildingAIPatch.CreateGetColorPatch<CommercialBuildingAI>();
                BuildingAIPatch.CreateGetColorPatch(   "PloppableRICO.GrowableCommercialAI");       // derives from CommercialBuildingAI
                BuildingAIPatch.CreateGetColorPatch(      "PloppableRICO.PloppableCommercialAI");   // derives from PloppableRICO.GrowableCommercialAI
                BuildingAIPatch.CreateGetColorPatch<IndustrialBuildingAI>();
                BuildingAIPatch.CreateGetColorPatch(   "PloppableRICO.GrowableIndustrialAI");       // derives from IndustrialBuildingAI
                BuildingAIPatch.CreateGetColorPatch(      "PloppableRICO.PloppableIndustrialAI");   // derives from PloppableRICO.GrowableIndustrialAI
                BuildingAIPatch.CreateGetColorPatch<IndustrialExtractorAI>();
                BuildingAIPatch.CreateGetColorPatch<   LivestockExtractorAI>();                     // derives from IndustrialExtractorAI
                BuildingAIPatch.CreateGetColorPatch(   "PloppableRICO.GrowableExtractorAI");        // derives from IndustrialExtractorAI
                BuildingAIPatch.CreateGetColorPatch(      "PloppableRICO.PloppableExtractorAI");    // derives from PloppableRICO.GrowableExtractorAI
                BuildingAIPatch.CreateGetColorPatch<OfficeBuildingAI>();
                BuildingAIPatch.CreateGetColorPatch(   "PloppableRICO.GrowableOfficeAI");           // derives from OfficeBuildingAI
                BuildingAIPatch.CreateGetColorPatch(      "PloppableRICO.PloppableOfficeAI");       // derives from PloppableRICO.GrowableOfficeAI

                BuildingAIPatch.CreateGetColorPatch<ExtractingFacilityAI >();
                BuildingAIPatch.CreateGetColorPatch<FishFarmAI           >();
                BuildingAIPatch.CreateGetColorPatch<FishingHarborAI      >();
                BuildingAIPatch.CreateGetColorPatch<HeatingPlantAI       >();
                BuildingAIPatch.CreateGetColorPatch<MarketAI             >();
                BuildingAIPatch.CreateGetColorPatch<PostOfficeAI         >();
                BuildingAIPatch.CreateGetColorPatch<PowerPlantAI         >();
                BuildingAIPatch.CreateGetColorPatch<   DamPowerHouseAI   >(); // derives from PowerPlantAI
                BuildingAIPatch.CreateGetColorPatch<   FusionPowerPlantAI>(); // derives from PowerPlantAI
                BuildingAIPatch.CreateGetColorPatch<   SolarPowerPlantAI >(); // derives from PowerPlantAI
                BuildingAIPatch.CreateGetColorPatch<   WindTurbineAI     >(); // derives from PowerPlantAI
                BuildingAIPatch.CreateGetColorPatch<ProcessingFacilityAI >();
                BuildingAIPatch.CreateGetColorPatch<   UniqueFactoryAI   >(); // derives from ProcessingFacilityAI
                BuildingAIPatch.CreateGetColorPatch<ShelterAI            >();
                BuildingAIPatch.CreateGetColorPatch<WarehouseAI          >();

                // create patches for all vehicle AIs where the GetColor routine has logic for Outside Connections
                VehicleAIPatch.CreateGetColorPatch<CargoPlaneAI>();     // derives from AircraftAI, but AircraftAI does not have its own GetColor routine
                VehicleAIPatch.CreateGetColorPatch<CargoShipAI >();     // derives from ShipAI,     but ShipAI     does not have its own GetColor routine
                VehicleAIPatch.CreateGetColorPatch<CargoTrainAI>();     // derives from TrainAI,    but TrainAI    does not have its own GetColor routine

                VehicleAIPatch.CreateGetColorPatch<CargoTruckAI>();
                VehicleAIPatch.CreateGetColorPatch<PostVanAI   >();

                // create update panel patch
                _updatePanelInitialized = false;
                OCIVPPatch.CreateUpdatePanelPatch();

                // success
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }


        /// <summary>
        /// create a resource on the specified legend panel
        /// basically, the existing resource panel is hidden and a new resource panel is created from scratch
        /// </summary>
        /// <returns>success status</returns>
        private static bool CreateResource(UIPanel legendPanel, ConnectionDirection direction, ResourceType type, float resourcePanelTop, UITextureAtlas ingameAtlas, out UIResource resource)
        {
            // create new resource
            resource = new UIResource();
            resource.Direction = direction;
            resource.Type = type;

            // hide the original resource panel
            // most, not all, resource panels are named the same, so use the resource color sprite to find the desired panel
            string originalColorSpriteName = (type == ResourceType.Farming ? "AgricultureColor" : type.ToString() + "Color");
            if (!Find(legendPanel, originalColorSpriteName, out UISprite originalColorSprite)) return false;
            if (originalColorSprite.parent.GetType() != typeof(UIPanel))
            {
                Debug.LogError($"Parent is not a UIPanel for sprite [{originalColorSpriteName}] on panel [{legendPanel.name}].");
                return false;
            }
            resource.OriginalPanel = (UIPanel)originalColorSprite.parent;
            resource.OriginalPanel.isVisible = false;

            // set component name prefix to the direction and resource type
            string componentNamePrefix = direction.ToString() + type.ToString();

            // create a new resource panel on the legend panel
            resource.Panel = legendPanel.AddUIComponent<UIPanel>();
            if (resource.Panel == null)
            {
                Debug.LogError($"Unable to create panel for resource [{componentNamePrefix}] on legend panel [{legendPanel.name}].");
                return false;
            }
            resource.Panel.name = componentNamePrefix + "Panel";
            resource.Panel.autoSize = false;
            resource.Panel.size = new Vector2(legendPanel.size.x, 23f);
            resource.Panel.relativePosition = new Vector3(0f, resourcePanelTop, 0f);
            resource.Panel.isVisible = true;

            // set up click event handler
            // a click on any contained component triggers a click event on the panel
            // therefore, each individual component does not need its own click event handler
            resource.Panel.eventClicked += ResourcePanel_eventClicked;

            // create the check box
            resource.CheckBox = resource.Panel.AddUIComponent<UISprite>();
            if (resource.CheckBox == null)
            {
                Debug.LogError($"Unable to create check box sprite for resource [{componentNamePrefix}] on resource panel [{resource.Panel.name}].");
                return false;
            }
            resource.CheckBox.name = componentNamePrefix + "CheckBox";
            resource.CheckBox.autoSize = false;
            const float CheckBoxSize = 15f;
            resource.CheckBox.size = new Vector2(CheckBoxSize, CheckBoxSize);
            resource.CheckBox.relativePosition = new Vector3(5f, (resource.Panel.size.y - resource.CheckBox.size.y) / 2f, 0f);    // centered vertically
            resource.CheckBox.atlas = ingameAtlas;
            resource.CheckBox.isVisible = true;

            // create a new color sprite
            // use same size and sprite as the original
            resource.ColorSprite = resource.Panel.AddUIComponent<UISprite>();
            if (resource.ColorSprite == null)
            {
                Debug.LogError($"Unable to create check box sprite for resource [{componentNamePrefix}] on resource panel [{resource.Panel.name}].");
                return false;
            }
            resource.ColorSprite.name = componentNamePrefix + "Color";
            resource.ColorSprite.autoSize = false;
            resource.ColorSprite.size = originalColorSprite.size;
            resource.ColorSprite.relativePosition = new Vector3(resource.CheckBox.relativePosition.x + resource.CheckBox.size.x + 5f, (resource.Panel.size.y - resource.ColorSprite.size.y) / 2f, 0f);    // centered vertically
            resource.ColorSprite.atlas = originalColorSprite.atlas;
            resource.ColorSprite.spriteName = originalColorSprite.spriteName;
            resource.ColorSprite.color = resource.ResourceColor = GetResourceColor(type);
            resource.ColorSprite.isVisible = true;

            // find the original description, they are all named "Type"
            if (!Find(resource.OriginalPanel, "Type", out UILabel originalDescription)) return false;

            // create a new description label
            // use the same font and text attributes as the original description
            resource.Description = resource.Panel.AddUIComponent<UILabel>();
            if (resource.Description == null)
            {
                Debug.LogError($"Unable to create count label for resource [{componentNamePrefix}] on resource panel [{resource.Panel.name}].");
                return false;
            }
            resource.Description.name = componentNamePrefix + "Description";
            resource.Description.text = originalDescription.text;
            resource.Description.textAlignment = UIHorizontalAlignment.Left;
            resource.Description.verticalAlignment = UIVerticalAlignment.Top;
            resource.Description.font = originalDescription.font;
            resource.Description.textScale = originalDescription.textScale;
            resource.Description.textColor = originalDescription.textColor;
            resource.Description.autoSize = false;
            resource.Description.size = new Vector2(200f, 16f);
            resource.Description.relativePosition = new Vector3(resource.ColorSprite.relativePosition.x + resource.ColorSprite.size.x + 5f, 5f, 0f);
            resource.Description.isVisible = true;

            // create the count label
            // use the same font and text attributes as the original description
            resource.Count = resource.Panel.AddUIComponent<UILabel>();
            if (resource.Count == null)
            {
                Debug.LogError($"Unable to create count label for resource [{componentNamePrefix}] on resource panel [{resource.Panel.name}].");
                return false;
            }
            resource.Count.name = componentNamePrefix + "Count";
            resource.Count.text = "99,999,999";
            resource.Count.textAlignment = UIHorizontalAlignment.Right;
            resource.Count.verticalAlignment = UIVerticalAlignment.Top;
            resource.Count.font = originalDescription.font;
            resource.Count.textScale = originalDescription.textScale;
            resource.Count.textColor = originalDescription.textColor;
            resource.Count.autoSize = false;
            resource.Count.size = new Vector2(82f, resource.Description.size.y);
            resource.Count.relativePosition = new Vector3(resource.Panel.size.x - resource.Count.size.x - 5f, resource.Description.relativePosition.y);
            resource.Count.isVisible = true;

            // add the resource to the list
            _resources.Add(resource);

            // success
            return true;
        }

        /// <summary>
        /// handle click on a resource panel to show/hide that resource
        /// a click on any contained component (i.e. check box, color sprite, description, count) triggers this click event
        /// </summary>
        private static void ResourcePanel_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            try
            {
                // find the resource with the panel that was clicked
                UIPanel clickedPanel = (UIPanel)component;
                foreach (UIResource resource in _resources)
                {
                    if (clickedPanel == resource.Panel)
                    {
                        // found it, toggle the check box
                        SetCheckBox(resource, !IsCheckBoxChecked(resource));
                        
                        // load the config
                        EOCVConfiguration config = Configuration<EOCVConfiguration>.Load();

                        // update the config properties
                        config.ImportGoods    = IsCheckBoxChecked(_importGoods);
                        config.ImportForestry = IsCheckBoxChecked(_importForestry);
                        config.ImportFarming  = IsCheckBoxChecked(_importFarming);
                        config.ImportOre      = IsCheckBoxChecked(_importOre);
                        config.ImportOil      = IsCheckBoxChecked(_importOil);
                        config.ImportMail     = IsCheckBoxChecked(_importMail);

                        config.ExportGoods    = IsCheckBoxChecked(_exportGoods);
                        config.ExportForestry = IsCheckBoxChecked(_exportForestry);
                        config.ExportFarming  = IsCheckBoxChecked(_exportFarming);
                        config.ExportOre      = IsCheckBoxChecked(_exportOre);
                        config.ExportOil      = IsCheckBoxChecked(_exportOil);
                        config.ExportMail     = IsCheckBoxChecked(_exportMail);
                        config.ExportFish     = IsCheckBoxChecked(_exportFish);

                        // save the config
                        Configuration<EOCVConfiguration>.Save();

                        // update colors on all buildings
                        Singleton<BuildingManager>.instance.UpdateBuildingColors();

                        // done finding
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// return whether or not the resource's check box is checked
        /// </summary>
        private static bool IsCheckBoxChecked(UIResource resource)
        {
            return resource.CheckBox.spriteName == "check-checked";
        }

        /// <summary>
        /// set the check box status and update other components in the resource
        /// </summary>
        private static void SetCheckBox(UIResource resource, bool value)
        {
            // update resource based on value
            if (value)
            {
                // set check box to checked
                resource.CheckBox.spriteName = "check-checked";

                // set sprite to normal color
                resource.ColorSprite.color = resource.ResourceColor;

                // set description to normal text color
                resource.Description.textColor = _textColorNormal;

                // show count
                resource.Count.isVisible = true;
            }
            else
            {
                // set check box to unchecked
                resource.CheckBox.spriteName = "check-unchecked";

                // set sprite to resource color mixed with neutral color
                Color neutralColor = Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                resource.ColorSprite.color = Color.Lerp(resource.ResourceColor, neutralColor, 0.5f);

                // set description text to a darker shade of the normal text color
                resource.Description.textColor = _textColorDisabled;

                // hide count
                resource.Count.isVisible = false;
            }
        }
        
        /// <summary>
        /// get the resource color for the resource type
        /// same colors are used for import and export
        /// </summary>
        private static Color GetResourceColor(ResourceType type)
        {
            // translate resource type to transfer reason
            int reason;
            switch (type)
            {
                // these are the reasons used in OutsideConnectionsInfoViewPanel in SetupImportLegend and SetupExportLegend
                case ResourceType.Goods:    reason = (int)TransferManager.TransferReason.Goods; break;
                case ResourceType.Forestry: reason = (int)TransferManager.TransferReason.Logs;  break;
                case ResourceType.Farming:  reason = (int)TransferManager.TransferReason.Grain; break;
                case ResourceType.Ore:      reason = (int)TransferManager.TransferReason.Ore;   break;
                case ResourceType.Oil:      reason = (int)TransferManager.TransferReason.Oil;   break;
                case ResourceType.Mail:     reason = (int)TransferManager.TransferReason.Mail;  break;
                case ResourceType.Fish:     reason = (int)TransferManager.TransferReason.Fish;  break;
                default:
                    Debug.LogError($"Unable to translate resource type [{type}] to resource color.");
                    return Color.black;
            }

            // get resource colors, same colors are used for both import and export
            // do not get colors from color sprites because they might not be initialized yet
            return Singleton<TransferManager>.instance.m_properties.m_resourceColors[reason];
        }

        /// <summary>
        /// create totals
        /// </summary>
        private static bool CreateTotal(UIPanel panel, UIPanel legendPanel, UIResource lastResource, ConnectionDirection direction, UITextureAtlas ingameAtlas, out UITotal total)
        {
            // create new total
            total = new UITotal();
            total.Direction = direction;

            // hide the original total label, which contained both the total text and total count
            string originalTotalLabelName = (direction == ConnectionDirection.Import ? "ImportTotal" : "ExportTotal");
            if (!Find(panel, originalTotalLabelName, out UILabel originalTotal)) return false;
            originalTotal.isVisible = false;

            // parse the format string for original total label to get both the total text and number format
            string format = ColossalFramework.Globalization.Locale.Get(originalTotal.localeID);
            int leftBracePosition = format.IndexOf("{");
            if (leftBracePosition == -1)
            {
                Debug.LogError($"Unable to find left brace in format string [{format}] of [{originalTotalLabelName}].");
                return false;
            }
            string totalText = format.Substring(0, leftBracePosition).Trim();   // total text is everything up to but not including the left brace
            _numberFormat = format.Substring(leftBracePosition).Trim();         // number format is everything from the left brace to the end

            // hide original radial chart
            string originalChartName = (direction == ConnectionDirection.Import ? "ImportChart" : "ExportChart");
            UIRadialChart originalChart = panel.Find<UIRadialChart>(originalChartName);
            if (originalChart == null)
            {
                Debug.LogError($"Unable to find chart [{originalChartName}] on panel [{panel.name}].");
                return false;
            }
            originalChart.isVisible = false;

            // set component name prefix based on connection direction
            string componentNamePrefix = direction.ToString() + "Total";

            // create a new radial chart on the import/export panel
            total.Chart = panel.AddUIComponent<UIRadialChart>();
            if (total.Chart == null)
            {
                Debug.LogError($"Unable to create radial chart for panel [{panel.name}].");
                return false;
            }
            total.Chart.name = componentNamePrefix + "Chart";
            total.Chart.size = originalChart.size;
            total.Chart.anchor = originalChart.anchor;
            total.Chart.relativePosition = originalChart.relativePosition;
            total.Chart.atlas = ingameAtlas;
            total.Chart.spriteName = "PieChartBg";
            total.Chart.flip = UISpriteFlip.FlipVertical;     // flip about the vertical so pie slices proceed clockwise
            total.Chart.isVisible = true;

            // create the chart slices
            // create slices for all possible resources, slices for unowned DLC will simply be zero
            int slices = (direction == ConnectionDirection.Import ? 6 : 7);
            for (int i = 0; i < slices; i++)
            {
                total.Chart.AddSlice();
            }

            // set the colors of the radial chart slices according to the order that the resources will be displayed
            UIRadialChart.SliceSettings slice;
            if (slices >= 1) { slice = total.Chart.GetSlice(0); slice.innerColor = slice.outterColor = GetResourceColor(ResourceType.Goods   ); }
            if (slices >= 2) { slice = total.Chart.GetSlice(1); slice.innerColor = slice.outterColor = GetResourceColor(ResourceType.Forestry); }
            if (slices >= 3) { slice = total.Chart.GetSlice(2); slice.innerColor = slice.outterColor = GetResourceColor(ResourceType.Farming ); }
            if (slices >= 4) { slice = total.Chart.GetSlice(3); slice.innerColor = slice.outterColor = GetResourceColor(ResourceType.Ore     ); }
            if (slices >= 5) { slice = total.Chart.GetSlice(4); slice.innerColor = slice.outterColor = GetResourceColor(ResourceType.Oil     ); }
            if (slices >= 6) { slice = total.Chart.GetSlice(5); slice.innerColor = slice.outterColor = GetResourceColor(ResourceType.Mail    ); }
            if (slices >= 7) { slice = total.Chart.GetSlice(6); slice.innerColor = slice.outterColor = GetResourceColor(ResourceType.Fish    ); }

            // draw a line on the legend panel under last resource panel
            total.Line = legendPanel.AddUIComponent<UISprite>();
            if (total.Line == null)
            {
                Debug.LogError($"Unable to create line sprite for legend panel [{legendPanel.name}].");
                return false;
            }
            total.Line.name = componentNamePrefix + "Line";
            total.Line.autoSize = false;
            total.Line.size = new Vector2(lastResource.Count.size.x, 5f);
            total.Line.relativePosition = new Vector3(lastResource.Panel.relativePosition.x + lastResource.Count.relativePosition.x, lastResource.Panel.relativePosition.y + lastResource.Panel.size.y);
            total.Line.atlas = ingameAtlas;
            total.Line.spriteName = "ButtonMenuMain";
            total.Line.isVisible = true;

            // create a new label on the legend panel for total text
            total.Text = legendPanel.AddUIComponent<UILabel>();
            if (total.Text == null)
            {
                Debug.LogError($"Unable to create total text label on legend panel [{legendPanel.name}].");
                return false;
            }
            total.Text.name = componentNamePrefix + "Text";
            total.Text.text = totalText;
            total.Text.textAlignment = UIHorizontalAlignment.Right;
            total.Text.verticalAlignment = UIVerticalAlignment.Top;
            total.Text.font = lastResource.Count.font;
            total.Text.textScale = lastResource.Count.textScale;
            total.Text.textColor = lastResource.Count.textColor;
            total.Text.autoSize = false;
            total.Text.size = new Vector2(200f, lastResource.Count.size.y);
            total.Text.relativePosition = new Vector3(total.Line.relativePosition.x - 5f - total.Text.size.x, total.Line.relativePosition.y + 14f);
            total.Text.isVisible = true;
            total.Text.BringToFront();

            // create a new label on the legend panel for total count
            total.Total = legendPanel.AddUIComponent<UILabel>();
            if (total.Total == null)
            {
                Debug.LogError($"Unable to create total count label on legend panel [{legendPanel.name}].");
                return false;
            }
            total.Total.name = componentNamePrefix + "Total";
            total.Total.text = "99,999,999";
            total.Total.textAlignment = UIHorizontalAlignment.Right;
            total.Total.verticalAlignment = UIVerticalAlignment.Top;
            total.Total.font = lastResource.Count.font;
            total.Total.textScale = lastResource.Count.textScale;
            total.Total.textColor = lastResource.Count.textColor;
            total.Total.autoSize = false;
            total.Total.size = lastResource.Count.size;
            total.Total.relativePosition = new Vector3(total.Line.relativePosition.x, total.Text.relativePosition.y);
            total.Total.isVisible = true;
            total.Total.BringToFront();

            // success
            return true;
        }

        /// <summary>
        /// find the named import/export panel on the OC info view panel
        /// </summary>
        /// <returns>success status</returns>
        private static bool FindImportExportPanel(OutsideConnectionsInfoViewPanel ocInfoViewPanel, string panelName, out UIPanel panel)
        {
            // find the panel
            panel = ocInfoViewPanel.Find<UIPanel>(panelName);
            if (panel == null)
            {
                Debug.LogError($"Unable to find panel [{panelName}] on [{ocInfoViewPanel.name}].");
                return false;
            }

            // panel found
            return true;
        }

        /// <summary>
        /// find the named component on the specified panel
        /// </summary>
        /// <returns>success status</returns>
        private static bool Find<T>(UIPanel onPanel, string componentName, out T component) where T : UIComponent
        {
            // find the component
            component = onPanel.Find<T>(componentName);
            if (component == null)
            {
                Debug.LogError($"Unable to find component type [{typeof(T)}] named [{componentName}] on panel [{onPanel.name}].");
                return false;
            }

            // component found
            return true;
        }

        /// <summary>
        /// update the user interface
        /// </summary>
        public static void UpdatePanel()
        {
            try
            {
                // check conditions
                if (!_updatePanelInitialized)
                {
                    // hide the original resource panels
                    // must do this here because the visibility gets set by the base processing according to owned DLC when the OC info view panel is first displayed
                    _importMail.OriginalPanel.isVisible = false;
                    _exportMail.OriginalPanel.isVisible = false;
                    _exportFish.OriginalPanel.isVisible = false;

                    // initialized
                    _updatePanelInitialized = true;
                }

                // info mode can change, make sure info mode is still outside connections
                if (!Singleton<InfoManager>.exists)
                {
                    return;
                }
                if (Singleton<InfoManager>.instance.CurrentMode != InfoManager.InfoMode.Connections)
                {
                    return;
                }

                // logic copied from OutsideConnectionsInfoViewPanel.UpdatePanel and then deselected resources are set to zero
                if (!Singleton<DistrictManager>.exists)
                {
                    return;
                }
                DistrictManager instance = Singleton<DistrictManager>.instance;

                // get import values
                DistrictResourceData importData = instance.m_districts.m_buffer[0].m_importData;
                int importGoods    = (int)(importData.m_averageGoods        + 99) / 100;
                int importForestry = (int)(importData.m_averageForestry     + 99) / 100;
                int importFarming  = (int)(importData.m_averageAgricultural + 99) / 100;
                int importOre      = (int)(importData.m_averageOre          + 99) / 100;
                int importOil      = (int)(importData.m_averageOil          + 99) / 100;
                int importMail     = (int)(importData.m_averageMail         + 99) / 100;

                // if check box is unchecked, then set value to zero
                if (!IsCheckBoxChecked(_importGoods   )) importGoods    = 0;
                if (!IsCheckBoxChecked(_importForestry)) importForestry = 0;
                if (!IsCheckBoxChecked(_importFarming )) importFarming  = 0;
                if (!IsCheckBoxChecked(_importOre     )) importOre      = 0;
                if (!IsCheckBoxChecked(_importOil     )) importOil      = 0;
                if (!IsCheckBoxChecked(_importMail    )) importMail     = 0;

                // display import values
                _importGoods.Count.text    = StringUtils.SafeFormat(_numberFormat, importGoods);
                _importForestry.Count.text = StringUtils.SafeFormat(_numberFormat, importForestry);
                _importFarming.Count.text  = StringUtils.SafeFormat(_numberFormat, importFarming);
                _importOre.Count.text      = StringUtils.SafeFormat(_numberFormat, importOre);
                _importOil.Count.text      = StringUtils.SafeFormat(_numberFormat, importOil);
                _importMail.Count.text     = StringUtils.SafeFormat(_numberFormat, importMail);

                // compute and display import total
                int importTotal = importGoods + importForestry + importFarming + importOre + importOil + importMail;
                _importTotal.Total.text = StringUtils.SafeFormat(_numberFormat, importTotal);

                // update import chart
                _importTotal.Chart.SetValues(
                    GetValue(importGoods,    importTotal),
                    GetValue(importForestry, importTotal),
                    GetValue(importFarming,  importTotal),
                    GetValue(importOre,      importTotal),
                    GetValue(importOil,      importTotal),
                    GetValue(importMail,     importTotal));
                _importTotal.Chart.Update();

                // get export values
                DistrictResourceData exportData = instance.m_districts.m_buffer[0].m_exportData;
                int exportGoods    = (int)(exportData.m_averageGoods        + 99) / 100;
                int exportForestry = (int)(exportData.m_averageForestry     + 99) / 100;
                int exportFarming  = (int)(exportData.m_averageAgricultural + 99) / 100;
                int exportOre      = (int)(exportData.m_averageOre          + 99) / 100;
                int exportOil      = (int)(exportData.m_averageOil          + 99) / 100;
                int exportMail     = (int)(exportData.m_averageMail         + 99) / 100;
                int exportFish     = (int)(exportData.m_averageFish         + 99) / 100;

                // if check box is unchecked, then set value to zero
                if (!IsCheckBoxChecked(_exportGoods   )) exportGoods    = 0;
                if (!IsCheckBoxChecked(_exportForestry)) exportForestry = 0;
                if (!IsCheckBoxChecked(_exportFarming )) exportFarming  = 0;
                if (!IsCheckBoxChecked(_exportOre     )) exportOre      = 0;
                if (!IsCheckBoxChecked(_exportOil     )) exportOil      = 0;
                if (!IsCheckBoxChecked(_exportMail    )) exportMail     = 0;
                if (!IsCheckBoxChecked(_exportFish    )) exportFish     = 0;

                // display export values
                _exportGoods.Count.text    = StringUtils.SafeFormat(_numberFormat, exportGoods);
                _exportForestry.Count.text = StringUtils.SafeFormat(_numberFormat, exportForestry);
                _exportFarming.Count.text  = StringUtils.SafeFormat(_numberFormat, exportFarming);
                _exportOre.Count.text      = StringUtils.SafeFormat(_numberFormat, exportOre);
                _exportOil.Count.text      = StringUtils.SafeFormat(_numberFormat, exportOil);
                _exportMail.Count.text     = StringUtils.SafeFormat(_numberFormat, exportMail);
                _exportFish.Count.text     = StringUtils.SafeFormat(_numberFormat, exportFish);

                // compute and display export total
                int exportTotal = exportGoods + exportForestry + exportFarming + exportOre + exportOil + exportMail + exportFish;
                _exportTotal.Total.text = StringUtils.SafeFormat(_numberFormat, exportTotal);

                // update export chart
                _exportTotal.Chart.SetValues(
                    GetValue(exportGoods,    exportTotal),
                    GetValue(exportForestry, exportTotal),
                    GetValue(exportFarming,  exportTotal),
                    GetValue(exportOre,      exportTotal),
                    GetValue(exportOil,      exportTotal),
                    GetValue(exportMail,     exportTotal),
                    GetValue(exportFish,     exportTotal));
                _exportTotal.Chart.Update();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// return the ratio for the given value and total
        /// radial chart uses ratio, not percent
        /// </summary>
        private static float GetValue(int value, int total)
        {
            if (total == 0) return 0f;
            return (float)value / (float)total;
        }

        /// <summary>
        /// return the color of the building
        /// </summary>
        /// <returns>whether or not to do base processing</returns>
        public static bool GetBuildingColor(ushort buildingID, ref Building data, ref Color color)
        {
            try
            {
                // get the building AI type
                Type buildingAIType = data.Info.m_buildingAI.GetType();
                string buildingAITypeString = buildingAIType.ToString();

                // the logic for each building AI below was derived from the GetColor method of that AI, unless specified otherwise

                if (buildingAIType == typeof(CommercialBuildingAI) ||
                    buildingAITypeString == "PloppableRICO.GrowableCommercialAI" ||     // derives from CommercialBuildingAI
                    buildingAITypeString == "PloppableRICO.PloppableCommercialAI")      // derives from PloppableRICO.GrowableCommercialAI
                {
                    // import only, not sure if the incoming resource will ever be other than Goods
                    if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Import)
                        return GetColorFromTransferReason(((CommercialBuildingAI)data.Info.m_buildingAI).m_incomingResource, ref color);
                    else
                        return true;
                }

                if (buildingAIType == typeof(IndustrialExtractorAI) ||
                    buildingAIType == typeof(LivestockExtractorAI) ||                   // derives from IndustrialExtractorAI
                    buildingAITypeString == "PloppableRICO.GrowableExtractorAI" ||      // derives from IndustrialExtractorAI
                    buildingAITypeString == "PloppableRICO.PloppableExtractorAI")       // derives from PloppableRICO.GrowableExtractorAI
                {
                    // export only
                    if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Import)
                        return true;
                    else
                    {
                        // logic is from IndustrialExtractorAI.GetOutgoingTransferReason
                        // convert item subservice to a transfer reason
                        TransferManager.TransferReason transferReason;
                        switch (data.Info.m_class.m_subService)
                        {
                            case ItemClass.SubService.IndustrialForestry: transferReason = TransferManager.TransferReason.Logs;  break;
                            case ItemClass.SubService.IndustrialFarming:  transferReason = TransferManager.TransferReason.Grain; break;
                            case ItemClass.SubService.IndustrialOre:      transferReason = TransferManager.TransferReason.Ore;   break;
                            case ItemClass.SubService.IndustrialOil:      transferReason = TransferManager.TransferReason.Oil;   break;
                            default: transferReason = TransferManager.TransferReason.None; break;
                        }

                        // get the color based on transfer reason
                        return GetColorFromTransferReason(transferReason, ref color);
                    }
                }

                if (buildingAIType == typeof(IndustrialBuildingAI) ||
                    buildingAITypeString == "PloppableRICO.GrowableIndustrialAI" ||     // derives from IndustrialBuildingAI
                    buildingAITypeString == "PloppableRICO.PloppableIndustrialAI")      // derives from PloppableRICO.GrowableIndustrialAI
                {
                    // both import and export
                    // convert item subservice to a transfer reason
                    TransferManager.TransferReason transferReason;
                    if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Import)
                    {
                        // logic is from IndustrialBuildingAI.GetIncomingTransferReason
                        switch (data.Info.m_class.m_subService)
                        {
                            case ItemClass.SubService.IndustrialForestry: transferReason = TransferManager.TransferReason.Logs;  break;
                            case ItemClass.SubService.IndustrialFarming:  transferReason = TransferManager.TransferReason.Grain; break;
                            case ItemClass.SubService.IndustrialOre:      transferReason = TransferManager.TransferReason.Ore;   break;
                            case ItemClass.SubService.IndustrialOil:      transferReason = TransferManager.TransferReason.Oil;   break;
                            default:
                                // if the subservice is not one of the above, then the resource type is assigned randomly based on the buildingID
                                switch (new Randomizer(buildingID).Int32(4u))
                                {
                                    case 0:  transferReason = TransferManager.TransferReason.Lumber; break;
                                    case 1:  transferReason = TransferManager.TransferReason.Food;   break;
                                    case 2:  transferReason = TransferManager.TransferReason.Petrol; break;
                                    case 3:  transferReason = TransferManager.TransferReason.Coal;   break;
                                    default: transferReason = TransferManager.TransferReason.None;   break;
                                }
                                break;
                        }
                    }
                    else
                    {
                        // logic is from IndustrialBuildingAI.GetOutgoingTransferReason
                        switch (data.Info.m_class.m_subService)
                        {
                            case ItemClass.SubService.IndustrialForestry: transferReason = TransferManager.TransferReason.Lumber; break;
                            case ItemClass.SubService.IndustrialFarming:  transferReason = TransferManager.TransferReason.Food;   break;
                            case ItemClass.SubService.IndustrialOre:      transferReason = TransferManager.TransferReason.Coal;   break;
                            case ItemClass.SubService.IndustrialOil:      transferReason = TransferManager.TransferReason.Petrol; break;
                            default:                                      transferReason = TransferManager.TransferReason.Goods;  break;
                        }
                    }

                    // get the color based on transfer reason
                    return GetColorFromTransferReason(transferReason, ref color);
                }

                if (buildingAIType == typeof(OfficeBuildingAI) ||
                    buildingAITypeString == "PloppableRICO.GrowableOfficeAI" ||     // derives from OfficeBuildingAI
                    buildingAITypeString == "PloppableRICO.PloppableOfficeAI")      // derives from PloppableRICO.GrowableOfficeAI
                {
                    // export only
                    if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Import)
                        return true;
                    else
                    {
                        // logic is from OfficeBuildingAI.GetOutgoingTransferReason
                        if (data.Info.m_class.m_subService == ItemClass.SubService.OfficeHightech)
                            return GetColorFromTransferReason(TransferManager.TransferReason.Goods, ref color);
                        else
                            return GetColorFromTransferReason(TransferManager.TransferReason.None, ref color);
                    }
                }


                if (buildingAIType == typeof(ExtractingFacilityAI))
                {
                    // export only
                    if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Import)
                        return true;
                    else
                        return GetColorFromTransferReason(((ExtractingFacilityAI)data.Info.m_buildingAI).m_outputResource, ref color);
                }

                if (buildingAIType == typeof(FishFarmAI))
                {
                    // export only, not sure if the output resource will ever be other than Fish
                    if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Import)
                        return true;
                    else
                        return GetColorFromTransferReason(((FishFarmAI)data.Info.m_buildingAI).m_outputResource, ref color);
                }

                if (buildingAIType == typeof(FishingHarborAI))
                {
                    // export only, not sure if output resource will ever be other than Fish
                    if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Import)
                        return true;
                    else
                        return GetColorFromTransferReason(((FishingHarborAI)data.Info.m_buildingAI).m_outputResource, ref color);
                }

                if (buildingAIType == typeof(HeatingPlantAI))
                {
                    // import only, not sure if the incoming resource will ever be other than Oil
                    if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Import)
                        return GetColorFromTransferReason(((HeatingPlantAI)data.Info.m_buildingAI).m_resourceType, ref color);
                    else
                        return true;
                }

                if (buildingAIType == typeof(MarketAI))
                {
                    // import only, not sure if the incoming resource will ever be other than Fish
                    // even though MarketAI has a GetColor routine with import logic for outside connections,
                    // it seems thru experimentation that fish will never be imported to the Market
                    if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Import)
                        return GetColorFromTransferReason(((MarketAI)data.Info.m_buildingAI).m_incomingResource, ref color);
                    else
                        return true;
                }

                if (buildingAIType == typeof(PostOfficeAI))
                {
                    // both import and export
                    // note that the resource colors for SortedMail and UnsortedMail are the same as for Mail
                    if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Import)
                        return GetColorFromTransferReason(TransferManager.TransferReason.SortedMail, ref color);
                    else
                        return GetColorFromTransferReason(TransferManager.TransferReason.UnsortedMail, ref color);
                }

                if (buildingAIType == typeof(PowerPlantAI      ) ||
                    buildingAIType == typeof(DamPowerHouseAI   ) ||    // derives from PowerPlantAI
                    buildingAIType == typeof(FusionPowerPlantAI) ||    // derives from PowerPlantAI
                    buildingAIType == typeof(SolarPowerPlantAI ) ||    // derives from PowerPlantAI
                    buildingAIType == typeof(WindTurbineAI     ))      // derives from PowerPlantAI
                {
                    // import only
                    // some buildings with PowerPlantAI have a real resource type (e.g. Coal or Oil) and some have None
                    // building AIs that derive from PowerPlantAI do not have their own GetColor routine and have a resource type of None
                    if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Import)
                        return GetColorFromTransferReason(((PowerPlantAI)data.Info.m_buildingAI).m_resourceType, ref color);
                    else
                        return true;
                }

                if (buildingAIType == typeof(ProcessingFacilityAI) ||
                    buildingAIType == typeof(UniqueFactoryAI     ))     // derives from ProcessingFacilityAI
                {
                    // both import and export
                    TransferManager.TransferReason transferReason;
                    if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Import)
                    {
                        // check input resources
                        ProcessingFacilityAI buildingAI = data.Info.m_buildingAI as ProcessingFacilityAI;
                        if      (buildingAI.m_inputResource1 != TransferManager.TransferReason.None && ((uint)(data.m_tempImport | data.m_finalImport) & 1u) != 0)
                            transferReason = buildingAI.m_inputResource1;
                        else if (buildingAI.m_inputResource2 != TransferManager.TransferReason.None && ((uint)(data.m_tempImport | data.m_finalImport) & 2u) != 0)
                            transferReason = buildingAI.m_inputResource2;
                        else if (buildingAI.m_inputResource3 != TransferManager.TransferReason.None && ((uint)(data.m_tempImport | data.m_finalImport) & 4u) != 0)
                            transferReason = buildingAI.m_inputResource3;
                        else if (buildingAI.m_inputResource4 != TransferManager.TransferReason.None && ((uint)(data.m_tempImport | data.m_finalImport) & 8u) != 0)
                            transferReason = buildingAI.m_inputResource4;
                        else
                            transferReason = TransferManager.TransferReason.None;
                    }
                    else
                    {
                        // always use output resource
                        transferReason = ((ProcessingFacilityAI)data.Info.m_buildingAI).m_outputResource;
                    }

                    // get the color based on transfer reason
                    return GetColorFromTransferReason(transferReason, ref color);
                }

                if (buildingAIType == typeof(ShelterAI))
                {
                    // import only, always Goods
                    if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Import)
                        return GetColorFromTransferReason(TransferManager.TransferReason.Goods, ref color);
                    else
                        return true;
                }

                if (buildingAIType == typeof(WarehouseAI))
                {
                    // both import and export
                    // import and export both use the same actual resource
                    return GetColorFromTransferReason(((WarehouseAI)data.Info.m_buildingAI).GetActualTransferReason(buildingID, ref data), ref color);
                }

                // if get here then a building AI patch was created without adding logic above for the AI
                Debug.LogError($"Unhandled building AI type [{buildingAIType}] for building ID [{buildingID}] while getting building color.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            // do base processing
            return true;
        }

        /// <summary>
        /// return the color of the vehicle
        /// </summary>
        /// <returns>whether or not to do base processing</returns>
        public static bool GetVehicleColor(ushort vehicleID, ref Vehicle data, ref Color color)
        {
            try
            {
                // get the vehicle AI type
                Type vehicleAIType = data.Info.m_vehicleAI.GetType();

                // the logic for each vehicle AI below was derived from the GetColor method of that AI, unless specified otherwise

                if (vehicleAIType == typeof(CargoPlaneAI) ||
                    vehicleAIType == typeof(CargoShipAI ) ||
                    vehicleAIType == typeof(CargoTrainAI) ||
                    vehicleAIType == typeof(CargoTruckAI) ||
                    vehicleAIType == typeof(PostVanAI   ))
                {
                    // both import and export, all use the same transfer reason
                    // some vehicles can be marked as both importing and exporting at the same time
                    // for plane, ship, and train, not sure if the resource will ever be other than None, but get it just in case
                    return GetColorFromTransferReason((TransferManager.TransferReason)data.m_transferType, ref color);
                }

                // if get here then vehicle AI patch was created without adding logic above for the AI
                Debug.LogError($"Unhandled vehicle AI type [{vehicleAIType}] for vehicle ID [{vehicleID}] while getting vehicle color.");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            // do base processing
            return true;
        }

        /// <summary>
        /// return the color of the building or vehicle based on transfer reason
        /// </summary>
        /// <returns>whether or not to do base processing</returns>
        private static bool GetColorFromTransferReason(TransferManager.TransferReason transferReason, ref Color color)
        {
            // check for no transfer reason
            if (transferReason == TransferManager.TransferReason.None)
            {
                // do base processing
                return true;
            }

            // convert the transfer reason to a resource type
            // some cases have more than one transfer reason that converts to a single resource type
            ResourceType resourceType;
            switch (transferReason)
            {
                case TransferManager.TransferReason.Goods:
                case TransferManager.TransferReason.LuxuryProducts:
                    resourceType = ResourceType.Goods;
                    break;

                case TransferManager.TransferReason.Logs:
                case TransferManager.TransferReason.Lumber:
                case TransferManager.TransferReason.Paper:
                case TransferManager.TransferReason.PlanedTimber:
                    resourceType = ResourceType.Forestry;
                    break;

                case TransferManager.TransferReason.Grain:
                case TransferManager.TransferReason.Food:
                case TransferManager.TransferReason.AnimalProducts:
                case TransferManager.TransferReason.Flours:
                    resourceType = ResourceType.Farming;
                    break;

                case TransferManager.TransferReason.Ore:
                case TransferManager.TransferReason.Coal:
                case TransferManager.TransferReason.Glass:
                case TransferManager.TransferReason.Metals:
                    resourceType = ResourceType.Ore;
                    break;

                case TransferManager.TransferReason.Oil:
                case TransferManager.TransferReason.Petrol:
                case TransferManager.TransferReason.Petroleum:
                case TransferManager.TransferReason.Plastics:
                    resourceType = ResourceType.Oil;
                    break;

                case TransferManager.TransferReason.IncomingMail:
                case TransferManager.TransferReason.OutgoingMail:
                case TransferManager.TransferReason.UnsortedMail:
                case TransferManager.TransferReason.SortedMail:
                case TransferManager.TransferReason.Mail:
                    resourceType = ResourceType.Mail;
                    break;

                case TransferManager.TransferReason.Fish:
                    resourceType = ResourceType.Fish;
                    break;

                default:
                    // transfer reason is unexpected, just use None
                    resourceType = ResourceType.None;
                    break;
            }

            // get the color based on resource type
            return GetColorFromResourceType(resourceType, ref color);
        }

        /// <summary>
        /// return the color of the building or vehicle based on resource type
        /// </summary>
        /// <returns>whether or not to do base processing</returns>
        private static bool GetColorFromResourceType(ResourceType type, ref Color color)
        {
            // check for no resource type
            if (type == ResourceType.None)
            {
                // do base processing
                return true;
            }

            // find the resource for the currently displayed import/export direction and for the specified resource type
            ConnectionDirection direction = (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Import ? ConnectionDirection.Import : ConnectionDirection.Export);
            foreach (UIResource resource in _resources)
            {
                if (resource.Direction == direction && resource.Type == type)
                {
                    // found the resource, check status of check box
                    if (IsCheckBoxChecked(resource))
                    {
                        // resource is turned on: allow base processing to set the color, which could be neutral
                        return true;
                    }
                    else
                    {
                        // resource is turned off: set the color to neutral and skip base processing
                        color = Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                        return false;
                    }
                }
            }

            // resource not found, not an error, just do base processing
            return true;
        }

        /// <summary>
        /// deinitialize user interface
        /// </summary>
        public static void Deinitialize()
        {
            try
            {
                // must destroy objects explicitly because loading a saved game from the Pause Menu
                // does not destroy the objects implicitly like returning to the Main Menu to load a saved game

                // destroy resources
                if (_resources != null)
                {
                    _resources.Clear();
                    _resources = null;
                }
                DestroyResource(_importGoods);
                DestroyResource(_importForestry);
                DestroyResource(_importFarming);
                DestroyResource(_importOre);
                DestroyResource(_importOil);
                DestroyResource(_importMail);

                DestroyResource(_exportGoods);
                DestroyResource(_exportForestry);
                DestroyResource(_exportFarming);
                DestroyResource(_exportOre);
                DestroyResource(_exportOil);
                DestroyResource(_exportMail);
                DestroyResource(_exportFish);

                // destroy totals
                DestroyTotal(_importTotal);
                DestroyTotal(_exportTotal);

                // no longer initialized
                _updatePanelInitialized = false;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// destroy a resource
        /// </summary>
        private static void DestroyResource(UIResource resource)
        {
            if (resource != null)
            {
                // destroy components
                DestroyUIComponent(ref resource.CheckBox);
                DestroyUIComponent(ref resource.ColorSprite);
                DestroyUIComponent(ref resource.Description);
                DestroyUIComponent(ref resource.Count);

                // destroy resource panel
                if (resource.Panel != null)
                {
                    resource.Panel.eventClicked -= ResourcePanel_eventClicked;
                    DestroyUIComponent(ref resource.Panel);
                }
            }
        }

        /// <summary>
        /// destroy a total
        /// </summary>
        private static void DestroyTotal(UITotal total)
        {
            if (total != null)
            {
                // destroy components
                DestroyUIComponent(ref total.Chart);
                DestroyUIComponent(ref total.Line);
                DestroyUIComponent(ref total.Text);
                DestroyUIComponent(ref total.Total);
            }
        }

        /// <summary>
        /// destroy a UI component
        /// </summary>
        private static void DestroyUIComponent<T>(ref T component) where T : UIComponent
        {
            if (component != null)
            {
                UnityEngine.Object.Destroy(component);
                component = null;
            }
        }

    }
}
