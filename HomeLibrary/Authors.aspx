<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Authors.aspx.cs" Inherits="Authors" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
     <title>Въвеждане на нов автор</title>
</head>
    <a href="Default.aspx"> Към общия списък с книги и автори </a>
    <p>Въвеждане на нов автор:</p>
<body>
    <form id="form1" runat="server">
        <div>Име на нов автор:
	         <asp:Textbox runat="server" id="txtName" />
	         <asp:Button runat="server" text="Въведи" ID="btnInput" OnClick="Input_Click" />
	    </div>
        <p></p>
        <table border = "1">
            <tr>
                <td colspan="3" align="center">&nbsp<font face="verdana" size="2"><strong><asp:Label runat="server" ID="lblError" Text="" ForeColor="Red" EnableViewState="false" Width="600" Visible="false"></asp:Label></strong></font></td>
            </tr>  
            <tr>
                <td colspan="3" align="center">
                    <asp:GridView ID="gridView" runat="server" BorderStyle="None" DataKeyNames="ID" Width="600" 
                        AutoGenerateColumns="False" 
                        OnRowDeleting="gridView_RowDeleting" 
                        OnRowEditing="gridView_RowEditing"
                        OnRowUpdating="gridView_RowUpdating"
                        OnRowCancelingEdit="OnRowCancelingEdit">
                        <Columns>
                            <asp:BoundField HeaderText="Име на автор:" SortExpression="Name" DataField="Name" HtmlEncode="false"><ItemStyle HorizontalAlign="Left" Width="250" /></asp:BoundField>
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center" ItemStyle-Width="40">
                                <ItemTemplate>
                                    <asp:LinkButton Text="Промяна" runat="server" CommandName="Edit" HorizontalAlign="Center" />
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:LinkButton Text="Редактиране" runat="server" CommandName="Update" HorizontalAlign="Center" />
                                    <asp:LinkButton Text="Отказ" runat="server" CommandName="Cancel" HorizontalAlign="Center" />
                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:ButtonField ButtonType="Link" text="Изтрий" CommandName="Delete" ><ItemStyle HorizontalAlign="Center"  Width="40" /></asp:ButtonField>
			             </Columns>
                    </asp:GridView>
                </td>
            </tr>
	    </table>
    </form>
</body>
</html>
