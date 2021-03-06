﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Drawing.Printing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDental {
	public partial class FormPatientPortal:ODForm {
		private Patient _patCur;
		///<summary>The current UserWeb instance from the passed in Patient.</summary>
		private UserWeb _userWebCur;
		///<summary>The unmodified UserWeb instance to compare to the current one when saving changes to the database.</summary>
		private UserWeb _userWebOld;
		///<summary>Keeps track if the user printed the patient's information.  Mainly used to show a reminder when the password changes and the user didn't print.</summary>
		private bool _wasPrinted;
		private bool _isNew;

		public FormPatientPortal(Patient patCur) {
			InitializeComponent();
			_patCur=patCur;
		}

		private void FormPatientPortal_Load(object sender,EventArgs e) {
			_userWebCur=UserWebs.GetByFKeyAndType(_patCur.PatNum,UserWebFKeyType.PatientPortal);
			if(_userWebCur==null) {
				_isNew=true;
				_userWebCur=new UserWeb();
				_userWebCur.UserName=CreatePatientPortalUserName();
				_userWebCur.FKey=_patCur.PatNum;
				_userWebCur.FKeyType=UserWebFKeyType.PatientPortal;
				_userWebCur.RequireUserNameChange=true;
				_userWebCur.Password="";
				UserWebs.Insert(_userWebCur);
			}
			_userWebOld=_userWebCur.Copy();
			textOnlineUsername.Text=_userWebCur.UserName;
			textOnlinePassword.Text="";
			if(_userWebCur.Password!="") {//if a password was already filled in
				butGiveAccess.Text="Remove Online Access";
				//We do not want to show the password hash that is stored in the database so we will fill the online password with asterisks.
				textOnlinePassword.Text="********";
				textOnlinePassword.ReadOnly=false;
				textOnlineUsername.ReadOnly=false;
			}
			textPatientPortalURL.Text=PrefC.GetString(PrefName.PatientPortalURL);
		}

		private string CreatePatientPortalUserName() {
			string retVal="";
			bool userNameFound=false;
			Random rand=new Random();
			while(!userNameFound) {
				retVal=_patCur.FName+rand.Next(100,100000);
				if(!UserWebs.UserNameExists(retVal,UserWebFKeyType.PatientPortal)) {
					userNameFound=true;
				}
			}
			return retVal;
		}

		private void menuItemSetup_Click(object sender,EventArgs e) {
			FormEServicesSetup formPPS=new FormEServicesSetup();
			formPPS.ShowDialog();
			textPatientPortalURL.Text=PrefC.GetString(PrefName.PatientPortalURL);
		}

		private void butGiveAccess_Click(object sender,EventArgs e) {
			if(butGiveAccess.Text=="Provide Online Access") {//When form open opens with a blank password
				if(PrefC.GetString(PrefName.PatientPortalURL)=="") {
					//User probably hasn't set up the patient portal yet.
					MsgBox.Show(this,"Patient Facing URL is required to be set before granting online access.  Click Setup to set the Patient Facing URL.");
					return;
				}
				string error=ValidatePatientAccess();
				if(error!="") {
					MessageBox.Show(error);
					return;
				}
				Cursor=Cursors.WaitCursor;
				//1. Fill password.
				string passwordGenerated=GenerateRandomPassword(8);
				if(_patCur.SSN!="" && _patCur.FName!="") {
					passwordGenerated=_patCur.FName.Substring(0,1).ToUpper()+_patCur.LName.Substring(0,1).ToLower()+_patCur.SSN;//First inital+Last initial+SSN
				}
				textOnlinePassword.Text=passwordGenerated;
				//2. Make the username and password editable in case they want to change it.
				textOnlineUsername.ReadOnly=false;
				textOnlinePassword.ReadOnly=false;
				//3. Save password to db.
				// We only save the hash of the generated password.
				string passwordHashed=Userods.HashPassword(passwordGenerated,false);
				_userWebCur.Password=passwordHashed;
				UserWebs.Update(_userWebCur,_userWebOld);
				_userWebOld.Password=passwordHashed;//Update _userWebOld in case the user changes password manually.
				//4. Insert EhrMeasureEvent
				EhrMeasureEvent newMeasureEvent=new EhrMeasureEvent();
				newMeasureEvent.DateTEvent=DateTime.Now;
				newMeasureEvent.EventType=EhrMeasureEventType.OnlineAccessProvided;
				newMeasureEvent.PatNum=_userWebCur.FKey;
				newMeasureEvent.MoreInfo="";
				EhrMeasureEvents.Insert(newMeasureEvent);
				//5. Rename button
				butGiveAccess.Text="Remove Online Access";
				Cursor=Cursors.Default;
			}
			else {//remove access
				Cursor=Cursors.WaitCursor;
				//1. Clear password
				textOnlinePassword.Text="";
				//2. Make in uneditable
				textOnlinePassword.ReadOnly=true;
				//3. Save password to db
				_userWebCur.Password=textOnlinePassword.Text;
				UserWebs.Update(_userWebCur,_userWebOld);
				_userWebOld.Password=textOnlinePassword.Text;//Update PatOld in case the user changes password manually.
				//4. Rename button
				butGiveAccess.Text="Provide Online Access";
				Cursor=Cursors.Default;
			}
		}

		private void butOpen_Click(object sender,EventArgs e) {
			if(textPatientPortalURL.Text=="") {
				MessageBox.Show("Please use Setup to set the Online Access Link first.");
				return;
			}
			try {
				System.Diagnostics.Process.Start(textPatientPortalURL.Text);
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		
		private void butGenerate_Click(object sender,EventArgs e) {
			if(textOnlinePassword.ReadOnly) {
				MessageBox.Show("Please use the Provide Online Access button first.");
				return;
			}
			Cursor=Cursors.WaitCursor;
			string passwordGenerated=GenerateRandomPassword(8);
			textOnlinePassword.Text=passwordGenerated;
			// We only save the hash of the generated password.
			string passwordHashed=Userods.HashPassword(passwordGenerated,false);
			_userWebCur.Password=passwordHashed;
			UserWebs.Update(_userWebCur,_userWebOld);
			_userWebOld.Password=passwordHashed;//Update PatOld in case the user changes password manually.
			Cursor=Cursors.Default;
		}

		///<summary>Generates a random password 8 char long containing at least one uppercase, one lowercase, and one number.</summary>
		private static string GenerateRandomPassword(int length) {
			//Chracters like o(letter O), 0 (Zero), l (letter l), 1 (one) etc are avoided because they can be ambigious.
			string PASSWORD_CHARS_LCASE="abcdefgijkmnopqrstwxyz";
			string PASSWORD_CHARS_UCASE="ABCDEFGHJKLMNPQRSTWXYZ";
			string PASSWORD_CHARS_NUMERIC="23456789";
			//Create a local array containing supported password characters grouped by types.
			char[][] charGroups=new char[][]{
            PASSWORD_CHARS_LCASE.ToCharArray(),
            PASSWORD_CHARS_UCASE.ToCharArray(),
            PASSWORD_CHARS_NUMERIC.ToCharArray(),};
			//Use this array to track the number of unused characters in each character group.
			int[] charsLeftInGroup=new int[charGroups.Length];
			//Initially, all characters in each group are not used.
			for(int i=0;i<charsLeftInGroup.Length;i++) {
				charsLeftInGroup[i]=charGroups[i].Length;
			}
			//Use this array to track (iterate through) unused character groups.
			int[] leftGroupsOrder=new int[charGroups.Length];
			//Initially, all character groups are not used.
			for(int i=0;i<leftGroupsOrder.Length;i++) {
				leftGroupsOrder[i]=i;
			}
			Random random=new Random();
			//This array will hold password characters.
			char[] password=new char[length];
			//Index of the next character to be added to password.
			int nextCharIdx;
			//Index of the next character group to be processed.
			int nextGroupIdx;
			//Index which will be used to track not processed character groups.
			int nextLeftGroupsOrderIdx;
			//Index of the last non-processed character in a group.
			int lastCharIdx;
			//Index of the last non-processed group.
			int lastLeftGroupsOrderIdx=leftGroupsOrder.Length - 1;
			//Generate password characters one at a time.
			for(int i=0;i<password.Length;i++) {
				//If only one character group remained unprocessed, process it;
				//otherwise, pick a random character group from the unprocessed
				//group list. To allow a special character to appear in the
				//first position, increment the second parameter of the Next
				//function call by one, i.e. lastLeftGroupsOrderIdx + 1.
				if(lastLeftGroupsOrderIdx==0) {
					nextLeftGroupsOrderIdx=0;
				}
				else {
					nextLeftGroupsOrderIdx=random.Next(0,lastLeftGroupsOrderIdx);
				}
				//Get the actual index of the character group, from which we will
				//pick the next character.
				nextGroupIdx=leftGroupsOrder[nextLeftGroupsOrderIdx];
				//Get the index of the last unprocessed characters in this group.
				lastCharIdx=charsLeftInGroup[nextGroupIdx] - 1;
				//If only one unprocessed character is left, pick it; otherwise,
				//get a random character from the unused character list.
				if(lastCharIdx==0) {
					nextCharIdx=0;
				}
				else {
					nextCharIdx=random.Next(0,lastCharIdx+1);
				}
				//Add this character to the password.
				password[i]=charGroups[nextGroupIdx][nextCharIdx];
				//If we processed the last character in this group, start over.
				if(lastCharIdx==0) {
					charsLeftInGroup[nextGroupIdx]=charGroups[nextGroupIdx].Length;
					//There are more unprocessed characters left.
				}
				else {
					//Swap processed character with the last unprocessed character
					//so that we don't pick it until we process all characters in
					//this group.
					if(lastCharIdx !=nextCharIdx) {
						char temp=charGroups[nextGroupIdx][lastCharIdx];
						charGroups[nextGroupIdx][lastCharIdx]=charGroups[nextGroupIdx][nextCharIdx];
						charGroups[nextGroupIdx][nextCharIdx]=temp;
					}
					//Decrement the number of unprocessed characters in
					//this group.
					charsLeftInGroup[nextGroupIdx]--;
				}
				//If we processed the last group, start all over.
				if(lastLeftGroupsOrderIdx==0) {
					lastLeftGroupsOrderIdx=leftGroupsOrder.Length - 1;
					//There are more unprocessed groups left.
				}
				else {
					//Swap processed group with the last unprocessed group
					//so that we don't pick it until we process all groups.
					if(lastLeftGroupsOrderIdx !=nextLeftGroupsOrderIdx) {
						int temp=leftGroupsOrder[lastLeftGroupsOrderIdx];
						leftGroupsOrder[lastLeftGroupsOrderIdx]=
                                leftGroupsOrder[nextLeftGroupsOrderIdx];
						leftGroupsOrder[nextLeftGroupsOrderIdx]=temp;
					}
					//Decrement the number of unprocessed groups.
					lastLeftGroupsOrderIdx--;
				}
			}
			//Convert password characters into a string and return the result.
			return new string(password);
		}

		private void butPrint_Click(object sender,EventArgs e) {
			if(textPatientPortalURL.Text=="") {
				MsgBox.Show(this,"Online Access Link required. Please use Setup to set the Online Access Link first.");
				return;
			}
			if(textOnlinePassword.Text=="" || textOnlinePassword.Text=="********") {
				MessageBox.Show("Password required. Please generate a new password.");
				return;
			}
			string error=Patients.IsPortalPasswordValid(textOnlinePassword.Text);
			if(error!="") {//Non-empty string means it was invalid.
				MessageBox.Show(this,error);
				return;
			}
			_wasPrinted=true;
			//Then, print the info that the patient will be given in order for them to log in online.
			PrintPatientInfo();
		}

		private void PrintPatientInfo() {
			PrintDocument pd=new PrintDocument();
			pd.PrintPage += new PrintPageEventHandler(this.pd_PrintPage);
			pd.DefaultPageSettings.Margins=new Margins(25,25,40,40);
			if(pd.DefaultPageSettings.PrintableArea.Height==0) {
				pd.DefaultPageSettings.PaperSize=new PaperSize("default",850,1100);
			}
			try {
				#if DEBUG
				FormRpPrintPreview pView = new FormRpPrintPreview();
				pView.printPreviewControl2.Document=pd;
				pView.ShowDialog();
				#else
						if(PrinterL.SetPrinter(pd,PrintSituation.Default,_patCur.PatNum,"Patient portal login information printed")) {
							pd.Print();
						}
				#endif
			}
			catch {
				MessageBox.Show(Lan.g(this,"Printer not available"));
			}
		}

		private void pd_PrintPage(object sender,System.Drawing.Printing.PrintPageEventArgs e) {
			Rectangle bounds=e.MarginBounds;
			//new Rectangle(50,40,800,1035);//Some printers can handle up to 1042
			Graphics g=e.Graphics;
			string text;
			Font headingFont=new Font("Arial",13,FontStyle.Bold);
			Font font=new Font("Arial",10,FontStyle.Regular);
			int yPos=bounds.Top+100;
			int center=bounds.X+bounds.Width/2;
			text="Online Access";
			g.DrawString(text,headingFont,Brushes.Black,center-g.MeasureString(text,font).Width/2,yPos);
			yPos+=50;
			text="Website: "+textPatientPortalURL.Text;
			g.DrawString(text,font,Brushes.Black,bounds.Left+100,yPos);
			yPos+=25;
			text="Username: "+textOnlineUsername.Text;
			g.DrawString(text,font,Brushes.Black,bounds.Left+100,yPos);
			yPos+=25;
			text="Password: "+textOnlinePassword.Text;
			g.DrawString(text,font,Brushes.Black,bounds.Left+100,yPos);
			g.Dispose();
			e.HasMorePages=false;
		}
		
		private string ValidatePatientAccess() {
			string strErrors="";
			if(_patCur.FName.Trim()=="") {
				if(strErrors!="") {
					strErrors+="\r\n";
				}
				strErrors+=Lan.g(this,"Missing patient first name.");
			}
			if(_patCur.LName.Trim()=="") {
				if(strErrors!="") {
					strErrors+="\r\n";
				}
				strErrors+=Lan.g(this,"Missing patient last name.");
			}
			if(_patCur.Address.Trim()=="") {
				if(strErrors!="") {
					strErrors+="\r\n";
				}
				strErrors+=Lan.g(this,"Missing patient address line 1.");
			}
			if(_patCur.City.Trim()=="") {
				if(strErrors!="") {
					strErrors+="\r\n";
				}
				strErrors+=Lan.g(this,"Missing patient city.");
			}
			if(_patCur.State.Trim().Length!=2) {
				if(strErrors!="") {
					strErrors+="\r\n";
				}
				strErrors+=Lan.g(this,"Invalid patient state.  Must be two letters.");
			}
			if(_patCur.Birthdate.Year<1880) {
				if(strErrors!="") {
					strErrors+="\r\n";
				}
				strErrors+=Lan.g(this,"Missing patient birth date.");
			}
			if(_patCur.HmPhone.Trim()=="" && _patCur.WirelessPhone.Trim()=="" && _patCur.WkPhone.Trim()=="") {
				if(strErrors!="") {
					strErrors+="\r\n";
				}
				strErrors+=Lan.g(this,"Missing patient phone;  Must have home, wireless, or work phone.");
			}
			return strErrors;
		}

		private void butOK_Click(object sender,EventArgs e) {
			bool shouldUpdateUserWeb=false;
			bool shouldPrint=false;
			if(textOnlineUsername.ReadOnly==false) {
				if(textOnlineUsername.Text=="") {
					MsgBox.Show(this,"Online Username cannot be blank.");
					return;
				}
				else if(_userWebCur.UserName!=textOnlineUsername.Text) {
					if(UserWebs.UserNameExists(textOnlineUsername.Text,UserWebFKeyType.PatientPortal)) {
						MsgBox.Show(this,"The Online Username already exists.");
						return;
					}
					_userWebCur.UserName=textOnlineUsername.Text;
					shouldUpdateUserWeb=true;
					if(!_wasPrinted) {
						shouldPrint=true;
					}
				}
			}
			if(textOnlinePassword.Text!="" && textOnlinePassword.Text!="********") {
				string error=Patients.IsPortalPasswordValid(textOnlinePassword.Text);
				if(error!="") {//Non-empty string means it was invalid.
					MessageBox.Show(this,error);
					return;
				}
				if(!_wasPrinted) {
					shouldPrint=true;
				}
				shouldUpdateUserWeb=true;
				_userWebCur.Password=Userods.HashPassword(textOnlinePassword.Text,false);
			}
			if(shouldPrint) {
				DialogResult result=MessageBox.Show(Lan.g(this,"Online Username or Password changed but was not printed, would you like to print?")
					,Lan.g(this,"Print Patient Info")
					,MessageBoxButtons.YesNoCancel);
				if(result==DialogResult.Yes) {
					//Print the showing information.
					PrintPatientInfo();
				}
				else if(result==DialogResult.No) {
					//User does not want to print.  Do nothing.
				}
				else if(result==DialogResult.Cancel) {
					return;
				}
			}
			if(shouldUpdateUserWeb) {
				UserWebs.Update(_userWebCur,_userWebOld);
			}
			DialogResult=DialogResult.OK;
		}

		private void butCancel_Click(object sender,EventArgs e) {
			if(_isNew) {
				UserWebs.Delete(_userWebCur.UserWebNum);
			}
			DialogResult=DialogResult.Cancel;
		}

	

		

		

		

	

	}
}
