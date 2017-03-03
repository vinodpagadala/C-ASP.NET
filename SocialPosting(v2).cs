using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASPSnippets.FaceBookAPI;
using System.Web.Script.Serialization;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.IO;
using System.Net;
using Facebook;
using TweetSharp;

namespace SocialList
{
    public partial class SL : System.Web.UI.Page
    {
        DataTable dtb = new DataTable();
        Table table = new Table();
        

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                if (TextBox2.Text != string.Empty)
                {
                    TextBox1.Text = TextBox2.Text;
                }
                if (Convert.ToString(Request.Form["__EVENTARGUMENT"]) == "Submit")
                {
                    FBLogin(sender, e);//submit action here
                }
                if (Convert.ToString(Request.Form["__EVENTARGUMENT"]) == "Tweet")
                {
                    TwLogin(sender, e);//submit action here
                }
            }
        }

        protected void TwLogin(object sender, EventArgs e)
        {
            string ConsumerKey = "yy4aasn9I4eLf1V8nwi7774sY";
            string ConsumerSecret = "1XvBnRBUVkovOewd2YiX44C7CgVIopUycRBXQg3WQfeQ4YrkeC";
            string TokenKey = "130485515-rTi0o1wolaWdTZQT8WQxZrQMiIgmvJVXz50oHNWH";
            string TokenSecretKey = "oSjy2f59Y6J5UnTuhVYIwAEPrMzQAeGkTMGrqlEbQL2MG";

            var service = new TweetSharp.TwitterService(ConsumerKey, ConsumerSecret, TokenKey, TokenSecretKey); //Replace keys with values from step #5
            var twitterStatus = service.SendTweet(new SendTweetOptions() { Status = FBpostbox.Text });
            if (twitterStatus != null)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Tweet was Successful');", true);
                Response.Redirect("https://twitter.com");
            }
        }

        protected void FBLogin(object sender, EventArgs e)
        {
            CheckAuthorization();
        }
        private void CheckAuthorization()
        {
            string app_id = "204352739943546";
            string app_secret = "70bc5834d9cc737b8a6b0d91aa118176";
            string scope = "publish_actions,manage_pages";

            if (Request["code"] == null)
            {
                Response.Redirect(string.Format(
                    "https://graph.facebook.com/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}",
                    app_id, Request.Url.AbsoluteUri, scope));
            }
            else
            {
                Dictionary<string, string> tokens = new Dictionary<string, string>();

                string url = string.Format("https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&scope={2}&code={3}&client_secret={4}",
                    app_id, Request.Url.AbsoluteUri, scope, Request["code"].ToString(), app_secret);

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    string vals = reader.ReadToEnd();

                    foreach (string token in vals.Split('&'))
                    {
                        tokens.Add(token.Substring(0, token.IndexOf("=")),
                            token.Substring(token.IndexOf("=") + 1, token.Length - token.IndexOf("=") - 1));
                    }
                }

                string access_token = tokens["access_token"];

                var client = new FacebookClient(access_token);

                client.Post("/me/feed", new { message = FBpostbox.Text });

                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Facebook post was successful');", true);

                Response.Redirect("https://www.facebook.com");
            }
        }

        protected void Choosefile_Click(object sender, EventArgs e)
        {
            string filename = Path.GetFileName(ImageUpload1.PostedFile.FileName);
            string contentType = ImageUpload1.PostedFile.ContentType;
            using (Stream fs = ImageUpload1.PostedFile.InputStream)
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    byte[] bytes = br.ReadBytes((Int32)fs.Length);
                    string base64String = Convert.ToBase64String(bytes, 0, bytes.Length);
                    ProfileImage.ImageUrl = "data:image/png;base64," + base64String;
                }
            }
        }

        protected void SubmitButton1_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dob = DateTime.Parse(Request.Form[TextBox1.UniqueID]);
                byte[] bytes;
                string filename = Path.GetFileName(ImageUpload1.PostedFile.FileName);
                string contentType = ImageUpload1.PostedFile.ContentType;
                using (Stream fs = ImageUpload1.PostedFile.InputStream)
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        bytes = br.ReadBytes((Int32)fs.Length);
                    }
                }
                Random random = new Random();
                int randomNumber = random.Next(0, 10000);
                string base64String = Convert.ToBase64String(bytes, 0, bytes.Length);
                ProfileImage.ImageUrl = "data:image/png;base64," + base64String;
                string connstr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                dtb.Columns.Add("UserName", typeof(string));
                dtb.Columns.Add("EmailID", typeof(string));
                dtb.Columns.Add("PhoneNumber", typeof(string));
                dtb.Columns.Add("FBLink", typeof(string));

                //save Image to a folder
                if (ImageUpload1.FileName != string.Empty)//ImageUpload1.HasFile
                {
                    // Your code to save the file
                    ImageUpload1.SaveAs(Server.MapPath("~/Images/" + filename));
                }

                //Write code for DB connection and saving rows into a table in DB
                using (MySqlConnection con = new MySqlConnection(connstr))
                {
                    using (MySqlCommand cmd = new MySqlCommand("INSERT INTO sociallist.sociallisttable (Name,DateofBirth,EMailID,Phone,FaceBook,ProfilePicture)VALUES (@Name,@DateofBirth,@Email,@Phone,@URI,@ProfilePicture)"))
                    {
                        cmd.Parameters.AddWithValue("@Name", TxUserName.Text);
                        cmd.Parameters.AddWithValue("@DateofBirth", dob);
                        cmd.Parameters.AddWithValue("@Email", TxEmail.Text);
                        cmd.Parameters.AddWithValue("@Phone", TxPhone.Text);
                        cmd.Parameters.AddWithValue("@URI", TxFB.Text);
                        cmd.Parameters.AddWithValue("@ProfilePicture", bytes);
                        using (MySqlDataAdapter sda = new MySqlDataAdapter())
                        {
                            cmd.Connection = con;
                            sda.SelectCommand = cmd;
                            con.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                dtb.Rows.Add(TxUserName.Text, TxEmail.Text, TxPhone.Text, TxFB.Text);
                GenerateTable(dtb);

                if (TxEmail.Text != string.Empty)
                {
                    if (TxFB.Text != string.Empty || TxUserName.Text != string.Empty || TxPhone.Text != string.Empty)
                    {
                        TxUserName.Focus();
                        TxPhone.Text = string.Empty;
                        TxFB.Text = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + ex.Message + "');", true);
            }
        }

        private void GridDataBind()
        {
            GridView1.Visible = true;
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                using (MySqlCommand cmd = new MySqlCommand("SELECT Name, EMailID, DateofBirth, Phone, FaceBook, ProfilePicture FROM sociallist.sociallisttable"))
                {
                    using (MySqlDataAdapter sda = new MySqlDataAdapter())
                    {
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        using (DataTable dt2 = new DataTable())
                        {
                            sda.Fill(dt2);

                            GridView1.DataSource = dt2;
                            GridView1.DataBind();
                        }
                    }
                }
            }
        }


        private void GenerateTable(DataTable dt)
        {
            TableRow row = null;
            //Add the Headers
            row = new TableRow();
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                TableHeaderCell headerCell = new TableHeaderCell();
                headerCell.Text = dt.Columns[j].ColumnName;
                row.Cells.Add(headerCell);
            }
            table.Rows.Add(row);
            //Add the Column values
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                row = new TableRow();
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    TableCell cell = new TableCell();
                    cell.Text = dt.Rows[i][j].ToString();
                    row.Cells.Add(cell);
                }
                // Add the TableRow to the Table
                table.Rows.Add(row);
            }
        }


        protected void TxUserName_TextChanged(object sender, EventArgs e)
        {
            if (TxUserName.Text == string.Empty)
            {
                //Error Message textbox cannot be empty
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Name cannot be null');", true);
            }
            else
            {
                return;
            }
        }

        protected void TxDate_TextChanged(object sender, EventArgs e)
        {
            TextBox2.Text = TextBox1.Text;
            TextBox1.ReadOnly = true;
        }

        protected void TxEmail_TextChanged(object sender, EventArgs e)
        {
            string email = TxEmail.Text; string subemail = string.Empty;
            if (email.Contains("@"))
            {
                subemail = email.Substring(email.IndexOf("@"));
                if (subemail.Contains(".com"))
                {
                    return;
                }
                else
                {
                    //Error message to user for valid Email ID
                    TxEmail.Text = "";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Please enter valid Email ID');", true);
                }
            }
            else
            {
                //Error message to user for valid Email ID
                TxEmail.Text = string.Empty;
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Please enter valid Email ID');", true);
            }
        }

        protected void TxPhone_TextChanged(object sender, EventArgs e)
        {
            string phNum = TxPhone.Text;
            if (phNum.Length == 10 || phNum.Length == 7)
            {
                return;
            }
            else
            {
                //Error message to user for valid phone number
                TxPhone.Text = string.Empty;
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Please enter valid Phone Number');", true);
            }
        }

        protected void TxFB_TextChanged(object sender, EventArgs e)
        {
            string fb = TxFB.Text;
            if (fb.Contains("www.facebook.com"))
            {
                return;
            }
            else
            {
                //Error message to user for valid link
                TxFB.Text = string.Empty;
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Please enter valid Facebook Link');", true);
            }
        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            DateTime dob1 = new DateTime();
            if (TxEmail.Text == string.Empty)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Cannot edit a record without valid Email ID');", true);
            }
            else
            {
                if (TextBox1.Text != string.Empty)
                {
                    dob1 = DateTime.Parse(Request.Form[TextBox1.UniqueID]);
                }
                byte[] bytes;
                string filename = Path.GetFileName(ImageUpload1.PostedFile.FileName);
                string contentType = ImageUpload1.PostedFile.ContentType;
                using (Stream fs = ImageUpload1.PostedFile.InputStream)
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        bytes = br.ReadBytes((Int32)fs.Length);
                    }
                }
                int bytecount = bytes.Length;
                string uName = TxUserName.Text; string phoneNum = TxPhone.Text; string FB = TxFB.Text; string query1 = string.Empty;
                query1 = "UPDATE sociallist.sociallisttable SET ";
                if (uName != string.Empty)
                {
                    query1 += "Name='" + uName + "', ";
                }
                if (dob1 != null)
                {
                    query1 += "DateofBirth='" + dob1.ToShortDateString() + "', ";
                }
                if (phoneNum != string.Empty)
                {
                    query1 += "Phone='" + TxPhone.Text + "', ";
                }
                if (FB != string.Empty)
                {
                    query1 += "FaceBook='" + TxFB.Text + "'";
                }
                if (bytecount > 0)
                {
                    query1 += "ProfilePicture='" + bytes;
                }
                string connstr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
                //write code to connect DB and query for record based on Email and then edit the row and commit the transaction
                using (MySqlConnection con = new MySqlConnection(connstr))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query1 + "WHERE EmailID='" + TxEmail.Text + "';"))
                    {
                        using (MySqlDataAdapter sda = new MySqlDataAdapter())
                        {
                            cmd.Connection = con;
                            sda.SelectCommand = cmd;
                            con.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                GridDataBind();
                return;
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            string connstr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            //write code to connect DB and query for record based on Email and then delete the row and commit the transaction
            using (MySqlConnection con = new MySqlConnection(connstr))
            {
                using (MySqlCommand cmd = new MySqlCommand("DELETE FROM sociallist.sociallisttable WHERE EmailID='" + TxEmail.Text + "';"))
                {
                    using (MySqlDataAdapter sda = new MySqlDataAdapter())
                    {
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            GridDataBind();
        }

        protected void OnRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                byte[] bytes = (byte[])(e.Row.DataItem as DataRowView)["ProfilePicture"];
                string base64String = Convert.ToBase64String(bytes, 0, bytes.Length);
                (e.Row.FindControl("ProfileImage1") as Image).ImageUrl = "data:image/png;base64," + base64String;

            }
        }
        protected void ViewRecordsButton_Click(object sender, EventArgs e)
        {
            GridDataBind();
        }

        protected void GridView1_SelectedIndexChanged1(object sender, EventArgs e)
        {
            GridViewRow row = GridView1.SelectedRow;
            TxUserName.Text = row.Cells[1].Text;//GridView1.Rows[e.NewSelectedIndex].Cells[1].Text;
            TxEmail.Text = row.Cells[2].Text;//GridView1.Rows[e.NewSelectedIndex].Cells[2].Text;
            TextBox1.Text = row.Cells[3].Text;//GridView1.Rows[e.NewSelectedIndex].Cells[3].Text;
            TxPhone.Text = row.Cells[4].Text;//GridView1.Rows[e.NewSelectedIndex].Cells[4].Text;
            TxFB.Text = row.Cells[5].Text;//GridView1.Rows[e.NewSelectedIndex].Cells[5].Text;
        }
    }
    public class FaceBookUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string PictureUrl { get; set; }
        public string Email { get; set; }
    }
}
