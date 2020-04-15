using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Core;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace SEHCodeExercise
{
    public partial class Form1 : Form
    {
        public Form1()
        {          
            InitializeComponent();
        }

        ImageList imgs = new ImageList();
        void LoadImageList()
        {
            imgs.ImageSize = new Size(100, 100);
            for (int i = 0; i < srcList.Count; i++)
            {
                try
                {
                    WebClient wc = new WebClient();
                    byte[] imageByte = wc.DownloadData(srcList[i]);
                    MemoryStream ms = new MemoryStream(imageByte);
                    Image img = Image.FromStream(ms);
                    imgs.Images.Add(img);
                    listView1.Items.Add((i + 1).ToString());
                }
                catch (Exception ex) { continue; }
            }
        }
        void LoadListView()
        {
            LoadImageList();
            listView1.GridLines = true;
            listView1.LargeImageList = imgs;
            for (int i = 0; i < srcList.Count; i++)
            {
                ListViewItem item = new ListViewItem();
                item.ImageIndex = i;
            }
        }

        List<string> srcList = new List<string>();
        private void searchButton_Click(object sender, EventArgs e)
        {
            string userinput = titleTextBox.Text + " " + textTextBox.Text;
                if (userinput==" ")
                {
                label3.Text = "Please enter text/title";
                return;
            }
            label3.Text = " ";
           
            string tempURL = "http://images.google.com/search?q=" + userinput + "&tbm=isch&site=imghp";
            string htmlCode;
            WebClient client = new WebClient();
            htmlCode = client.DownloadString(tempURL);
            Match m;
            string HRefPattern = @"<img.*?src=""(.*?)\""";
           try
            {
                m = Regex.Match(htmlCode, HRefPattern,
                                RegexOptions.IgnoreCase | RegexOptions.Compiled,
                                TimeSpan.FromSeconds(1));
                while (m.Success)
                {
                    m = m.NextMatch();
                    srcList.Add(m.Groups[1].Value);
                }
            }
            catch (RegexMatchTimeoutException reg)
            {
                MessageBox.Show(reg.ToString());
            }
            LoadListView();
        }
        private void Create_Click(object sender, EventArgs e)
        {
             // Create a new PowerPoint File      
            Microsoft.Office.Interop.PowerPoint.Application pptFile = new Microsoft.Office.Interop.PowerPoint.Application();
            Presentation pptPresentation = pptFile.Presentations.Add(MsoTriState.msoTrue);
            CustomLayout customLayout = pptPresentation.SlideMaster.CustomLayouts[PpSlideLayout.ppLayoutText];

            // Create the first slide
            Slides firstSlide;
            _Slide slide;
            TextRange objText;
            firstSlide = pptPresentation.Slides;
            slide = firstSlide.AddSlide(1, customLayout);

            // Add title from title textbob
            objText = slide.Shapes[1].TextFrame.TextRange;
            objText.Text = titleTextBox.Text;
            objText.Font.Name = "Arial";
            objText.Font.Size = 48;

            // Add content from text textbox
            objText = slide.Shapes[2].TextFrame.TextRange;
            objText.Text = textTextBox.Text;
            objText.Font.Size = 28;

            // create new slides for images
            for (int i = 0; i < imgIndex.Count; i++)
            {
                Slides iSlide;
                _Slide picSlide;
                TextRange picRange;
                iSlide = pptPresentation.Slides;
                picSlide = iSlide.AddSlide(2, customLayout);

                picRange = picSlide.Shapes[1].TextFrame.TextRange;
                Microsoft.Office.Interop.PowerPoint.Shape shape = picSlide.Shapes[1];

                string url = srcList[imgIndex[i]];

                string filename = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".png";
                string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                try
                {
                    WebClient client = new WebClient();
                    client.DownloadFile(url, filePath + "\\" + filename);
                    string picFile = filePath + "\\" + filename;             
                    picSlide.Shapes.AddPicture(picFile, MsoTriState.msoFalse, MsoTriState.msoTrue, shape.Left, shape.Top, shape.Width, shape.Height);

                }
                catch (Exception ex) { }         
            }
          
        }
        private void closeButton_Click(object sender, EventArgs e)
        {
            srcList.Clear();
            System.Windows.Forms.Application.Exit();
        }

        List<int> imgIndex = new List<int>();
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            imgIndex.Clear();
            ListView lw = sender as ListView;
            if (lw.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in lw.SelectedItems)
                {
                    imgIndex.Add(Int32.Parse(item.Text));
                }
            }
        }
    }
}