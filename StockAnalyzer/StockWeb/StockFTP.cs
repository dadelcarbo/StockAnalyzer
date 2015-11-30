﻿using StockAnalyzer.StockLogging;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace StockAnalyzer.StockWeb
{
   public class StockFTP
   {
      private string host = null;
      private string user = null;
      private string pass = null;
      private FtpWebRequest ftpRequest = null;
      private FtpWebResponse ftpResponse = null;
      private Stream ftpStream = null;
      private int bufferSize = 2048;

      /* Construct Object */
      public StockFTP(string hostIP, string userName, string password) { host = hostIP; user = userName; pass = password; }

      /* Download File */
      public void download(string remoteFile, string localFile)
      {
         try
         {
            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
            /* Establish Return Communication with the FTP Server */
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            /* Get the FTP Server's Response Stream */
            ftpStream = ftpResponse.GetResponseStream();
            /* Open a File Stream to Write the Downloaded File */
            FileStream localFileStream = new FileStream(localFile, FileMode.Create);
            /* Buffer for the Downloaded Data */
            byte[] byteBuffer = new byte[bufferSize];
            int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
            /* Download the File by Writing the Buffered Data Until the Transfer is Complete */
            try
            {
               while (bytesRead > 0)
               {
                  localFileStream.Write(byteBuffer, 0, bytesRead);
                  bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
               }
            }
            catch (Exception ex) { StockLog.Write(ex.ToString()); }
            /* Resource Cleanup */
            localFileStream.Close();
            ftpStream.Close();
            ftpResponse.Close();
            ftpRequest = null;
         }
         catch (Exception ex) { StockLog.Write(ex.ToString()); }
         return;
      }

      /* Upload Directory */
      public void uploadDirectory(string remoteDirectory, string localDirectory)
      {
         if (!this.createDirectory(remoteDirectory)) return;

         foreach (string file in Directory.GetFiles(localDirectory))
         {
            this.upload(remoteDirectory + "/" + Path.GetFileName(file), file);
         }

         foreach (string directory in Directory.GetDirectories(localDirectory))
         {
            string dir = directory.Split('\\').Last();
            this.uploadDirectory(remoteDirectory + "/" + dir, localDirectory + "\\" + dir);
         }

      }
      /* Upload File */
      public void upload(string remoteFile, string localFile)
      {
         try
         {
            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + remoteFile);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
            /* Establish Return Communication with the FTP Server */
            ftpStream = ftpRequest.GetRequestStream();
            /* Open a File Stream to Read the File for Upload */
            FileStream localFileStream = new FileStream(localFile, FileMode.Open);
            /* Buffer for the Downloaded Data */
            byte[] byteBuffer = new byte[bufferSize];
            int bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
            /* Upload the File by Sending the Buffered Data Until the Transfer is Complete */
            try
            {
               while (bytesSent != 0)
               {
                  ftpStream.Write(byteBuffer, 0, bytesSent);
                  bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
               }
            }
            catch (Exception ex) { StockLog.Write(ex.ToString()); }
            /* Resource Cleanup */
            localFileStream.Close();
            ftpStream.Close();
            ftpRequest = null;
         }
         catch (Exception ex) { StockLog.Write(ex.ToString()); }
         return;
      }

      /* Delete File */
      public void delete(string deleteFile)
      {
         try
         {
            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + deleteFile);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
            /* Establish Return Communication with the FTP Server */
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            /* Resource Cleanup */
            ftpResponse.Close();
            ftpRequest = null;
         }
         catch (Exception ex) { StockLog.Write(ex.ToString()); }
         return;
      }

      /* Rename File */
      public void rename(string currentFileNameAndPath, string newFileName)
      {
         try
         {
            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + currentFileNameAndPath);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.Rename;
            /* Rename the File */
            ftpRequest.RenameTo = newFileName;
            /* Establish Return Communication with the FTP Server */
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            /* Resource Cleanup */
            ftpResponse.Close();
            ftpRequest = null;
         }
         catch (Exception ex) { StockLog.Write(ex.ToString()); }
         return;
      }

      /* Create a New Directory on the FTP Server */
      public bool createDirectory(string newDirectory)
      {
         bool result = true;
         try
         {
            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)WebRequest.Create(host + "/" + newDirectory);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
            /* Establish Return Communication with the FTP Server */
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            /* Resource Cleanup */
            ftpResponse.Close();
            ftpRequest = null;
         }
         catch (Exception ex)
         {
            StockLog.Write(ex.ToString());
            result = false;
         }
         return result;
      }

      /* Get the Date/Time a File was Created */
      public string getFileCreatedDateTime(string fileName)
      {
         try
         {
            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;
            /* Establish Return Communication with the FTP Server */
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            /* Establish Return Communication with the FTP Server */
            ftpStream = ftpResponse.GetResponseStream();
            /* Get the FTP Server's Response Stream */
            StreamReader ftpReader = new StreamReader(ftpStream);
            /* Store the Raw Response */
            string fileInfo = null;
            /* Read the Full Response Stream */
            try { fileInfo = ftpReader.ReadToEnd(); }
            catch (Exception ex) { StockLog.Write(ex.ToString()); }
            /* Resource Cleanup */
            ftpReader.Close();
            ftpStream.Close();
            ftpResponse.Close();
            ftpRequest = null;
            /* Return File Created Date Time */
            return fileInfo;
         }
         catch (Exception ex) { StockLog.Write(ex.ToString()); }
         /* Return an Empty string Array if an Exception Occurs */
         return "";
      }

      /* Get the Size of a File */
      public string getFileSize(string fileName)
      {
         try
         {
            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + fileName);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
            /* Establish Return Communication with the FTP Server */
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            /* Establish Return Communication with the FTP Server */
            ftpStream = ftpResponse.GetResponseStream();
            /* Get the FTP Server's Response Stream */
            StreamReader ftpReader = new StreamReader(ftpStream);
            /* Store the Raw Response */
            string fileInfo = null;
            /* Read the Full Response Stream */
            try { while (ftpReader.Peek() != -1) { fileInfo = ftpReader.ReadToEnd(); } }
            catch (Exception ex) { StockLog.Write(ex.ToString()); }
            /* Resource Cleanup */
            ftpReader.Close();
            ftpStream.Close();
            ftpResponse.Close();
            ftpRequest = null;
            /* Return File Size */
            return fileInfo;
         }
         catch (Exception ex) { StockLog.Write(ex.ToString()); }
         /* Return an Empty string Array if an Exception Occurs */
         return "";
      }

      /* List Directory Contents File/Folder Name Only */
      public string[] directoryListSimple(string directory)
      {
         try
         {
            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            /* Establish Return Communication with the FTP Server */
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            /* Establish Return Communication with the FTP Server */
            ftpStream = ftpResponse.GetResponseStream();
            /* Get the FTP Server's Response Stream */
            StreamReader ftpReader = new StreamReader(ftpStream);
            /* Store the Raw Response */
            string directoryRaw = null;
            /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
            try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
            catch (Exception ex) { StockLog.Write(ex.ToString()); }
            /* Resource Cleanup */
            ftpReader.Close();
            ftpStream.Close();
            ftpResponse.Close();
            ftpRequest = null;
            /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
            try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
            catch (Exception ex) { StockLog.Write(ex.ToString()); }
         }
         catch (Exception ex) { StockLog.Write(ex.ToString()); }
         /* Return an Empty string Array if an Exception Occurs */
         return new string[] { "" };
      }

      /* List Directory Contents in Detail (Name, Size, Created, etc.) */
      public string[] directoryListDetailed(string directory)
      {
         try
         {
            /* Create an FTP Request */
            ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host + "/" + directory);
            /* Log in to the FTP Server with the User Name and Password Provided */
            ftpRequest.Credentials = new NetworkCredential(user, pass);
            /* When in doubt, use these options */
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;
            /* Specify the Type of FTP Request */
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            /* Establish Return Communication with the FTP Server */
            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            /* Establish Return Communication with the FTP Server */
            ftpStream = ftpResponse.GetResponseStream();
            /* Get the FTP Server's Response Stream */
            StreamReader ftpReader = new StreamReader(ftpStream);
            /* Store the Raw Response */
            string directoryRaw = null;
            /* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
            try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
            catch (Exception ex) { StockLog.Write(ex.ToString()); }
            /* Resource Cleanup */
            ftpReader.Close();
            ftpStream.Close();
            ftpResponse.Close();
            ftpRequest = null;
            /* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
            try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
            catch (Exception ex) { StockLog.Write(ex.ToString()); }
         }
         catch (Exception ex) { StockLog.Write(ex.ToString()); }
         /* Return an Empty string Array if an Exception Occurs */
         return new string[] { "" };
      }
   }
}
