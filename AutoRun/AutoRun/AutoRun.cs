using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;

namespace AutoRun
{
    public partial class AutoRun : Form
    {
        public const string HKLM_Logon = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        public const string HKCU_Logon = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
        public const string HKLM_IE_BHO = "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects";
        public const string HKLM_CLSID = "Software\\Classes\\CLSID";
        public const string HKLM_Services = "System\\CurrentControlSet\\Services";
        public const string ScheduledTask_Path = "C:\\Windows\\System32\\Tasks";
        public const string HKLM_KnownDLLs = "System\\CurrentControlSet\\Control\\Session Manager\\KnownDlls";
        public AutoRun()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ListViewItem listviewitem = new ListViewItem();
            string item, name;
            int i;
            int start, dot;

            //Logon
            listView1.Items.Clear();
            listView1.Groups.Clear();
            #region LocalMachine->Run
            ListViewGroup logongroup1 = new ListViewGroup();
            logongroup1.Header = "HKLM\\" + HKLM_Logon;
            listView1.Groups.Add(logongroup1);
            RegistryKey hklm_logon = Registry.LocalMachine.OpenSubKey(HKLM_Logon);
            for (i = 0; i < hklm_logon.GetValueNames().Length; i++)
            {
                item = hklm_logon.GetValueNames().ElementAt(i);
                name = hklm_logon.GetValue(item).ToString();
                //MessageBox.Show(name);
                if (name == "") continue;
                start = name.IndexOf(":") - 1;
                if (start == -2) start = 0;
                dot = name.IndexOf(".");
                string file = name.Substring(start, dot - start + 4);
                if (file.IndexOf(":") == -1) file = "C:\\Windows\\system32\\" + file;
                listviewitem = new ListViewItem(item);
                try
                {
                    FileVersionInfo filever = FileVersionInfo.GetVersionInfo(file);
                    listviewitem.SubItems.Add(filever.FileDescription);
                    listviewitem.SubItems.Add(filever.ProductName);
                }
                catch
                {
                    listviewitem.SubItems.Add("");
                    listviewitem.SubItems.Add("");
                }
                listviewitem.SubItems.Add(file);
                logongroup1.Items.Add(listviewitem);
                listView1.Items.Add(listviewitem);
            }
            #endregion
            #region CurrentUser->Run
            ListViewGroup logongroup2 = new ListViewGroup();
            logongroup2.Header = "HKCU\\" + HKCU_Logon;
            listView1.Groups.Add(logongroup2);
            RegistryKey hkcu_logon = Registry.CurrentUser.OpenSubKey(HKCU_Logon);
            for (i = 0; i < hkcu_logon.GetValueNames().Length; i++)
            {
                item = hkcu_logon.GetValueNames().ElementAt(i);
                name = hkcu_logon.GetValue(item).ToString();
                if (name == "") continue;
                //MessageBox.Show(name);
                start = name.IndexOf(":") - 1;
                if (start == -2) start = 0;
                dot = name.IndexOf(".");
                string file = name.Substring(start, dot - start + 4);
                if (file.IndexOf(":") == -1) file = "C:\\Windows\\system32\\" + file;
                listviewitem = new ListViewItem(item);
                try
                {
                    FileVersionInfo filever = FileVersionInfo.GetVersionInfo(file);
                    listviewitem.SubItems.Add(filever.FileDescription);
                    listviewitem.SubItems.Add(filever.ProductName);
                }
                catch
                {
                    listviewitem.SubItems.Add("");
                    listviewitem.SubItems.Add("");
                }
                listviewitem.SubItems.Add(file);
                logongroup2.Items.Add(listviewitem);
                listView1.Items.Add(listviewitem);
            }
            #endregion

