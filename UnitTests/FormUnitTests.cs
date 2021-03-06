using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenDental;
using OpenDentBusiness;

namespace UnitTests {
	public partial class FormUnitTests:Form {
		private bool isOracle;
		public FormUnitTests() {
			InitializeComponent();
		}

		private void FormUnitTests_Load(object sender,EventArgs e) {
			listType.Items.Add("MySql");
			listType.Items.Add("Oracle");
			listType.SelectedIndex=0;
			//throw new Exception("");
			//if(!DatabaseTools.SetDbConnection("unittest")){

			//}
			//if(!DatabaseTools.DbExists()){
			//	MessageBox.Show("Database does not exist: "+DatabaseTools.dbName);
			//}
		}

		private void listType_SelectedIndexChanged(object sender,EventArgs e) {
			if(listType.SelectedIndex==0) { //Only two selections, MySQL or Oracle.
				isOracle=false;
			}
			else {
				isOracle=true;
			}
		}

		private void butWebService_Click(object sender,EventArgs e) {
			RemotingClient.ServerURI=textMiddleTierURI.Text;
			Cursor=Cursors.WaitCursor;
			try{
				if(!isOracle){
					Userod user=Security.LogInWeb(textMiddleTierUser.Text,textMiddleTierPassword.Text,"",Application.ProductVersion,false);
					Security.CurUser=user;
					Security.PasswordTyped=textMiddleTierPassword.Text;
					RemotingClient.RemotingRole=RemotingRole.ClientWeb;
				}
				else if(isOracle) {
					MsgBox.Show(this,"Oracle does not have unit test for web service yet.");
					Cursor=Cursors.Default;
					return;
				}
			}
			catch(Exception ex){
				Cursor=Cursors.Default;
				MessageBox.Show(ex.Message);
				return;
			}
			textResults.Text="";
			Application.DoEvents();
			List<string> result=WebServiceT.RunAll();
			for(int i=0;i<result.Count;i++) {
				if(i>0) {
					textResults.AppendText("\r\n");
				}
				textResults.SelectionStart=textResults.TextLength;
				textResults.SelectionLength=0;
				if(result[i].Contains("Passed")) {
					textResults.SelectionColor=Color.Green;
				}
				else {
					textResults.SelectionColor=Color.Red;
				}
				textResults.AppendText(result[i]);
			}
			textResults.SelectionColor=textResults.ForeColor;
			Cursor=Cursors.Default;
		}

		private void butSchema_Click(object sender,EventArgs e) {
			Cursor=Cursors.WaitCursor;
			textResults.Text="";
			Application.DoEvents();
			if(radioSchema1.Checked) {
				textResults.Text+=SchemaT.TestProposedCrud(textAddr.Text,textPort.Text,textUserName.Text,textPassword.Text,isOracle);
			}
			else {
				textResults.Text+=SchemaT.CompareProposedToGenerated(isOracle);
			}
			Cursor=Cursors.Default;
		}

		private void butCore_Click(object sender,EventArgs e) {
			Cursor=Cursors.WaitCursor;
			textResults.Text="";
			Application.DoEvents();
			textResults.Text+=CoreTypesT.CreateTempTable(textAddr.Text,textPort.Text,textUserName.Text,textPassword.Text,isOracle);
			Application.DoEvents();
			textResults.Text+=CoreTypesT.RunAll();
			//}
			//else {
			//	textResults.Text+=CoreTypesT.RunAllMySql();
			//}
			Cursor=Cursors.Default;
		}

		private void butTopaz_Click(object sender,EventArgs e) {
			Cursor=Cursors.WaitCursor;
			textResults.Text="";
			Application.DoEvents();
			textResults.Text+=TopazT.RunAll();
			Cursor=Cursors.Default;
		}

		private void butNewDb_Click(object sender,EventArgs e) {
			textResults.Text="";
			Application.DoEvents();
			Cursor=Cursors.WaitCursor;
			if(!isOracle) {
				string serverAddr=textAddr.Text;
				string serverPort=textPort.Text;
				if(serverAddr=="") {
					serverAddr="localhost";
				}
				if(serverPort=="") {
					serverPort="3306";
				}
				if(!DatabaseTools.SetDbConnection("",serverAddr,serverPort,textUserName.Text,textPassword.Text,isOracle)) {
					MessageBox.Show("Could not connect");
					return;
				}
			}
			DatabaseTools.FreshFromDump(textAddr.Text,textPort.Text,textUserName.Text,textPassword.Text,isOracle);
			textResults.Text+="Fresh database loaded from sql dump.";
			Cursor=Cursors.Default;
		}

