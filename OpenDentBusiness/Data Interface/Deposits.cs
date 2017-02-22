using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace OpenDentBusiness{
	///<summary></summary>
	public class Deposits {

		///<summary>Gets all Deposits, ordered by DateDeposit, DepositNum.  </summary>
		public static Deposit[] Refresh() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<Deposit[]>(MethodBase.GetCurrentMethod());
			}
			string command="SELECT * FROM deposit "
				+"ORDER BY DateDeposit";
			return Crud.DepositCrud.SelectMany(command).ToArray();
		}

		///<summary>Gets only Deposits which are not attached to transactions.</summary>
		public static Deposit[] GetUnattached() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<Deposit[]>(MethodBase.GetCurrentMethod());
			}
			string command="SELECT * FROM deposit "
				+"WHERE NOT EXISTS(SELECT * FROM transaction WHERE deposit.DepositNum=transaction.DepositNum) "
				+"ORDER BY DateDeposit";
			return Crud.DepositCrud.SelectMany(command).ToArray();
		}

		///<summary>Gets a single deposit directly from the database.</summary>
		public static Deposit GetOne(long depositNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<Deposit>(MethodBase.GetCurrentMethod(),depositNum);
			}
			return Crud.DepositCrud.SelectOne(depositNum);
		}

		///<summary></summary>
		public static void Update(Deposit dep){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),dep);
				return;
			}
			Crud.DepositCrud.Update(dep);
		}

		///<summary></summary>
		public static void Update(Deposit dep, Deposit depOld) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),dep,depOld);
				return;
			}
			Crud.DepositCrud.Update(dep,depOld);
		}

		///<summary></summary>
		public static long Insert(Deposit dep) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				dep.DepositNum=Meth.GetLong(MethodBase.GetCurrentMethod(),dep);
				return dep.DepositNum;
			}
			return Crud.DepositCrud.Insert(dep);
		}

		///<summary>Also handles detaching all payments and claimpayments.  Throws exception if deposit is attached as a source document to a transaction.  The program should have detached the deposit from the transaction ahead of time, so I would never expect the program to throw this exception unless there was a bug.</summary>
		public static void Delete(Deposit dep){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),dep);
				return;
			}
			//check dependencies
			string command="";
			if(dep.DepositNum !=0){
				command="SELECT COUNT(*) FROM transaction WHERE DepositNum ="+POut.Long(dep.DepositNum);
				if(PIn.Long(Db.GetCount(command))>0) {
					throw new ApplicationException(Lans.g("Deposits","Cannot delete deposit because it is attached to a transaction."));
				}
			}
			/*/check claimpayment 
			command="SELECT COUNT(*) FROM claimpayment WHERE DepositNum ="+POut.PInt(DepositNum);
			if(PIn.PInt(Db.GetCount(command))>0){
				throw new InvalidProgramException(Lans.g("Deposits","Cannot delete deposit because it has payments attached"));
			}*/
			//ready to delete
			command="UPDATE payment SET DepositNum=0 WHERE DepositNum="+POut.Long(dep.DepositNum);
			Db.NonQ(command);
			command="UPDATE claimpayment SET DepositNum=0 WHERE DepositNum="+POut.Long(dep.DepositNum);
			Db.NonQ(command);
			Crud.DepositCrud.Delete(dep.DepositNum);
		}

		///<summary>Detach specific payments and claimpayments from passed in deposit.</summary>
		public static void DetachFromDeposit(long depositNum,List<long> listPayNums,List<long> listClaimPaymentNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),depositNum,listPayNums,listClaimPaymentNums);
				return;
			}
			string command="";
			if(listPayNums.Count>0) {
				command="UPDATE payment SET DepositNum=0 WHERE DepositNum="+POut.Long(depositNum)
					+" AND PayNum IN("+string.Join(",",listPayNums)+")";
				Db.NonQ(command);
			}
			if(listClaimPaymentNums.Count>0) {
				command="UPDATE claimpayment SET DepositNum=0 WHERE DepositNum="+POut.Long(depositNum)
					+" AND ClaimPaymentNum IN("+string.Join(",",listClaimPaymentNums)+")";
				Db.NonQ(command);
			}
		}


	



	
	}

	

	


}




















