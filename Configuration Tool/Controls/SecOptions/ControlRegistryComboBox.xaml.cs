﻿using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace Configuration_Tool.Controls.SecOptions
{
    /// <summary>
    /// Interaction logic for ControlRegistryComboBox.xaml
    /// </summary>
    public partial class ControlRegistryComboBox : UserControl, ISecurityOption
    {
        public ESecurityOptionType Type => ESecurityOptionType.RegistryComboBox;

        public string Header
        {
            get
            {
                return txtHeader.Text;
            }
            set
            {
                txtHeader.Text = value;
            }
        }

        public bool IsScored
        {
            get
            {
                if (!checkBoxScored.IsChecked.HasValue) return false;

                return checkBoxScored.IsChecked.Value;
            }
            set
            {
                checkBoxScored.IsChecked = value;
            }
        }

        public string Delimiter = ";";

        public string items = "";
        public string Items
        {
            get
            {
                return items;
            }
            set
            {
                if (value != items)
                {
                    items = value;
                    string[] itemsList = ItemsList;

                    comboBox.Items.Clear();

                    foreach (string item in itemsList)
                    {
                        comboBox.Items.Add(item);
                    }

                    if (itemsList.Length > 0)
                    {
                        comboBox.SelectedIndex = 0;
                    }
                }
            }
        }

        public string[] ItemsList => items.Split(new string[] { Delimiter }, StringSplitOptions.None);

        public string SelectedItem
        {
            get { return (string)comboBox.SelectedItem; }
            set
            {
                comboBox.SelectedItem = value;
            }
        }


        public int SelectedIndex
        {
            get { return comboBox.SelectedIndex; }
            set
            {
                comboBox.SelectedIndex = value;
            }
        }

        public string Key { get; set; }

        public string ValueName { get; set; }

        public void Parse(BinaryReader reader)
        {
            Header = reader.ReadString();

            // Get scoring status
            IsScored = reader.ReadBoolean();
            
            // Get items
            int count = reader.ReadInt32();
            string[] items = new string[count];

            for (int i = 0; i < count; i++)
            {
                items[i] = reader.ReadString();
            }

            Items = string.Join(";", items);

            // Get selected index
            SelectedIndex = reader.ReadInt32();

            // Get key and value name
            Key = reader.ReadString();
            ValueName = reader.ReadString();
        }

        public void Write(BinaryWriter writer)
        {
            // Write all classifying information
            writer.Write(Header);
            writer.Write(IsScored);
            writer.Write(comboBox.Items.Count);
            foreach (string item in comboBox.Items.Cast<string>())
            {
                writer.Write(item);
            }
            writer.Write(SelectedIndex);
            writer.Write(Key);
            writer.Write(ValueName);
        }

        public void CopyTo(ISecurityOption securityOption)
        {
            // Cast interface to self's type. This function should not be called to other type
            ControlRegistryComboBox control = securityOption as ControlRegistryComboBox;

            if (control == null) return;

            // We don't want to copy the header in case we update a header
            //control.Header = Header;
            control.IsScored = IsScored;
            //control.Items = Items;
            control.SelectedIndex = SelectedIndex;
            control.Key = Key;
            control.ValueName = ValueName;
        }

        public string Identifier()
        {
            return Key + " - " + ValueName;
        }

        public ControlRegistryComboBox()
        {
            InitializeComponent();
        }
    }
}
