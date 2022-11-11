using System;
using System.IO;
using GeniePlugin.Interfaces;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;
using System.Xml.Serialization;

namespace SendvarPlugin
{

    public class Sendvar : IPlugin
    {
        private bool _enabled = true;
        private IHost _host;
        private string _filePath;

        //## Value class for holding variables to send
        public class VariableToSend
        {
            public string name;
            public string value;
        }

        static void Main(string[] args)
        {
        }

        public void Initialize(IHost host)
        {
            this._host = host;

            this._filePath = _host.get_Variable("PluginPath");
            if (string.IsNullOrEmpty(this._filePath))
                this._filePath = System.Windows.Forms.Application.StartupPath + @"\Plugins\";
            if (!this._filePath.EndsWith(@"\"))
                this._filePath += @"\";
        }

        public void Show()
        {
            
        }

        public void VariableChanged(string variable)
        {
            this.setGenieVars();
        }

        public string ParseText(string text, string window)
        {
            this.setGenieVars();

            return text;
        }

        public string ParseInput(string text)
        {
            this.setGenieVars();

            if (text.StartsWith("/sendvar"))
            {
                char[] delimeter = { ' ' };
                string[] parts = text.Split(delimeter);

                if (parts.Length >= 4)
                {
                    var varValue = new string[parts.Length - 3];
                    Array.Copy(parts, 3, varValue, 0, varValue.Length);

                    //this._host.EchoText("Char: " + parts[1]);
                    //this._host.EchoText("Name: " + parts[2]);
                    //this._host.EchoText("Value: " + String.Join(" ", varValue));

                    this.setVar(parts[1], parts[2], String.Join(" ", varValue));
                }
                else
                {
                    this._host.EchoText("Usage is /setvar charactername varname value");
                }


                return "";
            }
            else
            {
                return text;
            }
        }
        
        public void ParseXML(string xml)
        {
        }

        public void ParentClosing()
        {
        }

        public string Name
        {
            get { return "Sendvar"; }
        }

        public string Version
        {
            get { return "1.0"; }
        }

        public string Description
        {
            get { return "Sends variables to other character within Genie";  }
        }

        public string Author
        {
            get { return "UFTimmy @ AIM"; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        private string getXmlFileName(string charName)
        {
            return _filePath + "Sendvar" + charName + ".xml";
        }

        private void setVar(string characterName, string name, string value)
        {
            //this._host.EchoText("About to set a variable");

            List<VariableToSend> variablesToSend = this.getVars(characterName);
            variablesToSend.Add(new VariableToSend { name = name, value = value });

            try
            {
                FileStream writer = new FileStream(this.getXmlFileName(characterName), FileMode.Create);
                XmlSerializer serializer = new XmlSerializer(typeof(List<VariableToSend>));
                serializer.Serialize(writer, variablesToSend);
                writer.Close();
            }
            catch (System.Exception)
            {
                //There are frequent accesses of the same file, just ignore.
                //_host.EchoText("Error writing Sendvar character file: " + ex.Message);
            }
        }

        private void setGenieVars()
        {
            string characterName = this._host.get_Variable("charactername");
            if (characterName.Length == 0)
                return;

            List<VariableToSend> variablesToSend = this.getVars(characterName);

            foreach (VariableToSend var in variablesToSend)
            {
                //this._host.SendText("#var " + var.name + " " + var.value);
                this._host.set_Variable(var.name, var.value);
            }

            try
            {
                File.Delete(this.getXmlFileName(characterName));
            }
            catch (System.Exception)
            {
                //There are frequent accesses of the same file, just ignore.
                //_host.EchoText("Error deleting Sendvar character file: " + ex.Message);
            }
        }

        private List<VariableToSend> getVars(string characterName)
        {
            //##this._host.EchoText("About to read variables for " + characterName);

            List<VariableToSend> variablesToSend = new List<VariableToSend>();

            string fileName = this.getXmlFileName(characterName);
            if (File.Exists(fileName))
            {
                try
                {
                    using (Stream stream = File.Open(fileName, FileMode.Open))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<VariableToSend>));
                        variablesToSend = (List<VariableToSend>) serializer.Deserialize(stream);
                        stream.Close();
                    }
                }
                catch (System.Exception)
                {
                    //There are frequent accesses of the same file, just ignore.
                    //_host.EchoText("Error reading Sendvar character file: " + ex.Message);
                }
            }

            return variablesToSend;
        }

    }
}
