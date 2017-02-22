using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using OpenDental.UI;
using System.Drawing;
using OpenDentBusiness;
using System.IO;
using CodeBase;

namespace OpenDental {
	public partial class FormFilePicker:ODForm {
		///<summary>List of selected files, including their path.</summary>
		public List<string> SelectedFiles=new List<string>();
		///<summary>Re-use the memory for each new thumbmail.  This prevents memory leaks.</summary>
		private Bitmap _thumbnail;

		public FormFilePicker(string defaultPath) {
			InitializeComponent();
			Lan.F(this);
			textPath.Text=defaultPath;
		}

		private void FormFilePicker_Load(object sender,EventArgs e) {
			FillGrid();
		}

		private void FillGrid() {
			//Get Dropbox directory based on textPath.Text
			gridMain.BeginUpdate();
			gridMain.Columns.Clear();
			ODGridColumn col=new ODGridColumn(Lan.g("FilePickerTable","File Name"),0);
			gridMain.Columns.Add(col);
			gridMain.Rows.Clear();
			ODGridRow row;
			//Get list of contents in directory of textPath.Text
			DropboxApi.TaskStateListFolders state=DropboxApi.ListFolderContents(ProgramProperties.GetPropVal(Programs.GetProgramNum(ProgramName.Dropbox),DropboxApi.PropertyDescs.AccessToken)
						,textPath.Text);
			List<string> listFiles=state.ListFolderPathsDisplay;
			for(int i=0;i<listFiles.Count;i++){
				row=new ODGridRow();
				row.Cells.Add(Path.GetFileName(listFiles[i]));	
				gridMain.Rows.Add(row);
			}
			gridMain.EndUpdate();
		}

		private void butGo_Click(object sender,EventArgs e) {
			//Refresh the grid contents to show whatever is in the Path textbox.
			if(!textPath.Text.Contains(ImageStore.GetPreferredAtoZpath())) {
				textPath.Text=ImageStore.GetPreferredAtoZpath();//They deleted the path for some reason.  It must have at least the base path.
			}
			FillGrid();
		}

		private void butPreview_Click(object sender,EventArgs e) {
			//A couple options here
			//Download the file and run the explorer windows process to show the temporary file
			if(!gridMain.Rows[gridMain.GetSelectedIndex()].Cells[0].Text.Contains(".")) {//File path doesn't contain an extension and thus is a subfolder.
				return;
			}
			FormProgress FormP=new FormProgress();
			FormP.DisplayText="Downloading from Dropbox...";
			FormP.NumberFormat="F";
			FormP.NumberMultiplication=1;
			FormP.MaxVal=100;//Doesn't matter what this value is as long as it is greater than 0
			FormP.TickMS=1000;
			DropboxApi.TaskStateDownload state=DropboxApi.DownloadAsync(ProgramProperties.GetPropVal(Programs.GetProgramNum(ProgramName.Dropbox),DropboxApi.PropertyDescs.AccessToken)
				,textPath.Text
				,Path.GetFileName(gridMain.Rows[gridMain.GetSelectedIndex()].Cells[0].Text)
				,new DropboxApi.ProgressHandler(FormP.OnProgress));
			if(FormP.ShowDialog()==DialogResult.Cancel) {
				state.DoCancel=true;
				return;
			}
			string tempFile=ODFileUtils.CreateRandomFile(Path.GetTempPath(),Path.GetExtension(gridMain.Rows[gridMain.GetSelectedIndex()].Cells[0].Text));
			File.WriteAllBytes(tempFile,state.FileContent);
			System.Diagnostics.Process.Start(tempFile);
		}

		private void butFileChoose_Click(object sender,EventArgs e) {
			//Choose file using standard windows choose file dialogue.
			OpenFileDialog dlg=new OpenFileDialog();
			dlg.Multiselect=true;
			dlg.InitialDirectory="";
			if(dlg.ShowDialog()!=DialogResult.OK) {
				return;
			}
			SelectedFiles=dlg.FileNames.ToList();
			DialogResult=DialogResult.OK;//Close the window when they choose files this way. 
			//It overrides their selected files from DropBox, but we have no way of clearing SelectedFiles of the ones they chose in the normal 
			//OpenFileDialog window, so I would rather attach less and be safe than attaching things they may forget they have selected if they had chosen
			//things with OpenFileDialog then went and selected more from DropBox.
		}

		private void gridMain_CellClick(object sender,UI.ODGridClickEventArgs e) {
			//Determine if it's a folder or a file that was clicked
			//If a folder, do nothing
			//If a file, download dropbox's thumbnail and display it
			if(gridMain.Rows[gridMain.GetSelectedIndex()].Cells[0].Text.Contains(".")) {//They selected a file because there is an extension.
				//Place thumbnail within odPictureox to display
				DropboxApi.TaskStateThumbnail state=DropboxApi.GetThumbnail(ProgramProperties.GetPropVal(Programs.GetProgramNum(ProgramName.Dropbox),DropboxApi.PropertyDescs.AccessToken)
					,ODFileUtils.CombinePaths(textPath.Text,gridMain.Rows[gridMain.GetSelectedIndex()].Cells[0].Text,'/'));
				if(state==null || state.FileContent==null) {
					labelThumbnail.Visible=true;
					odPictureBox.Visible=false;
				}
				else { 
					labelThumbnail.Visible=false;
					odPictureBox.Visible=true;
					using(MemoryStream stream=new MemoryStream(state.FileContent)) {
						_thumbnail=new Bitmap(Image.FromStream(stream));
					}
					odPictureBox.Image=_thumbnail;
					odPictureBox.Invalidate();
				}
			}
			else {
				labelThumbnail.Visible=false;
				odPictureBox.Visible=false;
			}
		}

		private void gridMain_CellDoubleClick(object sender,UI.ODGridClickEventArgs e) {
			//Determine if it's a folder or a file. 
			//If a folder, append the folder's name to the path and display folder contents
			//If it's a file, return it as the only item selected.
			if(gridMain.Rows[gridMain.GetSelectedIndex()].Cells[0].Text.Contains(".")) {//They selected a file because there is an extension.
				SelectedFiles.Clear();
				SelectedFiles.Add(ODFileUtils.CombinePaths(textPath.Text,gridMain.Rows[gridMain.GetSelectedIndex()].Cells[0].Text,'/'));
				DialogResult=DialogResult.OK;
			}
			else {
				textPath.Text=ODFileUtils.CombinePaths(textPath.Text,gridMain.Rows[gridMain.GetSelectedIndex()].Cells[0].Text,'/');
				FillGrid();
			}
		}

		private void textPath_KeyPress(object sender,KeyEventArgs e) {
			if(e.KeyCode==Keys.Enter) {
				e.Handled=true;
				e.SuppressKeyPress=true;
				butGo_Click(null,null);
			}
		}

		private void butOK_Click(object sender,EventArgs e) {
			//Add all selected files to the list to be returned
			SelectedFiles.Clear();//Just in case
			foreach(int idx in gridMain.SelectedIndices) {
				SelectedFiles.Add(ODFileUtils.CombinePaths(textPath.Text,gridMain.Rows[idx].Cells[0].Text,'/'));
			}
			DialogResult=DialogResult.OK;
		}

		private void butCancel_Click(object sender,EventArgs e) {
			DialogResult=DialogResult.Cancel;
		}

	}
}