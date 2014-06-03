<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PdfView.ascx.cs" Inherits="PDFRendering.PdfView" %>
<div id="pdfview" class="pdfviewstyle">
<asp:literal id="litPdfViewMsg" runat="server"></asp:literal>
<asp:imagebutton id="btnPdfView" runat="server" class="pdfbuttonstyle" 
        onclick="btnPdfView_Click"></asp:imagebutton>
</div>
