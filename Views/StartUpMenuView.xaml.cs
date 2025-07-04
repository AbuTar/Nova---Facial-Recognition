﻿using System;
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

namespace Nova.Views
{
    /// <summary>
    /// Interaction logic for StartupMenuView.xaml
    /// </summary>
    public partial class StartupMenuView : Window
    {
        public StartupMenuView()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new instance of MainWindow
            MainWindow mainWindow = new MainWindow();

            // Show the new window
            mainWindow.Show();

            // Close the current StartupMenuView's parent window
            Window.GetWindow(this)?.Close();

        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
