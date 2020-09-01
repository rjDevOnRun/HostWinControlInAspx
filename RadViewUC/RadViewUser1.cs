//using AutoRouting.SPPID.Resources;
using AutoRouting.SPPID.DataAccess;
using AutoRouting.SPPID.Factory;
using AutoRouting.SPPID.Globals;
using AutoRouting.SPPID.Interfaces;
using AutoRouting.SPPID.Repository;
using AutoRouting.SPPID.ViewModels;
using AutoRouting.SPPID.Views;
using AxSPPIDRADViewCtl;
using SPPIDRADViewCtl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoRouting.SPPID
{
	public partial class RADViewerUC : UserControl, IWPFBase //, IDisposable
	{
		/* GENERAL NOTES:

			Buttons: 1=Left, 2=Right, 3=??, 4=Middle
						'e.shift == 1'[shift-key is pressed]
		*/

		#region Delegates and Events

		/*	- Declare a delegate
			- declare event based on the delagate
			- init the event (in constuctor or as appropriate location)
		*/

		protected internal delegate void RAD_PrePrint_Delegate();
		protected internal event RAD_PrePrint_Delegate RAD_PrePrint_Event;

		protected internal delegate void RAD_PostPrint_Delegate();
		protected internal event RAD_PostPrint_Delegate RAD_PostPrint_Event;

		protected internal delegate void RAD_ReadStateChanged_Delegate();
		protected internal event RAD_ReadStateChanged_Delegate RAD_ReadyStateChanged_Event;

		protected internal delegate void RAD_CommandComplete_Delegate();
		protected internal event RAD_CommandComplete_Delegate RAD_CommandComplete_Event;

		protected internal delegate void RAD_Validating_Delegate();
		protected internal event RAD_Validating_Delegate RAD_Validating_Event;

		#endregion

		#region Backing Fields

		public event PropertyChangedEventHandler PropertyChanged;

		double newX;
		double newY;
		double currentX;
		double currentY;
		const double c_Factor = 8.0;
		const double c_FactorZero = 0.0;
		const double c_NegativeFactor = -8.0;
		const CoordinateSpaceConstants RADSpaceCoordSystem =
					CoordinateSpaceConstants.CoordinateSpacePixel;
		private object _defaultBackColor;
		private string _drawingFileName;
		private AxSPPIDRADViewCtl.AxRADViewCtl412 _radOCX;
		private string rADItemInfo = string.Empty;

		double locateRadius = 5;

		private IRadAttributes rADAttributes = ModelFactory.GetRadAttributes();

		#endregion

		#region Properties and Initializers

		//enum RADBackColors
		//{
		//    Black = 0,
		//    White = 16777215,
		//    Blue = 16711680,
		//    Cyan = 16776960,
		//    Green = 65280,
		//    Magenta = 16711935,
		//    Yellow = 65535
		//}
		private readonly object dataLock = new object();

		public string DrawingFileName
		{
			get { return _drawingFileName; }
			set
			{
				_drawingFileName = value;
				if (this.axRADViewCtl4121.IsDisposed) return;

				if (string.IsNullOrEmpty(value))
				{
					this.axRADViewCtl4121.Display = false;
					this.axRADViewCtl4121.Visible = false;
				}
				else
				{
					try
					{
						if (!string.IsNullOrEmpty(this.axRADViewCtl4121.FileName) &&
										!this.axRADViewCtl4121.FileName.Equals(_drawingFileName))
						{
							this.axRADViewCtl4121.FileName = _drawingFileName;
							this.axRADViewCtl4121.Visible = true;
							this.axRADViewCtl4121.Display = true;
							this.axRADViewCtl4121.Fit();
						}
						else if (string.IsNullOrEmpty(this.axRADViewCtl4121.FileName))
						{
							this.axRADViewCtl4121.FileName = _drawingFileName;
							this.axRADViewCtl4121.Visible = true;
							this.axRADViewCtl4121.Display = true;
							this.axRADViewCtl4121.Fit();
						}
					}
					catch (Exception ex)
					{
						Debug.Print(ex.Message + ex.StackTrace);
					}
				}
			}
		}

		public AxRADViewCtl412 RadOCX
		{
			get { return _radOCX; }
			set { _radOCX = value; }
		}

		public TopoTestUIView TopoTestUIView = null;

		private MergePSNViewModel MergePSNViewModel = null;

		private PSNEditViewModel PSNEditViewModel = null;

		private ARSMainViewModel ARSMainViewModel = null;

		private RADPrintSavePDFViewModel PrintSavePDFViewModel = null;

		public bool DrillDownLocate { get; set; } = false;

		public string RADItemInfo
		{
			get => rADItemInfo;
			set { rADItemInfo = value; NotifyChangedValues(nameof(RADItemInfo)); }
		}

		private int _backgroundColor = Constants.RADViewBackgroundColor;

		public int BackGroundColor
		{
			get { return _backgroundColor; }
			set
			{
				_backgroundColor = value;

				if (this.RadOCX.Display == true && !string.IsNullOrEmpty(this.RadOCX.FileName))
					this.RadOCX.SetBackgroundColor(_backgroundColor);
			}
		}

		public System.Windows.Controls.TextBlock oViewInfoText { get; set; } = null;

		public object GraphicOIDs { get; set; } = new List<string>();

		public List<IGraphicInfo> GraphicInfo = ModelFactory.GetGraphicInfos();

		public IRadAttributes RADAttributes
		{
			get => rADAttributes;
			set
			{
				lock (dataLock)
				{
					rADAttributes = value; NotifyChangedValues(nameof(RADAttributes));
				}
			}
		}

		private LocateNode CurrentLocatedNode { get; set; } = null;
		private List<IRadAttribute> PIDAttributes { get; set; } = new List<IRadAttribute>();
				
		private string selectedItemInfo = string.Empty;
		public string SelectedItemInfo 
		{ 
			get => selectedItemInfo;
			set 
			{ 
				selectedItemInfo = value;

				if (this.ARSMainViewModel != null)
				{
					this.ARSMainViewModel.SelectedItemInfo = selectedItemInfo;
				}
				NotifyChangedValues(nameof(selectedItemInfo)); 
			}
		}

		#endregion

		#region Constructor

		public RADViewerUC()
		{
			InitializeComponent();

			SetupRADView();
		}

		public RADViewerUC(MergePSNViewModel viewModel)
		{
			InitializeComponent();

			SetupRADView();

			this.MergePSNViewModel = viewModel;
		}

		public RADViewerUC(PSNEditViewModel viewModel)
		{
			InitializeComponent();

			SetupRADView();

			this.PSNEditViewModel = viewModel;
		}

		public RADViewerUC(ARSMainViewModel viewModel)
		{
			InitializeComponent();

			SetupRADView();

			this.ARSMainViewModel = viewModel;
		}

		public RADViewerUC(RADPrintSavePDFViewModel viewModel)
		{
			InitializeComponent();

			//if (this.RAD_PrePrint_Event != null) RAD_PrePrint_Event();

			SetupRADView();

			this.PrintSavePDFViewModel = viewModel;
		}

		protected internal void SetupRADView()
		{
			this.RadOCX = null;

			this.axRADViewCtl4121.SetLocalServer("RADSrv.dll");
			this.axRADViewCtl4121.CtlCursor = CursorTypeConstants.CursorLocate;
			this.DrillDownLocate = this.axRADViewCtl4121.DrillDownLocate;
			this.axRADViewCtl4121.DrillDownLocate = true;

			this.RadOCX = this.axRADViewCtl4121;
			axRADViewCtl4121.GetBackgroundColor(ref _defaultBackColor);

			// Subscribe to the RAD Events
			this.RadOCX.CommandComplete += RadOCX_CommandComplete;
			this.RadOCX.ReadyStateChange += RadOCX_ReadyStateChange;
			this.RadOCX.PrePrint += RadOCX_PrePrint;
			this.RadOCX.PostPrint += RadOCX_PostPrint;
			this.RadOCX.Validating += RadOCX_Validating;
		}

		private void RadOCX_Validating(object sender, CancelEventArgs e)
		{
			//// Init events
			//if (this.RAD_Validating_Event != null) RAD_Validating_Event();
			//// This checks/allows for Cancel event....
			//Debug.Print($"Validating event fired");
		}

		#region Event_Methods

		public void RadOCX_PrePrint(object sender, _DRadViewerEvents_PrePrintEvent e)
		{
			// Init events
			if (this.RAD_PrePrint_Event != null) RAD_PrePrint_Event();
			Debug.Print($"Pre-Print event fired");
		}

		public void RadOCX_PostPrint(object sender, _DRadViewerEvents_PostPrintEvent e)
		{
			// Init events
			if (this.RAD_PostPrint_Event != null) RAD_PostPrint_Event();
			Debug.Print($"Post-Print event fired");
		}

		public void RadOCX_ReadyStateChange(object sender, EventArgs e)
		{
			// Init events
			if (this.RAD_ReadyStateChanged_Event != null) RAD_ReadyStateChanged_Event();
			Debug.Print($"{e.ToString()}: ready state changed~");
		}

		public void RadOCX_CommandComplete(object sender, _DRadViewerEvents_CommandCompleteEvent e)
		{
			// Init events
			if (this.RAD_CommandComplete_Event != null) RAD_CommandComplete_Event();
			Debug.Print($"{e.command.ToString()}: Commmand completed");
		}

		#endregion

		#endregion

		#region RadView Events

		private async void AxRADViewCtl4121_MouseUpEvent(object sender, _DRadViewerEvents_MouseUpEvent e)
		{
			bool bItemFound = false;
			object locateNode = null;
			LocateNode locatedItem = null;
			this.SelectedItemInfo = string.Empty;

			try
			{
				// Get the item that was clicked upon by the mouse event
				bItemFound = this.axRADViewCtl4121.Locate(CoordinateSpaceConstants.CoordinateSpacePixel,
															Convert.ToDouble(e.x), Convert.ToDouble(e.y),
															locateRadius, ref locateNode);
				// Init new Set
				this.rADAttributes = ModelFactory.GetRadAttributes();

				// Cleanup if none
				if (!bItemFound || locateNode == null)
				{
					if (this.TopoTestUIView != null)
						this.TopoTestUIView.RadItemInfo = string.Empty;

					if (this.oViewInfoText != null)
						this.oViewInfoText.Text = string.Empty;

					if (this.PSNEditViewModel != null)
						this.PSNEditViewModel.RadAttributes = ModelFactory.GetRadAttributes();

					if (this.ARSMainViewModel != null)
						this.ARSMainViewModel.RadAttributes = ModelFactory.GetRadAttributes();

					this.SelectedItemInfo = string.Empty;
				
					CleanupRAD(locatedItem);
					return;
				}

				locatedItem = (LocateNode)locateNode as LocateNode;
				if (locatedItem == null)
				{
					this.SelectedItemInfo = string.Empty;
					CleanupRAD(locatedItem);
					return;
				}

				if (!e.button.Equals(2))
				{
					HighlightMouseOverItem(ref locateNode);
					//DisplayLocatedItemInfoToDebugWindow(locatedItem.Item[1]);
				}

				this.CurrentLocatedNode = locatedItem.Item[1];

				////* NOTE: Use Index start from '1' rather than '0' */
				this.RADItemInfo = DisplayAttributesOfThisItem(locatedItem.Item[1]);
				//this.RADItemInfo = await Task.Run(() => DisplayAttributesOfThisItem(locatedItem.Item[1]));

				// Display Context Menu on Right-Click
				if (e.button.Equals(2)) // Right Mouse Button
				{
					if (this.RADAttributes != null)
					{
						// Display Context menu only for 'PipeRun' item
						if (this.RADAttributes.Attributes.Find(x => x.Value.Contains(Constants.c_PipeRun_Item)) != null)
						{
							DisplayContextMenu(e);
						}
					}
				}
			}
			finally
			{
				AutoRouting.SPPID.Globals.AssemblyHelpers.ReleaseAllCOMObjects(locatedItem, locateNode);
			}
		}

		private void DisplayLocatedItemInfoToDebugWindow(LocateNode locatedItem)
		{
			Debug.Print("-------------------------------------------------------------------");
			Debug.Print($"ID: {locatedItem.ID.ToString()}");
			Debug.Print($"OwnerID: {locatedItem.OwnerID.ToString()}");
			Debug.Print($"RunTimeID: {locatedItem.RunTimeID.ToString()}");
			Debug.Print($"RunTimeOwnerID: {locatedItem.RunTimeOwnerID.ToString()}");
			Debug.Print($"Type: {locatedItem.Type.ToString()}");
			Debug.Print("-------------------------------------------------------------------");
		}

		private void CleanupRAD(LocateNode locatedItem)
		{
			DestroyMouseOverHighlightBuffer();
			CurrentLocatedNode = null;
			AutoRouting.SPPID.Globals.AssemblyHelpers.ReleaseAllCOMObjects(locatedItem);
		}

		private void DisplayContextMenu(_DRadViewerEvents_MouseUpEvent e)
		{
			// set the menu location
			this.RadOCX.ContextMenuStrip.Left = this.RadOCX.Left - e.x;
			this.RadOCX.ContextMenuStrip.Top = this.RadOCX.Left - e.y;

			// Display context menu
			this.RadOCX.ContextMenuStrip.Show(Cursor.Position);
		}

		private void axRADViewCtl4121_DblClick(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(this.axRADViewCtl4121.FileName) && axRADViewCtl4121.Visible.Equals(true))
			{
				this.axRADViewCtl4121.Fit();
			}
		}

		private void axRADViewCtl4121_MouseScroll(object sender, _DRadViewerEvents_MouseScrollEvent e)
		{
			// Validate
			if (axRADViewCtl4121.Visible.Equals(false)) return;

			// Make the current mouse x,y coords the zoom center point...

			// Zoom-In
			if (e.delta < 0)
			{
				this.axRADViewCtl4121.CtlCursor = CursorTypeConstants.CursorZoomIn;
				this.axRADViewCtl4121.Zoom(RADSpaceCoordSystem, e.x, e.y, 0.9);
			}
			// Zoom-Out
			else
			{
				this.axRADViewCtl4121.CtlCursor = CursorTypeConstants.CursorZoomOut;
				this.axRADViewCtl4121.Zoom(RADSpaceCoordSystem, e.x, e.y, 1.1);
			}
			this.axRADViewCtl4121.CtlCursor = CursorTypeConstants.CursorLocate;
		}

		private void axRADViewCtl4121_MouseMoveEvent(object sender, _DRadViewerEvents_MouseMoveEvent e)
		{
			if (axRADViewCtl4121.Visible.Equals(false))
				return;

			// View Panning
			newX = Convert.ToDouble(e.x);
			newY = Convert.ToDouble(e.y);

			// Check Mouse-Middle button clicked then do View-Pan...
			if (e.button == 4)
			{
				this.axRADViewCtl4121.CtlCursor = CursorTypeConstants.CursorPan;
				if (currentX < newX)
				{
					this.axRADViewCtl4121.Pan(RADSpaceCoordSystem, c_Factor, c_FactorZero);
				}
				else if (currentX > newX)
				{
					this.axRADViewCtl4121.Pan(RADSpaceCoordSystem, c_NegativeFactor, c_FactorZero);
				}
				else if (currentY < newY)
				{
					this.axRADViewCtl4121.Pan(RADSpaceCoordSystem, c_FactorZero, c_Factor);
				}
				else if (currentY > newY)
				{
					this.axRADViewCtl4121.Pan(RADSpaceCoordSystem, c_FactorZero, c_NegativeFactor);
				}
			}

			this.axRADViewCtl4121.CtlCursor = CursorTypeConstants.CursorLocate;

			currentX = Convert.ToDouble(e.x);
			currentY = Convert.ToDouble(e.y);
		}

		#endregion

		#region Methods and Functions

		/// <summary>
		/// Change thickness of Select Set Items:  
		/// Thickness in double,  
		/// Graphic OIDs as int[].
		/// </summary>
		/// <param name="thickness"></param>
		/// <param name="graphicOIDs"></param>
		public void ApplyThicknessToSelectionSet(double thickness, int[] graphicOIDs)
		{
			try
			{
				if (!this.RadOCX.Display || string.IsNullOrEmpty(this.RadOCX.FileName)) return;

				this.RadOCX.Refresh();

				// Reset existing Style Orverrides
				this.RadOCX.ResetObjectStyleOverride();

				// Assign Style details
				//object sReturnTypes = (object)StyleOverrideDataValidTypes.STYLE_DATA_ALL_VALID;
				object sColor = (object)ColorTypeConstants.Magenta;
				object sUnits = (object)StyleUnitsTypes.ViewStyleUnits;
				object sWidth = (object)Convert.ToDouble(thickness);
				object gOIDs = (object)graphicOIDs as object;

				// Apply Style
				this.RadOCX.SetObjectStyleOverride(ref gOIDs, ref sColor, ref sUnits, ref sWidth);

				// Notify
				this.RadOCX.MakeDirty();

			}
			catch (Exception ex)
			{
				Debug.Print(ex.Message + ex.InnerException + ex.StackTrace);
			}
		}

		private void HighlightMouseOverItem(ref object locateNode)
		{
			try
			{
				if (locateNode != null)
				{
					LocateNode locatedNode = (LocateNode)locateNode as LocateNode;

					if (locatedNode != null)
					{
						CreateMouseOverHighlightBuffer();

						AddItemToRADHighlighter(locatedNode.Item[1].RunTimeID.ToString());
					}
				}
				else
				{
					if (this.TopoTestUIView != null)
						this.TopoTestUIView.RadItemInfo = string.Empty;

					if (this.oViewInfoText != null)
						this.oViewInfoText.Text = string.Empty;

					DestroyMouseOverHighlightBuffer();
				}
			}
			finally
			{ /*AutoRouting.SPPID.Globals.AssemblyHelpers.ReleaseAllCOMObjects(locateNode);*/ }
		}

		public void SetObjectDisplayStyles(object objectIDs)
		{
			try
			{
				this.RadOCX.ResetObjectStyleOverride();
				//object sReturnTypes = (object)StyleOverrideDataValidTypes.STYLE_DATA_ALL_VALID;
				object sColor = (object)ColorTypeConstants.Magenta;
				object sUnits = (object)StyleUnitsTypes.ViewStyleUnits;
				object sWidth = (object)3.0;

				//object grpOIDOfInts = (object)graphOIDs.Select<string, int>(q => Convert.ToInt32(q)).ToArray();
				object gID = (object)Convert.ToInt32(objectIDs.ToString());

				this.RadOCX.SetObjectStyleOverride(ref gID, ref sColor, ref sUnits, ref sWidth);

			}
			catch (Exception ex)
			{
				Debug.Print(ex.Message + ex.InnerException + ex.StackTrace);
			}
		}

		public void SavePIDAsPDFFile(string PsnOID, string dwgFileName)
		{
			/*	Saves the current sheet to pdf
				Does not have any impact about the object styles
			*/

			try
			{
				//this.RadOCX.ResetObjectStyleOverride();
				this.RadOCX.SetLayerVisibility("ConsistencyChecks", LayerVisibilityConstants.LayerNotVisible);

				string outputPDFFile = "D:\\Temp\\TestSaveAsPDF.pdf";
				bool bookMarks = false;
				PDFResolutionConstants resolutionConstants = PDFResolutionConstants.igPDFResolution600DPI;
				PDFColorModeConstants modeConstants = PDFColorModeConstants.igPDFColorModePureBlackAndWhite;
				//PDFColorModeConstants modeConstants = PDFColorModeConstants.igPDFColorModeColor;
				PDFJpegCompressionConstants compressionConstants = PDFJpegCompressionConstants.igPDFJpegGoodQualityGoodCompression;
				//long sheetArraySize = Convert.ToInt64(sheetCount);
				object[] sheetArray = new string[1] { Constants.c_RAD_Default_SheetName };

				DisplayAttributeValueToUser("Saving as PDF Now.. Please Wait!");

				this.RadOCX.SaveAsPDF(
					outputPDFFile,
					bookMarks,
					resolutionConstants,
					modeConstants,
					compressionConstants,
					1,
					sheetArray);

				DisplayAttributeValueToUser("PDF Creation Completed!");

				this.RadOCX.SetLayerVisibility("ConsistencyChecks", LayerVisibilityConstants.LayerIsVisible);
			}
			catch (Exception ex)
			{
				Debug.Print(ex.Message + ex.StackTrace);
				DisplayAttributeValueToUser("PDF Creation: Error Happened!");
			}
		}

		public void GetRADSheetNames()
		{
			object sheetCount = null;
			object sheetNames = null;
			this.RadOCX.GetSheetNames(ref sheetCount, ref sheetNames);

			Array shts = (Array)sheetNames;

			foreach (var item in shts)
			{
				Debug.Print(item.ToString());
			}
		}

		public void GetViewNames()
		{
			/* Gives out view names as pid#+view_nbr */
			object count = null;
			object vn = null;
			this.RadOCX.GetViewNames(ref count, ref vn);
			if (vn == null) return;
			Array array = (Array)vn;
			foreach (var item in array)
			{ Debug.Print($"Layer: \t{item.ToString()}"); }
		}

		public void GetDocumentAttribs()
		{
			/* gives attributes related to drawing/pid and few other details */
			object docAttributes = null;

			this.RadOCX.GetDocumentAttributes(ref docAttributes);

			if (docAttributes != null)
			{
				AttributeSets attributeSets = (AttributeSets)docAttributes;
				if (attributeSets == null) return;

				for (int i = 1; i <= attributeSets.Count; i++)
				{
					AttributeSet attributeSet = attributeSets[i];
					//Debug.Print($"AtrSet-Name: \t{attributeSet.ToString()}");

					if (attributeSet == null) return;

					for (int iatr = 1; iatr <= attributeSet.Count; iatr++)
					{
						Debug.Print($"{attributeSet[iatr].Name}: {attributeSet[iatr].Value.ToString()}");
					}
				}
			}
		}

		public void GetGraphicDataSet(object objectID)
		{
			/*	- needs RuneTimeID of object
				- Displays graphic properties including
					any watched object gOID
					any relation gOID
			*/
			object graphicDataSet = null;

			this.RadOCX.GetGraphicData(objectID, ref graphicDataSet);

			if (graphicDataSet != null)
			{
				Debug.Print($"OjbID:\t {objectID.ToString()}");

				GraphicDataSet dataSet = (GraphicDataSet)graphicDataSet;
				if (dataSet == null) return;

				for (int i = 1; i <= dataSet.Count; i++)
				{
					GraphicData data = dataSet.Item[i];
					Debug.Print($"{data.Name}:\t{data.Value.ToString()}");
				}
			}
		}

		public void GetLayerDetails()
		{
			/* Gets Layers in the sheet*/

			object lCount = null;
			object layNames = null;
			SPPIDRADViewCtl.LayerOverrideConstants layerOverride = this.RadOCX.DisplayFilters;

			this.RadOCX.GetLayerNames(ref lCount, ref layNames);

			if (layNames != null)
			{
				int layerCount = Convert.ToInt32(lCount);
				Array array = (Array)layNames;

				foreach (var item in array)
				{
					Debug.Print($"Layer: \t{item.ToString()}");
				}
			}
		}

		private void TestRADMethods()
		{
			//GetLayerDetails();
			//object strPersistentID = null;
			//axRADViewCtl4121.ConvertRunTimeIDToPersistentID(locatedNode.Item[1].RunTimeID, ref strPersistentID);
			//object objID = strPersistentID;
			//SetObjectDisplayStyles(strPersistentID);

			//GetGraphicDataSet(locatedNode.Item[1].RunTimeID);
			//GetDocumentAttribs();
			//GetViewNames();
			//SavePIDAsPDFFile(locatedNode.Item[1].RunTimeID);
			//GetRADSheetNames();

			//object perID = (object)string.Empty;
			//var rii = locatedNode.Item[1].RunTimeID;

			//this.RadOCX.ConvertRunTimeIDToPersistentID(locatedNode.Item[1].RunTimeID, ref perID);
		}

		public List<string> ConvertGraphicOIDsToRunTimeOIDs(List<string> graphicOIDCollection)
		{
			// Validate
			if (this.RadOCX.Display == false) return null;

			List<string> ConvertedRunTimeIDCollection = new List<string>();
			object runTimeID = string.Empty;
			StringBuilder sb = new StringBuilder();

			try
			{
				// Loop each Graphic OID
				foreach (var graphicOID in graphicOIDCollection)
				{
					try
					{
						// Prepend Persistent text
						sb.Clear();
						sb.Append(Constants.c_PersistentIDPrefix).Append(graphicOID);

						// Convert Per to Run ID
						this.RadOCX.ConvertPersistentIDToRunTimeID(sb.ToString(), ref runTimeID);
						ConvertedRunTimeIDCollection.Add(runTimeID.ToString());

					}
					catch
					{ continue; }
				}
				return ConvertedRunTimeIDCollection;
			}
			catch (Exception ex)
			{
				Debug.Print(ex.Message + ex.StackTrace);
				Logger.Write(ex.Message + ex.StackTrace);
				return new List<string>();
			}
		}

		public void AddRunTimeIDsToSelectSet(List<string> RunTimeIDCollection)
		{
			// Validate
			if (this.RadOCX.Display == false) return;

			// Add runIDs to SelectSet
			foreach (object runID in RunTimeIDCollection)
			{
				try
				{
					this.RadOCX.AddToSelectSet(runID as object);
				}
				catch (Exception ex)
				{
					Debug.Print(ex.Message + ex.StackTrace);
					continue;
				}
			}
		}

		public void ZoomArea(List<string> runTimeIDs)
		{
			// Validate
			if (this.RadOCX.Display == false) return;

			// Collect OverAll Zoom Area
			IGraphicRange graphRange = this.GetZoomAreaRange(runTimeIDs);

			if (graphRange != null)
			{
				this.RadOCX.ZoomArea(SPPIDRADViewCtl.CoordinateSpaceConstants.CoordinateSpaceWorld,
											graphRange.lowX, graphRange.lowY, graphRange.highX, graphRange.highY);
			}
			else
			{
				this.RadOCX.Fit();
			}
		}

		private IGraphicRange GetZoomAreaRange(List<string> runTimeIDs)
		{
			IGraphicRange calculatedRange = null;

			List<IGraphicRange> graphicObjects = ModelFactory.GetGraphicRanges(); // new List<GraphicRange>();

			foreach (var id in runTimeIDs)
			{
				IGraphicRange grpRange = ModelFactory.GetGraphicRange(); // new GraphicRange();

				object dX1 = null; object dY1 = null;
				object dX2 = null; object dY2 = null;
				object currentObjID = id;

				this.RadOCX.GetRange(currentObjID, ref dX1, ref dY1, ref dX2, ref dY2);

				grpRange.lowX = Convert.ToDouble(dX1);
				grpRange.lowY = Convert.ToDouble(dY1);
				grpRange.highX = Convert.ToDouble(dX2);
				grpRange.highY = Convert.ToDouble(dY2);

				if ((grpRange.lowX != 0.0) ||
					(grpRange.lowY != 0.0) ||
					(grpRange.highX != 0.0) ||
					(grpRange.highY != 0.0))
				{
					graphicObjects.Add(grpRange);

					if (graphicObjects.Count > 0)
					{
						var topoSetRange = ModelFactory.GetGraphicRange();

						topoSetRange.lowX = graphicObjects.Min(x => x.lowX) - Constants.c_Zoom_Padding_Factor;
						topoSetRange.lowY = graphicObjects.Min(x => x.lowY) - Constants.c_Zoom_Padding_Factor;
						topoSetRange.highX = graphicObjects.Max(x => x.highX) + Constants.c_Zoom_Padding_Factor;
						topoSetRange.highY = graphicObjects.Max(x => x.highY) + Constants.c_Zoom_Padding_Factor;

						calculatedRange = topoSetRange;
					}
				}
			}

			return calculatedRange;
		}

		private string DisplayAttributesOfThisItem(LocateNode oItem)
		{
			/* NOTE: Use Index start from '1' rather than '0' */
			string radItemInfo = string.Empty;
			this.PIDAttributes = new List<IRadAttribute>();

			if (oItem == null)
			{
				if (this.TopoTestUIView != null)
					this.TopoTestUIView.RadItemInfo = string.Empty;

				if (this.oViewInfoText != null)
					this.oViewInfoText.Text = string.Empty;

				return string.Empty;
			}

			try
			{
				//// Validate
				radItemInfo = CollectRADAttributesOfSelectedItem(ref this.axRADViewCtl4121, ref oItem, ref this.rADAttributes);

				CollectAttributeValuesForUIDisplay(ref radItemInfo, ref this.rADAttributes);

				// Hardcoded attibute connections... need to find another way...

				// Populate PSNEdit Viewer
				if (this.PSNEditViewModel != null && this.rADAttributes != null)
				{
					lock (dataLock)
					{
						this.PSNEditViewModel.RADObjectInfo = radItemInfo.ToString();
						this.PSNEditViewModel.RadAttributes = this.RADAttributes;
					}
				}

				// Populate TestUI
				if (this.TopoTestUIView != null && !string.IsNullOrEmpty(radItemInfo))
				{
					lock (dataLock)
					{
						this.TopoTestUIView.Dispatcher.Invoke(new Action(() =>
						{
							this.TopoTestUIView.RadItemInfo = radItemInfo.ToString();
						}));
					}
				}

				// Populate ARS Main View
				if (this.ARSMainViewModel != null)
				{
					lock (dataLock)
					{
						this.ARSMainViewModel.RadAttributes = this.RADAttributes;
					}
				}

				if (this.oViewInfoText != null)
				{
					lock (dataLock)
					{
						this.oViewInfoText.Dispatcher.Invoke(new Action(() =>
						{
							this.oViewInfoText.Text = radItemInfo.ToString();
						}));
					}
				}
			}
			finally
			{ }

			return radItemInfo;
		}

		private static string CollectRADAttributesOfSelectedItem(ref AxRADViewCtl412 axRADViewCtl4121, ref LocateNode oItem,
			ref IRadAttributes radAttributes)
		{
			object attributeSetsObject = null;
			string radItemInfo = string.Empty;
			AttributeSets attributeSets = null;

			axRADViewCtl4121.GetAttributes(oItem.RunTimeID, ref attributeSetsObject);

			if (attributeSetsObject == null) // Child Item must collect atr of its parent....
				axRADViewCtl4121.GetAttributes(oItem.Item[1].RunTimeID, ref attributeSetsObject);

			try
			{
				// Validate
				if (attributeSetsObject == null) return string.Empty;

				attributeSets = (AttributeSets)attributeSetsObject as AttributeSets;

				// Validate
				if (attributeSets == null) return string.Empty;

				for (int i = 1; i <= attributeSets.Count; i++)
				{
					AttributeSet attributeSet = attributeSets[i];
					string AtrValues = string.Empty;

					for (int idx = 1; idx <= attributeSet.Count; idx++)
					{
						var radAttribute = ModelFactory.GetRadAttribute();
						radAttribute.Type = attributeSet.Name;
						radAttribute.Name = attributeSet.Item[idx].Name.ToString();
						radAttribute.Value = attributeSet.Item[idx].Value.ToString();
						radAttributes.Attributes.Add(radAttribute);

						AtrValues += "\t" + attributeSet.Item[idx].Name.ToString() + ": " + attributeSet.Item[idx].Value.ToString();

						if (attributeSet.Name.ToString().Equals(Constants.c_AttributeSet_PIDAttributes) &&
							attributeSet.Item[idx].Name.ToString().Equals(Constants.c_Attribute_ModelItemType))
						{
							radItemInfo += attributeSet.Item[idx].Value.ToString();
						}
						else if (attributeSet.Name.ToString().Equals(Constants.c_AttributeSet_PIDAttributes) &&
							attributeSet.Item[idx].Name.ToString().Equals(Constants.c_Attribute_ModelId))
						{
							radItemInfo += "\t" + attributeSet.Item[idx].Value.ToString();
						}
					}
					AutoRouting.SPPID.Globals.AssemblyHelpers.ReleaseAllCOMObjects(attributeSet);
				}
				AutoRouting.SPPID.Globals.AssemblyHelpers.ReleaseAllCOMObjects(attributeSets);
				return radItemInfo;
			}
			finally
			{
				AutoRouting.SPPID.Globals.AssemblyHelpers.ReleaseAllCOMObjects(attributeSetsObject, attributeSets);
			}
		}

		private void DisplayAttributeValueToUser(string message)
		{
			// Populate TestUI
			if (this.TopoTestUIView != null && !string.IsNullOrEmpty(message))
				this.TopoTestUIView.RadItemInfo = message.ToString();

			// Populate ARS Main View
			if (this.oViewInfoText != null)
			{
				this.oViewInfoText.Text = message.ToString();
			}

			// Populate PSNEdit Viewer
			if (this.PSNEditViewModel != null)
			{
				this.PSNEditViewModel.RADObjectInfo = message.ToString();
			}
		}

		private void CollectAttributeValuesForUIDisplay(ref string radItemInfo, ref IRadAttributes radAttributes)
		{
			var pidAtrs = radAttributes[Constants.c_AttributeSet_PIDAttributes]; // P&ID Attributes 
			var smartFrameAtrs = radAttributes[Constants.c_AttributeSet_SmartFrame]; // SmartFrame Attributes

			if (pidAtrs != null && pidAtrs.Count > 0)
			{
				CollectPIDAttributes(ref radItemInfo, ref radAttributes);
				this.PIDAttributes = pidAtrs;
			}
			else
				this.PIDAttributes = new List<IRadAttribute>();
		}

		private void DisplaySmartFrameAttributes(List<IRadAttribute> smartFrameAtrs, ref IRadAttributes radAttributes)
		{
			IRadAttribute prAttribute = ModelFactory.GetRadAttribute();

			for (int iCount = 0; iCount < smartFrameAtrs.Count; iCount++)
			{
				try
				{
					prAttribute = ModelFactory.GetRadAttribute();
					prAttribute.Name = smartFrameAtrs[iCount].Name.ToString();
					prAttribute.Value = smartFrameAtrs[iCount].Value.ToString();

					radAttributes.Attributes.Add(prAttribute);
				}
				catch (Exception ex)
				{
					Debug.Print($"{nameof(DisplaySmartFrameAttributes)}: {ex.Message + ex.StackTrace}");
					continue;
				}
			}
		}

		private void CollectPIDAttributes(ref string radItemInfo, ref IRadAttributes radAttributes)
		{
			var modelSPID = radAttributes[Constants.c_AttributeSet_PIDAttributes, Constants.c_Attribute_ModelId];
			var modelItemType = radAttributes[Constants.c_AttributeSet_PIDAttributes, Constants.c_Attribute_ModelItemType];
			var repSPID = radAttributes[Constants.c_AttributeSet_PIDAttributes, Constants.c_Attribute_DrawingID];

			string SPItemRealTagValue = CollectItemTagAndSymbolDetails(radAttributes);

			if (!string.IsNullOrEmpty(SPItemRealTagValue))
				radItemInfo = SPItemRealTagValue;

			if ((modelItemType != null && !string.IsNullOrEmpty(modelItemType.Value.ToString())) &&
				(modelSPID != null && !string.IsNullOrEmpty(modelSPID.Value.ToString())))
			{
				// Model Item Types
				var modelRadAttributes = DisplayModelItemDetails(modelItemType, modelSPID);

				if (modelRadAttributes != null && modelRadAttributes.Attributes.Count > 0)
					radAttributes = modelRadAttributes;
			}
			else if ((modelItemType == null || string.IsNullOrEmpty(modelItemType.Value.ToString())) &&
						(repSPID != null && !string.IsNullOrEmpty(repSPID.Value.ToString())))
			{
				// Handle Label Persist seperately
				var dataTable = SPPIDDataAccess.GetResultsAsDataTable(QueryStore.GetMinimumAttributesOfLabelPersist(repSPID.Value.ToString()));
				if (dataTable != null && dataTable.Rows.Count > 0)
				{
					radAttributes = ModelFactory.GetRadAttributes(); // Reset Attributes.

					CollectRadAttributesAndValuesIntoDataTable(radAttributes, ref dataTable);
				}
			}
		}

		private IRadAttributes DisplayModelItemDetails(IRadAttribute atrModelItemType, IRadAttribute atrModelItemID)
		{
			IRadAttributes radAttributes = ModelFactory.GetRadAttributes();
			DataTable dataTable = null;

			switch (atrModelItemType.Value.ToString())
			{
				case Constants.c_PipeRun_Item:
					{
						dataTable = SPPIDDataAccess.GetResultsAsDataTable(QueryStore.GetMinimumAttributesOfPipeRun(atrModelItemID.Value.ToString()));
					}
					break;
				case Constants.c_PipingComp_Item:
					{
						dataTable = SPPIDDataAccess.GetResultsAsDataTable(QueryStore.GetMimimumAttributesOfPipingComponent(atrModelItemID.Value.ToString()));
					}
					break;
				case Constants.c_Nozzle_Item:
					{
						dataTable = SPPIDDataAccess.GetResultsAsDataTable(QueryStore.GetMimimumAttributesOfNozzle(atrModelItemID.Value.ToString()));
					}
					break;
				case Constants.c_Instrument_Item:
					{
						dataTable = SPPIDDataAccess.GetResultsAsDataTable(QueryStore.GetMinimumAttributesOfInstrument(atrModelItemID.Value.ToString()));
					}
					break;
				case Constants.c_SignalRun_Item:
					{
						dataTable = SPPIDDataAccess.GetResultsAsDataTable(QueryStore.GetMimimumAttributesOfSignalRun(atrModelItemID.Value.ToString()));
					}
					break;
				case Constants.c_OPC_Item:
					{
						dataTable = SPPIDDataAccess.GetResultsAsDataTable(QueryStore.GetMinimumAttributesOfOPC(atrModelItemID.Value.ToString()));
					}
					break;
				case Constants.c_ItemNote_Item:
					{
						dataTable = SPPIDDataAccess.GetResultsAsDataTable(QueryStore.GetMinimumAttributesOfItemNote(atrModelItemID.Value.ToString()));
					}
					break;
				case Constants.c_Equipment_Item:
				case Constants.c_EquipmentOther_Item:
				case Constants.c_EquipComponent_Item:
				case Constants.c_Exchanger_Item:
				case Constants.c_Mechanical_Item:
				case Constants.c_Vessel_Item:
					{
						dataTable = SPPIDDataAccess.GetResultsAsDataTable(QueryStore.GetMimimumAttributesOfEquipmentClassItem(atrModelItemID.Value.ToString()));
					}
					break;
				default:
					break;
			}

			if (dataTable != null && dataTable.Rows.Count > 0) CollectRadAttributesAndValuesIntoDataTable(radAttributes, ref dataTable);

			return radAttributes;
		}

		private static void CollectRadAttributesAndValuesIntoDataTable(IRadAttributes radAttributes, ref DataTable dtItemAttributes)
		{
			IRadAttribute prAttribute = ModelFactory.GetRadAttribute();
			DataRow oRow = dtItemAttributes.Rows[0];

			for (int colNum = 0; colNum < dtItemAttributes.Columns.Count; colNum++)
			{
				try
				{
					prAttribute = ModelFactory.GetRadAttribute();
					prAttribute.Name = dtItemAttributes.Columns[colNum].ColumnName.ToString();
					prAttribute.Value = oRow[colNum].ToString();

					radAttributes.Attributes.Add(prAttribute);
				}
				catch (Exception ex)
				{
					Debug.Print($"{nameof(CollectRadAttributesAndValuesIntoDataTable)} [Row]{0} [Col]{colNum}: {ex.Message + ex.StackTrace}");
					continue;
				}
			}
		}

		private static string CollectItemTagAndSymbolDetails(IRadAttributes radAttributes)
		{
			string SPItemRealTagValue = string.Empty;
			try
			{
				if (radAttributes[Constants.c_AttributeSet_PIDAttributes, Constants.c_Attribute_ModelItemType].Value.Equals("Relationship"))
				{
					string relationshipID = radAttributes[Constants.c_AttributeSet_PIDAttributes, Constants.c_Attribute_ModelId].Value.ToString()
						.Split(new[] { "Relationship." }, StringSplitOptions.None)[1];
					if (!string.IsNullOrEmpty(relationshipID))
					{
						var relationshipValueString = SPPIDDataAccess.GetResultAsListFromDB(QueryStore.GetRelationshipDescriptionString(relationshipID));
						if (relationshipValueString == null || relationshipValueString.Count == 0) return string.Empty;

						IRadAttribute radAttribute = ModelFactory.GetRadAttribute();

						foreach (var relation in relationshipValueString)
						{
							radAttribute = ModelFactory.GetRadAttribute();

							// Add ItemTag Attribute
							radAttribute.Name = "Inconsistency";
							radAttribute.Value = relation;
							radAttributes.Attributes.Add(radAttribute);
						}
					}
				}
				else
				{
					SPItemRealTagValue = DisplayItemTagAndSymbolFileName(radAttributes, SPItemRealTagValue);
				}
			}
			catch
			{ }

			return SPItemRealTagValue;
		}

		private static string DisplayItemTagAndSymbolFileName(IRadAttributes radAttributes, string SPItemRealTagValue)
		{
			try
			{
				IRadAttribute modID = radAttributes[Constants.c_AttributeSet_PIDAttributes, Constants.c_Attribute_ModelId];
				// Get ItemTag and Symbol Names
				if (modID != null && !string.IsNullOrEmpty(modID.Value))
				{
					var iTag = SPPIDDataAccess.GetResultAsSingleStringFromDB(QueryStore.GetItemTagAndSymbolFileName(modID.Value));

					if (iTag != null && !string.IsNullOrEmpty(iTag))
					{
						IRadAttribute radAttribute = ModelFactory.GetRadAttribute();

						if (iTag[0] == ':')
						{
							iTag = "{ NULL } " + iTag;

							// Add ItemTag Attribute
							radAttribute.Name = "ItemTag";
							radAttribute.Value = "{ NULL }";
							radAttributes.Attributes.Add(radAttribute);
						}
						else
						{
							// Add ItemTag Attribute
							radAttribute.Name = "ItemTag";
							radAttribute.Value = iTag.Split(':')[0].ToString();
							radAttributes.Attributes.Add(radAttribute);
						}

						// Add SymbolFileName Attribute
						radAttribute = ModelFactory.GetRadAttribute();
						radAttribute.Name = "Symbol Name";
						radAttribute.Value = iTag.Split(':')[1].ToString();
						radAttributes.Attributes.Add(radAttribute);

						SPItemRealTagValue += radAttributes[Constants.c_AttributeSet_PIDAttributes, Constants.c_Attribute_ModelItemType].Value +
												": " + iTag;
					}
				}
			}
			catch (Exception ex)
			{
				Debug.Print($"{nameof(DisplayItemTagAndSymbolFileName)}: {ex.Message + ex.StackTrace}");
			}

			return SPItemRealTagValue;
		}

		private void CreateMouseOverHighlightBuffer()
		{
			//this.RadOCX.DestroyHighlightBuffer();
			this.RadOCX.CreateHighlightBuffer(SPPIDRADViewCtl.ColorTypeConstants.Green);
		}

		private void CreateSelectedItemsHighlightBuffer(ColorTypeConstants color)
		{
			this.RadOCX.DestroyHighlightBuffer();
			this.RadOCX.CreateHighlightBuffer(color);
		}

		private void AddItemsToRADHighlighter(List<string> runtimeIDs)
		{
			// Validate
			if (this.RadOCX.Display == false || runtimeIDs == null || runtimeIDs.Count == 0) return;

			// Add runIDs to SelectSet
			foreach (object runID in runtimeIDs)
			{
				try
				{
					this.RadOCX.AddToHighlightBuffer(runID as object);
				}
				catch (Exception ex)
				{
					Debug.Print(ex.Message + ex.StackTrace);
					continue;
				}
			}
			this.RadOCX.DisplayHighlightBuffer();
		}

		private void AddItemToRADHighlighter(object runTimeID)
		{
			if (this.RadOCX.Display == false) return;

			try
			{
				this.RadOCX.AddToHighlightBuffer(runTimeID);
				this.RadOCX.DisplayHighlightBuffer();
			}
			catch (Exception ex)
			{
				Debug.Print(ex.Message + ex.StackTrace);
			}
		}

		private void DestroyMouseOverHighlightBuffer()
		{
			this.RadOCX.DestroyHighlightBuffer();
		}

		private void GetTheParentSymbolOfThisItem(ref LocateNode oItem)
		{
			LocateNode inputItem = oItem;
			LocateNode currentItem = inputItem;

			bool bSymbolFound = false;

			do
			{
				currentItem = currentItem.Item[1];

				if (currentItem == null) break;
				else
				{
					for (int i = 1; i <= currentItem.Count; i++)
					{
						if (currentItem.Item[i].Type.Equals(ObjectTypeConstants.Symbol))
						{
							bSymbolFound = true; break;
						}
					}
				}

			} while (!bSymbolFound);

			if (currentItem == null)
			{
				currentItem = inputItem.Item[1];
				oItem = currentItem;
			}
		}

		public void NotifyChangedValues([CallerMemberName] string propName = null)
		{
			try
			{
				// Do the PropertyChanged actions
				PropertyChangedEventHandler handler = this.PropertyChanged;

				if (handler != null)
				{
					var e = new PropertyChangedEventArgs(propName);
					handler(this, e);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
			}
		}

		#endregion

		#region Dispose Methods

		//// Is using 'IDisposable' Interface
		//public void Dispose()
		//{
		//    Dispose(true);
		//    GC.SuppressFinalize(this);
		//}

		//protected virtual void Dispose(bool disposing)
		//{
		//    if (disposing)
		//    {
		//        //_oMainView.oUc.Child.Dispose();
		//        AutoRouting.SPPID.Resources.Helpers.ReleaseAllCOMObjects(this._radOCX);
		//    }
		//}
		#endregion

		private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
		{
			//AxRADViewCtl412 axRAD = (AxRADViewCtl412)sender;
		}

		private void runToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (CurrentLocatedNode == null) return;

				ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
				if (menuItem == null) return;

				var modelSPID = this.RADAttributes.GetAttribute(Constants.col_OID);
				var dwgSPID = this.PIDAttributes.Where(p => p.Name.Equals(Constants.c_Attribute_DrawingNo)).FirstOrDefault().Value.ToString();

				if ((modelSPID == null || string.IsNullOrEmpty(modelSPID.Value)) ||
					(dwgSPID == null || string.IsNullOrEmpty(dwgSPID))) return;

				var runIDs = new List<string>();
				List<string> runTimeIDs = new List<string>();
				string infoForDisplayToUI = string.Empty;

				switch (menuItem.Name)
				{
					case "Run": // locates item's GOID
						{
							CreateSelectedItemsHighlightBuffer(ColorTypeConstants.DarkMagenta);
							runTimeIDs.Add(CurrentLocatedNode.Item[1].RunTimeID);

							infoForDisplayToUI = $"Run: {modelSPID.Value.ToString()}";
						}
						break;
					case "Line": // PipeRuns with same SPID as selected item
						{
							CreateSelectedItemsHighlightBuffer(ColorTypeConstants.Green);
							runIDs = ContextMenuCollectRunTimeIDs(modelSPID.Value.ToString(), dwgSPID);
							if (runIDs != null && runIDs.Count > 0) runTimeIDs.AddRange(runIDs);

							infoForDisplayToUI = $"Line: {modelSPID.Value.ToString()}";
						}
						break;
					case "Topology":
						{
							CreateSelectedItemsHighlightBuffer(ColorTypeConstants.Cyan);
							var topologyIDs = string.Empty;

							var relatedPathItemOIDs = DataRepo.GetRelatedPathItemsOfThisPipeRunSPIDBasedOnTopologyOID(modelSPID.Value.ToString(), ref topologyIDs);
							if (relatedPathItemOIDs == null) return;

							foreach (var pathOID in relatedPathItemOIDs)
							{
								runIDs = ContextMenuCollectRunTimeIDs(pathOID, dwgSPID);
								if (runIDs != null && runIDs.Count > 0)
									runTimeIDs.AddRange(runIDs);
							}

							if (!string.IsNullOrEmpty(topologyIDs)) infoForDisplayToUI = $"TopologySet: {topologyIDs}";
						}
						break;
					case "PipeSystemNetwork":
						{
							CreateSelectedItemsHighlightBuffer(ColorTypeConstants.Yellow);
							var PsnIDs = string.Empty;

							var relatedPathItemOIDs = DataRepo.GetRelatedPathItemsOfThisPipeRunBasedOnPSN(modelSPID.Value.ToString(), ref PsnIDs);
							if (relatedPathItemOIDs == null) return;

							foreach (var pathOID in relatedPathItemOIDs)
							{
								runIDs = ContextMenuCollectRunTimeIDs(pathOID, dwgSPID);
								if (runIDs != null && runIDs.Count > 0)
									runTimeIDs.AddRange(runIDs);
							}

							if (!string.IsNullOrEmpty(PsnIDs)) infoForDisplayToUI = $"PSN: {PsnIDs}";
						}
						break;
					default:
						break;
				}

				this.SelectedItemInfo = infoForDisplayToUI;

				if (runTimeIDs.Count > 0) AddItemsToRADHighlighter(runTimeIDs);
			}
			catch (Exception ex)
			{
				Debug.Print(ex.Message + ex.StackTrace);
			}
		}

		private List<string> ContextMenuCollectRunTimeIDs(string ModelSPID, string DrawingNo)
		{
			var modelSPID = this.RADAttributes.GetAttribute(Constants.col_OID);
			if (modelSPID == null || string.IsNullOrEmpty(modelSPID.Value)) return null;

			DataTable dataTable = null;
			dataTable = SPPIDDataAccess.GetResultsAsDataTable(QueryStore.GetRADSelectedItemInformation(ModelSPID, DrawingNo));

			if (dataTable == null || dataTable.Rows.Count == 0) return null;

			List<string> runIDTimeIDs = new List<string>();
			for (int i = 0; i < dataTable.Rows.Count; i++)
			{
				runIDTimeIDs.Add(dataTable.Rows[i].ItemArray[0].ToString()); // ItemArray[0] == GRAPHICOID from query
			}

			if (runIDTimeIDs.Count > 0)
			{
				return ConvertGraphicOIDsToRunTimeOIDs(runIDTimeIDs);
			}
			else
				return null;

		}

	}
}
