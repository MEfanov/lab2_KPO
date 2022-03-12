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
using System.Windows.Shapes;

namespace lab2_KPO
{
    /// <summary>
    /// Логика взаимодействия для MultipleInputFieldWindow.xaml
    /// </summary>
    public partial class MultipleInputFieldWindow : Window
    {
        Dictionary<string, InputField> _inputFields;

        public double FieldWidth { get; set; } = 50;

        public MultipleInputFieldWindow()
        {
            InitializeComponent();
            _inputFields = new Dictionary<string, InputField>();
        }

        public void AddInputField(string fieldTag, string fieldMessage = null, string defaultText = "",
            Predicate<string> predicate = null)
        {
            if (_inputFields.ContainsKey(fieldTag))
                throw new Exception("Поле с указанным тэгом уже существует");
            _inputFields.Add(fieldTag, new InputField() { Message = fieldMessage, Text = defaultText,
                Predicate = predicate,
                Margin = new Thickness(5)
            });
        }

        public string GetInputFieldValue(string fieldTag)
        {
            if (_inputFields == null || !_inputFields.ContainsKey(fieldTag))
                throw new Exception("Не удалось найти поле с указанным тегом");
            if (!_inputFields[fieldTag].IsValid)
                return null;
            return _inputFields[fieldTag].Text;
        }

        private void FieldContainer_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var field in _inputFields.Values)
            {
                FieldContainer.Children.Add(field);
                field.Width = ActualWidth;
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
