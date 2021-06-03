using System;
using System.Windows.Forms;

namespace MapGenerator
{
    public partial class Form_info : Form
    {
        private Form_contol form_contol;

        public Form_info()
        {
            InitializeComponent();
        }

        private void Form_info_Load(object sender, EventArgs e)
        {
            form_contol = Owner as Form_contol;
        }

        private void Form_info_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}