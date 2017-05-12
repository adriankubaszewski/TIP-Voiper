using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjTIP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            textBox2.PasswordChar = '\u25CF';

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text;
            if (login == "user1")
            {
                Form2 f2 = new Form2(login);
                f2.Show();
                
            }
            else
            {
                label1.Text = "BŁĘDNY LOGIN";
            }
        }
    }
}
