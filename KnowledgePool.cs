using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HtmlAgilityPack;
using System.Text;
using Google.Apis.Customsearch;
using System.Data;
using Google.YouTube;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace KnowledgePool
{
    public partial class KPoolWebForm : System.Web.UI.Page
    {
        int pageC = 1; int lcount = 1;
        String Uname = String.Empty;
        String Pwd = String.Empty;
        String email = String.Empty;
        protected void Page_Load(object sender, EventArgs e)
        {
            Uname = (String)Request.QueryString["UName"];
            Pwd = (String)Request.QueryString["Password"];
            email = (String)Request.QueryString["emailid"];
            if (NameTag.Text == String.Empty)
            {
                NameTag.Text = Uname;
            }
            if (Uname == String.Empty)
            {
                NameTag.Visible = false;
            }
        }

        protected void Search_Click(object sender, EventArgs e)
        {
            int pageCount = 0;
            String keyword = String.Empty;
            keyword = SearchBox.Text;
            keyword = keyword.Replace(" ", "+");
            var webGet = new HtmlWeb();
            var doc = webGet.Load("https://www.google.com/search?safe=off&site=&source=hp&q=" + keyword);

            getLinks(doc, pageC);
        }

        public void getLinks(HtmlDocument doc, int pageCount)
        {
            Uname = (String)Request.QueryString["UName"];
            Pwd = (String)Request.QueryString["Password"];
            email = (String)Request.QueryString["emailid"];

            DataTable dt = new DataTable();
            DataRow dr = null;
            //Create the Columns Definition
            dt.Columns.Add(new DataColumn("Search Results", typeof(Uri)));
            StringBuilder sb = new StringBuilder();
            int startIndex = new int(); int endIndex = new int();
            string temp = string.Empty; int indi = new int();
            HtmlNodeCollection ourNode = doc.DocumentNode.SelectNodes("//div[@class='g']//h3[@class='r']");

            if (ourNode != null)
            {
                List<string> URLs = new List<string>();
                for (int i = 0; i < ourNode.Count; i++)
                {
                    HtmlNode link = ourNode[i].SelectSingleNode(".//a[@href]");
                    sb.Append((link.Attributes[0].Value).ToString() + ",");
                }

                string[] seqlinks = sb.ToString().Split(',');
                int n = seqlinks.Length;
                for (int j = 0; j < n; j++)
                {
                    temp = seqlinks[j];
                    if (temp.Contains("http"))
                        startIndex = temp.IndexOf("http");
                    if (temp.Contains("&amp;sa"))
                    {
                        endIndex = temp.IndexOf("&amp;sa", startIndex);
                        temp = temp.Substring(startIndex, endIndex - startIndex);
                    }
                    if (temp != string.Empty)
                    {
                        if (temp.Contains("forums.asp.net"))
                        {
                            indi = 1;
                        }
                        else if (temp.Contains("stackoverflow"))
                        {
                            indi = 2;
                        }
                        else if (temp.Contains("msdn"))
                        {
                            indi = 3;
                        }
                        else if (temp.Contains("youtube.com"))
                        {
                            indi = 4;
                        }
                        else if (temp.Contains("wikihow"))
                        {
                            indi = 5;
                        }
                        else if (temp.Contains("cnn"))
                        {
                            indi = 6;
                        }
                        else if (temp.Contains("reddit.com"))
                        {
                            indi = 7;
                        }
                        else if (temp.Contains("foxnews.com"))
                        {
                            indi = 8;
                        }
                        if (indi > 0)
                        {
                            nestedLinkContent(temp, indi);
                            storeLinks(email, temp);
                            indi = 0;
                        }
                    }
                }
            }
            getNextPageContent(doc);
        }

        public void storeLinks(String email, String link)
        {
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString; int lcount = new int();
            string urID = string.Empty; string retlinks = string.Empty; string formLink = string.Empty; int slID = 0;
            #region linkCheck
            if (link.Contains("forums.asp.net"))
            {
                formLink = "%forums.asp.net%";
            }
            else if (link.Contains("stackoverflow"))
            {
                formLink = "%stackoverflow%";
            }
            else if (link.Contains("msdn"))
            {
                formLink = "%msdn%";
            }
            else if (link.Contains("youtube.com"))
            {
                formLink = "%youtube.com%";
            }
            else if (link.Contains("wikihow"))
            {
                formLink = "%wikihow%";
            }
            else if (link.Contains("cnn"))
            {
                formLink = "%cnn%";
            }
            else if (link.Contains("reddit.com"))
            {
                formLink = "%reddit.com%";
            }
            else if (link.Contains("foxnews.com"))
            {
                formLink = "%foxnews.com%";
            }
            #endregion

            #region Check Email ID
            using (MySqlConnection con = new MySqlConnection(constr))
            {
                using (MySqlCommand cmd = new MySqlCommand("SELECT idUserRegister FROM knowledgepool.userregister WHERE EmailID='" + email + "'"))
                {
                    using (MySqlDataAdapter sda = new MySqlDataAdapter())
                    {
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        using (DataTable dt2 = new DataTable())
                        {
                            sda.Fill(dt2);
                            int rCount = dt2.Rows.Count;
                            if (rCount > 0)
                            {
                                urID = dt2.Rows[0]["idUserRegister"].ToString();
                            }
                            else
                            {
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Email ID does not exist');", true);
                            }
                        }
                    }
                }

                #region Insert into
                if (urID != string.Empty)
                {
                    //check for count
                    using (MySqlCommand cmd = new MySqlCommand("SELECT idSearchLinks,Links,SearchLinkCount FROM knowledgepool.searchlinks WHERE UserRegID='" + Convert.ToInt16(urID) + "' and Links like '" + formLink + "' order by SearchLinkCount desc"))
                    {
                        using (MySqlDataAdapter sda = new MySqlDataAdapter())
                        {
                            cmd.Connection = con;
                            sda.SelectCommand = cmd;
                            using (DataTable dt2 = new DataTable())
                            {
                                sda.Fill(dt2);
                                int rCount = dt2.Rows.Count;
                                if (rCount > 0)
                                {
                                    retlinks = dt2.Rows[0]["SearchLinkCount"].ToString();
                                    slID = Convert.ToInt16(dt2.Rows[0]["idSearchLinks"].ToString());
                                    lcount = Convert.ToInt16(retlinks);
                                    if (lcount == null)
                                    {
                                        lcount = 1;
                                    }
                                    else
                                    {
                                        lcount += 1;
                                    }
                                }
                                else
                                {
                                    lcount = 1;
                                }
                            }
                        }
                    }

                    if (slID == 0)
                    {
                        //insert into
                        using (MySqlCommand cmd = new MySqlCommand("INSERT INTO knowledgepool.searchlinks (Links,SearchLinkCount,UserRegID,Keyword)VALUES (@Links,@SearchLinkCount,@UserRegID,@keyword)"))
                        {
                            cmd.Parameters.AddWithValue("@Links", link);
                            cmd.Parameters.AddWithValue("@SearchLinkCount", lcount);
                            cmd.Parameters.AddWithValue("@UserRegID", Convert.ToInt16(urID));
                            cmd.Parameters.AddWithValue("@keyword", SearchBox.Text);
                            using (MySqlDataAdapter sda = new MySqlDataAdapter())
                            {
                                cmd.Connection = con;
                                sda.SelectCommand = cmd;
                                con.Open();
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    else if (slID > 0)
                    {
                        //Update to
                        using (MySqlCommand cmd = new MySqlCommand("UPDATE knowledgepool.searchlinks SET SearchLinkCount='" + lcount + "'" + "WHERE idSearchLinks='" + slID + "'"))
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
                }
                #endregion
            }

            #endregion
        }

        private void GenerateTable(DataTable dt)
        {
            Table table = new Table();
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

        public void nestedLinkContent(string extLink, int indicator)
        {
            #region aspdotnetforums
            if (indicator == 1)
            {
                string data = loadWebPageString(extLink);
                StringBuilder sb = new StringBuilder();
                var webGet2 = new HtmlWeb();
                var doc2 = webGet2.Load(extLink);
                if (data.Contains("class=\"comment-wrap  answered \"") || data.Contains("class=\"comment-wrap thread-starter answered \""))
                {
                    HtmlNodeCollection next = doc2.DocumentNode.SelectNodes("//ul[@class='comment-list']//li[@class='comment-wrap  answered ']");
                    HtmlNodeCollection head = doc2.DocumentNode.SelectNodes("//div[@class='content-wrapper']");

                    if (next != null)
                    {
                        TableRow taspHRow = new TableRow();
                        table1.Rows.Add(taspHRow);
                        table1.Visible = true;
                        TableCell taspHcell = new TableCell();
                        taspHcell.Text = "<u><h2>Answers from forums.asp.net</h2></u>";
                        taspHRow.Cells.Add(taspHcell);
                        if (head != null)
                        {
                            HtmlNode linkhead = head[0].SelectSingleNode(".//h1[@id='threadstatus']");
                            TableRow taspQRow = new TableRow();
                            table1.Rows.Add(taspQRow);
                            TableCell taspQCell = new TableCell();
                            taspQCell.Text = "<h3><u>Question</u>:</h3> <br /> " + linkhead.InnerText;
                            taspQRow.Cells.Add(taspQCell);
                        }
                        for (int i = 0; i < next.Count; i++)
                        {
                            HtmlNode link = next[i].SelectSingleNode(".//div[@class='comment-right-col']");
                            TableRow taspARow = new TableRow();
                            table1.Rows.Add(taspARow);
                            TableCell taspACell = new TableCell();
                            taspACell.Text = link.InnerHtml;
                            taspARow.Cells.Add(taspACell);
                        }
                    }
                }
            }
            #endregion

            #region stackoverflow
            if (indicator == 2)
            {
                string data = loadWebPageString(extLink);
                StringBuilder sb = new StringBuilder();
                var webGet2 = new HtmlWeb();
                var doc2 = webGet2.Load(extLink);
                if (data.Contains("class=\"answer accepted-answer\""))
                {
                    HtmlNodeCollection next = doc2.DocumentNode.SelectNodes("//div[@class='answer accepted-answer']");
                    HtmlNodeCollection header = doc2.DocumentNode.SelectNodes("//div[@class='question']");
                    if (next != null)
                    {
                        TableRow tstHRow = new TableRow();
                        table1.Rows.Add(tstHRow);
                        table1.Visible = true;
                        TableCell tstHcell = new TableCell();
                        tstHcell.Text = "<u><h2>Answers from StackOverFlow</h2></u>";
                        tstHRow.Cells.Add(tstHcell);
                        if (header != null)
                        {
                            HtmlNode headerlink = header[0].SelectSingleNode(".//div[@class='post-text']");
                            TableRow tstQRow = new TableRow();
                            table1.Rows.Add(tstQRow);
                            TableCell tstQCell = new TableCell();
                            tstQCell.Text = "<h3><u>Question</u>:</h3> <br /> " + headerlink.InnerHtml;
                            tstQRow.Cells.Add(tstQCell);
                        }
                        for (int i = 0; i < next.Count; i++)
                        {
                            HtmlNode link = next[i].SelectSingleNode(".//div[@class='post-text']");
                            TableRow tstARow = new TableRow();
                            table1.Rows.Add(tstARow);
                            TableCell tstACell = new TableCell();
                            tstACell.Text = link.InnerHtml;
                            tstARow.Cells.Add(tstACell);
                        }
                    }
                }
            }
            #endregion

            #region MSDN
            if (indicator == 3)
            {
                string data = loadWebPageString(extLink);
                StringBuilder sb = new StringBuilder();
                var webGet2 = new HtmlWeb();
                var doc2 = webGet2.Load(extLink);
                if (data.Contains("id=\"mainBody\""))
                {
                    HtmlNodeCollection next = doc2.DocumentNode.SelectNodes("//div[@id='mainBody']");//"//div[@class='LW_CollapsibleArea_HrDiv']//hr[@class='LW_CollapsibleArea_Hr']"
                    if (next != null)
                    {
                        TableRow tRow5 = new TableRow();
                        table1.Rows.Add(tRow5);
                        table1.Visible = true;
                        TableCell tcell5 = new TableCell();
                        tcell5.Text = "<u><h2>Answers from MSDN</h2></u>";
                        tRow5.Cells.Add(tcell5);
                        for (int i = 0; i < next.Count; i++)
                        {
                            HtmlNode link = next[i].SelectSingleNode(".//div[@class='sectionblock']");
                            TableRow tRow6 = new TableRow();
                            table1.Rows.Add(tRow6);
                            TableCell tCell6 = new TableCell();
                            tCell6.Text = link.InnerHtml;
                            tRow6.Cells.Add(tCell6);
                        }
                    }
                }
            }
            #endregion

            #region youtube videos
            if (indicator == 4)
            {
                string VideoID = extLink.Substring(extLink.IndexOf("%3D") + 3);//"nQJACVmankY";//
                if (!VideoID.Contains("youtube.com"))
                {
                    TableRow tRow7 = new TableRow();
                    table1.Rows.Add(tRow7);
                    table1.Visible = true;
                    TableCell tcell7 = new TableCell();
                    tcell7.Text = "<u><h2>Youtube</h2></u>";
                    tRow7.Cells.Add(tcell7);
                    Label Utube = new Label();
                    Utube.ID = "YouTube" + lcount.ToString();
                    Utube.Text = "<object width='425' height='355'><param name='movie' value='http://www.youtube.com/v/" + VideoID + "'></param><param name='wmode' value='transparent'></param><embed src='http://www.youtube.com/v/" + VideoID + "' type='application/x-shockwave-flash' wmode='transparent' width='425' height='355'></embed></object>";
                    TableRow tRow8 = new TableRow();
                    table1.Rows.Add(tRow8);
                    TableCell tCell8 = new TableCell();
                    tCell8.Controls.Add(Utube);
                    tRow8.Cells.Add(tCell8);
                    lcount += 1;
                }
            }
            #endregion

            #region wiki
            if (indicator == 5)
            {
                string data = loadWebPageString(extLink);
                StringBuilder sb = new StringBuilder();
                var webGet2 = new HtmlWeb();
                var doc2 = webGet2.Load(extLink);
                if (data.Contains("id=\"bodycontents\""))
                {
                    HtmlNodeCollection next = doc2.DocumentNode.SelectNodes("//div[@id='bodycontents']//a[@class='anchor']");//"//div[@class='LW_CollapsibleArea_HrDiv']//hr[@class='LW_CollapsibleArea_Hr']"
                    HtmlNodeCollection next2 = doc2.DocumentNode.SelectNodes("//div[@id='bodycontents']//div[@class='section steps   sticky ']");
                    if (next != null)
                    {
                        TableRow tWHRow = new TableRow();
                        table1.Rows.Add(tWHRow);
                        table1.Visible = true;
                        TableCell tcell7 = new TableCell();
                        tcell7.Text = "<u><h2>Answers from WikiHow</h2></u>";
                        tWHRow.Cells.Add(tcell7);

                        for (int i = 0; i < next2.Count; i++)
                        {
                            TableRow tWARow = new TableRow();
                            table1.Rows.Add(tWARow);
                            table1.Visible = true;
                            TableCell tWAcell = new TableCell();
                            tWAcell.Text = next2[i].InnerHtml;
                            tWARow.Cells.Add(tWAcell);
                        }
                    }
                }
            }
            #endregion

            #region CNN
            if (indicator == 6)
            {
                string data = loadWebPageString(extLink);
                StringBuilder sb = new StringBuilder();
                var webGet2 = new HtmlWeb();
                var doc2 = webGet2.Load(extLink);
                if (data.Contains("class=\"zn__containers\""))
                {
                    HtmlNodeCollection next2 = doc2.DocumentNode.SelectNodes("//div[@class='zn__containers']");
                    
                    if (next2 != null)
                    {
                        TableRow tCHRow = new TableRow();
                        table1.Rows.Add(tCHRow);
                        table1.Visible = true;
                        TableCell tCHcell = new TableCell();
                        tCHcell.Text = "<u><h2>Latest News on CNN</h2></u>";
                        tCHRow.Cells.Add(tCHcell);

                        for (int i = 0; i < next2.Count; i++)
                        {
                            HtmlNode link = next2[i].SelectSingleNode(".//div[@class='column zn__column--idx-1']");
                            TableRow tCARow = new TableRow();
                            table1.Rows.Add(tCARow);
                            table1.Visible = true;
                            TableCell tCAcell = new TableCell();
                            tCAcell.Text = next2[i].InnerHtml;
                            tCARow.Cells.Add(tCAcell);

                            HtmlNode nesLink = next2[i].SelectSingleNode(".//div[@class='column zn__column--idx-1']//h3[@class='cd__headline']");
                            
                        }
                    }
                }
            }
            #endregion

            #region reddit
            if (indicator == 7)
            {
               string data = loadWebPageString(extLink);
               StringBuilder sb = new StringBuilder();
               var webGet2 = new HtmlWeb();
               var doc2 = webGet2.Load(extLink);
               if (data.Contains("class=\"content\""))
               {
                   HtmlNodeCollection next2 = doc2.DocumentNode.SelectNodes("//div[@class='content']");
                   //ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('StackoverFlow');", true);
                   if (next2 != null)
                   {
                       TableRow tRHRow = new TableRow();
                       table1.Rows.Add(tRHRow);
                       table1.Visible = true;
                       TableCell tRHcell = new TableCell();
                       tRHcell.Text = "<u><h2>Trending on Reddit</h2></u>";
                       tRHRow.Cells.Add(tRHcell);

                       for (int i = 0; i < next2.Count; i++)
                       {
                           Label reddit = new Label();
                           HtmlNode link = next2[i].SelectSingleNode(".//div[@id='siteTable']");
                           reddit.Text = link.InnerHtml;
                           TableRow tRARow = new TableRow();
                           table1.Rows.Add(tRARow);
                           table1.Visible = true;
                           TableCell tRAcell = new TableCell();
                           tRAcell.Controls.Add(reddit);
                           tRARow.Cells.Add(tRAcell);

                           HtmlNode nesLink = next2[i].SelectSingleNode(".//div[@class='column zn__column--idx-1']//h3[@class='cd__headline']");
                           
                       }
                   }
               }
            }
            #endregion

            #region FOXNews
            if (indicator == 8)
            {
                string data = loadWebPageString(extLink);
                StringBuilder sb = new StringBuilder();
                var webGet2 = new HtmlWeb();
                var doc2 = webGet2.Load(extLink);
                if (data.Contains("class=\"home\""))
                {
                    HtmlNodeCollection next2 = doc2.DocumentNode.SelectNodes("//div[@id='wrapper']//div[@id='doc']");
                    if (next2 != null)
                    {
                        TableRow tFHRow = new TableRow();
                        table1.Rows.Add(tFHRow);
                        table1.Visible = true;
                        TableCell tFHcell = new TableCell();
                        tFHcell.Text = "<u><h2>Latest on Fox News</h2></u>";
                        tFHRow.Cells.Add(tFHcell);

                        for (int i = 0; i < next2.Count; i++)
                        {
                            Label foxN = new Label();
                            HtmlNode link = next2[i].SelectSingleNode(".//section[@id='latest']");
                            foxN.Text = link.InnerHtml;
                            TableRow tFARow = new TableRow();
                            table1.Rows.Add(tFARow);
                            table1.Visible = true;
                            TableCell tFAcell = new TableCell();
                            tFAcell.Controls.Add(foxN);
                            tFARow.Cells.Add(tFAcell);

                            HtmlNode nesLink = next2[i].SelectSingleNode(".//div[@class='column zn__column--idx-1']//h3[@class='cd__headline']");
                        }
                    }
                }
            }
            #endregion

            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Search was Successful');", true);
        }

        public string loadWebPageString(string eLink)
        {
            string data = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(eLink);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;
                if (response.CharacterSet == null)
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                data = readStream.ReadToEnd();
                response.Close();
                readStream.Close();
            }
            return data;
        }

        public void getNextPageContent(HtmlDocument doc)
        {
            string htmllink = string.Empty; string temp1 = string.Empty;
            pageC += 1;
            int srtIndex = new int(); int endIndex = new int();
            if (doc != null && pageC <= 5)
            {
                HtmlNodeCollection next = doc.DocumentNode.SelectNodes("//td//a[@class='fl']");
                if (next != null)
                {
                    int tagC = next.Count;
                    for (int k = 0; k < tagC; k++)
                    {
                        if (next[k].InnerText != "Previous" && next[k].InnerText != "Next")
                        {
                            if (!next[k].InnerText.Contains(" "))
                            {
                                if (pageC == Convert.ToInt16(next[k].InnerText))
                                {
                                    htmllink = next[k].OuterHtml;
                                    if (htmllink.Contains("href="))
                                        srtIndex = htmllink.IndexOf("href=") + 6;
                                    if (htmllink.Contains("\"><span"))
                                    {
                                        endIndex = htmllink.IndexOf("\"><span");
                                        temp1 = htmllink.Substring(srtIndex, endIndex - srtIndex);
                                    }
                                }
                            }
                        }
                    }
                }
                HtmlDocument pageN = (new HtmlWeb()).Load("https://www.google.com" + temp1.Replace("amp;", ""));
                getLinks(pageN, pageC);
            }
        }
    }
}