            //IE
            listView2.Items.Clear();
            listView2.Groups.Clear();
            #region LocalMachine->IE BHO
            ListViewGroup iegroup = new ListViewGroup();
            iegroup.Header = "HKLM\\" + HKLM_IE_BHO;
            listView2.Groups.Add(iegroup);
            RegistryKey hklm_ie_bho = Registry.LocalMachine.OpenSubKey(HKLM_IE_BHO);
            RegistryKey clsid = Registry.LocalMachine.OpenSubKey(HKLM_CLSID);
            //MessageBox.Show(hklm.GetSubKeyNames().Length.ToString());
            foreach (string internetName in hklm_ie_bho.GetSubKeyNames())
            {
                //MessageBox.Show(internetName);
                if (internetName == null) continue;
                foreach (string key in clsid.GetSubKeyNames())
                {
                    if (key == null) continue;
                    if (key.Equals(internetName))
                    {
                        //MessageBox.Show(internetName);
                        RegistryKey subkey = clsid.OpenSubKey(key).OpenSubKey("InprocServer32");
                        //MessageBox.Show(subkey.Name);
                        FileVersionInfo filever = FileVersionInfo.GetVersionInfo(subkey.GetValue("").ToString());
                        //MessageBox.Show(subkey.GetValue("").ToString());
                        ListViewItem ie = new ListViewItem();
                        ie.SubItems[0].Text = filever.InternalName;
                        ie.SubItems.Add(filever.FileDescription);
                        ie.SubItems.Add(filever.ProductName);
                        ie.SubItems.Add(filever.FileName);
                        if (filever.InternalName.Equals("")) { }
                        else
                        {
                            iegroup.Items.Add(ie);
                            listView2.Items.Add(ie);
                        }
                    }
                }
            }
            #endregion

            //Services
            listView3.Items.Clear();
            listView3.Groups.Clear();
            #region LocalMachine->Services
            ListViewGroup servicegroup = new ListViewGroup();
            servicegroup.Header = "HKLM\\" + HKLM_Services;
            listView3.Groups.Add(servicegroup);
            RegistryKey hklm_services = Registry.LocalMachine.OpenSubKey(HKLM_Services);
            int type;
            string path, imagepath;
            foreach (string serviceName in hklm_services.GetSubKeyNames())
            {
                RegistryKey key = hklm_services.OpenSubKey(serviceName);
                if (key.GetValue("Type") != null)
                {
                    type = (int)key.GetValue("Type");
                    //MessageBox.Show(type.ToString());
                }
                else
                    type = 0;
                /*Type:
                 * 1 2 4 8: drivers
                 * 16 win32服务，以其自身进程运行，不与其他服务共享可执行文件（即宿主进程）
                 * 32 win32服务，作为共享进程运行，与其他服务共享可执行文件（即宿主进程）
                 * 272 win32服务，以其自身进程运行，同时服务可与桌面交互，接受用户输入，交互服务必须以localsystem本地系统帐户运行
                 * 288	win32服务，以共享进程运行，同时服务可与桌面交互，接受用户输入，交互服务必须以localsystem本地系统帐户运行
                 */
                if (key.GetValue("ImagePath") != null)
                    path = key.GetValue("ImagePath").ToString();
                else
                    path = "";
                if ((type == 16) || (type == 32) || (type == 272) || (type == 288))
                {
                    string temppath = path.ToLower();//find svchost
                    if (path.ToLower().IndexOf("svchost") != -1)
                    {
                        RegistryKey parameter = key.OpenSubKey("Parameters");
                        if (parameter != null)
                            imagepath = parameter.GetValue("ServiceDLL").ToString();
                        else
                            //e.g. C:/windows/system32/browser.dll
                            if (path.IndexOf(":") == -1)
                                imagepath = "C:\\Windows\\system32\\" + path;
                            else
                            {
                                start = path.IndexOf(":") - 1;
                                //MessageBox.Show(start.ToString());
                                imagepath = path.Substring(start, path.IndexOf(".") - start + 4);
                            }
                    }
                    else
                        if (path.IndexOf(":") == -1)
                            imagepath = "C:\\Windows\\system32\\" + path;
                        else
                        {
                            start = path.IndexOf(":") - 1;
                            //MessageBox.Show(start.ToString());
                            imagepath = path.Substring(start, path.IndexOf(".") - start + 4);
                        }
                    try
                    {
                        FileVersionInfo filever = FileVersionInfo.GetVersionInfo(imagepath);
                        listviewitem = new ListViewItem(serviceName);
                        listviewitem.SubItems.Add(filever.FileDescription);
                        listviewitem.SubItems.Add(filever.ProductName);
                        listviewitem.SubItems.Add(imagepath);
                    }
                    catch
                    {
                        listviewitem = new ListViewItem(serviceName);
                        listviewitem.SubItems.Add("");
                        listviewitem.SubItems.Add("");
                        listviewitem.SubItems.Add(imagepath);
                    }
                    servicegroup.Items.Add(listviewitem);
                    listView3.Items.Add(listviewitem);
                }
            }
            #endregion

