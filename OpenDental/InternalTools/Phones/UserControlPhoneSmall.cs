﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDental {
	public partial class UserControlPhoneSmall:UserControl {
		private List<Phone> _listPhones;
		private List<PhoneEmpDefault> _listPhoneEmpDefaults;
		///<summary>When the GoToChanged event fires, this tells us which patnum.</summary>
		public long GotoPatNum;
		///<summary></summary>
		[Category("Property Changed"),Description("Event raised when user wants to go to a patient or related object.")]
		public event EventHandler GoToChanged=null;
		public int Extension;
		///<summary>A list of rooms for the phone map.</summary>
		private List<MapAreaContainer> _listRooms;
		///<summary>A list of all map areas.  Each map area represents a "tile" which is associated to a phone extension at HQ.</summary>
		private List<MapArea> _listMapAreas;
		private long _mapAreaContainerNum=0;
		
		public UserControlPhoneSmall() {
			InitializeComponent();
			phoneTile.GoToChanged += new System.EventHandler(this.phoneTile_GoToChanged);
			phoneTile.MenuNumbers=menuNumbers;
			phoneTile.MenuStatus=menuStatus;
			//_listMapAreas=MapAreas.Refresh();//PROBLEM: This gets called before databases are selected
		}

		///<summary>Set list of phones to display. Get/Set accessor won't work here because we require 2 seperate fields in order to update the control properly.</summary>
		public void SetPhoneList(List<PhoneEmpDefault> peds,List<Phone> phones) {
			//create a new list so our sorting doesn't affect this list elsewhere
			_listPhones=new List<Phone>();
			_listMapAreas=MapAreas.Refresh();
			if(_listRooms==null) {
				UpdateComboRooms();//Only call this once from here otherwise it gets called too often and causes strange flickering.
			}
			if(_mapAreaContainerNum < 1) {
				_listPhones.AddRange(phones);
			}
			else {//A specific room was selected so we only want to show those extensions.
				//Find all the map areas that correspond to the selected map area container.
				List<MapArea> listMapAreas=_listMapAreas.FindAll(x => x.MapAreaContainerNum==_mapAreaContainerNum);
				//Get all phones that correspond to the map areas found.
				_listPhones=phones.FindAll(x => listMapAreas.Exists(y => y.Extension==x.Extension));
			}
			//We always want to sort the list of phones so that they display in a predictable fashion.
			_listPhones.Sort(new Phones.PhoneComparer(Phones.PhoneComparer.SortBy.ext));
			_listPhoneEmpDefaults=peds;
			Invalidate();
		}

		private void UpdateComboRooms() {
			int selectedIndex=comboRoom.SelectedIndex;
			_listRooms=PhoneMapJSON.GetFromDb();
			comboRoom.Items.Clear();
			comboRoom.Items.Add("All");
			for(int i=0;i<_listRooms.Count;i++) {
				comboRoom.Items.Add(_listRooms[i].Description);
				if(_listRooms[i].MapAreaContainerNum==_mapAreaContainerNum) {
					selectedIndex=i+1;
				}
			}
			comboRoom.SelectedIndex=selectedIndex==-1 ? 0 : selectedIndex;
		}

		private void comboRoom_SelectionChangeCommitted(object sender,EventArgs e) {
			if(comboRoom.SelectedIndex < 1) {
				_mapAreaContainerNum=0;
			}
			else {
				_mapAreaContainerNum=_listRooms[comboRoom.SelectedIndex-1].MapAreaContainerNum;
			}
		}

		///<summary>Set the phone which is linked to the extension at this desk. If phone==null then no phone info shown.</summary>
		public void SetPhone(Phone phone,PhoneEmpDefault phoneEmpDefault,bool isTriageOperator) {
			phoneTile.SetPhone(phone,phoneEmpDefault,isTriageOperator);
		}

		private void FillTile() {
			UpdateComboRooms();//We can't do this in the constructor and all the other methods fire too often.  FillTile is a good place for this.
			//Get the new phone list from the database and redraw control.
			SetPhoneList(PhoneEmpDefaults.Refresh(),Phones.GetPhoneList());
			//Set the currently selected phone accordingly.
			if(_listPhones==null) {//No phone list. Shouldn't get here.
				phoneTile.SetPhone(null,null,false);
				return;
			}
			Phone phone=Phones.GetPhoneForExtension(_listPhones,Extension);
			PhoneEmpDefault phoneEmpDefault=null;
			if(phone!=null) {
				phoneEmpDefault=PhoneEmpDefaults.GetEmpDefaultFromList(phone.EmployeeNum,_listPhoneEmpDefaults);
			}
			phoneTile.SetPhone(phone,phoneEmpDefault,PhoneEmpDefaults.IsTriageOperatorForExtension(Extension,_listPhoneEmpDefaults));	
		}

		private void UserControlPhoneSmall_Paint(object sender,PaintEventArgs e) {
			Graphics g=e.Graphics;
			g.FillRectangle(SystemBrushes.Control,this.Bounds);
			if(_listPhones==null) {
				return;
			}
			int columns=9;
			int rows=1;
			//Dynamically figure out how many rows are needed if there are any phone tiles present.
			if(_listPhones.Count > 0 && columns > 0) {
				rows=(_listPhones.Count + columns - 1) / columns;//Rounds up the result of tile count / columns.
			}
			float boxWidth=((float)this.Width)/columns; //21.4f;
			float boxHeight=17f;
			float comboBoxHeight=21f;
			float hTot=boxHeight*rows;
			float x=0f;
			float y=0f;
			//Create a white "background" rectangle so that any empty squares (no employees) will show as white boxes instead of no color.
			g.FillRectangle(new SolidBrush(Color.White),x,y+comboBoxHeight,boxWidth*columns,boxHeight*rows);
			//Dynamically move the phone tile control down.
			int xTile=0;//Just in case this needs to be dynamic in the future as well.
			int yTile=(int)(boxHeight*rows) + (int)comboBoxHeight + 5;//The height of the lights plus a little padding.
			phoneTile.Location=new Point(xTile,yTile);
			//Dynamically resize the entire UserControlPhoneSmall.  If the width changes, update PhoneTile.LayoutHorizontal (property setter).
			this.Size=new System.Drawing.Size(213,yTile+phoneTile.Height);
			for(int i=0;i<_listPhones.Count;i++) {				
				//Draw the extension number if a person is available at that extension.
				if(_listPhones[i].ClockStatus!=ClockStatusEnum.Home
					&& _listPhones[i].ClockStatus!=ClockStatusEnum.None) {
					//Colors the box a color based on the corresponding phone's status.
					Color outerColor;
					Color innerColor;
					Color fontColor;
					bool isTriageOperatorOnTheClock=false;					
					//get the cubicle color and triage status
					PhoneEmpDefault ped=PhoneEmpDefaults.GetEmpDefaultFromList(_listPhones[i].EmployeeNum,_listPhoneEmpDefaults);
					Phones.GetPhoneColor(_listPhones[i],ped,false,out outerColor,out innerColor,out fontColor,out isTriageOperatorOnTheClock);
					using(Brush brush=new SolidBrush(outerColor)) {
						g.FillRectangle(brush,x*boxWidth,(y*boxHeight)+comboBoxHeight,boxWidth,boxHeight);
					}
					Font baseFont=new Font("Arial",7);
					SizeF extSize=g.MeasureString(_listPhones[i].Extension.ToString(),baseFont);
					float padX=(boxWidth-extSize.Width)/2;
					float padY=(boxHeight-extSize.Height)/2;
					using(Brush brush=new SolidBrush(Color.Black)) {
						g.DrawString(_listPhones[i].Extension.ToString(),baseFont,brush,(float)Math.Ceiling((x*boxWidth)+(padX)),(y*boxHeight)+(padY + comboBoxHeight));
					}
				}
				x++;
				if(x>=columns) {
					x=0f;
					y++;
				}
			}
			//horiz lines
			for(int i=0;i<rows+1;i++) {
				g.DrawLine(Pens.Black,0,(i*boxHeight)+comboBoxHeight,Width,(i*boxHeight)+comboBoxHeight);
			}
			//Very bottom
			g.DrawLine(Pens.Black,0,Height-1+comboBoxHeight,Width,Height-1+comboBoxHeight);
			//vert
			for(int i=0;i<columns;i++) {
				g.DrawLine(Pens.Black,i*boxWidth,comboBoxHeight,i*boxWidth,hTot+comboBoxHeight);
			}
			g.DrawLine(Pens.Black,Width-1,comboBoxHeight,Width-1,hTot+comboBoxHeight);
		}

		private void phoneTile_GoToChanged(object sender,EventArgs e) {
			if(phoneTile.PhoneCur==null) {
				return;
			}
			if(phoneTile.PhoneCur.PatNum==0) {
				return;
			}
			GotoPatNum=phoneTile.PhoneCur.PatNum;
			OnGoToChanged();
		}

		protected void OnGoToChanged() {
			if(GoToChanged!=null) {
				GoToChanged(this,new EventArgs());
			}
		}

		private void menuItemManage_Click(object sender,EventArgs e) {
			PhoneUI.Manage(phoneTile);
		}

		private void menuItemAdd_Click(object sender,EventArgs e) {
			PhoneUI.Add(phoneTile);
		}

		//Timecards-------------------------------------------------------------------------------------

		private void menuItemAvailable_Click(object sender,EventArgs e) {
			PhoneUI.Available(phoneTile);
			FillTile();
		}

		private void menuItemTraining_Click(object sender,EventArgs e) {
			PhoneUI.Training(phoneTile);
			FillTile();
		}

		private void menuItemTeamAssist_Click(object sender,EventArgs e) {
			PhoneUI.TeamAssist(phoneTile);
			FillTile();
		}

		private void menuItemNeedsHelp_Click(object sender,EventArgs e) {
			PhoneUI.NeedsHelp(phoneTile);
			FillTile();
		}

		private void menuItemWrapUp_Click(object sender,EventArgs e) {
			PhoneUI.WrapUp(phoneTile);
			FillTile();
		}

		private void menuItemOfflineAssist_Click(object sender,EventArgs e) {
			PhoneUI.OfflineAssist(phoneTile);
			FillTile();
		}

		private void menuItemUnavailable_Click(object sender,EventArgs e) {
			PhoneUI.Unavailable(phoneTile);
			FillTile();
		}

		private void menuItemBackup_Click(object sender,EventArgs e) {
			PhoneUI.Backup(phoneTile);
			FillTile();
		}		

		//RingGroups---------------------------------------------------

		private void menuItemQueueTech_Click(object sender,EventArgs e) {
			PhoneUI.QueueTech(phoneTile);
		}

		private void menuItemQueueNone_Click(object sender,EventArgs e) {
			PhoneUI.QueueNone(phoneTile);
		}

		private void menuItemQueueDefault_Click(object sender,EventArgs e) {
			PhoneUI.QueueDefault(phoneTile);
		}

		private void menuItemQueueBackup_Click(object sender,EventArgs e) {
			PhoneUI.QueueBackup(phoneTile);
		}

		//Timecard---------------------------------------------------

		private void menuItemLunch_Click(object sender,EventArgs e) {
			PhoneUI.Lunch(phoneTile);
			FillTile();
		}

		private void menuItemHome_Click(object sender,EventArgs e) {
			PhoneUI.Home(phoneTile);
			FillTile();
		}

		private void menuItemBreak_Click(object sender,EventArgs e) {
			PhoneUI.Break(phoneTile);
			FillTile();
		}



	}
}
