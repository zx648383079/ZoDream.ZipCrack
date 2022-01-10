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
using ZoDream.Shared.Crack;

namespace ZoDream.ZipCrack.Controls
{
    /// <summary>
    /// KeyInput.xaml 的交互逻辑
    /// </summary>
    public partial class KeyInput : UserControl
    {
        public KeyInput()
        {
            InitializeComponent();
        }



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(KeyInput), new PropertyMetadata(string.Empty, OnTextChanged));

        public event TextChangedEventHandler? TextChanged;

        public KeyItem? Keys
        {
            get {
                var data = KeyArray;
                if (data.Length >= 3)
                {
                    return new KeyItem();
                }
                return new KeyItem(data[0], data[1], data[2]);
            }
            set
            {
                Text = value.ToString();
            }
        }

        public string[] KeyArray
        {
            get
            {
                return keyTb.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
            set
            {
                Text = keyTb.Text = string.Join(" ", value).Trim();
                key3Label.Text = value.Length >= 3 ? value[2] : string.Empty;
                key2Label.Text = value.Length >= 2 ? value[1] : string.Empty;
                key1Label.Text = value.Length >= 1 ? value[0] : string.Empty;
            }
        }

        public bool IsCompleted => KeyArray.Length == 3;

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as KeyInput;
            if (self == null)
            {
                return;
            }
            if (e.NewValue == null)
            {
                self.keyTb.Text = self.key1Label.Text = 
                    self.key2Label.Text = self.key3Label.Text = string.Empty;
                return;
            }
            self.KeyArray = e.NewValue.ToString().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private void keyTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Text = (sender as TextBox).Text;
            TextChanged?.Invoke(this, Text);
        }

        private void keyTb_LostFocus(object sender, RoutedEventArgs e)
        {
            keyLabelPanel.Visibility = Visibility.Visible;
        }

        private void keyLabelPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            keyLabelPanel.Visibility = Visibility.Collapsed;
            keyTb.Focus();
        }
    }

    public delegate void TextChangedEventHandler(object sender, string text);
}