            //Drivers
            listView4.Items.Clear();
            listView4.Groups.Clear();
            #region LocalMachine->Drivers
            ListViewGroup driversgroup = new ListViewGroup();
            driversgroup.Header = "HKLM\\" + HKLM_Services;
            listView4.Groups.Add(driversgroup);
            foreach (string driversName in hklm_services.GetSubKeyNames())
            {
                RegistryKey key = hklm_services.OpenSubKey(driversName);
                if (key.GetValue("ImagePath") != null) 
                    path = key.GetValue("ImagePath").ToString();
                else 
                    path = "";
                if (key.GetValue("Type") != null) 
                    type = (int)key.GetValue("Type");
                else 
                    type = 0;
                if ((type == 1) || (type == 2) || (type == 4) || (type == 8))
                {

                    //  MessageBox.Show(path);
                    //  path = \SystemRoot\System32\..\*.dll
                    //  path = System32\..\*.dll
                    //  path = ??C:\windows\system32\..\*.dll
                    if (path.IndexOf(".sys") == -1) continue;
                    if (path.IndexOf("\\SystemRoot") != -1)
                    {
                        imagepath = "C:\\Windows" + path.Substring(path.IndexOf("\\SystemRoot") + 10, path.Length - 10);
                    }
                    else if (path.IndexOf(":") != -1)
                    { 
                        start = path.IndexOf(":") - 1;
                        dot = path.IndexOf(".");
                        imagepath = path.Substring(start, dot - start + 4);
                    }
                    else
                    {
                        imagepath = "C:\\Windows\\" + path;
                    }
                    listviewitem = new ListViewItem(driversName);
                    try
                    {
                        FileVersionInfo filever = FileVersionInfo.GetVersionInfo(imagepath);
                        listviewitem.SubItems.Add(filever.FileDescription);
                        listviewitem.SubItems.Add(filever.ProductName);
                    }
                    catch
                    {
                        listviewitem.SubItems.Add("");
                        listviewitem.SubItems.Add("");
                    }
                    listviewitem.SubItems.Add(imagepath);
                    driversgroup.Items.Add(listviewitem);
                    listView4.Items.Add(listviewitem);
                }
            }
            #endregion

