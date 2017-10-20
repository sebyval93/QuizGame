using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LitJson;
using System.IO;

namespace AdminTool
{
    public partial class QuestionAdmin : Form
    {
        List<Question> questionList;

        enum Mode : byte { ADD, EDIT };
        DBManager dbManager = DBManager.Instance;

        Mode currentMode;
        int currentID; 

        public QuestionAdmin()
        {
            InitializeComponent();
            currentMode = Mode.ADD;
            currentID = -1;
            questionList = new List<Question>();

            Width = splitContainer1.Panel1.Width;
            splitContainer1.Panel2Collapsed = true;
            btnEditQuestions.Text = "Edit Questions " + char.ConvertFromUtf32(0x25BA);
            splitContainer1.Panel1MinSize = 447;
            splitContainer1.Panel2MinSize = 200;
        }

        int GetAnswerKey()
        {
            int result = -1;
            if (radioButton1.Checked)
                result = 1;
            else if (radioButton2.Checked)
                result = 2;
            else if (radioButton3.Checked)
                result = 3;
            else if (radioButton4.Checked)
                result = 4;

            return result;
        }

        private void UpdateState()
        {
            if (currentMode == Mode.ADD)
            {
                currentID = -1;
                btnDeleteQuestion.Enabled = false;
                btnEditQuestion.Enabled = true;
                btnEditQuestion.Text = "Add";
                btnEditQuestions.Text = "Edit Questions " + char.ConvertFromUtf32(0x25BA);
                statusLabel.Text = "";
            }
            else
            {
                LoadQuestions();
                DisplayQuestions();
                DisableEditing();
                btnEditQuestion.Text = "Edit";
                btnEditQuestions.Text = "Add Questions " + char.ConvertFromUtf32(0x25C4);
            }
        }

        private void DisplayQuestions()
        {
            treeQuestions.Nodes.Clear();
            for (int i = 0; i < questionList.Count; ++i)
            {
                Question q = questionList[i];
                TreeNode node = new TreeNode(q.id + ": " + q.questionText);
                node.Tag = new Tuple<int, int>(q.id, i);
                node.Nodes.Add(new TreeNode(q.answer1));
                node.Nodes.Add(new TreeNode(q.answer2));
                node.Nodes.Add(new TreeNode(q.answer3));
                node.Nodes.Add(new TreeNode(q.answer4));
                node.Nodes[q.answerKey - 1].Text = "#" + node.Nodes[q.answerKey - 1].Text;

                treeQuestions.Nodes.Add(node);
            }
            statusLabel.Text = "Found " + questionList.Count + " question(s).";
        }

        private void EnableEditing()
        {
            btnEditQuestion.Enabled = true;
            btnDeleteQuestion.Enabled = true;
        }

        private void DisableEditing()
        {
            btnEditQuestion.Enabled = false;
            btnDeleteQuestion.Enabled = false;
        }

        void ResetControls()
        {
            questionText.Text = "";
            answer1.Text = "";
            answer2.Text = "";
            answer3.Text = "";
            answer4.Text = "";

            if (radioButton1.Checked)
                radioButton1.Checked = false;
            else if (radioButton2.Checked)
                radioButton2.Checked = false;
            else if (radioButton3.Checked)
                radioButton3.Checked = false;
            else
                radioButton4.Checked = false;
        }

        private void GetQuestion(Question q)
        {
            questionText.Text = q.questionText;
            answer1.Text = q.answer1;
            answer2.Text = q.answer2;
            answer3.Text = q.answer3;
            answer4.Text = q.answer4;

            switch (q.answerKey)
            {
                case 1:
                    radioButton1.Checked = true;
                    break;
                case 2:
                    radioButton2.Checked = true;
                    break;
                case 3:
                    radioButton3.Checked = true;
                    break;
                case 4:
                    radioButton4.Checked = true;
                    break;
                default:
                    Console.WriteLine("Error in question switch!");
                    break;
            }

        }

        private void LoadQuestions()
        {
            questionList = dbManager.LoadAllQuestions();
        }

        private void nextQuestion_Click(object sender, EventArgs e)
        {
            if (questionText.TextLength == 0 || answer1.TextLength == 0 || answer2.TextLength == 0 || answer3.TextLength == 0 || answer4.TextLength == 0 )
            {
                MessageBox.Show("Please complete all fields.");
                return;
            }

            if (!radioButton1.Checked && !radioButton2.Checked && !radioButton3.Checked && !radioButton4.Checked)
            {
                MessageBox.Show("Please select a correct answer.");
                return;
            }

            if (currentMode == Mode.ADD)
            {
                dbManager.AddQuestion(questionText.Text, answer1.Text, answer2.Text, answer3.Text, answer4.Text, GetAnswerKey());
                ResetControls();
            }
            else
            {
                if (currentID != -1)
                {
                    dbManager.UpdateQuestion(currentID, questionText.Text, answer1.Text, answer2.Text, answer3.Text, answer4.Text, GetAnswerKey());
                    DisplayQuestions();
                }
            }

        }

        private void btnDeleteQuestion_Click(object sender, EventArgs e)
        {
            dbManager.DeleteQuestion(currentID);
            LoadQuestions();
            DisplayQuestions();
            ResetControls();
            currentID = -1;
        }

        private void btnShowQuestions_Click(object sender, EventArgs e)
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

        private void splitContainer1_Panel1_Resize(object sender, EventArgs e)
        {
            controlsPanel.Location = new Point((splitContainer1.Panel1.ClientSize.Width - controlsPanel.Width) / 2, 
                (splitContainer1.Panel1.ClientSize.Height - controlsPanel.Height) / 2);
        }

        private void treeQuestions_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                if (((Tuple<int, int>)e.Node.Tag).Item1 != currentID)
                {
                    Console.WriteLine(((Tuple<int, int>)e.Node.Tag).Item1);
                    currentID = ((Tuple<int, int>)e.Node.Tag).Item1;
                    GetQuestion(questionList[((Tuple<int, int>)e.Node.Tag).Item2]);
                    EnableEditing();
                }
            }
        }

        private void QuestionAdmin_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
                FormManager.Instance.ShowMainFrom();
        }
    }
}
