using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

public partial class Collections : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            this.GrdLoad();
        }
    }

    protected void Input_Click(object sender, EventArgs e)
    {
        if(txtName.Text == string.Empty)
        {
            return;
        }
        
        // Въвеждане на запис
        if (dbClass.InsertData(txtName.Text, "collections"))
        {
            this.GrdLoad();
        }
        else
        {
            Response.Write("Database error.");
        }
    }

    private bool GrdLoad()
    {
        // load grid data
        string errMsg = "";

        gridView.DataSource = dbClass.GetTableForGrid(ref errMsg, "collections");
        gridView.DataBind();

        // have database error
        if (errMsg.Length > 0)
        {
            lblError.Visible = true;
            lblError.Text = errMsg;
            return false;
        }

        txtName.Text = "";

        return true;
    }

    protected void gridView_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        // Обработка на команда през грида
        if (e.CommandName == "grdCmd_Delete")
        {
            int row = 0;
            int id = 0;

            if (int.TryParse(e.CommandArgument.ToString(), out row) == false) return;
            if (!int.TryParse(gridView.DataKeys[row].Values[0].ToString(), out id))
            {
                Response.Redirect("Default.aspx", false);
                return;
            }

            if (id > 0)
            {
                if (!dbClass.DeleteCollectionData(id))
                {
                    lblError.Visible = true;
                    lblError.Text = "Неуспешно изтриване!";
                    return;
                }
                else
                {
                    lblError.Visible = true;
                    lblError.Text = "Успешно изтриване!";
                }
                this.GrdLoad();
            }
        }
    }

    protected void OnRowEditing(object sender, GridViewEditEventArgs e)
    {
        gridView.EditIndex = e.NewEditIndex;
        this.GrdLoad();
    }

    protected void OnUpdate(object sender, EventArgs e)
    {
        GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;
        string name = (row.Cells[0].Controls[0] as TextBox).Text;
        int id = Convert.ToInt32(gridView.DataKeys[row.RowIndex].Values[0]);

        if(name.Length > 0)
        {
            if (!dbClass.UpdateData(id, name, "collections"))
            {
                lblError.Visible = true;
                lblError.Text = "Неуспешна промяна!";
                return;
            }
            else
            {
                lblError.Visible = true;
                lblError.Text = "Успешна промяна!";
            }
            gridView.EditIndex = -1;
        }
        else
        {
            lblError.Visible = true;
            lblError.Text = "Името на колекцията е невалидно!";
        }

        this.GrdLoad();
    }

    protected void OnCancel(object sender, EventArgs e)
    {
        gridView.EditIndex = -1;
        this.GrdLoad();
    }
}