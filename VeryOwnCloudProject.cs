/*myVeryOwnCloud provides a encrypted mySQL based backend that is interfaced by a c# cookie aware web client.
Copyright(C) 2021 Emanuel Bierschneider

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.*/
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing.Imaging;

namespace WindowsFormsApplication2
{
    public partial class VeryOwnCloudProject : Form
    {

        static string uploadPath = (Path.GetTempPath() + "akmUpl.jpg");
        static string dwnlPath = Path.GetTempPath() + "akmDwnl.jpg";
        static Int64 lastId = 0;
        static bool TimTickEnabled = false;
        public delegate System.Threading.Tasks.Task<object> fPointer(object[] parameters); // point to every functions that it has void as return value and with no input parameter
        public VeryOwnCloudProject()
        {
            InitializeComponent();
            loadApp();
        }
        public string getUUID()
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "CMD.exe";
            startInfo.Arguments = "/C wmic csproduct get UUID";
            process.StartInfo = startInfo;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit();
            string output = process.StandardOutput.ReadToEnd();
            return output;
        }

        public string EncryptString(string plainText, byte[] key, byte[] iv)
        {
            // Instantiate a new Aes object to perform string symmetric encryption
            Aes encryptor = Aes.Create();

            encryptor.Mode = CipherMode.CBC;
            //encryptor.KeySize = 256;
            //encryptor.BlockSize = 128;
            //encryptor.Padding = PaddingMode.Zeros;

            // Set key and IV
            encryptor.Key = key;
            encryptor.IV = iv;

            // Instantiate a new MemoryStream object to contain the encrypted bytes
            MemoryStream memoryStream = new MemoryStream();

            // Instantiate a new encryptor from our Aes object
            ICryptoTransform aesEncryptor = encryptor.CreateEncryptor();

            // Instantiate a new CryptoStream object to process the data and write it to the 
            // memory stream
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write);

            // Convert the plainText string into a byte array
            byte[] plainBytes = Encoding.ASCII.GetBytes(plainText);

