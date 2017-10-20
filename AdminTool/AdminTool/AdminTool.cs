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
    public partial class AdminTool : Form
    {
        public AdminTool()
        {
            InitializeComponent();
        }

        private void AdminTool_Load(object sender, EventArgs e)
        {
            if (!JSONReader.ReadFile())
            {
                JSONReader.GenerateFile();
                MessageBox.Show("Settings file generated. Application will now exit.");
                Application.Exit();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FormManager.Instance.ShowUserForm();
        }

        private void BtnQuestionsAdmin_Click(object sender, EventArgs e)
        {
            FormManager.Instance.ShowQuestionForm();
        }
    }
}
