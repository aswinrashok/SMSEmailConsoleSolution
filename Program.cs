using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Threading;
using System.Resources;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;

namespace messageEmailSolution
{
	
	
    class Program
    {
        public static string connectionString = // your DB connection  string..
        static void Main(string[] args)
        {

            Console.WriteLine("Hello");
            try
            {
        
                DataSet ds = new DataSet();
                SqlConnection con = new SqlConnection(connectionString);
                con.Open();
                SqlCommand cmd = new SqlCommand("USP_S_MsgDetails", con);   // Data for sending the sms and email. Mobile numer ,Email ID ,Msg Conetnts
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
                using (SqlDataAdapter sqlda = new SqlDataAdapter(cmd))
                {
                    sqlda.Fill(ds);
                }
                con.Close();
                con.Dispose();

                bool Result = false;
                string ReceiverStatus;
                string toMailId = "";
             
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        
                        toMailId = ds.Tables[0].Rows[i]["Email_Id"].ToString();			// To Email ID
                        string email =ds.Tables[0].Rows[i]["emailContent"].ToString(); // Email Content
						string PhoneNumber = ds.Tables[0].Rows[i]["Mobile_No"].ToString();   // Mobile No
                        string Message =ds.Tables[0].Rows[i]["Message"].ToString(); //sms content
                        Result = SendSMS(PhoneNumber, Message);
                        ReceiverStatus = (Result == true) ? "Y" : "N";
                        sendemail(toMailId, email);
                        UpdateStatus(ReceiverStatus,email,PhoneNumber);
                    }
                }
                Console.WriteLine("All Messages Sent");					// Success Msg.
                Console.ReadLine();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

            }

        }


        private static void UpdateStatus( string ReceiverStatus,string email,string PhoneNumber)	// this method is to update the sms status.
        {
            try
            {
                SqlConnection con = new SqlConnection(connectionString);
                con.Open();
                SqlCommand cmd = new SqlCommand("USP_U_UpdateReceiverStatus", con);
                cmd.CommandType = CommandType.StoredProcedure;
               cmd.Parameters.Add("@status", SqlDbType.NVarChar).Value = ReceiverStatus;
			   cmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = email;
                cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = PhoneNumber;
                cmd.ExecuteNonQuery();
                con.Close();
                con.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
        static bool SendSMS(string strTo, string strMessage)
        {
           
            string strKey = "xxxxxxxxxx-xxxxx-xxx-xxxx-xxxxxxxxxxx";    // SMS API KEY
            string strFrom = "TESTMSG";									// SMS API FROM NAME
            string strSendSMSUrl = string.Empty;
            string strOutput = string.Empty;
            bool Res = false;
            try
            {
                strSendSMSUrl =              //     sms API url
               
                string strMainContent = "{\"outboundSMSMessageRequest\":{\"address\":[\"tel:!address!\"],\"senderAddress\":\"tel:!sendername!\",\"outboundSMSTextMessage\":{\"message\":\"!message!\"},\"clientCorrelator\":\"\",\"receiptRequest\": {\"notifyURL\":\"\",\"callbackData\":\"$(callbackData)\"} ,\"messageType\":\"4\",\"senderName\":\"\"}}";
                strMainContent = strMainContent.Replace("!address!", strTo).Replace("!sendername!", HttpUtility.UrlEncode(strFrom)).Replace("!key!", strKey).Replace("!message!", strMessage);
                HttpWebRequest objReq = WebRequest.Create(strSendSMSUrl) as HttpWebRequest;
                objReq.Method = "POST";
                objReq.Timeout = 50000;
                objReq.ContentType = "application/json";
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(strMainContent);
                objReq.ContentLength = byteArray.Length; objReq.Headers.Add("key", strKey);
                using (Stream dataStream = objReq.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
                using (HttpWebResponse objRes = objReq.GetResponse() as HttpWebResponse)
                {
                    StreamReader objSr = new StreamReader(objRes.GetResponseStream());
                    strOutput = objSr.ReadToEnd();

                

                    Res = strOutput.Contains("Submitted");
                   }
            }

            catch (Exception ex)
            {
                throw ex;
            }
            return Res;
        }

     
        private static void sendemail(string ToMailId,string maillBody)
        {
            string from = "abcdef@gmail.com";									// Sender(FROM) Email ID
            using (MailMessage mail = new MailMessage(from, ToMailId))
            {
                mail.Subject="MY Test Email Subject";								// Email Subject
                mail.Body = maillBody;

                mail.IsBodyHtml = false;
                SmtpClient smtp = new SmtpClient();								
                smtp.Host = "smtp.gmail.com";											// SMSTP HOST of your Mail Provider(GMail)
                smtp.EnableSsl = true;
                NetworkCredential networkCredential = new NetworkCredential(from, "password");		// Email Password
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = networkCredential;
                smtp.Port = 587;																// smtp Port 587 
                smtp.Send(mail);
              
            }
        }

    }
}

