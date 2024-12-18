using Microsoft.AspNetCore.Mvc;
using PdfSharpCore;
using PdfSharpCore.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfGeneratorController : ControllerBase
    {
        [HttpPost("FeesStructure")]
        public async Task<ActionResult> GeneratePdf(PdfRequest pdfRequest)
        {
            var data = new PdfDocument();
            string htmlContent = "<div style = 'margin: 20px auto; heigth:1000px; max-width: 600px; padding: 20px; border: 1px solid #ccc; background-color: #FFFFFF; font-family: Arial, sans-serif;' >";
            htmlContent += "<div style = 'margin-bottom: 20px; text-align: center;'>";
            htmlContent += "<img src = 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcROnYPD5QO8ZJvPQt8ClnJNPXduCeX89dSOxA&usqp=CAU' alt = 'School Logo' style = 'max-width: 100px; margin-bottom: 10px;' >";
            htmlContent += "</div>";
            htmlContent += "<p style = 'margin: 0;' >Stratford College</p>";
            htmlContent += "<p style = 'margin: 0;' > 1, Zion Road, Rathgar, Dublin 6</p>";
            htmlContent += "<p style = 'margin: 0;' > Phone: 353 - 1 - 4922315 </p>";
            htmlContent += "<p style = 'margin: 0;' > Ireland </p>";
            htmlContent += "<div style = 'text-align: center; margin-bottom: 20px;'>";
            htmlContent += "<h1> Fees Structure </h1>";
            htmlContent += "</div>";
            htmlContent += "<h3> Student Details:</h3>";
            htmlContent += "<p> Name:" + pdfRequest.Name + "</p>";
            htmlContent += "<p> STD:" + pdfRequest.Std + "</p>";
            htmlContent += "<table style = 'width: 100%; border-collapse: collapse;'>";
            htmlContent += "<thead>";
            htmlContent += "<tr>";
            htmlContent += "<th style = 'padding: 8px; text-align: left; border-bottom: 1px solid #ddd;' > Fee Description </th>";
            htmlContent += "<th style = 'padding: 8px; text-align: left; border-bottom: 1px solid #ddd;' > Amount ( € / EUR ) </th>";
            htmlContent += "</tr><hr/>";
            htmlContent += "</thead>";
            htmlContent += "<tbody>";
            decimal totalAmount = 0;
            if (pdfRequest.Fees != null && pdfRequest.Fees.Count > 0)
            {
                pdfRequest.Fees.ForEach(x =>
                {
                    htmlContent += "<tr>";
                    htmlContent += "<td style = 'padding: 8px; text-align: left; border-bottom: 1px solid #ddd;' >" + x.FeesDescription + " </td>";
                    htmlContent += "<td style = 'padding: 8px; text-align: left; border-bottom: 1px solid #ddd;' >€ " + x.Amount + "/- </td>";
                    htmlContent += "</tr>";
                    if (decimal.TryParse(x.Amount, out decimal feeAmount))
                    {
                        totalAmount += feeAmount;
                    }
                });
                htmlContent += "</tbody>";
                htmlContent += "<tfoot>";
                htmlContent += "<tr>";
                htmlContent += "<td style = 'padding: 8px; text-align: right; font-weight: bold;'> Total:</td>";
                htmlContent += "<td style = 'padding: 8px; text-align: left; border-top: 1px solid #ddd;' >€ " + totalAmount + "/- </td>";
                htmlContent += "</tr>";
                htmlContent += "</tfoot>";
            }
            htmlContent += "</table>";
            htmlContent += "</div>";
            PdfGenerator.AddPdfPages(data, htmlContent, PageSize.A4);
            byte[]? response = null;
            using (MemoryStream ms = new MemoryStream())
            {
                data.Save(ms);
                response = ms.ToArray();
            }
            string fileName = "FeesStructure" + pdfRequest.Date.ToString("yyyyMMdd") + ".pdf";
            return File(response, "application/pdf", fileName);
        }
    }
}
