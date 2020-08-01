<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RadUCWebForm.aspx.cs" 
    Inherits="WebGUI.Views.Home.RadUCWebForm" %>


<%--<%@ Register src="~/RadViewUC.dll" TagName="RadOcxUC" TagPrefix="RadUC"  %>--%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <div>

        <%--<form runat="server">
            <RadUC.RadOcxUC ID="Header" runat="server" />
        </form>--%>

        <object id="MyRadViewUserControl" height="800" width="500" 
                classid="http:RadViewUC.dll#RadViewUC.RadViewUserControl" VIEWASTEXT>
            <%--<PARAM NAME="Title" VALUE="My Title">--%>
        </object>
    </div>

 <%--   <OBJECT id="MyWinControl1" name="MyWinControl1" height="400" width="400" classid="WinControls.dll#WinControls.UserControl1" VIEWASTEXT>
        <param Name="RadVisible" value="true" />
        <param Name="RadFile" value="\\rjdell\SPPIDORCL\Plant\10\101\T-101-0001.pid" />
        <param Name="RadDisplay" value="true" />
    </OBJECT>--%>
</body>
</html>
