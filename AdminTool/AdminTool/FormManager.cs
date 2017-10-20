using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdminTool
{
    class FormManager
    {
        private static FormManager instance = new FormManager();
        private AdminTool mainForm;
        private UserAdmin userForm;
        private QuestionAdmin questionForm;

        private FormManager()
        {
            mainForm = new AdminTool();
        }

        public Form GetMainForm()
        {
            return mainForm;
        }

        public static FormManager Instance
        {
            get
            {
                return instance;
            }
        }

        public void ShowUserForm()
        {
            mainForm.Hide();
            userForm = new UserAdmin();
            userForm.Show();
        }

        public void ShowQuestionForm()
        {
            mainForm.Hide();
            questionForm = new QuestionAdmin();
            questionForm.Show();
        }

        public void ShowMainFrom()
        {
            mainForm.Show();
        }
    }
}
