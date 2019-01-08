using System;
using System.Net;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Project.SITCA.HttpDownloadFTPUpload
{
    class ProgramS
    {
        public static void Main(string[] args)
        {
            //SITCA_upload INI 檔案位置與名稱與執行檔同一目錄
            string ini_fileName = @".\SITCA_upload.ini";
            //呼叫方法 get key values from INI file FTP伺服器 IP, ftpID,ftpPW, 檔名開頭名稱皆定義在INI檔
            //投信基金應負擔費用率資訊來源連結定義在 INI 檔中
            string SITCA_FUNDT = GetKeyValueString(ini_fileName, "SITCA_download_file", "SITCA_FUNDT");
            //境外基金應負擔費用率資訊來源連結定義在 INI 檔中
            string SITCA_FUNDO = GetKeyValueString(ini_fileName, "SITCA_download_file", "SITCA_FUNDO");
            // 國證內部FTP伺服器定義在 INI 檔中
            string ftp_site = GetKeyValueString(ini_fileName, "FTP_upload", "ftp_site");
            // 國證內部FTP伺服器登入ID定義在 INI 檔中
            string ftp_id = GetKeyValueString(ini_fileName, "FTP_upload", "ftp_id");
            // 國證內部FTP伺服器登入PW定義在 INI 檔中
            string ftp_pw = GetKeyValueString(ini_fileName, "FTP_upload", "ftp_pw");
            //投信基金應負擔費用率資訊前置檔名名稱定義在 INI 檔中
            string filename_prefix_T = GetKeyValueString(ini_fileName, "FTP_upload", "FUNDT");
            //境外基金應負擔費用率資訊前置檔名名稱定義在 INI 檔中
            string filename_prefix_O = GetKeyValueString(ini_fileName, "FTP_upload", "FUNDO");
            //呼叫方法 從SITCA http下載檔案再 FTP上傳國證內部FTP伺服器
            Get_http_upload_FTP(SITCA_FUNDT, ftp_site, ftp_id, ftp_pw, filename_prefix_T);
            //境外基金應負擔費用率資訊前置檔名名稱定義在 INI 檔中
            Get_http_upload_FTP(SITCA_FUNDO, ftp_site, ftp_id, ftp_pw, filename_prefix_O);
        }
        private static string GetKeyValueString(string ini_fileName, string Section, string Key)
        {
            // INI file get key values
            StringBuilder value = new StringBuilder(1024);
            bool hasSection = false;
            //開啟IO串流
            StreamReader ini_sr = new StreamReader(ini_fileName, Encoding.UTF8);
            while (true)
            {
                string s = ini_sr.ReadLine();
                //空值或空字串判斷
                if (s == null || s == "")
                {
                    continue;
                }
                //以;或是#開頭作註解的判斷
                if (Regex.Match(s, @"^(;|#).*$").Success)
                {
                    continue;
                }
                //讀取[Section]
                if (Regex.Match(s, @"^\[.*\]$").Success)
                {
                    //判斷是否有符合Section名稱
                    if (Regex.Match(s, Section).Success)
                    {
                        hasSection = true;
                    }
                }
                //有找到Section，再去判斷Key值，等號左右字串(trim一下)拆成(Spilt)兩個args[0]和args[1] KeyValue string[]
                if (hasSection)
                {
                    string[] KeyValue = s.Split('=');

                    //判斷Key名稱是否符合
                    if (Regex.Match(KeyValue[0].Trim(), Key).Success)
                    {
                        value.Append(KeyValue[1].Trim());
                        break;
                    }
                }
            }
            //關閉IO串流
            ini_sr.Close();

            return value.ToString();
        }

        private static string Get_http_upload_FTP(string d_path, string ftp_site, string ftp_id, string ftp_pw, string filename_prefix)
        {
            // function for stream read write to get csv file from http then write to ftp site
            try
            {
                string download_path = d_path;
                string line = string.Empty;  // initial line with empty

                DateTime _now = DateTime.Now;
                string Download_date = _now.ToString("yyyyMMdd");
                string filename = filename_prefix + Download_date + ".csv";

                // Create a new WebClient instance for File input - myStream 讀來源檔
                WebClient myWebClient = new WebClient();
                // Open a stream to point to the data stream (file) coming from the Web resource - Stream Reader "sr"
                Stream myStream = myWebClient.OpenRead(download_path);
                StreamReader sr = new System.IO.StreamReader(myStream, Encoding.Default);
                // Write input file stream to FTP server - FtpWebRequest Object "request" Write Stream "requestStream"

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftp_site + filename);
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(ftp_id, ftp_pw);
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = true;
                //   Readline from request object readline then write to Stream Writer  "requestStream"        
                Stream requestStream = request.GetRequestStream();
                while ((line = sr.ReadLine()) != null)
                {
                    byte[] fileContents = Encoding.UTF8.GetBytes(sr.ReadToEnd());
                    request.ContentLength = fileContents.Length;
                    requestStream.Write(fileContents, 0, fileContents.Length);
                }
                requestStream.Close();
                myStream.Close();
            }
            catch (Exception e)
            {
                return e.Message;
            }
            finally { }
            return "done";
        }
    }

}