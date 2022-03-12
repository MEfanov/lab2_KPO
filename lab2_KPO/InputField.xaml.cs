using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lab2_KPO
{
    /// <summary>
    /// Логика взаимодействия для InputField.xaml
    /// </summary>
    public partial class InputField : UserControl
    {
        private bool isValid = true;

        public Predicate<string> Predicate { get; set; }

        public string Message
        {
            get { return FieldMessageBox.Text; }
            set { FieldMessageBox.Text = value; }
        }

        public string Text
        {
            get { return InputBox.Text; }
            set { InputBox.Text = value; }
        }

        public bool IsValid
        {
            get { return isValid; }
            set 
            {
                isValid = value;
                if (isValid)
                    InputBox.Foreground = new SolidColorBrush(Colors.Black);
                else
                    InputBox.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        public InputField()
        {
            InitializeComponent();
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Predicate == null || Predicate(InputBox.Text))
                IsValid = true;
            else IsValid = false;
        }
    }
}
