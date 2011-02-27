using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
/* This program is free software. It comes without any warranty, to
 * the extent permitted by applicable law. You can redistribute it
 * and/or modify it under the terms of the Do What The Fuck You Want
 * To Public License, Version 2, as published by Sam Hocevar. See
 * http://sam.zoy.org/wtfpl/COPYING for more details. */ 

namespace LunchShedualMaker
{
   
    public partial class Form1 : Form
    {
        public string filePath = "Data.txt";
        public string schedualBaseName = "Lunch";
        public List<kid> kids= new List<kid>();
        public List<kid> tableLeadCantidates = new List<kid>();
        public List<kid> alreadyBeenLead = new List<kid>();
        public week thisWeek = new week();
        public void showHelp()
        {
            string help = "";
            if (File.Exists("Instructions.txt"))
            {
                StreamReader file = null;
                try
                {
                    String line;
                    file = new StreamReader("Instructions.txt");
                    while ((line = file.ReadLine()) != null)
                    {
                        help += line + "\r\n";
                    }
                }
                finally
                {
                    if (file != null)
                        file.Close();
                }

            }
            textBox1disp.Text = help;
        }
        public void loadFromFile()
        {
            kids.Clear();
            if (File.Exists(filePath))
            {
                StreamReader file = null;
                try
                {
                    String line;
                    file = new StreamReader(filePath);
                    while ((line = file.ReadLine()) != null)
                    {
                        int hasl = line.IndexOf("has");
                        kids.Add(new kid(
                            line.Substring(0,hasl-1).Trim(),
                            !line.Contains("hasn't")
                            ));

                        
                    }
                }
                finally
                {
                    if (file != null)
                        file.Close();

                    kids = Shuffle<kid>(kids);
                    debugKids();
                }
            }
            else
            {
                File.Create(filePath);
            }
        }
        public void writeToFile()
        {
            //first save the new kids database
            TextWriter tw = new StreamWriter(filePath+"");
            foreach (kid k in kids)
            {
                tw.WriteLine(k.ToString());
            }
            tw.Close();
            //now save the scheduakl to a unique name
            int n = 1;
            while (File.Exists(schedualBaseName + n.ToString() + ".txt"))
            {
                n++;
            }
            //save text file
            tw = new StreamWriter(schedualBaseName + n.ToString() + ".txt");
            tw.WriteLine(thisWeek.ToString());
            tw.Close();
            //save csv?



        }
        public void debugKids()
        {
            String outp ="";
            foreach (kid k in kids)
            {
                outp += k.ToString()+"\r\n";
            }
            textBox1debug.Text=outp;

        }
        public void assignTables()
        {
            thisWeek = new week();
            alreadyBeenLead.Clear();
            tableLeadCantidates.Clear();
            //split up the leads first
            foreach (kid k in kids)
            {
                if (k.beenLead)
                {
                    alreadyBeenLead.Add(k);
                }
                else
                {
                    tableLeadCantidates.Add(k);
                }
            }
            //randomize order
            alreadyBeenLead = Shuffle<kid>(alreadyBeenLead);
            tableLeadCantidates = Shuffle<kid>(tableLeadCantidates);
            int numberOfTables = (int)Math.Ceiling((decimal)kids.Count / (decimal)numericUpDown1.Value);
            for (int s = 0; s < numberOfTables; s++)
            {
                table t = new table();
                //pick a lead
                if (tableLeadCantidates.Count > 0)
                {
                    t.tableLead = tableLeadCantidates[0];
                    tableLeadCantidates.RemoveAt(0);
                    t.kids.Add(t.tableLead);
                    kids.Find(delegate(kid o) { return o.name == t.tableLead.name; }).beenLead = true;
                }
                else
                {
                    //eveyone has been a lead!  change them all back to not
                    foreach (kid k in kids)
                    {
                        k.beenLead = false;
                    }
                    //pick a random student to be a lead
                    t.tableLead = alreadyBeenLead[0];
                    alreadyBeenLead.RemoveAt(0);
                    t.kids.Add(t.tableLead);
                    kids.Find(delegate(kid o) { return o.name == t.tableLead.name; }).beenLead = true;
                }
                thisWeek.tables.Add(t);
            }
            //ok, got all the tables made now.. and with a proper lead in this week, time to file in students
            //we can put them all in the same pool now

            List<kid> tempKidPool = new List<kid>();
            foreach (kid k in tableLeadCantidates)
            {
                tempKidPool.Add(k);
            }
            foreach (kid k in alreadyBeenLead)
            {
                tempKidPool.Add(k);
            }
            //run through the kids and filter them in
            int tableNum = 0;
            foreach (kid k in tempKidPool)
            {
                thisWeek.tables[tableNum].kids.Add(k);
                tableNum++;
                if (tableNum >= thisWeek.tables.Count) tableNum = 0;
            }            
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            loadFromFile();
            assignTables();
            textBox1disp.Text = thisWeek.ToString();
            writeToFile();
        }
        public List<T> Shuffle<T>(List<T> list)  
        {  
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();  
            int n = list.Count;  
            while (n > 1)  
            {  
                byte[] box = new byte[1];  
                do provider.GetBytes(box);  
                while (!(box[0] < n * (Byte.MaxValue / n)));  
                int k = (box[0] % n);  
                n--;  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }
            return list;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            showHelp();
        } 
    }
    public class kid
    {
        public String name;
        public bool beenLead = false;
        public override bool Equals(object obj)
        {
            kid temp = (kid)obj;
            return temp.name.Equals(this.name);
        }
        public override int GetHashCode()
        {
            return name.GetHashCode()+beenLead.GetHashCode();
        }
        public override string ToString()
        {
            return name +
                ( beenLead? " has" : " hasn't") + " been a table lead.";
        }
        public kid(string myname, bool leadyet)
        {
            name = myname;
            beenLead = leadyet;
        }
    }
    public class table
    {
        public List<kid> kids= new List<kid>();
        public kid tableLead;
        public table()
        {


        }
        public override string ToString()
        {
            string output = "Table Lead: ";
            foreach (kid k in kids)
            {
                output += k.name + "\r\n";
            }
            return output;
            
        }
    }
    public class week
    {
        public List<table> tables=new List<table>();
        public override string ToString()
        {
            string output="";
            int n = 1;
            foreach (table t in tables)
            {
                output += "Table #" + n.ToString() + "\r\n";
                output += t.ToString()+"\r\n";
                n++;
            }
            return output;
        }
    }
   
}