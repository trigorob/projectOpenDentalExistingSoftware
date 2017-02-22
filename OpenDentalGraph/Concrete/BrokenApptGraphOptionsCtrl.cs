using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDentalGraph {
	public partial class BrokenApptGraphOptionsCtrl:BaseGraphOptionsCtrl {

		private List<Def> _listAdjTypes=null;
		public List<Def> ListAdjTypes {
			get {
				if(_listAdjTypes==null) {
					_listAdjTypes=DefC.GetPositiveAdjTypes();
				}
				return _listAdjTypes;
			}
		}
		//public enum Grouping { provider, clinic };
		public enum RunFor { appointment, procedure, adjustment };
		public RunFor CurRunFor
		{
			get
			{
				if(radioRunAdjs.Checked) {
					return RunFor.adjustment;
				}
				else if(radioRunApts.Checked) {
					return RunFor.appointment;
				}
				else {
					return RunFor.procedure;
				}
			}
			set
			{
				switch(value) {
					case RunFor.adjustment:
						radioRunAdjs.Checked=true;
						break;
					case RunFor.appointment:
						radioRunApts.Checked=true;
						break;
					case RunFor.procedure:
						radioRunProcs.Checked=true;
						break;
				}
			}
		}

		public long AdjTypeDefNumCur
		{
			get
			{
				if(comboAdjType.SelectedIndex==-1) {
					return 0;
				}
				else {
					return ListAdjTypes[comboAdjType.SelectedIndex].DefNum;
				}
			}
			set
			{
				for(int i=0;i<ListAdjTypes.Count;i++) {
					if(ListAdjTypes[i].DefNum==value) {
						comboAdjType.SelectedIndex=i;
						return;
					}
				}
			}
		}
		
		public BrokenApptGraphOptionsCtrl() {
			InitializeComponent();
		}

		///<summary>Because definitions don't exist in the eservices table, we should only get these if necessary on load.</summary>
		private void BrokenApptGraphOptionsCtrl_Load(object sender,EventArgs e) {
			FillComboAdj();
		}

		public override int GetPanelHeight() {
			return this.Height;
		}

		private void OnBrokenApptGraphOptionsChanged(object sender,EventArgs e) {
			if(radioRunAdjs.Checked) {
				comboAdjType.Enabled=true;
			}
			else {
				comboAdjType.Enabled=false;
			}
			OnBaseInputsChanged(sender,e);
		}

		private void FillComboAdj() {
			foreach(Def adjType in ListAdjTypes) {
				comboAdjType.Items.Add(adjType.ItemName);
			}
			if(comboAdjType.Items.Count<=0) {
				comboAdjType.Items.Add(Lans.g(this,"Adj types not setup"));
				radioRunAdjs.Enabled=false;
			}
		}

	}
}