            //Scheduled Tasks
            listView5.Items.Clear();
            listView5.Groups.Clear();
            #region C:\Windows\System32\Tasks
            ListViewGroup tasksgroup = new ListViewGroup();
            tasksgroup.Header = ScheduledTask_Path;
            listView5.Groups.Add(tasksgroup);
            FileInfo filename;
            DirectoryInfo startdir = new DirectoryInfo(ScheduledTask_Path);
            for (i = 0; i < Directory.GetFiles(ScheduledTask_Path).Length; i++)
            {
                filename = startdir.GetFiles().ElementAt(i);
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(filename.FullName);
                FileStream fs = filename.OpenRead();
                long filelength = fs.Length;
                byte[] context = new byte[256];
                int j = 0;
                path = "";
                do
                {
                    fs.Read(context, 0, 256);
                    foreach (byte s in context){if (s != 0) path += (char)s;}
                    j += 256;
                } while (j < filelength);
                //MessageBox.Show(path);
                //if (path.IndexOf(".exe") == -1) continue;
                //start = path.IndexOf(":") - 1;
                //dot = path.ToLower().IndexOf(".exe");
                path = path.Substring(path.IndexOf("<Command>") + 9, path.IndexOf("</Command>") - path.IndexOf("<Command>") - 9);
                FileVersionInfo filever = FileVersionInfo.GetVersionInfo(path);
                while (!Char.IsLetter(path, 0))
                    path = path.Remove(0, 1);
                while (!Char.IsLetter(path, path.Length - 1))
                    path = path.Remove(path.Length - 1, 1);
                if (filename.Name[0]=='{') continue;
                try
                {
                    listviewitem=new ListViewItem(filename.Name);
                    listviewitem.SubItems.Add(filever.FileDescription);
                    listviewitem.SubItems.Add(filever.ProductName);
                }
                catch
                {
                    listviewitem = new ListViewItem(filename.Name);
                    listviewitem.SubItems.Add("");
                    listviewitem.SubItems.Add("");
                }
                listviewitem.SubItems.Add(path);
                tasksgroup.Items.Add(listviewitem);
                listView5.Items.Add(listviewitem);
            }

            #endregion

            //Known DLLs
            listView6.Items.Clear();
            listView6.Groups.Clear();
            #region LocalMachine->Known DLLs
            ListViewGroup dllgroup = new ListViewGroup();
            dllgroup.Header = "HKLM\\"+HKLM_KnownDLLs;
            listView6.Groups.Add(dllgroup);
            string dlldir = "";
            RegistryKey hklm = Registry.LocalMachine.OpenSubKey(HKLM_KnownDLLs);
            foreach (string dlls in hklm.GetValueNames())
            {
                if (dlls.Equals("DllDirectory"))
                {
                    dlldir = hklm.GetValue(dlls).ToString();
                    break;
                }
            }
            foreach (string valuename in hklm.GetValueNames())
            {
                string dllname = hklm.GetValue(valuename).ToString();
                if (dllname.IndexOf(".dll") == -1) continue;
                listviewitem = new ListViewItem(valuename);
                path = dlldir + "\\" + dllname;
                try
                {
                    FileVersionInfo filever = FileVersionInfo.GetVersionInfo(path);
                    listviewitem.SubItems.Add(filever.FileDescription);
                    listviewitem.SubItems.Add(filever.ProductName);
                    listviewitem.SubItems.Add(path);
                }
                catch
                {
                    listviewitem.SubItems.Add("");
                    listviewitem.SubItems.Add("");
                    listviewitem.SubItems.Add(path);
                }
                dllgroup.Items.Add(listviewitem);
                listView6.Items.Add(listviewitem);
            }

