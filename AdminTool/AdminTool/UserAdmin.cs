using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdminTool
{
    public partial class UserAdmin : Form
    {
        List<User> userList;

        enum Mode : byte { ADD, EDIT };
        DBManager dbManager = DBManager.Instance;

        Mode currentMode;
        int currentID;

        public UserAdmin()
        {
            InitializeComponent();
            currentMode = Mode.ADD;
            currentID = -1;
            userList = new List<User>();


            Width = splitContainer1.Panel1.Width;
            splitContainer1.Panel2Collapsed = true;
            btnEditUsers.Text = "Edit Users " + char.ConvertFromUtf32(0x25C4);
            splitContainer1.Panel1MinSize = 381;
            splitContainer1.Panel2MinSize = 146;
        }

        private void UpdateState()
        {
            if (currentMode == Mode.ADD)
            {
                currentID = -1;
                btnRemoveUser.Enabled = false;
                btnEditUsers.Enabled = true;
                btnAddUser.Text = "Add";
                btnEditUsers.Text = "Edit Users " + char.ConvertFromUtf32(0x25BA);
                statusLabel.Text = "";
            }
            else
            {
                LoadUsers();
                DisplayUsers();
                DisableEditing();
                btnAddUser.Text = "Edit";
                btnEditUsers.Text = "Add Users " + char.ConvertFromUtf32(0x25C4);
            }
        }

        private void DisplayUsers()
        {
            treeUsers.Nodes.Clear();
            for (int i = 0; i < userList.Count; ++i)
            {
                User u = userList[i];
                TreeNode node = new TreeNode(u.id + ": " + u.username);
                node.Tag = new Tuple<int, int>(u.id, i);
                node.Nodes.Add(new TreeNode(u.nickname));
                node.Nodes.Add(new TreeNode(u.score.ToString()));

                treeUsers.Nodes.Add(node);
            }
            statusLabel.Text = "Found " + userList.Count + " user(s).";
        }

        private void EnableEditing()
        {
            btnEditUsers.Enabled = true;
            btnRemoveUser.Enabled = true;
        }

        private void DisableEditing()
        {
            btnEditUsers.Enabled = false;
            btnRemoveUser.Enabled = false;
        }

        void ResetControls()
        {
            txtUsername.Text = "";
            txtNickname.Text = "";
            txtScore.Text = "";
            txtPassword.Text = "";
        }

        private void GetUser(User u)
        {
            txtUsername.Text = u.username;
            txtNickname.Text = u.nickname;
            txtScore.Text = u.score.ToString();
        }

        private void LoadUsers()
        {
            userList = dbManager.LoadAllUsers();
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void splitContainer1_Panel1_Resize(object sender, EventArgs e)
        {
            controlsPanel.Location = new Point((splitContainer1.Panel1.ClientSize.Width - controlsPanel.Width) / 2,
                (splitContainer1.Panel1.ClientSize.Height - controlsPanel.Height) / 2);
        }

        private void txtScore_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            if (txtNickname.TextLength == 0 || txtUsername.TextLength == 0 || txtScore.TextLength == 0)
            {
                MessageBox.Show("Please complete all relevant fields.");
                return;
            }

            if (currentMode == Mode.ADD)
            {
                if (txtPassword.TextLength == 0)
                {
                    MessageBox.Show("Please complete the password field");
                    return;
                }
                string passHash = dbManager.CalculateHash(txtUsername.Text, txtPassword.Text);

                int score = 0;
                if (!Int32.TryParse(txtScore.Text, out score))
                    return;

                dbManager.AddUser(txtUsername.Text, txtNickname.Text, score, passHash);
                ResetControls();
            }
            else
            {
                if (currentID != -1)
                {
                    int score = 0;
                    if (!Int32.TryParse(txtScore.Text, out score))
                        return;

                    dbManager.UpdateUser(currentID, txtUsername.Text, txtNickname.Text, score, txtPassword.Text);
                    DisplayUsers();
                }
            }
        }

        private void btnRemoveUser_Click(object sender, EventArgs e)
        {
            dbManager.DeleteUser(currentID);
            LoadUsers();
            DisplayUsers();
            ResetControls();
            currentID = -1;
        }

        private void btnEditUsers_Click(object sender, EventArgs e)
        {
            if (splitContainer1.Panel2Collapsed)
            {
                if (Width < splitContainer1.Panel1MinSize + splitContainer1.Panel2MinSize)
                    Width = splitContainer1.Panel1MinSize + splitContainer1.Panel2MinSize + 100;

                splitContainer1.Panel2Collapsed = false;
                currentMode = Mode.EDIT;
                UpdateState();
            }
            else
            {
                splitContainer1.Panel2Collapsed = true;
                currentMode = Mode.ADD;
                ResetControls();
                UpdateState();
            }
        }

        private void treeUsers_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                if (((Tuple<int, int>)e.Node.Tag).Item1 != currentID)
                {
                    Console.WriteLine(((Tuple<int, int>)e.Node.Tag).Item1);
                    currentID = ((Tuple<int, int>)e.Node.Tag).Item1;
                    GetUser(userList[((Tuple<int, int>)e.Node.Tag).Item2]);
                    EnableEditing();
                }
            }
        }

        private void UserAdmin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
                FormManager.Instance.ShowMainFrom();
        }
    }
}
