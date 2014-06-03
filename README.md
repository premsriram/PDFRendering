PDFRendering
============

Features
•	Specific device to design the PDF rendering
•	Configuration to set up the style, button image etc
•	Sub layout to render the content in PDF based on the configuration.
Considerations:
•	The markup should be XHTML so that the pdf can be rendered.
•	CSS should be specifically written for PDF including the basic elements such as H1, H2 etc
Usages:
•	Export product\recipe in specific format other than the web page.
•	Use the export feature in multiple devices
•	Server side processing.

Installation:	
	Install the package using sitecore package installer.
Configuration:
•	Set Configuration
o	Create a configuration item using the template (Template -> PDFView -> PDFConfiguration)
o	Set the export button image (This will be showed in the web page )
o	Select the PDF style that will be applied
o	Select the device which is used for adding the PDF rendering. (Recommended to use PDF device which comes as part of this package)
•	Rendering Configuration
o	Select the page which has to be exported to PDF. It can be header if it’s applicable for all the pages.
o	Drop the sub layout from in the appropriate placeholder
•	Export Content
o	Browse the page
o	Onclick of the export button the content of the current page will be rendered as PDF
o	The styles will be included from the configuration.
Tips:
•	Check the source code and integrate the control to any of the existing projects
•	Refer the iTextSharp documentation for more customization