            #endregion
            textBox1.Text=
"**********************************************\r\n"+
"*                                            *\r\n"+
"*                                            *\r\n"+
"*                                            *\r\n"+
"*                Auto Run v1.0               *\r\n"+
"*                                            *\r\n"+
"*                                            *\r\n"+
"*         Poewred by Ren Yanbin@SJTU_IS      *\r\n"+
"*                                            *\r\n"+
"*                                            *\r\n"+
"*                                            *\r\n"+
"*                                            *\r\n"+
"**********************************************\r\n\r\n"+
"   Contact me:\r\n\r\n"+
"   Email: ryb@sjtu.edu.cn\r\n\r\n"+
"   Mobile: +86 15921050722\r\n\r\n"+
"Copyright © 2013 SJTU_IS.All rights reserved.\r\n";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "文本文件|*.txt|CSV文件|*.csv|All files(*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string file = saveFileDialog1.FileName;
                FileInfo fi = new FileInfo(file);
                StreamWriter sw = fi.CreateText();
                sw.WriteLine("Autorun Entry\tDescription\tPublisher\tImage Path");
                for (int i = 0; i < this.listView1.Items.Count; i++)
                {
                    sw.WriteLine(string.Format("{0},{1},{2},{3}\r\t", listView1.Items[i].Text, listView1.Items[i].SubItems[1].Text, listView1.Items[i].SubItems[2].Text, listView1.Items[i].SubItems[3].Text));
                }
                sw.Close();
                System.Diagnostics.Process.Start(file);
            } 
        }
        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "文本文件|*.txt|CSV文件|*.csv|All files(*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string file = saveFileDialog1.FileName;
                FileInfo fi = new FileInfo(file);
                StreamWriter sw = fi.CreateText();
                sw.WriteLine("Autorun Entry\tDescription\tPublisher\tImage Path");
                for (int i = 0; i < this.listView1.Items.Count; i++)
                {
                    sw.WriteLine(string.Format("{0},{1},{2},{3}\r\t", listView1.Items[i].Text, listView1.Items[i].SubItems[1].Text, listView1.Items[i].SubItems[2].Text, listView1.Items[i].SubItems[3].Text));
                }
                sw.Close();
                System.Diagnostics.Process.Start(file);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "文本文件|*.txt|CSV文件|*.csv|All files(*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string file = saveFileDialog1.FileName;
                FileInfo fi = new FileInfo(file);
                StreamWriter sw = fi.CreateText();
                sw.WriteLine("Autorun Entry\tDescription\tPublisher\tImage Path");
                for (int i = 0; i < this.listView1.Items.Count; i++)
                {
                    sw.WriteLine(string.Format("{0},{1},{2},{3}\r\t", listView1.Items[i].Text, listView1.Items[i].SubItems[1].Text, listView1.Items[i].SubItems[2].Text, listView1.Items[i].SubItems[3].Text));
                }
                sw.Close();
                System.Diagnostics.Process.Start(file);
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "文本文件|*.txt|CSV文件|*.csv|All files(*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string file = saveFileDialog1.FileName;
                FileInfo fi = new FileInfo(file);
                StreamWriter sw = fi.CreateText();
                sw.WriteLine("Autorun Entry\tDescription\tPublisher\tImage Path");
                for (int i = 0; i < this.listView1.Items.Count; i++)
                {
                    sw.WriteLine(string.Format("{0},{1},{2},{3}\r\t", listView1.Items[i].Text, listView1.Items[i].SubItems[1].Text, listView1.Items[i].SubItems[2].Text, listView1.Items[i].SubItems[3].Text));
                }
                sw.Close();
                System.Diagnostics.Process.Start(file);
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "文本文件|*.txt|CSV文件|*.csv|All files(*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string file = saveFileDialog1.FileName;
                FileInfo fi = new FileInfo(file);
                StreamWriter sw = fi.CreateText();
                sw.WriteLine("Autorun Entry\tDescription\tPublisher\tImage Path");
                for (int i = 0; i < this.listView1.Items.Count; i++)
                {
                    sw.WriteLine(string.Format("{0},{1},{2},{3}\r\t", listView1.Items[i].Text, listView1.Items[i].SubItems[1].Text, listView1.Items[i].SubItems[2].Text, listView1.Items[i].SubItems[3].Text));
                }
                sw.Close();
                System.Diagnostics.Process.Start(file);
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "文本文件|*.txt|CSV文件|*.csv|All files(*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string file = saveFileDialog1.FileName;
                FileInfo fi = new FileInfo(file);
                StreamWriter sw = fi.CreateText();
                sw.WriteLine("Autorun Entry\tDescription\tPublisher\tImage Path");
                for (int i = 0; i < this.listView1.Items.Count; i++)
                {
                    sw.WriteLine(string.Format("{0},{1},{2},{3}\r\t", listView1.Items[i].Text, listView1.Items[i].SubItems[1].Text, listView1.Items[i].SubItems[2].Text, listView1.Items[i].SubItems[3].Text));
                }
                sw.Close();
                System.Diagnostics.Process.Start(file);
            }
        }


    }
}
