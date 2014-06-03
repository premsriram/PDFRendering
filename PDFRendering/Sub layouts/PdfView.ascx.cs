using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;
using System.Text;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml.html;
using System.Threading;
using Sitecore.Web.UI.WebControls;
using Sitecore.Layouts;


namespace PDFRendering
{
    public partial class PdfView : System.Web.UI.UserControl
    {
        private Sitecore.Data.Items.Item dataSourceItem;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (Parent is Sublayout)
                {
                    this.dataSourceItem = Sitecore.Context.Database.GetItem(((Sublayout)Parent).DataSource);

                    RenderPdfContent();

                    if (this.dataSourceItem == null)
                    {
                        litPdfViewMsg.Text = "Oops...Datasource is not yet configured.";
                    }
                }
            }
            catch (Exception ex)
            {
                litPdfViewMsg.Text = ex.Message;
            }

        }

        private void RenderPdfContent()
        {
            if (this.dataSourceItem["Button Image"] != null)
            {
                Sitecore.Data.Fields.ImageField imgField = ((Sitecore.Data.Fields.ImageField)dataSourceItem.Fields["Button Image"]);
                btnPdfView.ImageUrl = Sitecore.Resources.Media.MediaManager.GetMediaUrl(imgField.MediaItem);
            }
        }

        protected void btnPdfView_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                string Content = GetRenderingContent();

                string css = GetCSSContent();
                //GetPDFStream(Content, "");
                MemoryStream memStream = GetPDFStream(Content, css);
                if (memStream != null)
                {
                    memStream.Position = 0;
                    SendStream(memStream, (int)memStream.Length, "application\\pdf");
                }
            }
            catch (Exception ex)
            {
                litPdfViewMsg.Text = ex.Message;
            }
        }

        private string GetCSSContent()
        {
            string CSSContent = "";
            Sitecore.Data.Fields.MultilistField printstyles = this.dataSourceItem.Fields["PDF Styles"];
            if (printstyles != null)
            {
                Sitecore.Data.Items.Item[] items = printstyles.GetItems();

                if (items != null && items.Length > 0)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        string path = Sitecore.Resources.Media.MediaManager.GetMediaUrl(items[i]);
                        string baseurl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');

                        CSSContent = CSSContent + GenerateResponseXMl(baseurl + path);
                    }
                }
            }
            return CSSContent;


        }

        private string GetRenderingContent()
        {
            StringWriter sw = new StringWriter();

            //Sitecore.Data.Fields.ReferenceField  device = this.dataSourceItem.Fields["Use Device"];

            var pdfurl = new Sitecore.Text.UrlString(HttpContext.Current.Request.Url.ToString());
            pdfurl.Add("sc_device", this.dataSourceItem["Use Device"]);
            
            string html = GenerateResponseXMl(pdfurl.ToString());
            return html;
        }

        public static string GenerateResponseXMl(string url)
        {
            string _responseString = string.Empty;
            System.Net.HttpWebRequest request1 = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            request1.Method = "GET";
            request1.Accept = "*/*";
            System.Net.HttpWebResponse wResponse = (System.Net.HttpWebResponse)request1.GetResponse();
            System.IO.StreamReader strresponseReader = null;
            strresponseReader = new System.IO.StreamReader(wResponse.GetResponseStream());
            _responseString = strresponseReader.ReadToEnd();

            if (_responseString == string.Empty)
            {
                _responseString = wResponse.StatusDescription;
            }

            _responseString = _responseString.Replace("xmlns=\"http://schemas.datacontract.org/2004/07/\"", "");
            _responseString = _responseString.Replace("xmlns=\"\"", "");

            return _responseString;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
        }

        private MemoryStream GetPDFStream(string htmlContent, string CSSContent)
        {
            iTextSharp.text.Document document = new iTextSharp.text.Document();
            try
            {

                MemoryStream memStream = new System.IO.MemoryStream();
                PdfWriter writer = PdfWriter.GetInstance(document, memStream);

                document.Open();

                var interfaceProps = new Dictionary<string, Object>();
                var ih = new ImageHander() { BaseUri = Request.Url.ToString() };

                interfaceProps.Add(HTMLWorker.IMG_PROVIDER, ih);

                var cssResolver = new StyleAttrCSSResolver();

                var CSSStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CSSContent));

                var cssFile = XMLWorkerHelper.GetCSS(CSSStream);
                cssResolver.AddCss(cssFile);

                CssAppliers ca = new CssAppliersImpl();
                HtmlPipelineContext hpc = new HtmlPipelineContext(ca);
                hpc.SetTagFactory(Tags.GetHtmlTagProcessorFactory());

                PdfWriterPipeline pdf = new PdfWriterPipeline(document, writer);
                CssResolverPipeline css = new CssResolverPipeline(cssResolver, new HtmlPipeline(hpc, pdf));

                XMLWorker xworker = new XMLWorker(css, true);
                XMLParser p = new XMLParser(xworker);


                var HTMLStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(htmlContent));
                var CSSStream1 = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CSSContent));

                using (TextReader sr = new StringReader(htmlContent))
                {
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, HTMLStream, CSSStream1);
                    //p.Parse(sr);
                }

                writer.CloseStream = false;
                document.Close();

                return memStream;
            }
            catch (Exception ex)
            {
                document.Close();
                litPdfViewMsg.Text = ex.Message;
                //throw ex;
                return null;
            }

        }

        private void SendStream(Stream stream, int contentLen, string contentType)
        {
            try
            {
                Response.ClearContent();
                Response.ContentType = contentType;
                Response.AppendHeader("content-Disposition", string.Format("inline;filename=file.pdf"));
                Response.AppendHeader("content-length", contentLen.ToString());
                stream.CopyTo(Response.OutputStream);
                Response.End();
                Response.Flush();
                Response.Clear();
            }
            catch (Exception ex)
            {
                //do nothing
            }
        }

    }

    public class ImageHander : iTextSharp.text.html.simpleparser.IImageProvider
    {
        public string BaseUri;
        public iTextSharp.text.Image GetImage(string src,
        IDictionary<string, string> h,
        ChainedProperties cprops,
        IDocListener doc)
        {
            string imgPath = string.Empty;

            if (src.ToLower().Contains("http://") == false)
            {
                imgPath = HttpContext.Current.Request.Url.Scheme + "://" +

                HttpContext.Current.Request.Url.Authority + src;
            }
            else
            {
                imgPath = src;
            }

            return iTextSharp.text.Image.GetInstance(imgPath);
        }

    }


}