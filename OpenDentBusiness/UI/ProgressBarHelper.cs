using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDentBusiness {
	public class ProgressBarHelper {
		private string _labelValue;
		private string _percentValue;
		private int _blockValue;
		private int _blockMax;
		private string _tagString;
		private ProgBarStyle _progressStyle;
		private int _marqueeSpeed;

		

		///<summary>Used to set the label on the left of the progress bar</summary>
		public string LabelValue {
			get {
				return _labelValue;
			}
		}		
		
		///<summary>Used to set the label on the right of the progress bar</summary>
		public string PercentValue {
			get {
				return _percentValue;
			}
		}

		///<summary>Changes progress bar current block value</summary>
		public int BlockValue {
			get {
				return _blockValue;
			}
		}

		///<summary>Changes progress bar max value</summary>
		public int BlockMax {
			get {
				return _blockMax;
			}
		}

		///<summary>Used to uniquely identify this ODEvent for consumers. Can be null</summary>
		public string TagString {
			get {
				return _tagString;
			}
		}

		///<summary>Changes progress bar style</summary>
		public ProgBarStyle ProgressStyle {
			get {
				return _progressStyle;
			}
		}

		///<summary>Changes progress bar marquee speed</summary>
		public int MarqueeSpeed {
			get {
				return _marqueeSpeed;
			}
		}

		///<summary>Used as a shell to store information events need to update a progress window.</summary>
		public ProgressBarHelper(string labelValue,string percentValue=null,int blockValue=0,int blockMax=0
			,ProgBarStyle progressStyle=ProgBarStyle.NoneSpecified,string tagString=null,int marqueeSpeed=0) 
		{
			_labelValue=labelValue;
			_percentValue=percentValue;
			_blockValue=blockValue;
			_blockMax=blockMax;
			_progressStyle=progressStyle;
			_tagString=tagString;
			_marqueeSpeed=marqueeSpeed;

		}
	}
		
		public enum ProgBarStyle {
			NoneSpecified,
			Blocks,
			Marquee,
			Continuous
		}

}
