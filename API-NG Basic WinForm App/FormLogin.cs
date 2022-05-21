using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace API_NG_App
{
    public partial class FrmLogin : Form
    {
        private string m_password;
        private string m_username;

        public FrmLogin()
        {
            InitializeComponent();

            if (Connection.m_appKey.Length > 0)
            {
                lblWarning.Visible = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }


        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBoxUsername.Text))
            {
                //Focus box before showing a message
                txtBoxUsername.Focus();
                MessageBox.Show("Enter your username", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (string.IsNullOrEmpty(txtBoxPassword.Text))
            {
                txtBoxPassword.Focus();
                MessageBox.Show("Enter your password", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            lblLoginFeedback.ForeColor = Color.Black;

            m_password = txtBoxPassword.Text;
            m_username = txtBoxUsername.Text;

            // try to login
            lblLoginFeedback.Text = "Logging in, please wait...";

            //var sessionToken = getSessionToken(m_username, m_password, out string errStr);
            var sessionToken = Connection.getSessionToken(m_username, m_password, out string errStr);

            // this stores jsonRpcClient in Connection static variable, accessible from Main Form1
            Connection.createJsonRpcClient(sessionToken);

            // hmmm interesting issue, the lbl control is not updating ?
            // This is probably cos we are running this as a Modal Dialog disrupting Windows msg pump
            // quick kludge solution is force an explicit control refresh()
            lblLoginFeedback.Text = errStr;
            lblLoginFeedback.Refresh();

            if (errStr == "OK")
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                lblLoginFeedback.ForeColor = Color.Red;
                lblLoginFeedback.Refresh();

                // put this back to close down form on failed login
                //Thread.Sleep(4000); // Time to read error msg
                //DialogResult = DialogResult.No;
            }
        }

        private void txtBoxPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;  //trying to stop the annoying beep ?

                // Then Enter key was pressed
                btnLogin_Click(sender, e);
            }
        }
    }
}
