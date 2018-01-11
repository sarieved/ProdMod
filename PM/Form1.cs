using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Web;

namespace PM
{
    public partial class Form1 : Form
    {
        List<Fact> facts;
        List<Fact> trueFacts;
        List<Rule> rules;
        public class Fact
        {
            public string ID;
            public string inform;
            public double coeff;
            public Fact()
            {

            }
            public Fact(Fact f)
            {
                ID = f.ID;
                inform = f.inform;
                coeff = 0;
            }
            public Fact(string id, string inf)
            {
                ID = id;
                inform = inf;
                coeff = 0;
            }
            public override string ToString()
            {
                return ID + ": " + coeff.ToString() + " " + inform;
            }
            public void set_coeff(double c)
            {
                coeff = c;
            }
        }

        public class Rule
        {
            public string ID;
            public List<string> left;
            public string right;
            public string inform;
            public int check;
            public double coeff;
            public double min;
            public Rule()
            {
                check = 0;
            }
            public Rule(string id, List<string> l, string r, string inf, string coef)
            {
                ID = id;
                left = l;
                right = r;
                inform = inf;
                check = 0;
                coeff = Convert.ToDouble(coef);
                min = double.MaxValue;
            }

            public Rule(Rule r2)
            {
                ID = r2.ID;
                left = r2.left;
                right = r2.right;
                inform = r2.inform;
                check = r2.check;
                coeff = r2.coeff;
                min = r2.min;
            }
            public override string ToString()
            {
                string left_part = "";
                for (int i = 0; i < left.Count; i++)
                {
                    left_part += left[i] + " ";
                }
                return ID + ":|c =" + coeff.ToString() + "| " + left_part + "-->" + right + " " + inform;
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void загрузитьБазуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string fname = openFileDialog1.FileName;
            facts = new List<Fact>();
            rules = new List<Rule>();
            using (StreamReader reader = new StreamReader(File.Open(fname, FileMode.Open)))
            {
                string text;

                while ((text = reader.ReadLine()) != null)
                {
                    string[] bits = text.Split(';');
                    if (bits[0][0] == 'f')
                    {
                        facts.Add(new Fact(bits[0], bits[1]));
                        listBox1.Items.Add(new Fact(bits[0], bits[1]));
                    }
                    else
                    {
                        string[] left = bits[1].Split(',');
                        List<string> l = new List<string>();
                        for (int i = 0; i < left.Length; i++)
                            l.Add(left[i]);
                        rules.Add(new Rule(bits[0], l, bits[2], bits[3], bits[4]));
                        listBox2.Items.Add(new Rule(bits[0], l, bits[2], bits[3], bits[4]));
                    }
                }
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Fact F = new Fact((Fact)listBox1.SelectedItem);
            F.set_coeff(Convert.ToDouble((numericUpDown1.Value)));
            listBox3.Items.Add(F);
        }

        public int ind_find(List<Fact> l, string id)
        {
            int ind = -1;
            for (int i = 0; i < l.Count; ++i)
            {
                if (l[i].ID == id)
                {
                    ind = i;
                    break;
                }
            }
            return ind;
        }

        public int ind_lb_find(string id)
        {
            int res = -1;
            for (int i = 0; i < listBox4.Items.Count; ++i)
                if (((Fact)listBox4.Items[i]).ID == id)
                {
                    res = i;
                    break;
                }
            return res;
        }

        public double coef_c(double c1, double c2)
        {
            double res = 0;
            if (c1 > 0 && c2 > 0)
                res = c1 + c2 * (1 - c1);
            else if (c1 < 0 && c2 < 0)
                res = c1 + c2 * (1 + c1);
            else
                res = (c1 + c2) / (1 - Math.Min(Math.Abs(c1), Math.Abs(c2)));
            return res;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            trueFacts = new List<Fact>();

            for (int i = 0; i < listBox3.Items.Count; i++)
            {
                trueFacts.Add((Fact)listBox3.Items[i]);
            }

            bool stop = false;

            while (!stop)
            {
                stop = true;
                List<int> indToAdd = new List<int>();
                for (int i = 0; i < trueFacts.Count; ++i)
                {
                    for (int j = 0; j < rules.Count; ++j)
                    {
                        if (trueFacts[i].ID.Equals(rules[j].right))
                        {
                            rules[j].check = -1;
                            continue;
                        }
                        int ind = rules[j].left.IndexOf(trueFacts[i].ID);
                        if (ind == -1)
                            continue;
                        else
                        {
                            rules[j].check += 1;
                            rules[j].min = Math.Min(rules[j].min, trueFacts[i].coeff);
                        }
                    }
                }

                for (int i = 0; i < rules.Count; ++i)
                {
                    if (rules[i].check == rules[i].left.Count)
                    {
                        int ind = ind_find(facts, rules[i].right);
                        listBox5.Items.Add(rules[i].ToString());
                        Fact f = new Fact(facts[ind]);
                        f.set_coeff(rules[i].min * rules[i].coeff);
                        if (ind_find(trueFacts, f.ID) == -1)
                        {
                            trueFacts.Add(f);
                            listBox4.Items.Add(f);
                        }
                        else
                        {
                            int lb_ind = ind_lb_find(f.ID);
                            listBox4.Items.RemoveAt(lb_ind);
                            int tfacts_ind = ind_find(trueFacts, f.ID);
                            double new_coeff = coef_c(trueFacts[tfacts_ind].coeff, f.coeff);
                            trueFacts[tfacts_ind].set_coeff(new_coeff);
                            f.set_coeff(new_coeff);
                            listBox4.Items.Add(f);
                        }
                        stop = false;
                    }
                    rules[i].check = 0;
                    rules[i].min = double.MaxValue;
                }
            }
        }

        private void listBox3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.listBox3.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                listBox3.Items.RemoveAt(index);
            }
        }

        private void очиститьВсёToolStripMenuItem_Click(object sender, EventArgs e)
        {
            trueFacts = new List<Fact>();
            int c1 = listBox3.Items.Count;
            int c2 = listBox4.Items.Count;
            int c3 = listBox5.Items.Count;

            for (int i = 0; i < c1; i++)
            {
                listBox3.Items.RemoveAt(0);
            }
            for (int i = 0; i < c2; i++)
            {
                listBox4.Items.RemoveAt(0);
            }
            for (int i = 0; i < c3; i++)
            {
                listBox5.Items.RemoveAt(0);
            }
        }
    }
}
