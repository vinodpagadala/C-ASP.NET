using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mime;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;
using MySql.Data.MySqlClient;
using System.Data;
using System.Configuration;
using System.IO;

namespace MailBlastApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void SendMails_Click(object sender, EventArgs e)
        {
            string pathEx = @"C:/myfolder/MSAcademicia/Book1.xlsx";
            string passC = PassCode.Text;
            if (passC == "120391")
            {
                getMailIDs();
                System.Windows.Forms.Application.Exit();
                //Environment.Exit(1);
            }
            else
            {
                MessageBox.Show("Please enter the correct passcode");
                PassCode.Text = string.Empty;
            }
            //ExcelReaderInterop exobj = new ExcelReaderInterop();
            //exobj.ExcelOpenSpreadsheets(pathEx);
        }

        public void getMailIDs()
        {
            string ProfName = string.Empty; string Email = string.Empty;
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM mailidlist.mailtable2"))
                {
                    using (MySqlDataAdapter sda = new MySqlDataAdapter())
                    {
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        using (System.Data.DataTable dt2 = new System.Data.DataTable())
                        {
                            sda.Fill(dt2);
                            if (dt2.Rows.Count > 0)
                            {
                                for (int i = 0; i < dt2.Rows.Count;i++ )
                                {
                                    ProfName = dt2.Rows[i][1].ToString();
                                    Email = dt2.Rows[i][2].ToString();
                                    if (ProfName.StartsWith("Dr. "))
                                    {
                                        //break;
                                    }
                                    else
                                    {
                                        ProfName = "Dr. " + ProfName;
                                    }

                                    Send_Emails(Email, ProfName);
                                    //for (int j = 1; j < dt2.Columns.Count; j++)
                                    //{
                                    //    ProfName = dt2.Rows[i][j].ToString();
                                    //    Email = dt2.Rows[i][j].ToString();   
                                    //}
                                }
                                MessageBox.Show("Mail sent succesfully");
                            }
                        }
                    }
                }
            }
        }

        public void Send_Emails(string email, string contactName)
        {
            string smtpAddress = "smtp.gmail.com";
            string file = "Vinod_Pagadala_IT_Resume.docx";
            int portNumber = 587;
            bool enableSSL = true;
            string emailFrom = "vp50@zips.uakron.edu";
            string password = "yciscykm76";
            string subject = "Request for appointment regarding Assistant-ship";
            string body = contactName + ", <br /><br /> I am Vinod Pagadala computer science graduate student looking for assistant-ship."
                +"  I have 3 years of work experience in a Software service based company as a Software engineer in Microsoft Dynamics CRM "
                +"which is .Net based web application also worked on development of  web services and application integration. "
                +"I also have working experience on java and as a database administrator for few projects during my stint as software developer. "
                + "I got a chance to work and interact with clients like FOSROC (construction partners of Burj Khalifa) and CSS Corp during my job.<br /><br />"

                +"Kindly provide me with an appointment, as I would like to discuss on the same. "
                +"Could you please consider me for any positions available in your department. "
                +"I will be glad to provide you with any further information regarding my background, qualifications or references up on request. "
                + "Looking​ forward for your positive response. Thank you for your time and consideration. <br /><br />"

                + "Please find my CV attached. <br /><br /><br />"
                +"P.S: This is a \"Mail Blast App\"system generated Email. More details of this project is available my Curriculum Vitae";// +registerLink;

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(emailFrom);
            mail.To.Add(email);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            Attachment data = new Attachment(file, MediaTypeNames.Application.Octet);
            // Add time stamp information for the file.
            ContentDisposition disposition = data.ContentDisposition;
            disposition.CreationDate = System.IO.File.GetCreationTime(file);
            disposition.ModificationDate = System.IO.File.GetLastWriteTime(file);
            disposition.ReadDate = System.IO.File.GetLastAccessTime(file);
            // Add the file attachment to this e-mail message.
            mail.Attachments.Add(data);
            // Can set to false, if you are sending pure text.

            using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
            {
                smtp.Credentials = new NetworkCredential(emailFrom, password);
                smtp.EnableSsl = enableSSL;
                smtp.Send(mail);
            }
            //ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Mail sent succesfully');", true);
        }
    }
