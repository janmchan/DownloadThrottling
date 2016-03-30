using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FileStreamer.Controllers
{
    public class HomeController : AsyncController
    {
        const string fileName = "Test";
        const string fileExt = ".iso";
        
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        
        public async Task<ActionResult> Download()
        {
            Session["IsDownloading"]  = true;            
            await ServeFiles();
            return View("Index");
        }

        public ActionResult FileMethod()
        {
            string url = Path.Combine("C:\\temp\\", fileName + fileExt);
            var fileNameNew = fileName + Guid.NewGuid().ToString() + fileExt;
            return File(url, "application/octet-stream", fileNameNew);
        }

        public ActionResult CheckStatus()
        {
            var message = Session["IsDownloading"] ?? "Null";
            return Content(message.ToString());
        }

        public ActionResult Clear()
        {
            Session["IsDownloading"] = null;
            return Content("Cleared");
        }

        private async Task ServeFiles()
        {

            const string url = @"file:///C:/temp/" + fileName + fileExt;
            var fileNameNew = fileName + Guid.NewGuid() + fileExt;
            //Create a stream for the file
            Stream stream = null;

            //This controls how many bytes to read at a time and send to the client
            const int bytesToRead = 10000;

            // Buffer to read bytes in chunk size specified above
            var buffer = new Byte[bytesToRead];

            // The number of bytes read
            try
            {
                var urlObj = new Uri(url, UriKind.Absolute);
                //Create a WebRequest to get the file
                var fileReq = (FileWebRequest)FileWebRequest.Create(urlObj);

                //Create a response for this request
                var fileResp = (FileWebResponse)fileReq.GetResponse();

                if (fileReq.ContentLength > 0)
                    fileResp.ContentLength = fileReq.ContentLength;

                //Get the Stream returned from the response
                stream = fileResp.GetResponseStream();

                // prepare the response to the client. resp is the client Response
                var resp = Response;

                //Indicate the type of data being sent
                resp.ContentType = "application/octet-stream";

                //Name the file 
                resp.AddHeader("Content-Disposition", "attachment; filename=\"" + fileNameNew + "\"");
                resp.AddHeader("Content-Length", fileResp.ContentLength.ToString());



                int length;
                do
                {
                    // Verify that the client is connected.
                    if (resp.IsClientConnected)
                    {
                        // Read data into the buffer.
                        length = stream.Read(buffer, 0, bytesToRead);

                        // and write it out to the response's output stream
                        await resp.OutputStream.WriteAsync(buffer, 0, length);

                        // Flush the data
                        resp.Flush();

                        //Clear the buffer
                        buffer = new Byte[bytesToRead];
                    }
                    else
                    {
                        // cancel the download if client has disconnected
                        length = -1;
                       
                    }
                } while (length > 0); //Repeat until no data is read
            }
            finally
            {
                if (stream != null)
                {
                    //Close the input stream
                    stream.Close();
                }
                Session["IsDownloading"] = false;

            }

        }

        
    }
}