		private void butRun_Click(object sender,EventArgs e) {
			textResults.Text="";
			string serverAddr=textAddr.Text;
			string serverPort=textPort.Text;
			if(serverAddr=="") {
				serverAddr="localhost";
			}
			if(serverPort=="") {
				serverPort="3306";
			}
			Application.DoEvents();
			Cursor=Cursors.WaitCursor;
			if(!DatabaseTools.SetDbConnection("unittest",serverAddr,serverPort,textUserName.Text,textPassword.Text,false)) {//if database doesn't exist
				DatabaseTools.SetDbConnection("",serverAddr,serverPort,textUserName.Text,textPassword.Text,false);
				textResults.Text+=DatabaseTools.FreshFromDump(textAddr.Text,textPort.Text,textUserName.Text,textPassword.Text,false);//this also sets database to be unittest.
			}
			else {
				textResults.Text+=DatabaseTools.ClearDb();
			}
			Security.CurUser=Userods.GetAdminUser();  //Inserts of some objects now require a UserNum of currently logged in user.
			Prefs.UpdateBool(PrefName.InsPPOsecWriteoffs,false);
			try{
				textResults.Text+=BenefitT.BenefitComputeRenewDate();
			}
			catch(Exception ex) {
				textResults.Text+=ex.Message;//failed
			}
			try {
				textResults.Text+=ToothT.FormatRanges();
			}
			catch(Exception ex) {
				textResults.Text+=ex.Message;//failed
			}
			int specificTest=0;
			try {
				Application.DoEvents();
				specificTest=PIn.Int(textSpecificTest.Text);//typically zero
				textResults.Text+=AllTests.TestOneTwo(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="1&2: Failed. "+ex.Message+"\r\n";
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestThree(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="3: Failed. "+ex.Message+"\r\n";
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFour(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="4: Failed. "+ex.Message+"\r\n";
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFive(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="5: Failed. "+ex.Message+"\r\n";
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSix(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="6: Failed. "+ex.Message+"\r\n";
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSeven(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="7: Failed. "+ex.Message+"\r\n";
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestEight(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="8: Failed. "+ex.Message+"\r\n";
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestNine(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="9: Failed. "+ex.Message+"\r\n";
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestTen(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="10: Failed. "+ex.Message+"\r\n";
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestEleven(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="11: Failed. "+ex.Message+"\r\n";
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestTwelve(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="12: Failed. "+ex.Message+"\r\n";
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestThirteen(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="13: Failed. "+ex.Message+"\r\n";
			}
			//try {//There is a phantom TestFourteen that has been commented out. It is similary to TestOne.
			//  Application.DoEvents();
			//  textResults.Text+=AllTests.TestFourteen(specificTest);
			//}
			//catch(Exception ex) {
			//  textResults.Text+="14: Failed. "+ex.Message+"\r\n";
			//}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFourteen(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="14: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFifteen(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="15: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSixteen(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="16: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSeventeen(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="17: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestEighteen(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="18: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestNineteen(specificTest);
			}
			catch(Exception ex) {
			  textResults.Text+="19: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestTwenty(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="20: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestTwentyOne(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="21: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestTwentyTwo(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="22: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestTwentyThree(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="23: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestTwentyFour(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="24: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestTwentyFive(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="25: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestTwentySix(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="26: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestTwentySeven(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="27: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestTwentyEight(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="28: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestTwentyNine(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="29: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestThirty(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="30: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestThirtyOne(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="31: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestThirtyTwo(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="32: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestThirtyThree(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="33: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestThirtyFour(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="34: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestThirtyFive(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="35: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestThirtySix(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="36: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestThirtySeven(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="37: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestThirtyEight(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="38: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestThirtyNine(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="39: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFourty(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="40: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFourtyOne(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="41: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFourtyTwo(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="42: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFourtyThree(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="43: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFourtyFour(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="44: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFourtyFive(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="45: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFourtySix(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="46: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFourtySeven(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="47: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFourtyEight(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="48: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFourtyNine(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="49: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFifty(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="50: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFiftyOne(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="51: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFiftyTwo(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="52: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFiftyThree(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="53: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFiftyFour(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="54: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFiftyFive(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="55: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFiftySix(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="56: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFiftySeven(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="57: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFiftyEight(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="58: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestFiftyNine(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="59: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSixty(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="60: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSixtyOne(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="61: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSixtyTwo(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="62: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSixtyThree(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="63: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSixtyFour(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="64: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSixtyFive(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="65: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSixtySix(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="66: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSixtySeven(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="67: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSixtyEight(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="68: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSixtyNine(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="69: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSeventy(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="70: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSeventyOne(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="71: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSeventyTwo(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="72: Failed. "+ex.Message;
			}
			try {
				Application.DoEvents();
				textResults.Text+=AllTests.TestSeventyThree(specificTest);
			}
			catch(Exception ex) {
				textResults.Text+="73: Failed. "+ex.Message;
			}
			textResults.Text+="Done";
			Cursor=Cursors.Default;
		}

		private void butHL7_Click(object sender,EventArgs e) {
			textResults.Text="";
			string serverAddr=textAddr.Text;
			string serverPort=textPort.Text;
			if(serverAddr=="") {
				serverAddr="localhost";
			}
			if(serverPort=="") {
				serverPort="3306";
			}
			Application.DoEvents();
			Cursor=Cursors.WaitCursor;
			if(!DatabaseTools.SetDbConnection("unittest",serverAddr,serverPort,textUserName.Text,textPassword.Text,false)) {//if database doesn't exist
				DatabaseTools.SetDbConnection("",serverAddr,serverPort,textUserName.Text,textPassword.Text,false);
				textResults.Text+=DatabaseTools.FreshFromDump(textAddr.Text,textPort.Text,textUserName.Text,textPassword.Text,false);//this also sets database to be unittest.
			}
			foreach(HL7TestInterfaceEnum hl7TestInterfaceEnum in Enum.GetValues(typeof(HL7TestInterfaceEnum))) {
				textResults.Text+=DatabaseTools.ClearDb();
				textResults.Text+="Testing "+hl7TestInterfaceEnum.ToString()+" interface.\r\n";
				textResults.Text+=HL7Tests.HL7TestAll(hl7TestInterfaceEnum);
				textResults.Text+="\r\n\r\n";
			}
			textResults.Text+="Done\r\n";
			Cursor=Cursors.Default;
		}
	}
}