using CodeBase;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Stone;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace OpenDentBusiness {
	public class DropboxApi {

		///<summary>If a file is greater than 2MB in size, we will break it up into chunks when uploading it to Dropbox.</summary>
		const int MAX_FILE_SIZE_BYTES=2000000;
		public delegate void ProgressHandler(double newCurVal,string newDisplayText,double newMaxVal,string errorMessage);

		public class PropertyDescs {
			public static string AtoZPath="Dropbox AtoZ Path";
			public static string AccessToken="Dropbox API Token";
		}

		#region Auth

		///<summary>Called by OAuth web app to display this URL in their browser.</summary>
		public static string GetDropboxAuthorizationUrl(string appkey) {
			return ODDropbox.GetDropboxAuthorizationUrl(appkey);
		}
		
		///<summary>Throws exception.  Called by Open Dental Web Services HQ to get the real access code form the code given by Dropbox.</summary>
		public static string GetDropboxAccessToken(string code,string appkey,string appsecret) {
			return ODDropbox.GetDropboxAccessToken(code,appkey,appsecret);
		}

		#endregion

		#region IO
		
		///<summary>Asynchronous method.  Uploads the passed in file to Dropbox.  Will overwrite the passed in file if it already exists.
		///Pass in onProgress to hook up to a progress bar.  If onProgress is null, this method will break.</summary>
		public static TaskStateUpload UploadAsync(string accessToken,string folder,string fileName,byte[] fileContent,ProgressHandler onProgress) {
			//No need to check RemotingRole; no call to db.
			TaskStateUpload taskState=new TaskStateUpload(new DropboxClient(accessToken)) {
				Folder=folder,
				FileName=fileName,
				FileContent=fileContent,
				OnProgress=onProgress
			};
			ODDropbox.UploadAsync(taskState);
			return taskState;
		}
		
		///<summary>Synchronous.  Uploads the passed in file to Dropbox.  Will overwrite the passed in file if it already exists.</summary>
		public static TaskStateUpload Upload(string accessToken,string folder,string fileName,byte[] fileContent) {
			//No need to check RemotingRole; no call to db.
			TaskStateUpload taskState=new TaskStateUpload(new DropboxClient(accessToken)) {
				Folder=folder,
				FileName=fileName,
				FileContent=fileContent
			};
			ODDropbox.Upload(taskState);
			return taskState;
		}
		
		///<summary>Asynchronous.  Downloads the file from Dropbox with the passed in folder path and file name.
		///Pass in onProgress to hook up to a progress bar.  If onProgress is null, this method will break.</summary>
		public static TaskStateDownload DownloadAsync(string accessToken,string folder,string fileName,ProgressHandler onProgress) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<TaskStateDownload>(MethodBase.GetCurrentMethod(),accessToken,folder,fileName,onProgress);
			}
			TaskStateDownload taskState=new TaskStateDownload(new DropboxClient(accessToken)) {
				Folder=folder,
				FileName=fileName,
				OnProgress=onProgress,
				UseId=false
			};
			ODDropbox.DownloadAsync(taskState);
			return taskState;
		}
		
		///<summary>Synchronous.  Downloads the file from Dropbox with the passed in folder path and file name.</summary>
		public static TaskStateDownload Download(string accessToken,string folder,string fileName) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<TaskStateDownload>(MethodBase.GetCurrentMethod(),accessToken,folder,fileName);
			}
			TaskStateDownload taskState=new TaskStateDownload(new DropboxClient(accessToken)) {
				Folder=folder,
				FileName=fileName,
				UseId=false
			};
			ODDropbox.Download(taskState);
			return taskState;
		}
		
		///<summary>Asynchronous.  Downloads the file from Dropbox with the passed in file identifier.  
		///Pass in onProgress to hook up to a progress bar.</summary>
		public static TaskStateDownload DownloadByIDAsync(string accessToken,string fileId,ProgressHandler onProgress) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<TaskStateDownload>(MethodBase.GetCurrentMethod(),accessToken,fileId,onProgress);
			}
			TaskStateDownload taskState=new TaskStateDownload(new DropboxClient(accessToken)) {
				OnProgress=onProgress,
				FileId=fileId,
				UseId=true
			};
			ODDropbox.DownloadAsync(taskState);
			return taskState;
		}
		
		///<summary>Synchronous.  Downloads the file from Dropbox with the passed in file identifier.</summary>
		public static TaskStateDownload DownloadByID(string accessToken,string fileId) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<TaskStateDownload>(MethodBase.GetCurrentMethod(),accessToken,fileId);
			}
			TaskStateDownload taskState=new TaskStateDownload(new DropboxClient(accessToken)) {
				FileId=fileId,
				UseId=true
			};
			ODDropbox.Download(taskState);
			return taskState;
		}
		
		///<summary>Asynchronous.  Use TaskStateMove to get the end result information from running Move.</summary>
		public static TaskStateMove MoveAsync(string accessToken,string fromPath,string toPath,ProgressHandler onProgress) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<TaskStateMove>(MethodBase.GetCurrentMethod(),accessToken,fromPath,toPath);
			}
			TaskStateMove taskState=new TaskStateMove(new DropboxClient(accessToken)) {
				FromPath=fromPath,
				ToPath=toPath,
				OnProgress=onProgress
			};
			ODDropbox.MoveAsync(taskState);
			return taskState;
		}
		
		///<summary>Synchronous.  TaskStateListFolders will hold the result from the passed in path.</summary>
		public static TaskStateListFolders ListFolderContents(string accessToken,string folderPath) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<TaskStateListFolders>(MethodBase.GetCurrentMethod(),accessToken,folderPath);
			}
			TaskStateListFolders taskState=new TaskStateListFolders(new DropboxClient(accessToken)) {
				FolderPath=folderPath
			};
			ODDropbox.ListFolderContents(taskState);
			return taskState;
		}
		
		///<summary>Synchronous.  TaskStateThumbnail holds the thumbnail in bytes after the task thread is finished.</summary>
		public static TaskStateThumbnail GetThumbnail(string accessToken,string filePath) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<TaskStateThumbnail>(MethodBase.GetCurrentMethod(),accessToken,filePath);
			}
			TaskStateThumbnail taskState=new TaskStateThumbnail(new DropboxClient(accessToken)) {
				FilePath=filePath
			};
			ODDropbox.GetThumbnail(taskState);
			return taskState;
		}
		
		///<summary>Synchronous.  TaskStateDelete holds the Ids for the deleted file(s).</summary>
		public static TaskStateDelete Delete(string accessToken,string path,ProgressHandler onProgress=null) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<TaskStateDelete>(MethodBase.GetCurrentMethod(),accessToken,path);
			}
			TaskStateDelete taskState=new TaskStateDelete(new DropboxClient(accessToken)) {
				Path=path,
				OnProgress=onProgress
			};
			ODDropbox.Delete(taskState);
			return taskState;
		}
		
		///<summary>Synchronous.  Returns true if the file for the given path exists.</summary>
		public static bool FileExists(string accessToken,string path) {
			return ODDropbox.FileExists(new DropboxClient(accessToken),path);
		}

		#endregion

		///<summary>Remoting role not necessary.</summary>
		private class ODDropbox {	
			
			///<summary>Called by OAuth web app to display this URL in their browser.</summary>
			public static string GetDropboxAuthorizationUrl(string appkey) {
				try {
					string ret=DropboxOAuth2Helper.GetAuthorizeUri(appkey).ToString();
					if(string.IsNullOrEmpty(ret)) {
						throw new Exception("Invalid URL returned by Dropbox");
					}
					return ret;
				}
				catch(Exception e) {
					throw new ApplicationException(e.Message,e);
				}
			}
		
			///<summary>Called by Open Dental Proper to get the real access code form the code given by Dropbox.  Returns empty string if something went wrong.</summary>
			public static string GetDropboxAccessToken(string code,string appkey,string appsecret) {
				string ret="";
				ApplicationException ae=null;
				ManualResetEvent wait=new ManualResetEvent(false);
				new System.Threading.Tasks.Task(async () => {
					try {
						OAuth2Response resp=await DropboxOAuth2Helper.ProcessCodeFlowAsync(code,appkey,appsecret);
						if(string.IsNullOrEmpty(resp.AccessToken)) {
							throw new Exception("Empty token returned by Dropbox.");
						}
						ret=resp.AccessToken;
					}
					catch(Exception ex) {
						ae=new ApplicationException(ex.Message,ex);
					}
					wait.Set();
				}).Start();
				wait.WaitOne(10000);
				if(ae!=null) {
					throw ae;
				}
				return ret;
			}
			
			///<summary>Asynchronous.  Uploads the file using the passed in bytes for the passed in path and name.</summary>
			public static void UploadAsync(TaskStateUpload stateIn) {
				if(stateIn.FileContent.Length>=MAX_FILE_SIZE_BYTES) {
					UploadLargeFileAsync(stateIn);
					return;
				}
				new System.Threading.Tasks.Task(async () => {
					try {
						stateIn.OnProgress(0,Lans.g("Dropbox","Uploading in progress"),stateIn.FileContent.Length/(double)1024/(double)1024,"");						
						using(MemoryStream stream=new MemoryStream(stateIn.FileContent)) {
							FileMetadata data=await stateIn.Client.Files.UploadAsync(ODFileUtils.CombinePaths(stateIn.Folder,stateIn.FileName,'/'),WriteMode.Overwrite.Instance,body: stream);
							stateIn.FileId=data.Id;
						}
						stateIn.OnProgress(0,"",0,"");//This will automatically close the FormProgress window.		
					}
					catch(Exception e) {
						stateIn.Error=e;
						stateIn.OnProgress(0,"",100,"");
					}
					finally {
						stateIn.IsDone=true;	
					}
				}).Start();
			}
			
			///<summary>Synchronous.  Uploads the file using the passed in bytes for the passed in path and name.
			///Waits a minimum of 10 seconds before timing out.  Could be longer depending on internet connection.</summary>
			public static void Upload(TaskStateUpload stateIn) {
				ManualResetEvent wait=new ManualResetEvent(false);
				if(stateIn.FileContent.Length>=MAX_FILE_SIZE_BYTES) {
					UploadLargeFile(stateIn);
					return;
				}
				new System.Threading.Tasks.Task(async () => {
					try {				
						using(MemoryStream stream=new MemoryStream(stateIn.FileContent)) {
							FileMetadata data=await stateIn.Client.Files.UploadAsync(ODFileUtils.CombinePaths(stateIn.Folder,stateIn.FileName,'/'),WriteMode.Overwrite.Instance,body: stream);
							stateIn.FileId=data.Id;
						}
					}
					catch(Exception e) {
						stateIn.Error=e;
					}
					finally {
						stateIn.IsDone=true;
						wait.Set();
					}
				}).Start();
				if(!wait.WaitOne(10000)) {
					throw new Exception(Lans.g("Dropbox","Uploading file to Dropbox timed out."));
				}
			}
			
			///<summary>Only called by Upload() when file is too large.</summary>
			private static void UploadLargeFileAsync(TaskStateUpload stateIn) {
				new System.Threading.Tasks.Task(async (stateObj) => {
					TaskStateUpload state=(TaskStateUpload)stateObj;
					try {
						int numOfChunks=stateIn.FileContent.Length/MAX_FILE_SIZE_BYTES+1;//Add 1 so that we are under the max file size limit, since an integer will truncate remainders.
						int chunkSize=stateIn.FileContent.Length/numOfChunks;
						string sessionId=null;
						state.OnProgress(0,Lans.g("Dropbox","Uploading in progress"),state.FileContent.Length/(double)1024/(double)1024,"");
						int index=0;
						for(int i=1;i<=numOfChunks;i++) {						
							if(state.DoCancel) {
								throw new Exception(Lans.g("Dropbox","Operation cancelled by user"));
							}
							bool lastChunk=i==numOfChunks;
							int curChunkSize=chunkSize;
							if(lastChunk) {
								curChunkSize=stateIn.FileContent.Length-index;
							}
							using(MemoryStream memStream=new MemoryStream(stateIn.FileContent,index,curChunkSize)) {
								if(i==1) {
									UploadSessionStartResult result=await state.Client.Files.UploadSessionStartAsync(false,memStream);
									sessionId=result.SessionId;
								}
								else {
									UploadSessionCursor cursor=new UploadSessionCursor(sessionId,(ulong)index);
									if(lastChunk) {
										FileMetadata data=await state.Client.Files.UploadSessionFinishAsync(cursor
											,new CommitInfo(ODFileUtils.CombinePaths(state.Folder,state.FileName,'/'),WriteMode.Overwrite.Instance)
											,memStream);
										state.FileId=data.Id;
									}
									else {
										await state.Client.Files.UploadSessionAppendV2Async(cursor,false,memStream);
									}
								}
								index+=curChunkSize;
							}
							state.OnProgress((double)(chunkSize*i)/(double)1024/(double)1024,"?currentVal MB of ?maxVal MB uploaded",(double)state.FileContent.Length/(double)1024/(double)1024,"");
						}
						state.OnProgress(0,"",0,"");//This will automatically close the FormProgress window.
					}
					catch(Exception e) {
						state.Error=e;
						state.OnProgress(0,"",100,Lans.g("Dropbox","Error")+": "+e.Message);
					}
					finally {
						state.IsDone=true;						
					}
				},stateIn).Start();
			}
			
			///<summary>Synchronous.  Only called by Upload() when file is too large.</summary>
			private static void UploadLargeFile(TaskStateUpload stateIn) {
				ManualResetEvent wait=new ManualResetEvent(false);
				new System.Threading.Tasks.Task(async (stateObj) => {
					TaskStateUpload state=(TaskStateUpload)stateObj;
					try {
						int numOfChunks=stateIn.FileContent.Length/MAX_FILE_SIZE_BYTES+1;//Add 1 so that we are under the max file size limit, since an integer will truncate remainders.
						int chunkSize=stateIn.FileContent.Length/numOfChunks;
						string sessionId=null;
						int index=0;
						for(int i=1;i<=numOfChunks;i++) {						
							if(state.DoCancel) {
								throw new Exception(Lans.g("Dropbox","Operation cancelled by user"));
							}
							bool lastChunk=i==numOfChunks;
							int curChunkSize=chunkSize;
							if(lastChunk) {
								curChunkSize=stateIn.FileContent.Length-index;
							}
							using(MemoryStream memStream=new MemoryStream(stateIn.FileContent,index,curChunkSize)) {
								if(i==1) {
									UploadSessionStartResult result=await state.Client.Files.UploadSessionStartAsync(false,memStream);
									sessionId=result.SessionId;
								}
								else {
									UploadSessionCursor cursor=new UploadSessionCursor(sessionId,(ulong)index);
									if(lastChunk) {
										FileMetadata data=await state.Client.Files.UploadSessionFinishAsync(cursor
											,new CommitInfo(ODFileUtils.CombinePaths(state.Folder,state.FileName,'/'),WriteMode.Overwrite.Instance)
											,memStream);
										state.FileId=data.Id;
									}
									else {
										await state.Client.Files.UploadSessionAppendV2Async(cursor,false,memStream);
									}
								}
								index+=curChunkSize;
							}
						}
					}
					catch(Exception e) {
						state.Error=e;
					}
					finally {
						state.IsDone=true;
						wait.Set();	
					}
				},stateIn).Start();
				wait.WaitOne(-1);
			}
			
			///<summary>Asynchronous.  Downloads the file using the passed in file path and name.</summary>
			public static void DownloadAsync(TaskStateDownload stateIn) {				
				new System.Threading.Tasks.Task(async () => {
					try {
						IDownloadResponse<FileMetadata> response;
						if(stateIn.UseId) {
							response=await stateIn.Client.Files.DownloadAsync("id:"+stateIn.FileId);
						}
						else {
							response=await stateIn.Client.Files.DownloadAsync(ODFileUtils.CombinePaths(stateIn.Folder,stateIn.FileName,'/'));
						}
						stateIn.DownloadFileSize=response.Response.Size;
						ulong numChunks=stateIn.DownloadFileSize/MAX_FILE_SIZE_BYTES+1;
						int chunkSize=(int)stateIn.DownloadFileSize/(int)numChunks;
						byte[] buffer=new byte[chunkSize];
						byte[] finalBuffer=new byte[stateIn.DownloadFileSize];
						stateIn.OnProgress(0,Lans.g("Dropbox","Downloading in progress"),stateIn.DownloadFileSize,"");
						int index=0;
						using(Stream stream=await response.GetContentAsStreamAsync()) {
							int length=0;
							stateIn.FileId=response.Response.Id;
							do {
								if(stateIn.DoCancel) {
									throw new Exception(Lans.g("Dropbox","Operation cancelled by user"));
								}
								length=stream.Read(buffer,0,chunkSize);
								//Convert each chunk to a MemoryStream. This plays nicely with garbage collection.
								using(MemoryStream memstream=new MemoryStream()) {
									memstream.Write(buffer,0,length);
									Array.Copy(memstream.ToArray(),0,finalBuffer,index,length);
									index+=length;
									stateIn.OnProgress((double)index/(double)1024/(double)1024,"?currentVal MB of ?maxVal MB downloaded",(double)stateIn.DownloadFileSize/(double)1024/(double)1024,"");
								}								
							} while(length>0);
						}
						stateIn.FileContent=finalBuffer;
						stateIn.OnProgress(0,"",0,"");//This will automatically close the FormProgress window.		
					}
					catch(Exception e) {
						stateIn.Error=e;
						stateIn.OnProgress(0,"",100,Lans.g("Dropbox","Error")+": "+e.Message);
					}
					finally {
						stateIn.IsDone=true;						
					}
				}).Start();
			}
			
			///<summary>Synchronous.  Downloads the file using the passed in file path and name.</summary>
			public static void Download(TaskStateDownload stateIn) {
				ManualResetEvent wait=new ManualResetEvent(false);
				new System.Threading.Tasks.Task(async () => {
					try {
						IDownloadResponse<FileMetadata> response;
						if(stateIn.UseId) {
							response=await stateIn.Client.Files.DownloadAsync("id:"+stateIn.FileId);
						}
						else {
							response=await stateIn.Client.Files.DownloadAsync(ODFileUtils.CombinePaths(stateIn.Folder,stateIn.FileName,'/'));
						}
						stateIn.DownloadFileSize=response.Response.Size;
						ulong numChunks=stateIn.DownloadFileSize/MAX_FILE_SIZE_BYTES+1;
						int chunkSize=(int)stateIn.DownloadFileSize/(int)numChunks;
						byte[] buffer=new byte[chunkSize];
						byte[] finalBuffer=new byte[stateIn.DownloadFileSize];
						int index=0;
						using(Stream stream=await response.GetContentAsStreamAsync()) {
							int length=0;
							stateIn.FileId=response.Response.Id;
							do {
								if(stateIn.DoCancel) {
									throw new Exception(Lans.g("Dropbox","Operation cancelled by user"));
								}
								length=stream.Read(buffer,0,chunkSize);
								//Convert each chunk to a MemoryStream. This plays nicely with garbage collection.
								using(MemoryStream memstream=new MemoryStream()) {
									memstream.Write(buffer,0,length);
									Array.Copy(memstream.ToArray(),0,finalBuffer,index,length);
									index+=length;
								}
							} while(length>0);
						}
						stateIn.FileContent=finalBuffer;
					}
					catch(Exception e) {
						stateIn.Error=e;
					}
					finally {
						stateIn.IsDone=true;
						wait.Set();
					}
				}).Start();
				wait.WaitOne(-1);
			}
			
			///<summary>Synchronous.  If path is a folder, all contents will be deleted.  Returns the I.D. for any file that was deleted.</summary>
			public static void Delete(TaskStateDelete stateIn) {
				ManualResetEvent wait=new ManualResetEvent(false);
				List<string> listContentIds=new List<string>();
				//jsalmon - We removed keeping track of folder IDs because Josh and I agreed that they are useless at this time.
				//if(!path.Substring(path.LastIndexOf('/')+1).Contains(".")) {//If this path is a folder
				//	TaskStateListFolders stateList=new TaskStateListFolders();
				//	stateList.Client=stateIn.Client;
				//	ListFolderContents(stateList,path);
				//	listContentIds=stateList.ListFileIds;
				//}
				new System.Threading.Tasks.Task(async () => {
					try {
						Metadata data=await stateIn.Client.Files.DeleteAsync(stateIn.Path);
						if(data.IsFile) {
							stateIn.AddToListFileId(data.AsFile.Id);
						}
						//jsalmon - We removed keeping track of folder IDs because Josh and I agreed that they are useless at this time.
						//else if(data.IsFolder && listContentIds.Count>0) {
						//	foreach(string id in listContentIds) {
						//		stateIn.AddToListFileId(id);
						//	}
						//}
					}
					catch(Exception e) {
						stateIn.Error=e;
					}
					finally {
						stateIn.IsDone=true;
					}
					wait.Set();
				}).Start();
				wait.WaitOne(-1);
			}

			///<summary>Files that are over 20MB large will not return thumbnails.
			///Waits a minimum of 10 seconds before timing out.  Could be longer depending on internet connection.</summary>
			public static void GetThumbnail(TaskStateThumbnail stateIn) {
				ManualResetEvent wait=new ManualResetEvent(false);
				new System.Threading.Tasks.Task(async () => {
					try {
						IDownloadResponse<FileMetadata> data=await stateIn.Client.Files.GetThumbnailAsync(stateIn.FilePath);
						stateIn.FileContent=await data.GetContentAsByteArrayAsync();
					}
					catch(Exception e) {
						stateIn.Error=e;
					}
					finally {
						stateIn.IsDone=true;
					}
					wait.Set();
				}).Start();
				if(!wait.WaitOne(10000)) {
					throw new Exception(Lans.g("Dropbox","Getting thumbnail from Dropbox timed out."));
				}
			}
			
			///<summary>Synchronous.  Returns the path of every file and folder for the passed in path.  
			///Only returns the unique I.D. for files.
			///Waits a minimum of 10 seconds before timing out.  Could be longer depending on internet connection.</summary>
			public static void ListFolderContents(TaskStateListFolders stateIn) {
				ManualResetEvent wait=new ManualResetEvent(false);
				new System.Threading.Tasks.Task(async () => {
					try {
						ListFolderResult data=await stateIn.Client.Files.ListFolderAsync(stateIn.FolderPath);
						stateIn.ListFolderPathLower=data.Entries.Select(x => x.PathLower).ToList();
						stateIn.ListFolderPathsDisplay=data.Entries.Select(x => x.PathDisplay).ToList();
						List<string> listIds=new List<string>();
						foreach(Metadata dataCur in data.Entries) {
							if(dataCur.IsFile) {
								listIds.Add(dataCur.AsFile.Id);
							}
							//jsalmon - We removed keeping track of folder IDs because Josh and I agreed that they are useless at this time.
							//else if(dataCur.IsFolder) {
							//	listIds.Add(dataCur.AsFolder.Id);
							//}
						}
						stateIn.ListFileIds=listIds;
					}
					catch(Exception e) {
						stateIn.Error=e;
					}
					finally {
						stateIn.IsDone=true;
					}
					wait.Set();
				}).Start();
				if(!wait.WaitOne(10000)) {
					throw new Exception(Lans.g("Dropbox","Getting folder contents from Dropbox timed out."));
				}
			}
			
			///<summary>Asynchronous.  Moves the fromPath's contents to the toPath's location.  
			///This will not work if the toPath location already exists.  This means if passing in two folders, the toPath folder can't exist.
			///If trying to move a file, both paths must be a file path (i.e fromPath="/AtoZ/A/Albus/testfile.txt", toPath="/AtoZ/P/Peterson/testfile.txt".
			///If one path is a folder, and the other is a file, it will fail.
			///If toPath is a folder, it will attempt to move the entire directory with all of its contents along with it.</summary>
			public static void MoveAsync(TaskStateMove stateIn) {
				TaskStateListFolders stateList=new TaskStateListFolders();
				stateList.Client=stateIn.Client;
				stateList.FolderPath=stateIn.FromPath;
				ListFolderContents(stateList);
				//If there isn't a file extension, it is a subdirectory and we need to exclude it from the list.
				List<string> listFilePaths=stateList.ListFolderPathLower.FindAll(x => x.Substring(x.LastIndexOf('/')+1).Contains("."));
				stateIn.CountMoveTotal=listFilePaths.Count;
				for(int i=0;i<stateIn.CountMoveTotal;i++) {
					if(stateIn.DoCancel) {
						return;
					}
					string path=listFilePaths[i];
					try {
						//LastIndexOf '/' works because there will never be a '/' at the end of a path, even if it is a subfolder.
						string fileName=path.Substring(path.LastIndexOf('/')+1);
						string toPathFull=ODFileUtils.CombinePaths(stateIn.ToPath,fileName,'/');
						if(FileExists(stateIn.Client,toPathFull)) {
							throw new Exception();//Throw so that we can iterate CountMoveFailed
						}
						MoveFile(stateIn,ODFileUtils.CombinePaths(stateIn.FromPath,fileName,'/'),toPathFull);
						stateIn.CountMoveSuccess++;
						stateIn.OnProgress(i+1,"?currentVal files of ?maxVal files moved",stateIn.CountMoveTotal,"");
					}
					catch(Exception) {
						stateIn.CountMoveFailed++;
					}
				}
			}
			
			///<summary>Synchronous.  The folder of the corresponding file to be downloaded.  
			///fromPath and toPath should be specific file paths, not folder paths.</summary>
			private static bool MoveFile(TaskStateMove stateIn,string fromPath,string toPath) {
				bool retVal=false;
					ManualResetEvent wait=new ManualResetEvent(false);
					new System.Threading.Tasks.Task(async () => {
						try {
							Metadata data=await stateIn.Client.Files.MoveAsync(fromPath,toPath);
							stateIn.AddToListFileId(data.AsFile.Id);
							retVal=true;
						}
						catch(Exception) {
						}
						wait.Set();
					}).Start();
					wait.WaitOne(-1);
				return retVal;
			}
			
			///<summary>Synchronous.  Returns true if a file exists in the passed in filePath</summary>
			public static bool FileExists(DropboxClient client,string filePath) {
				bool retVal=false;
				ManualResetEvent wait=new ManualResetEvent(false);
				new System.Threading.Tasks.Task(async () => {
					try {
						Metadata data=await client.Files.GetMetadataAsync(filePath);
						retVal=true;
					}
					catch(Exception) {
					}
					wait.Set();
				}).Start();
				if(!wait.WaitOne(10000)) {
					throw new Exception(Lans.g("Dropbox","Checking if file exists in Dropbox timed out."));
				}
				return retVal;
			}

		}

		public abstract class TaskState {

			public DropboxClient Client;
			public ProgressHandler OnProgress;
			public bool IsDone;
			public Exception Error;

			private object _lock=new object();
			private bool _doCancel=false;
			
			///<summary>A quick identifier for whether or not an error was thrown.  It is much easier than a null check when error handling.</summary>
			public bool HasError {
				get {
					return Error!=null;
				}
			}
			
			///<summary>This property allows the user with this TaskState to cancel an async task if they so choose.  
			///This is usually wired up to a Cancel button in a progress form.</summary>
			public bool DoCancel {
				get {
					bool doCancel=false;
					lock(_lock) {
						doCancel=_doCancel;
					}
					return doCancel;
				}
				set {
					lock(_lock) {
						_doCancel=value;
					}
				}
			}
		}

		public class TaskStateUpload : TaskState {

			public TaskStateUpload(DropboxClient client) {
				Client=client;
			}

			private object _lock=new object();
			private string _folder;
			private string _fileName;
			private byte[] _fileContent;
			private string _fileId;
			
			///<summary>The folder of the corresponding file to be downloaded</summary>
			public string Folder {
				get {
					string folder="";
					lock(_lock) {
						folder=_folder;
					}
					return folder;
				}
				set {
					lock(_lock) {
						_folder=value;
					}
				}
			}
			
			///<summary>The file name of the file to be downloaded.</summary>
			public string FileName {
				get {
					string fileName="";
					lock(_lock) {
						fileName=_fileName;
					}
					return fileName;
				}
				set {
					lock(_lock) {
						_fileName=value;
					}
				}
			}
			
			///<summary>The file stored in bytes.  This value will grow while the download is still in progress.</summary>
			public byte[] FileContent {
				get {
					byte[] fileContent=new byte[1];
					lock(_lock) {
						fileContent=_fileContent;
					}
					return fileContent;
				}
				set {
					lock(_lock) {
						_fileContent=value;
					}
				}
			}

			///<summary>Uniquely identifies this file in the Dropbox account.</summary>
			public string FileId {
				get {
					string fileId="";
					lock(_lock) {
						fileId=_fileId;
					}
					return fileId;
				}
				set {
					lock(_lock) {
						_fileId=value;
					}
				}
			}
		}

		public class TaskStateDownload : TaskState {

			public TaskStateDownload(DropboxClient client) {
				Client=client;
			}

			private object _lock=new object();
			private string _folder;
			private string _fileName;
			private byte[] _fileContent=new byte[1];
			private string _fileId;
			private ulong _downloadFileSize;
			private bool _useId;
			
			///<summary>The folder of the corresponding file to be downloaded</summary>
			public string Folder {
				get {
					string folder="";
					lock(_lock) {
						folder=_folder;
					}
					return folder;
				}
				set {
					lock(_lock) {
						_folder=value;
					}
				}
			}
			
			///<summary>The file name of the file to be downloaded.</summary>
			public string FileName {
				get {
					string fileName="";
					lock(_lock) {
						fileName=_fileName;
					}
					return fileName;
				}
				set {
					lock(_lock) {
						_fileName=value;
					}
				}
			}
			
			///<summary>The file stored in bytes.  This value will grow while the download is still in progress.</summary>
			public byte[] FileContent {
				get {
					byte[] fileContent=new byte[1];
					lock(_lock) {
						fileContent=_fileContent;
					}
					return fileContent;
				}
				set {
					lock(_lock) {
						_fileContent=value;
					}
				}
			}
			
			///<summary>Uniquely identifies this file in the Dropbox account.</summary>
			public string FileId {
				get {
					string fileId="";
					lock(_lock) {
						fileId=_fileId;
					}
					return fileId;
				}
				set {
					lock(_lock) {
						_fileId=value;
					}
				}
			}
			
			///<summary>The total size of the file that is being downloaded.</summary>
			public ulong DownloadFileSize {
				get {
					ulong downloadFileSize=0;
					lock(_lock) {
						downloadFileSize=_downloadFileSize;
					}
					return downloadFileSize;
				}
				set {
					lock(_lock) {
						_downloadFileSize=value;
					}
				}
			}

			public bool UseId {
				get {
					bool useId=false;
					lock(_lock) {
						useId=_useId;
					}
					return useId;
				}
				set {
					lock(_lock) {
						_useId=value;
					}
				}
			}

		}

		public class TaskStateListFolders : TaskState {

			//This will not have a Client that is directly linked to this TaskState and must be linked separately.
			public TaskStateListFolders() {

			}

			public TaskStateListFolders(DropboxClient client) {
				Client=client;
			}

			private object _lock=new object();
			private List<string> _listFolderPathDisplay=new List<string>();
			private List<string> _listFolderPathLower=new List<string>();
			private List<string> _listFileIds=new List<string>();
			private string _folderPath;
			
			///<summary>The folder of the corresponding file to be downloaded</summary>
			public string FolderPath {
				get {
					string folderPath="";
					lock(_lock) {
						folderPath=_folderPath;
					}
					return folderPath;
				}
				set {
					lock(_lock) {
						_folderPath=value;
					}
				}
			}
			
			///<summary>List of cased paths that were found in </summary>
			public List<string> ListFolderPathsDisplay {
				get {
					List<string> listFolderPaths=new List<string>();
					lock(_lock) {
						foreach(string path in _listFolderPathDisplay) {
							listFolderPaths.Add(path);
						}
					}
					return listFolderPaths;
				}
				set {
					lock(_lock) {
						_listFolderPathDisplay=value;
					}
				}
			}
			
			///<summary>List of folder and file paths that were found for the given folder path.
			///PathLower is the preferred path for making transactions between Dropbox and Open Dental, as PathDisplay is used for UI.</summary>
			public List<string> ListFolderPathLower {
				get {
					List<string> listFolderPaths=new List<string>();
					lock(_lock) {
						foreach(string path in _listFolderPathLower) {
							listFolderPaths.Add(path);
						}
					}
					return listFolderPaths;
				}
				set {
					lock(_lock) {
						_listFolderPathLower=value;
					}
				}
			}

			///<summary>List of file ids that were found for the given folder path.</summary>
			public List<string> ListFileIds {
				get {
					List<string> listFileIds=new List<string>();
					lock(_lock) {
						foreach(string path in _listFileIds) {
							listFileIds.Add(path);
						}
					}
					return listFileIds;
				}
				set {
					lock(_lock) {
						_listFileIds=value;
					}
				}
			}

		}

		public class TaskStateMove : TaskState {

			public TaskStateMove(DropboxClient client) {
				Client=client;
			}

			private object _lock=new object();
			private List<string> _listFileIds=new List<string>();
			private int _countMoveFailed=0;
			private int _countMoveSuccess=0;
			private int _countMoveTotal=0;
			private string _fromPath;
			private string _toPath;
			
			///<summary>The folder of the corresponding file to be downloaded</summary>
			public string FromPath {
				get {
					string fromPath="";
					lock(_lock) {
						fromPath=_fromPath;
					}
					return fromPath;
				}
				set {
					lock(_lock) {
						_fromPath=value;
					}
				}
			}
			
			///<summary>The folder of the corresponding file to be downloaded</summary>
			public string ToPath {
				get {
					string toPath="";
					lock(_lock) {
						toPath=_toPath;
					}
					return toPath;
				}
				set {
					lock(_lock) {
						_toPath=value;
					}
				}
			}
			
			///<summary>Number of move attempts that failed and are still in the original folder.</summary>
			public int CountMoveFailed {
				get {
					int countMoveFailed=0;
					lock(_lock) {
						countMoveFailed=_countMoveFailed;
					}
					return countMoveFailed;
				}
				set {
					lock(_lock) {
						_countMoveFailed=value;
					}
				}
			}
			
			///<summary>Number of move attempts that succeeded and have been removed from the original folder.</summary>
			public int CountMoveSuccess {
				get {
					int countMoveSuccess=0;
					lock(_lock) {
						countMoveSuccess=_countMoveSuccess;
					}
					return countMoveSuccess;
				}
				set {
					lock(_lock) {
						_countMoveSuccess=value;
					}
				}
			}
			
			///<summary>Number of total files to move from the original folder.</summary>
			public int CountMoveTotal {
				get {
					int countMoveTotal=0;
					lock(_lock) {
						countMoveTotal=_countMoveTotal;
					}
					return countMoveTotal;
				}
				set {
					lock(_lock) {
						_countMoveTotal=value;
					}
				}
			}

			///<summary>List of file ids that were successfully moved.</summary>
			public List<string> ListFileIds {
				get {
					List<string> listFileIds=new List<string>();
					lock(_lock) {
						foreach(string path in _listFileIds) {
							listFileIds.Add(path);
						}
					}
					return listFileIds;
				}
				set {
					lock(_lock) {
						_listFileIds=value;
					}
				}
			}
			
			///<summary>Thread-safely adds the string to ListFieldIds.</summary>
			public void AddToListFileId(string item) {
				lock(_lock) {
					_listFileIds.Add(item);
				}
			}

		}

		public class TaskStateDelete : TaskState {

			public TaskStateDelete(DropboxClient client) {
				Client=client;
			}

			private object _lock=new object();
			private List<string> _listDeletedIds=new List<string>();
			private string _path;
			
			///<summary>The folder of the corresponding file to be downloaded</summary>
			public string Path {
				get {
					string path="";
					lock(_lock) {
						path=_path;
					}
					return path;
				}
				set {
					lock(_lock) {
						_path=value;
					}
				}
			}
			
			///<summary></summary>
			public List<string> ListDeletedIds {
				get {
					List<string> listDeletedIds=new List<string>();
					lock(_lock) {
						foreach(string path in _listDeletedIds) {
							listDeletedIds.Add(path);
						}
					}
					return listDeletedIds;
				}
				set {
					lock(_lock) {
						_listDeletedIds=value;
					}
				}
			}

			public void AddToListFileId(string item) {
				lock(_lock) {
					_listDeletedIds.Add(item);
				}
			}
			
			///<summary>This is a quick way to determine if there were complications with deleting the file.  
			///The Error variable can provide more info.</summary>
			public bool HasFailed {
				get {
					return (IsDone && Error!=null);
				}
			}

		}

		public class TaskStateThumbnail : TaskState {

			public TaskStateThumbnail(DropboxClient client) {
				Client=client;
			}

			private object _lock=new object();
			private byte[] _fileContent;
			private string _filePath;
			
			///<summary>The folder of the corresponding file to be downloaded</summary>
			public string FilePath {
				get {
					string filePath="";
					lock(_lock) {
						filePath=_filePath;
					}
					return filePath;
				}
				set {
					lock(_lock) {
						_filePath=value;
					}
				}
			}
			
			///<summary>The thumbnail file in bytes.</summary>
			public byte[] FileContent {
				get {
					byte[] fileContent=new byte[1];
					lock(_lock) {
						fileContent=_fileContent;
					}
					return fileContent;
				}
				set {
					lock(_lock) {
						_fileContent=value;
					}
				}
			}

		}

	}

}
