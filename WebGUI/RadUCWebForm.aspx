<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RadUCWebForm.aspx.cs" 
    Inherits="WebGUI.Views.Home.RadUCWebForm" %>


<%--<%@ Register src="~/RadViewUC.dll" TagName="RadOcxUC" TagPrefix="RadUC"  %>--%>

<%-- Maybe need to register assembly
        https://stackoverflow.com/questions/2062794/how-to-use-activex-with-asp-net

    Others:
        https://www.codeproject.com/Articles/14533/A-Complete-ActiveX-Web-Control-Tutorial
        https://stackoverflow.com/questions/10364651/activex-control-in-asp-net
        http://www.4guysfromrolla.com/webtech/091698-1.shtml
        https://www.codeproject.com/Articles/24089/Create-ActiveX-in-NET-Step-by-Step
        https://social.msdn.microsoft.com/Forums/windows/en-US/19b08a1d-39cb-4c8a-aec7-ee6cdfcedb03/host-windows-form-user-control-in-aspnet-web-page-as-activex-control?forum=winforms
        
    --%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <div>

        <form runat="server">
            <%--<RadUC.RadOcxUC ID="Header" runat="server" />--%>

            <%--<object id="MyRadViewUserControl" height="500" width="800" classid="{4BFC22FA-5D50-3038-8983-5A9E42697335}">
            </object>--%>

             <object id="MyRadViewUserControl" height="310" width="300" classid="RadViewUC.dll" viewastext></object>

        </form>

    </div>

 <%--   <OBJECT id="MyWinControl1" name="MyWinControl1" height="400" width="400" classid="WinControls.dll#WinControls.UserControl1" VIEWASTEXT>
        <param Name="RadVisible" value="true" />
        <param Name="RadFile" value="\\rjdell\SPPIDORCL\Plant\10\101\T-101-0001.pid" />
        <param Name="RadDisplay" value="true" />
    </OBJECT>--%>
</body>
</html>