            // Encrypt the input plaintext string
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);

            // Complete the encryption process
            cryptoStream.FlushFinalBlock();

            // Convert the encrypted data from a MemoryStream to a byte array
            byte[] cipherBytes = memoryStream.ToArray();

            // Close both the MemoryStream and the CryptoStream
            memoryStream.Close();
            cryptoStream.Close();

            // Convert the encrypted byte array to a base64 encoded string
            string cipherText = Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);

            // Return the encrypted data as a string
            return cipherText;
        }

        public string DecryptString(string cipherText, byte[] key, byte[] iv, int orgLength)
        {
            // Instantiate a new Aes object to perform string symmetric encryption
            Aes encryptor = Aes.Create();

            encryptor.Mode = CipherMode.CBC;
            //encryptor.KeySize = 256;
            //encryptor.BlockSize = 256;
            //encryptor.Padding = PaddingMode.None;
            //encryptor.Padding = PaddingMode.Zeros;

            // Set key and IV
            encryptor.Key = key;
            encryptor.IV = iv;

            // Instantiate a new MemoryStream object to contain the encrypted bytes
            MemoryStream memoryStream = new MemoryStream();

            // Instantiate a new encryptor from our Aes object
            ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();

            // Instantiate a new CryptoStream object to process the data and write it to the 
            // memory stream
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write);

            // Will contain decrypted plaintext
            string plainText = String.Empty;

            try
            {
                // Convert the ciphertext string into a byte array
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                // Decrypt the input ciphertext string
                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);

                // Complete the decryption process
                cryptoStream.FlushFinalBlock();

                // Convert the decrypted data from a MemoryStream to a byte array
                byte[] plainBytes = memoryStream.ToArray();

                // Convert the decrypted byte array to string
                plainText = Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);
            }
            finally
            {
                // Close both the MemoryStream and the CryptoStream
                memoryStream.Close();
                cryptoStream.Close();
            }

            // Return the decrypted data as a string
            return plainText;
        }
        public static async void WriteFileToPath(string input_path, string linestowrite)
        {
            string path = input_path;
            // This text is added only once to the file.
            try
            {
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(path, true))
                {
                    await file.WriteLineAsync(linestowrite);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR @ WriteFileToPath" + ex.ToString());
            }
        }

        public class CookieAwareWebClient : WebClient
        {
            public CookieAwareWebClient() : this(new CookieContainer())
            { }

            public CookieAwareWebClient(CookieContainer c)
            {
                this.CookieContainer = c;
                this.Headers.Add("Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                this.Headers.Add("Accept-Encoding: gzip, deflate, identity");
                this.Headers.Add("Accept-Language: de,en-US;q=0.7,en;q=0.3");
                this.Headers.Add("User-Agent: Mozilla / 5.0(Windows NT 10.0; Win64; x64; rv: 59.0) Gecko / 20100101 Firefox / 59.0");
            }

            public CookieContainer CookieContainer { get; set; }

            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                if (request is HttpWebRequest)
                {
                    (request as HttpWebRequest).CookieContainer = this.CookieContainer;
                }
                return request;
            }
        }

        //return result in specified length
        public string hex2Bin(string strHex, int bit)
        {
            int decNumber = hex2Dec(strHex);
            return dec2Bin(decNumber).PadLeft(bit, '0');
        }
        public string dec2Bin(int val, int bit)
        {
            return Convert.ToString(val, 2).PadLeft(bit, '0');
        }
        public string hex2Bin(string strHex)
        {
            int decNumber = hex2Dec(strHex);
            return dec2Bin(decNumber);
        }
        public string bin2Hex(string strBin)
        {
            int decNumber = bin2Dec(strBin);
            return dec2Hex(decNumber);
        }
        private string dec2Hex(int val)
        {
            return val.ToString("X");
        }
        private int hex2Dec(string strHex)
        {
            return Convert.ToInt16(strHex, 16);
        }
        private string dec2Bin(int val)
        {
            return Convert.ToString(val, 2);
        }
        public int bin2Dec(string strBin)
        {
            return Convert.ToInt16(strBin, 2);
        }
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        public static byte[] PackH(string hex)
        {
            if ((hex.Length % 2) == 1) hex += '0';
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
        public static Stream StringToStream(string src)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(src);
            return new MemoryStream(byteArray);
        }
        public static Stream ByteArrToStream(byte[] src)
        {
            return new MemoryStream(src);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string autoIncrementIndexOfData = cntAutoInc.ToString();
            string password = "";
            if (GlobalClass.TestSecretCloud() == true)
            {
                password = GlobalClass.ReadSecretCloud(); //Sha256
            }
            string InputString = "";
            //******************* INITIATE PHPSESSION ***************************
            NameValueCollection formData = new NameValueCollection();
            CookieAwareWebClient webClient = new CookieAwareWebClient();
            webClient.Encoding = System.Text.Encoding.Default;


            formData.Clear();
            formData["username"] = "admin";
            if (GlobalClass.TestPasswordCloud() == true)
            {
                formData["password"] = GlobalClass.ReadPasswordCloud(); //user pwd
            }
            byte[] responseBytes = webClient.UploadValues("http://your.url.here/lo.php", "POST", formData);
            string responseHTML = Encoding.UTF8.GetString(responseBytes);

            Uri uriStr;
            if (Uri.TryCreate("http://your.url.here", UriKind.RelativeOrAbsolute, out uriStr) == false)
            {
                System.Diagnostics.Debug.WriteLine("NO");
            }
            foreach (Cookie cookie in webClient.CookieContainer.GetCookies(uriStr))
            {
                System.Diagnostics.Debug.WriteLine(cookie.Name);
                System.Diagnostics.Debug.WriteLine(cookie.Value);
            }
            //******************* INITIATE PHPSESSION ***************************

            //******************* DOWNLOAD XML FILE LIST ***************************

            string fileList = webClient.DownloadString("http://your.url.here/filelist.php");
            DataSet ds = new DataSet();
            byte[] byteArray = Encoding.UTF8.GetBytes(fileList);
            ds.ReadXml(new MemoryStream(byteArray));
            if (ds.Tables.Count > 0)
            {
                var result = ds.Tables[0];
            }
            dataGridView1.DataSource = ds.Tables[0];
            //******************* DOWNLOAD XML FILE LIST ***************************

            //******************* DOWNLOAD IV ***************************
            byte[] ivArr = webClient.DownloadData("http://your.url.here/listiv.php?id=" + autoIncrementIndexOfData);
            //******************* DOWNLOAD IV ***************************

            //******************* DOWNLOAD LENGTH ***************************
            byte[] length = webClient.DownloadData("http://your.url.here/getLen.php?id=" + autoIncrementIndexOfData);
            //filesize from files
            int fileSize = 0;
            foreach (byte l in length)
            {
                fileSize += (byte)(l - (byte)(0x30));
                fileSize *= 10;
            }
            fileSize /= 10;
            System.Diagnostics.Debug.WriteLine("filesize: " + fileSize.ToString());
            //******************* DOWNLOAD LENGTH ***************************

            //******************* DOWNL DATA ***************************
            InputString = webClient.DownloadString("http://your.url.here/dwnl.php?id=" + autoIncrementIndexOfData);
            //******************* DOWNL DATA ***************************

            //******************* DOWNLOAD AS FILE ***************************
            webClient.DownloadFile("http://your.url.here/download.php?id=" + autoIncrementIndexOfData.ToString(), "C:\\your\\dir\\here\\akm2Raw.jpg");
            //******************* DOWNLOAD AS FILE ***************************

            ////******************* DOWNLOAD DOWNLOAD ***************************
            string InputStringDownload = webClient.DownloadString("http://your.url.here/download.php");
            ////******************* DOWNLOAD DOWNLOAD ***************************




            //******************* DECRYPT ***************************

            // Create sha256 hash
            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] key = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(password));  

            string decrypted = this.DecryptString(InputString, key, ivArr, fileSize);
            byte[] decryptedArr = Convert.FromBase64String(decrypted);
            //******************* DECRYPT ***************************

            //******************* CHECK SIGN ***************************
            Encoding encoding = Encoding.UTF8;
            DataTable dt = ds.Tables[0];
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (row["id"].ToString() == autoIncrementIndexOfData)
                    {
                        Debug.WriteLine(row["sign"].ToString());
                        bf.Serialize(ms, row["sign"]);
                    }
                }
                byte[] signObj = ms.ToArray();
            }
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                var hash = hmac.ComputeHash(StringToStream(decrypted));

                // Create a new Stringbuilder to collect the bytes and create a string.
                StringBuilder sBuilder = new StringBuilder();
                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < hash.Length; i++) {
                    sBuilder.Append(hash[i].ToString("x2"));
                }
                // Return the hexadecimal string.
                Debug.WriteLine(sBuilder.ToString());
            }

            Debug.WriteLine("getUUID: " + getUUID());
            //******************* CHECK SIGN ***************************
            File.WriteAllBytes("C:\\your\\dir\\here\\akm2.jpg", decryptedArr); // Requires System.IO
        }

        private void takeScreenshot()
        {
            //Create a new bitmap.
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                           Screen.PrimaryScreen.Bounds.Height,
                                           PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

            // Take the screenshot from the upper left corner to the right bottom corner.
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

            // Save the screenshot to the specified path that the user has chosen.
            bmpScreenshot.Save(uploadPath, ImageFormat.Jpeg);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e) //UPLOAD
        {
            //******************* INITIATE PHPSESSION ***************************
            NameValueCollection formData = new NameValueCollection();
            CookieAwareWebClient webClient = new CookieAwareWebClient();
            webClient.Encoding = System.Text.Encoding.Default;
            formData.Clear();
            formData["username"] = "admin";
            if (GlobalClass.TestPasswordCloud() == true)
            {
                formData["password"] = GlobalClass.ReadPasswordCloud(); //Sha256
            }
            byte[] responseBytes = webClient.UploadValues("http://your.url.here/lo.php", "POST", formData);
            string responseHTML = Encoding.UTF8.GetString(responseBytes);

            Uri uriStr;
            if (Uri.TryCreate("http://your.url.here", UriKind.RelativeOrAbsolute, out uriStr) == false)
            {
                System.Diagnostics.Debug.WriteLine("NO");
            }
            foreach (Cookie cookie in webClient.CookieContainer.GetCookies(uriStr))
            {
                System.Diagnostics.Debug.WriteLine(cookie.Name);
                System.Diagnostics.Debug.WriteLine(cookie.Value);
            }
            //******************* INITIATE PHPSESSION ***************************
            takeScreenshot();

            webClient.Headers.Add("Accept: text/html,application/xhtml+xml,application/xml;q=0.9,*;q=0.8");
            webClient.Headers.Add("User-Agent: Mozilla / 5.0(Windows NT 10.0; Win64; x64; rv: 59.0) Gecko / 20100101 Firefox / 59.0");
            webClient.Headers.Add("Referer", "http://your.url.here/welcome.php");
            webClient.Headers.Add("Content-Type", "image/jpeg");

            byte[] UploadReturnString = webClient.UploadFile("http://your.url.here/welcome.php?action=upload", uploadPath);
            string responseUploadHTMLAction = Encoding.UTF8.GetString(UploadReturnString);
            System.Diagnostics.Debug.WriteLine(responseUploadHTMLAction);
        }

        private void stopUploadButton_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void startDownloadButton_Click(object sender, EventArgs e)
        {
            //async
            TimTickEnabled = true;
        }
        private void stopDownloadButton_Click(object sender, EventArgs e)
        {
            //async
            TimTickEnabled = false;

        }
        public async void loadApp()
        {
            fPointer functionPointerDownload = new fPointer(TimTick);
            List<fPointer> functionPointerList = new List<fPointer>();
            functionPointerList.Add(functionPointerDownload);
            while (true)
            {
                //if (timer2.Enabled == true)
                {
                    try
                    {
                        await functionPointerList[0](null);
                        System.Diagnostics.Debug.WriteLine("functionPointerList[0] functionPointerDownload");
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("Exception + " + e.ToString());
                    }
                }
               Application.DoEvents();  // allow the picturebox to refresh itself before continuing
            }
        }

        static UInt64 cntAutoInc = 121;
        private void timer2_Tick(object sender, EventArgs e) //DOWNLOAD
        {
        }

        ////////////////////////////////////////////////////////////////////////
        public async System.Threading.Tasks.Task<object> TimTick(object[] parameters)
        {
            string password = "";
            byte[] decryptedArr = new byte[1];
            NameValueCollection formData = new NameValueCollection();
            //while (true)
            {
                if (TimTickEnabled == true)
                {
                    Uri uriStr;
                    string autoIncrementIndexOfData = cntAutoInc.ToString();
                    string InputString = "";
                    if(password == "" && GlobalClass.TestSecretCloud()==true)
                    {
                        password = GlobalClass.ReadSecretCloud(); //Sha256
                    }
                    //******************* INITIATE PHPSESSION ***************************
                    CookieAwareWebClient webClient = new CookieAwareWebClient();
                    webClient.Encoding = System.Text.Encoding.Default;
                    formData.Clear();
                    formData["username"] = "admin";
                    formData["password"] = "";
                    if ((formData["password"] ==  "") && GlobalClass.TestPasswordCloud() == true)
                    {
                        formData["password"] = GlobalClass.ReadPasswordCloud(); //Sha256
                    }

                    Uri.TryCreate("http://your.url.here/lo.php", UriKind.RelativeOrAbsolute, out uriStr);
                    Task<byte[]> loTaskString = webClient.UploadValuesTaskAsync(uriStr, "POST", formData);
                    await loTaskString;
                    byte[] responseBytes = loTaskString.Result;
                    string responseHTML = Encoding.UTF8.GetString(responseBytes);

                    if (Uri.TryCreate("http://your.url.here", UriKind.RelativeOrAbsolute, out uriStr) == false)
                    {
                        System.Diagnostics.Debug.WriteLine("NO");
                    }
                    foreach (Cookie cookie in webClient.CookieContainer.GetCookies(uriStr))
                    {
                        System.Diagnostics.Debug.WriteLine(cookie.Name);
                        System.Diagnostics.Debug.WriteLine(cookie.Value);
                    }
                    //******************* INITIATE PHPSESSION ***************************

                    //******************* DOWNLOAD XML FILE LIST ***************************
                    Uri.TryCreate("http://your.url.here/filelist.php", UriKind.RelativeOrAbsolute, out uriStr);
                    Task<string> fileListTaskString = webClient.DownloadStringTaskAsync(uriStr);
                    await fileListTaskString;
                    string fileList = fileListTaskString.Result;
                    DataSet ds = new DataSet();
                    byte[] byteArray = Encoding.UTF8.GetBytes(fileList);
                    ds.ReadXml(new MemoryStream(byteArray));
                    if (ds.Tables.Count > 0)
                    {
                        var result = ds.Tables[0];
                    }
                    dataGridView1.DataSource = ds.Tables[0];
                    //******************* DOWNLOAD XML FILE LIST ***************************

                    //******************* CHECK MAX ID ***************************
                    DataTable dtMaxId = ds.Tables[0];
                    using (var ms = new MemoryStream())
                    {
                        int cnt = dtMaxId.Rows.Count;
                        Int64 newID = Convert.ToInt64(dtMaxId.Rows[cnt - 1]["id"]);
                        autoIncrementIndexOfData = newID.ToString();
                        if (lastId < newID)
                        {
                            lastId = newID;
                            Debug.WriteLine(autoIncrementIndexOfData);
                            //******************* CHECK TIME ***************************
                            DateTime tim = Convert.ToDateTime(dtMaxId.Rows[cnt - 1]["time"]);
                            {
                                Debug.WriteLine(tim);
                            }

                            double timCmop = Convert.ToDouble((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
                            if (!((Convert.ToDouble(((tim.ToUniversalTime().AddSeconds(1)) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds) - timCmop) >= 0.0))
                            {  //last picture is oler than ten seconds
                                Debug.WriteLine("No newId");
                                return 0;
                            }
                            //******************* CHECK TIME ***************************

                        }
                        else //no new Id
                        {
                            Debug.WriteLine("No newId");
                            return 0;
                        }
                    }
                    //******************* CHECK MAX ID ***************************

                    //******************* CHECK TIME  ***************************
                    //DataTable dtMaxTime = ds.Tables[0];
                    //using (var ms = new MemoryStream())
                    //{
                    //    int cnt = dtMaxTime.Rows.Count;
                    //    DateTime tim = Convert.ToDateTime(dtMaxTime.Rows[cnt - 1]["time"]);
                    //    {
                    //        Debug.WriteLine(tim);
                    //    }
                    //}
                    //******************* CHECK TIME  ***************************

                    //******************* DOWNLOAD IV ***************************
                    Task<byte []> ivArrTaskByte = webClient.DownloadDataTaskAsync("http://your.url.here/listiv.php?id=" + autoIncrementIndexOfData);
                    await ivArrTaskByte;
                    byte[] ivArr = ivArrTaskByte.Result;
                    //******************* DOWNLOAD IV ***************************

                    //******************* DOWNLOAD LENGTH ***************************
                    Task<byte[]> lengthTaskByte = webClient.DownloadDataTaskAsync("http://your.url.here/getLen.php?id=" + autoIncrementIndexOfData);
                    await lengthTaskByte;
                    byte[] length = lengthTaskByte.Result;
                    //filesize from files
                    int fileSize = 0;
                    foreach (byte l in length)
                    {
                        fileSize += (byte)(l - (byte)(0x30));
                        fileSize *= 10;
                    }
                    fileSize /= 10;
                    System.Diagnostics.Debug.WriteLine("filesize: " + fileSize.ToString());
                    //******************* DOWNLOAD LENGTH ***************************

                    //******************* DOWNL DATA ***************************
                    Uri.TryCreate("http://your.url.here/dwnl_delete.php?id=" + autoIncrementIndexOfData, UriKind.RelativeOrAbsolute, out uriStr);
                    Task<string> InputTaskString = webClient.DownloadStringTaskAsync(uriStr);
                    await InputTaskString;
                    InputString = InputTaskString.Result;
                    //******************* DOWNL DATA ***************************


                    // Create sha256 hash
                    SHA256 mySHA256 = SHA256Managed.Create();
                    byte[] key = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(password)); 

                    try
                    {
                        string decrypted = this.DecryptString(InputString, key, ivArr, fileSize);
                        decryptedArr = Convert.FromBase64String(decrypted);

                        //******************* DECRYPT ***************************

                        //******************* CHECK SIGN ***************************
                        Encoding encoding = Encoding.UTF8;
                        DataTable dt = ds.Tables[0];
                        BinaryFormatter bf = new BinaryFormatter();
                        string dbSignStr = "";
                        using (var ms = new MemoryStream())
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                if (row["id"].ToString() == autoIncrementIndexOfData)
                                {
                                    Debug.WriteLine(row["sign"].ToString());
                                    bf.Serialize(ms, row["sign"]);
                                    dbSignStr = row["sign"].ToString();

                                }
                            }
                            byte[] signObj = ms.ToArray();
                        }

                        StringBuilder sBuilder = new StringBuilder();
                        using (HMACSHA256 hmac = new HMACSHA256(key))
                        {
                            var hash = hmac.ComputeHash(StringToStream(decrypted));

                            // Create a new Stringbuilder to collect the bytes and create a string.
                            // Loop through each byte of the hashed data 
                            // and format each one as a hexadecimal string.
                            for (int i = 0; i < hash.Length; i++)
                            {
                                sBuilder.Append(hash[i].ToString("x2"));
                            }
                            // Return the hexadecimal string.
                            Debug.WriteLine(sBuilder.ToString());
                        }
                        //******************* CHECK SIGN ***************************
                        if (sBuilder.ToString() == dbSignStr) //check signature
                        {   //SIGNATURE CHECK SUCCESSFUL
                            try
                            {
                                using (System.IO.StreamWriter file =
                                new System.IO.StreamWriter(dwnlPath, true))
                                {
                                    await file.WriteLineAsync(Encoding.UTF8.GetChars(decryptedArr));
                                }
                            }
                            catch (Exception ex)
                            {
                                //GlobalClass.Log("ERROR @ WriteFileToPath" + ex.ToString());
                            }
                            //******************* CHECK SIGN ***************************
                        }
                    }
                    catch { }
                    finally { }

                    try
                    {

                        Stream newStream = ByteArrToStream(decryptedArr);

                        // Stretches the image to fit the pictureBox.
                        if (pictureBox1.Image == null)
                            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        if (pictureBox1.Image != null) { pictureBox1.Image.Dispose(); }
                        using (Image MyImage = Image.FromStream(newStream))
                        {
                            pictureBox1.Image = (Image)MyImage.Clone();
                            pictureBox1.Update();
                            pictureBox1.Refresh();
                        }

                    }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.ToString()); }
                }
                else //false
                {   //stop
                    await System.Threading.Tasks.Task.Delay(1000);
                    return 0;
                }
            }
            return 0;
        }
    }
}
