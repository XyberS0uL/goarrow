/* Copyright (c) 2007 Ben Howell
 * This software is licensed under the MIT License
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using WindowsTimer = System.Windows.Forms.Timer;
using MouseButtons = System.Windows.Forms.MouseButtons;

using Decal.Adapter;
using Decal.Adapter.Wrappers;
using Decal.Interop.Input;

using GoArrow.Huds;
using GoArrow.RouteFinding;

namespace GoArrow
{
	public delegate bool DefaultViewActiveDelegate();

	[View("GoArrow.Properties.GoArrow.xml")]
	[WireUpControlEvents]
	public partial class PluginCore : PluginBase
	{
		#region Private Fields
		private const int GoIcon = 0x06001F80;
		private const int DeleteIcon = 0x0600606E;

		private const int RelativeCoordsFast = 1000;
		private const int RelativeCoordsSlow = 10000;

		private const int MaxRecentLocations = 15;

		private const string HideDetailsText = "Minimize";
		private const string ShowDetailsText = "Restore";

		private const string CrossroadsOfDerethUrl = "http://ac.warcry.com/atlas/atlas_xml.php";
		private const string ACSpediaUrl = "http://www.acspedia.com/places/places.aspx";

		private const int MinHudOpacity = 64;

		private WindowsTimer mRelativeCoordsTimer = new WindowsTimer();
		private Location mDetailsLoc = null;

		private Route mRoute;
		private int mRouteCopyIndex;
		private Location mRouteStartLoc, mRouteEndLoc;
		private RouteFinderPackage mRoutePackage;

		private WebClient mDownloadClient = new WebClient();
		private int mDownloadSizeEstimate = 3000 * 1024; // 3,000 KB
		private BackgroundWorker mXmlConverterWorker = new BackgroundWorker();

		private bool mDefaultViewActive;
		#endregion  Private Fields

		#region Control references
#pragma warning disable 649
		[ControlReference("nbkMain")]
		private NotebookWrapper nbkMain;
		Dictionary<int, Size> nbkMainTabsSize = new Dictionary<int, Size>();
		private struct MainTab
		{
			public const int HUDs = 0, Atlas = 1, Settings = 2;
			public const int COUNT = 3;
		}

		//
		#region HUDs Tab
		//
		[ControlReference("nbkHuds")]
		private NotebookWrapper nbkHuds;
		private struct HudsTab
		{
			public const int Arrow = 0, Dereth = 1, Dungeon = 2, General = 3;
			public const int COUNT = 4;
		}

		private struct MouseList
		{
			public const int Name = 0, Left = 1, Middle = 2, Right = 3, X1 = 4, X2 = 5;
		}

		private struct MouseControls
		{
			public const string PanMap = "Pan (Drag) Map";
			public const string SelectLocation = "Select Location";
			public const string ContextMenu = "Context Menu";
			public const string Details = "Show Details";
		}

		//
		// HUDs > Arrow HUD Tab
		//
		[ControlReference("edtArrowCoords")]
		private TextBoxWrapper edtArrowCoords;

		[ControlReference("chkShowArrow")]
		private CheckBoxWrapper chkShowArrow;

		[ControlReference("chkArrowIndoors")]
		private CheckBoxWrapper chkArrowIndoors;

		[ControlReference("chkShowDestination")]
		private CheckBoxWrapper chkShowDestination;

		[ControlReference("chkDistUnderArrow")]
		private CheckBoxWrapper chkDistUnderArrow;

		[ControlReference("chkLockArrowPosition")]
		private CheckBoxWrapper chkLockArrowPosition;

		[ControlReference("chkTrackCorpses")]
		private CheckBoxWrapper chkTrackCorpses;

		[ControlReference("chkLinkCoords")]
		private CheckBoxWrapper chkLinkCoords;

		[ControlReference("choArrowImage")]
		private ChoiceWrapper choArrowImage;

		[ControlReference("edtTextSize")]
		private TextBoxWrapper edtTextSize;

		[ControlReference("choTextColor")]
		private ChoiceWrapper choTextColor;

		[ControlReference("chkBold")]
		private CheckBoxWrapper chkBold;

		//
		// HUDs > Dereth Map Tab
		//
		[ControlReference("chkDerethMapShow")]
		private CheckBoxWrapper chkDerethMapShow;

		[ControlReference("chkDerethMapCenterPlayer")]
		private CheckBoxWrapper chkDerethMapCenterPlayer;

		[ControlReference("chkDerethMapShowLocs")]
		private CheckBoxWrapper chkDerethMapShowLocs;

		[ControlReference("chkDerethMapShowAllLocs")]
		private CheckBoxWrapper chkDerethMapShowAllLocs;

		[ControlReference("chkDerethMapShowLabels")]
		private CheckBoxWrapper chkDerethMapShowLabels;

		[ControlReference("edtDerethMapCenterOn")]
		private TextBoxWrapper edtDerethMapCenterOn;

		[ControlReference("lstDerethMapMouse")]
		private ListWrapper lstDerethMapMouse;

		//
		// HUDs > Dungeon Map Tab
		//
		[ControlReference("chkDungeonMapShow")]
		private CheckBoxWrapper chkDungeonMapShow;

		[ControlReference("choDungeonMap")]
		private ChoiceWrapper choDungeonMap;

		[ControlReference("chkDungeonMapAutoLoad")]
		private CheckBoxWrapper chkDungeonMapAutoLoad;

		[ControlReference("chkDungeonMapAutoRotate")]
		private CheckBoxWrapper chkDungeonMapAutoRotate;

		[ControlReference("chkDungeonMapCompass")]
		private CheckBoxWrapper chkDungeonMapCompass;

		[ControlReference("chkDungeonMapMoveWithPlayer")]
		private CheckBoxWrapper chkDungeonMapMoveWithPlayer;

		[ControlReference("lstDungeonMapMouse")]
		private ListWrapper lstDungeonMapMouse;

		//
		// HUDs > General Tab
		//
		[ControlReference("chkHudsToolbarShow")]
		private CheckBoxWrapper chkHudsToolbarShow;

		[ControlReference("chkHudToolbarIcons")]
		private CheckBoxWrapper chkHudToolbarIcons;

		[ControlReference("chkHudToolbarText")]
		private CheckBoxWrapper chkHudToolbarText;

		[ControlReference("chkHudToolbarHoriz")]
		private CheckBoxWrapper chkHudToolbarHoriz;

		[ControlReference("chkHudToolbarVert")]
		private CheckBoxWrapper chkHudToolbarVert;

		[ControlReference("lstToolbarButtons")]
		private ListWrapper lstToolbarButtons;
		private struct ToolbarButtonsList
		{
			public const int Visible = 0, Name = 1;
		}

		[ControlReference("chkAllowBothMaps")]
		private CheckBoxWrapper chkAllowBothMaps;

		[ControlReference("sldHudOpacityActive")]
		private SliderWrapper sldHudOpacityActive;

		[ControlReference("sldHudOpacityInactive")]
		private SliderWrapper sldHudOpacityInactive;
		#endregion HUDs Tab

		//
		#region Atlas Tab
		//
		private struct LocationList
		{
			public const int Icon = 0, Name = 1, Coords = 2, GoIcon = 3, ID = 4;
		}

		[ControlReference("nbkAtlas")]
		private NotebookWrapper nbkAtlas;
		private struct AtlasTab
		{
			public const int Search = 0, Route = 1, Details = 2,
				Favorites = 3, Recent = 4, Update = 5;
			public const int COUNT = 6;
		}

		//
		// Atlas > Search Tab
		//
		[ControlReference("chkSearchName")]
		private CheckBoxWrapper chkSearchName;

		[ControlReference("edtSearchName")]
		private TextBoxWrapper edtSearchName;

		[ControlReference("choSearchIn")]
		private ChoiceWrapper choSearchIn;
		private SearchField[] searchIn = { SearchField.Name, SearchField.Description, SearchField.Both };

		[ControlReference("chkSearchNearby")]
		private CheckBoxWrapper chkSearchNearby;

		[ControlReference("edtSearchRadius")]
		private TextBoxWrapper edtSearchRadius;

		[ControlReference("edtSearchCoords")]
		private TextBoxWrapper edtSearchCoords;

		[ControlReference("choSearchLimit")]
		private ChoiceWrapper choSearchLimit;

		[ControlReference("lstSearchResults")]
		private ListWrapper lstSearchResults;

		[ControlReference("chkSearchShowRelative")]
		private CheckBoxWrapper chkSearchShowRelative;

		[ControlReference("lblSearchMatchesFound")]
		private StaticWrapper lblSearchMatchesFound;

		//
		// Atlas > Details Tab
		//
		[ControlReference("icoDetailsIcon")]
		private ButtonWrapper icoDetailsIcon;

		[ControlReference("lblDetailsName")]
		private StaticWrapper lblDetailsName;

		[ControlReference("lblDetailsType")]
		private StaticWrapper lblDetailsType;

		[ControlReference("lblDetailsAbsCoords")]
		private StaticWrapper lblDetailsAbsCoords;

		[ControlReference("lblDetailsRelCoords")]
		private StaticWrapper lblDetailsRelCoords;

		[ControlReference("btnDetailsShrink")]
		private PushButtonWrapper btnDetailsShrink;

		[ControlReference("btnDetailsDungeonMap")]
		private PushButtonWrapper btnDetailsDungeonMap;

		//
		// Atlas > Details > Description Tab
		//
		[ControlReference("lstLocationDescription")]
		private ListWrapper lstLocationDescription;

		//
		// Atlas > Details > Modify/Add Location Tab
		//
		[ControlReference("edtModifyName")]
		private TextBoxWrapper edtModifyName;

		[ControlReference("edtModifyCoords")]
		private TextBoxWrapper edtModifyCoords;

		[ControlReference("edtModifyExitCoords")]
		private TextBoxWrapper edtModifyExitCoords;

		[ControlReference("choModifyType")]
		private ChoiceWrapper choModifyType;

		[ControlReference("chkModifyUseInRoute")]
		private CheckBoxWrapper chkModifyUseInRoute;

		[ControlReference("edtModifyDescription")]
		private TextBoxWrapper edtModifyDescription;

		//
		// Atlas > Route Tab
		//
		[ControlReference("choRouteFromLtr")]
		private ChoiceWrapper choRouteFromLtr;

		[ControlReference("choRouteFrom")]
		private ChoiceWrapper choRouteFrom;

		[ControlReference("edtRouteFromCoords")]
		private TextBoxWrapper edtRouteFromCoords;

		[ControlReference("chkRouteFromHere")]
		private CheckBoxWrapper chkRouteFromHere;

		[ControlReference("choRouteToLtr")]
		private ChoiceWrapper choRouteToLtr;

		[ControlReference("choRouteTo")]
		private ChoiceWrapper choRouteTo;

		[ControlReference("edtRouteToCoords")]
		private TextBoxWrapper edtRouteToCoords;

		[ControlReference("chkRouteToHere")]
		private CheckBoxWrapper chkRouteToHere;

		[ControlReference("lstRoute")]
		private ListWrapper lstRoute;

		[ControlReference("chkRouteShowRelative")]
		private CheckBoxWrapper chkRouteShowRelative;

		[ControlReference("lblRouteDistance")]
		private StaticWrapper lblRouteDistance;

		//
		// Atlas > Favorites Tab
		//
		[ControlReference("lstFavorites")]
		private ListWrapper lstFavorites;

		[ControlReference("chkFavoritesShowRelative")]
		private CheckBoxWrapper chkFavoritesShowRelative;

		//
		// Atlas > Recent Tab
		//
		[ControlReference("lstRecent")]
		private ListWrapper lstRecent;

		[ControlReference("lstRecentCoords")]
		private ListWrapper lstRecentCoords;
		private struct RecentCoordsList
		{
			public const int Icon = 0, Name = 1, Coords = 2;
		}

		[ControlReference("chkRecentShowRelative")]
		private CheckBoxWrapper chkRecentShowRelative;

		//
		// Atlas > Update Tab
		//
		[ControlReference("edtLocationsUrl")]
		private TextBoxWrapper edtLocationsUrl;

		[ControlReference("choUpdateDatabaseType")]
		private ChoiceWrapper choUpdateDatabaseType;

		private struct UpdateDatabaseType
		{
			public const int CrossroadsOfDereth = 0, ACSpedia = 1;
		}

		[ControlReference("chkUpdateOverwrite")]
		private CheckBoxWrapper chkUpdateOverwrite;

		[ControlReference("chkUpdateRemind")]
		private CheckBoxWrapper chkUpdateRemind;

		[ControlReference("lblUpdateNumLocations")]
		private StaticWrapper lblUpdateNumLocations;

		[ControlReference("lblUpdateLastUpdate")]
		private StaticWrapper lblUpdateLastUpdate;

		[ControlReference("btnLocationsUpdate")]
		private PushButtonWrapper btnLocationsUpdate;

		[ControlReference("prgLocationsProgress")]
		private ProgressWrapper prgLocationsProgress;

		[ControlReference("txtDownloadStatusA")]
		private StaticWrapper txtDownloadStatusA;

		[ControlReference("txtDownloadStatusB")]
		private StaticWrapper txtDownloadStatusB;

		private string codLocationsXmlPath;
		private string acsLocationsPath;
		#endregion Atlas Tab

		//
		#region Settings Tab
		//
		[ControlReference("nbkSettings")]
		private NotebookWrapper nbkSettings;
		private struct SettingsTab
		{
			public const int Chat = 0, RouteFinding = 1, About = 2;
			public const int COUNT = 3;
		}

		//
		// Settings > Chat Tab
		//
		[ControlReferenceArray("chkOutputMainChat", "chkOutput1", "chkOutput2", "chkOutput3", "chkOutput4")]
		private CheckBoxWrapper[] chkOutputs;

		[ControlReference("chkAlwaysShowErrors")]
		private CheckBoxWrapper chkAlwaysShowErrors;

		[ControlReference("edtChatCommand")]
		private TextBoxWrapper edtChatCommand;

		[ControlReference("lblHelpInfo")]
		private StaticWrapper lblHelpInfo;

		[ControlReference("chkEnableCoordsCommand")]
		private CheckBoxWrapper chkEnableCoordsCommand;

		[ControlReference("edtCoordsCommand")]
		private TextBoxWrapper edtCoordsCommand;

		[ControlReference("chkEnableDestCommand")]
		private CheckBoxWrapper chkEnableDestCommand;

		[ControlReference("edtDestCommand")]
		private TextBoxWrapper edtDestCommand;

		[ControlReference("chkEnableFindCommand")]
		private CheckBoxWrapper chkEnableFindCommand;

		[ControlReference("edtFindCommand")]
		private TextBoxWrapper edtFindCommand;

		//
		// Settings > Route Finding Tab
		//
		[ControlReference("nbkRouteSettings")]
		private NotebookWrapper nbkRouteSettings;
		private struct RouteSettingsTab
		{
			public const int StartLocations = 0, PortalDevices = 1;
			public const int COUNT = 2;
		}

		[ControlReference("lstStartLocations")]
		private ListWrapper lstStartLocations;
		private struct StartLocationsList
		{
			public const int Enabled = 0, Icon = 1, Name = 2, Coords = 3, Delete = 4;
		}

		[ControlReference("edtStartLocationName")]
		private TextBoxWrapper edtStartLocationName;

		[ControlReference("edtStartLocationRunDist")]
		private TextBoxWrapper edtStartLocationRunDist;

		[ControlReference("edtStartLocationCoords")]
		private TextBoxWrapper edtStartLocationCoords;

		[ControlReference("btnStartLocationAdd")]
		private PushButtonWrapper btnStartLocationAdd;

		[ControlReference("chkAutoUpdateRecalls")]
		private CheckBoxWrapper chkAutoUpdateRecalls;

		[ControlReference("edtMaxRunDist")]
		private TextBoxWrapper edtMaxRunDist;

		[ControlReference("edtPortalRunDist")]
		private TextBoxWrapper edtPortalRunDist;

		[ControlReference("lstPortalDevices")]
		private ListWrapper lstPortalDevices;
		private struct PortalDevicesList
		{
			public const int Enabled = 0, Icon = 1, Name = 2, Detected = 3;
		}

		[ControlReference("chkAutoDetectPortalDevices")]
		private CheckBoxWrapper chkAutoDetectPortalDevices;

		//
		// Settings > About Tab
		//
		[ControlReference("txtAboutNameVer")]
		private StaticWrapper txtAboutNameVer;

		[ControlReference("lblHelpInfo2")]
		private StaticWrapper lblHelpInfo2;
		#endregion Settings Tab

#pragma warning restore 649
		#endregion

		#region Init and Destroy
		private void InitMainViewBeforeSettings()
		{
			mDefaultViewActive = false;

			mRouteCopyIndex = 0;
			mRouteStartLoc = null;
			mRouteEndLoc = null;
			mRoutePackage = null;

			txtAboutNameVer.Text = Util.PluginNameVer;

			choArrowImage.Clear();
			foreach (string arrowName in mArrowHud.AvailableArrowNames)
			{
				choArrowImage.Add(arrowName, arrowName);
			}

			FillLocationTypeChoice(choSearchLimit, true);
			FillLocationTypeChoice(choModifyType, false);

			LoadRouteChoice(choRouteFrom, choRouteFromLtr.Text[choRouteFromLtr.Selected][0]);
			LoadRouteChoice(choRouteTo, choRouteToLtr.Text[choRouteToLtr.Selected][0]);

			nbkMainTabsSize[MainTab.HUDs] = new Size(216, 280);
			nbkMainTabsSize[MainTab.Atlas] = new Size(272, 338);
			nbkMainTabsSize[MainTab.Settings] = new Size(272, 338);

			codLocationsXmlPath = Util.FullPath("cod_locations.xml");
			acsLocationsPath = Util.FullPath("acspedia_locations.txt");

			mDownloadClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadClient_DownloadProgressChanged);
			mDownloadClient.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadClient_DownloadFileCompleted);
			mXmlConverterWorker.DoWork += new DoWorkEventHandler(xmlConverterWorker_DoWork);
			mXmlConverterWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(xmlConverterWorker_RunWorkerCompleted);
			mXmlConverterWorker.WorkerReportsProgress = true;
			mXmlConverterWorker.WorkerSupportsCancellation = true;

			mArrowHud.HudEnabledChanged += new EventHandler(mArrowHud_HudEnabledChanged);
			mMapHud.VisibleChanged += new EventHandler(MapHud_VisibleChanged);
			mDungeonHud.VisibleChanged += new EventHandler(DungeonHud_VisibleChanged);
			mDungeonHud.DungeonListUpdated += new EventHandler(DungeonHud_DungeonListUpdated);
			mDungeonHud.MapChanged += new EventHandler(DungeonHud_MapChanged);
			mDungeonHud.MapBlacklisted += new EventHandler<MapBlacklistedEventArgs>(DungeonHud_MapBlacklisted);

			DungeonHud_DungeonListUpdated(null, null);

			mToolbar.VisibleChanged += new EventHandler(Toolbar_VisibleChanged);
			mToolbar.DisplayChanged += new EventHandler(Toolbar_DisplayChanged);

			lblUpdateLastUpdate.Text = mLocDb.LastUpdateString;
		}

		private void CheckStartLocation(string name, RouteStartType type, double runDist, int icon, SavesPer savesPer)
		{
			RouteStart rs;
			if ((rs = GetStartLocationByType(type)) != null)
			{
				rs.SavesPer = savesPer;
			}
			else
			{
				mStartLocations[name] = new RouteStart(name, type, icon, runDist,
					Coordinates.NO_COORDINATES, savesPer, true);
			}
		}

		private void AddStartLocation(string name, Coordinates coords, double runDist, bool enabled, int icon)
		{
			if (!mStartLocations.ContainsKey(name))
			{
				mStartLocations[name] = new RouteStart(name, RouteStartType.Regular, icon, runDist,
					coords, SavesPer.All, enabled);
			}
		}

		private void InitMainViewAfterSettings()
		{
			bool noStartLocations = (mStartLocations.Count == 0);
			if (noStartLocations)
			{
				// No start locations loaded from settings; add defaults
				AddStartLocation("Abandoned Mines", new Coordinates(34.9, 54.6), 1.0, true, Location.LocationTypeInfo(LocationType.PortalHub).Icon);
				AddStartLocation("Aerlinthe Recall", new Coordinates(84.1, 47.2), 0.4, true, AcIcons.AerlintheRecall);
				AddStartLocation("Aphus Lassel Recall", new Coordinates(2.3, 95.5), 0.4, true, AcIcons.AphusLasselRecall);
				AddStartLocation("Sancturary Recall", new Coordinates(-82.6, 93.4), 0.4, true, AcIcons.SancturaryRecall);
				AddStartLocation("Mount Lethe Recall", new Coordinates(-33.8, -85.3), 0.4, true, AcIcons.MountLetheRecall);
				AddStartLocation("Singularity Caul Recall", new Coordinates(-98.0, -94.7), 0.4, true, AcIcons.SingularityCaulRecall);
				AddStartLocation("Glenden Wood Recall", new Coordinates(29.7, 26.5), 0.4, false, AcIcons.GlendenWoodRecall);
				AddStartLocation("Ulgrim's Recall", Coordinates.NO_COORDINATES, 0.4, false, AcIcons.UlgrimRecall);
			}
			if (mLoadedSettingsVersion < 2 || noStartLocations)
			{
				AddStartLocation("Portal Gem: Al-Arqas", new Coordinates(-31.3, 13.2), 0.4, false, AcIcons.AlArqasGem);
				AddStartLocation("Portal Gem: Holtburg", new Coordinates(42.1, 33.6), 0.4, false, AcIcons.HoltburgGem);
				AddStartLocation("Portal Gem: Lytelthorpe", new Coordinates(1.1, 51.7), 0.4, false, AcIcons.LytelthorpeGem);
				AddStartLocation("Portal Gem: Nanto", new Coordinates(-52.2, 82.5), 0.4, false, AcIcons.NantoGem);
				AddStartLocation("Portal Gem: Rithwic", new Coordinates(10.8, 59.3), 0.4, false, AcIcons.RithwicGem);
				AddStartLocation("Portal Gem: Samsur", new Coordinates(-3.2, 19.0), 0.4, false, AcIcons.SamsurGem);
				AddStartLocation("Portal Gem: Shoushi", new Coordinates(-33.5, 72.8), 0.4, false, AcIcons.ShoushiGem);
				AddStartLocation("Portal Gem: Xarabydun", new Coordinates(-41.9, 16.1), 0.4, false, AcIcons.XarabydunGem);
				AddStartLocation("Portal Gem: Yanshi", new Coordinates(-12.6, 42.4), 0.4, false, AcIcons.YanshiGem);
				AddStartLocation("Portal Gem: Yaraq", new Coordinates(-21.5, -1.8), 0.4, false, AcIcons.YaraqGem);
				AddStartLocation("Portal Gem: Celdiseth", new Coordinates(86.6, 21.6), 0.4, false, AcIcons.CeldisethGem);
				AddStartLocation("Portal Gem: Fadsahil", new Coordinates(-40.7, 11.9), 0.4, false, AcIcons.FadsahilGem);
				AddStartLocation("Portal Gem: Shoyanen", new Coordinates(-63.4, 85.8), 0.4, false, AcIcons.ShoyanenGem);
			}
			// Ensure special types exist
			CheckStartLocation("Lifestone Bind", RouteStartType.LifestoneBind, 1.0, AcIcons.Lifestone, SavesPer.Character);
			CheckStartLocation("Lifestone Tie", RouteStartType.LifestoneTie, 0.4, AcIcons.LifestoneTie, SavesPer.Character);
			CheckStartLocation("Primary Portal Tie", RouteStartType.PrimaryPortalTie, 0.4, AcIcons.PrimaryPortalTie, SavesPer.Character);
			CheckStartLocation("Secondary Portal Tie", RouteStartType.SecondaryPortalTie, 0.4, AcIcons.SecondaryPortalTie, SavesPer.Character);
			CheckStartLocation("House Recall", RouteStartType.HouseRecall, 1.0, Location.LocationTypeInfo(LocationType.Village).Icon, SavesPer.Account);
			CheckStartLocation("Mansion Recall", RouteStartType.MansionRecall, 1.0, AcIcons.Mansion, SavesPer.Monarchy);
			CheckStartLocation("Allegiance Hometown", RouteStartType.AllegianceBindstone, 1.0, Location.LocationTypeInfo(LocationType.Bindstone).Icon, SavesPer.Monarchy);

			// Update start locations
			if (mLoadedSettingsVersion < 2)
			{
				RouteStart hometown = GetStartLocationByType(RouteStartType.AllegianceBindstone);
				if (hometown != null && hometown.Name == "Allegiance Bindstone")
				{
					mStartLocations.Remove(hometown.Name);
					hometown.Name = "Allegiance Hometown";
					mStartLocations[hometown.Name] = hometown;
				}
			}

			lstStartLocations.Clear();
			foreach (RouteStart loc in mStartLocations.Values)
			{
				ListRow row = lstStartLocations.Add();
				row[StartLocationsList.Enabled][0] = loc.Enabled;
				row[StartLocationsList.Icon][1] = loc.Icon;
				row[StartLocationsList.Name][0] = loc.Name;
				row[StartLocationsList.Coords][0] = loc.Coords.ToString(true);
				if (loc.Coords != Coordinates.NO_COORDINATES)
				{
					row[StartLocationsList.Name].Color = Color.White;
					row[StartLocationsList.Coords].Color = Color.White;
				}
				else
				{
					row[StartLocationsList.Name].Color = Color.Gray;
					row[StartLocationsList.Coords].Color = Color.Gray;
				}

				if (loc.Type == RouteStartType.Regular)
					row[StartLocationsList.Delete][1] = DeleteIcon;
			}

			lstPortalDevices.Clear();
			int deviceRowNum = 0;
			foreach (PortalDevice device in mPortalDevices.Values)
			{
				ListRow row = lstPortalDevices.Add();
				row[PortalDevicesList.Enabled][0] = device.Enabled;
				row[PortalDevicesList.Icon][1] = device.Icon;
				row[PortalDevicesList.Name][0] = device.Name;
				if (device.Detected)
				{
					row[PortalDevicesList.Detected].Color = Color.LightGreen;
					row[PortalDevicesList.Detected][0] = "Detected";
				}
				else
				{
					row[PortalDevicesList.Name].Color = Color.Gray;
					row[PortalDevicesList.Detected].Color = Color.Gray;
					row[PortalDevicesList.Detected][0] = "Not Detected";
				}
				device.Row = deviceRowNum++;
				device.CoordsChanged += new EventHandler(PortalDevice_CoordsChanged);
			}

			lstToolbarButtons.Clear();
			foreach (ToolbarButton button in mToolbar)
			{
				ListRow row = lstToolbarButtons.Add();
				row[ToolbarButtonsList.Visible][0] = button.Visible;
				row[ToolbarButtonsList.Name][0] = button.Label;
			}

			MainChatCommandUpdated(edtChatCommand.Text);

			SetViewSize(nbkMainTabsSize[nbkMain.ActiveTab]);

			mArrowHud.DestinationChanged += new EventHandler<DestinationChangedEventArgs>(mArrowHud_DestinationChanged);
			edtArrowCoords.Text = mArrowHud.DestinationCoords.ToString();

			mRelativeCoordsTimer.Tick += new EventHandler(RelativeCoordsTimer_Tick);
			mRelativeCoordsTimer.Interval = RelativeCoordsFast;
			mRelativeCoordsTimer.Start();

			mLocDb.FavoritesListChanged += new EventHandler<LocationChangedEventArgs>(LocationDatabase_FavoritesListChanged);
			UpdateFavoritesList();

			Uri tmp;
			if (!Uri.TryCreate(edtLocationsUrl.Text, UriKind.Absolute, out tmp))
			{
				if (mLocDb.DatabaseType == DatabaseType.CrossroadsOfDereth)
				{
					choUpdateDatabaseType.Selected = UpdateDatabaseType.CrossroadsOfDereth;
					edtLocationsUrl.Text = CrossroadsOfDerethUrl;
				}
				else
				{
					choUpdateDatabaseType.Selected = UpdateDatabaseType.ACSpedia;
					edtLocationsUrl.Text = ACSpediaUrl;
				}
			}

			mLocDb.LocationAdded += new EventHandler<LocationChangedEventArgs>(LocationDatabase_LocationCountChanged);
			mLocDb.LocationRemoved += new EventHandler<LocationChangedEventArgs>(LocationDatabase_LocationCountChanged);
			LocationDatabase_LocationCountChanged(null, null);

			chkShowArrow.Checked = mArrowHud.Visible;
			edtArrowCoords.Text = mArrowHud.DestinationCoords.ToString();
			chkArrowIndoors.Checked = mArrowHud.DisplayIndoors;
			chkShowDestination.Checked = mArrowHud.ShowDestinationOver;
			chkDistUnderArrow.Checked = mArrowHud.ShowDistanceUnder;
			chkLockArrowPosition.Checked = mArrowHud.PositionLocked;
			chkBold.Checked = mArrowHud.TextBold;
			edtTextSize.Text = mArrowHud.TextSize.ToString();

			chkDerethMapShow.Checked = mMapHud.Visible;
			chkDerethMapCenterPlayer.Checked = mMapHud.CenterOnPlayer;
			chkDerethMapShowLocs.Checked = mMapHud.ShowLocations;
			chkDerethMapShowAllLocs.Checked = mMapHud.ShowLocationsAllZooms;
			chkDerethMapShowLabels.Checked = mMapHud.ShowLabels;
			edtDerethMapCenterOn.Text = mMapHud.CoordsAtCenter.ToString("0.0");
			InitMouseList(lstDerethMapMouse, MouseControls.PanMap, mMapHud.DragButton);
			InitMouseList(lstDerethMapMouse, MouseControls.SelectLocation, mMapHud.SelectLocationButton);
			InitMouseList(lstDerethMapMouse, MouseControls.ContextMenu, mMapHud.ContextMenuButton);
			InitMouseList(lstDerethMapMouse, MouseControls.Details, mMapHud.DetailsButton);

			chkDungeonMapShow.Checked = mDungeonHud.Visible;
			chkDungeonMapAutoLoad.Checked = mDungeonHud.AutoLoadMaps;
			chkDungeonMapCompass.Checked = mDungeonHud.ShowCompass;
			chkDungeonMapAutoRotate.Checked = mDungeonHud.AutoRotateMap;
			chkDungeonMapMoveWithPlayer.Checked = mDungeonHud.MoveWithPlayer;
			InitMouseList(lstDungeonMapMouse, MouseControls.PanMap, mDungeonHud.DragButton);

			chkHudsToolbarShow.Checked = mToolbar.Visible;
			chkHudToolbarIcons.Checked = (mToolbar.Display & ToolbarDisplay.Icons) != 0;
			chkHudToolbarText.Checked = (mToolbar.Display & ToolbarDisplay.Text) != 0;
			chkHudToolbarHoriz.Checked = (mToolbar.Orientation == ToolbarOrientation.Horizontal);
			chkHudToolbarVert.Checked = (mToolbar.Orientation == ToolbarOrientation.Vertical);

			if (!chkAllowBothMaps.Checked && mMapHud.Visible)
			{
				mDungeonHud.Visible = false;
			}

			if (mMapHud.AlphaFrameInactive < MinHudOpacity)
				mMapHud.AlphaFrameInactive = MinHudOpacity;
			if (mMapHud.AlphaFrameActive < mMapHud.AlphaFrameInactive)
				mMapHud.AlphaFrameActive = mMapHud.AlphaFrameInactive;

			sldHudOpacityActive.SliderPostition = mArrowHud.Alpha =
				mDungeonHud.AlphaFrameActive = mMapHud.AlphaFrameActive;
			sldHudOpacityInactive.SliderPostition =
				mDungeonHud.AlphaFrameInactive = mMapHud.AlphaFrameInactive;

			chkOutputs[0].Checked = (Util.DefaultWindow & ChatWindow.MainChat) != 0;
			chkOutputs[1].Checked = (Util.DefaultWindow & ChatWindow.One) != 0;
			chkOutputs[2].Checked = (Util.DefaultWindow & ChatWindow.Two) != 0;
			chkOutputs[3].Checked = (Util.DefaultWindow & ChatWindow.Three) != 0;
			chkOutputs[4].Checked = (Util.DefaultWindow & ChatWindow.Four) != 0;

			DefaultView.Underlying.OnActivate += new Decal.Interop.Inject.IViewEvents_OnActivateEventHandler(MainView_OnActivate);
			DefaultView.Underlying.OnDeactivate += new Decal.Interop.Inject.IViewEvents_OnDeactivateEventHandler(MainView_OnDeactivate);

			// Do this after all other initialization
			if (lstRecent.RowCount > 0)
				ShowDetails(GetLocation(lstRecent[0][LocationList.ID][0] as string), false);
			else
				ShowDetails(null, false);
		}

		private void DisposeMainView()
		{
			DefaultView.Underlying.OnActivate -= MainView_OnActivate;
			DefaultView.Underlying.OnDeactivate -= MainView_OnDeactivate;

			mArrowHud.HudEnabledChanged -= mArrowHud_HudEnabledChanged;
			mArrowHud.DestinationChanged -= mArrowHud_DestinationChanged;
			mMapHud.VisibleChanged -= MapHud_VisibleChanged;
			mDungeonHud.VisibleChanged -= DungeonHud_VisibleChanged;
			mDungeonHud.DungeonListUpdated -= DungeonHud_DungeonListUpdated;
			mDungeonHud.MapChanged -= DungeonHud_MapChanged;
			mDungeonHud.MapBlacklisted -= DungeonHud_MapBlacklisted;

			mDownloadClient.DownloadProgressChanged -= downloadClient_DownloadProgressChanged;
			mDownloadClient.DownloadFileCompleted -= downloadClient_DownloadFileCompleted;
			mDownloadClient.Dispose();
			mDownloadClient = null;

			mXmlConverterWorker.DoWork -= xmlConverterWorker_DoWork;
			mXmlConverterWorker.RunWorkerCompleted -= xmlConverterWorker_RunWorkerCompleted;
			mXmlConverterWorker.Dispose();
			mXmlConverterWorker = null;

			mRelativeCoordsTimer.Dispose();
		}
		#endregion Init and Destroy

		#region Helper Functions
		private void MainView_OnActivate()
		{
			try
			{
				if (!mMainViewToolButton.Disposed)
				{
					mMainViewToolButton.Selected = true;
				}
				mDefaultViewActive = true;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		private void MainView_OnDeactivate()
		{
			try
			{
				if (!mMainViewToolButton.Disposed)
				{
					mMainViewToolButton.Selected = false;
				}
				mDefaultViewActive = false;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		private Location GetLocation(string idString)
		{
			int id;
			if (!int.TryParse(idString, out id))
			{
				return null;
			}
			if (!Location.IsInternalId(id))
			{
				Location loc;
				if (mLocDb.TryGet(id, out loc))
				{
					return loc;
				}
			}
			else
			{
				if (mRouteStartLoc != null && mRouteStartLoc.Id == id)
					return mRouteStartLoc;
				if (mRouteEndLoc != null && mRouteEndLoc.Id == id)
					return mRouteEndLoc;
				foreach (RouteStart loc in mStartLocations.Values)
				{
					if (loc.Id == id)
						return loc.ToLocation(mLocDb);
				}
				foreach (PortalDevice device in mPortalDevices.Values)
				{
					if (device.InfoLocation.Id == id)
						return device.InfoLocation;

					foreach (Location dest in device.Destinations)
					{
						if (dest.Id == id)
							return dest;
					}
				}
			}
			return null;
		}

		private RouteStart GetStartLocationByType(RouteStartType type)
		{
			foreach (RouteStart loc in mStartLocations.Values)
			{
				if (loc.Type == type)
					return loc;
			}
			return null;
		}

		private void FillLocationTypeChoice(ChoiceWrapper cho, bool addCompositeTypes)
		{
			cho.Clear();
			LocationTypeInfo typeInfo;
			if (addCompositeTypes)
			{
				typeInfo = Location.LocationTypeInfo(LocationType.Any);
				if (typeInfo.ShowFor(mLocDb.DatabaseType))
					cho.Add(typeInfo.FriendlyName, LocationType.Any);

				typeInfo = Location.LocationTypeInfo(LocationType.AnyPortal);
				if (typeInfo.ShowFor(mLocDb.DatabaseType))
					cho.Add(typeInfo.FriendlyName, LocationType.AnyPortal);
			}
			int customIndex = 0;
			foreach (LocationType type in Enum.GetValues(typeof(LocationType)))
			{
				if (type != LocationType.Any && type != LocationType.AnyPortal && type != LocationType._Unknown)
				{
					if (type == LocationType.Custom)
					{
						customIndex = cho.Count;
					}
					typeInfo = Location.LocationTypeInfo(type);
					if (typeInfo.ShowFor(mLocDb.DatabaseType))
						cho.Add(typeInfo.FriendlyName, type);
				}
			}
			cho.Selected = addCompositeTypes ? 0 : customIndex;
		}

		private bool ResetArrowPosition()
		{
			mArrowHud.ResetPosition();
			if (mArrowHud.IsHudVisible())
			{
				return true;
			}
			else if (!mArrowHud.Visible)
			{
				Util.Message("The arrow's position has been reset, but it is not visible because it is disabled");
			}
			else if (!mArrowHud.DisplayIndoors && mArrowHud.InDungeon)
			{
				Util.Message("The arrow's position has been reset, but it is not visible because you are indoors");
			}
			else
			{
				Util.Warning("The arrow's position has been reset, but it is not visible "
					+ "(possibly because of an error loading the image file)");
			}
			return false;
		}

		private void ShowHideArrow(bool show)
		{
			mArrowHud.Visible = show;
			if (show && !mArrowHud.IsHudVisible())
			{
				if (!mArrowHud.DisplayIndoors && mArrowHud.InDungeon)
				{
					Util.Message("The arrow is enabled, but not visible because you are indoors");
				}
				else
				{
					Util.Warning("The arrow is enabled, but not visible (possibly because of an error "
						+ "loading the image file)");
				}
			}
		}

		private void FillLocationList(ListWrapper lst, List<Location> locations, bool relativeCoords)
		{
			lst.Clear();
			foreach (Location loc in locations)
			{
				ListRow row = lst.Add();
				row[LocationList.Icon][1] = loc.Icon;
				row[LocationList.Name][0] = loc.Name;
				row[LocationList.GoIcon][1] = GoIcon;
				row[LocationList.ID][0] = loc.Id.ToString();
			}
			UpdateListCoords(lst, relativeCoords);
		}

		private void UpdateListCoords(ListWrapper lst, bool relative)
		{
			if (lst.ColCount <= LocationList.ID)
			{
				// Called on wrong list?
				return;
			}
			if (relative)
			{
				for (int r = 0; r < lst.RowCount; r++)
				{
					Location loc = GetLocation(lst[r][LocationList.ID][0] as string);
					if (loc != null)
					{
						lst[r][LocationList.Coords][0] =
							PlayerCoords.RelativeTo(loc.Coords).ToString("0.0");
						lst[r][LocationList.Coords].Color = Color.Gold;
					}
				}
			}
			else
			{
				for (int r = 0; r < lst.RowCount; r++)
				{
					Location loc = GetLocation(lst[r][LocationList.ID][0] as string);
					if (loc != null)
					{
						lst[r][LocationList.Coords][0] = loc.Coords.ToString("0.0");
						lst[r][LocationList.Coords].Color = Color.White;
					}
				}
			}
		}

		private void RelativeCoordsTimer_Tick(object sender, EventArgs e)
		{
			try
			{
				if (mDefaultViewActive && nbkMain.ActiveTab == MainTab.Atlas)
				{
					UpdateRelativeCoords();
				}

			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		private void UpdateRelativeCoords()
		{
			switch (nbkAtlas.ActiveTab)
			{
				case AtlasTab.Search:
					if (chkSearchShowRelative.Checked) { UpdateListCoords(lstSearchResults, true); }
					break;
				case AtlasTab.Details:
					if (mDetailsLoc != null)
						lblDetailsRelCoords.Text = PlayerCoords.RelativeTo(mDetailsLoc.Coords).ToString("0.0");
					break;
				case AtlasTab.Route:
					if (chkRouteShowRelative.Checked) { UpdateListCoords(lstRoute, true); }
					if (chkRouteFromHere.Checked) { edtRouteFromCoords.Text = PlayerCoords.ToString(); }
					if (chkRouteToHere.Checked) { edtRouteToCoords.Text = PlayerCoords.ToString(); }
					break;
				case AtlasTab.Favorites:
					if (chkFavoritesShowRelative.Checked) { UpdateListCoords(lstFavorites, true); }
					break;
				case AtlasTab.Recent:
					if (chkRecentShowRelative.Checked) { UpdateListCoords(lstRecent, true); }
					break;
			}
		}

		internal void SetRouteStart(Coordinates coords)
		{
			Location loc = mLocDb.GetLocationAt(coords);
			if (loc != null)
			{
				SetRouteStart(loc);
			}
			else
			{
				choRouteFrom.Selected = 0;
				chkRouteFromHere.Checked = false;
				edtRouteFromCoords.Text = coords.ToString();

				AddToRecentCoords(coords);

				nbkAtlas.ActiveTab = AtlasTab.Route;
				nbkMain.ActiveTab = MainTab.Atlas;
				DefaultView.Activated = true;
			}
		}

		internal void SetRouteStart(Location loc)
		{
			if (char.IsLetter(loc.Name[0]))
				choRouteFromLtr.Selected = char.ToUpper(loc.Name[0]) - 'A' + 1;
			else
				choRouteFromLtr.Selected = 0;
			int selIdx = mLocDb.GetAlphaIndex(loc.Name[0]).BinarySearch(loc);
			if (selIdx < 0 || selIdx + 1 >= choRouteFrom.Count)
				selIdx = 0;
			else
				selIdx += 1;
			choRouteFrom.Selected = selIdx;
			chkRouteFromHere.Checked = false;
			edtRouteFromCoords.Text = loc.Coords.ToString();

			AddToRecentLocations(loc);

			nbkAtlas.ActiveTab = AtlasTab.Route;
			nbkMain.ActiveTab = MainTab.Atlas;
			DefaultView.Activated = true;
		}

		internal void SetRouteEnd(Coordinates coords)
		{
			Location loc = mLocDb.GetLocationAt(coords);
			if (loc != null)
			{
				SetRouteEnd(loc);
			}
			else
			{
				choRouteTo.Selected = 0;
				chkRouteToHere.Checked = false;
				edtRouteToCoords.Text = coords.ToString();

				AddToRecentCoords(coords);

				nbkAtlas.ActiveTab = AtlasTab.Route;
				nbkMain.ActiveTab = MainTab.Atlas;
				DefaultView.Activated = true;
			}
		}

		internal void SetRouteEnd(Location loc)
		{
			if (char.IsLetter(loc.Name[0]))
				choRouteToLtr.Selected = char.ToUpper(loc.Name[0]) - 'A' + 1;
			else
				choRouteToLtr.Selected = 0;
			int selIdx = mLocDb.GetAlphaIndex(loc.Name[0]).BinarySearch(loc);
			if (selIdx < 0 || selIdx + 1 >= choRouteTo.Count)
				selIdx = 0;
			else
				selIdx += 1;
			choRouteTo.Selected = selIdx;
			chkRouteToHere.Checked = false;
			edtRouteToCoords.Text = loc.Coords.ToString();

			AddToRecentLocations(loc);

			nbkAtlas.ActiveTab = AtlasTab.Route;
			nbkMain.ActiveTab = MainTab.Atlas;
			DefaultView.Activated = true;
		}

		internal void ShowDetails(Location loc)
		{
			ShowDetails(loc, loc != null);
		}

		internal void ShowDetails(Location loc, bool switchTabs)
		{
			mDetailsLoc = loc;
			int icon;
			lstLocationDescription.Clear();
			if (loc == null)
			{
				lblDetailsName.Text = "No location selected";
				lblDetailsType.Text = "";
				lblDetailsAbsCoords.Text = "";
				lblDetailsRelCoords.Text = "";
				edtModifyName.Text = "";
				edtModifyCoords.Text = "";
				edtModifyExitCoords.Text = "";
				choModifyType.Selected = choModifyType.Count - 1;
				chkModifyUseInRoute.Checked = false;
				edtModifyDescription.Text = "";
				icon = Location.LocationTypeInfo(LocationType._Unknown).Icon;
				btnDetailsDungeonMap.TextColor = Color.Gray;
			}
			else
			{
				lblDetailsName.Text = loc.Name;
				lblDetailsType.Text = Location.LocationTypeInfo(loc.Type).FriendlyName;
				if (loc.HasExitCoords)
					lblDetailsType.Text += " (Exit " + loc.ExitCoords.ToString("0.0") + ")";

				lblDetailsAbsCoords.Text = loc.Coords.ToString("0.0");
				lblDetailsRelCoords.Text = PlayerCoords.RelativeTo(loc.Coords).ToString("0.0");
				edtModifyName.Text = loc.Name;
				edtModifyCoords.Text = loc.Coords.ToString("0.0");
				edtModifyExitCoords.Text = loc.ExitCoords.ToString("0.0");
				for (int i = 0; i < choModifyType.Count; i++)
				{
					if (loc.Type == (LocationType)choModifyType.Data[i])
					{
						choModifyType.Selected = i;
						break;
					}
				}
				chkModifyUseInRoute.Checked = loc.HasExitCoords && loc.UseInRouteFinding;
				edtModifyDescription.Text = loc.Notes;
				icon = loc.Icon;

				const int WRAP_WIDTH = 52;
				string notes = loc.Notes.Replace("\r", "").Replace("\t", " ");
				notes = Regex.Replace(notes, "  +", " ");
				List<string> lines = new List<string>();
				for (int i = 0; ; )
				{
					int newLine = notes.IndexOf('\n', i, Math.Min(WRAP_WIDTH, notes.Length - i));
					if (newLine >= 0)
					{
						lines.Add(notes.Substring(i, newLine - i));
						i = newLine + 1;
					}
					else if (i + WRAP_WIDTH >= notes.Length)
					{
						lines.Add(notes.Substring(i));
						break;
					}
					else
					{
						int wordBound = notes.LastIndexOf(' ', i + WRAP_WIDTH, WRAP_WIDTH);
						if (wordBound < 0) { wordBound = i + WRAP_WIDTH; }
						lines.Add(notes.Substring(i, wordBound - i));
						i = wordBound + 1;
					}
				}

				foreach (string line in lines)
				{
					lstLocationDescription.Add()[0][0] = line.Trim();
				}

				AddToRecentLocations(loc);

				btnDetailsDungeonMap.TextColor = mDungeonHud.DungeonMapAvailable(loc) ? Color.White : Color.LightGray;
			}
			icoDetailsIcon.SetImages(icon, icon);

			if (switchTabs)
			{
				nbkAtlas.ActiveTab = AtlasTab.Details;
				nbkMain.ActiveTab = MainTab.Atlas;
				DefaultView.Activated = true;
			}
		}

		private void AddToRecentLocations(Location loc)
		{
			if (!loc.IsInternalLocation)
			{
				string idString = loc.Id.ToString();
				for (int r = 0; r < lstRecent.RowCount; r++)
				{
					if (r > MaxRecentLocations || idString.Equals(lstRecent[r][LocationList.ID][0]))
					{
						lstRecent.Delete(r);
					}
				}
				ListRow row = lstRecent.Insert(0);
				row[LocationList.Icon][1] = loc.Icon;
				row[LocationList.Name][0] = loc.Name;
				row[LocationList.GoIcon][1] = GoIcon;
				row[LocationList.ID][0] = idString;
				UpdateListCoords(lstRecent, chkRecentShowRelative.Checked);
			}
		}

		private void AddToRecentCoords(Coordinates coords)
		{
			AddToRecentCoords(AcIcons.YellowTarget, coords.ToString(), coords);
		}

		private void AddToRecentCoords(int icon, string name, Coordinates coords)
		{
			for (int r = 0; r < lstRecentCoords.RowCount; r++)
			{
				if (r > MaxRecentLocations || name.Equals(lstRecentCoords[r][RecentCoordsList.Name][0]))
				{
					lstRecentCoords.Delete(r);
				}
			}
			ListRow row = lstRecentCoords.Insert(0);
			row[RecentCoordsList.Icon][1] = icon;
			row[RecentCoordsList.Name][0] = name;
			row[RecentCoordsList.Coords][0] = coords.ToString();
		}

		private void RefreshStartLocationListCoords()
		{
			for (int r = 0; r < lstStartLocations.RowCount; r++)
			{
				Coordinates coords = mStartLocations[(string)lstStartLocations[r][StartLocationsList.Name][0]].Coords;
				lstStartLocations[r][StartLocationsList.Coords][0] = coords.ToString(true);
				if (coords != Coordinates.NO_COORDINATES)
				{
					lstStartLocations[r][StartLocationsList.Name].Color = Color.White;
					lstStartLocations[r][StartLocationsList.Coords].Color = Color.White;
				}
				else
				{
					lstStartLocations[r][StartLocationsList.Name].Color = Color.Gray;
					lstStartLocations[r][StartLocationsList.Coords].Color = Color.Gray;
				}
			}
		}

		private void LocationDatabase_FavoritesListChanged(object sender, LocationChangedEventArgs e)
		{
			if (mSettingsLoaded)
				UpdateFavoritesList();
		}

		private void UpdateFavoritesList()
		{
			lstFavorites.Clear();
			foreach (Location loc in mLocDb.Favorites)
			{
				ListRow row = lstFavorites.Add();
				row[LocationList.Icon][1] = loc.GetIcon(false);
				row[LocationList.Name][0] = loc.Name;
				row[LocationList.GoIcon][1] = GoIcon;
				row[LocationList.ID][0] = loc.Id.ToString();
			}
			UpdateListCoords(lstFavorites, chkFavoritesShowRelative.Checked);
		}

		private void mArrowHud_HudEnabledChanged(object sender, EventArgs e)
		{
			chkShowArrow.Checked = mArrowHud.Visible;
			mArrowToolButton.Selected = mArrowHud.Visible;
		}

		private void mArrowHud_DestinationChanged(object sender, DestinationChangedEventArgs e)
		{
			string coordsStr = mArrowHud.DestinationCoords.ToString();
			edtArrowCoords.Text = coordsStr;
			edtArrowCoords.Caret = 0;

			if (mArrowHud.HasDestinationLocation)
			{
				AddToRecentCoords(mArrowHud.DestinationLocation.Icon, mArrowHud.DestinationLocation.Name, mArrowHud.DestinationCoords);
			}
			else if (mArrowHud.HasDestinationObject)
			{
				AddToRecentCoords(mArrowHud.DestinationObject.Icon, mArrowHud.DestinationObject.Name, mArrowHud.DestinationCoords);
			}
			else
			{
				AddToRecentCoords(mArrowHud.DestinationCoords);
			}
		}

		private void MapHud_VisibleChanged(object sender, EventArgs e)
		{
			chkDerethMapShow.Checked = mMapHud.Visible;
			mDerethToolButton.Selected = mMapHud.Visible;
			if (!chkAllowBothMaps.Checked && mMapHud.Visible)
			{
				mDungeonHud.Visible = false;
			}
		}

		private void DungeonHud_VisibleChanged(object sender, EventArgs e)
		{
			chkDungeonMapShow.Checked = mDungeonHud.Visible;
			mDungeonToolButton.Selected = mDungeonHud.Visible;
			if (!chkAllowBothMaps.Checked && mDungeonHud.Visible)
			{
				mMapHud.Visible = false;
			}
		}

		private void DungeonHud_DungeonListUpdated(object sender, EventArgs e)
		{
			ReloadDungeonList();
		}

		private void Toolbar_VisibleChanged(object sender, EventArgs e)
		{
			chkHudsToolbarShow.Checked = mToolbar.Visible;
		}

		private void Toolbar_DisplayChanged(object sender, EventArgs e)
		{
			chkHudToolbarIcons.Checked = (mToolbar.Display & ToolbarDisplay.Icons) != 0;
			chkHudToolbarText.Checked = (mToolbar.Display & ToolbarDisplay.Text) != 0;
		}

		private void ReloadDungeonList()
		{
			choDungeonMap.Clear();
			choDungeonMap.Add("<None>", 0);
			foreach (KeyValuePair<string, int> kvp in mDungeonHud.DungeonNamesAndIDs)
			{
				if (!mDungeonHud.IsBlacklisted(kvp.Value))
					choDungeonMap.Add(kvp.Key, kvp.Value);
			}
			//DungeonHud_MapChanged(null, null);
		}

		private void DungeonHud_MapChanged(object sender, EventArgs e)
		{
			for (int i = 0; i < choDungeonMap.Count; i++)
			{
				object id = choDungeonMap.Data[i];
				if (id is int && (int)id == mDungeonHud.CurrentDungeon)
				{
					choDungeonMap.Selected = i;
					return;
				}
			}
			choDungeonMap.Selected = 0;
		}

		private void DungeonHud_MapBlacklisted(object sender, MapBlacklistedEventArgs e)
		{
			if (e.DungeonId == mDungeonHud.CurrentDungeon)
			{
				choDungeonMap.Selected = 0;
			}
			for (int i = 0; i < choDungeonMap.Count; i++)
			{
				object id = choDungeonMap.Data[i];
				if (id is int && (int)id == e.DungeonId)
				{
					choDungeonMap.Remove(i);
					break;
				}
			}
		}

		private void PortalDevice_CoordsChanged(object sender, EventArgs e)
		{
			PortalDevice device = (PortalDevice)sender;
			ListRow row = lstPortalDevices[device.Row];

			if (device.Detected)
			{
				row[PortalDevicesList.Name].Color = Color.White;
				row[PortalDevicesList.Detected].Color = Color.LightGreen;
				row[PortalDevicesList.Detected][0] = "Detected";
			}
			else
			{
				row[PortalDevicesList.Name].Color = Color.Gray;
				row[PortalDevicesList.Detected].Color = Color.Gray;
				row[PortalDevicesList.Detected][0] = "Not Detected";
			}
		}

		private void SetWindowAlpha(WindowHud window, bool active, int alpha)
		{
			if (active)
			{
				window.AlphaFrameActive = alpha;
			}
			else
			{
				window.AlphaFrameInactive = alpha;
			}
		}

		private void InitMouseList(ListWrapper lst, string controlName, MouseButtons buttons)
		{
			ListRow r = lst.Add();
			r[MouseList.Name][0] = controlName;
			r[MouseList.Left][0] = (buttons & MouseButtons.Left) != 0;
			r[MouseList.Middle][0] = (buttons & MouseButtons.Middle) != 0;
			r[MouseList.Right][0] = (buttons & MouseButtons.Right) != 0;
			r[MouseList.X1][0] = (buttons & MouseButtons.XButton1) != 0;
			r[MouseList.X2][0] = (buttons & MouseButtons.XButton2) != 0;
		}

		private void ShowMapMouseHelp()
		{
			Util.HelpMessage("Use this to set up the mouse buttons to control the map. " +
				"Click on the text in the list for a description of what that control does.\n" +
				"The buttons are:\n" +
				"L = Left Mouse Button\n" +
				"M = Middle Mouse Button (click the scroll wheel)\n" +
				"R = Right Mouse Button\n" +
				"X1 = X-Button 1 (only on some Microsoft mice)\n" +
				"X2 = X-Button 2 (only on some Microsoft mice)\n");
		}

		private void SetViewRegion(Rectangle rect)
		{
			if (DefaultView.Position != rect)
			{
				bool reactivate = mDefaultViewActive && DefaultView.Position.Size != rect.Size;
				DefaultView.Position = rect;
				if (reactivate)
				{
					// Workaround for bug that causes OnDeactivate to be 
					// called when the view size changes
					DefaultView.Deactivate();
					DefaultView.Activate();
				}
			}
		}

		private void SetViewSize(Size size)
		{
			Rectangle rect = new Rectangle(DefaultView.Position.Location, size);
			SetViewRegion(rect);
		}

		private void SetViewLocation(Point point)
		{
			Rectangle rect = new Rectangle(point, DefaultView.Position.Size);
			SetViewRegion(rect);
		}
		#endregion

		//
		#region Control Events
		//

		[ControlEvent("nbkMain", "Change")]
		private void nbkMain_Change(object sender, IndexChangeEventArgs e)
		{
			try
			{
				SetViewSize(nbkMainTabsSize[e.Index]);
				if (e.Index == MainTab.Atlas)
				{
					btnDetailsShrink.Text = HideDetailsText;
					UpdateRelativeCoords();
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		//
		#region HUDs Tab
		//

		//
		#region HUDs > Arrow Tab
		//
		[ControlEvent("chkShowArrow", "Change")]
		private void chkShowArrow_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				ShowHideArrow(e.Checked);
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkArrowIndoors", "Change")]
		private void chkArrowIndoors_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				mArrowHud.DisplayIndoors = e.Checked;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("edtArrowCoords", "End")]
		private void edtArrowCoords_End(object sender, TextBoxEndEventArgs e)
		{
			try
			{
				Coordinates dest;
				if (Coordinates.TryParse(edtArrowCoords.Text, out dest))
				{
					Location loc = mLocDb.GetLocationAt(dest);
					if (loc != null)
						mArrowHud.DestinationLocation = loc;
					else
						mArrowHud.DestinationCoords = dest;
				}
				else
				{
					Util.Error("Invalid coordinates. Coordinates must be in the form: 00.0N, 00.0E");
					edtArrowCoords.Text = mArrowHud.DestinationCoords.ToString();
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnArrowHere", "Click")]
		private void btnArrowHere_Click(object sender, ControlEventArgs e)
		{
			try
			{
				Coordinates coords = PlayerCoords;
				Location loc = mLocDb.GetLocationAt(coords);
				if (loc != null)
					mArrowHud.DestinationLocation = loc;
				else
					mArrowHud.DestinationCoords = coords;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkShowDestination", "Change")]
		private void chkShowDestination_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				mArrowHud.ShowDestinationOver = e.Checked;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkDistUnderArrow", "Change")]
		private void chkDistUnderArrow_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				mArrowHud.ShowDistanceUnder = e.Checked;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkLockArrowPosition", "Change")]
		private void chkLockArrowPosition_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				mArrowHud.PositionLocked = e.Checked;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnLinkCoordsHelp", "Click")]
		private void btnLinkCoordsHelp_Click(object sender, ControlEventArgs e)
		{
			try
			{
				Util.HelpMessage("This option turns coordinates in the chat area into clickable links "
					+ "(like this: <Tell:IIDString:110011:GoArrow_Example>10.7N, 52.3E<\\Tell>).");
				Util.HelpMessage("Click on the link to make the arrow point to those coords.");
				Util.HelpMessage("Shift + Click on the link to set the route start to those coords.");
				Util.HelpMessage("Ctrl + Click on the link to set the route end to those coords.");
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnResetArrowPosition", "Click")]
		private void btnResetArrowPosition_Click(object sender, ControlEventArgs e)
		{
			mArrowHud.ResetPosition();
			Util.Message("The arrow should now be in a visible location on the screen");
		}

		[ControlEvent("choArrowImage", "Change")]
		private void choArrowImage_Change(object sender, IndexChangeEventArgs e)
		{
			try
			{
				if (mSettingsLoaded && e.Index >= 0 && e.Index < choArrowImage.Count)
					mArrowHud.LoadArrowImageAsync(choArrowImage.Text[e.Index]);
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("edtTextSize", "End")]
		private void edtTextSize_End(object sender, TextBoxEndEventArgs e)
		{
			try
			{
				int textSize;
				if (int.TryParse(edtTextSize.Text, out textSize))
				{
					if (textSize >= ArrowHud.MinTextSize && textSize <= ArrowHud.MaxTextSize)
					{
						mArrowHud.TextSize = textSize;
					}
					else
					{
						Util.Error("Text size must be between " + ArrowHud.MinTextSize
							+ " and " + ArrowHud.MaxTextSize);
					}
				}
				else
				{
					Util.Error("Text size must be a number");
				}
				edtTextSize.Text = mArrowHud.TextSize.ToString();
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnTextSizeUp", "Click")]
		private void btnTextSizeUp_Click(object sender, ControlEventArgs e)
		{
			try
			{
				if (mArrowHud.TextSize < ArrowHud.MaxTextSize)
				{
					mArrowHud.TextSize++;
					edtTextSize.Text = mArrowHud.TextSize.ToString();
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnTextSizeDown", "Click")]
		private void btnTextSizeDown_Click(object sender, ControlEventArgs e)
		{
			try
			{
				if (mArrowHud.TextSize > ArrowHud.MinTextSize)
				{
					mArrowHud.TextSize--;
					edtTextSize.Text = mArrowHud.TextSize.ToString();
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("choTextColor", "Change")]
		private void choTextColor_Change(object sender, IndexChangeEventArgs e)
		{
			try
			{
				if (e.Index >= 0 && e.Index < choTextColor.Count)
				{
					// The color is stored as a hex RGB color in the data
					uint rgb = Convert.ToUInt32(choTextColor.Data[e.Index].ToString(), 16);
					mArrowHud.TextColor = Color.FromArgb(unchecked((int)(0xFF000000 | rgb)));
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkBold", "Change")]
		private void chkBold_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				mArrowHud.TextBold = e.Checked;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}
		#endregion HUDs > Arrow Tab

		//
		#region HUDs > Dereth Map Tab
		//
		[ControlEvent("chkDerethMapShow", "Change")]
		private void chkDerethMapShow_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				mMapHud.Visible = e.Checked;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkDerethMapCenterPlayer", "Change")]
		private void chkDerethMapCenterPlayer_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				mMapHud.CenterOnPlayer = e.Checked;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkDerethMapShowRoute", "Change")]
		private void chkDerethMapShowRoute_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				mMapHud.ShowRoute = e.Checked;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkDerethMapShowLocs", "Change")]
		private void chkDerethMapShowLocs_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				mMapHud.ShowLocations = e.Checked;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkDerethMapShowAllLocs", "Change")]
		private void chkDerethMapShowAllLocs_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				mMapHud.ShowLocationsAllZooms = e.Checked;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkDerethMapShowLabels", "Change")]
		private void chkDerethMapShowLabels_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				mMapHud.ShowLabels = e.Checked;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("edtDerethMapCenterOn", "End")]
		private void edtDerethMapCenterOnn_End(object sender, TextBoxEndEventArgs e)
		{
			try
			{
				Coordinates center;
				if (e.Success)
				{
					btnDerethMapCenterGo_Click(null, null);
				}
				else if (Coordinates.TryParse(edtDerethMapCenterOn.Text, out center))
				{
					edtDerethMapCenterOn.Text = center.ToString();
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDerethMapCenterGo", "Click")]
		private void btnDerethMapCenterGo_Click(object sender, ControlEventArgs e)
		{
			try
			{
				Coordinates center;
				if (Coordinates.TryParse(edtDerethMapCenterOn.Text, out center))
				{
					mMapHud.Visible = true;
					mMapHud.CoordsAtCenter = center;
					edtDerethMapCenterOn.Text = center.ToString();
				}
				else
				{
					Util.Warning(edtDerethMapCenterOn.Text
						+ " are not valid coordinates. They must be in the form 0.0N, 0.0E");
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDerethMapMouseHelp", "Click")]
		private void btnDerethMapMouseHelp_Click(object sender, ControlEventArgs e)
		{
			try
			{
				ShowMapMouseHelp();
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("lstDerethMapMouse", "Selected")]
		private void lstDerethMapMouse_Selected(object sender, ListSelectEventArgs e)
		{
			try
			{
				MouseButtons b;
				switch (e.Column)
				{
					case MouseList.Name:
						switch ((string)lstDerethMapMouse[e.Row][MouseList.Name][0])
						{
							case MouseControls.PanMap:
								Util.HelpMessage("Drag the map with this button to pan the map.");
								break;
							case MouseControls.SelectLocation:
								Util.HelpMessage("Click with this button to make the arrow point to the location. "
									+ "Shift+Click to set the location as the route start, "
									+ "or Ctrl+Click to set it as the route end.");
								break;
							case MouseControls.ContextMenu:
								Util.HelpMessage("Click with this button to show a context menu for the location.");
								break;
						}
						return;
					case MouseList.Left: { b = MouseButtons.Left; break; }
					case MouseList.Middle: { b = MouseButtons.Middle; break; }
					case MouseList.Right: { b = MouseButtons.Right; break; }
					case MouseList.X1: { b = MouseButtons.XButton1; break; }
					case MouseList.X2: { b = MouseButtons.XButton2; break; }
					default: { return; }
				}

				// If the checkbox was selected...
				if ((bool)lstDerethMapMouse[e.Row][e.Column][0])
				{
					switch ((string)lstDerethMapMouse[e.Row][MouseList.Name][0])
					{
						case MouseControls.PanMap: { mMapHud.DragButton |= b; break; }
						case MouseControls.SelectLocation: { mMapHud.SelectLocationButton |= b; break; }
						case MouseControls.ContextMenu: { mMapHud.ContextMenuButton |= b; break; }
						case MouseControls.Details: { mMapHud.DetailsButton |= b; break; }
					}
				}
				else
				{
					switch ((string)lstDerethMapMouse[e.Row][MouseList.Name][0])
					{
						case MouseControls.PanMap: { mMapHud.DragButton &= ~b; break; }
						case MouseControls.SelectLocation: { mMapHud.SelectLocationButton &= ~b; break; }
						case MouseControls.ContextMenu: { mMapHud.ContextMenuButton &= ~b; break; }
						case MouseControls.Details: { mMapHud.DetailsButton &= ~b; break; }
					}
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}
		#endregion HUDs > Dereth Map Tab

		//
		#region HUDs > Dungeon Map Tab
		//
		[ControlEvent("chkDungeonMapShow", "Change")]
		private void chkDungeonMapShow_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				mDungeonHud.Visible = e.Checked;
				if (e.Checked)
					mDungeonHud.Minimized = false;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDungeonMapThisDungeon", "Click")]
		private void btnDungeonMapThisDungeon_Click(object sender, ControlEventArgs e)
		{
			try
			{
				if (!mDungeonHud.IsDungeon(Host.Actions.Landcell))
				{
					Util.Warning("You are not in a dungeon.");
				}
				else if (!mDungeonHud.DungeonMapAvailable(mDungeonHud.GetDungeonId(Host.Actions.Landcell)))
				{
					Util.Warning("No dungeon map available for your current location.");
				}
				else
				{
					mDungeonHud.Visible = true;
					mDungeonHud.Minimized = false;
					mDungeonHud.LoadDungeonByLandblock(Host.Actions.Landcell);
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("choDungeonMap", "Change")]
		private void choDungeonMap_Change(object sender, IndexChangeEventArgs e)
		{
			try
			{
				if (e.Index >= 0 && e.Index < choDungeonMap.Count)
				{
					object id = choDungeonMap.Data[e.Index];
					if (id is int)
					{
						mDungeonHud.Visible = true;
						mDungeonHud.Minimized = false;
						mDungeonHud.LoadDungeonById((int)id);
					}
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkDungeonMapAutoLoad", "Change")]
		private void chkDungeonMapAutoLoad_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				mDungeonHud.AutoLoadMaps = e.Checked;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDungeonMapN", "Click")]
		private void btnDungeonMapN_Click(object sender, ControlEventArgs e)
		{
			try
			{
				mDungeonHud.RotationDegrees = 0;
				chkDungeonMapAutoRotate.Checked = false;
				mDungeonHud.AutoRotateMap = false;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDungeonMapS", "Click")]
		private void btnDungeonMapS_Click(object sender, ControlEventArgs e)
		{
			try
			{
				mDungeonHud.RotationDegrees = 180;
				chkDungeonMapAutoRotate.Checked = false;
				mDungeonHud.AutoRotateMap = false;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDungeonMapE", "Click")]
		private void btnDungeonMapE_Click(object sender, ControlEventArgs e)
		{
			try
			{
				mDungeonHud.RotationDegrees = 90;
				chkDungeonMapAutoRotate.Checked = false;
				mDungeonHud.AutoRotateMap = false;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDungeonMapW", "Click")]
		private void btnDungeonMapW_Click(object sender, ControlEventArgs e)
		{
			try
			{
				mDungeonHud.RotationDegrees = 270;
				chkDungeonMapAutoRotate.Checked = false;
				mDungeonHud.AutoRotateMap = false;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkDungeonMapCompass", "Change")]
		private void chkDungeonMapCompass_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				mDungeonHud.ShowCompass = e.Checked;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkDungeonMapAutoRotate", "Change")]
		private void chkDungeonMapAutoRotate_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				if (e.Checked)
				{
					mDungeonHud.AutoRotateMap = e.Checked;
				}
				else
				{
					btnDungeonMapN_Click(null, null);
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkDungeonMapMoveWithPlayer", "Change")]
		private void chkDungeonMapMoveWithPlayer_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				mDungeonHud.MoveWithPlayer = e.Checked;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDungeonMapMouseHelp", "Click")]
		private void btnDungeonMapMouseHelp_Click(object sender, ControlEventArgs e)
		{
			try
			{
				ShowMapMouseHelp();
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("lstDungeonMapMouse", "Selected")]
		private void lstDungeonMapMouse_Selected(object sender, ListSelectEventArgs e)
		{
			try
			{
				MouseButtons b;
				switch (e.Column)
				{
					case MouseList.Name:
						switch ((string)lstDungeonMapMouse[e.Row][MouseList.Name][0])
						{
							case MouseControls.PanMap:
								Util.HelpMessage("Drag the map with this button to pan the map.");
								break;
						}
						return;
					case MouseList.Left: { b = MouseButtons.Left; break; }
					case MouseList.Middle: { b = MouseButtons.Middle; break; }
					case MouseList.Right: { b = MouseButtons.Right; break; }
					case MouseList.X1: { b = MouseButtons.XButton1; break; }
					case MouseList.X2: { b = MouseButtons.XButton2; break; }
					default: { return; }
				}

				// If the checkbox was selected...
				if ((bool)lstDungeonMapMouse[e.Row][e.Column][0])
				{
					switch ((string)lstDungeonMapMouse[e.Row][MouseList.Name][0])
					{
						case MouseControls.PanMap: { mDungeonHud.DragButton |= b; break; }
					}
				}
				else
				{
					switch ((string)lstDungeonMapMouse[e.Row][MouseList.Name][0])
					{
						case MouseControls.PanMap: { mDungeonHud.DragButton &= ~b; break; }
					}
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDungeonMapClearCache", "Click")]
		private void btnDungeonMapClearCache_Click(object sender, ControlEventArgs e)
		{
			try
			{
				mDungeonHud.ClearCache();
				ReloadDungeonList();
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDungeonMapCancelDL", "Click")]
		private void btnDungeonMapCancelDL_Click(object sender, ControlEventArgs e)
		{
			try
			{
				if (mDungeonHud.CancelDownload())
				{
					Util.Message("The download has been canceled");
				}
				else
				{
					Util.Warning("There is no download currently in progress");
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}
		#endregion HUDs > Dungeon Map Tab

		//
		#region HUDs > General Tab
		//
		[ControlEvent("chkHudsToolbarShow", "Change")]
		private void chkHudsToolbarShow_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				mToolbar.Visible = e.Checked;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkHudToolbarIcons", "Change")]
		private void chkHudToolbarIcons_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				if (e.Checked)
				{
					mToolbar.Display |= ToolbarDisplay.Icons;
				}
				else
				{
					mToolbar.Display &= ~ToolbarDisplay.Icons;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkHudToolbarText", "Change")]
		private void chkHudToolbarText_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				if (e.Checked)
				{
					mToolbar.Display |= ToolbarDisplay.Text;
				}
				else
				{
					mToolbar.Display &= ~ToolbarDisplay.Text;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkHudToolbarHoriz", "Change")]
		private void chkHudToolbarHoriz_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				chkHudToolbarVert.Checked = !e.Checked;
				if (chkHudToolbarHoriz.Checked)
					mToolbar.Orientation = ToolbarOrientation.Horizontal;
				else
					mToolbar.Orientation = ToolbarOrientation.Vertical;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkHudToolbarVert", "Change")]
		private void chkHudToolbarVert_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				chkHudToolbarHoriz.Checked = !e.Checked;
				if (chkHudToolbarHoriz.Checked)
					mToolbar.Orientation = ToolbarOrientation.Horizontal;
				else
					mToolbar.Orientation = ToolbarOrientation.Vertical;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("lstToolbarButtons", "Selected")]
		private void lstToolbarButtons_Selected(object sender, ListSelectEventArgs e)
		{
			try
			{
				bool visible = (bool)lstToolbarButtons[e.Row][ToolbarButtonsList.Visible][0];
				if (e.Column != ToolbarButtonsList.Visible)
				{
					lstToolbarButtons[e.Row][ToolbarButtonsList.Visible][0] = (visible = !visible);
				}
				mToolbar[e.Row].Visible = visible;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkAllowBothMaps", "Change")]
		private void chkAllowBothMaps_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				if (!e.Checked && mMapHud.Visible)
				{
					mDungeonHud.Visible = false;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("sldHudOpacityActive", "Change")]
		private void sldHudOpacityActive_Change(object sender, IndexChangeEventArgs e)
		{
			try
			{
				int index = e.Index;
				if (index < MinHudOpacity)
				{
					sldHudOpacityActive.SliderPostition = index = MinHudOpacity;
				}

				mArrowHud.Alpha = index;
				SetWindowAlpha(mMapHud, true, index);
				SetWindowAlpha(mDungeonHud, true, index);
				if (index < sldHudOpacityInactive.SliderPostition)
				{
					sldHudOpacityInactive.SliderPostition = index;
					SetWindowAlpha(mMapHud, false, index);
					SetWindowAlpha(mDungeonHud, false, index);
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("sldHudOpacityInactive", "Change")]
		private void sldHudOpacityInactive_Change(object sender, IndexChangeEventArgs e)
		{
			try
			{
				int index = e.Index;
				if (index < MinHudOpacity)
				{
					sldHudOpacityInactive.SliderPostition = index = MinHudOpacity;
				}

				SetWindowAlpha(mMapHud, false, index);
				SetWindowAlpha(mDungeonHud, false, index);
				if (index > sldHudOpacityActive.SliderPostition)
				{
					sldHudOpacityActive.SliderPostition = index;
					mArrowHud.Alpha = index;
					SetWindowAlpha(mMapHud, true, index);
					SetWindowAlpha(mDungeonHud, true, index);
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnResetHudPositions", "Click")]
		private void btnResetHudPositions_Click(object sender, ControlEventArgs e)
		{
			try
			{
				ResetHudPositions(true);
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}
		#endregion HUDs > General Tab

		#endregion HUDs Tab

		//
		#region Atlas Tab
		//
		[ControlEvent("nbkAtlas", "Change")]
		private void nbkAtlas_Change(object sender, IndexChangeEventArgs e)
		{
			try
			{
				SetViewSize(nbkMainTabsSize[MainTab.Atlas]);
				btnDetailsShrink.Text = HideDetailsText;
				UpdateRelativeCoords();
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		private void HandleLocationListClick(ListWrapper lst, ListSelectEventArgs e, bool isRouteList)
		{
			try
			{
				Location loc;
				switch (e.Column)
				{
					case LocationList.Icon:
						lst.Delete(e.Row);
						if (isRouteList && e.Row < mRoute.Count)
						{
							mRoute.RemoveAt(e.Row);
							mMapHud.Route = mRoute;
						}
						break;
					case LocationList.Name:
					case LocationList.Coords:
						if (null != (loc = GetLocation(lst[e.Row][LocationList.ID][0] as string)))
						{
							if (Util.IsControlDown())
								SetRouteEnd(loc);
							else if (Util.IsShiftDown())
								SetRouteStart(loc);
							else
								ShowDetails(loc);
						}
						else
						{
							Util.Error("An error has occurred getting the details of "
								+ lst[e.Row][LocationList.Name][0]);
						}
						break;
					case LocationList.GoIcon:
						if (isRouteList)
						{
							mArrowHud.Route = mRoute;
							mArrowHud.RouteIndex = e.Row;
							mArrowHud.Visible = true;
						}
						else if (null != (loc = GetLocation(lst[e.Row][LocationList.ID][0] as string)))
						{
							mArrowHud.DestinationLocation = loc;
							mArrowHud.Visible = true;
						}
						else
						{
							Util.Error("An error has occurred getting the details of "
								+ lst[e.Row][LocationList.Name][0]);
						}
						break;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		//
		#region Atlas > Search Tab
		//
		[ControlEvent("chkSearchName", "Change")]
		private void chkSearchName_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				if (Util.IsControlDown())
				{
					chkSearchName.Checked = true;
					chkSearchNearby.Checked = false;
				}
				else if (!e.Checked && !chkSearchNearby.Checked)
					chkSearchNearby.Checked = true;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkSearchNearby", "Change")]
		private void chkSearchNearby_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				if (Util.IsControlDown())
				{
					chkSearchName.Checked = false;
					chkSearchNearby.Checked = true;
				}
				else if (!e.Checked && !chkSearchName.Checked)
					chkSearchName.Checked = true;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("edtSearchName", "Change")]
		private void edtSearchName_Change(object sender, TextBoxChangeEventArgs e)
		{
			try
			{
				chkSearchName.Checked = (e.Text != "");
				if (e.Text != "")
				{
					chkSearchName.Checked = true;
					if (edtSearchCoords.Text == "")
						chkSearchNearby.Checked = false;
				}
				else
				{
					chkSearchName.Checked = !chkSearchNearby.Checked;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("edtSearchName", "End")]
		private void edtSearchName_End(object sender, TextBoxEndEventArgs e)
		{
			if (e.Success)
				btnSearchGo_Click(null, null);
		}

		[ControlEvent("edtSearchRadius", "End")]
		private void edtSearchRadius_End(object sender, TextBoxEndEventArgs e)
		{
			double val;
			if (!double.TryParse(edtSearchRadius.Text, out val) || val <= 0)
			{
				Util.Error("Invalid search radius; must be a number greater than 0");
				edtSearchRadius.Text = "5";
			}
		}

		private void edtSearchCoords_Change(string text)
		{
			try
			{
				if (text != "")
				{
					chkSearchNearby.Checked = true;
					if (edtSearchName.Text == "")
						chkSearchName.Checked = false;
				}
				else
				{
					chkSearchNearby.Checked = false;
					chkSearchName.Checked = true;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("edtSearchCoords", "Change")]
		private void edtSearchCoords_Change(object sender, TextBoxChangeEventArgs e)
		{
			edtSearchCoords_Change(e.Text);
		}

		[ControlEvent("edtSearchCoords", "End")]
		private void edtSearchCoords_End(object sender, TextBoxEndEventArgs e)
		{
			try
			{
				Coordinates coords;
				if (Coordinates.TryParse(edtSearchCoords.Text, out coords))
				{
					edtSearchCoords.Text = coords.ToString();
					edtSearchCoords.Caret = 0;
				}
				if (e.Success)
					btnSearchGo_Click(null, null);
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnSearchHere", "Click")]
		private void btnSearchHere_Click(object sender, ControlEventArgs e)
		{
			try
			{
				edtSearchCoords.Text = PlayerCoords.ToString();
				edtSearchCoords_Change(edtSearchCoords.Text);
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnSearchGo", "Click")]
		private void btnSearchGo_Click(object sender, ControlEventArgs e)
		{
			try
			{
				List<Location> results;
				LocationType limitTo = (LocationType)choSearchLimit.Data[choSearchLimit.Selected];

				if (chkSearchNearby.Checked)
				{
					Coordinates coords;
					if (!Coordinates.TryParse(edtSearchCoords.Text, out coords))
					{
						Util.Error("Invalid coordinates for nearby search");
						return;
					}
					double radius;
					if (!double.TryParse(edtSearchRadius.Text, out radius) || radius <= 0)
					{
						Util.Error("Invalid search radius; must be a number greater than 0");
						return;
					}
					if (chkSearchName.Checked)
					{
						results = mLocDb.Search(edtSearchName.Text, searchIn[choSearchIn.Selected],
							coords, radius, limitTo);
					}
					else
					{
						results = mLocDb.Search(coords, radius, limitTo);
					}
				}
				else if (chkSearchName.Checked)
				{
					results = mLocDb.Search(edtSearchName.Text, searchIn[choSearchIn.Selected], limitTo);
				}
				else
				{
					Util.Error("You must select either a Text or Nearby search");
					return;
				}
				FillLocationList(lstSearchResults, results, chkSearchShowRelative.Checked);
				lblSearchMatchesFound.Text = results.Count.ToString("#,0");
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("lstSearchResults", "Selected")]
		private void lstSearchResults_Selected(object sender, ListSelectEventArgs e)
		{
			HandleLocationListClick(lstSearchResults, e, false);
		}

		[ControlEvent("chkSearchShowRelative", "Change")]
		private void chkSearchShowRelative_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				UpdateListCoords(lstSearchResults, e.Checked);
				//mRelativeCoordsTimer.Interval = e.Checked ? RelativeCoordsFast : RelativeCoordsSlow;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}
		#endregion Atlas > Search Tab

		//
		#region Atlas > Details Tab
		//
		[ControlEvent("btnDetailsShrink", "Click")]
		private void btnDetailsShrink_Click(object sender, ControlEventArgs e)
		{
			try
			{
				Rectangle pos = DefaultView.Position;
				if (pos.Size != nbkMainTabsSize[MainTab.Atlas])
				{
					SetViewSize(nbkMainTabsSize[MainTab.Atlas]);
					btnDetailsShrink.Text = HideDetailsText;
				}
				else
				{
					SetViewSize(new Size(pos.Width, 136));
					btnDetailsShrink.Text = ShowDetailsText;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDetailsCopyCoords", "Click")]
		private void btnDetailsCopyCoords_Click(object sender, ControlEventArgs e)
		{
			try
			{
				if (mDetailsLoc == null)
				{
					Util.Error("Select a location first");
				}
				else
				{
					System.Windows.Forms.Clipboard.SetText(mDetailsLoc.Name + " ["
						+ mDetailsLoc.Coords.ToString("0.0") + "]");
					//+ " (Relative to me: "
					//+ PlayerCoords.RelativeTo(mDetailsLoc.Coords).ToString("0.0") + ")");
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDetailsRouteStart", "Click")]
		private void btnDetailsRouteStart_Click(object sender, ControlEventArgs e)
		{
			try
			{
				if (mDetailsLoc == null)
					Util.Error("Select a location first");
				else
					SetRouteStart(mDetailsLoc);
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDetailsRouteEnd", "Click")]
		private void btnDetailsRouteEnd_Click(object sender, ControlEventArgs e)
		{
			try
			{
				if (mDetailsLoc == null)
					Util.Error("Select a location first");
				else
					SetRouteEnd(mDetailsLoc);
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDetailsArrowTarget", "Click")]
		private void btnDetailsArrowTarget_Click(object sender, ControlEventArgs e)
		{
			try
			{
				if (mDetailsLoc == null)
					Util.Error("Select a location first");
				else
				{
					mArrowHud.DestinationLocation = mDetailsLoc;
					mArrowHud.Visible = true;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDetailsAddFav", "Click")]
		private void btnDetailsAddFav_Click(object sender, ControlEventArgs e)
		{
			try
			{
				if (mDetailsLoc == null)
					Util.Error("Select a location first");
				else if (mDetailsLoc.IsInternalLocation)
					Util.Warning("This location cannot be added as a favorite");
				else if (mDetailsLoc.IsFavorite)
					Util.Warning(mDetailsLoc.Name + " is already in your favorites list");
				else
				{
					mDetailsLoc.IsFavorite = true;
					nbkAtlas.ActiveTab = AtlasTab.Favorites;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDetailsLocateOnMap", "Click")]
		private void btnDetailsLocateOnMap_Click(object sender, ControlEventArgs e)
		{
			try
			{
				if (mDetailsLoc == null)
				{
					Util.Error("Select a location first");
				}
				else
				{
					if (mMapHud.Zoom < 4.1f)
						mMapHud.Zoom = 4.1f;
					else if (mMapHud.Zoom > 10f)
						mMapHud.Zoom = 10f;

					mMapHud.CenterOnCoords(mDetailsLoc.Coords);
					mMapHud.Visible = true;
					mMapHud.Minimized = false;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDetailsDungeonMap", "Click")]
		private void btnDetailsDungeonMap_Click(object sender, ControlEventArgs e)
		{
			try
			{
				if (mDetailsLoc == null)
				{
					Util.Error("Select a location first");
				}
				else if (!mDungeonHud.DungeonMapAvailable(mDetailsLoc))
				{
					if (mDetailsLoc.Type != LocationType.Dungeon)
					{
						Util.Warning("Dungeon maps are only available for dungeons.");
					}
					else
					{
						Util.Warning("No dungeon map available for " + mDetailsLoc.Name);
					}
				}
				else
				{
					mDungeonHud.Visible = true;
					mDungeonHud.Minimized = false;
					mDungeonHud.LoadDungeonById(mDungeonHud.GetDungeonId(mDetailsLoc));
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		//
		// Atlas > Details > Modify/Add Location Tab
		//
		[ControlEvent("btnModifyCoordsHere", "Click")]
		private void btnModifyCoordsHere_Click(object sender, ControlEventArgs e)
		{
			try
			{
				edtModifyCoords.Text = PlayerCoords.ToString("0.0");
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnModifyExitCoordsHere", "Click")]
		private void btnModifyExitCoordsHere_Click(object sender, ControlEventArgs e)
		{
			try
			{
				edtModifyExitCoords.Text = PlayerCoords.ToString("0.0");
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnModifyExitCoordsNone", "Click")]
		private void btnModifyExitCoordsNone_Click(object sender, ControlEventArgs e)
		{
			try
			{
				edtModifyExitCoords.Text = Coordinates.NO_COORDINATES_STRING;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkModifyUseInRoute", "Change")]
		private void chkModifyUseInRoute_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				if (e.Checked && (edtModifyExitCoords.Text == Coordinates.NO_COORDINATES_STRING
						|| edtModifyExitCoords.Text == ""))
				{
					chkModifyUseInRoute.Checked = false;
					Util.Warning("In order to be used in route finding, a location must have exit coordinates");
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnModifySave", "Click")]
		private void btnModifySave_Click(object sender, ControlEventArgs e)
		{
			try
			{
				Coordinates coords, exitCoords;
				if (mDetailsLoc == null)
				{
					Util.Error("You must select a location before modifying it");
					return;
				}
				if (mDetailsLoc.IsInternalLocation)
				{
					Util.Error("This location cannot be modified");
					return;
				}
				if (!Coordinates.TryParse(edtModifyCoords.Text, out coords))
				{
					Util.Error("Invalid location coordinates: " + edtModifyCoords.Text);
					return;
				}
				if (!Coordinates.TryParse(edtModifyExitCoords.Text, true, out exitCoords))
				{
					Util.Error("Invalid exit coordinates: " + edtModifyExitCoords.Text);
					return;
				}
				if (exitCoords == Coordinates.NO_COORDINATES && chkModifyUseInRoute.Checked)
				{
					Util.Error("In order to be used in route finding, a location must have exit coordinates");
					return;
				}
				mDetailsLoc.Name = edtModifyName.Text;
				mDetailsLoc.Coords = coords;
				mDetailsLoc.ExitCoords = exitCoords;
				mDetailsLoc.Type = (LocationType)choModifyType.Data[choModifyType.Selected];
				mDetailsLoc.UseInRouteFinding = chkModifyUseInRoute.Checked;
				mDetailsLoc.Notes = edtModifyDescription.Text;
				mDetailsLoc.IsCustomized = true;
				mLocDb.Save(Util.FullPath("locations.xml"));
				Util.Message("The location " + mDetailsLoc.Name + " has been successfully modified");
				if (mDetailsLoc.Equals(choRouteFrom.Data[choRouteFrom.Selected]))
				{
					choRouteFrom.Text[choRouteFrom.Selected] = mDetailsLoc.Name;
					chkRouteFromHere.Checked = false;
					edtRouteFromCoords.Text = mDetailsLoc.Coords.ToString();
				}
				if (mDetailsLoc.Equals(choRouteTo.Data[choRouteTo.Selected]))
				{
					choRouteTo.Text[choRouteTo.Selected] = mDetailsLoc.Name;
					chkRouteToHere.Checked = false;
					edtRouteToCoords.Text = mDetailsLoc.Coords.ToString();
				}
				ShowDetails(mDetailsLoc);
				UpdateFavoritesList();
				UpdateListCoords(lstSearchResults, chkSearchShowRelative.Checked);
				UpdateListCoords(lstRoute, chkRouteShowRelative.Checked);
				UpdateListCoords(lstFavorites, chkFavoritesShowRelative.Checked);
				UpdateListCoords(lstRecent, chkRecentShowRelative.Checked);
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnModifySaveNew", "Click")]
		private void btnModifySaveNew_Click(object sender, ControlEventArgs e)
		{
			try
			{
				Coordinates coords, exitCoords;
				if (!Coordinates.TryParse(edtModifyCoords.Text, out coords))
				{
					Util.Error("Invalid location coordinates: " + edtModifyCoords.Text);
					return;
				}
				if (!Coordinates.TryParse(edtModifyExitCoords.Text, true, out exitCoords))
				{
					Util.Error("Invalid exit coordinates: " + edtModifyExitCoords.Text);
					return;
				}
				if (exitCoords == Coordinates.NO_COORDINATES && chkModifyUseInRoute.Checked)
				{
					Util.Error("In order to be used in route finding, a location must have exit coordinates");
					return;
				}
				Location newLocation = new Location(
					Location.GetNextCustomId(),
					edtModifyName.Text,
					(LocationType)choModifyType.Data[choModifyType.Selected],
					coords,
					edtModifyDescription.Text,
					exitCoords);
				newLocation.UseInRouteFinding = chkModifyUseInRoute.Checked;
				newLocation.IsCustomized = true;
				mLocDb.Add(newLocation);
				mLocDb.Save(Util.FullPath("locations.xml"));
				Util.Message("The new location " + newLocation.Name
					+ " has been added to the locations database");
				char firstChar = char.IsLetter(newLocation.Name[0]) ? char.ToUpper(newLocation.Name[0]) : '#';
				if (choRouteFromLtr.Text[choRouteFromLtr.Selected][0] == firstChar)
					choRouteFrom.Add(newLocation.Name, newLocation);
				if (choRouteToLtr.Text[choRouteToLtr.Selected][0] == firstChar)
					choRouteTo.Add(newLocation.Name, newLocation);
				ShowDetails(newLocation);
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnModifyReset", "Click")]
		private void btnModifyReset_Click(object sender, ControlEventArgs e)
		{
			try
			{
				if (mDetailsLoc == null)
					Util.Error("Select a location first");
				else
					ShowDetails(mDetailsLoc);
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		//
		// Atlas > Details > Delete... Tab
		//
		[ControlEvent("btnDeleteLocation", "Click")]
		private void btnDeleteLocation_Click(object sender, ControlEventArgs e)
		{
			try
			{
				if (mDetailsLoc == null)
				{
					Util.Error("You must select a location to delete it");
				}
				else if (!Util.IsControlDown())
				{
					Util.Warning("You must hold down the control key while clicking to delete a location");
				}
				else
				{
					if (mLocDb.Remove(mDetailsLoc))
					{
						mLocDb.Save(Util.FullPath("locations.xml"));
						Util.Message("The location " + mDetailsLoc.Name
							+ " has been successfully removed from the database");
						ShowDetails(null);
					}
					else
					{
						Util.Error("This location cannot be deleted");
					}
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}
		#endregion Atlas > Details Tab

		//
		#region Atlas > Route Tab
		//
		private void LoadRouteChoice(ChoiceWrapper choice, char firstLetter)
		{
			choice.Clear();
			choice.Add("<Custom>", Location.NO_LOCATION);
			foreach (Location loc in mLocDb.GetAlphaIndex(firstLetter))
			{
				choice.Add(loc.Name, loc);
			}
			choice.Selected = 0;
		}

		[ControlEvent("choRouteFromLtr", "Change")]
		private void choRouteFromLtr_Change(object sender, IndexChangeEventArgs e)
		{
			try
			{
				LoadRouteChoice(choRouteFrom, choRouteFromLtr.Text[e.Index][0]);
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("choRouteFrom", "Change")]
		private void choRouteFrom_Change(object sender, IndexChangeEventArgs e)
		{
			try
			{
				Location loc = choRouteFrom.Data[e.Index] as Location;
				if (loc != null && loc != Location.NO_LOCATION)
				{
					chkRouteFromHere.Checked = false;
					edtRouteFromCoords.Text = loc.Coords.ToString();
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("edtRouteFromCoords", "Change")]
		private void edtRouteFromCoords_Change(object sender, TextBoxChangeEventArgs e)
		{
			try
			{
				chkRouteFromHere.Checked = false;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("edtRouteFromCoords", "End")]
		private void edtRouteFromCoords_End(object sender, TextBoxEndEventArgs e)
		{
			try
			{
				choRouteFrom.Selected = 0;
				Coordinates coords;
				if (Coordinates.TryParse(edtRouteFromCoords.Text, out coords))
				{
					edtRouteFromCoords.Text = coords.ToString();
				}
				if (e.Success)
				{
					btnRouteGenerate_Click(null, null);
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkRouteFromHere", "Change")]
		private void chkRouteFromHere_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				if (e.Checked)
				{
					edtRouteFromCoords.Text = PlayerCoords.ToString();
					choRouteFrom.Selected = 0;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("choRouteToLtr", "Change")]
		private void choRouteToLtr_Change(object sender, IndexChangeEventArgs e)
		{
			try
			{
				LoadRouteChoice(choRouteTo, choRouteToLtr.Text[e.Index][0]);
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("choRouteTo", "Change")]
		private void choRouteTo_Change(object sender, IndexChangeEventArgs e)
		{
			try
			{
				Location loc = choRouteTo.Data[e.Index] as Location;
				if (loc != null && loc != Location.NO_LOCATION)
				{
					chkRouteToHere.Checked = false;
					edtRouteToCoords.Text = loc.Coords.ToString();
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("edtRouteToCoords", "Change")]
		private void edtRouteToCoords_Change(object sender, TextBoxChangeEventArgs e)
		{
			try
			{
				chkRouteToHere.Checked = false;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("edtRouteToCoords", "End")]
		private void edtRouteToCoords_End(object sender, TextBoxEndEventArgs e)
		{
			try
			{
				choRouteTo.Selected = 0;
				Coordinates coords;
				if (Coordinates.TryParse(edtRouteToCoords.Text, out coords))
				{
					chkRouteToHere.Checked = false;
					edtRouteToCoords.Text = coords.ToString();
				}
				if (e.Success)
				{
					btnRouteGenerate_Click(null, null);
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkRouteToHere", "Change")]
		private void chkRouteToHere_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				if (e.Checked)
				{
					edtRouteToCoords.Text = PlayerCoords.ToString();
					choRouteTo.Selected = 0;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnRouteGenerate", "Click")]
		private void btnRouteGenerate_Click(object sender, ControlEventArgs e)
		{
			try
			{
				Coordinates fromCoords, toCoords;
				Coordinates.TryParse(edtRouteFromCoords.Text, true, out fromCoords);
				if (!Coordinates.TryParse(edtRouteToCoords.Text, out toCoords))
				{
					Util.Error("Invalid end point coordinates");
					return;
				}

				RouteStart.StartPoint.Coords = fromCoords;

				mRouteStartLoc = choRouteFrom.Data[choRouteFrom.Selected] as Location;
				mRouteEndLoc = choRouteTo.Data[choRouteTo.Selected] as Location;

				if (mRouteStartLoc == null || mRouteStartLoc.Coords != fromCoords)
				{
					mRouteStartLoc = RouteStart.StartPoint.ToLocation(mLocDb);
				}
				if (mRouteEndLoc == null || mRouteEndLoc.Coords != toCoords)
				{
					mRouteEndLoc = new Location(Location.GetNextInternalId(), "End Point",
						LocationType._EndPoint, toCoords, "");
				}

				if (fromCoords == Coordinates.NO_COORDINATES)
					mRouteStartLoc = Location.NO_LOCATION;

				mRoute = RouteFinder.FindRoute(mLocDb, mStartLocations.Values,
						mPortalDevices.Values, mRouteStartLoc, mRouteEndLoc, out mRoutePackage);

				if (mRoute.Count > 0)
				{
					lblRouteDistance.Text = (mRoute.Distance.ToString("0.00") + " km");
				}
				else
				{
					lblRouteDistance.Text = "No route found";
					Util.Warning("Unable to find a route.  Try turning on more start locations or "
						+ "increasing the maximum run distance between portals.");
				}
				mRouteCopyIndex = 0;
				FillLocationList(lstRoute, mRoute, chkRouteShowRelative.Checked);
				mMapHud.Route = mRoute;
				mMapHud.ShowRoute = true;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnRouteNext", "Click")]
		private void btnRouteNext_Click(object sender, ControlEventArgs e)
		{
			try
			{
				if (mRoutePackage == null)
				{
					btnRouteGenerate_Click(null, null);
					return;
				}
				Route route = RouteFinder.FindRoute(mRoutePackage);

				if (route.Count > 0)
				{
					lblRouteDistance.Text = (route.Distance.ToString("0.00") + " km");
					mRouteCopyIndex = 0;
					FillLocationList(lstRoute, route, chkRouteShowRelative.Checked);
					mRoute = route;
					mMapHud.Route = mRoute;
					mMapHud.ShowRoute = true;
				}
				else
				{
					Util.Warning("Can't find any more routes.");
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnRouteCopy", "Click")]
		private void btnRouteCopy_Click(object sender, ControlEventArgs e)
		{
			const int MAX_LENGTH = 225;

			try
			{
				if (lstRoute.RowCount == 0)
				{
					Util.Error("There is no route to copy");
				}
				else
				{
					if (mRouteCopyIndex >= lstRoute.RowCount)
						mRouteCopyIndex = 0;
					string route = "";
					for (; mRouteCopyIndex < lstRoute.RowCount; mRouteCopyIndex++)
					{
						Location loc;
						if (null != (loc = GetLocation(lstRoute[mRouteCopyIndex][LocationList.ID][0] as string)))
						{
							string routeSegment = (mRouteCopyIndex > 0 ? " >> " : "")
								+ loc.Name + " [" + loc.Coords.ToString("0.0") + "]";
							if (route.Length + routeSegment.Length > MAX_LENGTH)
							{
								route += " >>";
								Util.HelpMessage("The route was too long to fit into a tell, so only part "
									+ "of it was copied. Click Copy again to copy the next part.");
								break;
							}

							route += routeSegment;
						}
					}
					System.Windows.Forms.Clipboard.SetText(route);
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("lstRoute", "Selected")]
		private void lstRoute_Selected(object sender, ListSelectEventArgs e)
		{
			HandleLocationListClick(lstRoute, e, true);
		}

		[ControlEvent("chkRouteShowRelative", "Change")]
		private void chkRouteShowRelative_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				UpdateListCoords(lstRoute, e.Checked);
				//mRelativeCoordsTimer.Interval = e.Checked ? RelativeCoordsFast : RelativeCoordsSlow;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}
		#endregion Atlas > Route Tab

		//
		#region Atlas > Favorites Tab
		//
		[ControlEvent("lstFavorites", "Selected")]
		private void lstFavorites_Selected(object sender, ListSelectEventArgs e)
		{
			try
			{
				if (e.Column == LocationList.Icon)
				{
					if (Util.IsControlDown())
					{
						Location loc = GetLocation(lstFavorites[e.Row][LocationList.ID][0] as string);
						if (loc != null)
							loc.IsFavorite = false;
					}
				}
				else
				{
					HandleLocationListClick(lstFavorites, e, true);
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkFavoritesShowRelative", "Change")]
		private void chkFavoritesShowRelative_End(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				UpdateListCoords(lstFavorites, e.Checked);
				//mRelativeCoordsTimer.Interval = e.Checked ? RelativeCoordsFast : RelativeCoordsSlow;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}
		#endregion Atlas > Favorites Tab

		//
		#region Atlas > Recent Tab
		//
		[ControlEvent("lstRecent", "Selected")]
		private void lstRecent_Selected(object sender, ListSelectEventArgs e)
		{
			HandleLocationListClick(lstRecent, e, false);
		}

		[ControlEvent("lstRecentCoords", "Selected")]
		private void lstRecentCoords_Selected(object sender, ListSelectEventArgs e)
		{
			try
			{
				switch (e.Column)
				{
					case RecentCoordsList.Icon:
						lstRecentCoords.Delete(e.Row);
						break;
					case RecentCoordsList.Name:
					case RecentCoordsList.Coords:
						Coordinates coords;
						if (Coordinates.TryParse((string)lstRecentCoords[e.Row][RecentCoordsList.Coords][0], out coords))
						{
							if (Util.IsControlDown())
							{
								SetRouteEnd(coords);
							}
							else if (Util.IsShiftDown())
							{
								SetRouteStart(coords);
							}
							else
							{
								string name = (string)lstRecentCoords[e.Row][RecentCoordsList.Name][0];
								if (name == (string)lstRecentCoords[e.Row][RecentCoordsList.Coords][0])
									name = "";
								Location loc = new Location(Location.GetNextInternalId(), name, LocationType.Custom, coords, "");
								loc.Icon = (int)lstRecentCoords[e.Row][RecentCoordsList.Icon][1];
								mArrowHud.DestinationLocation = loc;
								mArrowHud.Visible = true;
							}
						}
						break;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkRecentShowRelative", "Change")]
		private void chkRecentShowRelative_Change(object sender, CheckBoxChangeEventArgs e)
		{
			try
			{
				UpdateListCoords(lstRecent, e.Checked);
				//mRelativeCoordsTimer.Interval = e.Checked ? RelativeCoordsFast : RelativeCoordsSlow;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}
		#endregion Atlas > Recent Tab

		//
		#region Atlas > Update Tab
		//
		[ControlEvent("choUpdateDatabaseType", "Change")]
		private void choUpdateDatabaseType_Change(object sender, IndexChangeEventArgs e)
		{
			try
			{
				if (mSettingsLoaded)
				{
					if (choUpdateDatabaseType.Selected == UpdateDatabaseType.ACSpedia)
					{
						edtLocationsUrl.Text = ACSpediaUrl;
					}
					else
					{
						edtLocationsUrl.Text = CrossroadsOfDerethUrl;
					}
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnLocationsUpdate", "Click")]
		private void btnLocationsUpdate_Click(object sender, ControlEventArgs e)
		{
			try
			{
				bool downloading = mDownloadClient.IsBusy;
				bool converting = mXmlConverterWorker.IsBusy;
				if (mDownloadClient.IsBusy || mXmlConverterWorker.IsBusy)
				{
					btnLocationsUpdate.Text = "Update Locations Database";
					if (mDownloadClient.IsBusy)
						mDownloadClient.CancelAsync();
					else if (mXmlConverterWorker.IsBusy)
						mXmlConverterWorker.CancelAsync();
					Util.Message("Update cancelled");
				}
				else
				{
					Uri requestUrl;
					if (!Uri.TryCreate(edtLocationsUrl.Text, UriKind.Absolute, out requestUrl))
					{
						Util.Error("Invalid download URL specified: " + edtLocationsUrl.Text);
						return;
					}

					FileInfo locationsFile;
					switch (choUpdateDatabaseType.Selected)
					{
						case UpdateDatabaseType.CrossroadsOfDereth:
							locationsFile = new FileInfo(codLocationsXmlPath);
							if (locationsFile.Exists && locationsFile.Length > 0)
							{
								mDownloadSizeEstimate = (int)(locationsFile.Length * 1.02); // 2% larger
							}
							else
							{
								mDownloadSizeEstimate = 3000 * 1024; // 3,000 KB
							}
							break;
						case UpdateDatabaseType.ACSpedia:
							locationsFile = new FileInfo(acsLocationsPath);
							if (locationsFile.Exists && locationsFile.Length > 0)
							{
								mDownloadSizeEstimate = (int)(locationsFile.Length * 1.02); // 2% larger
							}
							else
							{
								mDownloadSizeEstimate = 1400 * 1024; // 1,400 KB
							}
							break;
						default:
							Util.Error("Invalid database type selected: " + choUpdateDatabaseType.Selected);
							return;
					}

					mDownloadClient.DownloadFileAsync(requestUrl, locationsFile.FullName);

					txtDownloadStatusA.Text = "Connecting...";
					btnLocationsUpdate.Text = "Cancel";
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		private string bytesToString(long bytes)
		{
			if (bytes < 1000)
				return bytes + "b";
			if (bytes < 1024 * 1000)
				return (bytes / 1024.0).ToString("0.00") + "KB";
			if (bytes < 1024 * 1024 * 1000)
				return (bytes / (1024.0 * 1024.0)).ToString("0.00") + "MB";
			return (bytes / (1024.0 * 1024.0 * 1024.0)).ToString("0.00") + "GB";
		}

		private void downloadClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			try
			{
				Util.QueueAction(delegate()
				{
					long totalBytes;
					int pct;
					if (e.TotalBytesToReceive < 0)
					{
						totalBytes = mDownloadSizeEstimate;
						pct = (int)(e.BytesReceived * 100 / totalBytes);
					}
					else
					{
						totalBytes = e.TotalBytesToReceive;
						pct = e.ProgressPercentage;
					}
					txtDownloadStatusA.Text = "Downloading... " + pct + "%";
					txtDownloadStatusB.Text = bytesToString(e.BytesReceived) + "/" + bytesToString(totalBytes);
					prgLocationsProgress.Value = pct;
				});
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		private void downloadClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
		{
			try
			{
				txtDownloadStatusB.Text = "";
				if (e.Error != null)
				{
					btnLocationsUpdate.Text = "Update Locations Database";
					txtDownloadStatusA.Text = "Update failed!";
					Util.Error("Error downloading file [" + e.Error.GetType().Name + "]: " + e.Error.Message);
					Util.LogException(e.Error);
				}
				else if (e.Cancelled)
				{
					btnLocationsUpdate.Text = "Update Locations Database";
					txtDownloadStatusA.Text = "";
				}
				else
				{
					txtDownloadStatusA.Text = "";
					mXmlConverterWorker.RunWorkerAsync();
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
			finally
			{
				prgLocationsProgress.Value = 0;
			}
		}

		private void xmlConverterWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture =
				   new System.Globalization.CultureInfo("en-US", false);

			Util.QueueAction(delegate()
			{
				txtDownloadStatusA.Text = "Converting... 0%";
				prgLocationsProgress.Value = 0;
				txtDownloadStatusB.Text = "";
			});

			LocationDatabase dbNew;
			if (choUpdateDatabaseType.Selected == UpdateDatabaseType.CrossroadsOfDereth)
			{
				dbNew = new LocationDatabase(DatabaseType.CrossroadsOfDereth);
			}
			else
			{
				dbNew = new LocationDatabase(DatabaseType.ACSpedia);
			}

			if (!chkUpdateOverwrite.Checked && dbNew.DatabaseType == mLocDb.DatabaseType)
			{
				foreach (Location loc in mLocDb.Locations.Values)
				{
					if (loc.IsCustomized)
						dbNew.Add(loc);
				}
			}

			if (choUpdateDatabaseType.Selected == UpdateDatabaseType.CrossroadsOfDereth)
			{
				XmlDocument codXml = new XmlDocument();
				try
				{
					codXml.Load(codLocationsXmlPath);
				}
				catch (Exception ex)
				{
					txtDownloadStatusA.Text = "Update failed!";
					Util.HandleException(ex, "Failed to open downloaded locations file; make sure "
						+ "you have the right database URL and type selected", false);

					try { File.Delete(codLocationsXmlPath); }
					catch { /* Ignore */ }
					e.Cancel = true;
					return;
				}

				XmlNodeList codNodes = codXml.DocumentElement.GetElementsByTagName("location");
				double total = codNodes.Count;
				int lastPct = 0;
				for (int i = 0; i < codNodes.Count; i++)
				{
					if (mXmlConverterWorker.CancellationPending)
					{
						e.Cancel = true;
						return;
					}

					int curPct = (int)Math.Round(100 * i / total);
					if (curPct > lastPct)
					{
						Util.QueueAction(delegate()
						{
							txtDownloadStatusA.Text = "Converting... " + curPct + "%";
							prgLocationsProgress.Value = curPct;
						});
						lastPct = curPct;
					}

					Location oldLoc;
					Location newLoc = Location.FromXmlWarcry((XmlElement)codNodes[i]);
					if (!newLoc.IsRetired && !dbNew.Contains(newLoc.Id))
					{
						if (dbNew.DatabaseType == mLocDb.DatabaseType && mLocDb.TryGet(newLoc.Id, out oldLoc))
							newLoc.IsFavorite = oldLoc.IsFavorite;
						dbNew.Add(newLoc);
					}
				}
			}
			else
			{ // ACSpedia Database
				string[] locations;
				try
				{
					using (StreamReader reader = new StreamReader(acsLocationsPath))
					{
						locations = reader.ReadToEnd().Split(new string[] { "\r\n!", "\n!" }, StringSplitOptions.RemoveEmptyEntries);
					}
				}
				catch (Exception ex)
				{
					txtDownloadStatusA.Text = "Update failed!";
					Util.Error("Failed to open downloaded locations file ("
						+ ex.GetType().Name + ": " + ex.Message + ")");

					try { File.Delete(acsLocationsPath); }
					catch { /* Ignore */ }
					e.Cancel = true;
					return;
				}
				// locations[0] is the number of locations
				// locations[1] is the header info
				// locations[2..n] are the locations
				int ct;
				if (!int.TryParse(locations[0], out ct) || ct != locations.Length - 2)
				{
					txtDownloadStatusA.Text = "Update failed!";
					Util.Error("The ACSpedia database format was not in the expected format.");

					try { File.Delete(acsLocationsPath); }
					catch { /* Ignore */ }
					e.Cancel = true;
					return;
				}

				double total = locations.Length;
				int lastPct = 0;
				for (int i = 2; i < locations.Length; i++)
				{
					if (mXmlConverterWorker.CancellationPending)
					{
						e.Cancel = true;
						return;
					}

					int curPct = (int)Math.Round(100 * i / total);
					if (curPct > lastPct)
					{
						Util.QueueAction(delegate()
						{
							txtDownloadStatusA.Text = "Converting... " + curPct + "%";
							prgLocationsProgress.Value = curPct;
						});
						lastPct = curPct;
					}

					Location oldLoc, newLoc;
					if (Location.TryParseACSpedia(locations[i], out newLoc) && !newLoc.IsRetired && !dbNew.Contains(newLoc.Id))
					{
						if (dbNew.DatabaseType == mLocDb.DatabaseType && mLocDb.TryGet(newLoc.Id, out oldLoc))
							newLoc.IsFavorite = oldLoc.IsFavorite;
						dbNew.Add(newLoc);
					}
				}
			}

			txtDownloadStatusA.Text = "Integrating...";
			e.Result = dbNew;
		}

		private void xmlConverterWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				System.Threading.Thread cur = System.Threading.Thread.CurrentThread;
				System.Threading.Thread plug = Util.MainPluginThread;

				txtDownloadStatusB.Text = "";
				if (e.Error != null)
				{
					txtDownloadStatusA.Text = "Error: " + e.Error.Message;
					Util.Error("Error converting locations file [" + e.Error.GetType().Name + "]: "
						+ e.Error.Message);
					Util.HandleException(e.Error);
				}
				else if (e.Cancelled)
				{
				}
				else
				{
					// Integrate
					Util.QueueAction(delegate()
					{
						try
						{
							LocationDatabase dbNew = (LocationDatabase)e.Result;

							if (dbNew.DatabaseType != mLocDb.DatabaseType)
							{
								lstSearchResults.Clear();
								lstRoute.Clear();
								lstRecent.Clear();
							}

							mLocDb.FavoritesListChanged -= LocationDatabase_FavoritesListChanged;
							mLocDb.LocationAdded -= LocationDatabase_LocationCountChanged;
							mLocDb.LocationRemoved -= LocationDatabase_LocationCountChanged;
							dbNew.FavoritesListChanged += new EventHandler<LocationChangedEventArgs>(LocationDatabase_FavoritesListChanged);
							dbNew.LocationAdded += new EventHandler<LocationChangedEventArgs>(LocationDatabase_LocationCountChanged);
							dbNew.LocationRemoved += new EventHandler<LocationChangedEventArgs>(LocationDatabase_LocationCountChanged);

							mLocDb = dbNew;
							mLocDb.UpdatedNow();
							lblUpdateLastUpdate.Text = mLocDb.LastUpdateString;
							UpdateFavoritesList();
							FillLocationTypeChoice(choSearchLimit, true);
							FillLocationTypeChoice(choModifyType, false);
							ShowDetails(null);
							mLocDb.Save(Util.FullPath("locations.xml"));
							mMapHud.LocationDatabase = mLocDb;
							txtDownloadStatusA.Text = "Completed";
						}
						catch (Exception ex) { Util.HandleException(ex); }
						finally
						{
							prgLocationsProgress.Value = 0;
							btnLocationsUpdate.Text = "Update Locations Database";
						}
					});
				}
				LocationDatabase_LocationCountChanged(null, null);
			}
			catch (Exception ex) { Util.HandleException(ex); }
			finally
			{
				prgLocationsProgress.Value = 0;
				btnLocationsUpdate.Text = "Update Locations Database";
			}
		}

		private void LocationDatabase_LocationCountChanged(object sender, LocationChangedEventArgs e)
		{
			lblUpdateNumLocations.Text = mLocDb.Locations.Count.ToString("#,0");
		}
		#endregion Atlas > Update Tab
		#endregion Atlas Tab

		//
		#region Settings Tab
		//

		//
		#region Settings > Chat Tab
		//
		private void SetTargetWindow(ChatWindow window, bool enabled)
		{
			try
			{
				Util.SetDefaultWindow(window, enabled);
				if (Util.DefaultWindow == ChatWindow.Default)
				{
					Util.SetDefaultWindow(ChatWindow.MainChat, true);
					chkOutputs[0].Checked = true;
					if (window != ChatWindow.MainChat)
					{
						Util.Warning(Util.PluginName + " messages will no longer be shown in this window", window);
						Util.HelpMessage(Util.PluginName + " messages will now be shown in this window");
					}
				}
				else
				{
					if (enabled)
					{
						Util.HelpMessage(Util.PluginName + " messages will now be shown in this window", window);
					}
					else
					{
						Util.Warning(Util.PluginName + " messages will no longer be shown in this window", window);
					}
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("chkOutputMainChat", "Change")]
		private void chkOutputMainChat_Change(object sender, CheckBoxChangeEventArgs e)
		{
			SetTargetWindow(ChatWindow.MainChat, e.Checked);
		}

		[ControlEvent("chkOutput1", "Change")]
		private void chkOutput1_Change(object sender, CheckBoxChangeEventArgs e)
		{
			SetTargetWindow(ChatWindow.One, e.Checked);
		}

		[ControlEvent("chkOutput2", "Change")]
		private void chkOutput2_Change(object sender, CheckBoxChangeEventArgs e)
		{
			SetTargetWindow(ChatWindow.Two, e.Checked);
		}

		[ControlEvent("chkOutput3", "Change")]
		private void chkOutput3_Change(object sender, CheckBoxChangeEventArgs e)
		{
			SetTargetWindow(ChatWindow.Three, e.Checked);
		}

		[ControlEvent("chkOutput4", "Change")]
		private void chkOutput4_Change(object sender, CheckBoxChangeEventArgs e)
		{
			SetTargetWindow(ChatWindow.Four, e.Checked);
		}

		[ControlEvent("chkAlwaysShowErrors", "Change")]
		private void chkAlwaysShowErrors_End(object sender, CheckBoxChangeEventArgs e)
		{
			Util.WriteErrorsToMainChat = e.Checked;
		}

		[ControlEvent("edtChatCommand", "Change")]
		private void edtChatCommand_Change(object sender, TextBoxChangeEventArgs e)
		{
			MainChatCommandUpdated(e.Text);
		}

		[ControlEvent("edtChatCommand", "End")]
		private void edtChatCommand_End(object sender, TextBoxEndEventArgs e)
		{
			try
			{
				ValidateCommandName(edtChatCommand, true,
					"goarrow", "go", "go_arrow", "ga", "goarrow2");
				MainChatCommandUpdated(edtChatCommand.Text);
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		private void MainChatCommandUpdated(string cmd)
		{
			lblHelpInfo.Text = "Type '/" + cmd + " help' to see available chat commands";
			lblHelpInfo2.Text = "Type '/" + cmd + " help' to see available chat commands";
			chkEnableCoordsCommand.Text = "Enable '/" + cmd + " loc' command alias";
			chkEnableDestCommand.Text = "Enable '/" + cmd + " dest' command alias";
			chkEnableFindCommand.Text = "Enable '/" + cmd + " find' command alias";
		}

		[ControlEvent("btnCoordsCommandHelp", "Click")]
		private void btnCoordsCommandHelp_Click(object sender, ControlEventArgs e)
		{
			try
			{
				ShowLocCommandHelp(true);
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("edtCoordsCommand", "End")]
		private void edtCoordsCommand_End(object sender, TextBoxEndEventArgs e)
		{
			try
			{
				ValidateCommandName(edtCoordsCommand, false,
					"loc", "coords", "pos", "sendcoords", "sendpos");
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnDestCommandHelp", "Click")]
		private void btnDestCommandHelp_Click(object sender, ControlEventArgs e)
		{
			try
			{
				ShowLocCommandHelp(false);
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("edtDestCommand", "End")]
		private void edtDestCommand_End(object sender, TextBoxEndEventArgs e)
		{
			try
			{
				ValidateCommandName(edtDestCommand, false,
					"dest", "gdest", "arrowdest", "destination", "goarrowdest");
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("edtFindCommand", "End")]
		private void edtFindCommand_End(object sender, TextBoxEndEventArgs e)
		{
			try
			{
				ValidateCommandName(edtFindCommand, false,
					"find", "gfind", "search", "gofind", "goarrowfind");
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		private void ValidateCommandName(TextBoxWrapper check, bool isMainChatCommand,
				string primaryCommand, params string[] alternateCommands)
		{

			TextBoxWrapper[] commandEdits = { edtChatCommand, edtCoordsCommand, 
				edtDestCommand, edtFindCommand };

			bool ok;
			if (check.Text == "")
			{
				ok = false;
			}
			else
			{
				ok = true;
				foreach (TextBoxWrapper edt in commandEdits)
				{
					if (edt != check && edt.Text == check.Text)
					{
						if (isMainChatCommand)
						{
							Util.Error("The main chat command cannot be the same as another "
								+ "goarrow chat command.");
						}
						else
						{
							Util.Error("The '/" + edtChatCommand.Text + " " + primaryCommand
								+ "' command alias cannot be the same as another goarrow chat command.");
						}
						ok = false;
						break;
					}
				}
			}

			if (!ok)
			{
				List<string> possibleCommands = new List<string>(alternateCommands.Length + 1);
				possibleCommands.Add(primaryCommand);
				possibleCommands.AddRange(alternateCommands);

				foreach (string cmd in possibleCommands)
				{
					ok = true;
					foreach (TextBoxWrapper edt in commandEdits)
					{
						if (edt != check && edt.Text == cmd)
						{
							ok = false;
							break;
						}
					}
					if (ok)
					{
						check.Text = cmd;
						break;
					}
				}
			}
		}
		#endregion Settings > Chat Tab

		//
		#region Settings > Route Finding Tab
		//
		[ControlEvent("lstStartLocations", "Selected")]
		private void lstStartLocations_Selected(object sender, ListSelectEventArgs e)
		{
			try
			{
				RouteStart loc =
					mStartLocations[(string)lstStartLocations[e.Row][StartLocationsList.Name][0]];

				switch (e.Column)
				{
					case StartLocationsList.Icon:
						lstStartLocations[e.Row][StartLocationsList.Enabled][0] =
							!(bool)lstStartLocations[e.Row][StartLocationsList.Enabled][0];
						goto case StartLocationsList.Enabled;
					case StartLocationsList.Enabled:
						loc.Enabled = (bool)lstStartLocations[e.Row][StartLocationsList.Enabled][0];
						break;
					case StartLocationsList.Name:
					case StartLocationsList.Coords:
						edtStartLocationName.Text = loc.Name;
						edtStartLocationCoords.Text = loc.Coords.ToString();
						edtStartLocationRunDist.Text = loc.RunDistance.ToString();
						btnStartLocationAdd.Text = "Modify";
						break;
					case StartLocationsList.Delete:
						if (loc.Type == RouteStartType.Regular)
						{
							if (!Util.IsControlDown())
							{
								Util.Warning("You must hold down Ctrl while clicking to delete a start location");
							}
							else
							{
								mStartLocations.Remove(loc.Name);
								lstStartLocations.Delete(e.Row);
							}
						}
						break;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("edtStartLocationName", "Change")]
		private void edtStartLocationName_Change(object sender, TextBoxChangeEventArgs e)
		{
			try
			{
				if (mStartLocations.ContainsKey(e.Text))
					btnStartLocationAdd.Text = "Modify";
				else
					btnStartLocationAdd.Text = "Add";
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnStartLocationRunDistHelp", "Click")]
		private void btnStartLocationRunDistHelp_Click(object sender, ControlEventArgs e)
		{
			Util.HelpMessage("Some start locations (such as Abandoned Mines) require running "
				+ "through a dungeon.  The number you enter here will be added to the route "
				+ "distance when calculating a route from this start location.");
		}

		[ControlEvent("edtStartLocationCoords", "End")]
		private void edtStartLocationCoords_End(object sender, TextBoxEndEventArgs e)
		{
			try
			{
				Coordinates coords;
				if (Coordinates.TryParse(edtStartLocationCoords.Text, out coords))
				{
					edtStartLocationCoords.Text = coords.ToString();
					edtStartLocationCoords.Caret = 0;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnStartLocationHere", "Click")]
		private void btnStartLocationHere_Click(object sender, ControlEventArgs e)
		{
			try
			{
				edtStartLocationCoords.Text = PlayerCoords.ToString("0.0");
				edtStartLocationCoords.Caret = 0;
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("btnStartLocationAdd", "Click")]
		private void btnStartLocationAdd_Click(object sender, ControlEventArgs e)
		{
			try
			{
				string name = edtStartLocationName.Text;
				Coordinates coords;
				if (!Coordinates.TryParse(edtStartLocationCoords.Text, true, out coords))
				{
					Util.Error("Invalid coordinates");
					return;
				}
				double runDistance;
				if (!double.TryParse(edtStartLocationRunDist.Text, out runDistance)
						&& edtStartLocationRunDist.Text != "")
				{
					Util.Error("Invalid run distance");
					return;
				}
				RouteStart loc;
				ListRow row = null;
				int rowIndex = -1;
				if (mStartLocations.TryGetValue(name, out loc))
				{
					loc.Name = name;
					loc.RunDistance = runDistance;
					loc.Coords = coords;
					if (loc.Type == RouteStartType.MansionRecall)
					{
						PortalDevice.MansionRunDistance = loc.RunDistance;
					}

					for (int r = 0; r < lstStartLocations.RowCount; r++)
					{
						if (0 == StringComparer.OrdinalIgnoreCase.Compare(name,
								lstStartLocations[r][StartLocationsList.Name][0]))
						{
							rowIndex = r;
							row = lstStartLocations[r];
							break;
						}
					}
				}
				else
				{
					loc = new RouteStart(name, RouteStartType.Regular, Location.LocationTypeInfo(LocationType.Custom).Icon,
						runDistance, coords, SavesPer.All, true);
					mStartLocations[name] = loc;
				}
				if (row == null)
				{
					int idx = 0;
					foreach (string nameKey in mStartLocations.Keys)
					{
						if (0 == mStartLocations.Comparer.Compare(nameKey, name))
							break;
						idx++;
					}
					if (idx == lstStartLocations.RowCount)
						row = lstStartLocations.Add();
					else
						row = lstStartLocations.Insert(idx);
					rowIndex = idx;

					row[StartLocationsList.Icon][1] = loc.Icon;
					row[StartLocationsList.Delete][1] = DeleteIcon;
				}
				loc.Enabled = true;
				row[StartLocationsList.Enabled][0] = true;
				row[StartLocationsList.Name][0] = name;
				row[StartLocationsList.Coords][0] = coords.ToString(true);

				if (coords != Coordinates.NO_COORDINATES)
				{
					row[StartLocationsList.Name].Color = Color.White;
					row[StartLocationsList.Coords].Color = Color.White;
				}
				else
				{
					row[StartLocationsList.Name].Color = Color.Gray;
					row[StartLocationsList.Coords].Color = Color.Gray;
				}

				if (btnStartLocationAdd.Text == "Add")
				{
					lstStartLocations.ScrollPosition = rowIndex;
				}

				edtStartLocationName.Text = "";
				edtStartLocationCoords.Text = "";
				edtStartLocationRunDist.Text = "";
				btnStartLocationAdd.Text = "Add";
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("edtMaxRunDist", "End")]
		private void edtMaxRunDist_End(object sender, TextBoxEndEventArgs e)
		{
			try
			{
				double val;
				if (!double.TryParse(edtMaxRunDist.Text, out val) || val <= 0)
				{
					Util.Error("Invalid maximum run distance");
					edtMaxRunDist.Text = RouteFinder.MaxRunDistance.ToString();
				}
				else
				{
					RouteFinder.MaxRunDistance = val;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("edtPortalRunDist", "End")]
		private void edtPortalRunDist_End(object sender, TextBoxEndEventArgs e)
		{
			try
			{
				double val;
				if (!double.TryParse(edtPortalRunDist.Text, out val) || val <= 0)
				{
					Util.Error("Invalid portal run distance");
					edtPortalRunDist.Text = RouteFinder.PortalWeight.ToString();
				}
				else
				{
					RouteFinder.PortalWeight = val;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}

		[ControlEvent("lstPortalDevices", "Selected")]
		private void lstPortalDevices_Selected(object sender, ListSelectEventArgs e)
		{
			try
			{
				string name = (string)lstPortalDevices[e.Row][PortalDevicesList.Name][0];

				switch (e.Column)
				{
					case PortalDevicesList.Icon:
						lstPortalDevices[e.Row][PortalDevicesList.Enabled][0] =
							!(bool)lstPortalDevices[e.Row][PortalDevicesList.Enabled][0];
						goto case PortalDevicesList.Enabled;
					case PortalDevicesList.Enabled:
						mPortalDevices[name].Enabled = (bool)lstPortalDevices[e.Row][PortalDevicesList.Enabled][0];
						break;
					case PortalDevicesList.Name:
					case PortalDevicesList.Detected:
						ShowDetails(mPortalDevices[name].InfoLocation);
						break;
				}
			}
			catch (Exception ex) { Util.HandleException(ex); }
		}
		#endregion Settings > Route Finding Tab
		#endregion Settings Tab
		#endregion Control Events
	}
}
